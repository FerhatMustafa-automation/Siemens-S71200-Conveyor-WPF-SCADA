# Teknik Mülakat ve Mühendislik Savunma Notları

Bu doküman; **Dijital İkiz (Factory I/O + Siemens S7-1200 + C# WPF SCADA)** projesinin teknik mülakatlarda, işe alım görüşmelerinde (Makine İmalatçıları OEM & Sistem Entegratörleri) ve **MYK Seviye 5 Otomasyon Sistemleri Programcısı** sınavlarında profesyonelce savunulabilmesi için hazırlanmıştır.

---

## 1. Mimari ve Donanım Seçimleri

### Soru 1: *"Siemens S7-1200 PLC'de SCADA için neden Non-Optimized DB (Standart DB) tercih ettiniz?"*
> **Mühendislik Savunması:**
> *"Siemens TIA Portal'da 'Optimized Block Access' açık olduğunda değişkenler derleyici tarafından rastgele optimize edilir ve dış dünyadan mutlak byte/bit ofsetiyle erişilemez hale gelir. C# tarafında kullandığım `S7NetPlus` ve endüstriyel sürücüler, doğrudan ISO-on-TCP (RFC 1006 / Port 102) üzerinden mutlak bellek ofsetlerine (`DB1.DBX0.0`, `DB1.DBW2` vb.) istek atar. 
> 
> Bu nedenle `DB_SCADA_Exchange` bloğunda `Optimized Block Access` özelliğini bilerek KAPALI tuttum. Böylece 24 byte'lık tüm telemetri ve komut paketini tek bir asenkron TCP paketinde (`ReadClassAsync`) **< 5 milisaniye** içinde okuyarak PLC CPU'suna ve ağ bandına binen yükü minimize ettim."*

---

### Soru 2: *"C# WPF SCADA uygulamasında PLC döngüsü çalışırken arayüzün (UI Thread) donmasını nasıl engellediniz?"*
> **Mühendislik Savunması:**
> *"Endüstriyel SCADA'larda en sık yapılan hata PLC okuma döngülerini UI thread üzerinde çalıştırmaktır. Bu durum ağda 1 saniyelik bir gecikme olduğunda bile butonların tıklanamamasına ve ekranın kilitlenmesine yol açar.
> 
> Ben bu projede **MVVM (Model-View-ViewModel)** mimarisi kurarak PLC haberleşmesini `PlcCommunicationService` içinde tamamen arka plan iş parçacığına (`Task.Run`) devrettim. Döngüyü `async/await` ve `CancellationTokenSource` ile non-blocking olarak yürüttüm. PLC'den gelen verileri UI'a aktarırken ise `Application.Current.Dispatcher.Invoke` ile thread-safe bir şekilde aktardım. Sonuç olarak hat kopsa dahi operatör ekranı donmaz, anında görsel alarm verir."*

---

### Soru 3: *"Sistemde Auto-Reconnect ve Watchdog (Kalp Atışı) mekanizmasını nasıl kurguladınız?"*
> **Mühendislik Savunması:**
> *"İki yönlü bir emniyet kurguladım:
> 1. **SCADA Tarafında (Auto-Reconnect):** Eğer saha kablosu kopar veya PLC kapanırsa servis `Exception` fırlatıp çökmek yerine soketi temiz kapatır, UI'da kırmızı 'Bağlantı Kesildi' uyarısı verir ve 3 saniyede bir arka planda otomatik yeniden bağlanmayı dener (`Auto-Reconnect`).
> 2. **PLC Tarafında (Watchdog Heartbeat):** SCADA her 500ms'de bir `DB1.DBX0.7` bitini tersler (toggle). PLC'deki SCL kodunda `tonWatchdog` zamanlayıcısı bu bitin değişip değişmediğini kontrol eder. Eğer SCADA bilgisayarı çöker veya kablo koparsa ve 3 saniye boyunca sinyal gelmezse PLC bunu 4 numaralı arıza olarak algılar ve tüm konveyör motorlarını güvenli duruşa (Safe Stop) geçirir."*

---

### Soru 4: *"Saha güvenliği ve E-Stop (Acil Stop) mekanizmasını hem yazılımda hem donanımda nasıl modellediniz?"*
> **Mühendislik Savunması:**
> *"Endüstriyel otomasyonda (ISO 13849-1 ve IEC 62061 standartlarına göre) acil durdurma **asla sadece yazılıma emanet edilemez**. 
> - **Donanım Katmanı:** Fiziksel panodaki E-Stop butonu çift kanal (NC) emniyet rölesi üzerinden motor kontaktörlerinin bobin enerjisini doğrudan keser. Aynı zamanda PLC'nin `%I0.2` dijital girişine bilgi verir.
> - **Yazılım Katmanı:** PLC SCL kodunda `#di_EmergencyStop` kontağı açıldığı an durum makinesi derhal `State 99 (Arıza / Güvenli Duruş)` durumuna geçer, sinyal kulesinde kırmızı lamba ve siren aktif edilir. SCADA arayüzünde de acil durum banner'ı açılarak sistem kilitlenir. Acil stop fiziksel olarak kurulmadan ve SCADA'dan reset verilmeden motorların tekrar başlatılması imkansızdır."*

---

### Soru 5: *"Bu 3D Dijital İkiz (Factory I/O) projesini yarın fabrikadaki gerçek bir hatta nasıl entegre edersiniz?"*
> **Mühendislik Savunması:**
> *"Bu projenin en büyük gücü, PLC kodunun ve SCADA mimarisinin tamamen gerçek donanım standartlarında (IEC 61131-3) yazılmış olmasıdır. 
> 
> Sahaya çıkıldığında yapılması gereken tek şey:
> 1. TIA Portal'da PLCSIM yerine sahada panoda bulunan gerçek **S7-1200 CPU 1214C** donanımına programı yüklemek.
> 2. Pano klemenslerine gerçek fotoselleri (PNP NO), limit switchleri ve kontaktör bobinlerini I/O haritasına göre bağlamak.
> 3. C# SCADA uygulamasında PLC IP adresini sahadaki endüstriyel switch IP'si (`192.168.0.10` vb.) olarak girmektir. Hiçbir C# veya SCL satırını değiştirmeye gerek kalmadan sistem canlıya alınabilir."*

---

## 2. MYK Seviye 5 Sınavı ile Eşleştirme

| MYK Yeterlilik Birimi | Bu Projedeki Uygulama Karşılığı |
| :--- | :--- |
| **A1: İş Sağlığı, Güvenliği ve Çevre** | Çift kanal E-Stop tasarımı, motor termik koruması (`%I0.3`), güvenli duruş durum makinesi (`State 99`). |
| **B1: Otomasyon Hazırlığı & I/O Planlama** | Donanım I/O listesi, sensör/eyleyici seçimi (fotosel, pnömatik silindir, 5/2 valf), Non-Optimized DB adres tablosu. |
| **B2: Programlama ve SCADA Entegrasyonu** | IEC 61131-3 SCL modüler fonksiyon bloğu (`FB_ConveyorDigitalTwin`), C# .NET 8 WPF MVVM SCADA arayüzü, S7NetPlus ISO-on-TCP sürücüsü. |
| **B3: Devreye Alma, Simülasyon ve Arıza Arama** | Factory I/O 3D fiziksel simülasyonu, Jamming (sıkışma) arıza testi, Watchdog haberleşme kesilme simülasyonu. |
