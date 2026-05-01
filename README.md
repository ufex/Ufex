# Ufex — Universal File Explorer

Ufex is a cross-platform desktop application for inspecting the internal structure and metadata of many different file formats. Open any file and instantly see its hex data, parsed structure, visual layout, and validation results — all in one place.

## Getting Started

Downloads: https://github.com/ufex/Ufex/releases

## Features

- **File Format Identification** — Automatically identifies file types using a signature-based database of magic bytes, file extensions, and MIME types. The database is defined in XML config files and can be extended without writing any code.
- **Hex Viewer** — Browse the raw bytes of any file with synchronized hex and ASCII columns.
- **Info Tab** — View file system attributes (size, timestamps, permissions) and format-specific metadata at a glance.
- **Structure Tab** — Explore parsed file segments in an interactive tree view. Select any node to see its fields, offsets, and decoded values in a detail panel.
- **Visual Tab** — See graphical representations of file data such as file maps that show how a file is divided into segments, embedded image previews, and more.
- **Validation Tab** — Review format-specific validation results (info, warnings, errors) to check if a file conforms to its specification.
- **Number Format Toggle** — Switch between hexadecimal, decimal, binary, and ASCII display modes for numeric values throughout the UI.
- **Drag & Drop** — Open files by dragging them onto the window or by using the file browser.
- **Search** — Find byte patterns and text within files.

## Plugin Architecture

Ufex is designed to be extended via **.NET assembly plugins**. Each plugin teaches Ufex how to parse, display, and validate a specific file format (for example PNG, ZIP, BMP, or PDF).

Plugins are standard .NET class library projects that reference the `Ufex.API` library and are loaded dynamically at runtime from the `plugins/` directory. This means third-party developers can add support for new file formats without modifying the core application.

## Tech Stack

| Component | Technology |
|-----------|------------|
| Language | C# |
| Runtime | .NET 10 |
| UI Framework | [Avalonia UI](https://avaloniaui.net/) 11.2 |
| UI Theme | Avalonia Fluent Theme |
| Icons | [Fluent Icons](https://github.com/nicehash/FluentIcons.Avalonia) |
| Text/Code Editing | [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit) |
| CI/CD | GitHub Actions |
| Installer (Windows) | MSI via [WiX Toolset](https://wixtoolset.org/) |
| Installer (macOS) | DMG (Intel x64 + Apple Silicon arm64) |
| Installer (Linux) | AppImage |

## Project Structure

```
ufex/
├── src/
│   ├── Ufex.Desktop           # Main desktop application (Avalonia)
│   ├── Ufex.API               # Public API & base classes for plugins
│   ├── Ufex.FileType          # File type identification engine
│   └── Ufex.Controls.Avalonia # Reusable Avalonia UI controls
├── config/                    # XML file-type signature database
├── ext/                       # Extension plugin source (git submodules)
├── tests/                     # Unit tests
├── build/                     # Build output staging
└── .github/workflows/         # CI/CD pipeline
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

### Build & Run

```bash
# Clone the repository
git clone https://github.com/ufex/Ufex.git
cd Ufex

# Restore dependencies and build
dotnet build ufex.sln

# Run the application
dotnet run --project src/Ufex.Desktop
```

### Running Tests

```bash
dotnet test ufex.sln
```

### Publish a Self-Contained Release

Ufex release builds are self-contained — users do not need to install the .NET runtime separately.

```bash
# Windows
dotnet publish src/Ufex.Desktop/Ufex.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# macOS (Apple Silicon)
dotnet publish src/Ufex.Desktop/Ufex.Desktop.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true

# Linux
dotnet publish src/Ufex.Desktop/Ufex.Desktop.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

## Contributing

Contributions are welcome! Here's how to get started:

1. **Fork** the repository and create a feature branch.
2. **Follow the code style:**
   - Use **tabs** for indentation.
   - Use **PascalCase** for classes, methods, properties, and constants.
   - Use **camelCase** for local variables and parameters.
3. **Write tests** for new functionality where applicable.
4. **Submit a pull request** with a clear description of your changes.

### Ways to Contribute

- **Add file type signatures** — Extend the XML config files in `config/` to identify more file formats (no C# required).
- **Build a plugin** — Create a new .NET class library that parses and displays a file format. 
- **Improve the core app** — Bug fixes, UI improvements, new features for the desktop application.
- **Documentation** — Improve or add documentation for the project or individual file formats.

## License

Ufex is licensed under the [Apache License 2.0](LICENSE).
