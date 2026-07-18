using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

/// <summary>
/// Управляет выделением объектов на холсте.
/// </summary>
public sealed partial class SelectionManager : ObservableObject, IDisposable
{
    private readonly Action _onSelectionChanged;
    private readonly NotifyCollectionChangedEventHandler _onCollectionChanged;

    public SelectionManager(Action onSelectionChanged)
    {
        _onSelectionChanged = onSelectionChanged;
        _onCollectionChanged = (s, e) =>
        {
            _onSelectionChanged();
            OnPropertyChanged(nameof(ShowSelectionMarkers));
        };
        SelectedObjects.CollectionChanged += _onCollectionChanged;
    }

    public void Dispose()
    {
        SelectedObjects.CollectionChanged -= _onCollectionChanged;
    }

    public ObservableCollection<TemplateObjectBase> SelectedObjects { get; } = new();

    public bool ShowSelectionMarkers => SelectedObjects.Count > 0;

    public TemplateObjectBase? SingleSelectedObject => SelectedObjects.Count == 1 ? SelectedObjects[0] : null;

    public bool IsObjectSelected(TemplateObjectBase obj) => SelectedObjects.Contains(obj);

    public void SelectSingle(TemplateObjectBase obj)
    {
        SelectedObjects.Clear();
        SelectedObjects.Add(obj);
    }

    public void AddToSelection(TemplateObjectBase obj)
    {
        if (!SelectedObjects.Contains(obj))
            SelectedObjects.Add(obj);
    }

    public void RemoveFromSelection(TemplateObjectBase obj)
    {
        SelectedObjects.Remove(obj);
    }

    public void ClearSelection()
    {
        SelectedObjects.Clear();
    }

    public void PurgeOrphaned(IEnumerable<TemplateObjectBase> allObjects)
    {
        var orphaned = SelectedObjects.Where(obj => !allObjects.Contains(obj)).ToList();
        foreach (var obj in orphaned)
            SelectedObjects.Remove(obj);
    }

    public void SelectAll(IEnumerable<TemplateObjectBase> allObjects)
    {
        SelectedObjects.Clear();
        foreach (var obj in allObjects)
            SelectedObjects.Add(obj);
    }


}
