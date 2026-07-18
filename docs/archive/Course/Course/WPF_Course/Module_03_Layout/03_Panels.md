# Тема 3.3: StackPanel, WrapPanel, DockPanel

### Теория

Три простых, но мощных панели для различных сценариев layout.

#### StackPanel

**Назначение:** Элементы в стопку (вертикально или горизонтально).

```
Вертикально:          Горизонтально:
┌──────────┐          ┌──────────────────────┐
│ Button 1 │          │ Btn1 │ Btn2 │ Btn3  │
├──────────┤          └──────────────────────┘
│ Button 2 │
├──────────┤
│ Button 3 │
└──────────┘
```

#### WrapPanel

**Назначение:** Элементы с переносом на новую строку/колонку.

```
Горизонтально (с переносом):
┌────────────────────────────┐
│ Btn1 │ Btn2 │ Btn3 │      │
│ Btn4 │ Btn5 │              │
│ Btn6 │                      │
└────────────────────────────┘
```

#### DockPanel

**Назначение:** Прикрепление элементов к краям.

```
┌────────────────────────────┐
│         Top (1)            │
├────────┬───────────┬───────┤
│ Left   │  Fill     │ Right │
│ (2)    │  (5)      │ (3)   │
├────────┴───────────┴───────┤
│        Bottom (4)          │
└────────────────────────────┘
```

### Примеры кода

#### Пример 1: StackPanel — базовое использование

```xml
<!-- Вертикальная StackPanel -->
<StackPanel Margin="10">
    <TextBlock Text="Заголовок" FontSize="20" FontWeight="Bold"/>
    <TextBlock Text="Описание" Margin="0,10,0,0"/>
    <TextBox Margin="0,10,0,0" Height="100"/>
    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,10,0,0">
        <Button Content="OK" Padding="20,5" Margin="0,0,10,0"/>
        <Button Content="Cancel" Padding="20,5"/>
    </StackPanel>
</StackPanel>
```

#### Пример 2: StackPanel — Orientation и FlowDirection

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Вертикальная -->
    <StackPanel Grid.Column="0" Margin="5">
        <Button Content="1"/>
        <Button Content="2"/>
        <Button Content="3"/>
    </StackPanel>
    
    <!-- Горизонтальная -->
    <StackPanel Grid.Column="1" Orientation="Horizontal" Margin="5">
        <Button Content="1"/>
        <Button Content="2"/>
        <Button Content="3"/>
    </StackPanel>
</Grid>
```

#### Пример 3: WrapPanel — адаптивный layout

```xml
<WrapPanel Margin="10">
    <Button Content="Button 1" Margin="5" Padding="10,5"/>
    <Button Content="Button 2" Margin="5" Padding="10,5"/>
    <Button Content="Button 3" Margin="5" Padding="10,5"/>
    <Button Content="Button 4" Margin="5" Padding="10,5"/>
    <Button Content="Button 5" Margin="5" Padding="10,5"/>
    <Button Content="Button 6" Margin="5" Padding="10,5"/>
    <!-- Кнопки автоматически переносятся -->
</WrapPanel>
```

#### Пример 4: WrapPanel — ItemWidth и ItemHeight

```xml
<WrapPanel ItemWidth="100" ItemHeight="50" Margin="10">
    <Button Content="1" Background="LightBlue"/>
    <Button Content="2" Background="LightGreen"/>
    <Button Content="3" Background="LightYellow"/>
    <Button Content="4" Background="LightCoral"/>
    <!-- Все кнопки одинакового размера 100x50 -->
</WrapPanel>
```

#### Пример 5: WrapPanel — вертикальный с переносом

```xml
<WrapPanel Orientation="Vertical" 
           ItemWidth="150" 
           ItemHeight="100"
           Margin="10">
    <Border Background="LightBlue" BorderBrush="Blue" BorderThickness="1">
        <TextBlock Text="Item 1" Margin="10"/>
    </Border>
    <Border Background="LightGreen" BorderBrush="Green" BorderThickness="1">
        <TextBlock Text="Item 2" Margin="10"/>
    </Border>
    <!-- Перенос в новую колонку -->
