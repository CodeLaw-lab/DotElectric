# Тема 10.4: Interop (Win32, DirectX)

### Теория

**WPF Interop** — взаимодействие с Win32 API и DirectX.

#### Технологии

| Технология | Описание | Когда использовать |
|------------|----------|-------------------|
| **P/Invoke** | Вызов Win32 функций | Работа с OS API |
| **HwndSource** | Обёртка WPF вокруг Win32 окна | Интеграция Win32 UI |
| **D3DImage** | DirectX интеграция | 3D графика, видео |
| **WindowsFormsHost** | WinForms в WPF | Legacy контролы |

### Примеры кода

#### Пример 1: P/Invoke для Win32 API

```csharp
// Win32Interop.cs
using System;
using System.Runtime.InteropServices;

public static class Win32Interop
{
    // Импорт функции из user32.dll
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll")]
    public static extern int GetWindowText(IntPtr hWnd, 
                                            System.Text.StringBuilder text, 
                                            int count);
    
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    
    [DllImport("kernel32.dll")]
    public static extern void Beep(int frequency, int duration);
    
    // Обёртка для получения заголовка активного окна
    public static string GetActiveWindowTitle()
    {
        const int nChars = 256;
        var buff = new System.Text.StringBuilder(nChars);
        var handle = GetForegroundWindow();
        
        if (GetWindowText(handle, buff, nChars) > 0)
        {
            return buff.ToString();
        }
        
        return null;
    }
}

// Использование
private void GetActiveWindow_Click(object sender, RoutedEventArgs e)
{
    var title = Win32Interop.GetActiveWindowTitle();
    TitleText.Text = $"Active: {title}";
}

private void Beep_Click(object sender, RoutedEventArgs e)
{
    Win32Interop.Beep(1000, 500); // 1000Hz, 500ms
}
```

#### Пример 2: HwndSourceHook для перехвата сообщений

```csharp
public partial class MainWindow : Window
{
    private HwndSource _hwndSource;
    private IntPtr _hwnd;
    
    // Win32 messages
    private const int WM_SYSCOMMAND = 0x0112;
    private const int SC_MAXIMIZE = 0xF030;
    private const int SC_MINIMIZE = 0xF020;
    
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        
        _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
        _hwndSource.AddHook(WndProc);
        
        _hwnd = new WindowInteropHelper(this).Handle;
    }
    
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_SYSCOMMAND)
        {
            int command = wParam.ToInt32() & 0xFFF0;
            
            if (command == SC_MAXIMIZE)
            {
                // Окно разворачивается
                Debug.WriteLine("Window maximizing");
            }
            else if (command == SC_MINIMIZE)
            {
                // Окно сворачивается
                Debug.WriteLine("Window minimizing");
            }
        }
        
        return IntPtr.Zero;
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _hwndSource?.RemoveHook(WndProc);
        base.OnClosed(e);
    }
}
```

#### Пример 3: D3DImage для DirectX

```csharp
// D3DImageHost.xaml
<DockPanel>
    <Image x:Name="d3dImage" Source="{Binding D3DImageSource}"/>
</DockPanel>

// D3DImageHost.xaml.cs
using System.Windows.Interop;
using System.Windows.Media;

public partial class D3DImageHost : UserControl
{
    private D3DImage _d3dImage;
    
    public D3DImageHost()
    {
        InitializeComponent();
        
        _d3dImage = new D3DImage();
        d3dImage.Source = _d3dImage;
        
        InitializeDirectX();
    }
    
    private void InitializeDirectX()
    {
        // Инициализация DirectX (требует DirectX SDK)
        // Это упрощённый пример
        
        var frontBuffer = new System.Drawing.Bitmap(800, 600);
        var graphics = System.Drawing.Graphics.FromImage(frontBuffer);
        
        // Рисуем что-то
        graphics.Clear(System.Drawing.Color.Blue);
        graphics.DrawString("DirectX Content", 
                           new System.Drawing.Font("Arial", 24), 
                           System.Drawing.Brushes.White, 
                           100, 100);
        
        // Копируем в D3DImage
        var bitmapSource = ConvertToBitmapSource(frontBuffer);
        _d3dImage.Lock();
        _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, bitmapSource);
        _d3dImage.Unlock();
    }
    
    private BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            bitmap.PixelFormat);
        
        var bitmapSource = BitmapSource.Create(
            bitmapData.Width,
            bitmapData.Height,
            96, 96,
            PixelFormats.Bgr32,
            null,
            bitmapData.Scan0,
            bitmapData.Stride * bitmapData.Height,
            bitmapData.Stride);
        
        bitmap.UnlockBits(bitmapData);
        return bitmapSource;
    }
}
```

#### Пример 4: Интеграция WinForms контрола

```xml
<!-- WindowsFormsHost в WPF -->
<Window x:Class="WpfApp.MainWindow">
    <DockPanel>
        <WindowsFormsHost DockPanel.Dock="Top">
            <wf:DataGridView x:Name="winFormsGrid"/>
        </WindowsFormsHost>
        
        <Button Content="Add Row" 
                Click="AddRow_Click"
                DockPanel.Dock="Bottom"/>
    </DockPanel>
</Window>
```

