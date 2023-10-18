using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PrizePicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private Random _random = new();
        private List<ImageSource> _prizeImages;
        private int _randomIndex;
        private bool _isRouletteRunning = false;

        // リソースから音声ファイルを読み込む
        private SoundPlayer _runSound = new(Properties.Resources.決定ボタンを押す15);
        private SoundPlayer _rouletteRunningSound = new(Properties.Resources.電子ルーレット回転中);
        private SoundPlayer _rouletteStopSound = new(Properties.Resources.電子ルーレット停止ボタンを押す);

        public MainWindow()
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.PrizePictureFolderPath))
            {
                PrizePictureFolderTextBox.Text = Properties.Settings.Default.PrizePictureFolderPath;
            }
        }

        // Main
        private void Register_Button_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            // 設定ファイルにも景品画像フォルダのパスにもパスが設定されていない場合は、エラーを表示して終了
            if (string.IsNullOrEmpty(PrizePictureFolderTextBox.Text) || !Directory.Exists(PrizePictureFolderTextBox.Text))
            {
                MessageBox.Show("景品画像フォルダのパスが設定されていないか、パスが有効ではありません。景品画像ボタンから画像が格納されたフォルダを指定してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MainPanel.Visibility = Visibility.Collapsed;
            RoulettePanel.Visibility = Visibility.Visible;

            InitializeRoulette();
        }

        private void InitializeRoulette()
        {
            _prizeImages = GetPrizeImages(PrizePictureFolderTextBox.Text);
            MessageLabel.Content = "クリックしてください";
            StartRoulette();
        }

        private List<string> GetAllPrizeImagePaths(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                return new List<string>();
            }

            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            return Directory.GetFiles(folderPath).Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower())).ToList();
        }

        private List<ImageSource> GetPrizeImages(string folderPath)
        {
            var imagePaths = GetAllPrizeImagePaths(folderPath);
            var prizeImages = new List<ImageSource>();
            foreach (var imagePath in imagePaths)
            {
                prizeImages.Add(LoadImage(imagePath));
            }

            return prizeImages;
        }

        private ImageSource LoadImage(string path)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; // これは画像をメモリにキャッシュするためのものです。
            image.UriSource = new Uri(path);
            image.EndInit();
            return image;
        }

        private void StartRoulette()
        {
            if (_prizeImages.Count == 0)
            {
                return;
            }

            _isRouletteRunning = true;
            PlayRunSound();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1.0 / 64); // 1/64秒ごと
            _timer.Tick += OnTimerTick;
            _timer.Start();
            PlayRouletteRunningSound();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            // ランダムに画像を表示
            _randomIndex = _random.Next(_prizeImages.Count);
            PrizeImage.Source = _prizeImages[_randomIndex];
        }

        private void PrizeImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isRouletteRunning)
            {
                // ルーレットが動いている場合は、ルーレットを停止し、画像を表示する
                StopRoulette();

                // ランダムに画像を選び、それをリストから削除
                _randomIndex = _random.Next(_prizeImages.Count);
                PrizeImage.Source = _prizeImages[_randomIndex];
                _prizeImages.RemoveAt(_randomIndex);

                if (_prizeImages.Count == 1)
                {
                    MessageLabel.Content = "次がラストの景品です。クリックしてください";
                    PlayRouletteStopSound();
                }
                else
                {
                    MessageLabel.Content = "クリックしてください";
                    PlayRouletteStopSound();
                }
            }
            else
            {
                // ルーレットが動いていない場合は、ルーレットを開始する
                if (_prizeImages.Count == 0)
                {
                    // すべての画像が表示された後
                    RoulettePanel.Visibility = Visibility.Collapsed;
                    MainPanel.Visibility = Visibility.Visible;
                    return;
                }
                else if (_prizeImages.Count == 1)
                {
                    // 1枚だけ残っている場合は、それを表示する
                    PlayRunSound();
                    PrizeImage.Source = _prizeImages[0];
                    _prizeImages.Clear();
                    MessageLabel.Content = "ラストの景品です。クリックするとTOPに戻ります";
                    PlayRouletteStopSound();
                }
                else
                {
                    StartRoulette();
                }
            }
        }

        private void StopRoulette()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= OnTimerTick;
                _timer = null;
            }

            _isRouletteRunning = false;
            _rouletteRunningSound.Stop();
        }

        private void ReturnToTop_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("本島に戻りますか？", "沖縄", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                StopRoulette();
                RoulettePanel.Visibility = Visibility.Collapsed;
                MainPanel.Visibility = Visibility.Visible;
            }
        }

        private void ReturnTOP_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
        }

        private void PlayRunSound()
        {
            _runSound.PlaySync();
        }

        private void PlayRouletteRunningSound()
        {
            _rouletteRunningSound.PlayLooping();
        }

        private void PlayRouletteStopSound()
        {
            _rouletteStopSound.Play();
        }

        // Register
        private void ReferenceButton_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                IsFolderPicker = true  // フォルダ選択モードにする
            };

            // ダイアログを表示する
            var ret = dialog.ShowDialog();
            if (ret == CommonFileDialogResult.Ok)
            {
                //選択されたフォルダ名をテキストボックスに設定する
                PrizePictureFolderTextBox.Text = dialog.FileName;

                Properties.Settings.Default.PrizePictureFolderPath = PrizePictureFolderTextBox.Text;
                Properties.Settings.Default.Save();
            }
        }
    }
}
