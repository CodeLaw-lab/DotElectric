using System;
using System.Windows;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Инструмент выделения и перетаскивания объектов.
/// Поддерживает: клик (выделение), Shift+клик (добавление), Ctrl+клик (снятие),
/// перетаскивание (move), Delete, стрелки, Selection Box (рамка выделения).
/// </summary>
public sealed class SelectTool : ITool
{
    private readonly IEditorContext _context;

    /// <summary>
    /// Перетаскиваемый объект (для drag).
    /// </summary>
    private TemplateObjectBase? _draggedObject;

    /// <summary>
    /// Начальная позиция перетаскивания (для расчёта дельты).
    /// </summary>
    private PointMicrons _dragStartModelPoint;

    /// <summary>
    /// Сохранённые начальные координаты ВСЕХ выделенных объектов (для Undo multi-selection).
    /// Ключ — объект, Значение — (X, Y) в микронах на момент OnMouseDown.
    /// </summary>
    private Dictionary<TemplateObjectBase, (long X, long Y)> _initialPositions = new();

    /// <summary>
    /// Флаг: идёт ли перетаскивание (а не просто клик).
    /// </summary>
    private bool _isDragging;

    /// <summary>
    /// Флаг: было ли перетаскивание при MouseMove (для предотвращения двойного срабатывания).
    /// </summary>
    private bool _wasDragged;

    // === Selection Box ===

    /// <summary>
    /// Флаг: рисуется ли рамка выделения.
    /// </summary>
    private bool _isDrawingSelectionBox;

    /// <summary>
    /// Начальная точка рамки выделения.
    /// </summary>
    private PointMicrons _selectionBoxStart;

    public string Name => "Выделение";

