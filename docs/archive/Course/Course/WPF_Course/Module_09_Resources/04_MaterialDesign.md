# Тема 9.4: MaterialDesignThemes интеграция

### Теория

**MaterialDesignThemes** — популярная библиотека тем для WPF с Material Design стилем.

#### Установка

```
Install-Package MaterialDesignThemes
Install-Package MaterialDesignColors
```

#### Возможности

✅ **Готовые стили** — для всех стандартных контролов  
✅ **Темы** — Light/Dark из коробки  
✅ **Цвета** — Material Design палитра  
✅ **Иконки** — PackIcon с 2000+ иконок  
✅ **Кастомизация** — настройка цветов

### Примеры кода

#### Пример 1: Базовая настройка

```xml
<!-- App.xaml -->
<Application x:Class="WpfApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Material Design цвета -->
                <materialDesign:CustomColorTheme BaseTheme="Dark"
                                                  PrimaryColor="#0078D4"
                                                  SecondaryColor="#66BB6A"/>
                
                <!-- Material Design стили -->
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

#### Пример 2: Material Design Button

```xml
<StackPanel Margin="20">
    <!-- Обычная кнопка -->
    <Button Content="Normal Button"
            Style="{StaticResource MaterialDesignRaisedButton}"
            Margin="5"/>
    
    <!-- Primary кнопка -->
    <Button Content="Primary Button"
            Style="{StaticResource MaterialDesignRaisedPrimaryButton}"
            Margin="5"/>
    
    <!-- Accent кнопка -->
    <Button Content="Accent Button"
            Style="{StaticResource MaterialDesignRaisedAccentButton}"
            Margin="5"/>
    
    <!-- Flat кнопка -->
    <Button Content="Flat Button"
            Style="{StaticResource MaterialDesignFlatButton}"
            Margin="5"/>
    
    <!-- Кнопка с иконкой -->
    <Button Style="{StaticResource MaterialDesignIconButton}">
        <materialDesign:PackIcon Kind="Heart" Width="24" Height="24"/>
    </Button>
    
    <!-- Кнопка с иконкой и текстом -->
    <Button Style="{StaticResource MaterialDesignRaisedButton}">
        <StackPanel Orientation="Horizontal">
            <materialDesign:PackIcon Kind="Save" Width="20" Height="20" Margin="0,0,8,0"/>
            <TextBlock Text="Save"/>
        </StackPanel>
    </Button>
</StackPanel>
```

#### Пример 3: Material Design TextBox

```xml
<StackPanel Margin="20">
    <!-- Обычный TextBox -->
    <TextBox Style="{StaticResource MaterialDesignTextBox}"
             materialDesign:HintAssist.Hint="Enter text"
             Margin="5"/>
    
    <!-- TextBox с floating hint -->
    <TextBox Style="{StaticResource MaterialDesignFloatingHintTextBox}"
             materialDesign:HintAssist.Hint="Floating hint"
             Margin="5"/>
    
    <!-- TextBox с иконкой -->
    <TextBox Style="{StaticResource MaterialDesignTextBox}"
             materialDesign:HintAssist.Hint="Search"
             materialDesign:TextFieldAssist.HasClearButton="True"
             Margin="5">
        <TextBox.Text>
            <Binding Path="SearchText" UpdateSourceTrigger="PropertyChanged"/>
        </TextBox.Text>
    </TextBox>
    
    <!-- PasswordBox -->
    <PasswordBox Style="{StaticResource MaterialDesignPasswordBox}"
                 materialDesign:HintAssist.Hint="Password"
                 Margin="5"/>
    
    <!-- ComboBox -->
    <ComboBox Style="{StaticResource MaterialDesignComboBox}"
              materialDesign:HintAssist.Hint="Select item"
              Margin="5">
        <ComboBoxItem Content="Item 1"/>
        <ComboBoxItem Content="Item 2"/>
        <ComboBoxItem Content="Item 3"/>
    </ComboBox>
