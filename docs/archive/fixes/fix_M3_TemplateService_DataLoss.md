# P2-M3: Data loss при неудачном сохранении в TemplateService

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Services/TemplateService.cs:245-250`

**Симптом:** При сбое во время сохранения файла (например, диск полон, потеря питания) оригинальный файл будет потерян безвозвратно.

**Корень:** Алгоритм сохранения:
```csharp
// 1. Удаляем старый файл, если существует
if (File.Exists(filePath))
    File.Delete(filePath);

// 2. Создаём ZIP-архив (открываем stream на запись)
using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
```

Если шаг 2 прервётся (исключение, сбой), файл уже удалён, а новый не создан.

**Оптимальный паттерн (write-then-move):**
1. Записать во временный файл в той же директории
2. Атомарно заменить целевой файл временным через `File.Replace()`

## Пошаговый план исправления

### Шаг 1: Изменить метод Save

```csharp
public void Save(Template template, string filePath)
{
    if (template == null)
        throw new ArgumentNullException(nameof(template));
    if (string.IsNullOrWhiteSpace(filePath))
        throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(filePath));

    // Обновляем дату модификации
    template.Metadata.ModifiedDate = DateTime.UtcNow;

    var dto = MapToDto(template);

    // Создаём директорию, если не существует
    var directory = Path.GetDirectoryName(filePath);
    if (!string.IsNullOrEmpty(directory))
        Directory.CreateDirectory(directory);

    // Пишем во временный файл
    var tempPath = filePath + ".tmp";
    try
    {
        using (var archive = ZipFile.Open(tempPath, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry(TemplateXmlFileName, CompressionLevel.Optimal);
            using var entryStream = entry.Open();

            var serializer = GetTemplateSerializer();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using var xmlWriter = XmlWriter.Create(entryStream, settings);
            serializer.Serialize(xmlWriter, dto);
        }

        // Атомарно заменяем
        if (File.Exists(filePath))
            File.Replace(tempPath, filePath, null);          // Windows API: ReplaceFile
        else
            File.Move(tempPath, filePath);
    }
    catch
    {
        // Очищаем временный файл при ошибке
        try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
        throw;
    }

    _logger?.LogInformation("Сохранён шаблон: filePath={FilePath}, objects={ObjectCount}", filePath, template.Objects.Count);
}
```

### Шаг 2: Рассмотреть вариант с backup-файлом

Вместо `File.Replace(tempPath, filePath, null)` (который принимает третий параметр — backup filename), можно делать backup:

```csharp
// Сохранить старую версию как .bak
var backupPath = filePath + ".bak";
if (File.Exists(filePath))
    File.Replace(tempPath, filePath, backupPath);
else
    File.Move(tempPath, filePath);
```

Это даёт пользователю возможность восстановить предыдущую версию из `.bak` файла.

**Недостаток:** Накопление `.bak` файлов. Можно их периодически чистить.

### Шаг 3: (Опционально) Асинхронный вариант

Можно сделать `SaveAsync` с `CancellationToken`, но это потребует изменения контракта `ITemplateService`.

## Сравнение подходов

| Подход | Безопасность | Скорость | Сложность |
|--------|-------------|----------|-----------|
| Текущий (delete+create) | ❌ Потеря при сбое | ⭐ Быстро (сразу в файл) | 🟢 Просто |
| Write-then-move | ✅ Атомарно | ⭐ Быстро | 🟢 Просто |
| Write-then-move + backup | ✅✅ Двойная защита | 🟡 Чуть медленнее (копирование) | 🟡 Средняя |

**Рекомендуется:** Write-then-move без backup (минимальное изменение, максимальная безопасность).

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~TemplateService"
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- `File.Replace()` работает только на Windows (NTFS). Код уже предполагает Windows (WPF приложение), так что это не проблема.
- Временный файл сохраняется в той же директории, что целевой. Если нет прав на запись — исключение из `ZipFile.Open` будет обработано catch-блоком.
- На очень медленных дисках (сетевые) атомарность `File.Replace()` не гарантируется. Для сетевых дисков лучше использовать `File.Move`.
