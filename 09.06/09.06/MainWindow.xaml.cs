using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorsApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Список цветов (имена соответствуют статическим свойствам Colors)
            string[] colorNames = new string[]
            {
                "Navy", "Blue", "Aqua", "Teal", "Olive", "Green", "Lime",
                "Yellow", "Orange", "Red", "Maroon", "Fuchsia", "Purple",
                "Black", "Silver", "Gray", "White"
            };

            foreach (string name in colorNames)
            {
                // Получаем цвет из системной палитры
                Color color = (Color)typeof(Colors).GetProperty(name).GetValue(null);
                Brush brush = new SolidColorBrush(color);

                Button btn = new Button
                {
                    Content = name,
                    Foreground = brush,
                    Margin = new Thickness(2),
                    Padding = new Thickness(5, 2, 5, 2),
                    FontWeight = FontWeights.Bold,
                    // Для светлых цветов (например, White) можно добавить тёмную рамку
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1)
                };

                ButtonsPanel.Children.Add(btn);
            }
        }
    }
}