</StackPanel>
```

#### Пример 4: Material Design Card

```xml
<ScrollViewer>
    <WrapPanel Margin="20">
        <!-- Card 1 -->
        <materialDesign:Card Width="300" Margin="10">
            <StackPanel>
                <Image Source="/images/image1.jpg" Height="200" Stretch="UniformToFill"/>
                <StackPanel Margin="16">
                    <TextBlock Text="Card Title"
                               Style="{StaticResource MaterialDesignHeadline6TextBlock}"/>
                    <TextBlock Text="Card description text goes here."
                               Style="{StaticResource MaterialDesignBody2TextBlock}"
                               TextWrapping="Wrap"
                               Margin="0,8,0,16"/>
                    <Button Content="Action"
                            Style="{StaticResource MaterialDesignFlatButton}"
                            HorizontalAlignment="Right"/>
                </StackPanel>
            </StackPanel>
        </materialDesign:Card>
        
        <!-- Card 2 -->
        <materialDesign:Card Width="300" Margin="10">
            <StackPanel>
                <StackPanel Margin="16">
                    <TextBlock Text="Simple Card"
                               Style="{StaticResource MaterialDesignTitleTextBlock}"/>
                    <TextBlock Text="This is a simple card with just text."
                               Style="{StaticResource MaterialDesignBody2TextBlock}"
                               TextWrapping="Wrap"
                               Margin="0,8,0,16"/>
                </StackPanel>
                <Separator/>
                <StackPanel Orientation="Horizontal" Margin="8">
                    <Button Content="Share" Style="{StaticResource MaterialDesignFlatButton}"/>
                    <Button Content="Explore" Style="{StaticResource MaterialDesignFlatButton}"/>
                </StackPanel>
            </StackPanel>
        </materialDesign:Card>
    </WrapPanel>
</ScrollViewer>
```

#### Пример 5: Material Design Dialog

```csharp
// MainWindow.xaml.cs
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ShowDialog_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SampleDialog();
        
        var result = await DialogHost.Show(dialog, "RootDialog");
        
        if (result is bool dialogResult && dialogResult)
        {
            MessageBox.Show("Dialog accepted!");
        }
    }
}
```

```xml
<!-- SampleDialog.xaml -->
<materialDesign:DialogHost>
    <StackPanel Margin="20" MinWidth="300">
        <TextBlock Text="Dialog Title"
                   Style="{StaticResource MaterialDesignHeadline6TextBlock}"
                   Margin="0,0,0,20"/>
        
        <TextBox Style="{StaticResource MaterialDesignTextBox}"
                 materialDesign:HintAssist.Hint="Enter value"/>
        
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,20,0,0">
            <Button Content="Cancel"
                    Style="{StaticResource MaterialDesignFlatButton}"
                    Command="{x:Static materialDesign:DialogHost.CloseDialogCommand}"
                    CommandParameter="False"/>
            <Button Content="OK"
                    Style="{StaticResource MaterialDesignFlatButton}"
                    Command="{x:Static materialDesign:DialogHost.CloseDialogCommand}"
                    CommandParameter="True"/>
        </StackPanel>
    </StackPanel>
</materialDesign:DialogHost>
```

#### Пример 6: Material Design Snackbar

```csharp
// Snackbar уведомление
private async void ShowSnackbar_Click(object sender, RoutedEventArgs e)
{
    var snackbarMessage = new SnackbarMessage
    {
        Content = "Operation completed successfully!",
        ActionContent = "UNDO"
    };
    
    snackbarMessage.ActionClick += (s, args) =>
    {
        // Undo logic
    };
    
    await SnackbarMessageQueue.Enqueue(snackbarMessage);
}
```

```xml
<!-- MainWindow.xaml -->
<Grid>
    <materialDesign:DialogHost x:Name="RootDialog" Identifier="RootDialog">
        <materialDesign:SnackbarMessageQueue x:Name="SnackbarMessageQueue"/>
        
        <Grid>
            <Button Content="Show Snackbar"
                    Click="ShowSnackbar_Click"
                    Style="{StaticResource MaterialDesignRaisedButton}"/>
        </Grid>
    </materialDesign:DialogHost>
