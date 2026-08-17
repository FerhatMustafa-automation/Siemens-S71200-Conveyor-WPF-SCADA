# Factory I/O 3D Sahne Mimarisi ve Sinyal Eşleştirmesi

Bu doküman; **Factory I/O 3D Dijital İkiz Sahnesi**'nin Siemens S7-1200 PLC ve C# WPF SCADA ile tam entegre çalışması için gerekli sahne yerleşimini, donanım sürücüsü ayarlarını ve tag eşleştirmelerini açıklar.

---

## 1. 3D Sahne Bileşenleri ve Yerleşim Şeması

Sahne, tipik bir **Endüstriyel Parça Taşıma, Kalite Kontrol ve Hatalı Parça Ayırma Hattı** olarak modellenmiştir.

```text
 [Ürün Besleyici / Emitter]
           │
           ▼
 ┌─────────────────────────────────────────────────────────────┐
 │  Giriş Konveyörü (Conveyor Entry 6m - %Q0.0)                │
 │  ├── [Giriş Fotoseli (Diffuse Sensor) - %I0.4]              │
 │  │                                                          │
 │  ├── [Metal / Ayırma Sensörü (Inductive Sensor) - %I0.6]    │
 │  │                                                          │
 │  └── [Pnömatik İtici / Pusher (%Q0.2) + Limitler %I0.7/%I1.0] │ ──► [Hatalı Ürün Kaydırağı (Chute)]
 └─────────────────────────────────────────────────────────────┘
           │
           ▼
 ┌─────────────────────────────────────────────────────────────┐
 │  Çıkış Konveyörü (Conveyor Exit 2m - %Q0.1)                 │
 │  └── [Çıkış Sayıcı Fotoseli (Retroreflective Sensor) - %I0.5]│
 └─────────────────────────────────────────────────────────────┘
           │
           ▼
 [Ürün Kabul / Remover]
```

### Kullanılan 3D Parçalar:
1. **Belt Conveyor (6m & 2m):** Değişken hızlı, dijital tetiklemeli ve analog hız kontrollü konveyörler.
2. **Pusher (Pnömatik İtici):** Çift etkili pnömatik silindir ve 5/2 tek bobin solenoid valf simülasyonu.
3. **Diffuse Sensor (Cisimden Yansımalı Fotosel):** Parçanın banta girdiğini algılar (`%I0.4`).
4. **Inductive / Capacitive Sensor:** Metal veya hatalı ürünleri tespit eder (`%I0.6`).
5. **Retroreflective Sensor (Reflektörlü Fotosel):** Sağlam parçaların hattan çıktığını sayar (`%I0.5`).
6. **Electric Panel (Operatör Paneli):** Yeşil Start Butonu (`%I0.0`), Kırmızı Stop Butonu (`%I0.1`), Acil Stop Mantar Butonu (`%I0.2`).
7. **Stack Light (Sinyal Kulesi):** Yeşil (`%Q0.3`), Sarı (`%Q0.4`), Kırmızı (`%Q0.5`) ve Sesli Alarm (`%Q0.6`).
8. **Item Emitter & Remover:** Otomatik palet/kutu/metal silindir besleme ve hattan tahliye üniteleri.

---

## 2. Factory I/O Driver (Sürücü) Ayarları

### Siemens S7-PLCSIM / S7-1200 Sürücü Konfigürasyonu:
1. Factory I/O menüsünden **File -> Drivers** seçilir.
2. Sürücü tipi olarak **Siemens S7-1200/1500** veya **Siemens S7-PLCSIM** seçilir.
3. **Configuration** sekmesinde aşağıdaki parametreler ayarlanır:
   - **Model:** S7-1200
   - **Host:** `192.168.0.1` (veya PLCSIM sanal ethernet IP'si)
   - **Rack:** `0`
   - **Slot:** `1`
   - **Digital Inputs Size:** `2 Bytes` (%I0.0 - %I1.7)
   - **Digital Outputs Size:** `2 Bytes` (%Q0.0 - %Q1.7)
   - **Analog Outputs Size:** `1 Word` (%QW64)

---

## 3. Factory I/O Tag - PLC Adres Eşleştirme Tablosu

| Factory I/O Sinyali (Tag) | Sinyal Tipi | PLC Adresi | Açıklama |
| :--- | :--- | :--- | :--- |
| **Start Button (NO)** | Bool Giriş | `%I0.0` | Panodaki Başlat Butonu |
| **Stop Button (NC)** | Bool Giriş | `%I0.1` | Panodaki Durdur Butonu (Normalde 1) |
| **Emergency Stop (NC)** | Bool Giriş | `%I0.2` | Acil Durdurma Mantar Buton |
| **Thermal OK Contact (NC)** | Bool Giriş | `%I0.3` | Motor Koruma Şalter Kontağı |
| **Entry Sensor** | Bool Giriş | `%I0.4` | Giriş Parça Algılama Fotoseli |
| **Exit Sensor** | Bool Giriş | `%I0.5` | Çıkış Parça Sayıcı Fotosel |
| **Metal / Defect Sensor** | Bool Giriş | `%I0.6` | Hatalı Ürün Ayırma Sensörü |
| **Pusher Front Limit** | Bool Giriş | `%I0.7` | Piston İleri Manyetik Sensör |
| **Pusher Back Limit** | Bool Giriş | `%I1.0` | Piston Geri Manyetik Sensör |
| **Conveyor 6m (Entry)** | Bool Çıkış | `%Q0.0` | Giriş Konveyörü Motoru |
| **Conveyor 2m (Exit)** | Bool Çıkış | `%Q0.1` | Çıkış Konveyörü Motoru |
| **Pusher Extend Solenoid**| Bool Çıkış | `%Q0.2` | Ayırıcı Piston Valf Bobini |
| **Light Green** | Bool Çıkış | `%Q0.3` | Sinyal Kulesi Yeşil Işık |
| **Light Yellow** | Bool Çıkış | `%Q0.4` | Sinyal Kulesi Sarı Işık |
| **Light Red** | Bool Çıkış | `%Q0.5` | Sinyal Kulesi Kırmızı Flaşör |
| **Alarm Horn** | Bool Çıkış | `%Q0.6` | Sesli İkaz Sireni |
| **Conveyor Speed (Analog)**| Int Çıkış | `%QW64` | Hız Sürücüsü Set Değeri (0-27648) |

---

## 4. Emitter (Nesne Üretici) ve Test Senaryoları

- **Emitter Konfigürasyonu:**
  - **Malzeme Türleri:** Yeşil Kutu (Standart Ürün), Metal Silindir (Ayrılacak Hatalı Ürün).
  - **Üretim Sıklığı:** Her 3.5 saniyede bir parça.
  - **Rastgelelik Oranı:** %75 Normal Kutu, %25 Metal Kutu.
- **Beklenen Davranış:**
  1. Normal Kutu geldiğinde indüktif sensör tetiklenmez -> İki konveyörden geçip çıkış fotoselinde `plc_pieceCountTotal` sayısını 1 artırır.
  2. Metal Kutu geldiğinde indüktif sensör `%I0.6` tetiklenir -> Konveyör durmadan veya yavaşlayarak Piston `%Q0.2` anında ürünü yan kaydırağa iter -> `plc_pieceCountRejected` sayısını 1 artırır.
  3. Konveyör üzerinde ürün takılı kalırsa -> 4 saniye sonra `Jamming Alarmı (Kod 3)` devreye girer.
