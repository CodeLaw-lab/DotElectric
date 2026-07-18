# Модуль 6: Работа с графикой

**Время прохождения:** 14 часов  
**Уровень:** Продвинутый

---

## Цели модуля

После изучения этого модуля вы:
- ✅ Научитесь использовать Shapes (Line, Rectangle, Ellipse, Path)
- ✅ Освоите Transform (RotateTransform, ScaleTransform, TranslateTransform)
- ✅ Поймёте Geometry и Path-язык
- ✅ Сможете использовать Visual Layer и DrawingVisual
- ✅ Научитесь переопределять OnRender для кастомного рендеринга

---

## Темы модуля

| № | Тема | Время | Материалы |
|---|------|-------|-----------|
| 6.1 | [Shapes (Line, Rectangle, Ellipse)](./01_Shapes.md) | 3 часа | Теория, примеры, 6 задач |
| 6.2 | [Transform (Rotate, Scale, Translate)](./02_Transforms.md) | 3 часа | Теория, примеры, 6 задач |
| 6.3 | [Geometry и Path-язык](./03_Geometry.md) | 3 часа | Теория, примеры, 6 задач |
| 6.4 | [Visual Layer и DrawingVisual](./04_Visual_Layer.md) | 2 часа | Теория, примеры, 6 задач |
| 6.5 | [OnRender override](./05_OnRender.md) | 2 часа | Теория, примеры, 6 задач |
| 6.6 | [Практическая работа](./06_Практическая_работа.md) | 4 часа | Интеграционное задание (100 баллов + бонусы) |

---

## Предварительные требования

Перед началом модуля убедитесь, что вы:
- [ ] Прошли Модули 1-5
- [ ] Понимаете Data Binding и MVVM
- [ ] Работали с Canvas для абсолютного позиционирования
- [ ] Знаете основы C# (классы, методы, свойства)

---

## Краткое содержание тем

### Тема 6.1: Shapes

**Изучите:**
- Line, Rectangle, Ellipse, Polygon, Polyline
- Stroke, Fill, StrokeThickness
- Stretch и выравнивание

**Пример:**
```xml
<Canvas>
    <Line X1="0" Y1="0" X2="100" Y2="100" 
          Stroke="Black" StrokeThickness="2"/>
    <Rectangle Width="100" Height="50" 
               Fill="Blue" Stroke="Black"/>
    <Ellipse Width="80" Height="80" 
             Fill="Red"/>
</Canvas>
```

---

### Тема 6.2: Transform

**Изучите:**
- RotateTransform — вращение
- ScaleTransform — масштабирование
- TranslateTransform — перемещение
- TransformGroup — комбинация трансформаций

**Пример:**
```xml
<Rectangle Width="100" Height="50">
    <Rectangle.RenderTransform>
        <TransformGroup>
            <RotateTransform Angle="45"/>
            <ScaleTransform ScaleX="1.5"/>
            <TranslateTransform X="50" Y="50"/>
        </TransformGroup>
    </Rectangle.RenderTransform>
</Rectangle>
```

---

### Тема 6.3: Geometry и Path

**Изучите:**
- PathGeometry, CombinedGeometry, StreamGeometry
- Path-язык (M, L, C, A команды)
- GeometryGroup для сложных фигур

**Пример:**
```xml
<Path Data="M 10,100 L 100,100 100,200 Z" 
      Fill="Blue" Stroke="Black"/>
```

---

### Тема 6.4: Visual Layer

**Изучите:**
- DrawingVisual для низкоуровневого рендеринга
- DrawingContext для рисования
- VisualCollection для управления визуалами

---

### Тема 6.5: OnRender

**Изучите:**
- Переопределение OnRender в UIElement
- DrawingContext команды
- Производительность кастомного рендеринга

---

## Практическая работа

**Задание:** Создание графического редактора

**Время:** 4 часа

**Требования:**
1. Canvas для рисования
2. Shapes (Line, Rectangle, Ellipse)
3. Transform для манипуляции
4. Path для сложных фигур
5. OnRender для сетки

**Критерии оценки:** 100 баллов

---

## Контрольный список

Перед переходом к Модулю 7 убедитесь, что вы:

- [ ] Создали все базовые Shapes
- [ ] Использовали TransformGroup
- [ ] Нарисовали Path с использованием Path-языка
- [ ] Создали DrawingVisual для рендеринга
- [ ] Переопределили OnRender для кастомного элемента
- [ ] Завершили практическую работу (≥80 баллов)

---

## Глоссарий модуля

| Термин | Определение |
|--------|-------------|
| **Shape** | Базовый класс для графических примитивов |
| **Geometry** | Описание формы без визуализации |
| **Path** | Контрол для отображения Geometry |
| **Transform** | Преобразование координат (вращение, масштаб, сдвиг) |
| **DrawingVisual** | Низкоуровневый визуал для рендеринга |
| **DrawingContext** | Контекст для рисования в Visual Layer |
| **OnRender** | Метод для кастомного рендеринга |
| **RenderTransform** | Трансформация при рендеринге |
| **LayoutTransform** | Трансформация до layout pass |

---

## Переход к следующему модулю

➡️ **[Модуль 7: Анимации и Storyboards](../Module_07_Animations/README.md)**

В Модуле 7 изучим:
- Timeline-анимации (DoubleAnimation, ColorAnimation)
- Storyboard и ControlTemplate анимации
- Easing Functions
- Анимация в коде vs XAML
