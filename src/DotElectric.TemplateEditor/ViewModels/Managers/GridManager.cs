using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

public sealed partial class GridManager : ObservableObject
{
    private readonly Template _template;
    private readonly GridSettings _gridSettings;
    private readonly ZoomPanManager _zoomPanManager;

    private long[] _nodeData = [];
    private int _nodeCount;

    private readonly List<GridHelper.GridNode> _nodeBuffer = new();

    private const double ViewportMargin = 1.5;

    public GridManager(
        Template template,
        GridSettings gridSettings,
        ZoomPanManager zoomPanManager)
    {
        _template = template ?? throw new ArgumentNullException(nameof(template));
        _gridSettings = gridSettings ?? throw new ArgumentNullException(nameof(gridSettings));
        _zoomPanManager = zoomPanManager ?? throw new ArgumentNullException(nameof(zoomPanManager));
    }

    public long[] RawNodeData => _nodeData;

    public int RawNodeCount => _nodeCount;

    public Action? GridInvalidated { get; set; }

    public void ToggleGrid()
    {
        IsGridEnabled = !IsGridEnabled;
    }

    public void ToggleSnap()
    {
        _gridSettings.SnapEnabled = !_gridSettings.SnapEnabled;
    }

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
        get => _gridSettings.StepMicrons / (double)PhysicalConstants.MicronsPerMm;
        set
        {
            _gridSettings.StepMicrons = (long)(value * PhysicalConstants.MicronsPerMm);
            RefreshGridNodes();
            OnPropertyChanged();
        }
    }

    public long GridStepMicrons => _gridSettings.StepMicrons;

    public void RefreshGridNodes()
    {
        if (!_gridSettings.Enabled || !_gridSettings.Visible)
        {
            _nodeData = [];
            _nodeCount = 0;
            InvalidateGrid();
            return;
        }

        var (vpLeft, vpBottom, vpWidth, vpHeight) = _zoomPanManager.GetViewportMicrons(ViewportMargin);

        // If viewport is zero (tab not visible yet), fall back to full-sheet
        if (vpWidth <= 0 || vpHeight <= 0)
        {
            vpLeft = 0;
            vpBottom = 0;
            vpWidth = _template.Sheet.WidthMicrons;
            vpHeight = _template.Sheet.HeightMicrons;
        }

        var sheetWidth = _template.Sheet.WidthMicrons;
        var sheetHeight = _template.Sheet.HeightMicrons;

        var displayStep = GridHelper.ComputeDisplayStep(
            _zoomPanManager.Zoom,
            EditorSettings.MaxGridNodes,
            sheetWidth,
            sheetHeight,
            vpWidth,
            vpHeight,
            _gridSettings.StepMicrons);

        var nodes = GridHelper.GenerateGridNodes(
            _template.Sheet,
            displayStep,
            _zoomPanManager.Zoom,
            vpLeft,
            vpBottom,
            vpWidth,
            vpHeight,
            _nodeBuffer);

        // Allocate new array each refresh — no shared mutable state with GridNodesLayer
        _nodeCount = nodes.Count;
        var data = new long[_nodeCount * 2];
        for (int i = 0; i < _nodeCount; i++)
        {
            data[i * 2] = nodes[i].XMicrons;
            data[i * 2 + 1] = nodes[i].YMicrons;
        }
        _nodeData = data;

        InvalidateGrid();
    }

    private void InvalidateGrid()
    {
        GridInvalidated?.Invoke();
    }
}
