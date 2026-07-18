# P2-M5: Аллокация массива при каждом вызове ValidateRotation

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Helpers/ValidationService.cs:424`

**Симптом:** При каждом вызове `ValidateRotation(int rotation)` создаётся новый массив `int[]`.

```csharp
public static string? ValidateRotation(int rotation)
{
    var validAngles = new[] { 0, 90, 180, 270 };    // ← аллокация на каждый вызов
    if (!validAngles.Contains(rotation))
        return $"Угол поворота должен быть одним из: {string.Join(", ", validAngles)}.";
    return null;
}
```

**Корень:** Массив создаётся локально в методе. Каждый вызов (а `ValidateRotation` может вызываться на каждый ввод пользователя в PropertyChanged) аллоцирует новый объект в куче.

**Влияние:**
- Каждый вызов: аллокация `int[]` + header + элементы
- Если вызывается 1000 раз — аллоцируется 1000 массивов
- GC должен собирать эти массивы
- В методе также вызывается `string.Join(", ", validAngles)` — ещё одна аллокация строки, но она неизбежна только при ошибке

**Правильное поведение:** Вынести массив в `private static readonly`.

## Пошаговый план исправления

### Шаг 1: Добавить статическое поле

В класс `ValidationService` (в начало, рядом с другими статическими членами):

```csharp
private static readonly int[] ValidRotationAngles = { 0, 90, 180, 270 };
```

### Шаг 2: Изменить ValidateRotation

```csharp
public static string? ValidateRotation(int rotation)
{
    if (!ValidRotationAngles.Contains(rotation))
        return $"Угол поворота должен быть одним из: {string.Join(", ", ValidRotationAngles)}.";
    return null;
}
```

### Шаг 3: Проверить, что массив не мутируется

`ValidRotationAngles` помечен как `readonly`. Содержимое массива технически можно изменить (`ValidRotationAngles[0] = 42`), но это будет нарушением контракта. В нашем случае массив не мутируется нигде.

### Шаг 4: (Оптимизация) Рассмотреть использование HashSet

```csharp
private static readonly HashSet<int> ValidRotationAngles = [0, 90, 180, 270];
```

`HashSet.Contains()` — O(1) против O(n) у массива. Но для 4 элементов разница ничтожна. Массив компактнее.

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~ValidationService"
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- Изменение тривиальное, рисков нет
- `ValidRotationAngles` используется только внутри `ValidateRotation`, область видимости `private` — никак не влияет на внешний API
