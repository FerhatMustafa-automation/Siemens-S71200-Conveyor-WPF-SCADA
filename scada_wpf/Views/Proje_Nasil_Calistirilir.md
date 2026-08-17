# 🚀 PROJE NASIL ÇALIŞTIRILIR? (ADIM ADIM KURULUM VE DEVREYE ALMA REHBERİ)

**Proje:** 3D Dijital İkiz (Factory I/O) ⟷ Siemens SIMATIC S7-1200 ⟷ C# .NET 8 WPF SCADA  
**Konum:** `01-Digital-Twin-FactoryIO-S71200-WPF-SCADA`

Bu rehber; projeyi bilgisayarınızda sıfırdan kurup çalıştırmanız için yapmanız gereken tüm teknik adımları sırasıyla anlatmaktadır.

---

## 🛠️ 1. Gerekli Programlar ve Ön Hazırlık

Projeyi tam entegre çalıştırmak için bilgisayarınızda aşağıdaki yazılımların bulunması yeterlidir:
1. **Siemens TIA Portal (V17, V18 veya V19)** & **S7-PLCSIM**
2. **Factory I/O (v2.5 veya üstü)**
3. **.NET 8 SDK** (veya Visual Studio 2022 / VS Code)

---

## 💻 2. ADIM 1: TIA Portal ve PLC Programının Yüklenmesi

### 2.1. Yeni Proje ve Donanım Seçimi
1. TIA Portal'ı açın ve **"Create new project"** deyin.
2. **"Add new device"** -> **"Controllers"** -> **"SIMATIC S7-1200"** -> **"CPU 1214C DC/DC/DC"** (Örn: `6ES7 214-1AG40-0XB0`) modelini seçin.
3. PLC Ethernet portuna tıklayıp IP adresini kontrol edin: `192.168.0.1` (Subnet: `255.255.255.0`).

### 2.2. KRİTİK GÜVENLİK AYARI (PUT/GET İznini Açmak)
> [!IMPORTANT]
> Bu ayar yapılmazsa C# SCADA programı PLC'ye bağlanamaz!

1. Sol ağaçta CPU'ya sağ tıklayıp **"Properties"** (Özellikler) penceresini açın.
2. **"General"** sekmesi altında sol menüden **"Protection & Security"** başlığına gelin.
3. **"Connection mechanisms"** altındaki:
   - ✅ **"Permit access with PUT/GET communication from remote partner"** kutucuğunu **İŞARETLEYİN**.

### 2.3. SCL Kaynak Dosyalarını İçeri Aktarma
1. Sol taraftaki proje ağacında **"External source files"** klasörüne sağ tıklayın -> **"Add new external file"** seçin.
2. Proje klasörümüzdeki şu 2 dosyayı seçin:
   - `plc/DB_SCADA_Exchange.scl`
   - `plc/FB_ConveyorDigitalTwin.scl`
3. Eklenen dosyalara sırayla sağ tıklayıp **"Generate blocks from source"** seçeneğine tıklayın.
   - TIA Portal otomatik olarak **`DB1 (DB_SCADA_Exchange)`** ve **`FB1 (FB_ConveyorDigitalTwin)`** bloklarını derleyip oluşturacaktır.

> [!NOTE]
> `DB1` bloğunun özelliklerinde **"Optimized block access"** ayarı otomatik olarak **KAPALI** gelir. Bu sayede C# programı mutlak byte ofsetleri ile doğrudan haberleşir.

### 2.4. Fonksiyon Bloğunu OB1 (Main) İçine Çağırma
1. **"Program blocks"** -> **"Main [OB1]"** bloğunu çift tıklayarak açın.
2. Sol ağaçtaki **`FB1 (FB_ConveyorDigitalTwin)`** bloğunu Network 1 içine sürükleyip bırakın.
3. Çıkan pencerede Instance DB adını onaylayın (`DB_Conveyor_Instance`).
4. Giriş/Çıkış bacaklarını aşağıdaki donanım adreslerine bağlayın:
   - `di_StartButton` -> `%I0.0`
   - `di_StopButton` -> `%I0.1`
   - `di_EmergencyStop` -> `%I0.2`
   - `di_ThermalRelayOK` -> `%I0.3`
   - `di_SensorEntry` -> `%I0.4`
   - `di_SensorExit` -> `%I0.5`
   - `di_SensorMetalSort` -> `%I0.6`
   - `di_PusherFrontLimit` -> `%I0.7`
   - `di_PusherBackLimit` -> `%I1.0`
   - `dq_ConveyorEntry` -> `%Q0.0`
   - `dq_ConveyorExit` -> `%Q0.1`
   - `dq_PusherExtend` -> `%Q0.2`
   - `dq_StackLightGreen` -> `%Q0.3`
   - `dq_StackLightYellow` -> `%Q0.4`
   - `dq_StackLightRed` -> `%Q0.5`
   - `dq_AlarmHorn` -> `%Q0.6`
   - `aq_SetSpeedAnalog` -> `%QW64`

### 2.5. PLCSIM Simülasyonunu Başlatma
1. TIA Portal üst menüsündeki bilgisayar simgesine tıklayın: **"Start simulation"**.
2. Açılan pencerede **"Load"** butonuna basarak programı sanal PLC'ye yükleyin.
3. PLCSIM panelinde CPU durumunu **RUN** moduna (Yeşil LED) getirin.

---

## 🏭 3. ADIM 2: Factory I/O 3D Dijital İkizin Bağlanması

