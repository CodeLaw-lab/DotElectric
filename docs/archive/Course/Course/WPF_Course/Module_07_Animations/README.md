# Модуль 7: Анимации и Storyboards

**Время прохождения:** 10 часов  
**Уровень:** Продвинутый

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Научитесь использовать Timeline-анимации (DoubleAnimation, ColorAnimation)
- ✅ Освоите Storyboard и ControlTemplate анимации
- ✅ Поймёте Easing Functions для плавности
- ✅ Сможете создавать анимации в коде и XAML

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 7.1 | [Timeline-анимации](./01_Timeline_Animations.md) | 2 часа | Теория, примеры, 6 задач |
| 7.2 | [Storyboard](./02_Storyboard.md) | 3 часа | Теория, примеры, 6 задач |
| 7.3 | [Easing Functions](./03_Easing_Functions.md) | 2 часа | Теория, примеры, 6 задач |
| 7.4 | [Анимация в коде vs XAML](./04_Code_vs_XAML.md) | 1 час | Теория, примеры, 4 задачи |
| 7.5 | [Практическая работа](./05_Практическая_работа.md) | 3 часа | Интеграционное задание (100 баллов + бонусы) |

---

## Предварительные требования

Перед началом модуля убедитесь, что вы:
- [ ] Прошли Модули 1-6
- [ ] Понимаете Data Binding и MVVM
- [ ] Работали с Transform
- [ ] Знаете основы C# (классы, события, async/await)

---

## Краткое содержание тем

### Тема 7.1: Timeline-анимации

**Изучите:**
- DoubleAnimation — анимация double значений
- ColorAnimation — анимация цвета
- PointAnimation — анимация точек
- Duration, BeginTime, RepeatBehavior

**Пример:**
```xml
<DoubleAnimation Storyboard.TargetProperty="Width"
                 From="100" To="200"
                 Duration="0:0:1"/>
```

---

### Тема 7.2: Storyboard

**Изучите:**
- Storyboard как контейнер анимаций
- Storyboard.TargetName, Storyboard.TargetProperty
- Begin(), Pause(), Resume(), Stop()
- Анимации в ControlTemplate

**Пример:**
```xml
<Storyboard x:Key="MyStoryboard">
    <DoubleAnimation Storyboard.TargetName="rect"
                     Storyboard.TargetProperty="Width"
                     From="100" To="200" Duration="0:0:1"/>
</Storyboard>
```

---

### Тема 7.3: Easing Functions

**Изучите:**
- Что такое easing functions
- Встроенные easing (Quadratic, Cubic, Quartic, etc.)
- EaseIn, EaseOut, EaseInOut
- Custom easing

**Пример:**
```xml
<DoubleAnimation Duration="0:0:1">
    <DoubleAnimation.EasingFunction>
        <QuadraticEase EasingMode="EaseOut"/>
    </DoubleAnimation.EasingFunction>
</DoubleAnimation>
```

---

### Тема 7.4: Анимация в коде

**Изучите:**
- Создание анимаций в C#
- BeginStoryboard в коде
- Анимация через CompositionTarget
- Performance considerations

---

## Практическая работа

**Задание:** Создание анимированного UI

**Время:** 3 часа

**Требования:**
1. Анимация появления (Fade In)
2. Анимация перемещения
3. Анимация цвета
4. Easing functions
5. Интерактивные анимации

**Критерии оценки:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 8 убедитесь, что вы:

- [ ] Создали DoubleAnimation и ColorAnimation
- [ ] Использовали Storyboard с TargetName/TargetProperty
- [ ] Применили Easing Function (QuadraticEase, CubicEase)
- [ ] Создали анимацию в XAML и в коде
- [ ] Завершили практическую работу (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **Timeline** | Базовый класс для всех анимаций |
| **Storyboard** | Контейнер для организации анимаций |
| **DoubleAnimation** | Анимация double значений |
| **ColorAnimation** | Анимация цвета |
| **PointAnimation** | Анимация точек |
| **Duration** | Длительность анимации |
| **Easing Function** | Функция плавности для анимации |
| **BeginTime** | Задержка перед началом |
| **RepeatBehavior** | Повторение (Forever или N раз) |
| **AutoReverse** | Автоматический возврат |

---

## Переход к следующему модулю

➡️ **[Модуль 8: Кастомные контролы и Behaviors](../Module_08_CustomControls/README.md)**

В Модуле 8 изучим:
- UserControls vs Custom Controls
- Templated Controls (Themes/Generic.xaml)
- Attached Behaviors
- Interactivity (System.Windows.Interactivity)
