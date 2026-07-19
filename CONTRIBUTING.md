# Contributing to DotElectric

Thank you for your interest in contributing to DotElectric! This document provides guidelines and information for developers.

## Development Setup

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2026 or JetBrains Rider 2026+
- Git

### Build Commands

```bash
# Build solution
dotnet build src/DotElectric.TemplateEditor.slnx

# Run application
dotnet run --project src/DotElectric.TemplateEditor

# Run all tests
dotnet test src/DotElectric.TemplateEditor.Tests

# Run single test
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~YourTestName"

# Run tests with coverage
dotnet test src/DotElectric.TemplateEditor.Tests --collect:"XPlat Code Coverage"
```

## Architecture

### Key Principles
- **MVVM** with CommunityToolkit.Mvvm
- **DI** via Microsoft.Extensions.DependencyInjection (all services Singleton)
- **Fixed-point coordinates** in microns (`long`, not double)
- **1mm = 1000 microns**
- **Coordinate system:** Model = Cartesian (Y↑), WPF = Inverted Y (Y↓)

### Project Structure
```
src/
├── DotElectric.TemplateEditor/          # Main WPF application
│   ├── Commands/                        # Undo/Redo commands
│   ├── Constants/                       # PhysicalConstants, EditorSettings
│   ├── Converters/                      # Value converters (16 files)
│   ├── Helpers/                         # Utility classes
│   ├── Models/                          # Domain models (microns)
│   │   └── Objects/                     # TemplateObjectBase, Rectangle, Line, Text
│   ├── Services/                        # File, Settings, Autosave, Print
│   ├── Tools/                           # State pattern tools
│   ├── ViewModels/                      # MVVM ViewModels
│   │   └── Managers/                    # 9 managers (ZoomPan, Selection, Clipboard, Tool, Preview, InlineEdit, StatusBar, Grid, DirtyState)
│   ├── Behaviors/                       # Attached behaviors (EditorCanvas, PreviewLine, TabItem, TextBox, ComboBox, ZoomCombo)
│   └── WpfCommands/                     # WPF command routing (ToolCommands)
└── DotElectric.TemplateEditor.Tests/    # xUnit v3 tests (~1840+ tests)
```

### Coding Standards
- Use `long` for all coordinates (microns), never `double`
- Follow C# naming conventions (`TemplateObjectBase`, not `ITemplateObject`)
- Use `PhysicalConstants` and `EditorSettings` for all thresholds and limits
- MVVM: ViewModels should not know about WPF types
- Commands implement `DotElectric.TemplateEditor.Commands.IUndoCommand` (NOT `System.Windows.Input.ICommand`)
- All DI services are Singleton; use `IEditorViewModelFactory` for EditorViewModel

### Testing
- xUnit v3 with Moq
- Target coverage: 80%+ line-rate
- Test naming: `MethodName_Scenario_ExpectedResult`
- Mock WPF dependencies (dialogs, services)
- Behaviors should be tested via unit tests (not just integration)

## Git Workflow

### Branch Naming
- `feature/description` — new features
- `bugfix/description` — bug fixes
- `refactor/description` — refactoring
- `docs/description` — documentation

### Commit Messages
Follow [Conventional Commits](https://www.conventionalcommits.org/):
```
feat: add symbol editor panel
fix: correct line hit-testing tolerance
refactor: extract magic numbers to PhysicalConstants/EditorSettings
docs: update AGENTS.md with Sprint 29 metrics
test: add CustomResizeCommand tests
```

### Pull Requests
- Include description of changes
- Link to related issues
- Ensure CI passes (build + tests)
- Update CHANGELOG.md for user-facing changes

## Sprint Process

Sprints are 1-week cycles tracked in `docs/`:
- `docs/47_План_развития_Этап2.md` — Roadmap
- `docs/00_Индекс_документов.md` — Document index
- `AGENTS.md` — Detailed sprint reports (All Common Mistakes and fixes)

## Common Mistakes to Avoid

1. Don't use `double` for coordinates — use microns (`long`)
2. Don't create new Shape on every MouseMove — update properties instead
3. Don't do hit-testing on MouseMove — only on MouseDown
4. Don't use Grid/StackPanel in EditorCanvas — use Canvas
5. Always use `Mode=OneWay` when binding to readonly properties
6. IsDirty must be set by commands (`MarkDirty()`), NOT manually
7. Preview shapes: create once, update properties only, then re-assign reference
8. EditorViewModel — instantiate via `IEditorViewModelFactory`, NOT `new` directly
9. Drag delta from `_initialPositions[obj]`, NOT current `obj.MicronsX` (already updated)
10. Pan delta from Window-relative coordinates, NOT `e.GetPosition(canvas)` (RenderTransform shift)
11. After Undo/Redo — always purge orphaned objects from `SelectedObjects`
12. Rectangle hit-test — use border-band approach, NOT full AABB
13. `PreviewKeyDown` for tool switching — check `ModifierKeys.None` before handling V/L/R/T
14. `GetClipboardContents()` — clone objects on EVERY call, not only during `Copy()`
15. Type guard in SetProperty helper — use `is not` pattern, NEVER direct cast `((Line)SelectedObject!)`

## Resources

- `AGENTS.md` — Quick reference for AI assistants
- `docs/00_Индекс_документов.md` — Document index
- `docs/03_Спецификация_требований_Этап1.md` — Architecture and API spec