    public SelectTool(IEditorContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void OnMouseDown(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (button != ToolMouseButton.Left)
            return;

        _dragStartModelPoint = modelPoint;
        _isDragging = false;
        _wasDragged = false;
        _draggedObject = null;
        _initialPositions.Clear();

        // === Приоритет 1: Hit по маркеру single-selected объекта → ResizeTool ===
        if (_context.SingleSelectedObject != null)
        {
            var handle = HitTestHelper.GetHitHandle(modelPoint, _context.SingleSelectedObject);
            if (handle.HasValue)
            {
                _context.ActiveResizeHandle = handle;
                _context.HoveredHandle = handle;
                _context.PushTool("Resize");
                // Делегируем ResizeTool
                var resizeTool = _context.GetOrCreateTool<ResizeTool>();
                resizeTool.OnMouseDown(modelPoint, button, modifiers);
                return;
            }
        }

        // === Приоритет 2: Hit по объекту → выделение + drag ===
        var hit = HitTestHelper.HitTest(modelPoint, _context.Template.Objects);

        if (hit != null)
        {
            _draggedObject = hit;

            // Логика выделения
            if ((modifiers & ToolModifiers.Shift) != 0)
            {
                // Shift+клик — добавить к выделению
                _context.AddToSelection(hit);
            }
            else if ((modifiers & ToolModifiers.Ctrl) != 0)
            {
                // Ctrl+клик — снять с выделения
                if (_context.SelectedObjects.Contains(hit))
                    _context.RemoveFromSelection(hit);
            }
            else if (!_context.SelectedObjects.Contains(hit))
            {
                // Обычный клик — выделить только этот объект
                _context.SelectSingle(hit);
            }

            // Сохраняем начальные позиции ВСЕХ выделенных объектов (для Undo multi-selection)
            foreach (var obj in _context.SelectedObjects)
            {
                _initialPositions[obj] = (obj.MicronsX, obj.MicronsY);
            }

            // Начинаем перетаскивание
            _isDragging = true;
        }
        else
        {
            // Клик в пустое место — начать рисование Selection Box
            if ((modifiers & ToolModifiers.Shift) == 0)
            {
                _context.ClearSelection();
            }

            _isDrawingSelectionBox = true;
            _selectionBoxStart = modelPoint;

            // Сбросить preview рамки
            _context.SelectionBoxLeft = 0;
            _context.SelectionBoxBottom = 0;
            _context.SelectionBoxWidth = 0;
            _context.SelectionBoxHeight = 0;
            _context.SelectionDirection = SelectionDirection.LeftToRight;
        }
    }

    public void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        // === Hover detection (когда НЕ зажата кнопка) ===
        if (!_isDragging && !_isDrawingSelectionBox && button != ToolMouseButton.Left)
        {
            UpdateHoverState(modelPoint);
        }

        // === Selection Box drawing ===
        if (_isDrawingSelectionBox)
        {
            var dx = modelPoint.MicronsX - _selectionBoxStart.MicronsX;
            var dy = modelPoint.MicronsY - _selectionBoxStart.MicronsY;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            // Показываем рамку только если отрисовали хотя бы 3мм
            if (distance >= PhysicalConstants.SelectionBoxThresholdMicrons)
            {
                var left = Math.Min(_selectionBoxStart.MicronsX, modelPoint.MicronsX);
                var bottom = Math.Min(_selectionBoxStart.MicronsY, modelPoint.MicronsY);
                var width = Math.Abs(dx);
                var height = Math.Abs(dy);

                _context.SelectionBoxLeft = left;
                _context.SelectionBoxBottom = bottom;
                _context.SelectionBoxWidth = width;
                _context.SelectionBoxHeight = height;
                _context.SelectionDirection = _selectionBoxStart.MicronsX <= modelPoint.MicronsX ? SelectionDirection.LeftToRight : SelectionDirection.RightToLeft;
            }
            return;
        }

        // === Object dragging ===
        if (!_isDragging || _draggedObject == null)
            return;

        // Проверяем, достаточно ли сдвинулся курсор (threshold 3мм модели с учётом зума)
        var deltaDragX = modelPoint.MicronsX - _dragStartModelPoint.MicronsX;
        var deltaDragY = modelPoint.MicronsY - _dragStartModelPoint.MicronsY;
        var dragDistance = Math.Sqrt(deltaDragX * deltaDragX + deltaDragY * deltaDragY);

        // Threshold: 3мм модели, но не меньше 1мм на экране
        var thresholdMicrons = Math.Max(PhysicalConstants.SelectionBoxThresholdMicrons, (long)(PhysicalConstants.SelectionBoxThresholdMicrons / _context.Zoom));

        if (dragDistance < thresholdMicrons)
            return;

        _wasDragged = true;

        // Дельта перемещения от точки старта перетаскивания
        var delta = new PointMicrons(deltaDragX, deltaDragY);

        // Применяем snap to grid если включён
        var stepMicrons = _context.GridSettings.StepMicrons;
        var snapEnabled = _context.GridSettings.SnapEnabled;

        foreach (var obj in _context.SelectedObjects)
        {
            (long X, long Y) pos;
            if (!_initialPositions.TryGetValue(obj, out pos))
                pos = (obj.MicronsX, obj.MicronsY);
            var newX = pos.X + delta.MicronsX;
            var newY = pos.Y + delta.MicronsY;

            if (snapEnabled && stepMicrons > 0)
            {
                newX = SnapHelper.SnapX(newX, stepMicrons);
                newY = SnapHelper.SnapY(newY, stepMicrons);
            }

            obj.Move(
                _context.ClampX(newX),
                _context.ClampY(newY));
        }
    }

