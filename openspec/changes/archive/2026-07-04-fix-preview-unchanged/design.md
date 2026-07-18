## Context

`PreviewManager` — менеджер, хранящий preview-объекты для отображения «резиновой нити» при рисовании. Три поля `_previewLine`, `_previewRectangle`, `_previewText` были переведены на `[ObservableProperty]` в рамках R2 (модельная чистка). Source-генератор создаёт сеттер с `EqualityComparer<T>.Default.Equals()`, который для reference-типов использует `ReferenceEquals`. Инструменты мутируют тот же экземпляр объекта и re-assign — сеттер видит «та же ссылка → пропускаем», PropertyChanged не стреляет.

SelectionBox-поля (`_selectionBoxLeft`, `_selectionBoxBottom`, `_selectionBoxWidth`, `_selectionBoxHeight`, `_selectionBoxDirection`) — value-типы (`long`, `byte`). Для них equality check корректен: если значение не изменилось, событие не нужно.

## Goals / Non-Goals

**Goals:**
- Preview-объекты снова отображаются при рисовании (Line, Rectangle, Text)
- SelectionBox-поля продолжают работать (их не трогать)
- Минимальное изменение — 3 поля, ~6 строк кода

**Non-Goals:**
- Не менять поведение PropertyChanged для остальных `[ObservableProperty]` в PreviewManager (SelectionBox, Direction)
- Не менять инструменты, behavior, XAML, EditorViewModel
- Не рефакторить архитектуру preview-механизма

## Decisions

**Решение: ручные сеттеры вместо `[ObservableProperty]` для 3 полей.**

Альтернативы:
1. **Безусловный вызов после присвоения** — `PreviewLine = value; OnPropertyChanged(nameof(PreviewLine))`. Два события: одно может быть подавлено, второе безусловное. Сбивает с толку читателя.
2. **Убрать `[ObservableProperty]`, оставить поле, написать сеттер вручную** — выбрано. Прозрачно, предсказуемо, соответствует Common Mistake #13 («Preview shapes — create once, update properties, re-assign reference for unconditional PropertyChanged»).
3. **Добавить `UnconditionalNotify()` метод** — лишняя абстракция для 6 строк.
4. **Переписать инструменты, чтобы каждый раз создавали новый объект** — антипаттерн (аллокации на каждый MouseMove).

## Risks / Trade-offs

- **Риск:** Забыть `OnPropertyChanged()` в одном из трёх сеттеров → [Mitigation] Все три поля идентичны по паттерну, code review тривиален
- **Риск:** Кто-то в будущем снова поставит `[ObservableProperty]` → [Mitigation] Добавить комментарий `// NOT [ObservableProperty] — unconditional notify needed for preview re-assign`
- **Недостаток:** Потеря source-генерации для трёх полей — но выигрыш в правильности поведения
