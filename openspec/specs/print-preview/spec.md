# Print Preview

## Purpose

The Print Preview capability allows users to preview the current template (sheet and objects) before printing, using a WPF DocumentViewer with zoom controls and a built-in Windows PrintDialog.

## Requirements

### Requirement: User can open print preview

The system SHALL provide a "Предпросмотр печати" command accessible from the main menu and via keyboard shortcut `Ctrl+Shift+P`.

#### Scenario: Open preview from menu
- **WHEN** user clicks menu `Файл > Предпросмотр печати`
- **THEN** a preview window opens showing the current template

#### Scenario: Open preview from keyboard
- **WHEN** user presses `Ctrl+Shift+P`
- **THEN** a preview window opens showing the current template

#### Scenario: No active tab
- **WHEN** user invokes preview command with no open tabs
- **THEN** the preview window does NOT open

### Requirement: Preview shows only sheet and objects

The preview window SHALL display only the sheet border and the drawn objects (Line, Rectangle, Text). UI chrome such as grid nodes, selection markers, preview shapes, selection box, inline editor, and scrollbars SHALL NOT appear in the preview.

#### Scenario: Grid nodes hidden
- **WHEN** grid is enabled in the editor
- **THEN** grid dots are NOT visible in the preview

#### Scenario: Selection markers hidden
- **WHEN** objects are selected in the editor
- **THEN** selection markers are NOT visible in the preview

#### Scenario: Preview shapes hidden
- **WHEN** a drawing tool is active with a preview shape
- **THEN** the preview shape is NOT visible in the preview

#### Scenario: Inline editor hidden
- **WHEN** a text object is being edited inline
- **THEN** the inline TextBox is NOT visible in the preview

### Requirement: All object types rendered correctly

The preview SHALL render Line, Rectangle, and Text objects with the correct visual properties.

#### Scenario: Line rendered
- **WHEN** the template contains a Line object
- **THEN** the preview shows the line at the correct position with correct stroke color, stroke thickness, stroke dash array

#### Scenario: Rectangle rendered
- **WHEN** the template contains a Rectangle object
- **THEN** the preview shows the rectangle at the correct position with correct stroke color, fill color, stroke thickness, stroke dash array

#### Scenario: Text rendered
- **WHEN** the template contains a Text object
- **THEN** the preview shows the text content at the correct position with correct font, font size, foreground color, text alignment, text wrapping, and rotation angle

#### Scenario: GOST fonts used
- **WHEN** a Text object uses font "ГОСТ А" or "ГОСТ Б"
- **THEN** the preview renders using the corresponding GOST font family

#### Scenario: Rotated text
- **WHEN** a Text object has a non-zero RotationAngle
- **THEN** the preview shows the text rotated by that angle

### Requirement: Preview uses DocumentViewer

The preview window SHALL use WPF `DocumentViewer` control to display a `FixedDocument`.

#### Scenario: DocumentViewer shows FixedDocument
- **WHEN** preview window opens
- **THEN** the `DocumentViewer.Document` is set to a valid `FixedDocument`

#### Scenario: Zoom controls visible
- **WHEN** preview window is open
- **THEN** the `DocumentViewer` toolbar shows zoom controls (slider or buttons)

### Requirement: Print from preview

The preview window SHALL allow the user to print from the preview. The `DocumentViewer`'s built-in Print button SHALL open the standard Windows `PrintDialog`.

#### Scenario: Print button opens PrintDialog
- **WHEN** user clicks the Print button in the preview toolbar
- **THEN** the standard Windows PrintDialog opens

#### Scenario: User cancels print
- **WHEN** user clicks the Print button and then cancels the PrintDialog
- **THEN** the preview window remains open

#### Scenario: User confirms print
- **WHEN** user clicks the Print button and confirms the PrintDialog
- **THEN** the document is sent to the printer and the preview window remains open

### Requirement: Sheet rendered at correct size

The preview SHALL render the sheet at its actual dimensions. The `FixedPage` size SHALL match the sheet format in WPF units (1/96 inch) with correct aspect ratio.

#### Scenario: A4 sheet
- **WHEN** the template is A4 (210 × 297 mm)
- **THEN** the FixedPage width and height are set to the corresponding WPF units

#### Scenario: A0 sheet
- **WHEN** the template is A0 (841 × 1189 mm)
- **THEN** the FixedPage width and height are set to the corresponding WPF units

### Requirement: Preview window can be closed

The preview window SHALL have a Close button (title bar X) that returns the user to the editor.

#### Scenario: Close preview
- **WHEN** user clicks the Close button on the preview window
- **THEN** the preview window closes and the editor is shown unchanged
