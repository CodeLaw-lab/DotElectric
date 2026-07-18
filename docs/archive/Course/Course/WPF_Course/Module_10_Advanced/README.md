# Модуль 10: Продвинутые темы

**Время прохождения:** 12 часов  
**Уровень:** Эксперт

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Научитесь работать с Multi-threading и Dispatcher
- ✅ Освоите Virtualization для больших коллекций
- ✅ Сможете оптимизировать производительность (Freezable, BitmapCache)
- ✅ Поймёте основы Interop (Win32, DirectX)

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 10.1 | [Multi-threading и Dispatcher](./01_Multithreading.md) | 3 часа | Теория, примеры, 5 задач |
| 10.2 | [Virtualization](./02_Virtualization.md) | 3 часа | Теория, примеры, 5 задач |
| 10.3 | [Performance Optimization](./03_Performance.md) | 3 часа | Теория, примеры, 5 задач |
| 10.4 | [Interop (Win32, DirectX)](./04_Interop.md) | 2 часа | Теория, примеры, 4 задачи |
| 10.5 | [Практическая работа](./05_Практическая_работа.md) | 4 часа | Интеграционное задание (100 баллов) |

---

## Предварительные требования

Перед началом модуля убедитесь, что вы:
- [ ] Прошли Модули 1-9
- [ ] Понимаете MVVM и Data Binding
- [ ] Работали с коллекциями
- [ ] Знаете основы многопоточности C#

---

## Краткое содержание тем

### Тема 10.1: Multi-threading и Dispatcher

**Изучите:**
- UI thread vs Background thread
- Dispatcher и DispatcherPriority
- async/await в WPF
- Progress reporting

**Пример:**
```csharp
// Неправильно - блокировка UI
private void LoadData()
{
    Thread.Sleep(5000); // UI замораживается
}

// Правильно - async/await
private async Task LoadDataAsync()
{
    await Task.Delay(5000); // UI не блокируется
}
```

---

### Тема 10.2: Virtualization

**Изучите:**
- UI Virtualization
- Data Virtualization
- VirtualizingStackPanel
- Custom VirtualizingPanel

**Пример:**
```xml
<ListBox VirtualizingPanel.IsVirtualizing="True"
         VirtualizingPanel.VirtualizationMode="Recycling">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

---

### Тема 10.3: Performance Optimization

**Изучите:**
- Freezable объекты
- BitmapCache
- Binding оптимизация
- Layout оптимизация

**Пример:**
```csharp
// Без Freeze - создаётся новый объект
var brush = new SolidColorBrush(Colors.Blue);

// С Freeze - переиспользуется
var brush = new SolidColorBrush(Colors.Blue);
brush.Freeze();
```

---

### Тема 10.4: Interop

**Изучите:**
- P/Invoke для Win32 API
- HwndSource для Win32 окон
- DirectX интеграция
- D3DImage

---

## Практическая работа

**Задание:** Оптимизация производительности приложения

**Время:** 4 часа

**Требования:**
1. Virtualization для ListBox
2. Async загрузка данных
3. Progress reporting
4. Freezable оптимизация

**Критерии оценки:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 11 убедитесь, что вы:

- [ ] Использовали Dispatcher для UI обновлений
- [ ] Включили Virtualization для ListBox
- [ ] Применили Freeze() для кистей
- [ ] Использовали async/await для долгих операций
- [ ] Завершили практическую работу (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **Dispatcher** | Очередь задач для UI потока |
| **UI Thread** | Поток, обрабатывающий UI |
| **Background Thread** | Фоновый поток для вычислений |
| **Virtualization** | Отрисовка только видимых элементов |
| **Freezable** | Объект, который можно заморозить для производительности |
| **BitmapCache** | Кэширование визуала в bitmap |
| **P/Invoke** | Вызов Win32 API из .NET |
| **HwndSource** | Обёртка для Win32 окна в WPF |

---

## Переход к следующему модулю

➡️ **[Модуль 11: Тестирование](../Module_11_Testing/README.md)**

В Модуле 11 изучим:
- Unit-тесты ViewModel
- Мокирование сервисов (Moq)
- UI-тесты (FlaUI, White)
- Code Coverage