</WrapPanel>
```

#### Пример 6: DockPanel — классический layout окна

```xml
<DockPanel>
    <!-- Menu сверху -->
    <Menu DockPanel.Dock="Top">
        <MenuItem Header="_Файл"/>
        <MenuItem Header="_Правка"/>
        <MenuItem Header="_Справка"/>
    </Menu>
    
    <!-- ToolBar под menu -->
    <ToolBarTray DockPanel.Dock="Top" Background="LightGray">
        <ToolBar>
            <Button Content="New"/>
            <Button Content="Open"/>
            <Button Content="Save"/>
        </ToolBar>
    </ToolBarTray>
    
    <!-- StatusBar снизу -->
    <StatusBar DockPanel.Dock="Bottom" Background="LightGray">
        <StatusBarItem>
            <TextBlock Text="Ready"/>
        </StatusBarItem>
    </StatusBar>
    
    <!-- Левая панель -->
    <Border DockPanel.Dock="Left" 
            Width="200" 
            Background="LightBlue"
            BorderBrush="Gray" 
            BorderThickness="0,0,1,0">
        <TextBlock Text="Left Panel" Margin="10"/>
    </Border>
    
    <!-- Правая панель -->
    <Border DockPanel.Dock="Right" 
            Width="250" 
            Background="LightGreen"
            BorderBrush="Gray" 
            BorderThickness="1,0,0,0">
        <TextBlock Text="Right Panel" Margin="10"/>
    </Border>
    
    <!-- Центральная область (заполняет剩余) -->
    <Grid Background="White">
        <TextBlock Text="Main Content" 
                   HorizontalAlignment="Center" 
                   VerticalAlignment="Center"/>
    </Grid>
</DockPanel>
```

#### Пример 7: DockPanel — LastChildFill

```xml
<!-- LastChildFill=True (по умолчанию) -->
<DockPanel LastChildFill="True">
    <Button Content="Left" DockPanel.Dock="Left" Width="100"/>
    <Button Content="Top" DockPanel.Dock="Top" Height="50"/>
    <!-- Последний элемент заполняет剩余 -->
    <Button Content="Fill"/>
</DockPanel>

<!-- LastChildFill=False -->
<DockPanel LastChildFill="False">
    <Button Content="Left" DockPanel.Dock="Left" Width="100"/>
    <Button Content="Top" DockPanel.Dock="Top" Height="50"/>
    <Button Content="Bottom" DockPanel.Dock="Bottom" Height="50"/>
    <!-- Все элементы dock, последний не заполняет -->
</DockPanel>
```

#### Пример 8: Комбинирование панелей

```xml
<DockPanel>
    <!-- Верхняя панель -->
    <StackPanel DockPanel.Dock="Top" 
                Orientation="Horizontal" 
                Margin="10">
        <TextBlock Text="Заголовок:" VerticalAlignment="Center"/>
        <TextBox Width="200" Margin="10,0,0,0"/>
        <Button Content="Search" Margin="10,0,0,0"/>
    </StackPanel>
    
    <!-- Нижняя панель с кнопками -->
    <WrapPanel DockPanel.Dock="Bottom" 
               Margin="10" 
               HorizontalAlignment="Right">
        <Button Content="OK" Padding="20,5" Margin="5"/>
        <Button Content="Cancel" Padding="20,5" Margin="5"/>
        <Button Content="Apply" Padding="20,5" Margin="5"/>
    </WrapPanel>
    
    <!-- Центральная область -->
    <Grid Margin="10">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        
        <!-- Левая часть -->
        <StackPanel Grid.Column="0" Margin="5">
            <TextBlock Text="Левая колонка" FontWeight="Bold"/>
            <ListBox Height="200" Margin="0,10,0,0"/>
        </StackPanel>
        
        <!-- Правая часть -->
        <StackPanel Grid.Column="1" Margin="5">
            <TextBlock Text="Правая колонка" FontWeight="Bold"/>
            <TextBox Height="200" Margin="0,10,0,0" 
                     TextWrapping="Wrap"
                     AcceptsReturn="True"/>
        </StackPanel>
    </Grid>
