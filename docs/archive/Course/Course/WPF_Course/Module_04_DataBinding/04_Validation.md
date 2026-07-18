# Тема 4.4: Валидация (IDataErrorInfo, INotifyDataErrorInfo)

### Теория

**Валидация в WPF** — проверка данных, введённых пользователем, с визуальной обратной связью.

#### Уровни валидации

```
┌─────────────────────────────────────────────────────────┐
│ 1. Exception Validation                                 │
│    └─ Binding выбрасывает исключение                    │
├─────────────────────────────────────────────────────────┤
│ 2. IDataErrorInfo                                       │
│    └─ Синхронная валидация через indexer                │
├─────────────────────────────────────────────────────────┤
│ 3. INotifyDataErrorInfo                                 │
│    └─ Асинхронная валидация с множественными ошибками   │
├─────────────────────────────────────────────────────────┤
│ 4. Custom Validation Rules                              │
│    └─ ValidationRule для специфичной логики             │
└─────────────────────────────────────────────────────────┘
```

#### IDataErrorInfo

```csharp
public interface IDataErrorInfo
{
    string Error { get; }
    string this[string columnName] { get; }
}
```

| Свойство | Описание |
|----------|----------|
| **Error** | Общая ошибка объекта (редко используется) |
| **Indexer** | Ошибка для конкретного свойства |

#### INotifyDataErrorInfo

```csharp
public interface INotifyDataErrorInfo
{
    bool HasErrors { get; }
    event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    IEnumerable GetErrors(string propertyName);
}
```

| Член | Описание |
|------|----------|
| **HasErrors** | Есть ли ошибки |
| **ErrorsChanged** | Событие изменения ошибок |
| **GetErrors()** | Получение списка ошибок для свойства |

### Примеры кода

#### Пример 1: Простая валидация с IDataErrorInfo

```csharp
public class Person : IDataErrorInfo
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }

    public string Error => null; // Не используется

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case "FirstName":
                    if (string.IsNullOrWhiteSpace(FirstName))
                        return "Имя обязательно";
                    if (FirstName.Length < 2)
                        return "Минимум 2 символа";
                    break;

                case "LastName":
                    if (string.IsNullOrWhiteSpace(LastName))
                        return "Фамилия обязательна";
                    break;

                case "Age":
                    if (Age < 0 || Age > 150)
                        return "Возраст от 0 до 150";
                    break;
            }
            return null;
        }
    }
}
```

```xml
<TextBox>
    <TextBox.Text>
        <Binding Path="FirstName" 
                 UpdateSourceTrigger="PropertyChanged"
                 ValidatesOnDataErrors="True"
                 NotifyOnValidationError="True"/>
    </TextBox.Text>
</TextBox>
```

#### Пример 2: Валидация с визуализацией ошибок

```xml
<Window.Resources>
    <!-- Стиль для TextBox с валидацией -->
    <Style TargetType="TextBox">
        <Setter Property="BorderBrush" Value="#CCCCCC"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="8,4"/>
        
        <Style.Triggers>
            <Trigger Property="Validation.HasError" Value="True">
                <Setter Property="BorderBrush" Value="#DC3545"/>
                <Setter Property="BorderThickness" Value="2"/>
                <Setter Property="ToolTip">
                    <Setter.Value>
                        <Binding Path="(Validation.Errors)[0].ErrorContent" 
                                 RelativeSource="{RelativeSource Self}"/>
                    </Setter.Value>
                </Setter>
            </Trigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<StackPanel Margin="20">
    <TextBlock Text="First Name:"/>
    <TextBox Text="{Binding FirstName, UpdateSourceTrigger=PropertyChanged, 
                        ValidatesOnDataErrors=True}"
             Margin="0,5,0,15"/>
    
    <TextBlock Text="Last Name:"/>
    <TextBox Text="{Binding LastName, UpdateSourceTrigger=PropertyChanged, 
                        ValidatesOnDataErrors=True}"
             Margin="0,5,0,15"/>
    
    <TextBlock Text="Age:"/>
    <TextBox Text="{Binding Age, UpdateSourceTrigger=PropertyChanged, 
                        ValidatesOnDataErrors=True}"
             Margin="0,5,0,15"/>
</StackPanel>
```

