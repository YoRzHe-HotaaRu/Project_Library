# Project Library

[![Platform: Windows](https://img.shields.io/badge/platform-Windows_10%20%2F%2011-blue.svg)](#)
[![Tech Stack: WPF + C#](https://img.shields.io/badge/tech_stack-WPF_%2B_C%23-purple.svg)](#)
[![Database: SQLite](https://img.shields.io/badge/database-SQLite-green.svg)](#)

A native Windows desktop application that acts as a "Steam Library" for your development projects. Add your project folders, browse them in a beautiful, highly stylized UI, auto-detect their tech stacks, and launch them in your favorite editor with a single click.

---

## Features

- **Visual Aesthetic**: Ink-stroke borders (3px solid ink), halftone screentone patterns, warm paper backgrounds, and diagonal speed-line effects.
- **Smart Tech Auto-Detection**: Scans project directories to auto-identify project types (React, Node, Python, Rust, Go, Java, C#, Markdown, HTML/CSS).
- **One-Click Launch**: Open your projects directly in VS Code, Terminal, Visual Studio, or File Explorer.
- **Library Grid & List Views**: Real-time searching, sorting (Name, Date Added, Last Active), and filtering by custom tags or tech stacks.
- **Favorites/Pinning**: Pin your most important projects to show them on the Home Hero carousel.
- **Tag Management**: Define custom tags and see them styled as unique speech bubbles.

---

## Design Vision & Theme

The application employs a curated light theme with a custom ink-and-paper aesthetic and a modern ruby accent:

| Token | Hex | Usage |
|-------|-----|-------|
| `--ruby` | `#E0115F` | Primary accent, CTAs, active states |
| `--ruby-dark` | `#9B1B30` | Hover states, darker accent |
| `--ruby-light` | `#F5A0B8` | Light backgrounds, subtle highlights |
| `--ink` | `#1A1A1A` | Borders, primary text, manga ink |
| `--paper` | `#F8F5F0` | Main background, warm manga paper |
| `--paper-alt` | `#FFFFFF` | Card/panel backgrounds |
| `--halftone` | `#E8E4E0` | Dot patterns, subtle shading |

---

## Application Architecture

The project is designed using the **MVVM (Model-View-ViewModel)** architectural pattern:

```
ProjectLibrary/
├── Converters/        # UI Value Converters for Boolean/Visibility transformations
├── Helpers/           # Constants, TechRegistry, and application resources
├── Models/            # Data entities mapped to SQLite (Project, Tag, Activity, TechInfo)
├── Services/          # Backend logic (DatabaseService, ProjectScanner, TechDetector, LauncherService)
├── Themes/            # Styling dictionaries (MangaColors, MangaTypography, HalftonePattern, etc.)
├── ViewModels/        # Application states (MainViewModel, HomeViewModel, LibraryViewModel, etc.)
├── Views/             # XAML views (MainWindow, HomeView, LibraryView, AddProjectDialog, etc.)
├── App.xaml           # Application resources and ViewModel-to-View DataTemplates
└── MainWindow.xaml    # Shell layout (Top bar, navigation, page content area)
```

---

## Getting Started

### Prerequisites
- **Operating System**: Windows 10 or 11
- **IDE**: Visual Studio 2022 (with .NET Desktop Development workload)
- **SDK**: .NET 6.0 or newer
- **Third-Party Packages**:
  - `MaterialDesignThemes` (WPF UI elements)
  - `System.Data.SQLite` (Embedded local database)

### Installation & Build
1. Clone this repository:
   ```bash
   git clone https://github.com/yourusername/project-library.git
   cd project-library
   ```
2. Open the solution file `ProjectLibrary.sln` in Visual Studio 2022.
3. Restore NuGet packages.
4. Set the build configuration to `Debug` or `Release`.
5. Press `F5` or click **Start** to build and run the application.

---

## Auto-Detection Signature Matrix

The `TechDetector` service automatically detects project types based on local folder files:

| File Signature | Detected Technology |
|----------------|---------------------|
| `package.json` | JavaScript / Node.js (checks deps for React, Vue, Angular, Next.js) |
| `requirements.txt` / `pyproject.toml` | Python |
| `Cargo.toml` | Rust |
| `go.mod` | Go |
| `pom.xml` / `build.gradle` | Java |
| `*.csproj` / `*.sln` | C# / .NET |
| `index.html` | Static Web (HTML/CSS) |
| `*.md` | Markdown |

---

## License
This project is licensed under the MIT License - see the LICENSE file for details.
