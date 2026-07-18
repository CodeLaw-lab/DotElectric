# Модуль 1: Введение в WPF

**Время прохождения:** 8 часов  
**Уровень:** Начинающий → Базовый

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Поймёте архитектуру WPF и систему рендеринга
- ✅ Научитесь создавать Dependency Properties и Attached Properties
- ✅ Разберётесь с Routed Events (Bubbling/Tunneling)
- ✅ Создадите первое WPF-приложение с кастомными контролами

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 1.1 | [Архитектура WPF](./01_Архитектура_WPF.md) | 2 часа | Теория, примеры, 5 задач |
| 1.2 | [Dependency Properties](./02_Dependency_Properties.md) | 3 часа | Теория, примеры, 6 задач |
| 1.3 | [Routed Events](./03_Routed_Events.md) | 2 часа | Теория, примеры, 6 задач |
| 1.4 | [Практическая работа](./04_Практическая_работа.md) | 3-4 часа | Интеграционное задание |

---

## Структура изучения

### Шаг 1: Архитектура WPF (01_Архитектура_WPF.md)

**Изучите:**
- Логическое vs визуальное дерево
- Система рендеринга через DirectX
- Device-independent units

**Выполните задачи:**
- 🟢 1.1.1: Первое окно (30 мин)
- 🟢 1.1.2: App.xaml с обработчиком исключений (30 мин)
- 🟡 1.1.3: Окно с Menu и StatusBar (1 час)
- 🔴 1.1.4: Анализ визуального дерева (2 часа)

---

### Шаг 2: Dependency Properties (02_Dependency_Properties.md)

**Изучите:**
- Чем DP отличаются от обычных свойств
- Регистрация DP с метаданными
- Attached Properties для любых элементов

**Выполните задачи:**
- 🟢 1.2.1: ColorBox control (45 мин)
- 🟢 1.2.2: AbsolutePanel с Attached Properties (45 мин)
- 🟡 1.2.3: PercentageSlider с валидацией (1.5 часа)
- 🟡 1.2.4: ResponsiveGrid Attached Properties (1.5 часа)
- 🔴 1.2.5: Система стилей (3 часа)

---

### Шаг 3: Routed Events (03_Routed_Events.md)

**Изучите:**
- Bubbling events (всплывающие)
- Tunneling events (нисходящие, Preview)
- Handled flag и маршрутизация

**Выполните задачи:**
- 🟢 1.3.1: Изучение маршрутизации (30 мин)
- 🟢 1.3.2: Блокировка клавиш (30 мин)
- 🟡 1.3.3: GlobalKeyInterceptor (1.5 часа)
- 🟡 1.3.4: Custom Routed Event с данными (1.5 часа)
- 🔴 1.3.5: Система команд через routed events (2.5 часа)

---

### Шаг 4: Практическая работа (04_Практическая_работа.md)

**Интеграционное задание:**
Создайте каркас WPF-приложения с кастомными контролами:
- ColorBox (DP: FillColor, BorderColor, CornerRadius)
- PercentageSlider (DP с валидацией)
- SmartButton (Custom Routed Event)
- GlobalKeyInterceptor (Attached Properties)

**Время:** 3-4 часа  
**Критерии:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 2 убедитесь, что вы:

- [ ] Понимаете разницу между логическим и визуальным деревом
- [ ] Можете зарегистрировать Dependency Property с метаданными
- [ ] Понимаете разницу между Bubbling и Tunneling событиями
- [ ] Создали хотя бы один кастомный контроль с DP
- [ ] Реализовали перехват клавиш через Preview-события
- [ ] Завершили практическую работу модуля (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **Logical Tree** | Иерархия элементов, объявленная в XAML |
| **Visual Tree** | Детализированная структура всех визуальных элементов |
| **Dependency Property** | Специальная система свойств WPF для binding, styling, animation |
| **Attached Property** | Свойство, которое можно «прикрепить» к любому элементу |
| **Bubbling Event** | Событие, идущее от источника к корню |
| **Tunneling Event** | Событие, идущее от корня к источнику (Preview) |
| **Handled Flag** | Флаг `e.Handled`, останавливающий маршрутизацию |
| **Coercion** | Корректировка значения перед установкой |
| **Validation** | Валидация значения перед установкой |

---

## Дополнительные ресурсы

### Официальная документация
- [WPF Architecture](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/)
- [Dependency Properties](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/properties/dependency-properties-overview)
- [Routed Events](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/input/routed-events-overview)

### Видео
- [WPF Architecture (NDC)](https://www.youtube.com/watch?v=...)
- [Deep Dive: Dependency Properties](https://www.youtube.com/watch?v=...)

### Книги
- «WPF 4.5 Unleashed» — Adam Nathan (Глава 7: Dependency Properties)
- «Pro WPF in .NET» — Adam Freeman (Главы 4-6)

---

## Переход к следующему модулю

➡️ **[Модуль 2: XAML — язык разметки](../Module_02_XAML/README.md)**

В Модуле 2 изучим:
- Синтаксис XAML и пространства имён
- Markup Extensions
- Ресурсы (StaticResource, DynamicResource)
- Стили и триггеры
- Шаблоны (ControlTemplate, DataTemplate)