#### Пример 3: Асинхронная валидация с INotifyDataErrorInfo

```csharp
public class UserRegistration : INotifyDataErrorInfo
{
    private string _username;
    private readonly Dictionary<string, List<string>> _errors 
        = new Dictionary<string, List<string>>();

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
            
            // Асинхронная проверка уникальности
            ValidateUsernameAsync(value);
        }
    }

    public bool HasErrors => _errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    public IEnumerable GetErrors(string propertyName)
    {
        if (_errors.TryGetValue(propertyName, out var errors))
        {
            return errors;
        }
        return null;
    }

    private async Task ValidateUsernameAsync(string username)
    {
        ClearErrors("Username");

        if (string.IsNullOrWhiteSpace(username))
        {
            AddError("Username", "Username is required");
            return;
        }

        if (username.Length < 3)
        {
            AddError("Username", "Minimum 3 characters");
            return;
        }

        // Имитация проверки на сервере
        await Task.Delay(500);

        // Проверка на уникальность
        var isTaken = await IsUsernameTakenAsync(username);
        if (isTaken)
        {
            AddError("Username", "Username is already taken");
        }
    }

    private async Task<bool> IsUsernameTakenAsync(string username)
    {
        // Имитация API вызова
        await Task.Delay(300);
        return username.ToLower() == "admin";
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
        {
            _errors[propertyName] = new List<string>();
        }

        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.ContainsKey(propertyName))
        {
            _errors.Remove(propertyName);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<StackPanel Margin="20">
    <TextBlock Text="Username:"/>
    <TextBox>
        <TextBox.Text>
            <Binding Path="Username" 
                     UpdateSourceTrigger="PropertyChanged"
                     ValidatesOnNotifyDataErrors="True"
                     NotifyOnValidationError="True"
                     AsyncState="UsernameValidation"/>
        </TextBox.Text>
    </TextBox>
    
    <!-- Индикатор валидации -->
    <StackPanel Orientation="Horizontal" Margin="0,5,0,0">
        <TextBlock Text="{Binding HasErrors, Converter={StaticResource BoolToVisibility}, 
                            ConverterParameter=Invert}"
                   Foreground="Green"
                   Text="✓ Available"
                   Visibility="Visible"/>
        <TextBlock Text="Checking..."
                   Foreground="Gray"
                   Visibility="{Binding HasErrors}"/>
    </StackPanel>
</StackPanel>
```

#### Пример 4: ValidationRule для специфичной логики

```csharp
public class EmailValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var email = value?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(email))
        {
            return new ValidationResult(false, "Email is required");
        }

        // Простая проверка формата email
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            return new ValidationResult(false, "Invalid email format");
        }

        return ValidationResult.ValidResult;
    }
}

public class AgeRangeValidationRule : ValidationRule
{
    public int MinAge { get; set; }
    public int MaxAge { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is string str && int.TryParse(str, out var age))
        {
            if (age < MinAge || age > MaxAge)
            {
                return new ValidationResult(false, 
                    $"Age must be between {MinAge} and {MaxAge}");
            }
        }
        else
        {
            return new ValidationResult(false, "Invalid age value");
        }

        return ValidationResult.ValidResult;
    }
}
```

```xml
<TextBox>
    <TextBox.Text>
        <Binding Path="Email" UpdateSourceTrigger="PropertyChanged">
            <Binding.ValidationRules>
                <local:EmailValidationRule/>
            </Binding.ValidationRules>
        </Binding>
    </TextBox.Text>
</TextBox>

<TextBox>
    <TextBox.Text>
        <Binding Path="Age" UpdateSourceTrigger="PropertyChanged">
            <Binding.ValidationRules>
                <local:AgeRangeValidationRule MinAge="18" MaxAge="100"/>
            </Binding.ValidationRules>
        </Binding>
    </TextBox.Text>
</TextBox>
```

#### Пример 5: Комбинированная валидация

