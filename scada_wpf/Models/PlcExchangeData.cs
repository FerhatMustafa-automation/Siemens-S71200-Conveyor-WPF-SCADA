using System;

namespace DigitalTwinScada.Models
{
    /// <summary>
    /// Siemens S7-1200 Non-Optimized DB1 (SCADA_Exchange) ile birebir byte-ofset eşleşen veri modeli.
    /// Toplam Paket Boyutu: 24 Byte (0..23)
    /// </summary>
    public class PlcExchangeData
    {
        // ---------------------------------------------------------------------
        // BYTE 0: SCADA -> PLC KONTROL KOMUTLARI
        // ---------------------------------------------------------------------
        public bool CmdStart { get; set; }           // DBX0.0
        public bool CmdStop { get; set; }            // DBX0.1
        public bool CmdResetAlarm { get; set; }      // DBX0.2
        public bool CmdEStop { get; set; }           // DBX0.3
        public bool CmdManualPusher { get; set; }    // DBX0.4
        public bool CmdAutoMode { get; set; }        // DBX0.5
        public bool CmdClearCount { get; set; }      // DBX0.6
        public bool WatchdogIn { get; set; }         // DBX0.7

        // ---------------------------------------------------------------------
        // BYTE 1: PLC -> SCADA DURUM VE GERİ BİLDİRİM BİTLERİ
        // ---------------------------------------------------------------------
        public bool StatusRunning { get; set; }      // DBX1.0
        public bool StatusStopped { get; set; }      // DBX1.1
        public bool StatusAlarm { get; set; }        // DBX1.2
        public bool StatusEStopActive { get; set; }  // DBX1.3
        public bool StatusSensorEntry { get; set; }  // DBX1.4
        public bool StatusSensorExit { get; set; }   // DBX1.5
        public bool StatusPusherExtended { get; set;}// DBX1.6
        public bool WatchdogEcho { get; set; }       // DBX1.7

        // ---------------------------------------------------------------------
        // BYTE 2 - 23: TELEMETRİ VE ANALOG VERİLER
        // ---------------------------------------------------------------------
        public short SetSpeedRpm { get; set; }       // DBW2  (2 Byte - INT)
        public short ActualSpeedRpm { get; set; }    // DBW4  (2 Byte - INT)
        public int PieceCountTotal { get; set; }     // DBD6  (4 Byte - DINT)
        public int PieceCountRejected { get; set; }  // DBD10 (4 Byte - DINT)
        public float MotorCurrentAmp { get; set; }   // DBD14 (4 Byte - REAL IEEE 754)
        public float MotorTemperature { get; set; }  // DBD18 (4 Byte - REAL IEEE 754)
        public short AlarmCode { get; set; }         // DBW22 (2 Byte - INT)
    }
}
