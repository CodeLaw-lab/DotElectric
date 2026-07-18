# Модуль 9: Ресурсы и темы (MaterialDesignThemes)

**Время прохождения:** 8 часов  
**Уровень:** Продвинутый

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Научитесь использовать ResourceDictionary и MergedDictionaries
- ✅ Сможете создавать темы (Light/Dark)
- ✅ Освоите динамическую смену тем
- ✅ Сможете интегрировать MaterialDesignThemes

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 9.1 | [ResourceDictionary и MergedDictionaries](./01_ResourceDictionary.md) | 2 часа | Теория, примеры, 5 задач |
| 9.2 | [Создание тем (Light/Dark)](./02_Creating_Themes.md) | 2 часа | Теория, примеры, 5 задач |
| 9.3 | [Динамическая смена тем](./03_Theme_Switching.md) | 2 часа | Теория, примеры, 5 задач |
| 9.4 | [MaterialDesignThemes интеграция](./04_MaterialDesign.md) | 2 часа | Теория, примеры, 5 задач |
| 9.5 | [Практическая работа](./05_Практическая_работа.md) | 3 часа | Интеграционное задание (100 баллов) |

---

## Предварительные требования

Перед началом модуля убедитесь, что вы:
- [ ] Прошли Модули 1-8
- [ ] Понимаете Dependency Properties
- [ ] Работали со стилями и шаблонами
- [ ] Знают основы XAML ресурсов

---

## Краткое содержание тем

### Тема 9.1: ResourceDictionary

**Изучите:**
- ResourceDictionary структура
- MergedDictionaries
- Pack URI для внешних словарей
- Организация ресурсов

**Пример:**
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Styles/Colors.xaml"/>
            <ResourceDictionary Source="Styles/Buttons.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

---

### Тема 9.2: Создание тем

**Изучите:**
- Light и Dark темы
- Цветовые палитры
- Темизация контролов
- Организация файлов тем

**Пример:**
```xml
<!-- LightTheme.xaml -->
<Color x:Key="WindowBackgroundColor">#FFFFFF</Color>
<Color x:Key="TextPrimaryColor">#212121</Color>

<!-- DarkTheme.xaml -->
<Color x:Key="WindowBackgroundColor">#212121</Color>
<Color x:Key="TextPrimaryColor">#FFFFFF</Color>
```

---

### Тема 9.3: Динамическая смена тем

**Изучите:**
- IThemeService интерфейс
- ThemeService реализация
- Переключение в runtime
- Сохранение настроек

**Пример:**
```csharp
public interface IThemeService
{
    bool IsDarkTheme { get; }
    void ToggleTheme();
}
```

---

### Тема 9.4: MaterialDesignThemes

**Изучите:**
- Установка пакета
- Базовая настройка
- MaterialDesign цвета
- Кастомизация

**Пример:**
```xml
<materialDesign:CustomColorTheme BaseTheme="Dark"
                                  PrimaryColor="#0078D4"
                                  SecondaryColor="#66BB6A"/>
```

---

## Практическая работа

**Задание:** Создание системы тем для приложения

**Время:** 3 часа

**Требования:**
1. ResourceDictionary с ресурсами
2. Light и Dark темы
3. ThemeService для переключения
4. MaterialDesignThemes интеграция

**Критерии оценки:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 10 убедитесь, что вы:

- [ ] Создали ResourceDictionary с ресурсами
- [ ] Создали Light и Dark темы
- [ ] Реализовали IThemeService
- [ ] Интегрировали MaterialDesignThemes
- [ ] Завершили практическую работу (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **ResourceDictionary** | Коллекция переиспользуемых ресурсов |
| **MergedDictionaries** | Объединение нескольких словарей |
| **Theme** | Набор ресурсов для визуального стиля |
| **DynamicResource** | Ресурс, обновляемый в runtime |
| **MaterialDesignThemes** | Популярная библиотека тем для WPF |
| **PrimaryColor** | Основной цвет темы |
| **SecondaryColor** | Дополнительный цвет темы |
| **BaseTheme** | Базовая тема (Light/Dark) |

---

## Переход к следующему модулю

➡️ **[Модуль 10: Продвинутые темы](../Module_10_Advanced/README.md)**

В Модуле 10 изучим:
- Multi-threading и Dispatcher
- Virtualization (UIVirtualizingPanel)
- Performance optimization (Freezable, BitmapCache)
- Interop (Win32, DirectX)
