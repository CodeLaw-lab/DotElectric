using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Messages;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.Abstractions;
using DotElectric.TemplateEditor.ViewModels.Managers;

namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// ViewModel редактора одного шаблона.
/// Каждая вкладка — отдельный экземпляр EditorViewModel.
/// </summary>
public partial class EditorViewModel : ObservableObject, IDisposable, IAutosaveTab, IEditorContext
{
    private readonly ITemplateService _templateService;
    private readonly IPrintService _printService;
    private readonly CommandHistory _commandHistory;
    private bool _isDisposed;

    // === Менеджеры (делегирование ответственности) ===
    private readonly ZoomPanManager _zoomPanManager;
    private readonly SelectionManager _selectionManager;
    private readonly ClipboardManager _clipboardManager;
    private readonly ToolManager _toolManager;
    private readonly PreviewManager _previewManager;
    private readonly InlineEditManager _inlineEditManager;
    private readonly StatusBarManager _statusBarManager;
    private readonly GridManager _gridManager;
    private readonly DirtyStateManager _dirtyStateManager;

    // All PropertyChanged handlers are now managed by individual managers.
    // No forwarding from Manager → EditorViewModel is needed since XAML binds directly.

    // === Команды закрытия вкладок (для контекстного меню TabItem) ===
    // Используют WeakReferenceMessenger для отправки запросов в MainViewModel.

    /// <summary>
    /// Закрыть эту вкладку (из контекстного меню).
    /// </summary>
    [RelayCommand]
    private void CloseTab()
    {
        WeakReferenceMessenger.Default.Send(new CloseTabRequestMessage(this));
    }

    /// <summary>
    /// Закрыть другие вкладки (из контекстного меню).
    /// </summary>
    [RelayCommand]
    private void CloseOtherTabs()
    {
        WeakReferenceMessenger.Default.Send(new CloseOtherTabsRequestMessage(this));
    }

    /// <summary>
    /// Закрыть все вкладки (из контекстного меню).
    /// </summary>
    [RelayCommand]
    private void CloseAllTabs()
    {
        WeakReferenceMessenger.Default.Send(new CloseAllTabsRequestMessage());
    }

    // === Состояние ===

    /// <summary>
    /// Уникальный ID вкладки (для автосохранения).
    /// </summary>
    public string TabId { get; }

    /// <summary>
    /// Шаблон (модель данных).
    /// </summary>
    public Template Template { get; }

    object IAutosaveTab.Template => Template;
    bool IAutosaveTab.IsDirty => _dirtyStateManager.IsDirty;
    string? IAutosaveTab.FilePath
    {
        get => _dirtyStateManager.FilePath;
        set => _dirtyStateManager.FilePath = value;
    }
    string IAutosaveTab.DisplayName => _dirtyStateManager.DisplayName;

    /// <summary>
    /// История команд для Undo/Redo.
    /// </summary>
    public CommandHistory CommandHistory => _commandHistory;

    /// <summary>
    /// ViewModel панели свойств (привязан к SelectedObjects).
    /// </summary>
    public PropertiesViewModel PropertiesVm { get; }

    // === Прямой доступ к менеджерам (для XAML-биндингов R3) ===

    public ZoomPanManager ZoomPanManager => _zoomPanManager;
    public ToolManager ToolManager => _toolManager;
    public SelectionManager SelectionManager => _selectionManager;
    public PreviewManager PreviewManager => _previewManager;
    public InlineEditManager InlineEditManager => _inlineEditManager;
    public StatusBarManager StatusBarManager => _statusBarManager;
    public GridManager GridManager => _gridManager;
    public DirtyStateManager DirtyStateManager => _dirtyStateManager;
    public ClipboardManager ClipboardManager => _clipboardManager;

    // === IEditorContext properties (делегировано менеджерам) ===

    public ObservableCollection<TemplateObjectBase> SelectedObjects => _selectionManager.SelectedObjects;
    public TemplateObjectBase? SingleSelectedObject => _selectionManager.SingleSelectedObject;