</Grid>
```

#### Пример 7: Material Design PackIcon

```xml
<WrapPanel Margin="20">
    <!-- Basic icons -->
    <materialDesign:PackIcon Kind="Home" Width="24" Height="24" Margin="5"/>
    <materialDesign:PackIcon Kind="Account" Width="24" Height="24" Margin="5"/>
    <materialDesign:PackIcon Kind="Settings" Width="24" Height="24" Margin="5"/>
    
    <!-- Colored icons -->
    <materialDesign:PackIcon Kind="Heart" Width="24" Height="24" Foreground="Red" Margin="5"/>
    <materialDesign:PackIcon Kind="Star" Width="24" Height="24" Foreground="Gold" Margin="5"/>
    <materialDesign:PackIcon Kind="Check" Width="24" Height="24" Foreground="Green" Margin="5"/>
    
    <!-- Large icons -->
    <materialDesign:PackIcon Kind="Warning" Width="48" Height="48" Foreground="Orange" Margin="5"/>
    <materialDesign:PackIcon Kind="Information" Width="48" Height="48" Foreground="Blue" Margin="5"/>
</WrapPanel>
```

#### Пример 8: Реальное использование из DotElectric

```xml
<!-- App.xaml из DotElectric -->
<Application x:Class="DotElectric.TemplateEditor.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Material Design с кастомными цветами -->
                <materialDesign:CustomColorTheme BaseTheme="Dark"
                                                  PrimaryColor="#0078D4"
                                                  SecondaryColor="#66BB6A"/>
                
                <!-- Material Design стили по умолчанию -->
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml"/>
                
                <!-- Кастомные темы приложения -->
                <ResourceDictionary Source="Resources/Styles/DarkTheme.xaml"/>
                
                <!-- Конвертеры -->
                <ResourceDictionary Source="Resources/Converters/Converters.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

