using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;

namespace PrizePicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
            }
        }

        private void ReturnTOP_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
        }
    }
}
