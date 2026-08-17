# PLC I/O & Non-Optimized Memory DB Offset Haritası

Bu doküman; **Siemens SIMATIC S7-1200 CPU 1214C**, **Factory I/O 3D Dijital İkiz Sahnesi** ve **C# .NET 8 WPF SCADA** arasındaki fiziksel ve mantıksal adres eşleştirmelerini tanımlar.

---

## 1. Fiziksel / Simüle Edilen Dijital ve Analog Girişler (Inputs)

| Donanım Adresi | Veri Tipi | Sembolik İsim | Elektriksel Tip | Açıklama & Sahadaki Karşılığı |
| :--- | :--- | :--- | :--- | :--- |
| `%I0.0` | `BOOL` | `di_StartButton` | NO (24V DC) | Operatör Paneli Başlat Butonu (Yeşil) |
| `%I0.1` | `BOOL` | `di_StopButton` | NC (24V DC) | Operatör Paneli Durdur Butonu (Kırmızı) |
| `%I0.2` | `BOOL` | `di_EmergencyStop`| NC (Çift Kanal) | E-Stop Mantar Butonu / Güvenlik Rölesi Kontağı |
| `%I0.3` | `BOOL` | `di_ThermalRelayOK`| NC (24V DC) | Motor Koruma Şalteri / Termik Röle Yardımcı Kontağı |
| `%I0.4` | `BOOL` | `di_SensorEntry` | NO (PNP) | Giriş Konveyörü Parça Algılama Fotoseli (Optik) |
| `%I0.5` | `BOOL` | `di_SensorExit` | NO (PNP) | Çıkış Konveyörü Bitiş Fotoseli (Parça Sayıcı) |
| `%I0.6` | `BOOL` | `di_SensorMetalSort`| NO (İndüktif) | Metal / Hatalı Parça Ayırma Sensörü |
| `%I0.7` | `BOOL` | `di_PusherFrontLimit`| NO (Manyetik) | Ayırıcı Pnömatik Silindir İleri Sınır Sensörü |
| `%I1.0` | `BOOL` | `di_PusherBackLimit`| NC (Manyetik) | Ayırıcı Pnömatik Silindir Geri Sınır Sensörü |

---

## 2. Fiziksel / Simüle Edilen Dijital ve Analog Çıkışlar (Outputs)

| Donanım Adresi | Veri Tipi | Sembolik İsim | Elektriksel Tip | Açıklama & Eyleyici Fonksiyonu |
| :--- | :--- | :--- | :--- | :--- |
| `%Q0.0` | `BOOL` | `dq_ConveyorEntry` | Transistör / Röle | Giriş Konveyör Motoru Kontaktörü / İleri Sürücü |
| `%Q0.1` | `BOOL` | `dq_ConveyorExit` | Transistör / Röle | Çıkış Konveyör Motoru Kontaktörü / İleri Sürücü |
| `%Q0.2` | `BOOL` | `dq_PusherExtend` | 24V DC Solenoid | 5/2 Tek Bobin Yay Geri Dönüşlü Yön Valfi Bobini |
| `%Q0.3` | `BOOL` | `dq_StackLightGreen`| 24V DC LED | Sinyal Kulesi Yeşil Işık (Sistem Normal Çalışıyor) |
| `%Q0.4` | `BOOL` | `dq_StackLightYellow`| 24V DC LED | Sinyal Kulesi Sarı Işık (Sistem Beklemede / Stop) |
| `%Q0.5` | `BOOL` | `dq_StackLightRed` | 24V DC LED | Sinyal Kulesi Kırmızı Flaşör (Arıza / Acil Stop) |
| `%Q0.6` | `BOOL` | `dq_AlarmHorn` | 24V DC Buzzer | Sesli Alarm Sireni |
| `%QW64` | `INT` | `aq_SetSpeedAnalog` | 0-10V (0-27648)| Hız Sürücüsü Frekans Set Değeri (Analog Çıkış) |

---

## 3. Non-Optimized Data Block (DB1: SCADA_Exchange) Bellek Haritası

Siemens S7-1200'de **Optimized Block Access** kapatıldığında, her değişken kesin ve sabit bir bellek ofsetine yerleşir. C# tarafında `S7NetPlus` kütüphanesi bu ofsetleri mutlak adres olarak okur/yazar.

