## Context

Кнопка «Вписать в экран» и Ctrl+0 не работают. Команда не достигает `ZoomPanManager.FitToScreen()`.

Текущий код:
```csharp
[RelayCommand]
private void FitToScreenCommand(string? parameter)  // ← метод уже содержит "Command"
{
    // ...
    FitToScreen(viewportWidth, viewportHeight);     // ← вызывает public overload
}

public void FitToScreen(double canvasWidth, double canvasHeight)
    => _zoomPanManager.FitToScreen(canvasWidth, canvasHeight);  // ← delegation
```

CommunityToolkit.Mvvm 8.4.2 source generator:
- Видит метод `FitToScreenCommand`
- Append "Command" → генерирует свойство `FitToScreenCommandCommand`
- XAML `{Binding FitToScreenCommand}` не находит свойство

Подтверждено анализом бинарника DLL: строка `get_FitToScreenCommandCommand` присутствует, XAML строка `FitToScreenCommand` не совпадает.

## Goals / Non-Goals

**Goals:**
- Ctrl+0 и кнопка «Вписать в экран» работают
- Минимальное изменение — rename одного метода

**Non-Goals:**
- Изменения логики FitToScreen, ZoomPanManager, XAML
- Изменения других RelayCommand (ZoomIn, ZoomOut, ToggleGrid и т.д.)

## Decisions

### 1. Переименовать метод, а не XAML

**Решение:** `FitToScreenCommand(...)` → `FitToScreen(...)`, XAML остаётся `FitToScreenCommand`.

**Rationale:**
- Source generator: `FitToScreen` → `FitToScreenCommand` (совпадает с XAML)
- Все остальные команды следуют этому паттерну: `ZoomIn` → `ZoomInCommand`
- Менять XAML (на `FitToScreenCommandCommand`) — неверный стиль, ломает паттерн

### 2. Удалить public overload `FitToScreen(double, double)`

**Решение:** Перенести делегирование `ZoomPanManager.FitToScreen()` прямо в команду.

**Rationale:**
- Overload используется ТОЛЬКО внутри команды (1 callsite)
- После переименования метода команды в `FitToScreen` будет конфликт имён с `FitToScreen(double, double)`
- `ZoomPanManager.FitToScreen()` — единственная необходимая public точка входа

## Risks / Trade-offs

**R1: Сломаются тесты, использующие `editor.FitToScreen(800, 600)`**
- Проверить: есть ли тесты, вызывающие `editor.FitToScreen()` напрямую, а не через команду
- Mitigation: найти все вызовы, обновить на команду или на прямой вызов `_zoomPanManager.FitToScreen()`