    /// <summary>
    /// Настройки сетки (НЕ сериализуются — состояние редактора).
    /// </summary>
    public GridSettings GridSettings { get; }

    // === Свойства состояния ===

    // === IEditorContext properties (делегировано менеджерам) ===

    public double Zoom => _zoomPanManager.Zoom;
    public string StatusMessage
    {
        get => _statusBarManager.StatusMessage;
        set => _statusBarManager.StatusMessage = value;
    }
    public void PanCanvas(double deltaXMm, double deltaYMm) => _zoomPanManager.PanCanvas(deltaXMm, deltaYMm);
    public void PushTool(string tool) => _toolManager.PushTool(tool);
    public void PopTool() => _toolManager.PopTool();

    /// <summary>
    /// Объект, на который наведён курсор (для hover-эффектов).
    /// </summary>
    [ObservableProperty]
    private TemplateObjectBase? _hoveredObject;

    /// <summary>
    /// Маркер изменения размера, на который наведён курсор (null если не на маркере).
    /// </summary>
    [ObservableProperty]
    private ResizeHandle? _hoveredHandle;

    /// <summary>
    /// Активный маркер изменения размера (null если не ресайзим).
    /// </summary>

    // === IEditorContext Preview/SelectionBox (делегировано PreviewManager, без OnPropertyChanged) ===

    public Line? PreviewLine
    {
        get => _previewManager.PreviewLine;
        set => _previewManager.PreviewLine = value;
    }
    public Models.Objects.Rectangle? PreviewRectangle
    {
        get => _previewManager.PreviewRectangle;
        set => _previewManager.PreviewRectangle = value;
    }
    public Text? PreviewText
    {
        get => _previewManager.PreviewText;
        set => _previewManager.PreviewText = value;
    }
    public long SelectionBoxLeft
    {
        get => _previewManager.SelectionBoxLeft;
        set => _previewManager.SelectionBoxLeft = value;
    }
    public long SelectionBoxBottom
    {
        get => _previewManager.SelectionBoxBottom;
        set => _previewManager.SelectionBoxBottom = value;
    }
    public long SelectionBoxTop => _previewManager.SelectionBoxTop;
    public long SelectionBoxWidth
    {
        get => _previewManager.SelectionBoxWidth;
        set => _previewManager.SelectionBoxWidth = value;
    }
    public long SelectionBoxRight => _previewManager.SelectionBoxRight;
    public long SelectionBoxHeight
    {
        get => _previewManager.SelectionBoxHeight;
        set => _previewManager.SelectionBoxHeight = value;
    }
    public SelectionDirection SelectionDirection
    {
        get => _previewManager.SelectionBoxDirection;
        set => _previewManager.SelectionBoxDirection = value;
    }

    /// <summary>
    /// Активный маркер изменения размера (null если не ресайзим).
    /// </summary>
    [ObservableProperty]
    private ResizeHandle? _activeResizeHandle;

    // === Inline-редактирование (делегировано InlineEditManager) ===

    public void StartInlineEditing(Text textObj) => _inlineEditManager.Start(textObj);

    /// <summary>
    /// Завершить inline-редактирование (делегировано InlineEditManager).
    /// </summary>
    [RelayCommand]
    private void CommitInlineEditing() => _inlineEditManager.Commit();

    /// <summary>
    /// Отменить inline-редактирование (делегировано InlineEditManager).
    /// </summary>
    [RelayCommand]
    private void CancelInlineEditing() => _inlineEditManager.Cancel();

    // === Кэшированные инструменты (делегировано ToolManager) ===

    /// <summary>
    /// Получить или создать кэшированный инструмент. Делегировано ToolManager.
    /// </summary>
    public T GetOrCreateTool<T>() where T : class, ITool => _toolManager.GetOrCreateTool<T>();

    // === Команда смены инструмента (делегировано ToolManager) ===

