## Why

В редакторе шаблонов DotElectric отсутствует предпросмотр печати — пользователь не может увидеть, как лист с объектами будет выглядеть на бумаге, до отправки на принтер. Это снижает качество работы: инженер узнаёт об ошибках позиционирования или несоответствии формату только после печати.

## What Changes

- Добавить `IPrintDocumentGenerator` — сервис, который строит `FixedDocument` из `Template` (модельные объекты) без UI-хлама (сетка, маркеры, preview)
- Добавить `PrintPreviewWindow` — окно с `DocumentViewer` для просмотра `FixedDocument`
- Добавить `PreviewCommand` в `MainViewModel` (Ctrl+Shift+P)
- Подключить существующий пункт меню «Предпросмотр печати» (сейчас заглушка)
- Существующий `IPrintService.PrintWithVisual()` и Ctrl+P не изменяются

## Capabilities

### New Capabilities
- `print-preview`: Предпросмотр печати шаблона перед отправкой на принтер. Отображает только лист и нарисованные объекты (без сетки, маркеров выделения, preview-призраков). Использует `DocumentViewer` со встроенными zoom, пагинацией и кнопкой «Печать» (открывает стандартный `PrintDialog`).

### Modified Capabilities

(нет изменений существующих спецификаций)

## Impact

- **Новые файлы:** `IPrintDocumentGenerator.cs`, `PrintDocumentGenerator.cs`, `PrintPreviewWindow.xaml/.cs`
- **Изменяемые файлы:** `MainViewModel.cs` (+PreviewCommand), `MainWindow.xaml` (привязать команду к существующему MenuItem), `App.xaml.cs` (регистрация DI)
- **Тесты:** `PrintDocumentGeneratorTests.cs` (~50 тестов на рендеринг каждого типа объектов)
- **Зависимости:** WPF `DocumentViewer` (встроенный, дополнительных пакетов не требует)
