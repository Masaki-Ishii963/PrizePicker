using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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
        private List<string> _imagePaths;
        private Random _random = new();
        private List<string> _remainingImages;
        private bool _isRouletteRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.PrizePictureFolderPath))
            {
                PrizePictureFolderTextBox.Text = Properties.Settings.Default.PrizePictureFolderPath;
            }
        }

        // Main
        private void Setting_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Register_Button_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            RoulettePanel.Visibility = Visibility.Visible;

            InitializeRoulette();
        }

        // アプリ起動時 or ルーレット開始時
        private void InitializeRoulette()
        {
            _remainingImages = GetAllPrizeImages(PrizePictureFolderTextBox.Text);
            MessageLabel.Content = "クリックしてください";
            StartRoulette();
        }

        private List<string> GetAllPrizeImages(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                return new List<string>();
            }

            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" }; // 他の形式も必要に応じて追加可能
            var files = Directory.GetFiles(folderPath).Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower())).ToList();

            return files;
        }

        private void StartRoulette()
        {
            if (_remainingImages.Count == 0)
            {
                return;
            }

            _isRouletteRunning = true;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1.0 / 24); // 1/24秒ごと
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            var randomImagePath = _remainingImages[_random.Next(_remainingImages.Count)];

            // Assuming you have an Image control named "RouletteImage"
            PrizeImage.Source = new BitmapImage(new Uri(randomImagePath));
        }

        private void PrizeImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isRouletteRunning)
            {
                // ルーレットが動いている場合は、ルーレットを停止し、画像を表示する
                StopRoulette();

                // ランダムに画像を選び、それをリストから削除
                var selectedImage = SelectRandomImage(_remainingImages);
                _remainingImages.Remove(selectedImage);

                DisplayImage(selectedImage);

                if (_remainingImages.Count == 1)
                {
                    MessageLabel.Content = "次がラストの景品です。クリックしてください";
                }
                else
                {
                    MessageLabel.Content = "クリックしてください";
                }
            }
            else
            {
                // ルーレットが動いていない場合は、ルーレットを開始する
                if (_remainingImages.Count == 0)
                {
                    // すべての画像が表示された後
                    RoulettePanel.Visibility = Visibility.Collapsed;
                    MainPanel.Visibility = Visibility.Visible;
                    return;
                }
                else if (_remainingImages.Count == 1)
                {
                    // 1枚だけ残っている場合は、それを表示する
                    DisplayImage(_remainingImages[0]);
                    _remainingImages.Clear();
                    MessageLabel.Content = "ラストの景品です。クリックするとTOPに戻ります";
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
        }

        private string SelectRandomImage(List<string> images)
        {
            var random = new Random();
            var randomIndex = random.Next(images.Count);
            return images[randomIndex];
        }

        private void DisplayImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();

                PrizeImage.Source = bitmap;
            }
        }

        private void ReturnToTop_Click(object sender, RoutedEventArgs e)
        {
            StopRoulette();

            RoulettePanel.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
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

        private void ReturnTOP_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
        }
    }
}
