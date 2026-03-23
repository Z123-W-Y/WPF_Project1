using Project.Views.Pages;
using System.Collections.ObjectModel;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;


namespace Project
{
    public class MotoAxisParam
    {
        public string AxisName { get; set; } = " ";       //轴号
        public double PositiveLimit { get; set; }   //正极限
        public double NegativeLimit { get; set; }   //负极限
        public double AccTime { get; set; }         //加速时间
        public double DecTime { get; set; }         //减速时间
        public double JogSpeed { get; set; }       // JOG速度
    }

    // 添加数据模型类
    public class DataItem
    {
        public int No { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double MeasureValue { get; set; }
        public double ControlDepth { get; set; }
        public double CorrectedDepth { get; set; }
        // ✅ 新增：标记是否为占位空行 (默认 false)
        public bool IsPlaceholder { get; set; } = false;
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private Button? _selectedButton;

        private readonly Dictionary<string, BitmapSource> _imageCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Button, string> _buttonImageNames = new();

        private const int MIN_VISIBLE_ROWS = 30;
        public ObservableCollection<DataItem> DataList { get; set; } = new ObservableCollection<DataItem>();
        public MainWindow()
        {
            InitializeComponent();

            // 初始化定时器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // 每秒触发一次
            timer.Tick += Timer_Tick; // 绑定事件
            timer.Start(); // 启动定时器

        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // 现在这里可以识别 txtDateTime 了
            txtDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterButton(btnHome, "home");
            RegisterButton(btnReset, "reset");
            RegisterButton(btnStart, "start");
            RegisterButton(btnPause, "pause");
            RegisterButton(btnStop, "stop");
            RegisterButton(btnManual, "manual");
            RegisterButton(btnSet, "set");

            PreloadButtonImages();

            InitButtonImage(btnHome);
            InitButtonImage(btnReset);
            InitButtonImage(btnStart);
            InitButtonImage(btnPause);
            InitButtonImage(btnStop);
            InitButtonImage(btnManual);
            InitButtonImage(btnSet);

            SelectButton(btnHome);
            LoadTestData();
            //// 首次加载首页页面右侧内容
            MainContent.Content = new StartView();

        }
        private void RegisterButton(Button button, string imageName)
        {
            _buttonImageNames[button] = imageName;
        }
        private void PreloadButtonImages()
        {
            foreach (var imageName in _buttonImageNames.Values.Distinct())
            {
                LoadBitmap(GetImageUri($"{imageName}1"), decodePixelWidth: 40);
                LoadBitmap(GetImageUri($"{imageName}2"), decodePixelWidth: 40);
            }
        }

        private void InitButtonImage(Button button)
        {
            // 设置默认图片
            SetButtonImage(button, GetButtonImageKey(button, isSelected: false));

            // 鼠标悬停：非选中状态才切换（选中状态保持选中图）
            button.MouseEnter += (s, e) =>
            {
                if (_selectedButton != button) // 不是当前选中按钮
                {
                    SetButtonImage(button, GetButtonImageKey(button, isSelected: true));
                }
            };

            // 鼠标离开：非选中状态才恢复默认（选中状态保持选中图）
            button.MouseLeave += (s, e) =>
            {
                if (_selectedButton != button) // 不是当前选中按钮
                {
                    SetButtonImage(button, GetButtonImageKey(button, isSelected: false));
                }
            };

            // 点击事件：切换选中状态
            button.Click += (s, e) =>
            {
                SelectButton(button);
                UserControl? newPage = null;
                if (button == btnHome)
                    newPage = new StartView();
                else if (button == btnReset)
                    newPage = new StartView();
                else if (button == btnStart)
                    newPage = new StartView();
                else if (button == btnPause)
                    newPage = new StartView();
                else if (button == btnStop)
                    newPage = new StartView();
                else if (button == btnManual)
                    newPage = new ManualView();
                else if (button == btnSet)
                    newPage = new SetView();

                if (newPage != null)
                {
                    MainContent.Content = newPage;
                }
            };
        }

        private void SelectButton(Button button)
        {
            // 取消上一个选中状态
            if (_selectedButton != null)
            {
                _selectedButton.Foreground = ConvertColor("#E6E6E6");
                SetButtonImage(_selectedButton, GetButtonImageKey(_selectedButton, isSelected: false));
            }

            _selectedButton = button;
            _selectedButton.Foreground = ConvertColor("#00FFFF");
            SetButtonImage(_selectedButton, GetButtonImageKey(_selectedButton, isSelected: true));
        }
        private void LoadTestData()
        {
            DataList.Clear();

            // --- 模拟真实数据 (这里保持你原来的20条，或者你可以改成更少来测试效果) ---
            int realDataCount = 20;
            for (int i = 0; i < realDataCount; i++)
            {
                DataList.Add(new DataItem
                {
                    No = i + 1,
                    X = Math.Round(12.358 + i * 0.1, 3),
                    Y = Math.Round(32.807 + i * 0.1, 3),
                    MeasureValue = Math.Round(2.28424 + i * 0.01, 5),
                    ControlDepth = Math.Round(1.855 + i * 0.01, 3),
                    CorrectedDepth = Math.Round(1.83814 + i * 0.01, 5),
                    IsPlaceholder = false // 标记为真实数据
                });
            }

            // ✅ 关键：加载完真实数据后，立即补充空行
            SyncPlaceholderRows();
        }

        private void SyncPlaceholderRows()
        {
            // A. 先移除所有现有的占位行 (防止重复添加)
            // 注意：必须转成 List 再遍历删除，否则集合会被修改报错
            var placeholders = DataList.Where(x => x.IsPlaceholder).ToList();
            foreach (var item in placeholders)
            {
                DataList.Remove(item);
            }

            // B. 计算当前真实数据数量
            int realCount = DataList.Count;

            // C. 如果不足最小行数，补足
            if (realCount < MIN_VISIBLE_ROWS)
            {
                int needToAdd = MIN_VISIBLE_ROWS - realCount;
                for (int i = 0; i < needToAdd; i++)
                {
                    DataList.Add(new DataItem
                    {
                        No = 0,
                        X = 0,
                        Y = 0,
                        MeasureValue = 0,
                        ControlDepth = 0,
                        CorrectedDepth = 0,
                        IsPlaceholder = true // ✅ 标记为空行
                    });
                }
            }
        }

        private void SetButtonImage(Button button, string imagePath)
        {
            var icon = button.Template.FindName("icon", button) as Image;
            if (icon != null)
            {
                try
                {
                    var bitmap = LoadBitmap(imagePath, decodePixelWidth: 40);
                    icon.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"图片加载失败：{imagePath}\n错误：{ex.Message}");
                }
            }
        }

