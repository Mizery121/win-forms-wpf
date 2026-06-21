using System;
using System.Windows;
using System.Windows.Controls;

namespace CalculatorApp
{
    public partial class MainWindow : Window
    {
        private string currentNumber = "0";
        private string previousNumber = "";
        private string operation = "";
        private bool isNewNumber = true; // флаг, что следующая цифра начнёт новое число

        public MainWindow()
        {
            InitializeComponent();
            UpdateCurrentDisplay();
        }

        // Обработчик для цифр (0-9)
        private void ButtonDigit_Click(object sender, RoutedEventArgs e)
        {
            string digit = ((Button)sender).Content.ToString();

            if (isNewNumber)
            {
                currentNumber = digit;
                isNewNumber = false;
            }
            else
            {
                // Не допускаем ведущие нули без десятичной точки
                if (currentNumber == "0" && digit != ".")
                    currentNumber = digit;
                else
                    currentNumber += digit;
            }
            UpdateCurrentDisplay();
        }

        // Обработчик для десятичной точки
        private void ButtonDot_Click(object sender, RoutedEventArgs e)
        {
            if (!currentNumber.Contains("."))
            {
                if (isNewNumber)
                {
                    currentNumber = "0.";
                    isNewNumber = false;
                }
                else
                {
                    currentNumber += ".";
                }
                UpdateCurrentDisplay();
            }
        }

        // Обработчик для операций (+, -, *, /)
        private void ButtonOperation_Click(object sender, RoutedEventArgs e)
        {
            string op = ((Button)sender).Content.ToString();

            if (!string.IsNullOrEmpty(operation) && !isNewNumber)
            {
                // Выполняем предыдущую операцию перед сохранением новой
                Calculate();
            }

            // Сохраняем текущее число как предыдущее, запоминаем операцию
            previousNumber = currentNumber;
            operation = op;
            isNewNumber = true;

            // Отображаем выражение
            ExpressionBox.Text = $"{previousNumber} {operation}";
        }

        // Обработчик "="
        private void ButtonEqual_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(operation) && !isNewNumber)
            {
                Calculate();
                operation = "";
                previousNumber = "";
                ExpressionBox.Text = "";
                isNewNumber = true;
            }
        }

        // Общая функция вычисления
        private void Calculate()
        {
            double prev = double.Parse(previousNumber);
            double curr = double.Parse(currentNumber);
            double result = 0;

            switch (operation)
            {
                case "+": result = prev + curr; break;
                case "-": result = prev - curr; break;
                case "*": result = prev * curr; break;
                case "/": result = prev / curr; break;
            }

            currentNumber = result.ToString();
            UpdateCurrentDisplay();
            // Обновляем выражение для отображения результата
            ExpressionBox.Text = $"{previousNumber} {operation} {curr} =";
        }

        // CE – очистить текущее число
        private void ButtonCE_Click(object sender, RoutedEventArgs e)
        {
            currentNumber = "0";
            isNewNumber = true;
            UpdateCurrentDisplay();
        }

        // C – очистить всё
        private void ButtonC_Click(object sender, RoutedEventArgs e)
        {
            currentNumber = "0";
            previousNumber = "";
            operation = "";
            isNewNumber = true;
            ExpressionBox.Text = "";
            UpdateCurrentDisplay();
        }

        // Backspace (<) – удалить последний символ
        private void ButtonBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (!isNewNumber && currentNumber.Length > 1)
            {
                currentNumber = currentNumber.Substring(0, currentNumber.Length - 1);
                UpdateCurrentDisplay();
            }
            else
            {
                currentNumber = "0";
                isNewNumber = true;
                UpdateCurrentDisplay();
            }
        }

        private void UpdateCurrentDisplay()
        {
            CurrentBox.Text = currentNumber;
        }
    }
}