# Тема 3.2: Grid — универсальный контейнер

### Теория

**Grid** — самый мощный layout-контейнер в WPF. Размещает элементы в строках и столбцах.

#### GridUnitType

| Тип | Синтаксис | Описание | Пример |
|-----|-----------|----------|--------|
| **Auto** | `Auto` | По размеру содержимого | `Height="Auto"` |
| **Pixel** | Число | Фиксированный размер | `Width="200"` |
| **Star** | `*`, `2*`, `3*` | Пропорционально остатку | `Width="*"` |

#### Вложенность

Grid может содержать вложенные Grid для сложных layout:

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    
    <!-- Вложенный Grid -->
    <Grid Grid.Row="1">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="200"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
    </Grid>
</Grid>
```

### Примеры кода

#### Пример 1: Базовый Grid

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>     <!-- Меню -->
        <RowDefinition Height="*"/>        <!-- Контент -->
        <RowDefinition Height="Auto"/>     <!-- StatusBar -->
    </Grid.RowDefinitions>
    
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200"/>    <!-- Левая панель -->
        <ColumnDefinition Width="*"/>      <!-- Центр -->
        <ColumnDefinition Width="250"/>    <!-- Правая панель -->
    </Grid.ColumnDefinitions>
    
    <!-- Элементы -->
    <Menu Grid.Row="0" Grid.ColumnSpan="3">
        <MenuItem Header="Файл"/>
        <MenuItem Header="Правка"/>
    </Menu>
    
    <ListBox Grid.Row="1" Grid.Column="0" Margin="10">
        <ListBoxItem Content="Элемент 1"/>
        <ListBoxItem Content="Элемент 2"/>
    </ListBox>
    
    <TabControl Grid.Row="1" Grid.Column="1" Margin="10">
        <TabItem Header="Вкладка 1"/>
        <TabItem Header="Вкладка 2"/>
    </TabControl>
    
    <Grid Grid.Row="1" Grid.Column="2" Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <TextBlock Grid.Row="0" Text="Свойства:"/>
        <TextBox Grid.Row="1"/>
    </Grid>
    
    <StatusBar Grid.Row="2" Grid.ColumnSpan="3">
        <StatusBarItem>
            <TextBlock Text="Готов"/>
        </StatusBarItem>
    </StatusBar>
</Grid>
```

#### Пример 2: Star Sizing

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <!-- 1 часть -->
        <ColumnDefinition Width="*"/>
        <!-- 2 части (в 2 раза шире) -->
        <ColumnDefinition Width="2*"/>
        <!-- 3 части (в 3 раза шире) -->
        <ColumnDefinition Width="3*"/>
    </Grid.ColumnDefinitions>
    
    <Border Grid.Column="0" Background="LightBlue">
        <TextBlock Text="1*"/>
    </Border>
    <Border Grid.Column="1" Background="LightGreen">
        <TextBlock Text="2*"/>
    </Border>
    <Border Grid.Column="2" Background="LightYellow">
        <TextBlock Text="3*"/>
    </Border>
</Grid>

<!-- Результат: 1:2:3 = 16.7% : 33.3% : 50% -->
```

#### Пример 3: GridSplitter

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200" MinWidth="150" MaxWidth="400"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="250" MinWidth="200" MaxWidth="500"/>
    </Grid.ColumnDefinitions>
    
    <!-- Левая панель -->
    <Border Grid.Column="0" Background="LightGray">
        <TextBlock Text="Левая панель"/>
    </Border>
    
    <!-- GridSplitter вертикальный -->
    <GridSplitter Grid.Column="1" 
                  Width="5" 
                  HorizontalAlignment="Center"
                  VerticalAlignment="Stretch"
                  Background="Gray"/>
    
    <!-- Центральная область -->
    <Border Grid.Column="2" Background="White">
        <TextBlock Text="Центральная область"/>
    </Border>
    
    <!-- GridSplitter вертикальный -->
    <GridSplitter Grid.Column="3" 
                  Width="5" 
                  HorizontalAlignment="Center"
                  VerticalAlignment="Stretch"
                  Background="Gray"/>
    
    <!-- Правая панель -->
    <Border Grid.Column="4" Background="LightGray">
        <TextBlock Text="Правая панель"/>
    </Border>
</Grid>
```

