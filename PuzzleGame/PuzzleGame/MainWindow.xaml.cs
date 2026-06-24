using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PuzzleGame
{
    public partial class MainWindow : Window
    {
        private const int GridSize = 4;
        private const int PieceSize = 100;
        private int totalPieces = GridSize * GridSize;

        private BitmapSource originalImage;
        private CroppedBitmap[] pieces;
        private int[] order;
        private Border[] borderControls;   // теперь храним Border, а не Image
        private Image[] imageControls;     // для доступа к изображениям внутри Border

        private int selectedIndex = -1;
        private int moves = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            originalImage = CreateSampleImage(PieceSize * GridSize, PieceSize * GridSize);

            pieces = new CroppedBitmap[totalPieces];
            for (int i = 0; i < GridSize; i++)
                for (int j = 0; j < GridSize; j++)
                {
                    int index = i * GridSize + j;
                    pieces[index] = new CroppedBitmap(originalImage,
                        new Int32Rect(j * PieceSize, i * PieceSize, PieceSize, PieceSize));
                }

            order = new int[totalPieces];
            for (int i = 0; i < totalPieces; i++) order[i] = i;
            ShuffleOrder();

            CreateGrid();

            moves = 0;
            MovesLabel.Text = "0";
            selectedIndex = -1;
        }

        private BitmapSource CreateSampleImage(int width, int height)
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                Rect rect = new Rect(0, 0, width, height);
                LinearGradientBrush bgBrush = new LinearGradientBrush(Colors.Red, Colors.Blue, 45);
                dc.DrawRectangle(bgBrush, null, rect);
                dc.DrawEllipse(Brushes.Yellow, new Pen(Brushes.Black, 3),
                               new Point(width / 2, height / 2), width / 3, height / 3);
                FormattedText ft = new FormattedText("ПАЗЛ",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial Black"),
                    60,
                    Brushes.White);
                dc.DrawText(ft, new Point(width / 2 - ft.Width / 2, height / 2 - ft.Height / 2));
                dc.DrawRectangle(null, new Pen(Brushes.Black, 4), rect);
            }
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            return rtb;
        }

        private void ShuffleOrder()
        {
            Random rnd = new Random();
            for (int i = 0; i < order.Length; i++)
            {
                int j = rnd.Next(i, order.Length);
                int temp = order[i];
                order[i] = order[j];
                order[j] = temp;
            }
            bool isSolved = true;
            for (int i = 0; i < order.Length; i++)
                if (order[i] != i) { isSolved = false; break; }
            if (isSolved)
                ShuffleOrder();
        }

        private void CreateGrid()
        {
            gameGrid.Children.Clear();
            gameGrid.RowDefinitions.Clear();
            gameGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < GridSize; i++)
            {
                gameGrid.RowDefinitions.Add(new RowDefinition());
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            borderControls = new Border[totalPieces];
            imageControls = new Image[totalPieces];

            for (int i = 0; i < totalPieces; i++)
            {
                // Создаём Border как контейнер для рамки
                Border border = new Border();
                border.BorderThickness = new Thickness(2);
                border.BorderBrush = Brushes.Transparent; // без рамки по умолчанию
                border.Margin = new Thickness(2);
                border.Tag = i; // позиция в order
                border.MouseLeftButtonDown += Border_MouseLeftButtonDown;

                // Создаём Image внутри Border
                Image img = new Image();
                img.Source = pieces[order[i]];
                img.Stretch = Stretch.Fill;
                border.Child = img;

                // Размещаем в сетке
                int row = i / GridSize;
                int col = i % GridSize;
                Grid.SetRow(border, row);
                Grid.SetColumn(border, col);
                gameGrid.Children.Add(border);

                borderControls[i] = border;
                imageControls[i] = img;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            if (border == null) return;

            int pos = (int)border.Tag;

            if (selectedIndex == -1)
            {
                // Выделяем: красная рамка
                border.BorderBrush = Brushes.Red;
                border.BorderThickness = new Thickness(4);
                selectedIndex = pos;
            }
            else if (selectedIndex == pos)
            {
                // Снимаем выделение
                border.BorderBrush = Brushes.Transparent;
                border.BorderThickness = new Thickness(2);
                selectedIndex = -1;
            }
            else
            {
                // Меняем местами два кусочка
                // Убираем выделение со старого
                borderControls[selectedIndex].BorderBrush = Brushes.Transparent;
                borderControls[selectedIndex].BorderThickness = new Thickness(2);

                // Меняем порядок в массиве order
                int temp = order[selectedIndex];
                order[selectedIndex] = order[pos];
                order[pos] = temp;

                // Обновляем изображения
                imageControls[selectedIndex].Source = pieces[order[selectedIndex]];
                imageControls[pos].Source = pieces[order[pos]];

                moves++;
                MovesLabel.Text = moves.ToString();

                // Проверка победы
                bool solved = true;
                for (int i = 0; i < order.Length; i++)
                    if (order[i] != i) { solved = false; break; }
                if (solved)
                {
                    MessageBox.Show("Поздравляем! Пазл собран!", "Победа", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                selectedIndex = -1;
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            ShuffleOrder();
            for (int i = 0; i < totalPieces; i++)
            {
                imageControls[i].Source = pieces[order[i]];
            }
            moves = 0;
            MovesLabel.Text = "0";
            selectedIndex = -1;
            // Сбрасываем рамки у всех Border
            foreach (var b in borderControls)
            {
                b.BorderBrush = Brushes.Transparent;
                b.BorderThickness = new Thickness(2);
            }
        }
    }
}