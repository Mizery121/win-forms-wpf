using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfWASD
{
    public partial class MainWindow : Window
    {
        private readonly Random _random = new Random();
        private string _targetText = "";
        private int _currentIndex = 0;
        private int _correctCount = 0;
        private int _errorCount = 0;
        private DateTime _startTime;
        private bool _isActive = false;
        private readonly Dictionary<char, Border> _keyBorders;

        public MainWindow()
        {
            InitializeComponent();

            // Привязка визуальных элементов клавиш к символам
            _keyBorders = new Dictionary<char, Border>
            {
                { 'w', KeyW },
                { 'a', KeyA },
                { 's', KeyS },
                { 'd', KeyD }
            };

            // Обновление надписи длины при изменении ползунка
            LengthSlider.ValueChanged += (s, e) => LengthLabel.Content = LengthSlider.Value;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Генерируем строку из букв W, A, S, D заданной длины
            int length = (int)LengthSlider.Value;
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = "wasd"[_random.Next(4)];
            }
            _targetText = new string(chars);
            TargetTextBlock.Text = _targetText;

            // Сброс состояния
            _currentIndex = 0;
            _correctCount = 0;
            _errorCount = 0;
            CorrectLabel.Content = "0";
            ErrorsLabel.Content = "0";
            SpeedLabel.Content = "0 зн/мин";
            _startTime = DateTime.Now;
            _isActive = true;

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            LengthSlider.IsEnabled = false;

            // Сброс подсветки клавиш
            foreach (var border in _keyBorders.Values)
                border.Background = Brushes.LightGray;

            // Фокус на окно для приёма клавиатурных событий
            this.Focus();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _isActive = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            LengthSlider.IsEnabled = true;
            TargetTextBlock.Text = "";
            foreach (var border in _keyBorders.Values)
                border.Background = Brushes.LightGray;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isActive) return;

            // Преобразуем нажатую клавишу в символ (только W, A, S, D, регистр не важен)
            char pressed = char.ToLowerInvariant(KeyToChar(e.Key));
            if (!"wasd".Contains(pressed)) return; // игнорируем другие клавиши

            // Подсветка нажатой клавиши
            if (_keyBorders.TryGetValue(pressed, out var border))
                border.Background = Brushes.LightBlue;

            // Проверка
            if (_currentIndex >= _targetText.Length)
            {
                // Если строка уже закончена, игнорируем ввод
                return;
            }

            char expected = _targetText[_currentIndex];
            if (pressed == expected)
            {
                _correctCount++;
                CorrectLabel.Content = _correctCount;
                // Можно сделать зелёную подсветку на мгновение
                border.Background = Brushes.LightGreen;
            }
            else
            {
                _errorCount++;
                ErrorsLabel.Content = _errorCount;
                border.Background = Brushes.LightCoral;
            }

            _currentIndex++;

            // Обновление скорости (зн/мин)
            double minutes = (DateTime.Now - _startTime).TotalMinutes;
            if (minutes > 0)
            {
                int speed = (int)(_correctCount / minutes);
                SpeedLabel.Content = speed + " зн/мин";
            }

            // Если все символы введены
            if (_currentIndex >= _targetText.Length)
            {
                _isActive = false;
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                LengthSlider.IsEnabled = true;
                // Можно показать финальное сообщение
                MessageBox.Show($"Поздравляем! Вы ввели все символы.\nОшибок: {_errorCount}\nСкорость: {SpeedLabel.Content}", "Завершено");
            }
        }

        // Вспомогательный метод для преобразования Key в символ
        private char KeyToChar(Key key)
        {
            switch (key)
            {
                case Key.W: return 'W';
                case Key.A: return 'A';
                case Key.S: return 'S';
                case Key.D: return 'D';
                default: return '\0';
            }
        }

        // Можно также обрабатывать отпускание клавиш для сброса подсветки
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            char pressed = char.ToLowerInvariant(KeyToChar(e.Key));
            if (_keyBorders.TryGetValue(pressed, out var border))
            {
                // Возвращаем цвет в зависимости от правильности (можно сделать просто серый)
                border.Background = Brushes.LightGray;
            }
        }
    }
}