#### Пример 4: GridSplitter горизонтальный

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" MinHeight="30"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*" MinHeight="100"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto" MinHeight="25"/>
    </Grid.RowDefinitions>
    
    <!-- Menu -->
    <Menu Grid.Row="0">
        <MenuItem Header="Файл"/>
    </Menu>
    
    <!-- GridSplitter -->
    <GridSplitter Grid.Row="1" 
                  Height="5" 
                  HorizontalAlignment="Stretch"
                  VerticalAlignment="Center"
                  Background="Gray"/>
    
    <!-- Content -->
    <Grid Grid.Row="2">
        <TextBlock Text="Content Area"/>
    </Grid>
    
    <!-- GridSplitter -->
    <GridSplitter Grid.Row="3" 
                  Height="5" 
                  HorizontalAlignment="Stretch"
                  VerticalAlignment="Center"
                  Background="Gray"/>
    
    <!-- StatusBar -->
    <StatusBar Grid.Row="4">
        <TextBlock Text="Status"/>
    </StatusBar>
</Grid>
```

#### Пример 5: SharedSizeGroup

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <!-- Grid.IsSharedSizeScope="True" на родителе -->
    <Grid Grid.IsSharedSizeScope="True">
        <!-- Row 1 -->
        <TextBlock Grid.Row="0" Grid.Column="0" Text="Имя:" Margin="5"/>
        <TextBox Grid.Row="0" Grid.Column="1" Margin="5"/>
        
        <!-- Row 2 -->
        <TextBlock Grid.Row="1" Grid.Column="0" Text="Фамилия:" Margin="5"/>
        <TextBox Grid.Row="1" Grid.Column="1" Margin="5"/>
        
        <!-- Row 3 -->
        <TextBlock Grid.Row="2" Grid.Column="0" Text="Email:" Margin="5"/>
        <TextBox Grid.Row="2" Grid.Column="1" Margin="5"/>
    </Grid>
</Grid>

<!-- Все колонки "Label" будут одинаковой ширины -->
```

#### Пример 6: Реальный Grid из DotElectric (MainWindow)

```xml
<!-- MainWindow.xaml из DotElectric -->
<Grid>
    <!-- Row Definitions -->
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>     <!-- Menu -->
        <RowDefinition Height="Auto"/>     <!-- ToolBar -->
        <RowDefinition Height="*"/>        <!-- Основная область -->
        <RowDefinition Height="Auto"/>     <!-- StatusBar -->
    </Grid.RowDefinitions>

    <!-- Column Definitions -->
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200" MinWidth="150" MaxWidth="400"/>  <!-- Левая панель -->
        <ColumnDefinition Width="Auto"/>     <!-- GridSplitter -->
        <ColumnDefinition Width="*"/>        <!-- Центр (TabControl) -->
        <ColumnDefinition Width="Auto"/>     <!-- GridSplitter -->
        <ColumnDefinition Width="250" MinWidth="200" MaxWidth="500"/> <!-- Правая панель -->
    </Grid.ColumnDefinitions>

    <!-- Menu -->
    <Menu Grid.Row="0" Grid.ColumnSpan="5"
          Background="{DynamicResource MenuBackgroundBrush}">
        <MenuItem Header="_Файл">
            <MenuItem Header="_Новый" Command="{Binding NewTabCommand}"/>
            <MenuItem Header="_Открыть" Command="{Binding OpenFileCommand}"/>
            <Separator/>
            <MenuItem Header="_Сохранить" Command="{Binding SaveCommand}"/>
        </MenuItem>
        <MenuItem Header="_Правка"/>
        <MenuItem Header="_Вид"/>
        <MenuItem Header="_Справка"/>
    </Menu>

    <!-- ToolBar -->
    <Border Grid.Row="1" Grid.ColumnSpan="5"
            Background="{DynamicResource ToolBarBackgroundBrush}">
        <WrapPanel Margin="4,2">
            <Button Command="{Binding NewTabCommand}">
                <Image Source="/icons/new.png"/>
            </Button>
            <Separator/>
            <Button Command="{Binding UndoCommand}">
                <Image Source="/icons/undo.png"/>
            </Button>
        </WrapPanel>
    </Border>

    <!-- Левая панель -->
    <Border Grid.Row="2" Grid.Column="0"
            Background="{DynamicResource PanelBackgroundBrush}">
        <Grid>
            <TextBlock Text="Библиотека шаблонов" 
                       HorizontalAlignment="Center"
                       VerticalAlignment="Center"/>
        </Grid>
    </Border>

    <!-- GridSplitter 1 -->
    <GridSplitter Grid.Row="2" Grid.Column="1"
                  Width="5"
                  HorizontalAlignment="Center"
                  VerticalAlignment="Stretch"
                  Background="{DynamicResource BorderBrush}"/>

    <!-- Центральная область -->
    <TabControl Grid.Row="2" Grid.Column="2"
                ItemsSource="{Binding OpenedTabs}"
                SelectedItem="{Binding SelectedTab}">
        <TabControl.ItemTemplate>
            <DataTemplate>
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="{Binding Title}"/>
                    <Button Content="✕" Command="{Binding CloseTabCommand}"/>
                </StackPanel>
            </DataTemplate>
        </TabControl.ItemTemplate>
    </TabControl>

    <!-- GridSplitter 2 -->
    <GridSplitter Grid.Row="2" Grid.Column="3"
                  Width="5"
                  HorizontalAlignment="Center"
                  VerticalAlignment="Stretch"
                  Background="{DynamicResource BorderBrush}"/>

    <!-- Правая панель -->
    <Border Grid.Row="2" Grid.Column="4"
            Background="{DynamicResource PanelBackgroundBrush}">
        <Grid>
            <TextBlock Text="Свойства" 
                       HorizontalAlignment="Center"
                       VerticalAlignment="Center"/>
        </Grid>
    </Border>

    <!-- StatusBar -->
    <StatusBar Grid.Row="3" Grid.ColumnSpan="5">
        <StatusBarItem>
            <TextBlock Text="{Binding StatusMessage}"/>
        </StatusBarItem>
        <Separator/>
        <StatusBarItem>
            <TextBlock Text="{Binding SheetFormat}"/>
        </StatusBarItem>
    </StatusBar>
</Grid>
```

