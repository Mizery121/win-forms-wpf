using System.Windows;

namespace Questionnaire
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tbResult.Text =
                "Имя: " + tbName.Text + "\n" +
                "Возраст: " + tbAge.Text + "\n" +
                "Город: " + tbCity.Text;
        }
    }
}
