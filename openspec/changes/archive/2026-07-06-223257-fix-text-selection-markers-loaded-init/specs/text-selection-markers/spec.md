## ADDED Requirements

### Requirement: Crash-free initialization during DataTemplate parsing
The behavior SHALL NOT throw exceptions or crash during WPF DataTemplate materialization. The constructor MUST defer all visual tree access (parent lookup, sibling Canvas discovery) until after the TextBlock is attached to the visual tree.

#### Scenario: Text object inserted from TextTool
- **WHEN** user clicks canvas with TextTool active
- **THEN** a new Text object is added to `Template.Objects`
- **AND** `TextSelectionMarkerBehavior` attached to the TextBlock in the DataTemplate SHALL NOT throw `InvalidOperationException`
- **AND** the application SHALL NOT crash

#### Scenario: Text object pasted from clipboard
- **WHEN** user presses Ctrl+V with Text objects in clipboard
- **THEN** TextBlock DataTemplate is materialized
- **AND** `TextSelectionMarkerBehavior` constructor SHALL complete without exceptions

#### Scenario: Template loaded from file with text objects
- **WHEN** user opens a .tdel file containing Text objects
- **THEN** `TextSelectionMarkerBehavior` on each TextBlock SHALL initialize without crashing

### Requirement: Marker creation on visual tree attachment
The behavior SHALL create 4 Rectangle markers in the sibling Canvas after the TextBlock is attached to the visual tree and the `Loaded` event has fired.

#### Scenario: Markers created after Loaded
- **WHEN** `TextSelectionMarkerBehavior.IsEnabled` is set to `true` on a TextBlock
- **AND** the TextBlock's `Loaded` event fires
- **THEN** `_overlayCanvas` SHALL have 4 Rectangle children

#### Scenario: No duplicate markers on multiple Loaded
- **WHEN** `Loaded` fires more than once on the same TextBlock
- **THEN** markers SHALL NOT be created more than once (deduplication guard)

#### Scenario: Gray background highlight removed
- **WHEN** a Text object is selected
- **THEN** the TextBlock SHALL NOT have a Background setter in its DataTrigger (only Foreground + Bold)
- **AND** selection is indicated solely by the 4 corner markers

### Requirement: Reactive marker updates
Markers SHALL reposition when the text content, rotation, or font size changes.

#### Scenario: Reposition on content change
- **WHEN** `Text.Content` property changes
- **THEN** markers SHALL reposition to match the new text bounds

#### Scenario: Reposition on rotation change
- **WHEN** `Text.RotationAngle` property changes
- **THEN** markers SHALL reposition to the rotated corners

#### Scenario: Reposition on font size change
- **WHEN** `Text.FontSizeMicrons` property changes
- **THEN** markers SHALL reposition to match the new font size bounds

### Requirement: Empty content guard
The behavior SHALL hide markers when the TextBlock content is null or empty.

#### Scenario: Markers hidden on empty content
- **WHEN** `Text.Content` is `null` or `""`
- **THEN** all marker Rectangles SHALL have `Visibility = Collapsed`

### Requirement: Cleanup on disable or unload
The behavior SHALL remove markers and unsubscribe from all events when disabled or when the TextBlock is unloaded.

#### Scenario: Markers removed on disable
- **WHEN** `TextSelectionMarkerBehavior.IsEnabled` is set to `false`
- **THEN** all marker Rectangles SHALL be removed from `_overlayCanvas.Children`

#### Scenario: Events unsubscribed on disable
- **WHEN** `IsEnabled` is set to `false`
- **THEN** `Loaded`, `Unloaded`, `SizeChanged`, and `DataContextChanged` events SHALL be unsubscribed

#### Scenario: Markers removed on unload
- **WHEN** the TextBlock is removed from the visual tree (Unloaded)
- **THEN** markers SHALL be removed and events SHALL be unsubscribed