```csharp
public class Employee : INotifyPropertyChanged, IDataErrorInfo
{
    private string _email;
    private int _age;
    private readonly Dictionary<string, List<string>> _asyncErrors 
        = new Dictionary<string, List<string>>();

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            
            // Асинхронная проверка email
            ValidateEmailAsync(value);
        }
    }

    public int Age
    {
        get => _age;
        set
        {
            _age = value;
            OnPropertyChanged();
        }
    }

    // IDataErrorInfo для синхронной валидации
    public string Error => null;

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case "Email":
                    if (string.IsNullOrWhiteSpace(Email))
                        return "Email is required";
                    if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        return "Invalid email format";
                    break;

                case "Age":
                    if (Age < 18 || Age > 100)
                        return "Age must be between 18 and 100";
                    break;
            }
            return null;
        }
    }

    // INotifyDataErrorInfo для асинхронной валидации
    public bool HasErrors => _asyncErrors.Any();

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    public IEnumerable GetErrors(string propertyName)
    {
        if (_asyncErrors.TryGetValue(propertyName, out var errors))
        {
            return errors;
        }
        return null;
    }

    private async Task ValidateEmailAsync(string email)
    {
        // Проверка домена
        var domain = email.Split('@')[1];
        await Task.Delay(500); // Имитация проверки

        if (domain == "invalid.com")
        {
            AddError("Email", "This domain is blocked");
        }
        else
        {
            ClearError("Email");
        }
    }

    private void AddError(string propertyName, string error)
    {
        if (!_asyncErrors.ContainsKey(propertyName))
        {
            _asyncErrors[propertyName] = new List<string>();
        }

        if (!_asyncErrors[propertyName].Contains(error))
        {
            _asyncErrors[propertyName].Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void ClearError(string propertyName)
    {
        if (_asyncErrors.ContainsKey(propertyName))
        {
            _asyncErrors.Remove(propertyName);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### Пример 6: Реальная валидация из DotElectric

```csharp
// ValidationService.cs
public static class ValidationService
{
    private static readonly Regex HexColorRegex = 
        new Regex(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");

    public static bool ValidateHexColor(string color, out string error)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            error = "Color is required";
            return false;
        }

        if (!HexColorRegex.IsMatch(color))
        {
            error = "Invalid hex color format (use #RGB or #RRGGBB)";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidateCoordinate(long microns, out string error)
    {
        const long MaxMicrons = 1000000000; // 1000 метров

        if (microns < 0 || microns > MaxMicrons)
        {
            error = $"Coordinate must be between 0 and {MaxMicrons / 1000}mm";
            return false;
        }

        error = null;
        return true;
    }
}

// Line.cs - модель с валидацией
public class Line : ITemplateObject, IDataErrorInfo
{
    private PointMicrons _start;
    private PointMicrons _end;

    public PointMicrons Start
    {
        get => _start;
        set { _start = value; OnPropertyChanged(); }
    }

    public PointMicrons End
    {
        get => _end;
        set { _end = value; OnPropertyChanged(); }
    }

    // IDataErrorInfo
    public string Error => null;

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case "Start":
                    if (!ValidationService.ValidateCoordinate(Start.MicronsX, out var errorX))
                        return $"Start X: {errorX}";
                    if (!ValidationService.ValidateCoordinate(Start.MicronsY, out var errorY))
                        return $"Start Y: {errorY}";
                    break;

