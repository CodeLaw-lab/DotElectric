# Глоссарий терминов WPF

## A

**Attached Property** — Свойство, определённое одним классом, но используемое на другом элементе. Пример: `Grid.Row`, `Canvas.Left`.

**Auto (GridUnitType)** — Тип размера строки/колонки Grid, означающий «по размеру содержимого».

## B

**Binding** — Механизм связывания свойства UI со свойством источника данных. Поддерживает режимы: OneTime, OneWay, TwoWay, OneWayToSource.

**Bubbling Event** — Событие, которое маршрутизируется от источника к корню дерева элементов. Пример: `MouseDown`, `Click`.

**BasedOn** — Атрибут Style для наследования свойств из другого стиля.

## C

**ContentProperty** — Свойство элемента, которое может быть указано неявно (без имени). Пример: `Button.Content`.

**ControlTemplate** — Шаблон, определяющий визуальную структуру и поведение контрола.

**Converter** — Класс, реализующий `IValueConverter` или `IMultiValueConverter` для преобразования значений binding.

**Custom Panel** — Пользовательская панель с кастомной логикой layout через переопределение `MeasureOverride` и `ArrangeOverride`.

## D

**Data Binding** — Процесс связывания свойств UI с данными (ViewModel).

**DataTemplate** — Шаблон, определяющий визуальное представление данных.

**Dependency Property (DP)** — Специальное свойство WPF, поддерживающее binding, styling, animation, inheritance и default values.

**DataTrigger** — Trigger, который активируется при определённом значении binding.

**DockPanel** — Панель, размещающая элементы по краям (Top, Bottom, Left, Right, Fill).

**DynamicResource** — Markup extension для получения ресурса в runtime с поддержкой изменений.

## E

**EventTrigger** — Trigger, активируемый событием (например, `Loaded`, `MouseEnter`).

## G

**Grid** — Универсальная панель с строками и колонками. Поддерживает `Auto`, `Pixel`, `Star` sizing.

**GridSplitter** — Контрол для изменения размеров строк/колонок Grid пользователем.

## I

**INotifyPropertyChanged** — Интерфейс для уведомления UI об изменении свойств ViewModel.

**Implicit Style** — Style без `x:Key`, применяемый ко всем элементам указанного типа.

**ItemsPanelTemplate** — Шаблон, определяющий панель для ItemsControl (ListBox, ComboBox).

## L

**Layout** — Процесс измерения и расположения элементов UI. Состоит из двух проходов: Measure и Arrange.

**Logical Tree** — Иерархия элементов, объявленная в XAML.

## M

**Markup Extension** — Механизм XAML для вычисления значений в runtime. Синтаксис: `{Extension Параметры}`.

**Measure Pass** — Первый проход layout: элементы сообщают желаемый размер.

**MultiBinding** — Binding, объединяющий несколько источников в одно значение.

**MultiTrigger** — Trigger с несколькими условиями Property.

**MultiDataTrigger** — Trigger с несколькими условиями DataTrigger.

## O

**ObservableCollection** — Коллекция, уведомляющая UI об изменениях (добавление, удаление, перемещение элементов).

## P

**Panel** — Базовый класс для всех layout-контейнеров (Grid, StackPanel, Canvas, etc.).

**PriorityBinding** — Binding с приоритетами: используется первое успешное значение.

## R

**Routed Event** — Событие, которое маршрутизируется через дерево элементов (Bubbling, Tunneling, Direct).

**ResourceDictionary** — Коллекция ресурсов (кисти, стили, шаблоны) с поддержкой слияния (MergedDictionaries).

**RelativeSource** — Способ указания источника binding относительно текущего элемента (AncestorType, Self, TemplatedParent).

## S

**Star Sizing** — Пропорциональное распределение места в Grid (`*`, `2*`, `3*`).

**StaticResource** — Markup extension для получения ресурса один раз при загрузке.

**Style** — Набор настроек свойств (Setter) для переиспользования.

**StackPanel** — Панель, размещающая элементы в стопку (вертикально или горизонтально).

**SharedSizeGroup** — Имя группы колонок/строк Grid для синхронизации размеров.

## T

**TemplateBinding** — Binding внутри ControlTemplate для передачи свойств контрола в шаблон.

**Trigger** — Изменение свойств при выполнении условия. Типы: PropertyTrigger, DataTrigger, MultiTrigger, EventTrigger.

**Tunneling Event** — Событие, которое маршрутизируется от корня к источнику (Preview-события).

**Type Converter** — Класс для преобразования строки в нужный тип (Color, Thickness, CornerRadius).

## V

**Visual Tree** — Детализированная структура всех визуальных элементов, включая части шаблонов.

**Validation** — Проверка данных binding. Реализуется через `IDataErrorInfo` или `INotifyDataErrorInfo`.

**ViewBox** — Контрол для масштабирования содержимого.

## W

**WrapPanel** — Панель, размещающая элементы с переносом на новую строку/колонку.

**WeakReferenceMessenger** — Паттерн для обмена сообщениями между ViewModel без циклических зависимостей.

---

## Сокращения

| Сокращение | Расшифровка |
|------------|-------------|
| **DP** | Dependency Property |
| **DP** | Dependency Property |
| **MVVM** | Model-View-ViewModel |
| **INPC** | INotifyPropertyChanged |
| **OD** | ObservableCollection |
| **ME** | Markup Extension |
| **DR** | DynamicResource |
| **SR** | StaticResource |
| **CT** | ControlTemplate |
| **DT** | DataTemplate |

---

## Common Patterns

| Паттерн | Описание |
|---------|----------|
| **MVVM** | Разделение на Model, View, ViewModel |
| **Command** | Инкапсуляция действия (ICommand) |
| **Messenger** | Обмен сообщениями между VM |
| **Factory** | Создание объектов через фабрику |
| **Singleton** | Один экземпляр на приложение |
| **Repository** | Абстракция доступа к данным |
| **Service** | Бизнес-логика вне UI |

---

## Best Practices

✅ **DP** для свойств контролов  
✅ **INotifyPropertyChanged** для ViewModel  
✅ **ObservableCollection** для коллекций  
✅ **DynamicResource** для тем  
✅ **StaticResource** для статичных ресурсов  
✅ **OneWay binding** где возможно  
✅ **Commands** вместо event handlers  
✅ **DataTemplate** для визуализации данных  
✅ **ControlTemplate** для кастомизации контролов  
✅ **Converters** для логики отображения  

❌ **Code-behind** для бизнес-логики  
❌ **TwoWay binding** без необходимости  
❌ **Event handlers** вместо Commands  
❌ **DynamicResource** без необходимости  
❌ **Direct DB access** из ViewModel  