```xml
<!-- MainWindow.xaml из DotElectric -->
<Window x:Class="DotElectric.TemplateEditor.MainWindow">
    <materialDesign:DialogHost x:Name="RootDialog" Identifier="RootDialog">
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>
            
            <!-- Menu с Material Design -->
            <Menu Grid.Row="0"
                  Style="{StaticResource MaterialDesignMenu}">
                <MenuItem Header="_Файл">
                    <MenuItem Header="_Новый"
                              Command="{Binding NewTabCommand}">
                        <MenuItem.Icon>
                            <materialDesign:PackIcon Kind="FilePlusOutline" Width="16" Height="16"/>
                        </MenuItem.Icon>
                    </MenuItem>
                    <MenuItem Header="_Открыть"
                              Command="{Binding OpenFileCommand}">
                        <MenuItem.Icon>
                            <materialDesign:PackIcon Kind="FolderOpenOutline" Width="16" Height="16"/>
                        </MenuItem.Icon>
                    </MenuItem>
                    <Separator/>
                    <MenuItem Header="_Сохранить"
                              Command="{Binding SaveCommand}">
                        <MenuItem.Icon>
                            <materialDesign:PackIcon Kind="ContentSaveOutline" Width="16" Height="16"/>
                        </MenuItem.Icon>
                    </MenuItem>
                </MenuItem>
            </Menu>
            
            <!-- ToolBar с Material Design кнопками -->
            <ToolBarTray Grid.Row="1" Background="{DynamicResource PanelBackgroundBrush}">
                <ToolBar>
                    <Button Style="{StaticResource MaterialDesignIconButton}"
                            Command="{Binding NewTabCommand}"
                            ToolTip="Новый (Ctrl+N)">
                        <materialDesign:PackIcon Kind="FilePlusOutline" Width="20" Height="20"/>
                    </Button>
                    <Button Style="{StaticResource MaterialDesignIconButton}"
                            Command="{Binding OpenFileCommand}"
                            ToolTip="Открыть (Ctrl+O)">
                        <materialDesign:PackIcon Kind="FolderOpenOutline" Width="20" Height="20"/>
                    </Button>
                    <Separator Style="{StaticResource MaterialDesignSeparator}"/>
                    <Button Style="{StaticResource MaterialDesignIconButton}"
                            Command="{Binding UndoCommand}"
                            ToolTip="Отменить (Ctrl+Z)">
                        <materialDesign:PackIcon Kind="Undo" Width="20" Height="20"/>
                    </Button>
                </ToolBar>
            </ToolBarTray>
            
            <!-- StatusBar с Material Design -->
            <StatusBar Grid.Row="3"
                       Style="{StaticResource MaterialDesignStatusBar}">
                <StatusBarItem>
                    <TextBlock Text="{Binding StatusMessage}"/>
                </StatusBarItem>
                <Separator/>
                <StatusBarItem>
                    <TextBlock Text="{Binding SheetFormat}"/>
                </StatusBarItem>
            </StatusBar>
            
            <!-- Snackbar для уведомлений -->
            <materialDesign:SnackbarMessageQueue x:Name="SnackbarMessageQueue"/>
        </Grid>
    </materialDesign:DialogHost>
</Window>
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 9.4.1: MaterialDesign Setup**

Настройте MaterialDesignThemes:
- Установите пакеты
- Добавьте в App.xaml
- Создайте кнопку с иконкой

**Задача 9.4.2: MaterialDesign TextBox**

Создайте TextBox:
- FloatingHint стиль
- ClearButton
- Hint текст

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 9.4.3: MaterialDesign Cards**

Создайте галерею карточек:
- 3+ Card
- Изображения
- Кнопки действий

**Задача 9.4.4: MaterialDesign Dialog**

Реализуйте диалог:
- DialogHost
- SampleDialog
- OK/Cancel кнопки

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 9.4.5: Custom MaterialDesign Theme**

Создайте кастомную тему:
- CustomColorTheme
- Свои цвета
- Интеграция с MaterialDesign

**Задача 9.4.6: Full MaterialDesign App**

Создайте приложение:
- Все контролы MaterialDesign
- Dialogs и Snackbars
- PackIcon иконки

---

### Решения

<details>
<summary>✅ Решение задачи 9.4.1</summary>

```xml
<!-- App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <materialDesign:CustomColorTheme BaseTheme="Dark"
                                              PrimaryColor="#0078D4"
                                              SecondaryColor="#66BB6A"/>
            <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

```xml
<!-- MainWindow.xaml -->
<Button Style="{StaticResource MaterialDesignRaisedButton}">
    <StackPanel Orientation="Horizontal">
        <materialDesign:PackIcon Kind="Heart" Width="20" Height="20" Margin="0,0,8,0"/>
        <TextBlock Text="Click Me"/>
    </StackPanel>
</Button>
```
</details>

<details>
<summary>✅ Решение задачи 9.4.4</summary>

```csharp
// Dialog код
private async void ShowDialog_Click(object sender, RoutedEventArgs e)
{
    var dialog = new SampleDialog();
    var result = await DialogHost.Show(dialog, "RootDialog");
}
```

```xml
<!-- DialogHost в MainWindow -->
<materialDesign:DialogHost x:Name="RootDialog" Identifier="RootDialog">
    <Grid>
        <Button Content="Show Dialog" Click="ShowDialog_Click"/>
    </Grid>
</materialDesign:DialogHost>
```
</details>

---

## Ключевые выводы

✅ **MaterialDesignThemes** — готовая библиотека стилей  
✅ **CustomColorTheme** — кастомизация цветов  
✅ **PackIcon** — 2000+ иконок  
✅ **DialogHost** — модальные диалоги  
✅ **Snackbar** — уведомления  
✅ **Card** — карточки в Material Design стиле  
✅ **Все контролы** — стилизация из коробки

---

## Дополнительные ресурсы

- [MaterialDesignThemes GitHub](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- [Material Design Spec](https://material.io/design)
- [PackIcon List](https://materialdesignicons.com/)