#### Пример 7: ShowGridLines для отладки

```xml
<Grid ShowGridLines="True">
    <Grid.RowDefinitions>
        <RowDefinition Height="*"/>
        <RowDefinition Height="*"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Линии сетки видны для отладки layout -->
    
    <Button Grid.Row="0" Grid.Column="0" Content="1,0"/>
    <Button Grid.Row="0" Grid.Column="1" Content="1,1"/>
    <Button Grid.Row="1" Grid.Column="1" Content="2,1"/>
</Grid>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 3.2.1: Простой Grid**

Создайте Grid с:
- 3 строками (Auto, *, Auto)
- 2 колонками (200px, *)
- Menu в первой строке
- ListBox во второй строке, первой колонке
- ContentControl во второй строке, второй колонке
- StatusBar в третьей строке

**Задача 3.2.2: Star Sizing**

Создайте Grid с 4 колонками:
- Первая: 100px
- Вторая: *
- Третья: 2*
- Четвёртая: Auto (по кнопке)

Разместите кнопки в каждой колонке. Изменяйте размер окна и наблюдайте за поведением.

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 3.2.3: Grid с GridSplitter**

Создайте окно с resizable панелями:
- Левая панель: 200px (MinWidth=150, MaxWidth=400)
- GridSplitter: 5px
- Центральная область: *
- GridSplitter: 5px
- Правая панель: 250px (MinWidth=200, MaxWidth=500)

**Задача 3.2.4: Форма с SharedSizeGroup**

Создайте форму регистрации:
- 5 полей (Имя, Фамилия, Email, Телефон, Пароль)
- Labels в одной колонке с SharedSizeGroup
- TextBox в другой колонке
- Все labels должны быть одинаковой ширины

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 3.2.5: Complex Dashboard Layout**

Создайте dashboard с complex layout:
```
┌─────────────────────────────────────────────┐
│                 Menu (Auto)                 │
├─────────────────────────────────────────────┤
│                ToolBar (Auto)               │
├──────────┬──────────────────────┬───────────┤
│          │                      │           │
│  Tree    │     Main Content     │ Properties│
│  (200)   │         (*)          │   (250)   │
│          │                      │           │
│          │  ┌──────┬──────────┐ │           │
│          │  │      │          │ │           │
│          │  │ Left │   Right  │ │           │
│          │  │ (*)  │    (*)   │ │           │
│          │  │      │          │ │           │
│          │  └──────┴──────────┘ │           │
│          │                      │           │
├──────────┴──────────────────────┴───────────┤
│              StatusBar (Auto)               │
└─────────────────────────────────────────────┘
```

Все панели resizable через GridSplitter.

**Задача 3.2.6: Adaptive Grid Layout**

Создайте адаптивный layout:
- Narrow (<600px): 1 колонка, всё вертикально
- Medium (600-1000px): 2 колонки (Sidebar + Content)
- Wide (>1000px): 3 колонки (Sidebar + Content + Properties)

Используйте MultiBinding с конвертером для переключения видимости.

---

### Решения

<details>
<summary>✅ Решение задачи 3.2.1</summary>

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Menu -->
    <Menu Grid.Row="0" Grid.ColumnSpan="2">
        <MenuItem Header="Файл"/>
        <MenuItem Header="Правка"/>
        <MenuItem Header="Справка"/>
    </Menu>
    
    <!-- ListBox -->
    <ListBox Grid.Row="1" Grid.Column="0" Margin="10">
        <ListBoxItem Content="Item 1"/>
        <ListBoxItem Content="Item 2"/>
        <ListBoxItem Content="Item 3"/>
    </ListBox>
    
    <!-- Content -->
    <ContentControl Grid.Row="1" Grid.Column="1" Margin="10">
        <TextBlock Text="Main Content"/>
    </ContentControl>
    
    <!-- StatusBar -->
    <StatusBar Grid.Row="2" Grid.ColumnSpan="2">
        <StatusBarItem>
            <TextBlock Text="Ready"/>
        </StatusBarItem>
    </StatusBar>
</Grid>
```
</details>

