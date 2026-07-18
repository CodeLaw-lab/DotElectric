## Context

`SelectionManager.ShowSelectionMarkers` — computed property: `=> SelectedObjects.Count > 0`. После R3.1 XAML биндится напрямую к `SelectionManager.ShowSelectionMarkers` (EditorCanvas.xaml:443), но `PropertyChanged` для этого свойства никогда не вызывается. Обработчик `CollectionChanged` вызывает только переданный извне `_onSelectionChanged` (который идёт в `EditorViewModel.OnSelectionChangedInternal`).

`SelectionManager` наследует `ObservableObject` — метод `OnPropertyChanged()` доступен.

## Goals / Non-Goals

**Goals:**
- Маркеры выделения появляются при выборе объекта
- Минимальное изменение — добавление `OnPropertyChanged()` в существующий обработчик

**Non-Goals:**
- Не менять архитектуру селекции
- Не добавлять `[ObservableProperty]` на computed property
- Не менять XAML, EditorViewModel, инструменты

## Decisions

**Решение:** добавить `OnPropertyChanged(nameof(ShowSelectionMarkers))` в лямбду `CollectionChanged` в конструкторе.

Альтернативы:
1. **Сделать `ShowSelectionMarkers` полноценным свойством с ручным сеттером** — потребует менять все места, где коллекция модифицируется (SelectSingle, AddToSelection, RemoveFromSelection, Clear, PurgeOrphaned, SelectAll). Хрупко — новое место может забыть вызвать сеттер.
2. **`[DependsOn(nameof(ShowSelectionMarkers))]`** — не существует в CommunityToolkit.Mvvm; `[NotifyPropertyChangedFor]` работает только на `[ObservableProperty]`.
3. **Передать `PropertyChanged` через `_onSelectionCallback` из EditorViewModel** — костыль, ломает инкапсуляцию.
4. **Выбранный подход:** 1 строка в единственном месте (CollectionChanged). Никаких пропущенных вызовов — коллекция меняется только через CollectionChanged, и мы реагируем на любое изменение.

## Risks / Trade-offs

- **Риск:** `OnPropertyChanged` вызовется дважды при операциях `Clear()+Add()` (два CollectionChanged) → [Mitigation] WPF эффективно обрабатывает частые PropertyChanged; визуально незаметно
- **Риск:** Кто-то в будущем сделает `ShowSelectionMarkers` ObservableProperty и сломает логику → [Mitigation] Комментарий над свойством