```csharp
using System.Windows.Forms; // WinForms
using System.Windows.Forms.Integration; // WindowsFormsIntegration

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Настройка WinForms DataGridView
        winFormsGrid.Columns.Add("Name", "Name");
        winFormsGrid.Columns.Add("Value", "Value");
    }
    
    private void AddRow_Click(object sender, RoutedEventArgs e)
    {
        winFormsGrid.Rows.Add($"Row {winFormsGrid.Rows.Count}", "Value");
    }
}
```

#### Пример 5: Реальное использование из DotElectric

```csharp
// NativeMethods.cs - Win32 интероп для работы с файлами
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

public static class NativeMethods
{
    // Flags для CreateFile
    [Flags]
    public enum FileAccess : uint
    {
        GenericRead = 0x80000000,
        GenericWrite = 0x40000000
    }
    
    [Flags]
    public enum FileMode : uint
    {
        OpenExisting = 3,
        CreateAlways = 2
    }
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern SafeFileHandle CreateFile(
        string lpFileName,
        FileAccess dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        FileMode dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);
    
    // Оптимизированное чтение файла
    public static async Task<byte[]> ReadFileAsync(string path)
    {
        var handle = CreateFile(
            path,
            FileAccess.GenericRead,
            0,
            IntPtr.Zero,
            FileMode.OpenExisting,
            0x20000000, // FILE_FLAG_OVERLAPPED
            IntPtr.Zero);
        
        if (handle.IsInvalid)
        {
            throw new System.ComponentModel.Win32Exception(
                Marshal.GetLastWin32Error());
        }
        
        using (var stream = new FileStream(handle, FileAccess.GenericRead))
        {
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}

// MouseHook.cs - глобальный перехват мыши
public class GlobalMouseHook : IDisposable
{
    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, 
                                                   IntPtr hMod, uint dwThreadId);
    
    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    
    private const int WH_MOUSE_LL = 14;
    private HookProc _proc;
    private IntPtr _hook = IntPtr.Zero;
    
    public event EventHandler<MouseEventArgs> MouseMoved;
    
    public void Install()
    {
        _proc = HookCallback;
        _hook = SetWindowsHookEx(WH_MOUSE_LL, _proc, IntPtr.Zero, 0);
    }
    
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && MouseMoved != null)
        {
            // Парсинг MOUSEHOOKSTRUCT
            MouseMoved?.Invoke(this, new MouseEventArgs());
        }
        
        return CallNextHookEx(_hook, nCode, wParam, lParam);
    }
    
    public void Dispose()
    {
        UnhookWindowsHookEx(_hook);
    }
}
```

---

### Задачи

#### 🟢 Базовый уровень (30 минут)

**Задача 10.4.1: Simple P/Invoke**

Создайте P/Invoke:
- MessageBox из user32.dll
- Вызов из WPF кнопки
- Показать сообщение

**Задача 10.4.2: GetForegroundWindow**

Создайте метод:
- GetForegroundWindow из user32.dll
- Получение заголовка окна
- Отображение в TextBlock

---

#### 🟡 Средний уровень (1.5 часа)

**Задача 10.4.3: HwndSourceHook**

Реализуйте:
- Перехват WM_SYSCOMMAND
- Логирование разворота/сворачивания
- OnSourceInitialized

**Задача 10.4.4: WindowsFormsHost**

Интегрируйте:
- DataGridView в WPF
- Добавление строк
- Двусторонняя связь

---

#### 🔴 Продвинутый уровень (2 часа)

**Задача 10.4.5: Global Keyboard Hook**

Создайте хук:
- WH_KEYBOARD_LL
- Перехват нажатий
- Горячие клавиши

**Задача 10.4.6: D3DImage Integration**

Интегрируйте DirectX:
- D3DImage
- Рендеринг 3D сцены
- Обновление в WPF

---

### Решения

<details>
<summary>✅ Решение задачи 10.4.1</summary>

```csharp
[DllImport("user32.dll")]
public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

private void ShowMessageBox_Click(object sender, RoutedEventArgs e)
{
    var helper = new WindowInteropHelper(this);
    MessageBox(helper.Handle, "Hello from Win32!", "Title", 0);
}
```
</details>

<details>
<summary>✅ Решение задачи 10.4.2</summary>

```csharp
[DllImport("user32.dll")]
public static extern IntPtr GetForegroundWindow();

[DllImport("user32.dll")]
public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

private void GetActiveWindow_Click(object sender, RoutedEventArgs e)
{
    var hWnd = GetForegroundWindow();
    var text = new StringBuilder(256);
    GetWindowText(hWnd, text, text.Capacity);
    TitleText.Text = $"Active Window: {text}";
}
```
</details>

---

## Ключевые выводы

✅ **P/Invoke** — вызов Win32 функций из C#  
✅ **DllImport** — атрибут для импорта функций  
✅ **HwndSourceHook** — перехват Win32 сообщений  
✅ **D3DImage** — интеграция DirectX в WPF  
✅ **WindowsFormsHost** — WinForms контролы в WPF  
✅ **SafeHandle** — безопасная работа с handle  
✅ **Marshal** — межоперационная память

---

## Дополнительные ресурсы

- [Platform Invoke](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)
- [HwndSourceHook](https://docs.microsoft.com/en-us/dotnet/api/system.windows.interop.hwndsourcehook)
- [D3DImage](https://docs.microsoft.com/en-us/dotnet/api/system.windows.interop.d3dimage)
