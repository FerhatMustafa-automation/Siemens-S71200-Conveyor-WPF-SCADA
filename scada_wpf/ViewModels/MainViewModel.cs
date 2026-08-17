using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DigitalTwinScada.Models;
using DigitalTwinScada.Services;

namespace DigitalTwinScada.ViewModels
{
    /// <summary>
    /// SCADA Ana Arayüz ViewModel'i.
    /// Tüm PLC telemetrisini, buton komutlarını ve alarm durumlarını MVVM deseninde bağlar.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly PlcCommunicationService _plcService;

        // --- Bağlantı Parametreleri ---
        private string _ipAddress = "192.168.0.1";
        private bool _isConnected;
        private string _connectionStatusText = "PLC Bağlantısı Bekleniyor...";

        // --- PLC Telemetri & Durumlar ---
        private bool _statusRunning;
        private bool _statusStopped = true;
        private bool _statusAlarm;
        private bool _statusEStopActive;
        private bool _statusSensorEntry;
        private bool _statusSensorExit;
        private bool _statusPusherExtended;
        private bool _watchdogEcho;

        private short _setSpeedRpm = 1000;
        private short _actualSpeedRpm;
        private int _pieceCountTotal;
        private int _pieceCountRejected;
        private float _motorCurrentAmp;
        private float _motorTemperature = 25.0f;
        private short _alarmCode;
        private string _alarmDescription = "Sistem Normal";
        private bool _isAutoMode = true;

        public ObservableCollection<string> ActivityLogs { get; } = new ObservableCollection<string>();

        // --- Properties ---
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string ConnectionStatusText
        {
            get => _connectionStatusText;
            set => SetProperty(ref _connectionStatusText, value);
        }

        public bool StatusRunning
        {
            get => _statusRunning;
            set => SetProperty(ref _statusRunning, value);
        }

        public bool StatusStopped
        {
            get => _statusStopped;
            set => SetProperty(ref _statusStopped, value);
        }

        public bool StatusAlarm
        {
            get => _statusAlarm;
            set => SetProperty(ref _statusAlarm, value);
        }

        public bool StatusEStopActive
        {
            get => _statusEStopActive;
            set => SetProperty(ref _statusEStopActive, value);
        }

        public bool StatusSensorEntry
        {
            get => _statusSensorEntry;
            set => SetProperty(ref _statusSensorEntry, value);
        }

        public bool StatusSensorExit
        {
            get => _statusSensorExit;
            set => SetProperty(ref _statusSensorExit, value);
        }

        public bool StatusPusherExtended
        {
            get => _statusPusherExtended;
            set => SetProperty(ref _statusPusherExtended, value);
        }

        public bool WatchdogEcho
        {
            get => _watchdogEcho;
            set => SetProperty(ref _watchdogEcho, value);
        }

        public short SetSpeedRpm
        {
            get => _setSpeedRpm;
            set => SetProperty(ref _setSpeedRpm, value);
        }

        public short ActualSpeedRpm
        {
            get => _actualSpeedRpm;
            set => SetProperty(ref _actualSpeedRpm, value);
        }

        public int PieceCountTotal
        {
            get => _pieceCountTotal;
            set => SetProperty(ref _pieceCountTotal, value);
        }

        public int PieceCountRejected
        {
            get => _pieceCountRejected;
            set => SetProperty(ref _pieceCountRejected, value);
        }

        public float MotorCurrentAmp
        {
            get => _motorCurrentAmp;
            set => SetProperty(ref _motorCurrentAmp, value);
        }

        public float MotorTemperature
        {
            get => _motorTemperature;
            set => SetProperty(ref _motorTemperature, value);
        }

        public short AlarmCode
        {
            get => _alarmCode;
            set => SetProperty(ref _alarmCode, value);
        }

        public string AlarmDescription
        {
            get => _alarmDescription;
            set => SetProperty(ref _alarmDescription, value);
        }

        public bool IsAutoMode
        {
            get => _isAutoMode;
            set => SetProperty(ref _isAutoMode, value);
        }

        // --- Commands ---
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ResetAlarmCommand { get; }
        public ICommand ToggleEStopCommand { get; }
        public ICommand ManualPusherCommand { get; }
        public ICommand ToggleAutoModeCommand { get; }
        public ICommand ClearCountCommand { get; }
        public ICommand ApplySpeedCommand { get; }