                case "End":
                    if (!ValidationService.ValidateCoordinate(End.MicronsX, out errorX))
                        return $"End X: {errorX}";
                    if (!ValidationService.ValidateCoordinate(End.MicronsY, out errorY))
                        return $"End Y: {errorY}";
                    break;
            }
            return null;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

#### Пример 7: Визуализация нескольких ошибок

```xml
<Window.Resources>
    <!-- Template для отображения нескольких ошибок -->
    <ControlTemplate x:Key="ValidationTemplate" TargetType="Control">
        <Grid>
            <AdornedElementPlaceholder/>
            <StackPanel Orientation="Horizontal" 
                        HorizontalAlignment="Right"
                        VerticalAlignment="Top">
                <!-- Индикатор ошибки -->
                <Ellipse Width="16" Height="16" 
                         Fill="#DC3545"
                         ToolTip="{Binding (Validation.Errors)[0].ErrorContent, 
                                    RelativeSource={RelativeSource TemplatedParent}}">
                    <Ellipse.Visibility>
                        <Binding Path="(Validation.HasError)" 
                                 RelativeSource="{RelativeSource TemplatedParent}"
                                 Converter="{StaticResource BoolToVisibility}"/>
                    </Ellipse.Visibility>
                </Ellipse>
            </StackPanel>
        </Grid>
    </ControlTemplate>

    <!-- Стиль для TextBox с множественными ошибками -->
    <Style x:Key="ValidatedTextBox" TargetType="TextBox">
        <Setter Property="Validation.ErrorTemplate" 
                Value="{StaticResource ValidationTemplate}"/>
        <Setter Property="BorderBrush" Value="#CCCCCC"/>
        <Setter Property="BorderThickness" Value="1"/>
        
        <Style.Triggers>
            <Trigger Property="Validation.HasError" Value="True">
                <Setter Property="BorderBrush" Value="#DC3545"/>
                <Setter Property="BorderThickness" Value="2"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<StackPanel Margin="20">
    <!-- TextBox с множественными ошибками -->
    <TextBox Style="{StaticResource ValidatedTextBox}"
             Margin="0,0,0,20">
        <TextBox.Text>
            <Binding Path="Email" 
                     UpdateSourceTrigger="PropertyChanged"
                     ValidatesOnDataErrors="True"
                     ValidatesOnExceptions="True"
                     NotifyOnValidationError="True">
                <Binding.ValidationRules>
                    <local:EmailValidationRule/>
                </Binding.ValidationRules>
            </Binding>
        </TextBox.Text>
    </TextBox>

    <!-- Список всех ошибок -->
    <ItemsControl ItemsSource="{Binding (Validation.Errors), 
                                RelativeSource={RelativeSource Self}}">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <TextBlock Text="{Binding ErrorContent}" 
                           Foreground="#DC3545"
                           FontSize="12"
                           Margin="0,2,0,0"/>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</StackPanel>
```

---

### Задачи

#### 🟢 Базовый уровень (45 минут)

**Задача 4.4.1: Required Validation**

Создайте класс с валидацией обязательных полей:
- Name (не пустой, минимум 2 символа)
- Email (не пустой)
- Реализуйте IDataErrorInfo

**Задача 4.4.2: Range Validation**

Создайте валидацию диапазона:
- Age (от 0 до 150)
- Salary (от 0 до 1000000)
- Используйте ValidationRule

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 4.4.3: Email Format Validation**

Создайте валидацию email:
- Проверка формата (regex)
- Проверка домена (список разрешённых)
- Визуализация ошибки (красная рамка, tooltip)

**Задача 4.4.4: Password Validation**

Создайте валидацию пароля:
- Минимум 8 символов
- Хотя бы 1 заглавная буква
- Хотя бы 1 цифра
- Хотя бы 1 специальный символ
- Отображение требований списком

---

#### 🔴 Продвинутый уровень (2.5 часа)

**Задача 4.4.5: Async Unique Check**

Реализуйте асинхронную проверку:
- Username (проверка уникальности)
- Имитация API (Task.Delay)
- Индикатор "Checking..."
- INotifyDataErrorInfo

**Задача 4.4.6: Multi-Field Validation**

Создайте кросс-полевую валидацию:
- StartDate и EndDate (EndDate > StartDate)
- MinSalary и MaxSalary (Max > Min)
- Ошибка на обоих полях

---

### Решения

<details>
<summary>✅ Решение задачи 4.4.1</summary>

```csharp
public class Person : IDataErrorInfo
{
    public string Name { get; set; }
    public string Email { get; set; }

    public string Error => null;

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case "Name":
                    if (string.IsNullOrWhiteSpace(Name))
                        return "Name is required";
                    if (Name.Length < 2)
                        return "Minimum 2 characters";
                    break;

                case "Email":
                    if (string.IsNullOrWhiteSpace(Email))
                        return "Email is required";
                    break;
            }
            return null;
        }
    }
}
```

```xml
<StackPanel>
    <TextBlock Text="Name:"/>
    <TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged, 
                        ValidatesOnDataErrors=True}"
             Margin="0,5,0,15"/>
    
    <TextBlock Text="Email:"/>
    <TextBox Text="{Binding Email, UpdateSourceTrigger=PropertyChanged, 
                        ValidatesOnDataErrors=True}"
             Margin="0,5,0,15"/>
</StackPanel>
```
</details>

<details>
<summary>✅ Решение задачи 4.4.4</summary>

```csharp
public class PasswordValidator : INotifyPropertyChanged, IDataErrorInfo
{
    private string _password;

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }

    public bool HasMinLength => Password?.Length >= 8;
    public bool HasUpperCase => Password?.Any(char.IsUpper) == true;
    public bool HasDigit => Password?.Any(char.IsDigit) == true;
    public bool HasSpecialChar => Password?.Any(c => !char.IsLetterOrDigit(c)) == true;

    public bool IsValid => HasMinLength && HasUpperCase && HasDigit && HasSpecialChar;

    public string Error => null;

    public string this[string columnName]
    {
        get
        {
            if (columnName == "Password")
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(Password))
                    return "Password is required";
                
                if (!HasMinLength) errors.Add("Minimum 8 characters");
                if (!HasUpperCase) errors.Add("At least 1 uppercase letter");
                if (!HasDigit) errors.Add("At least 1 digit");
                if (!HasSpecialChar) errors.Add("At least 1 special character");

                return errors.Count > 0 ? string.Join("; ", errors) : null;
            }
            return null;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        // Обновляем все свойства требований
        OnPropertyChanged(nameof(HasMinLength));
        OnPropertyChanged(nameof(HasUpperCase));
        OnPropertyChanged(nameof(HasDigit));
        OnPropertyChanged(nameof(HasSpecialChar));
        OnPropertyChanged(nameof(IsValid));
    }
}
```

```xml
<StackPanel Margin="20">
    <TextBlock Text="Password:"/>
    <PasswordBox Password="{Binding Password, UpdateSourceTrigger=PropertyChanged, 
                              ValidatesOnDataErrors=True}"
                 Margin="0,5,0,15"/>
    
    <!-- Требования -->
    <StackPanel Margin="0,0,0,20">
        <StackPanel.Resources>
            <Style TargetType="TextBlock">
                <Style.Triggers>
                    <DataTrigger Binding="{Binding HasMinLength}" Value="True">
                        <Setter Property="Foreground" Value="Green"/>
                    </DataTrigger>
                    <DataTrigger Binding="{Binding HasMinLength}" Value="False">
                        <Setter Property="Foreground" Value="Red"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </StackPanel.Resources>
        
        <TextBlock>
            <Run Text="•"/>
            <Run Text="Minimum 8 characters"/>
        </TextBlock>
        <TextBlock>
            <Run Text="•"/>
            <Run Text="At least 1 uppercase letter"/>
        </TextBlock>
        <TextBlock>
            <Run Text="•"/>
            <Run Text="At least 1 digit"/>
        </TextBlock>
        <TextBlock>
            <Run Text="•"/>
            <Run Text="At least 1 special character"/>
        </TextBlock>
    </StackPanel>
    
    <!-- Индикатор валидности -->
    <TextBlock Text="✓ Valid password" 
               Foreground="Green"
               Visibility="{Binding IsValid, Converter={StaticResource BoolToVisibility}}"/>
</StackPanel>
```
</details>

---

## Ключевые выводы

✅ **IDataErrorInfo** — синхронная валидация через indexer  
✅ **INotifyDataErrorInfo** — асинхронная валидация с множественными ошибками  
✅ **ValidatesOnDataErrors** — включить валидацию через IDataErrorInfo  
✅ **ValidatesOnExceptions** — включить валидацию через исключения  
✅ **ValidatesOnNotifyDataErrors** — включить валидацию через INotifyDataErrorInfo  
✅ **ValidationRule** — кастомная логика валидации  
✅ **Validation.HasError** — свойство для определения наличия ошибки  
✅ **Validation.Errors** — коллекция ошибок элемента

---

## Дополнительные ресурсы

- [IDataErrorInfo](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.idataerrorinfo)
- [INotifyDataErrorInfo](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifydataerrorinfo)
- [Validation](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.validation)
