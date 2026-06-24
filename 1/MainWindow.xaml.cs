using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace TextCounter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                string text = File.ReadAllText(dialog.FileName);

                progressBar.Value = 100;

                resultText.Text = $"Количество символов: {text.Length}";
            }
        }
    }
}
