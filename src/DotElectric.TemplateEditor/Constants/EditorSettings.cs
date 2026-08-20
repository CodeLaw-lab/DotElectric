namespace DotElectric.TemplateEditor.Constants;

public static class EditorSettings
{
    // Zoom / Pan
    public const double ZoomMin = 0.1;
    public const double ZoomMax = 10.0;
    public const double ZoomIncrement = 0.1;
    public const double MouseWheelZoomFactor = 1.1;
    public const double FitToScreenPadding = 0.95;

    // Grid
    public const long DefaultGridStepMicrons = 5000;
    public const int MaxGridNodes = 250000;
    public const double MinPixelSpacing = 5.0;
    public const double DefaultGridNodeSize = 2.0;

    // Nudge
    public const long NudgeStepMicrons = 100;
    public const long BigNudgeStepMicrons = 10000;

    // Undo/Redo
    public const int CommandHistoryMaxLevels = 50;

    // Autosave
    public const int AutosaveCleanupDays = 7;

    // Interaction
    public const long HandleHitToleranceMicrons = 8000;
    public const long SelectionBoxThresholdMicrons = 3000;
    public const long MinResizeSizeMicrons = 1000;
    public const long MinFontSizeMicrons = 1000;
    public const long MinDimensionMicrons = 400;
    public const long MaxCustomSheetSizeMm = 2000;
}