| DB Ofseti | Değişken Adı | PLC Veri Tipi | C# Veri Tipi | Byte Boyutu | Erişim Yönü | Açıklama |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **`DB1.DBX0.0`** | `scada_cmdStart` | `BOOL` | `bool` | 1 bit | SCADA -> PLC | Konveyörü Başlatma Komutu |
| **`DB1.DBX0.1`** | `scada_cmdStop` | `BOOL` | `bool` | 1 bit | SCADA -> PLC | Konveyörü Durdurma Komutu |
| **`DB1.DBX0.2`** | `scada_cmdResetAlarm`| `BOOL` | `bool` | 1 bit | SCADA -> PLC | Arıza / Alarm Sıfırlama |
| **`DB1.DBX0.3`** | `scada_cmdEStop` | `BOOL` | `bool` | 1 bit | SCADA -> PLC | Yazılımsal Acil Durdurma |
| **`DB1.DBX0.4`** | `scada_cmdManualPusher`| `BOOL` | `bool` | 1 bit | SCADA -> PLC | Manuel Piston İtme Komutu |
| **`DB1.DBX0.5`** | `scada_cmdAutoMode` | `BOOL` | `bool` | 1 bit | SCADA -> PLC | Otomatik / Manuel Seçimi |
| **`DB1.DBX0.6`** | `scada_cmdClearCount`| `BOOL` | `bool` | 1 bit | SCADA -> PLC | Parça Sayacını Sıfırlama |
| **`DB1.DBX0.7`** | `scada_watchdogIn` | `BOOL` | `bool` | 1 bit | SCADA -> PLC | SCADA Kalp Atışı (Heartbeat Toggle) |
| **`DB1.DBX1.0`** | `plc_statusRunning` | `BOOL` | `bool` | 1 bit | PLC -> SCADA | Sistem Çalışıyor Geri Bildirimi |
| **`DB1.DBX1.1`** | `plc_statusStopped` | `BOOL` | `bool` | 1 bit | PLC -> SCADA | Sistem Durdu Geri Bildirimi |
| **`DB1.DBX1.2`** | `plc_statusAlarm` | `BOOL` | `bool` | 1 bit | PLC -> SCADA | Genel Alarm Bayrağı |
| **`DB1.DBX1.3`** | `plc_statusEStopActive`| `BOOL` | `bool` | 1 bit | PLC -> SCADA | E-Stop Kilitli Durumu |
| **`DB1.DBX1.4`** | `plc_statusSensorEntry`| `BOOL` | `bool` | 1 bit | PLC -> SCADA | Giriş Sensörü Dolu |
| **`DB1.DBX1.5`** | `plc_statusSensorExit` | `BOOL` | `bool` | 1 bit | PLC -> SCADA | Çıkış Sensörü Dolu |
| **`DB1.DBX1.6`** | `plc_statusPusherExtended`| `BOOL`| `bool` | 1 bit | PLC -> SCADA | Piston İleride Sinyali |
| **`DB1.DBX1.7`** | `plc_watchdogEcho` | `BOOL` | `bool` | 1 bit | PLC -> SCADA | PLC Watchdog Eko Cevabı |
| **`DB1.DBW2`** | `scada_setSpeedRpm` | `INT` | `short` | 2 byte (Word) | SCADA -> PLC | İstenen Hız (0 - 1500 RPM) |
| **`DB1.DBW4`** | `plc_actualSpeedRpm` | `INT` | `short` | 2 byte (Word) | PLC -> SCADA | Gerçek Hız Geri Beslemesi |
| **`DB1.DBD6`** | `plc_pieceCountTotal`| `DINT` | `int` | 4 byte (DWord)| PLC -> SCADA | Toplam Geçen Sağlam Parça Sayısı |
| **`DB1.DBD10`**| `plc_pieceCountRejected`| `DINT`| `int` | 4 byte (DWord)| PLC -> SCADA | Ayrılan Hatalı Parça Sayısı |
| **`DB1.DBD14`**| `plc_motorCurrentAmp` | `REAL` | `float` | 4 byte (IEEE 754)| PLC -> SCADA | Anlık Motor Akımı (A) |
| **`DB1.DBD18`**| `plc_motorTemperature`| `REAL` | `float` | 4 byte (IEEE 754)| PLC -> SCADA | Motor Sıcaklığı (°C) |
| **`DB1.DBW22`**| `plc_alarmCode` | `INT` | `short` | 2 byte (Word) | PLC -> SCADA | Hata Kodu (0: Normal, 1: EStop, 2: Termik, 3: Jam, 4: WDog)|

> **Toplam DB Boyutu:** 24 Byte (0..23). C# S7NetPlus ile tek bir asenkron çağrıda `plc.ReadBytesAsync(DataType.DataBlock, 1, 0, 24)` veya `plc.ReadClassAsync(model, 1)` şeklinde tüm paket **< 5ms** içinde okunabilir.

---

## 4. Hata ve Alarm Kodları Tablosu

| Alarm Kodu (`plc_alarmCode`) | Alarm Tanımı | Tetikleyen Durum | Alınan Güvenlik Önlemi | Sıfırlama Koşulu |
| :---: | :--- | :--- | :--- | :--- |
| `0` | **Sistem Normal** | Hata yok | Normal çalışma serbest | - |
| `1` | **Acil Stop (E-Stop)** | `%I0.2` kontağı açıldı veya SCADA `cmdEStop` aktif | Tüm motorlar ve valfler derhal enerjisiz bırakılır | Buton kurulmalı + SCADA Reset |
| `2` | **Termik / Sürücü Arızası** | `%I0.3` aşırı akım kontağı açıldı | Motor durdurulur, aşırı ısınma engellenir | Şalter kurulmalı + SCADA Reset |
| `3` | **Konveyör Tıkanması (Jamming)** | `%I0.4` fotoseli > 4 sn kesintisiz TRUE | Ürün sıkışmasını önlemek için bant durdurulur | Parça temizlenmeli + SCADA Reset |
| `4` | **Watchdog İletişim Kesintisi**| SCADA kalp atışı > 3 sn boyunca değişmedi | Hat güvenliğe alınır, otomatik duruş yapılır | Haberleşme tekrar kurulmalı + Reset |
