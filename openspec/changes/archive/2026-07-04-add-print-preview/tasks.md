## 1. IPrintDocumentGenerator interface

- [ ] 1.1 Create `Services/IPrintDocumentGenerator.cs` with method `FixedDocument BuildDocument(Template template, Size pageSize)`

## 2. PrintDocumentGenerator implementation

- [ ] 2.1 Create `Services/PrintDocumentGenerator.cs` — class skeleton with constructor dependency on `IFontNameToFamilyConverter` logic
- [ ] 2.2 Implement Sheet border rendering (white Rectangle with gray stroke)
- [ ] 2.3 Implement Line object rendering (WPF Line with Y-flip, stroke color, thickness, dash array)
- [ ] 2.4 Implement Rectangle object rendering (WPF Rectangle with Y-flip, stroke color, fill color, thickness, dash array)
- [ ] 2.5 Implement Text object rendering (WPF TextBlock with Y-flip, font family, font size, foreground, alignment, wrapping, rotation)
- [ ] 2.6 Extract `MmToWpfUnits(double mm)` helper (`mm * 96.0 / 25.4`)
- [ ] 2.7 Extract `HexToBrush(string hex)` helper (reuse logic from `HexToBrushConverter`)
- [ ] 2.8 Extract `LineTypeToDashArray(LineType)` helper (reuse logic from `LineTypeToDashArrayConverter`)
- [ ] 2.9 Extract `FontNameToFamily(string name)` helper (reuse logic from `FontNameToFamilyConverter`)
- [ ] 2.10 Handle `FixedPage` sizing from `Sheet.WidthMicrons`/`HeightMicrons`
- [ ] 2.11 Handle multi-page: wrap in `FixedDocument` with `PageContent`

## 3. PrintPreviewWindow

- [ ] 3.1 Create `Views/PrintPreviewWindow.xaml` — Window with `DocumentViewer` filling the client area
- [ ] 3.2 Create `Views/PrintPreviewWindow.xaml.cs` — constructor accepting `FixedDocument`, sets `DocumentViewer.Document`
- [ ] 3.3 Add static helper `Show(FixedDocument document)` for convenient display

## 4. MainViewModel integration

- [ ] 4.1 Inject `IPrintDocumentGenerator` into `MainViewModel` constructor
- [ ] 4.2 Add `PreviewCommand` (`[RelayCommand]`) — gets `SelectedTab.Template`, calls generator, opens `PrintPreviewWindow`
- [ ] 4.3 Add null guard: if `SelectedTab == null`, do nothing

## 5. MainWindow.xaml — UI bindings

- [ ] 5.1 Bind existing `MenuItem Header="Предпросмотр печати"` to `PreviewCommand`
- [ ] 5.2 Add `KeyBinding` for `Ctrl+Shift+P` bound to `PreviewCommand`

## 6. DI registration

- [ ] 6.1 Register `IPrintDocumentGenerator` / `PrintDocumentGenerator` as Transient in `App.xaml.cs`

## 7. Tests — PrintDocumentGenerator

- [ ] 7.1 Create `Tests/Services/PrintDocumentGeneratorTests.cs`
- [ ] 7.2 Test: empty template returns FixedDocument with one empty page
- [ ] 7.3 Test: single Line rendered at correct WPF coordinates
- [ ] 7.4 Test: single Rectangle rendered at correct WPF coordinates
- [ ] 7.5 Test: single Text rendered at correct WPF coordinates
- [ ] 7.6 Test: Line with non-solid LineType has correct StrokeDashArray
- [ ] 7.7 Test: Rectangle with FillColor="Transparent" has null Fill
- [ ] 7.8 Test: Text with non-zero RotationAngle has LayoutTransform
- [ ] 7.9 Test: Text with TextWrapping=True has TextWrapping.Wrap
- [ ] 7.10 Test: Sheet size matches FixedPage size (A4, A0)
- [ ] 7.11 Test: Y-flip is applied correctly (model Y=0 → WPF bottom of page)
- [ ] 7.12 Test: multiple objects of different types render without error
- [ ] 7.13 Test: null Template throws ArgumentNullException
