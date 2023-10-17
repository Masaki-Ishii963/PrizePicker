using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
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
        private Random _random = new();
        private List<string> _remainingImages;
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
            MainPanel.Visibility = Visibility.Collapsed;
            RoulettePanel.Visibility = Visibility.Visible;

            InitializeRoulette();
        }

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

            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            return Directory.GetFiles(folderPath).Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower())).ToList();
        }

        private void StartRoulette()
        {
            if (_remainingImages.Count == 0)
            {
                return;
            }

            _isRouletteRunning = true;
            PlayRunSound();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1.0 / 24); // 1/24秒ごと
            _timer.Tick += OnTimerTick;
            _timer.Start();
            PlayRouletteRunningSound();
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
                    PlayRunSound();
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
            _rouletteRunningSound.Stop();
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
                PlayRouletteStopSound();
            }
        }

        private void ReturnToTop_Click(object sender, RoutedEventArgs e)
        {
            StopRoulette();

            RoulettePanel.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
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