        private Brush ConvertColor(string colorHex)
        {
            // 尝试转换，失败返回默认浅灰色
            return new BrushConverter().ConvertFrom(colorHex) as Brush ?? Brushes.LightGray;
        }

        private string GetButtonImageKey(Button button, bool isSelected)
        {
            if (!_buttonImageNames.TryGetValue(button, out var imageName))
            {
                throw new InvalidOperationException($"未找到按钮 {button.Name} 对应的图片配置。");
            }

            var stateSuffix = isSelected ? "2" : "1";
            return GetImageUri($"{imageName}{stateSuffix}");
        }

        private BitmapSource LoadBitmap(string imagePath, int decodePixelWidth)
        {
            if (_imageCache.TryGetValue(imagePath, out var cachedBitmap))
            {
                return cachedBitmap;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            _imageCache[imagePath] = bitmap;
            return bitmap;
        }
        private static string GetImageUri(string imageName)
        {
            return $"pack://application:,,,/Resources/{imageName}.png";
        }

        //========================警报===================
        private void imgAlarm_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateAlarmImage();
        }

        // 点击时
        private void imgAlarm_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //直接调用，无需额外 using
            AlarmStateManager.Instance.ToggleAlarm();
            UpdateAlarmImage();
        }

        private void UpdateAlarmImage()
        {
            string path = AlarmStateManager.Instance.GetCurrentImagePath();
            imgAlarm.Source = new BitmapImage(new Uri(path));
        }
    }
}