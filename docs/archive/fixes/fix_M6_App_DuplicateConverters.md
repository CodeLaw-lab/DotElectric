# P2-M6: Дублирование конвертеров в App.xaml

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/App.xaml:21-29`

**Симптом:** Два конвертера зарегистрированы дважды под разными ключами:

```xml
<!-- Дубликат 1 -->
<converters:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
<converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>

<!-- Дубликат 2 -->
<converters:NotNullToVisibilityConverter x:Key="NotNullToVisibility"/>
<converters:NotNullToVisibilityConverter x:Key="NotNullToVisibilityConverter"/>
```

**Последствия:**
1. **Путаница:** Разработчики не знают, какой ключ использовать (`BoolToVisibility` или `BoolToVisibilityConverter`)
2. **Избыточность:** Два объекта одного типа в памяти
3. **Сложность рефакторинга:** При изменении одного из конвертеров нужно менять в двух местах

**Корень:** Несогласованное именование. Часть конвертеров использует суффикс `Converter`, часть — нет.

## Пошаговый план исправления

### Шаг 1: Определить стиль именования

В текущем App.xaml:
- **С суффиксом `Converter`:** `MicronsToMmConverter`, `ZoomToStringConverter`, `LineTypeToStringConverter`, `TextTypeToStringConverter`
- **Без суффикса:** `BoolToVisibility`, `IsNull`, `NotNullToVisibility`, `Not`

Большинство (4 из 6 однозначных) используют суффикс. Рекомендуем **единый стиль — без суффикса** (короче, в XAML читается естественнее): `{StaticResource BoolToVisibility}`.

### Шаг 2: Удалить дубликаты, оставить единый ключ

```xml
<!-- Глобальные конвертеры приложения -->
<converters:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
<converters:MicronsToMmConverter x:Key="MicronsToMmConverter"/>
<converters:ZoomToStringConverter x:Key="ZoomToStringConverter"/>
<converters:LineTypeToStringConverter x:Key="LineTypeToStringConverter"/>
<converters:TextTypeToStringConverter x:Key="TextTypeToStringConverter"/>
<converters:IsNullConverter x:Key="IsNull"/>
<converters:NotNullToVisibilityConverter x:Key="NotNullToVisibility"/>
<converters:TopEdgeMicronsConverter x:Key="TopEdgeMicronsConverter"/>
<converters:TopEdgeMicronsMultiConverter x:Key="TopEdgeMicronsMultiConverter"/>
<converters:LeftEdgeMicronsConverter x:Key="LeftEdgeMicronsConverter"/>
<converters:LeftEdgeMicronsMultiConverter x:Key="LeftEdgeMicronsMultiConverter"/>
<converters:LocalX1Converter x:Key="LocalX1Converter"/>
<converters:LocalY1Converter x:Key="LocalY1Converter"/>
<converters:LocalX2Converter x:Key="LocalX2Converter"/>
<converters:LocalY2Converter x:Key="LocalY2Converter"/>
<converters:InverseBoolConverter x:Key="InverseBoolConverter"/>
<converters:ZeroToVisibilityConverter x:Key="ZeroToVisibilityConverter"/>
<converters:NonZeroToVisibilityConverter x:Key="NonZeroToVisibilityConverter"/>
<converters:EqualToBoolConverter x:Key="EqualToBoolConverter"/>
<converters:ModelXToCanvasLeftConverter x:Key="ModelXToCanvasLeftConverter"/>
<converters:ModelYToCanvasTopConverter x:Key="ModelYToCanvasTopConverter"/>
<converters:MicronsToPixelConverter x:Key="MicronsToPixelConverter"/>
<converters:RelativeMicronsToPixelConverter x:Key="RelativeMicronsToPixelConverter"/>
<converters:LineTypeToDashArrayConverter x:Key="LineTypeToDashArrayConverter"/>
<converters:NotConverter x:Key="Not"/>
```

Удалены строки:
```xml
<converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
<converters:NotNullToVisibilityConverter x:Key="NotNullToVisibilityConverter"/>
```

### Шаг 3: Найти и обновить XAML-файлы, использующие удалённые ключи

```bash
# Поиск использований удалённых ключей
rg "BoolToVisibilityConverter" src/ --include "*.xaml"
rg "NotNullToVisibilityConverter" src/ --include "*.xaml"
```

Заменить `{StaticResource BoolToVisibilityConverter}` → `{StaticResource BoolToVisibility}` во всех найденных файлах.

### Шаг 4: Проверить C# код

```bash
rg "BoolToVisibilityConverter" src/ --include "*.cs"
rg "NotNullToVisibilityConverter" src/ --include "*.cs"
```

Если используются в C# (например, `TryFindResource("BoolToVisibilityConverter")`) — обновить строковые константы.

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests
# Визуально: запустить приложение, проверить, что конвертеры работают
# (видимость элементов, преобразование единиц)
dotnet run --project src/DotElectric.TemplateEditor
```

## Риски

- **Medium:** Если пропустить какой-то XAML-файл, использующий удалённый ключ — будет `ResourceReferenceKeyNotFoundException` при загрузке
- **Низкий:** После замены поведение не меняется (это один и тот же тип конвертера)
- **Рекомендация:** После замены запустить все UI-тесты и пройти критические сценарии вручную