    public void OnMouseUp(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        // === Завершение Selection Box ===
        if (_isDrawingSelectionBox)
        {
            _isDrawingSelectionBox = false;

            var dx = modelPoint.MicronsX - _selectionBoxStart.MicronsX;
            var dy = modelPoint.MicronsY - _selectionBoxStart.MicronsY;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            // Если рамка была достаточно большой — выделяем объекты
            if (distance >= PhysicalConstants.SelectionBoxThresholdMicrons)
            {
                var selectionBox = new RectMicrons(
                    Math.Min(_selectionBoxStart.MicronsX, modelPoint.MicronsX),
                    Math.Min(_selectionBoxStart.MicronsY, modelPoint.MicronsY),
                    Math.Max(_selectionBoxStart.MicronsX, modelPoint.MicronsX),
                    Math.Max(_selectionBoxStart.MicronsY, modelPoint.MicronsY));

                var direction = SelectionBoxHelper.GetDirection(_selectionBoxStart, modelPoint);
                var selected = SelectionBoxHelper.GetSelectedObjects(selectionBox, _context.Template.Objects, direction);

                _context.SelectedObjects.Clear();
                foreach (var obj in selected)
                    _context.SelectedObjects.Add(obj);
            }

            // Скрыть preview рамки
            _context.SelectionBoxLeft = 0;
            _context.SelectionBoxBottom = 0;
            _context.SelectionBoxWidth = 0;
            _context.SelectionBoxHeight = 0;
            _context.SelectionDirection = SelectionDirection.LeftToRight;
            return;
        }

        // === Завершение drag ===
        if (!_isDragging)
            return;

        _isDragging = false;

        if (_wasDragged && _context.SelectedObjects.Count > 0)
        {
            if (_context.SelectedObjects.Count == 1)
            {
                var obj = _context.SelectedObjects[0];
                var oldPos = _initialPositions.TryGetValue(obj, out var pos) ? pos : (obj.MicronsX, obj.MicronsY);
                var cmd = new ChangePropertyCommand<(long X, long Y)>(
                    oldPos,
                    v => obj.Move(v.X, v.Y),
                    (obj.MicronsX, obj.MicronsY),
                    "Переместить объект",
                    _context.MarkDirty);
                _context.CommandHistory.Push(cmd);
            }
            else
            {
                var subCommands = new List<IUndoCommand>();
                foreach (var obj in _context.SelectedObjects)
                {
                    var oldPos = _initialPositions.TryGetValue(obj, out var pos) ? pos : (obj.MicronsX, obj.MicronsY);
                    var cmd = new ChangePropertyCommand<(long X, long Y)>(
                        oldPos,
                        v => obj.Move(v.X, v.Y),
                        (obj.MicronsX, obj.MicronsY),
                        "Переместить объект",
                        null);
                    subCommands.Add(cmd);
                }
                var batchCmd = new BatchCommand(subCommands, "Переместить объекты", _context.MarkDirty);
                _context.CommandHistory.Push(batchCmd);
            }
        }

        _draggedObject = null;
    }

    public void OnDoubleClick(PointMicrons modelPoint)
    {
        // Двойной клик по текстовому объекту — начать inline-редактирование
        var hit = HitTestHelper.HitTest(modelPoint, _context.Template.Objects);
        if (hit is Text text)
        {
            _context.SelectSingle(text);
            _context.StartInlineEditing(text);
        }
    }

    public bool OnMouseWheel(int delta, PointMicrons modelPoint) => false;

    public ToolCursor GetCursor()
    {
        // Если наведены на маркер — перекрестие
        if (_context.HoveredHandle.HasValue)
            return ToolCursor.Cross;
        // Если drag — рука
        if (_isDragging)
            return ToolCursor.Hand;
        // Если hover на объекте — рука
        if (_context.HoveredObject != null)
            return ToolCursor.Hand;
        return ToolCursor.Arrow;
    }

    /// <summary>
    /// Обновить состояние hover: определить объект и маркер под курсором.
    /// </summary>
    private void UpdateHoverState(PointMicrons modelPoint)
    {
        // Сначала проверяем hit по маркерам single-selected объекта
        if (_context.SingleSelectedObject != null)
        {
            var handle = HitTestHelper.GetHitHandle(modelPoint, _context.SingleSelectedObject);
            _context.HoveredHandle = handle;
            if (handle.HasValue)
            {
                _context.HoveredObject = null;
                return;
            }
        }

        // Проверяем hit по объектам
        var hit = HitTestHelper.HitTest(modelPoint, _context.Template.Objects);
        _context.HoveredObject = hit;
        _context.HoveredHandle = null;
    }

    public void Reset()
    {
        _draggedObject = null;
        _isDragging = false;
        _wasDragged = false;
        _isDrawingSelectionBox = false;
        _initialPositions.Clear();
        _context.SelectionBoxLeft = 0;
        _context.SelectionBoxBottom = 0;
        _context.SelectionBoxWidth = 0;
        _context.SelectionBoxHeight = 0;
        _context.SelectionDirection = SelectionDirection.LeftToRight;
    }

    public bool OnKeyDown(ToolKey key, ToolModifiers modifiers)
    {
        if (key == ToolKey.Delete)
        {
            _context.DeleteSelected();
            return true;
        }

        if (key == ToolKey.Escape)
        {
            _context.ClearSelection();
            _context.HoveredObject = null;
            _context.HoveredHandle = null;
            Reset();
            return true;
        }

        return false;
    }
}