        public MainViewModel()
        {
            _plcService = new PlcCommunicationService();
            _plcService.OnDataReceived += HandleDataReceived;
            _plcService.OnConnectionStatusChanged += HandleConnectionStatusChanged;
            _plcService.OnLogMessage += AddLogMessage;

            // Komut Bağlamaları
            ConnectCommand = new RelayCommand(_ => ConnectPlc(), _ => !IsConnected);
            DisconnectCommand = new RelayCommand(_ => DisconnectPlc(), _ => IsConnected);
            StartCommand = new RelayCommand(async _ => await _plcService.WriteStartCommandAsync(), _ => IsConnected && !StatusRunning && !StatusAlarm);
            StopCommand = new RelayCommand(async _ => await _plcService.WriteStopCommandAsync(), _ => IsConnected && StatusRunning);
            ResetAlarmCommand = new RelayCommand(async _ => await _plcService.WriteResetAlarmCommandAsync(), _ => IsConnected && StatusAlarm);
            ToggleEStopCommand = new RelayCommand(async _ => await _plcService.WriteEStopCommandAsync(!StatusEStopActive), _ => IsConnected);
            ManualPusherCommand = new RelayCommand(async _ => await _plcService.WriteManualPusherCommandAsync(), _ => IsConnected && !IsAutoMode);
            ToggleAutoModeCommand = new RelayCommand(async _ =>
            {
                IsAutoMode = !IsAutoMode;
                await _plcService.WriteAutoModeCommandAsync(IsAutoMode);
            }, _ => IsConnected);
            ClearCountCommand = new RelayCommand(async _ => await _plcService.WriteClearCountCommandAsync(), _ => IsConnected);
            ApplySpeedCommand = new RelayCommand(async _ => await _plcService.WriteSpeedRpmAsync(SetSpeedRpm), _ => IsConnected);

            AddLogMessage("SCADA Başlatıldı. Siemens S7-1200 / Factory I/O Bağlantısı Hazır.");
        }

        private void ConnectPlc()
        {
            _plcService.StartPolling(IpAddress);
        }

        private void DisconnectPlc()
        {
            _plcService.StopPolling();
        }

        private void HandleDataReceived(PlcExchangeData data)
        {
            // Thread-Safe UI Güncellemesi (WPF Dispatcher)
            Application.Current?.Dispatcher.Invoke(() =>
            {
                StatusRunning = data.StatusRunning;
                StatusStopped = data.StatusStopped;
                StatusAlarm = data.StatusAlarm;
                StatusEStopActive = data.StatusEStopActive;
                StatusSensorEntry = data.StatusSensorEntry;
                StatusSensorExit = data.StatusSensorExit;
                StatusPusherExtended = data.StatusPusherExtended;
                WatchdogEcho = data.WatchdogEcho;

                ActualSpeedRpm = data.ActualSpeedRpm;
                PieceCountTotal = data.PieceCountTotal;
                PieceCountRejected = data.PieceCountRejected;
                MotorCurrentAmp = (float)Math.Round(data.MotorCurrentAmp, 2);
                MotorTemperature = (float)Math.Round(data.MotorTemperature, 1);
                AlarmCode = data.AlarmCode;

                AlarmDescription = data.AlarmCode switch
                {
                    0 => "Sistem Normal / Hazır",
                    1 => "Acil Stop (E-Stop) Devrede!",
                    2 => "Motor Koruma Termiği Açtı!",
                    3 => "Konveyör Ürün Tıkanması (Jamming)!",
                    4 => "SCADA-PLC Watchdog İletişim Hatası!",
                    _ => $"Bilinmeyen Hata Kodu ({data.AlarmCode})"
                };
            });
        }

        private void HandleConnectionStatusChanged(bool connected, string message)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                IsConnected = connected;
                ConnectionStatusText = message;
                AddLogMessage(message);
            });
        }

        private void AddLogMessage(string msg)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                ActivityLogs.Insert(0, $"[{timestamp}] {msg}");
                if (ActivityLogs.Count > 100)
                {
                    ActivityLogs.RemoveAt(ActivityLogs.Count - 1);
                }
            });
        }
    }
}