    /// <summary>
    /// Команда установки активного инструмента. Делегировано ToolManager.
    /// </summary>
    [RelayCommand]
    private void SetActiveTool(string tool)
    {
        var prevTool = _toolManager.ActiveTool;
        if (prevTool != tool)
        {
            _toolManager.ResetTool(prevTool);
            _previewManager.ClearAll();
            _toolManager.ActiveTool = tool;
        }
    }

    // === Команды клавиатуры ===

    /// <summary>
    /// Удалить выделенные объекты.
    /// </summary>
    [RelayCommand]
    private void DeleteSelectedObjects()
    {
        DeleteSelected();
    }

    /// <summary>
    /// Копировать выделенные объекты в буфер. Делегировано ClipboardManager.
    /// </summary>
    [RelayCommand]
    private void CopySelected()
    {
        var count = _selectionManager.SelectedObjects.Count;
        _clipboardManager.Copy(_selectionManager.SelectedObjects);
        _statusBarManager.StatusMessage = count > 0
            ? $"Скопировано: {count} {GetObjectWord(count)}"
            : "Нет выделенных объектов";
    }

    /// <summary>
    /// Вырезать выделенные объекты (копировать + удалить). Делегировано ClipboardManager.
    /// </summary>
    [RelayCommand]
    private void CutSelected()
    {
        var count = _selectionManager.SelectedObjects.Count;
        if (count == 0)
        {
            _statusBarManager.StatusMessage = "Нет выделенных объектов";
            return;
        }
        _clipboardManager.Cut(_selectionManager.SelectedObjects, _ => DeleteSelected());
        _statusBarManager.StatusMessage = $"Вырезано: {count} {GetObjectWord(count)}";
    }

    /// <summary>
    /// Вставить объекты из буфера. Делегировано ClipboardManager.
    /// </summary>
    [RelayCommand]
    private void PasteFromClipboard()
    {
        var clipboard = _clipboardManager.GetClipboardContents();
        if (clipboard.Count == 0)
        {
            _statusBarManager.StatusMessage = "Буфер обмена пуст";
            return;
        }
        if (clipboard.Count == 1)
        {
            CommandHistory.Push(new AddObjectCommand(Template.Objects, clipboard[0], nameOverride: "Вставить объект"));
        }
        else
        {
            var commands = clipboard
                .Select(obj => (IUndoCommand)new AddObjectCommand(Template.Objects, obj, nameOverride: "Вставить объект"))
                .ToList();
            CommandHistory.Push(new Commands.BatchCommand(commands, "Вставить объекты"));
        }

        _selectionManager.SelectAll(clipboard);
        _statusBarManager.StatusMessage = $"Вставлено: {clipboard.Count} {GetObjectWord(clipboard.Count)}";
    }

    /// <summary>
    /// Выделить все объекты. Делегировано SelectionManager.
    /// </summary>
    [RelayCommand]
    private void SelectAllObjects() => _selectionManager.SelectAll(Template.Objects);

    /// <summary>
    /// Шаг перемещения стрелками: шаг сетки (snap on) или 0.1 мм (snap off).
    /// </summary>
    private long NudgeStep => GridSettings.SnapEnabled
        ? GridSettings.StepMicrons
        : 100; // 0.1 мм

    /// <summary>
    /// Большой шаг (Shift+стрелка): 10 мм (snap on) или шаг сетки (snap off).
    /// </summary>
    private long BigNudgeStep => GridSettings.SnapEnabled
        ? EditorSettings.BigNudgeStepMicrons
        : GridSettings.StepMicrons;

    /// <summary>
    /// Переместить выделенные объекты на заданную дельту.
    /// </summary>
    [RelayCommand]
    private void NudgeUp() => MoveSelected(0, NudgeStep);

    [RelayCommand]
    private void NudgeDown() => MoveSelected(0, -NudgeStep);

    [RelayCommand]
    private void NudgeLeft() => MoveSelected(-NudgeStep, 0);

