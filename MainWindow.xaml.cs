using ICSharpCode.BamlDecompiler;
using ICSharpCode.BamlDecompiler.Xaml;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Documents;

namespace DecompileTool;

public partial class MainWindow : Window
{
    private string _lastOutputDir = "";
    private bool _isDark = false;
    private bool _overlayVisible;

    private const string RegPath = @"Software\dottext\textile";
    private const string RegKey = "Theme";

    private static readonly Duration AnimDuration = new(TimeSpan.FromMilliseconds(300));
    private static readonly IEasingFunction AnimEase = new CubicEase { EasingMode = EasingMode.EaseInOut };

    private readonly SolidColorBrush _brushBg = new(Color.FromRgb(245, 245, 245));
    private readonly SolidColorBrush _brushTitleBar = new(Color.FromRgb(245, 245, 245));
    private readonly SolidColorBrush _brushCard = new(Color.FromRgb(243, 243, 243));
    private readonly SolidColorBrush _brushCardStroke = new(Color.FromRgb(224, 224, 224));
    private readonly SolidColorBrush _brushBorder = new(Color.FromRgb(224, 224, 224));
    private readonly SolidColorBrush _brushInputBg = new(Colors.White);
    private readonly SolidColorBrush _brushInputStroke = new(Color.FromRgb(209, 209, 209));
    private readonly SolidColorBrush _brushPrimary = new(Color.FromRgb(26, 26, 26));
    private readonly SolidColorBrush _brushSecondary = new(Color.FromRgb(97, 97, 97));
    private readonly SolidColorBrush _brushOpenFolderBg = new(Color.FromRgb(224, 224, 224));
    private readonly SolidColorBrush _brushOpenFolderFg = new(Color.FromRgb(97, 97, 97));
    private readonly SolidColorBrush _brushIconFg = new(Color.FromRgb(97, 97, 97));
    private readonly SolidColorBrush _brushFooterFg = new(Color.FromRgb(153, 153, 153));

    public MainWindow()
    {
        InitializeComponent();
        LoadTheme();
        if (_isDark) ApplyDarkBrushes();
        ApplyTheme();
    }

