## Context

Текст в `EditorCanvas.xaml` рендерится через `LayoutTransform` с `RotateTransform`. После тщательного анализа выяснено:

1. **WPF RotateTransform** с `CenterX=CenterY=0` (по умолчанию) вращает вокруг **верх-левого угла**, а не центра
2. **Направление**: WPF использует CW (Y-down), модель использует CCW (Y-up). Но после Y-flip (модель → WPF через `ModelYToCanvasTopConverter`) направления **совпадают** — положительный угол даёт одинаковый визуальный результат
3. **Маркеры выделения** (`RotatedCorner0-3` через `MarkerPosition` behavior) позиционируются корректно — совпадают с визуальными углами повёрнутого текста (в пределах точности FontMetrics)

**Реальная проблема:** `InlineTextEditor` (TextBox для редактирования текста на холсте) позиционируется с **неправильной Y-координатой**:
- `Canvas.Top` использует `MicronsY` (низ текста в модели)
- Должен использовать `BottomMicronsY` (= `MicronsY + HeightMicrons`, верх текста в модели)
- Разница: `HeightMicrons` — редактор находится **ниже** текста на `HeightMicrons` пикселей

Для неповёрнутого текста редактор появляется под текстом вместо поверх него.
Для повёрнутого текста — смещение по двум осям из-за LayoutTransform.

## Goals / Non-Goals

**Goals:**
- InlineTextEditor отображается ровно в той же позиции, что и редактируемый TextBlock
- Для повёрнутого текста редактор корректно позиционирован поверх текста

**Non-Goals:**
- Изменение модели (`Text.cs`) — математика корректна
- Изменение маркеров выделения — работают верно
- Изменение хит-тестирования — работает верно
- Изменение `RotateTransform` — работает верно (дефолтный центр = top-left, что и требуется)

## Decisions

### Решение 1: Исправить Y-позицию InlineTextEditor

**Проблема:** InlineTextEditor использует `Binding Path="InlineEditManager.InlineEditingText.MicronsY"` для `Canvas.Top`. Это позиционирует редактор на `HeightMicrons` ниже нужной точки (должен быть `MicronsY + HeightMicrons`).

**Решение:** Заменить `MicronsY` на `BottomMicronsY` в привязке `Canvas.Top` для InlineTextEditor.

**Доказательство:**
```
TextBlock Canvas.Top:    (sheetHeight - ToMm(MicronsY + HeightMicrons)) * zoom  ✓
InlineEditor Canvas.Top: (sheetHeight - ToMm(MicronsY)) * zoom                  ✗
Исправление:             (sheetHeight - ToMm(BottomMicronsY)) * zoom            ✓
```

### Решение 2: ModelXToCanvasLeftConverter — то же самое, что LeftEdgeMicronsMultiConverter

Оба дают `ToMm(MicronsX) * zoom` для Text. X-позиция корректна, менять не требуется.

### Почему маркеры НЕ трогаем

Маркеры (`RotatedCorner0-3`) используют ту же математику, что и WPF `RotateTransform` (CCW матрица, Y-flip, pivot=top-left). Позиции маркеров совпадают с визуальными углами текста. Бага в маркерах нет.

Если визуальное несовпадение всё же есть — это точность FontMetrics (WidthMicrons/HeightMicrons), а не ошибка в rotation-логике.

## Risks / Trade-offs

- **[InlineTextEditor высота]** Даже с правильной Y-позицией, InlineTextEditor может не совпадать по высоте с TextBlock (у TextBox другой шрифт, padding, border). **Mitigation:** Это expected — TextBox должен быть чуть больше для комфортного редактирования.
- **[Повёрнутый текст]** Для повёрнутого текста LayoutTransform на InlineTextEditor уже есть (RotateTransform), поворот будет корректен вокруг верх-левого угла — так же, как у TextBlock.