<details>
<summary>✅ Решение задачи 3.2.2</summary>

```xml
<Grid ShowGridLines="True">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="100"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="2*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    
    <Button Grid.Column="0" Content="100px"/>
    <Button Grid.Column="1" Content="* (Star)"/>
    <Button Grid.Column="2" Content="2* (Double Star)"/>
    <Button Grid.Column="3" Content="Auto"/>
</Grid>
```
</details>

<details>
<summary>✅ Решение задачи 3.2.3</summary>

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200" MinWidth="150" MaxWidth="400"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="250" MinWidth="200" MaxWidth="500"/>
    </Grid.ColumnDefinitions>
    
    <Border Grid.Column="0" Background="LightBlue">
        <TextBlock Text="Left Panel" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </Border>
    
    <GridSplitter Grid.Column="1" Width="5" 
                  HorizontalAlignment="Center"
                  VerticalAlignment="Stretch"
                  Background="Gray"/>
    
    <Border Grid.Column="2" Background="White">
        <TextBlock Text="Center Content" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </Border>
    
    <GridSplitter Grid.Column="3" Width="5" 
                  HorizontalAlignment="Center"
                  VerticalAlignment="Stretch"
                  Background="Gray"/>
    
    <Border Grid.Column="4" Background="LightGreen">
        <TextBlock Text="Right Panel" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </Border>
</Grid>
```
</details>

<details>
<summary>✅ Решение задачи 3.2.4</summary>

```xml
<Grid Grid.IsSharedSizeScope="True" Margin="20">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" SharedSizeGroup="Label"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <TextBlock Grid.Row="0" Grid.Column="0" Text="Имя:" Margin="5"/>
    <TextBox Grid.Row="0" Grid.Column="1" Margin="5"/>
    
    <TextBlock Grid.Row="1" Grid.Column="0" Text="Фамилия:" Margin="5"/>
    <TextBox Grid.Row="1" Grid.Column="1" Margin="5"/>
    
    <TextBlock Grid.Row="2" Grid.Column="0" Text="Email:" Margin="5"/>
    <TextBox Grid.Row="2" Grid.Column="1" Margin="5"/>
    
    <TextBlock Grid.Row="3" Grid.Column="0" Text="Телефон:" Margin="5"/>
    <TextBox Grid.Row="3" Grid.Column="1" Margin="5"/>
    
    <TextBlock Grid.Row="4" Grid.Column="0" Text="Пароль:" Margin="5"/>
    <PasswordBox Grid.Row="4" Grid.Column="1" Margin="5"/>
</Grid>
```
</details>

---

## Ключевые выводы

✅ **Grid** — самый мощный layout контейнер  
✅ **GridUnitType:** Auto (по содержимому), Pixel (фикс), Star (пропорция)  
✅ **GridSplitter** позволяет изменять размеры строк/колонок  
✅ **MinWidth/MaxWidth** ограничивают изменение размеров  
✅ **SharedSizeGroup** синхронизирует размеры колонок/строк  
✅ **ShowGridLines="True"** для отладки layout  
✅ **Grid.IsSharedSizeScope="True"** на родителе для shared size

---

## Дополнительные ресурсы

- [Grid Overview](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.grid)
- [GridSplitter](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.gridsplitter)
- [Panel Comparison](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/fundamentals/panels-overview)
