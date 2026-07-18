using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.ViewModels;

public partial class PropertiesViewModel : ObservableObject, IDisposable
{
    private readonly ObservableCollection<TemplateObjectBase> _selectedObjects;
    private bool _isDisposed;

    [ObservableProperty]
    private TemplateObjectBase? _selectedObject;

    [ObservableProperty]
    private int _selectionCount;

    [ObservableProperty]
    private bool _isSingleSelection;

    [ObservableProperty]
    private string? _validationError;

    public LinePropertiesViewModel LineVM { get; }
    public RectanglePropertiesViewModel RectVM { get; }
    public TextPropertiesViewModel TextVM { get; }

    public bool IsLineSelected => SelectedObject is Line;
    public bool IsRectangleSelected => SelectedObject is Rectangle;
    public bool IsTextSelected => SelectedObject is Text;

    public string? ObjectId => SelectedObject?.Id;
    public string? ObjectTypeName => SelectedObject switch
    {
        Line => "Линия",
        Rectangle => "Прямоугольник",
        Text => "Текст",
        _ => null
    };

    public PropertiesViewModel(
        ObservableCollection<TemplateObjectBase> selectedObjects,
        CommandHistory? commandHistory,
        Action? markDirty)
    {
        _selectedObjects = selectedObjects ?? throw new ArgumentNullException(nameof(selectedObjects));
        LineVM = new(commandHistory, markDirty, v => ValidationError = v);
        RectVM = new(commandHistory, markDirty, v => ValidationError = v);
        TextVM = new(commandHistory, markDirty, v => ValidationError = v);
        _selectedObjects.CollectionChanged += OnSelectedObjectsChanged;
        OnSelectedObjectsChanged(null, null);
    }

    public PropertiesViewModel(ObservableCollection<TemplateObjectBase> selectedObjects)
        : this(selectedObjects, null, null)
    {
    }

    private void OnSelectedObjectsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs? e)
    {
        if (_isDisposed) return;
        UpdateSelection();
    }

    public void Refresh() => UpdateSelection();

    private void UpdateSelection()
    {
        SelectionCount = _selectedObjects.Count;
        IsSingleSelection = _selectedObjects.Count == 1;
        ValidationError = null;

        SelectedObject = IsSingleSelection ? _selectedObjects[0] : null;

        LineVM.UpdateObject(SelectedObject as Line);
        RectVM.UpdateObject(SelectedObject as Rectangle);
        TextVM.UpdateObject(SelectedObject as Text);

        OnPropertyChanged(nameof(IsLineSelected));
        OnPropertyChanged(nameof(IsRectangleSelected));
        OnPropertyChanged(nameof(IsTextSelected));
        OnPropertyChanged(nameof(ObjectId));
        OnPropertyChanged(nameof(ObjectTypeName));
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _selectedObjects.CollectionChanged -= OnSelectedObjectsChanged;
        LineVM.Dispose();
        RectVM.Dispose();
        TextVM.Dispose();
    }
}
