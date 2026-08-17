using System;
using System.Threading;
using System.Threading.Tasks;
using S7.Net;
using DigitalTwinScada.Models;

namespace DigitalTwinScada.Services
{
    /// <summary>
    /// Siemens S7-1200 PLC ile asenkron, thread-safe haberleşen ve otomatik yeniden bağlanma
    /// (Auto-Reconnect) ile Watchdog kalp atışı yöneten endüstriyel haberleşme servisi.
    /// </summary>
    public class PlcCommunicationService : IDisposable
    {
        private Plc? _plc;
        private CancellationTokenSource? _cts;
        private bool _isDisposed;
        private bool _watchdogToggle;
        private int _watchdogCounter;

        public event Action<PlcExchangeData>? OnDataReceived;
        public event Action<bool, string>? OnConnectionStatusChanged;
        public event Action<string>? OnLogMessage;

        public bool IsConnected => _plc != null && _plc.IsConnected;
        public string IpAddress { get; private set; } = "192.168.0.1";
        public CpuType Cpu { get; private set; } = CpuType.S71200;
        public short Rack { get; private set; } = 0;
        public short Slot { get; private set; } = 1;
        public int PollingIntervalMs { get; set; } = 100;

        public PlcCommunicationService()
        {
        }

        /// <summary>
        /// PLC Okuma ve İletişim Döngüsünü Arka Plan Görevi Olarak Başlatır (Non-blocking).
        /// </summary>
        public void StartPolling(string ipAddress = "192.168.0.1", CpuType cpuType = CpuType.S71200, short rack = 0, short slot = 1)
        {
            StopPolling();

            IpAddress = ipAddress;
            Cpu = cpuType;
            Rack = rack;
            Slot = slot;

            _cts = new CancellationTokenSource();
            Task.Run(() => PollingLoopAsync(_cts.Token), _cts.Token);
            OnLogMessage?.Invoke($"[OT/IT Service] PLC döngüsü başlatıldı ({IpAddress}).");
        }

        /// <summary>
        /// İletişim döngüsünü durdurur ve PLC soketini temiz kapatır.
        /// </summary>
        public void StopPolling()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            if (_plc != null)
            {
                try
                {
                    if (_plc.IsConnected)
                    {
                        _plc.Close();
                    }
                }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke($"[Kapatma Hatası] {ex.Message}");
                }
                finally
                {
                    _plc = null;
                }
            }

            OnConnectionStatusChanged?.Invoke(false, "Bağlantı kullanıcı tarafından durduruldu.");
        }

        /// <summary>
        /// Asenkron Haberleşme ve Auto-Reconnect Döngüsü
        /// </summary>
        private async Task PollingLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 1. Bağlantı Yoksa Otomatik Bağlanmayı Dene
                    if (_plc == null || !_plc.IsConnected)
                    {
                        OnConnectionStatusChanged?.Invoke(false, $"PLC'ye bağlanılıyor ({IpAddress}:102)...");
                        _plc = new Plc(Cpu, IpAddress, Rack, Slot);
                        _plc.ReadTimeout = 1500;
                        _plc.WriteTimeout = 1500;

                        await _plc.OpenAsync();

                        if (_plc.IsConnected)
                        {
                            OnConnectionStatusChanged?.Invoke(true, $"PLC Bağlantısı Başarılı ({IpAddress})");
                            OnLogMessage?.Invoke($"[Bağlantı] Siemens S7-1200 ile TCP/IP bağlantısı kuruldu.");
                        }
                    }

                    // 2. Bağlantı Aktif İse DB1'den Verileri Oku
                    if (_plc != null && _plc.IsConnected)
                    {
                        // DB1'den 24 byte'lık paketi oku
                        var data = new PlcExchangeData();
                        await _plc.ReadClassAsync(data, 1); // DB1
                        OnDataReceived?.Invoke(data);

                        // 3. Watchdog / Heartbeat Toggle Gönderimi (Her 500ms'de bir)
                        _watchdogCounter++;
                        if (_watchdogCounter >= (500 / PollingIntervalMs))
                        {
                            _watchdogCounter = 0;
                            _watchdogToggle = !_watchdogToggle;
                            await _plc.WriteAsync("DB1.DBX0.7", _watchdogToggle);
                        }
                    }

                    await Task.Delay(PollingIntervalMs, token);
                }
                catch (Exception ex)
                {
                    OnConnectionStatusChanged?.Invoke(false, $"Haberleşme Hatası: {ex.Message}");
                    OnLogMessage?.Invoke($"[Alarm] PLC Okuma Hatası: {ex.Message}. 3 sn sonra tekrar denenecek...");

                    if (_plc != null)
                    {
                        try { _plc.Close(); } catch { }
                        _plc = null;
                    }

                    // Auto-Reconnect bekleme süresi
                    try
                    {
                        await Task.Delay(3000, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        // =====================================================================
        // ASENKRON SCADA YAZMA (KONTROL) METOTLARI
        // =====================================================================

        public async Task<bool> WriteStartCommandAsync()
        {
            return await WriteBitAsync("DB1.DBX0.0", true);
        }

        public async Task<bool> WriteStopCommandAsync()
        {
            return await WriteBitAsync("DB1.DBX0.1", true);
        }

        public async Task<bool> WriteResetAlarmCommandAsync()
        {
            return await WriteBitAsync("DB1.DBX0.2", true);
        }

        public async Task<bool> WriteEStopCommandAsync(bool isEmergency)
        {
            return await WriteBitAsync("DB1.DBX0.3", isEmergency);
        }

        public async Task<bool> WriteManualPusherCommandAsync()
        {
            return await WriteBitAsync("DB1.DBX0.4", true);
        }

        public async Task<bool> WriteAutoModeCommandAsync(bool isAuto)
        {
            return await WriteBitAsync("DB1.DBX0.5", isAuto);
        }

        public async Task<bool> WriteClearCountCommandAsync()
        {
            return await WriteBitAsync("DB1.DBX0.6", true);
        }

        public async Task<bool> WriteSpeedRpmAsync(short speedRpm)
        {
            if (IsConnected && _plc != null)
            {
                try
                {
                    await _plc.WriteAsync("DB1.DBW2", speedRpm);
                    OnLogMessage?.Invoke($"[Komut] Hedef Hız {speedRpm} RPM olarak güncellendi.");
                    return true;
                }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke($"[Yazma Hatası] Hız gönderilemedi: {ex.Message}");
                }
            }
            return false;
        }

        private async Task<bool> WriteBitAsync(string address, bool value)
        {
            if (IsConnected && _plc != null)
            {
                try
                {
                    await _plc.WriteAsync(address, value);
                    return true;
                }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke($"[Yazma Hatası] Adres: {address}, Hata: {ex.Message}");
                }
            }
            return false;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                StopPolling();
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
