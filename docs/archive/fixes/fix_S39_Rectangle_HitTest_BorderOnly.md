# Sprint 39 — Rectangle HitTest: селекция только по границе

**Дата:** 24.06.2026
**Ветка:** —
**Статус:** Готово

---

## Список исправлений

### Fix S39-1: Прямоугольник выделяется при клике внутри области

**Проблема:** `Rectangle.ContainsPoint()` использовал полную AABB-проверку — любая точка *внутри* прямоугольника (включая центр) считалась попаданием. Это противоречило ожидаемому поведению: выделение должно происходить только при клике на линии сторон, а не на пустой области внутри.

**Исправление:** Метод переписан на **border-band подход**:
- Точка считается попавшей на прямоугольник, только если она находится в пределах `LineHitToleranceMicrons` (5 мм) от любой из четырёх сторон
- Внутренняя область (дальше 5 мм от краёв) не селектируется
- Для маленьких прямоугольников (< 10 мм в любом измерении) вся область остаётся селектируемой — корректное UX для мелких объектов

**Логика:**
```csharp
public override bool ContainsPoint(PointMicrons point)
{
    long tol = EditorConstants.LineHitToleranceMicrons;

    // Точка в расширенных границах (border + tol)?
    bool insideExpanded = point.MicronsX >= MicronsX - tol &&
                          point.MicronsX <= RightMicronsX + tol &&
                          point.MicronsY >= MicronsY - tol &&
                          point.MicronsY <= BottomMicronsY + tol;

    if (!insideExpanded)
        return false;

    // Точка во внутренней области (дальше tol от краёв)?
    bool insideInterior = point.MicronsX >= MicronsX + tol &&
                          point.MicronsX <= RightMicronsX - tol &&
                          point.MicronsY >= MicronsY + tol &&
                          point.MicronsY <= BottomMicronsY - tol;

    return !insideInterior; // селектируем только border-band
}
```

**Файлы:**
- `Models/Objects/Rectangle.cs` (`ContainsPoint` — заменена реализация)
- `Tests/Helpers/HitTestHelperTests.cs` (обновлены тесты: `PointInside_ReturnsTrue` → `PointInsideLargeRect_ReturnsFalse`, обновлены outside/negative с учётом expanded bounds)
- `Tests/Helpers/HitTestHelperExtendedTests.cs` (аналогично + новый тест `PointNearEdgeLargeRect_ReturnsTrue`)
- `Tests/Helpers/AdditionalHelperTests.cs` (обновлены `OnRectangle`/`OutsideRectangle`/`HitTestAll`/`NoObjectsUnderPoint`)
- `Tests/IntegrationTests.cs` (обновлён `HitTestAll_OverlappingObjects` — точка (14k,14k) вместо (15k,15k))

---

### Поведение после исправления

| Ситуация | Результат |
|----------|-----------|
| Клик по центру большого прямоугольника (200×100 мм) | ❌ Не выделяется |
| Клик рядом с границей (в пределах 5 мм) | ✅ Выделяется |
| Клик точно на границе | ✅ Выделяется |
| Клик далеко за границей | ❌ Не выделяется |
| Клик по центру маленького прямоугольника (< 10 мм) | ✅ Выделяется (весь объект — border-band) |

---

## Итог

| Метрика | До | После |
|---------|----|-------|
| **Build** | 0 errors, 4 warnings | 0 errors, 4 warnings |
| **Tests** | 589+ passed | 840+ passed |

**Изменённые файлы (5):**
- `Models/Objects/Rectangle.cs`
- `Tests/Helpers/HitTestHelperTests.cs`
- `Tests/Helpers/HitTestHelperExtendedTests.cs`
- `Tests/Helpers/AdditionalHelperTests.cs`
- `Tests/IntegrationTests.cs`