    private void LoadTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegPath);
            if (key?.GetValue(RegKey) is string val)
                _isDark = val == "dark";
        }
        catch { }
    }

    private void SaveTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegPath);
            key.SetValue(RegKey, _isDark ? "dark" : "light", RegistryValueKind.String);
        }
        catch { }
    }

    private void ApplyDarkBrushes()
    {
        _brushBg.Color = Color.FromRgb(30, 30, 30);
        _brushTitleBar.Color = Color.FromRgb(25, 25, 25);
        _brushCard.Color = Color.FromRgb(40, 40, 40);
        _brushCardStroke.Color = Color.FromRgb(55, 55, 55);
        _brushBorder.Color = Color.FromRgb(50, 50, 50);
        _brushInputBg.Color = Color.FromRgb(25, 25, 25);
        _brushInputStroke.Color = Color.FromRgb(60, 60, 60);
        _brushPrimary.Color = Colors.White;
        _brushSecondary.Color = Color.FromRgb(170, 170, 170);
        _brushOpenFolderBg.Color = Color.FromRgb(60, 60, 60);
        _brushOpenFolderFg.Color = Color.FromRgb(170, 170, 170);
        _brushIconFg.Color = Color.FromRgb(180, 180, 180);
        _brushFooterFg.Color = Color.FromRgb(100, 100, 100);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Theme_Click(object sender, RoutedEventArgs e)
    {
        _isDark = !_isDark;
        AnimateTheme();
        SaveTheme();
    }

    private static void AnimateBrush(SolidColorBrush brush, Color target)
    {
        var anim = new ColorAnimation(target, AnimDuration) { EasingFunction = AnimEase };
        brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
    }

    private void AnimateTheme()
    {
        AnimateBrush(_brushBg, _isDark ? Color.FromRgb(30, 30, 30) : Color.FromRgb(245, 245, 245));
        AnimateBrush(_brushTitleBar, _isDark ? Color.FromRgb(25, 25, 25) : Color.FromRgb(245, 245, 245));
        AnimateBrush(_brushCard, _isDark ? Color.FromRgb(40, 40, 40) : Color.FromRgb(243, 243, 243));
        AnimateBrush(_brushCardStroke, _isDark ? Color.FromRgb(55, 55, 55) : Color.FromRgb(224, 224, 224));
        AnimateBrush(_brushBorder, _isDark ? Color.FromRgb(50, 50, 50) : Color.FromRgb(224, 224, 224));
        AnimateBrush(_brushInputBg, _isDark ? Color.FromRgb(25, 25, 25) : Colors.White);
        AnimateBrush(_brushInputStroke, _isDark ? Color.FromRgb(60, 60, 60) : Color.FromRgb(209, 209, 209));
        AnimateBrush(_brushPrimary, _isDark ? Colors.White : Color.FromRgb(26, 26, 26));
        AnimateBrush(_brushSecondary, _isDark ? Color.FromRgb(170, 170, 170) : Color.FromRgb(97, 97, 97));
        AnimateBrush(_brushOpenFolderBg, _isDark ? Color.FromRgb(60, 60, 60) : Color.FromRgb(224, 224, 224));
        AnimateBrush(_brushOpenFolderFg, _isDark ? Color.FromRgb(170, 170, 170) : Color.FromRgb(97, 97, 97));
        AnimateBrush(_brushIconFg, _isDark ? Color.FromRgb(180, 180, 180) : Color.FromRgb(97, 97, 97));
        AnimateBrush(_brushFooterFg, _isDark ? Color.FromRgb(100, 100, 100) : Color.FromRgb(153, 153, 153));

        MoonIcon.Visibility = _isDark ? Visibility.Collapsed : Visibility.Visible;
        SunIcon.Visibility = _isDark ? Visibility.Visible : Visibility.Collapsed;
        InfoLightIcon.Visibility = _isDark ? Visibility.Collapsed : Visibility.Visible;
        InfoDarkIcon.Visibility = _isDark ? Visibility.Visible : Visibility.Collapsed;
        RemoveBlackIcon.Visibility = _isDark ? Visibility.Collapsed : Visibility.Visible;
        RemoveWhiteIcon.Visibility = _isDark ? Visibility.Visible : Visibility.Collapsed;
        CloseBlackIcon.Visibility = _isDark ? Visibility.Collapsed : Visibility.Visible;
        CloseWhiteIcon.Visibility = _isDark ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ApplyTheme()
    {
        MainGrid.Background = _brushBg;
        TitleBar.Background = _brushTitleBar;
        WindowBorder.BorderBrush = _brushBorder;
        WindowBorder.Background = _brushBg;

        PathCard.Background = _brushCard;
        PathCard.BorderBrush = _brushCardStroke;
        LogCard.Background = _brushCard;
        LogCard.BorderBrush = _brushCardStroke;

        InputBorder.Background = _brushInputBg;
        InputBorder.BorderBrush = _brushInputStroke;

        TitleBarText.Foreground = _brushIconFg;
        DllFileLabel.Foreground = _brushPrimary;
        TitleText.Foreground = _brushPrimary;
        PathLabel.Foreground = _brushSecondary;
        DllPathBox.Foreground = _brushPrimary;
        LogBlock.Foreground = _brushPrimary;
        StatusText.Foreground = _brushSecondary;
        FooterText.Foreground = _brushFooterFg;

        OpenFolderBtn.Background = _brushOpenFolderBg;
        OpenFolderBtn.Foreground = _brushOpenFolderFg;

        DragIconPath.Stroke = _brushPrimary;
        DragTitleText.Foreground = _brushPrimary;
        DragSubText.Foreground = _brushPrimary;

        MoonIcon.Visibility = _isDark ? Visibility.Collapsed : Visibility.Visible;
        SunIcon.Visibility = _isDark ? Visibility.Visible : Visibility.Collapsed;
        InfoLightIcon.Visibility = _isDark ? Visibility.Collapsed : Visibility.Visible;
        InfoDarkIcon.Visibility = _isDark ? Visibility.Visible : Visibility.Collapsed;
        RemoveBlackIcon.Visibility = _isDark ? Visibility.Collapsed : Visibility.Visible;
        RemoveWhiteIcon.Visibility = _isDark ? Visibility.Visible : Visibility.Collapsed;
        CloseBlackIcon.Visibility = _isDark ? Visibility.Collapsed : Visibility.Visible;
        CloseWhiteIcon.Visibility = _isDark ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Telegram_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://t.me/dottextlol") { UseShellExecute = true });
    }

    private void Info_Click(object sender, RoutedEventArgs e)
    {
        string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
        bool dark = _isDark;
        var bg = dark ? new SolidColorBrush(Color.FromRgb(40, 40, 40)) : new SolidColorBrush(Color.FromRgb(255, 255, 255));
        var fg = dark ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromRgb(26, 26, 26));
        var fgSec = dark ? new SolidColorBrush(Color.FromRgb(170, 170, 170)) : new SolidColorBrush(Color.FromRgb(97, 97, 97));
        var dividerBrush = dark ? new SolidColorBrush(Color.FromRgb(55, 55, 55)) : new SolidColorBrush(Color.FromRgb(230, 230, 230));
        var cardBg = dark ? new SolidColorBrush(Color.FromRgb(50, 50, 50)) : new SolidColorBrush(Color.FromRgb(243, 243, 243));
        var cardStroke = dark ? new SolidColorBrush(Color.FromRgb(65, 65, 65)) : new SolidColorBrush(Color.FromRgb(210, 210, 210));
        var blueBrush = new SolidColorBrush(Color.FromRgb(0, 95, 184));
        var personIcon = dark ? "Resources/PersonWhite.png" : "Resources/PersonBlack.png";
        var codeIcon = dark ? "Resources/CodeWhite.png" : "Resources/CodeBlack.png";

        var info = new Window
        {
            Title = "About textile",
            Width = 460,
            Height = 420,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize,
            FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/CoolveticaRg.otf#Coolvetica"),
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent
        };

        var titleBar = new Grid { Height = 32, Background = Brushes.Transparent };
        titleBar.MouseLeftButtonDown += (_, _) => { info.DragMove(); };
        var titleBarLabel = new TextBlock
        {
            Text = "About textile",
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = fgSec,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 0, 0)
        };
        var closeBtn = new Button
        {
            Content = "\u2715",
            HorizontalAlignment = HorizontalAlignment.Right,
            Width = 32,
            Height = 32,
            FontSize = 12,
            Cursor = Cursors.Hand,
            Background = Brushes.Transparent,
            Foreground = fgSec,
            BorderThickness = new Thickness(0)
        };
        closeBtn.Click += (_, _) => info.Close();
        titleBar.Children.Add(titleBarLabel);
        titleBar.Children.Add(closeBtn);

        var title = new TextBlock
        {
            Text = "textile",
            FontSize = 40,
            FontWeight = FontWeights.Bold,
            Foreground = blueBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 24, 0, 4)
        };

        var ver = new TextBlock
        {
            Text = $"v{version}",
            FontSize = 13,
            Foreground = fgSec,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16)
        };

        var div1 = new Border
        {
            Height = 1,
            Background = dividerBrush,
            Margin = new Thickness(40, 0, 40, 16)
        };

        var subtitle = new TextBlock
        {
            Text = ".NET DLL Decompiler",
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Foreground = fg,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 2)
        };


        var personImg = new Image
        {
            Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"pack://application:,,,/{personIcon}")),
            Width = 18,
            Height = 18,
            VerticalAlignment = VerticalAlignment.Center
        };

        var createdText = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        createdText.Inlines.Add(new System.Windows.Documents.Run("Created by: ") { Foreground = fgSec, FontSize = 13 });
        createdText.Inlines.Add(new System.Windows.Documents.Run(".text") { Foreground = fg, FontWeight = FontWeights.SemiBold, FontSize = 13 });

        var createdRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        createdRow.Children.Add(personImg);
        createdRow.Children.Add(new TextBlock { Width = 12 });
        createdRow.Children.Add(createdText);

        var codeImg = new Image
        {
            Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"pack://application:,,,/{codeIcon}")),
            Width = 18,
            Height = 18,
            VerticalAlignment = VerticalAlignment.Center
        };

        var testedText = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        testedText.Inlines.Add(new System.Windows.Documents.Run("Tested by: ") { Foreground = fgSec, FontSize = 13 });
        testedText.Inlines.Add(new System.Windows.Documents.Run("blvcksyxx & Piper Shpr") { Foreground = fg, FontWeight = FontWeights.SemiBold, FontSize = 13 });

        var testedRow = new StackPanel { Orientation = Orientation.Horizontal };
        testedRow.Children.Add(codeImg);
        testedRow.Children.Add(new TextBlock { Width = 12 });
        testedRow.Children.Add(testedText);

        var card = new Border
        {
            Background = cardBg,
            BorderBrush = cardStroke,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(50, 0, 50, 16),
            Child = new StackPanel { Children = { createdRow, testedRow } }
        };

        var div2 = new Border
        {
            Height = 1,
            Background = dividerBrush,
            Margin = new Thickness(40, 0, 40, 16)
        };

        var tgLink = new TextBlock
        {
            Text = "t.me/dottextlol",
            FontSize = 13,
            Foreground = fgSec,
            HorizontalAlignment = HorizontalAlignment.Center,
            Cursor = Cursors.Hand
        };
        tgLink.MouseLeftButtonDown += (_, _) =>
            Process.Start(new ProcessStartInfo("https://t.me/dottextlol") { UseShellExecute = true });

        var stack = new StackPanel { Margin = new Thickness(0) };
        stack.Children.Add(title);
        stack.Children.Add(ver);
        stack.Children.Add(div1);
        stack.Children.Add(subtitle);

        stack.Children.Add(card);
        stack.Children.Add(div2);
        stack.Children.Add(tgLink);

        var contentGrid = new Grid();
        contentGrid.Children.Add(stack);

        var contentBorder = new Border
        {
            Background = bg,
            Padding = new Thickness(0, 0, 0, 16)
        };

        var innerStack = new StackPanel();
        innerStack.Children.Add(titleBar);
        innerStack.Children.Add(contentBorder);

        contentBorder.Child = contentGrid;

        var outerBorder = new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = bg,
            BorderBrush = dark
                ? new SolidColorBrush(Color.FromRgb(55, 55, 55))
                : new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Child = innerStack
        };

        info.Content = outerBorder;
        info.ShowDialog();
    }

    private static bool IsDropable(DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return false;
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        return files != null && files.Any(f =>
            f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
            f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
    }

    private void ShowDragOverlay()
    {
        if (_overlayVisible) return;
        _overlayVisible = true;

        DragOverlay.Visibility = Visibility.Visible;

        DragOverlay.BeginAnimation(UIElement.OpacityProperty,
            new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });

        DragScale.BeginAnimation(ScaleTransform.ScaleXProperty,
            new DoubleAnimation(0.85, 1, TimeSpan.FromMilliseconds(350))
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.4 }
            });
        DragScale.BeginAnimation(ScaleTransform.ScaleYProperty,
            new DoubleAnimation(0.85, 1, TimeSpan.FromMilliseconds(350))
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.4 }
            });
    }

    private void HideDragOverlay()
    {
        if (!_overlayVisible) return;
        _overlayVisible = false;

        var fade = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(160),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        fade.Completed += (_, _) =>
        {
            if (!_overlayVisible) DragOverlay.Visibility = Visibility.Collapsed;
        };
        DragOverlay.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    private void Window_DragEnter(object sender, DragEventArgs e)
    {
        bool ok = IsDropable(e);
        e.Effects = ok ? DragDropEffects.Copy : DragDropEffects.None;
        if (ok) ShowDragOverlay();
        e.Handled = true;
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        bool ok = IsDropable(e);
        e.Effects = ok ? DragDropEffects.Copy : DragDropEffects.None;
        if (ok) ShowDragOverlay();
        e.Handled = true;
    }

    private void Window_DragLeave(object sender, DragEventArgs e)
    {
        var p = e.GetPosition(this);
        if (p.X < 0 || p.Y < 0 || p.X > ActualWidth || p.Y > ActualHeight)
            HideDragOverlay();
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        HideDragOverlay();

        if (IsDropable(e))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var file = files.First(f =>
                f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
            DllPathBox.Text = file;
        }
        e.Handled = true;
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        HideDragOverlay();
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Assemblies (*.dll;*.exe)|*.dll;*.exe|DLL files (*.dll)|*.dll|EXE files (*.exe)|*.exe|All files (*.*)|*.*",
            Title = "Select assembly"
        };

        if (dlg.ShowDialog() == true)
            DllPathBox.Text = dlg.FileName;
    }

    private async void Decompile_Click(object sender, RoutedEventArgs e)
    {
        string dllPath = DllPathBox.Text.Trim();

        if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
        {
            Log("File not found!", Colors.Red);
            return;
        }

        DecompileBtn.IsEnabled = false;
        OpenFolderBtn.IsEnabled = false;
        StatusText.Text = "Decompiling...";
        LogBlock.Inlines.Clear();

        string dllName = Path.GetFileNameWithoutExtension(dllPath);
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string outputDir = Path.Combine(desktop, dllName + "_decompiled");
        _lastOutputDir = outputDir;

        try
        {
            Log($"Source: {dllPath}");
            Log($"Output: {outputDir}");
            Log("");

            await Task.Run(() => DecompileDll(dllPath, outputDir));

            Log("");
            Log("Done!", null, true);
            StatusText.Text = "Done";
            OpenFolderBtn.IsEnabled = true;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("managed metadata") || ex.Message.Contains("RVA is 0"))
                Log("Not a standard .NET assembly (self-contained bundle or native exe).", Colors.Red);
            else
                LogError(ex.Message);

            StatusText.Text = "Error";
        }
        finally
        {
            DecompileBtn.IsEnabled = true;
        }
    }

    private void DecompileDll(string dllPath, string outputDir)
    {
        var settings = new DecompilerSettings(LanguageVersion.CSharp11_0)
        {
            ThrowOnAssemblyResolveErrors = false
        };

        var decompiler = new CSharpDecompiler(dllPath, settings);
        var types = decompiler.TypeSystem.MainModule.TypeDefinitions;

        Directory.CreateDirectory(outputDir);

        var topLevelTypes = types.Where(t => t.DeclaringType == null && !IsCompilerGenerated(t)).ToList();
        int count = 0;
        int total = topLevelTypes.Count;

        Dispatcher.Invoke(() => Log($"Found {total} Types:"));

        foreach (var type in topLevelTypes)
        {
            count++;
            Dispatcher.Invoke(() => Log($"[{count}/{total}] {type.Name}"));

            try
            {
                string fullCode = decompiler.DecompileTypeAsString(type.FullTypeName);

                string header =
                    "// DECOMPILED BY .text" + Environment.NewLine +
                    $"// Source: {Path.GetFileName(dllPath)}" + Environment.NewLine +
                    $"// Type: {type.FullName}" + Environment.NewLine +
                    Environment.NewLine;

                string dir = outputDir;

                if (!string.IsNullOrEmpty(type.Namespace))
                {
                    string nsDir = type.Namespace.Replace('.', Path.DirectorySeparatorChar);
                    dir = Path.Combine(outputDir, nsDir);
                    Directory.CreateDirectory(dir);
                }

                string filePath = Path.Combine(dir, SanitizeFileName(type.Name) + ".cs");
                File.WriteAllText(filePath, header + fullCode);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogError($"  Skipped {type.Name}: {ex.Message}"));
            }
        }

        ExtractBamlResources(dllPath, outputDir);
    }

    private void ExtractBamlResources(string dllPath, string outputDir)
    {
        try
        {
            using var fs = File.OpenRead(dllPath);
            using var pe = new PEReader(fs);

            if (!pe.HasMetadata) return;

            var md = pe.GetMetadataReader();

            foreach (var resHandle in md.ManifestResources)
            {
                var res = md.GetManifestResource(resHandle);
                string resName = md.GetString(res.Name);

                if (!resName.EndsWith(".g.resources", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!res.Implementation.IsNil)
                    continue;

                var sectionData = pe.GetSectionData(pe.PEHeaders.CorHeader!.ResourcesDirectory.RelativeVirtualAddress);
                var reader = sectionData.GetReader((int)res.Offset, sectionData.Length - (int)res.Offset);
                int length = reader.ReadInt32();
                byte[] data = reader.ReadBytes(length);

                using var ms = new MemoryStream(data);
                using var rr = new System.Resources.ResourceReader(ms);

                var bamlEntries = new List<System.Collections.DictionaryEntry>();
                foreach (System.Collections.DictionaryEntry e in rr)
                {
                    string en = e.Key?.ToString() ?? "";
                    if (en.EndsWith(".baml", StringComparison.OrdinalIgnoreCase))
                        bamlEntries.Add(e);
                }

                string xamlDir = Path.Combine(outputDir, "_xaml");
                Directory.CreateDirectory(xamlDir);

                var xamlDecompiler = new XamlDecompiler(dllPath, new BamlDecompilerSettings
                {
                    ThrowOnAssemblyResolveErrors = false
                });

                int wpfTotal = bamlEntries.Count;
                Dispatcher.Invoke(() => Log($"\nFound {wpfTotal} WPF Resources:"));

                int wpfCount = 0;
                foreach (var entry in bamlEntries)
                {
                    wpfCount++;
                    string entryName = entry.Key?.ToString() ?? "";
                    try
                    {
                        byte[] bamlData = entry.Value is Stream s ? ReadStream(s)
                                        : entry.Value as byte[] ?? [];

                        using var bamlStream = new MemoryStream(bamlData);
                        var result = xamlDecompiler.Decompile(bamlStream);

                        string outName = Path.GetFileNameWithoutExtension(entryName) + ".xaml";
                        string subDir = Path.GetDirectoryName(entryName) ?? "";
                        string fullDir = Path.Combine(xamlDir, subDir);
                        Directory.CreateDirectory(fullDir);
                        string outPath = Path.Combine(fullDir, outName);

                        result.Xaml.Save(outPath);

                        Dispatcher.Invoke(() => Log($"[{wpfCount}/{wpfTotal}] {outName}"));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() => LogError($"  Skipped {entryName}: {ex.Message}"));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => LogError($"BAML extraction failed: {ex.Message}"));
        }
    }

    private static byte[] ReadStream(Stream s)
    {
        using var ms = new MemoryStream();
        s.CopyTo(ms);
        return ms.ToArray();
    }

    private static bool IsCompilerGenerated(ITypeDefinition type)
    {
        string name = type.Name;
        string fullName = type.FullName;

        if (name.Contains("<")) return true;
        if (fullName.Contains("<")) return true;
        if (name.Contains("__DisplayClass")) return true;
        if (name.Contains("__DynamicView")) return true;
        if (name.Contains("AnonymousType")) return true;
        if (name.Contains("PrivateImplementationDetails")) return true;
        if (name == "Module") return true;

        bool nested = type.DeclaringType != null;
        if (nested && (name.Contains("d__") || name.Contains("c__") || name.Contains("<<") || name.Contains(">d__"))) return true;

        if (name.StartsWith("UnitySourceGenerated", StringComparison.Ordinal)) return true;
        if (name.EndsWith("_v1", StringComparison.Ordinal) && name.Contains("MonoScript")) return true;
        if (name.StartsWith("Il2CppComObjectProxy", StringComparison.Ordinal)) return true;
        if (name.StartsWith("__Il2CppComObject", StringComparison.Ordinal)) return true;
        if (name.Contains("$BurstManaged")) return true;
        if (name.Contains("_BurstJobScheduler")) return true;
        if (name.EndsWith("Attribute") && name.StartsWith("System.Runtime.CompilerServices", StringComparison.Ordinal)) return true;
        if (fullName.StartsWith("Unity.Collections.LowLevel", StringComparison.Ordinal) && name.Contains("__")) return true;

        return false;
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_lastOutputDir) && Directory.Exists(_lastOutputDir))
        {
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{_lastOutputDir}\"")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                LogError($"Could not open folder: {ex.Message}");
            }
        }
    }

    private void Log(string text, Color? color = null, bool bold = false)
    {
        var run = new Run((text ?? string.Empty) + Environment.NewLine);
        if (color.HasValue)
            run.Foreground = new SolidColorBrush(color.Value);
        if (bold)
            run.FontWeight = FontWeights.Bold;
        LogBlock.Inlines.Add(run);
        LogScroll.ScrollToEnd();
    }

    private void LogError(string message)
    {
        Log("Error!", Colors.Red, true);
        if (!string.IsNullOrWhiteSpace(message))
            Log(message, Colors.Red);
        Log("Please, open an issue on Official GitHub Repository of Textile! (https://github.com/wh0amitg/textile)", Colors.Red);
    }
}