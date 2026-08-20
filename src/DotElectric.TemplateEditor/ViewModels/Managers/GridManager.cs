using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

public sealed partial class GridManager : ObservableObject, IDisposable
{
    private readonly Template _template;
    private readonly GridSettings _gridSettings;
    private readonly ZoomPanManager _zoomPanManager;
    private readonly IGridNodeGenerator _gridNodeGenerator;

    private IReadOnlyList<GridNode> _nodes = [];

    public GridManager(
        Template template,
        GridSettings gridSettings,
        ZoomPanManager zoomPanManager,
        IGridNodeGenerator gridNodeGenerator)
    {
        _template = template ?? throw new ArgumentNullException(nameof(template));
        _gridSettings = gridSettings ?? throw new ArgumentNullException(nameof(gridSettings));
        _zoomPanManager = zoomPanManager ?? throw new ArgumentNullException(nameof(zoomPanManager));
        _gridNodeGenerator = gridNodeGenerator ?? throw new ArgumentNullException(nameof(gridNodeGenerator));

        _template.PropertyChanged += OnTemplatePropertyChanged;
    }

    /// <summary>
    /// Узлы сетки в абсолютных координатах листа (микроны).
    /// Регенерируются при zoom/смене шага/смене формата. НЕ регенерируются при пане —
    /// RenderTransform в GridNodesLayer двигает их бесплатно.
    /// </summary>
    public IReadOnlyList<GridNode> Nodes => _nodes;

    public Action? GridInvalidated { get; set; }

    public void ToggleGrid() => IsGridEnabled = !IsGridEnabled;
    public void ToggleSnap() => IsSnapEnabled = !IsSnapEnabled;

    public bool IsGridEnabled
    {
        get => _gridSettings.Enabled && _gridSettings.Visible;
        set
        {
            _gridSettings.Enabled = value;
            _gridSettings.Visible = value;
            RefreshGridNodes();
            OnPropertyChanged();
        }
    }

    public bool IsSnapEnabled
    {
        get => _gridSettings.SnapEnabled;
        set
        {
            _gridSettings.SnapEnabled = value;
            OnPropertyChanged();
        }
    }

    public double GridStepMm
    {
        get => _gridSettings.StepMicrons / (double)Coordinate.MicronsPerMm;
        set
        {
            _gridSettings.StepMicrons = (long)(value * Coordinate.MicronsPerMm);
            RefreshGridNodes();
            OnPropertyChanged();
        }
    }

    public long GridStepMicrons => _gridSettings.StepMicrons;

    public void RefreshGridNodes()
    {
        if (!_gridSettings.Enabled || !_gridSettings.Visible)
        {
            _nodes = [];
            InvalidateGrid();
            return;
        }

        var displayStep = _gridNodeGenerator.ComputeDisplayStep(
            _zoomPanManager.Zoom,
            _gridSettings.MaxGridNodes,
            _template.Sheet.WidthMicrons,
            _template.Sheet.HeightMicrons,
            _gridSettings.StepMicrons);

        _nodes = _gridNodeGenerator.GenerateGridNodes(
            displayStep,
            _zoomPanManager.Zoom,
            _template.Sheet.WidthMicrons,
            _template.Sheet.HeightMicrons,
            _gridSettings.MaxGridNodes);

        InvalidateGrid();
    }

    private void OnTemplatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Template.Sheet))
            RefreshGridNodes();
    }

    private void InvalidateGrid() => GridInvalidated?.Invoke();

    public void Dispose()
    {
        _template.PropertyChanged -= OnTemplatePropertyChanged;
    }
}
