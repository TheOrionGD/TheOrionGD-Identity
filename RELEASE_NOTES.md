# TheOrionGD Identity Widget v1.0.0 — Official Launch

Welcome to the initial release of **TheOrionGD Identity Widget** (`v1.0.0`), a lightweight, high-performance 32bpp hardware-composited Windows desktop HUD identity dashboard representing **Godfrey T. R** ([@TheOrionGD](https://github.com/TheOrionGD)).

Built with **C# 12** and **.NET 8.0 Windows Forms** using direct Win32 GDI+ interop, this widget provides a seamless, cyberpunk-inspired visual interface showcasing engineering focus domains, core technical competencies, verified internship milestones, and verified social profiles.

---

## ⚡ Highlights & Key Capabilities

- **32bpp Hardware-Composited Layered Window (`WS_EX_LAYERED`)**:
  - Leverages low-level Win32 `UpdateLayeredWindow` with an offscreen 32-bit ARGB DIB section.
  - Features true per-pixel alpha transparency, soft outer shadows, and zero redraw flicker or tearing.
- **Industrial Cyberpunk Aesthetic**:
  - Engineered with a sleek, dark charcoal palette (`#0D0F11` / `#111315`), subtle surface elevations, and vivid crimson energy accents (`#E53935` / `#FF3B30`).
- **Dynamic Particle Constellation Network**:
  - Real-time ambient cybernetic particles floating seamlessly across the canvas with proximity-based energy connection filaments drifting behind HUD cards with near-zero CPU footprint.
- **Smooth 30 FPS Motion Engine**:
  - Non-blocking timer controlling breathing pulse glows, animated orbital avatar rings, and dynamic particle physics.
- **Interactive Hitbox Routing & Quick Actions**:
  - Cursor tracking across focus cards, skill pills, internship timeline cards, and platform connection buttons.
  - Direct one-click access to Portfolio ([the-orion-gd.vercel.app](https://the-orion-gd.vercel.app/)), GitHub, LinkedIn, HackerRank, and YouTube.
- **System Tray Companion**:
  - Minimizes smoothly to the Windows notification tray with a custom-generated brand icon (`OGD`).
  - Right-click context menu for instant profile navigation and window toggling.
- **Per-Monitor V2 DPI Scaling**:
  - Dynamic bounds, typography, and line rendering recalculation ensuring crisp display across 1080p, 1440p, and 4K screens.
- **Headless CLI Preview Engine**:
  - Run with `--preview <output.png>` to render pixel-perfect snapshots to disk without opening a GUI window.

---

## 📦 Release Assets & Binaries

| Asset | Type | Description |
| :--- | :--- | :--- |
| **`OrionGDWidget.exe`** | Executable | Direct standalone application binary (requires .NET 8.0 Desktop Runtime) |
| **`OrionGDWidget-v1.0.0-windows-x64.zip`** | Zip Archive | Complete release package including `OrionGDWidget.exe`, dependencies, and runtime configuration |

### Checksums (SHA-256)

```text
OrionGDWidget.exe:
3DF499E3A9F44ED386ADFCAA7A2D9022C5FBC29932EE4776A18395DEFEC05637

OrionGDWidget-v1.0.0-windows-x64.zip:
7051C8BE996F2285165CFBB24EE8856ED2CBD44E9473B0B53AEF585341954E0D
```

---

## 💻 System Requirements & Running

- **Operating System**: Windows 10 (Build 1809+) or Windows 11 (64-bit)
- **Runtime**: [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (x64)

### Quick Start

1. Download `OrionGDWidget-v1.0.0-windows-x64.zip` (or `OrionGDWidget.exe`).
2. Extract the archive into a folder of your choice.
3. Double-click `OrionGDWidget.exe` to launch the HUD widget.
4. Drag to reposition anywhere on your desktop. Use the `_` button or system tray icon to minimize/restore.

---

**Developed with precision by [Godfrey T. R (@TheOrionGD)](https://github.com/TheOrionGD)**
