# Fix S49 — ResizeTool: Clamp двигал фиксированные грани + тесты под старую бажную логику

## Проблема: Clamp игнорировал, какая грань фиксированная

**Корень:** Минимальный размер (`MinResizeSizeMicrons = 1000`) накладывался через безусловный `Min`/`Max` на **обе** грани в оси:
```csharp
newX = Math.Min(newX, newRight - minSize);
newRight = Math.Max(newRight, newX + minSize);
```

Это двигало **фиксированную** грань, когда движущаяся грань пересекала границу minSize. Например, для маркера `TopRight` (двигаются правая и верхняя грани, левая и нижняя — фиксированы):
- Пользователь тянет правую грань влево за левую грань
- Clamp двигает **левую** (фиксированную) грань влево, хотя должна ограничиваться **правая** (движущаяся)

### Затронутые сценарии:
- `TopRight`: `newX` (левая) сдвигалась, хотя должна оставаться на `_startX`
- `TopLeft`: `newRight` (правая) сдвигалась, хотя должна оставаться на `startRight`
- `BottomRight`: `newTop` (верхняя) сдвигалась, хотя должна оставаться на `startTop` (только при dy > 0)
- `BottomLeft`: `newRight` и `newTop` сдвигались

## Исправление: Clamp только движущихся граней

**Файл:** `Tools/ResizeTool.cs` (строки 248-282)

Заменён безусловный clamp на handle-зависимый:
1. Определяются `leftMoves`, `rightMoves`, `bottomMoves`, `topMoves` по типу маркера
2. При Ctrl — все грани считаются движущимися
3. Для каждой оси:
   - Движется только одна грань → ограничивается **только она** (фиксированная не трогается)
   - Движутся обе (Ctrl) → симметричный схлоп через середину при нарушении minSize

### Логика для каждого маркера (без Ctrl):

| Маркер | Движущиеся грани | Clamp |
|--------|-----------------|-------|
| `Left` | Левая | `newX = Min(newX, newRight - minSize)` |
| `Right` | Правая | `newRight = Max(newRight, newX + minSize)` |
| `Top` | Верхняя | `newTop = Max(newTop, newY + minSize)` |
| `Bottom` | Нижняя | `newY = Min(newY, newTop - minSize)` |
| `TopLeft` | Левая, верхняя | `newX = Min(...)`, `newTop = Max(...)` |
| `TopRight` | Правая, верхняя | `newRight = Max(...)`, `newTop = Max(...)` |
| `BottomLeft` | Левая, нижняя | `newX = Min(...)`, `newY = Min(...)` |
| `BottomRight` | Правая, нижняя | `newRight = Max(...)`, `newY = Min(...)` |
| Любой + Ctrl | Все | симметричный схлоп |

## Сопутствующее: Тесты под старую бажную логику

14 тестов в `ResizeToolTests.cs` и `ResizeToolExtendedTests.cs` содержали ожидаемые значения, соответствующие **старой бажной формуле**. Некоторые тесты ожидали:
- `_startHeight + dy` для BottomRight — но в edge-based модели BottomRight двигает нижнюю грань: `newHeight = startTop - (_startY + dy) = _startHeight - dy` (для положительного dy высота уменьшается)
- `_startWidth - dx + (_startX - newX)` (double-delta) для Left/TopLeft/BottomLeft
- `_startY + (-dy/2)` (half-delta) для TopRight
- Перемещение фиксированных граней при минимальном размере

Все тесты переписаны под корректную edge-based модель.

## Проверка
- 63/63 ResizeTool тестов пройдены
- 1500+ тестов всех остальных категорий пройдены (кроме pre-existing hang в FileService)
