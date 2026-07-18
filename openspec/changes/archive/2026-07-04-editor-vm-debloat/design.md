## Context

EditorViewModel — центральный ViewModel редактора, наследует `ObservableObject` и реализует `IAutosaveTab` / `IEditorContext`. В спринте R3.1 XAML-биндинги уже переведены на прямые обращения к менеджерам (`{Binding ZoomPanManager.Zoom}` вместо `{Binding Zoom}`), но 43 forwarding-свойства и 4 PropertyChanged-обработчика остались в коде как мёртвый груз.

### Текущая архитектура

```
XAML ──▶ forwarding property ──▶ manager (PropertyChanged)
        (EditorViewModel)              │
                                       ▼
                              INPC handler (EditorViewModel)
                                       │
                                       ▼
                              OnPropertyChanged → WPF binding update
```

Сейчас цепочка уведомлений: `manager.OnPropertyChanged → forwarding handler → EditorViewModel.OnPropertyChanged → WPF`. После рефакторинга: `manager.OnPropertyChanged → WPF` напрямую.

### Что было сделано в R3.1

- XAML переведён на прямые биндинги: `ZoomPanManager.Zoom`, `SelectionManager.SelectedObjects`, `PreviewManager.SelectionBoxWidth` и т.д.
- 9 менеджеров доступны как public properties (`ZoomPanManager`, `ToolManager`, `SelectionManager` и т.д.)
- Удаление forwarding-свойств логически завершает R3.1

## Goals / Non-Goals

**Goals:**
- Удалить ~290 строк мёртвого кода (43 forwarding-свойства, 4 INPC-обработчика)
- Устранить источник багов «забыли пробросить свойство» (Sprint 48)
- Сократить EditorViewModel с ~1194 до ~904 строк
- Упростить добавление новых свойств в менеджеры (не требует правки EditorViewModel)
- Сохранить полную обратную совместимость API (IAutosaveTab, IEditorContext, XAML-биндинги)

**Non-Goals:**
- Не менять поведение системы
- Не менять архитектуру менеджеров или их public API
- Не трогать PropertiesViewModel (это отдельный де-bloat)
- Не рефакторить команды или тулы
- Не менять IEditorContext или IAutosaveTab интерфейсы

## Decisions

### D1: Explicit interface implementation для IAutosaveTab вместо удаления 3 свойств

**Решение:** `FilePath`, `DisplayName` и `IsDirty` переводятся на explicit implementation `IAutosaveTab`. Не удаляются полностью, потому что `MainViewModel` обращается к ним через `IAutosaveTab` (через `tab.X` и `SelectedTab.X`).

**Альтернативы:**
- **Оставить как public forwarding** — не решает проблему, эти 3 свойства так и останутся прослойкой
- **Изменить MainViewModel на `tab.DirtyStateManager.X`** — принято (см. D2)

**Итог:** Explicit impl + MainViewModel changes. Убирает public API, но сохраняет контракт IAutosaveTab.

### D2: MainViewModel использует DirtyStateManager напрямую

**Решение:** `tab.FilePath` → `tab.DirtyStateManager.FilePath`, `tab.IsDirty` → `tab.DirtyStateManager.IsDirty`, `tab.DisplayName` → `tab.DirtyStateManager.DisplayName`. Это делает зависимость явной: MainViewModel напрямую зависит от DirtyStateManager через EditorViewModel.

**Почему не `((IAutosaveTab)tab).FilePath`:** DirtyStateManager — правильный источник истины для этих данных. Cast к интерфейсу скрывал бы реальную архитектуру.

### D3: OnZoomChangedInternal удаляется целиком

**Решение:** ZoomPanManager уже является `ObservableObject` и оповещает WPF при изменении своих `[ObservableProperty]` (Zoom, CanvasWidthPixels и т.д.). XAML уже подписан на `ZoomPanManager.Zoom`, поэтому ретрансляция через EditorViewModel избыточна.

### D4: OnSelectionChangedInternal упрощается

**Решение:** Убирается `OnPropertyChanged(nameof(ShowSelectionMarkers))` и `OnPropertyChanged(nameof(SingleSelectedObject))`. Эти оповещения нужны были, когда XAML биндился к `{Binding ShowSelectionMarkers}` на EditorViewModel. Теперь XAML использует `{Binding SelectionManager.ShowSelectionMarkers}`, и SelectionManager оповещает самостоятельно.

`SelectionVersion` остаётся: он используется в `IsObjectSelectedConverter` (IMultiValueConverter) для принудительного пересчёта биндингов выделения.

### D5: Внутренние использования SelectedObjects

**Решение:** Все места внутри EditorViewModel, где используется `SelectedObjects` (команды DeleteSelected, MoveSelected, RotateSelected и т.д.), меняются на `_selectionManager.SelectedObjects`. Это последовательный стиль: внутри класса используем `_manager`, снаружи — public manager properties.

## Риски / Trade-offs

| Риск | Вероятность | Митигация |
|------|-------------|-----------|
| Пропущенный external usage forwarding-свойства | Низкая | Полный grep по солюшену после каждого этапа |
| Поломка биндингов TemplateLibraryView (DisplayName, StatusMessage) | Низкая | Проверить: TemplateLibraryView использует TemplateLibraryItemViewModel, не EditorViewModel |
| MainViewModel.tab — тип EditorViewModel, не IAutosaveTab | Средняя | tab — EditorViewModel, все вызовы идут через public forwarding. После удаления нужно переключить на DirtyStateManager |
| Тесты обращаются к удаляемым свойствам | Средняя | Найти через grep, исправить на `editor.ZoomPanManager.Zoom` и т.д. |
| EditorCanvas.xaml.cs.GridInvalidated / GridNodes | Низкая | Заменить на `vm.GridManager.GridInvalidated` и `vm.GridManager.GridNodes` — GridManager уже имеет эти свойства |