1. **Factory I/O** programını açın.
2. Sahneye şunları yerleştirin (veya hazır konveyör ayırma sahnesi açın):
   - **Giriş Konveyörü (6m):** `%Q0.0`
   - **Çıkış Konveyörü (2m):** `%Q0.1`
   - **Pnömatik İtici (Pusher):** `%Q0.2` (Limit Switchler: `%I0.7` ve `%I1.0`)
   - **Giriş Fotoseli:** `%I0.4` | **Çıkış Fotoseli:** `%I0.5` | **Metal Sensörü:** `%I0.6`
   - **Operatör Panosu:** Start (`%I0.0`), Stop (`%I0.1`), Acil Stop (`%I0.2`)
3. Üst menüden **File -> Drivers** (veya `F4`) sekmesine gidin.
4. Sürücü olarak **"Siemens S7-PLCSIM"** seçin.
5. Sağ üstteki **"Configuration"** butonuna tıklayın:
   - **Model:** `S7-1200`
   - **Digital Inputs:** `2 Byte` (%I0.0 - %I1.7)
   - **Digital Outputs:** `2 Byte` (%Q0.0 - %Q1.7)
6. Geri gelip **"Connect"** butonuna basın. Yanında **Yeşil Onay İşareti (✓)** belirecektir.
7. Factory I/O ekranındaki **Play (▶)** butonuna basarak 3D fizik simülasyonunu başlatın.

---

## 🖥️ 4. ADIM 3: C# .NET 8 WPF SCADA'nın Çalıştırılması

SCADA arayüzünü çalıştırmak için iki yöntemden birini kullanabilirsiniz:

### Yöntem A: Terminal / Komut Satırı ile (Hızlı)
1. PowerShell veya Komut İstemi'ni açın.
2. SCADA klasörüne gidin ve çalıştırın:
   ```powershell
   cd "D:\Calisma ve Test yeri\OT git projeleri hazırlığı\01-Digital-Twin-FactoryIO-S71200-WPF-SCADA\scada_wpf"
   dotnet run
   ```

### Yöntem B: Visual Studio ile
1. `scada_wpf/DigitalTwinScada.csproj` dosyasını Visual Studio 2022 ile açın.
2. Üst menüdeki yeşil **"DigitalTwinScada"** veya `F5` tuşuna basarak projeyi başlatın.

---

## 🎯 5. ADIM 4: Canlı Bağlantı ve Test Senaryoları

SCADA ekranı açıldığında:

### 1. PLC'ye Bağlanma
- Üst paneldeki **PLC IP** kutusuna `127.0.0.1` (PLCSIM için) veya `192.168.0.1` yazın.
- **"Bağlan"** butonuna tıklayın. Durum lambası **Yeşil** yanacak ve *"PLC Bağlantısı Başarılı"* yazacaktır.

### 2. Normal Çalıştırma Testi
- Yeşil **"SİSTEMİ BAŞLAT (START)"** butonuna basın.
- **Gözlem:** Factory I/O'da konveyör dönmeye başlar, sinyal kulesinde yeşil ışık yanar, geçen kutular SCADA'daki **"Sağlam Ürün"** sayacını artırır.

### 3. Hatalı Ürün Ayırma (Sorting) Testi
- Hattan metal veya farklı bir parça geçtiğinde indüktif sensör algılar.
- Pnömatik piston anında açılarak parçayı yan kaydırağa iter.
- SCADA'daki **"Ayrılan (Defect)"** sayacı 1 artar.

### 4. Hız Değiştirme Testi
- SCADA sol panelindeki hız slider'ını `1200 RPM` değerine çekip **"Hız Değerini Gönder (DBW2)"** butonuna basın.
- Konveyörün daha hızlı döndüğünü ve motor akımının (Amper) yükseldiğini canlı görün.

### 5. Sıkışma / Jamming Arıza Testi
- Factory I/O içinde farenizle bir kutuyu giriş fotoselinin önünde **4 saniye sabit tutun**.
- **Gözlem:** PLC durumu `State 99 (Arıza)` yapar. Hat otomatik durur ve SCADA ekranında kırmızı banner açılır:
  `⚠️ AKTİF ALARM: Konveyör Ürün Tıkanması (Jamming)! (Kod: 3)`
- Kutuyu çektikten sonra SCADA'dan **"Alarmı Sıfırla (Reset)"** butonuna basarak sistemi normale döndürün.

### 6. Acil Stop (E-Stop) Güvenlik Testi
- SCADA ekranındaki büyük kırmızı **"🛑 ACİL DURDURMA"** butonuna basın.
- Bütün motorlar anında enerjisiz kalır, siren öter ve sistem kilitlenir.
- Butona tekrar basıp kilidi açmadan ve Reset vermeden sistem yeniden başlatılamaz.

---

## ❓ 6. Sık Karşılaşılan Sorunlar ve Çözümleri (Troubleshooting)

| Sorun / Hata | Olası Neden | Kesin Çözüm |
| :--- | :--- | :--- |
| **"Haberleşme Hatası: Connection refused / Port 102"** | TIA Portal'da PUT/GET izni verilmemiştir. | CPU Properties -> Protection & Security -> *Permit access with PUT/GET communication* kutusunu işaretleyip PLC'ye tekrar yükleyin. |
| **"Veriler SCADA'da yanlış veya 0 görünüyor"** | DB1 Optimized olarak oluşturulmuştur. | `DB_SCADA_Exchange` özelliklerinden *Optimized block access* işaretini kaldırın. |
| **"Factory I/O PLCSIM'e bağlanmıyor (Kırmızı Çarpı)"** | PLCSIM RUN modunda değildir veya I/O boyutu 2 Byte seçilmemiştir. | PLCSIM'i RUN moduna alın; Factory I/O Drivers -> Configuration -> Inputs/Outputs değerini `2 Byte` yapın. |
| **"dotnet run çalışmıyor"** | .NET 8 SDK yüklü değildir. | [dotnet.microsoft.com](https://dotnet.microsoft.com) adresinden .NET 8 SDK yükleyin. |