    [RelayCommand]
    private void NudgeRight() => MoveSelected(NudgeStep, 0);

    /// <summary>
    /// Переместить выделенные объекты на большой шаг (Shift+стрелка).
    /// </summary>
    [RelayCommand]
    private void BigNudgeUp() => MoveSelected(0, BigNudgeStep);

    [RelayCommand]
    private void BigNudgeDown() => MoveSelected(0, -BigNudgeStep);

    [RelayCommand]
    private void BigNudgeLeft() => MoveSelected(-BigNudgeStep, 0);

    [RelayCommand]
    private void BigNudgeRight() => MoveSelected(BigNudgeStep, 0);

    private static int NormalizeAngle(int angle) => ((angle % 360) + 360) % 360;

    /// <summary>
    /// Вращать выделенные текстовые объекты на +90°.
    /// </summary>
    [RelayCommand]
    private void RotateSelectedClockwise()
    {
        var texts = _selectionManager.SelectedObjects.OfType<Text>().ToList();
        if (texts.Count == 0) return;

        var commands = texts
            .Select(t => (IUndoCommand)new ChangePropertyCommand<int>(
                () => t.RotationAngle,
                v => t.RotationAngle = NormalizeAngle(v),
                NormalizeAngle(t.RotationAngle + 90),
                "Повернуть текст",
                MarkDirty))
            .ToList();

        if (commands.Count == 1)
            CommandHistory.Push(commands[0]);
        else
            CommandHistory.Push(new BatchCommand(commands, "Повернуть текст"));
    }

    /// <summary>
    /// Вращать выделенные текстовые объекты на -90°.
    /// </summary>
    [RelayCommand]
    private void RotateSelectedCounterClockwise()
    {
        var texts = _selectionManager.SelectedObjects.OfType<Text>().ToList();
        if (texts.Count == 0) return;

        var commands = texts
            .Select(t => (IUndoCommand)new ChangePropertyCommand<int>(
                () => t.RotationAngle,
                v => t.RotationAngle = NormalizeAngle(v),
                NormalizeAngle(t.RotationAngle - 90),
                "Повернуть текст",
                MarkDirty))
            .ToList();

        if (commands.Count == 1)
            CommandHistory.Push(commands[0]);
        else
            CommandHistory.Push(new BatchCommand(commands, "Повернуть текст"));
    }

    // === Undo/Redo команды для XAML binding ===

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        var lastCmd = _commandHistory.PeekUndo();
        _commandHistory.Undo();
        PurgeOrphanedSelection();

        if (lastCmd is Commands.DeleteObjectCommand deleteCmd &&
            _selectionManager != null &&
            Template.Objects.Contains(deleteCmd.Object))
        {
            _selectionManager.SelectSingle(deleteCmd.Object);
        }
        else if (lastCmd is Commands.BatchCommand batchCmd &&
                 _selectionManager != null)
        {
            // Попробовать выбрать объекты, восстановленные из DeleteObjectCommand внутри BatchCommand
            var restored = batchCmd.GetRestoredObjects();
            if (restored.Count > 0)
            {
                _selectionManager.ClearSelection();
                foreach (var obj in restored)
                    _selectionManager.AddToSelection(obj);
            }
        }

