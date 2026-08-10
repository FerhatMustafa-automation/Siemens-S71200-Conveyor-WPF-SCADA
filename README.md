# 🏭 Siemens S7-1200 Conveyor & Piston SCADA / HMI System (WPF .NET 8)

![C#](https://img.shields.io/badge/C%23-.NET%208.0%20WPF-blue)
![Siemens](https://img.shields.io/badge/PLC-Siemens%20S7--1200-009999)
![Protocol](https://img.shields.io/badge/Protocol-S7NetPlus%20Ethernet%20TCP%2FIP-orange)
![License](https://img.shields.io/badge/License-MIT-green)

A modern, industrial-grade **SCADA / HMI Control Application** built with **C# WPF (.NET 8)** and **MVVM Architecture** to monitor and control a **Siemens S7-1200 PLC** conveyor & pneumatic piston automation system.

Developed for **ASOSEM (Ankara Chamber of Industry Continuous Education Center)** Industrial Automation Course.

---

## 🌟 Key Features

- **📺 Real-Time SCADA Interface**: Dark industrial SCADA UI featuring animated conveyor belt rollers, moving bottles, laser proximity sensor effects, and pneumatic piston extension/retraction animations.
- **⚡ Dual Mode Operation**:
  - **Live PLC Mode**: Real-time communication with physical **Siemens S7-1200 PLC** over Ethernet TCP/IP via `S7NetPlus`.
  - **Embedded Simulation Engine**: Pure software PLC engine simulating Network 1–4 ladder logic for standalone testing and presentations without requiring physical PLC hardware.
- **🔄 Synchronous Animation & PLC State**: Visual elements (conveyor motor state, piston position, bottle displacement, and count) stay 100% in sync with physical PLC I/O.
- **📊 Digital Telemetry**: Real-time bottle counter (`CTU` CV), Motor status LED (`%Q0.0`), Piston status LED (`%Q0.1`), and Sensor indicator (`%I0.2`).

---

## 📐 Siemens TIA Portal Ladder Logic (Network 1 – 4)

The WPF application strictly adheres to the following TIA Portal Ladder Logic specification:

| Network | PLC Memory / Tag | Logic & Action |
| :--- | :--- | :--- |
| **Network 1** | `%I0.0` (START) | **SET** `%Q0.0` (Conveyor Belt Motor ON) |
| **Network 2** | `%I0.1` (STOP) | **RESET** `%Q0.0` (Conveyor Belt Motor OFF) |
| **Network 3** | `%I0.2` (Sensor) | Increments `CTU` Up-Counter. When `CV >= 3` (Target):<br>• **RESET** `%Q0.0` (Motor Stops)<br>• Trigger `TP` Pulse Timer (`T#2s`) for `%Q0.1` (Piston Extends for 2 seconds)<br>• Counter Resets for next cycle |
| **Network 4** | `%Q0.1` (`-( N )-` Falling Edge) | When Piston retracts (falling edge of 2s pulse), **SET** `%Q0.0` (Motor Automatically Restarts) |

---

## 🖥️ SCADA Interface Preview

```
+-----------------------------------------------------------------------------------+
|                        ASOSEM INDUSTRIAL AUTOMATION LABORATORY                    |
|                Siemens S7-1200 Conveyor & Piston SCADA System                    |
+-----------------------------------------------------------------------------------+
|                                                                                   |
|   [PNEUMATIC PISTON (%Q0.1)]                                                      |
|           |                                                                       |
|           v                                                                       |
|   [SENSOR S1 (%I0.2)]                                                             |
|           |                                                                       |
|   =======[ BOTTLE ]====================================================           |
|   (o)   (o)   (o)   (o)   (o)   (o)   (o)   (o)   (o) [CONVEYOR BELT]           |
|                                                                                   |
|   [MOTOR (%Q0.0): RUNNING]   [PISTON (%Q0.1): RETRACTED]   [BOTTLE COUNT: 2/3]    |
+-----------------------------------------------------------------------------------+
|  [START (%I0.0)]  |  [STOP (%I0.1)]  |  [MANUAL BOTTLE]  |  [CONNECT S7-1200]     |
+-----------------------------------------------------------------------------------+
```

---

## ⚙️ Hardware I/O & Address Mapping

| Hardware / Variable Name | Address | Type | Description |
| :--- | :--- | :--- | :--- |
| `START_BTN` | `%I0.0` / `%M0.0` | `BOOL` | Start Conveyor Belt Motor |
| `STOP_BTN` | `%I0.1` / `%M0.1` | `BOOL` | Emergency Stop Motor |
| `BOTTLE_SENSOR` | `%I0.2` / `%M0.2` | `BOOL` | Optical Proximity Sensor |
| `CONVEYOR_MOTOR` | `%Q0.0` | `BOOL` | Conveyor Belt Motor Drive Output |
| `PISTON_CYLINDER` | `%Q0.1` | `BOOL` | Pneumatic Cylinder Solenoid Valve Output |
| `BOTTLE_COUNTER` | `%MW10` | `INT/WORD` | Up-Counter Current Value (`CTU CV`) |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or [Visual Studio 2022](https://visualstudio.microsoft.com/)
- Siemens TIA Portal V14+ (For PLC deployment)
- Siemens S7-1200 CPU (CPU 1214C / 1212C / 1211C)

### Building & Running via Visual Studio
1. Clone the repository:
   ```bash
   git clone https://github.com/FerhatMustafa-automation/Siemens-S71200-Conveyor-WPF-SCADA.git
   cd Siemens-S71200-Conveyor-WPF-SCADA/KonveyorWpf
   ```
2. Open `KonveyorWpf.sln` in Visual Studio 2022.
3. Press **`F5`** to run the application.

### Running via .NET CLI
```bash
cd KonveyorWpf
dotnet run
```

---

## 🔧 Siemens TIA Portal PLC Setup Guide

To enable communication between C# (S7NetPlus) and the Siemens S7-1200 CPU:

1. Open **TIA Portal** -> Open your project.
2. Double-click **Device Configuration** -> Select **S7-1200 CPU**.
3. Go to **Properties** -> **Protection & Security** -> **Connection mechanisms**.
4. Check **"Permit access with PUT/GET communication from remote partner"**.
5. Set your PC's Ethernet IP to `192.168.0.200` (Subnet: `255.255.255.0`).
6. Download the project to the PLC.
7. Click **"🔌 Connect Siemens PLC"** in the WPF SCADA interface.

---

## 📜 License

Distributed under the MIT License. See `LICENSE` for more information.

---

*Developed by Ferhat Mustafa for ASOSEM Industrial Automation Course.*