</DockPanel>
```

#### Пример 9: Реальное использование из DotElectric

```xml
<!-- MainWindow.xaml — ToolBar区域 -->
<Border Grid.Row="1" Grid.ColumnSpan="5"
        Background="{DynamicResource ToolBarBackgroundBrush}"
        BorderBrush="{DynamicResource BorderBrush}"
        BorderThickness="0,0,0,1">
    <WrapPanel Margin="4,2" Orientation="Horizontal">
        <!-- Файл -->
        <Button Style="{DynamicResource ToolBarButtonStyle}"
                Command="{Binding NewTabWithLastFormatCommand}"
                ToolTip="Новый шаблон (Ctrl+N)">
            <materialDesign:PackIcon Kind="FilePlusOutline" Width="20" Height="20"/>
        </Button>
        <Button Style="{DynamicResource ToolBarButtonStyle}"
                Command="{Binding OpenFileCommand}"
                ToolTip="Открыть (Ctrl+O)">
            <materialDesign:PackIcon Kind="FolderOpenOutline" Width="20" Height="20"/>
        </Button>
        <Separator Style="{StaticResource ToolBarSeparatorStyle}"/>
        
        <!-- Правка -->
        <Button Style="{DynamicResource ToolBarButtonStyle}"
                Command="{Binding SelectedTab.UndoCommand}"
                ToolTip="Отменить (Ctrl+Z)">
            <materialDesign:PackIcon Kind="Undo" Width="20" Height="20"/>
        </Button>
        
        <!-- Separator -->
        <Separator Style="{StaticResource ToolBarSeparatorStyle}"/>
        
        <!-- Масштаб -->
        <ComboBox Width="90" Height="30" Margin="4,0"
                  VerticalAlignment="Center">
            <ComboBoxItem Content="10%"/>
            <ComboBoxItem Content="25%"/>
            <ComboBoxItem Content="50%"/>
            <ComboBoxItem Content="100%"/>
        </ComboBox>
    </WrapPanel>
</Border>
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 3.3.1: Вертикальная форма**

Создайте форму входа с StackPanel:
- Заголовок "Вход"
- TextBox для логина
- PasswordBox для пароля
- Две кнопки (OK, Cancel) горизонтально

**Задача 3.3.2: WrapPanel с кнопками**

Создайте панель с 10 кнопками:
- WrapPanel с кнопками "Button 1" — "Button 10"
- Кнопки должны переноситься на новую строку
- Margin между кнопками 5px

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 3.3.3: DockPanel layout окна**

Создайте главное окно приложения:
- Menu сверху (Файл, Правка, Вид, Справка)
- ToolBar под menu (3-5 кнопок)
- Left панель (TreeView, 200px)
- Right панель (Properties, 250px)
- StatusBar снизу
- Центральная область (ContentControl)

**Задача 3.3.4: Адаптивная галерея**

Создайте галерею изображений:
- WrapPanel с ItemWidth=150, ItemHeight=150
- Border с изображением и подписью
- При изменении размера окна — перенос элементов

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 3.3.5: Комбинированный layout**

Создайте сложный layout с комбинацией:
- DockPanel для основной структуры
- StackPanel для форм
- WrapPanel для кнопок и тегов
- Grid для вложенных структур

**Задача 3.3.6: Dynamic Panel Selector**

Реализуйте панель с выбором типа layout:
- ComboBox для выбора (Stack, Wrap, Dock)
- При изменении — меняется панель
- Один и тот же контент в разных панелях