        MarkDirty();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(UndoDisplayName));
        OnPropertyChanged(nameof(RedoDisplayName));
    }

    private bool CanUndo() => _commandHistory.CanUndo;

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        _commandHistory.Redo();
        PurgeOrphanedSelection();
        MarkDirty();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(UndoDisplayName));
        OnPropertyChanged(nameof(RedoDisplayName));
    }

    private bool CanRedo() => _commandHistory.CanRedo;

    private void PurgeOrphanedSelection()
    {
        _selectionManager.PurgeOrphaned(Template.Objects);
    }

    /// <summary>
    /// Отображаемое имя для меню Undo.
    /// </summary>
    public string UndoDisplayName =>
        _commandHistory.CanUndo
            ? $"↶ {_commandHistory.LastUndoName}"
            : "↶ Отменить";

    /// <summary>
    /// Отображаемое имя для меню Redo.
    /// </summary>
    public string RedoDisplayName =>
        _commandHistory.CanRedo
            ? $"↷ {_commandHistory.LastRedoName}"
            : "↷ Повторить";

    // === Конструкторы ===

    /// <summary>
    /// Создать EditorViewModel для нового шаблона.
    /// </summary>
    public EditorViewModel(
        Template template,
        ITemplateService templateService,
        GridSettings? gridSettings = null,
        IPrintService? printService = null)
    {
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _printService = printService ?? throw new ArgumentNullException(nameof(printService));
        Template = template ?? throw new ArgumentNullException(nameof(template));
        _commandHistory = new CommandHistory(maxLevels: EditorSettings.CommandHistoryMaxLevels, markDirty: MarkDirty);
        TabId = Guid.NewGuid().ToString("N")[..12];
        GridSettings = gridSettings ?? GridSettings.FromDefaultGrid();

        // Инициализация менеджеров
        _selectionManager = new SelectionManager(OnSelectionChangedInternal);
        _zoomPanManager = new ZoomPanManager(Template, () => { }, () => { });
        _gridManager = new GridManager(Template, GridSettings, _zoomPanManager);
        _zoomPanManager.SetGridRefreshCallback(_gridManager.RefreshGridNodes);
        _clipboardManager = new ClipboardManager();
        _toolManager = new ToolManager(this);
        _previewManager = new PreviewManager();
        _statusBarManager = new StatusBarManager(
            Template,
            () => _gridManager.IsGridEnabled, v => _gridManager.IsGridEnabled = v,
            () => _gridManager.GridStepMm, v => _gridManager.GridStepMm = v,
            () => _gridManager.IsSnapEnabled, v => _gridManager.IsSnapEnabled = v,
            _gridManager.RefreshGridNodes);
        _inlineEditManager = new InlineEditManager(_commandHistory, MarkDirty, msg => _statusBarManager.StatusMessage = msg);
        _dirtyStateManager = new DirtyStateManager(Template);

        // Свойства IAutosaveTab делегированы в DirtyStateManager — PropertyChanged не нужен,
        // т.к. XAML биндится напрямую к DirtyStateManager.*, а AutosaveService читает значения синхронно.

        PropertiesVm = new PropertiesViewModel(_selectionManager.SelectedObjects, _commandHistory, MarkDirty);
        var orientLabel = template.Sheet.Orientation == SheetOrientation.Portrait ? "кн." : "алб.";
        _dirtyStateManager.DisplayName = $"{template.Sheet.Format} ({orientLabel}) — Без имени";

        _gridManager.RefreshGridNodes();
    }

    public int SelectionVersion { get; private set; }

    public bool IsObjectSelected(TemplateObjectBase obj) => _selectionManager.IsObjectSelected(obj);

    private void OnSelectionChangedInternal()
    {
        SelectionVersion++;
        OnPropertyChanged(nameof(SelectionVersion));
        PropertiesVm.Refresh();

        if (_selectionManager.SingleSelectedObject is Text text)
        {
            var fontSizeMm = Coordinate.ToMm(text.FontSizeMicrons);
            _statusBarManager.StatusMessage = $"Текст: {text.FontName}, {fontSizeMm:F1}мм";
        }
        else if (_selectionManager.SingleSelectedObject is Line)
        {
            _statusBarManager.StatusMessage = "Линия";
        }
        else if (_selectionManager.SingleSelectedObject is Rectangle)
        {
            _statusBarManager.StatusMessage = "Прямоугольник";
        }
        else if (_selectionManager.SelectedObjects.Count == 0)
        {
            _statusBarManager.StatusMessage = "Готово";
        }
    }

    /// <summary>
    /// Создать EditorViewModel из загруженного файла.
    /// </summary>
    public EditorViewModel(
        Template template,
        string filePath,
        ITemplateService templateService,
        GridSettings? gridSettings = null,
        IPrintService? printService = null)
        : this(template, templateService, gridSettings, printService)
    {
        _dirtyStateManager.FilePath = filePath;
        _dirtyStateManager.DisplayName = System.IO.Path.GetFileName(filePath);
        _dirtyStateManager.IsDirty = false; // Загруженный файл — не «грязный»
    }

    // === Методы ===

    /// <summary>
    /// Пометить шаблон как изменённый и обновить состояние Undo/Redo.
    /// </summary>
    public void MarkDirty()
    {
        _dirtyStateManager.MarkDirty();

        // Undo/Redo могли измениться
        OnPropertyChanged(nameof(UndoDisplayName));
        OnPropertyChanged(nameof(RedoDisplayName));
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Сбросить флаг «грязности» (после сохранения).
    /// </summary>
    public void ClearDirty()
    {
        _dirtyStateManager.ClearDirty();
    }

    /// <summary>
    /// Обновить зум (с ограничениями 10% — 1000%). Делегировано ZoomPanManager.
    /// </summary>
    public void SetZoom(double newZoom) => _zoomPanManager.SetZoom(newZoom);

    /// <summary>
    /// Обновить зум в процентах. Делегировано ZoomPanManager.
    /// </summary>
    public void SetZoomPercent(int percent) => _zoomPanManager.SetZoomPercent(percent);

    /// <summary>
    /// Центрировать лист в viewport. Делегировано ZoomPanManager.
    /// </summary>
    public void CenterCanvas() => _zoomPanManager.CenterCanvas();

    /// <summary>
    /// Команда «Вписать в экран» (Fit to Screen).
    /// Принимает параметр в формате "ширина,высота" в пикселях (размер viewport).
    /// </summary>
    [RelayCommand]
    private void FitToScreen(string? parameter)
    {
        double viewportWidth = 800;
        double viewportHeight = 600;

        if (!string.IsNullOrEmpty(parameter))
        {
            var parts = parameter.Split(',');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out var w) &&
                double.TryParse(parts[1], out var h))
            {
                viewportWidth = w;
                viewportHeight = h;
            }
        }

        _zoomPanManager.FitToScreen(viewportWidth, viewportHeight);
        _statusBarManager.StatusMessage = "Вписано в экран";
    }

    /// <summary>
    /// Увеличить масштаб на 10%. Делегировано ZoomPanManager.
    /// </summary>
    [RelayCommand]
    private void ZoomIn() => _zoomPanManager.ZoomIn();

    /// <summary>
    /// Уменьшить масштаб на 10%. Делегировано ZoomPanManager.
    /// </summary>
    [RelayCommand]
    private void ZoomOut() => _zoomPanManager.ZoomOut();

    /// <summary>
    /// Переключить отображение сетки. Делегировано GridManager.
    /// </summary>
    [RelayCommand]
    private void ToggleGrid()
    {
        _gridManager.ToggleGrid();
    }

    /// <summary>
    /// Переключить привязку к сетке. Делегировано GridManager.
    /// </summary>
    [RelayCommand]
    private void ToggleSnap()
    {
        _gridManager.ToggleSnap();
    }

    /// <summary>
    /// Провайдер Visual для печати (устанавливается View при загрузке).
    /// </summary>
    public IPrintVisualProvider? PrintVisualProvider { get; set; }

    /// <summary>
    /// Команда печати.
    /// </summary>
    [RelayCommand]
    private void Print()
    {
        var visual = PrintVisualProvider?.GetPrintVisual();
        if (visual == null)
        {
            _statusBarManager.StatusMessage = "Печать доступна через меню печати";
            return;
        }

        var settings = new PrintSettings { Scaling = "FitToPage" };
        var result = _printService.PrintWithVisual(visual, "DotElectric Template", settings);
        _statusBarManager.StatusMessage = result ? "Печать выполнена" : "Печать отменена";
    }

    /// <summary>
    /// Проверить, является ли объект hovered (на него наведён курсор).
    /// Используется в XAML через binding для hover-эффектов.
    /// </summary>
    public bool IsObjectHovered(TemplateObjectBase obj) => obj == HoveredObject;

    /// <summary>
    /// Выделить один объект (снимает предыдущее выделение). Делегировано SelectionManager.
    /// </summary>
    public void SelectSingle(TemplateObjectBase obj) => _selectionManager.SelectSingle(obj);

    /// <summary>
    /// Добавить объект к выделению. Делегировано SelectionManager.
    /// </summary>
    public void AddToSelection(TemplateObjectBase obj) => _selectionManager.AddToSelection(obj);

    /// <summary>
    /// Снять объект с выделения. Делегировано SelectionManager.
    /// </summary>
    public void RemoveFromSelection(TemplateObjectBase obj) => _selectionManager.RemoveFromSelection(obj);

    /// <summary>
    /// Очистить выделение. Делегировано SelectionManager.
    /// </summary>
    public void ClearSelection() => _selectionManager.ClearSelection();

    /// <summary>
    /// Выделить все объекты. Делегировано SelectionManager.
    /// </summary>
    public void SelectAll() => _selectionManager.SelectAll(Template.Objects);

    /// <summary>
    /// Удалить выделенные объекты.
    /// </summary>
    public void DeleteSelected()
    {
        var toDelete = _selectionManager.SelectedObjects.ToList();
        if (toDelete.Count == 0) return;
        if (toDelete.Count == 1)
        {
            _commandHistory.Push(new DeleteObjectCommand(Template.Objects, toDelete[0]));
        }
        else
        {
            var commands = toDelete
                .Select(obj => (IUndoCommand)new DeleteObjectCommand(Template.Objects, obj))
                .ToList();
            _commandHistory.Push(new BatchCommand(commands, "Удалить объекты"));
        }
        ClearSelection();
    }

    /// <summary>
    /// Переместить выделенные объекты на дельту.
    /// </summary>
    public void MoveSelected(long deltaX, long deltaY)
    {
        var objs = _selectionManager.SelectedObjects.ToList();
        if (objs.Count == 0) return;

        var commands = objs.Select(obj =>
        {
            var newX = ClampX(obj.MicronsX + deltaX);
            var newY = ClampY(obj.MicronsY + deltaY);
            return (IUndoCommand)new ChangePropertyCommand<(long X, long Y)>(
                () => (obj.MicronsX, obj.MicronsY),
                v => obj.Move(v.X, v.Y),
                (newX, newY),
                "Переместить объект",
                MarkDirty);
        }).ToList();

        if (commands.Count == 1)
            _commandHistory.Push(commands[0]);
        else
            _commandHistory.Push(new BatchCommand(commands, "Переместить объекты"));
    }

    /// <summary>
    /// Ограничить координату X так, чтобы опорная точка не выходила за границы листа.
    /// </summary>
    public long ClampX(long x) => Math.Clamp(x, 0, Template.Sheet.WidthMicrons);

    /// <summary>
    /// Ограничить координату Y так, чтобы опорная точка не выходила за границы листа.
    /// </summary>
    public long ClampY(long y) => Math.Clamp(y, 0, Template.Sheet.HeightMicrons);

    // === IEditorContext explicit implementation ===
    ICommand IEditorContext.SetActiveToolCommand => SetActiveToolCommand;

    private static string GetObjectWord(int count) => count switch
    {
        1 => "объект",
        2 or 3 or 4 => "объекта",
        _ => "объектов"
    };

    #region IDisposable

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // PropertyChanged подписки на менеджеры не требуют отписки — все они живут в том же
        // EditorViewModel и умирают вместе с ним. PreviewLineChangedBehavior — отдельная отписка.

        PreviewLineChangedBehavior.Unregister(this);
        _selectionManager.Dispose();
        PropertiesVm.Dispose();
        _clipboardManager.Clear();
        PrintVisualProvider = null;
    }

    #endregion
}
