namespace DotElectric.TemplateEditor.Constants;

public static class EditorSettings
{
    // Zoom / Pan
    public const double ZoomMin = 0.1;
    public const double ZoomMax = 10.0;
    public const double ZoomIncrement = 0.1;
    public const double MouseWheelZoomFactor = 1.1;
    public const double DefaultSheetOffsetMm = 10.0;
    public const double FitToScreenPadding = 0.95;

    // Grid
    public const long DefaultGridStepMicrons = 5000;
    public const int MaxGridNodes = 250000;
    public const double MinPixelSpacing = 0.5;

    // Nudge
    public const long NudgeStepMicrons = 1000;
    public const long BigNudgeStepMicrons = 10000;

    // Undo/Redo
    public const int CommandHistoryMaxLevels = 50;

    // Stroke
    public const long DefaultStrokeThicknessMicrons = 500;

    // Text
    public const long DefaultFontSizeMicrons = 14000;
    public const string DefaultFontName = "ГОСТ А";

    // Colors
    public const string DefaultStrokeColor = "#000000";
    public const string DefaultFillColor = "Transparent";
    public const string DefaultTextForeground = "#000000";

    // Timing
    public const int DoubleClickThresholdMs = 500;

    // Autosave
    public const int AutosaveCleanupDays = 7;
}