---

### Решения

<details>
<summary>✅ Решение задачи 3.3.1</summary>

```xml
<StackPanel Width="300" Margin="20">
    <TextBlock Text="Вход" 
               FontSize="24" 
               FontWeight="Bold"
               HorizontalAlignment="Center"
               Margin="0,0,0,20"/>
    
    <TextBlock Text="Логин:"/>
    <TextBox Margin="0,5,0,15" Padding="8,4"/>
    
    <TextBlock Text="Пароль:"/>
    <PasswordBox Margin="0,5,0,15" Padding="8,4"/>
    
    <StackPanel Orientation="Horizontal" 
                HorizontalAlignment="Center">
        <Button Content="OK" 
                Padding="20,5" 
                Margin="0,0,10,0"/>
        <Button Content="Cancel" 
                Padding="20,5"/>
    </StackPanel>
</StackPanel>
```
</details>

<details>
<summary>✅ Решение задачи 3.3.3</summary>

```xml
<DockPanel>
    <!-- Menu -->
    <Menu DockPanel.Dock="Top">
        <MenuItem Header="_Файл">
            <MenuItem Header="_Новый"/>
            <MenuItem Header="_Открыть"/>
            <MenuItem Header="_Сохранить"/>
        </MenuItem>
        <MenuItem Header="_Правка"/>
        <MenuItem Header="_Вид"/>
        <MenuItem Header="_Справка"/>
    </Menu>
    
    <!-- ToolBar -->
    <ToolBarTray DockPanel.Dock="Top" Background="LightGray">
        <ToolBar>
            <Button Content="New"/>
            <Button Content="Open"/>
            <Button Content="Save"/>
            <Separator/>
            <Button Content="Undo"/>
            <Button Content="Redo"/>
        </ToolBar>
    </ToolBarTray>
    
    <!-- StatusBar -->
    <StatusBar DockPanel.Dock="Bottom" Background="LightGray">
        <StatusBarItem>
            <TextBlock Text="Ready"/>
        </StatusBarItem>
    </StatusBar>
    
    <!-- Left Panel -->
    <Border DockPanel.Dock="Left" 
            Width="200" 
            Background="LightBlue"
            BorderBrush="Gray" 
            BorderThickness="0,0,1,0">
        <TreeView Margin="10">
            <TreeViewItem Header="Root">
                <TreeViewItem Header="Child 1"/>
                <TreeViewItem Header="Child 2"/>
            </TreeViewItem>
        </TreeView>
    </Border>
    
    <!-- Right Panel -->
    <Border DockPanel.Dock="Right" 
            Width="250" 
            Background="LightGreen"
            BorderBrush="Gray" 
            BorderThickness="1,0,0,0">
        <StackPanel Margin="10">
            <TextBlock Text="Properties" FontWeight="Bold"/>
            <TextBox Margin="0,10,0,5"/>
            <TextBox/>
        </StackPanel>
    </Border>
    
    <!-- Center -->
    <ContentControl Margin="10" Content="Main Content"/>
</DockPanel>
```
</details>

---

## Ключевые выводы

✅ **StackPanel** — элементы в стопку (вертикально/горизонтально)  
✅ **WrapPanel** — элементы с переносом на новую строку  
✅ **DockPanel** — прикрепление к краям (Top, Bottom, Left, Right, Fill)  
✅ **LastChildFill** — последний элемент DockPanel заполняет剩余  
✅ **ItemWidth/ItemHeight** — фиксированный размер элементов в WrapPanel  
✅ **Комбинирование** — используйте панели вместе для сложных layout  
✅ **Производительность** — StackPanel/WrapPanel легче чем Grid

---

## Дополнительные ресурсы

- [StackPanel](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.stackpanel)
- [WrapPanel](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.wrappanel)
- [DockPanel](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.dockpanel)
