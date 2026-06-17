using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KeyboardTrainer
{
    public partial class MainWindow : Window
    {
        string taskText = "";
        string userText = "";

        int errors = 0;

        Stopwatch watch = new Stopwatch();

        Dictionary<Key, Button> keyboard =
            new Dictionary<Key, Button>();

        Random rnd = new Random();

        public MainWindow()
        {
            InitializeComponent();

            keyboard.Add(Key.Q, Q);
            keyboard.Add(Key.W, W);
            keyboard.Add(Key.E, E);
            keyboard.Add(Key.R, R);
            keyboard.Add(Key.T, T);
            keyboard.Add(Key.Y, Y);
            keyboard.Add(Key.U, U);
            keyboard.Add(Key.I, I);
            keyboard.Add(Key.O, O);
            keyboard.Add(Key.P, P);

            keyboard.Add(Key.A, A);
            keyboard.Add(Key.S, S);
            keyboard.Add(Key.D, D);
            keyboard.Add(Key.F, F);
            keyboard.Add(Key.G, G);
            keyboard.Add(Key.H, H);
            keyboard.Add(Key.J, J);
            keyboard.Add(Key.K, K);
            keyboard.Add(Key.L, L);

            keyboard.Add(Key.Z, Z);
            keyboard.Add(Key.X, X);
            keyboard.Add(Key.C, C);
            keyboard.Add(Key.V, V);
            keyboard.Add(Key.B, B);
            keyboard.Add(Key.N, N);
            keyboard.Add(Key.M, M);

            btnStop.IsEnabled = false;
        }

        private string GenerateText(int len)
        {
            string chars;

            if (cbCase.IsChecked == true)
                chars =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            else
                chars =
                    "abcdefghijklmnopqrstuvwxyz";

            string result = "";

            for (int i = 0; i < len; i++)
            {
                result += chars[rnd.Next(chars.Length)];
            }

            return result;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            taskText =
                GenerateText((int)slLength.Value);

            txtTask.Text = taskText;

            userText = "";

            txtInput.Text = "";

            errors = 0;

            txtErrors.Text = "0";

            txtSpeed.Text = "0";

            watch.Restart();

            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            watch.Stop();

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private void UpdateKeyboard()
        {
            bool upper =
                Keyboard.IsKeyToggled(Key.CapsLock)
                ^ Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

            foreach (var key in keyboard)
            {
                string text = key.Key.ToString();

                key.Value.Content =
                    upper
                    ? text.ToUpper()
                    : text.ToLower();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateKeyboard();

            if (keyboard.ContainsKey(e.Key))
            {
                keyboard[e.Key].Background =
                    Brushes.Yellow;
            }

            if (!btnStop.IsEnabled)
                return;

            string key = e.Key.ToString();

            if (key.Length != 1)
                return;

            bool shift =
                Keyboard.Modifiers.HasFlag(
                    ModifierKeys.Shift);

            bool caps =
                Keyboard.IsKeyToggled(
                    Key.CapsLock);

            if (shift ^ caps)
                key = key.ToUpper();
            else
                key = key.ToLower();

            userText += key;

            txtInput.Text = userText;

            int pos = userText.Length - 1;

            if (pos < taskText.Length)
            {
                bool ok;

                if (cbCase.IsChecked == true)
                {
                    ok =
                        taskText[pos].ToString()
                        == key;
                }
                else
                {
                    ok =
                        taskText[pos]
                        .ToString()
                        .ToUpper()
                        ==
                        key.ToUpper();
                }

                if (!ok)
                {
                    errors++;

                    txtErrors.Text =
                        errors.ToString();
                }
            }

            if (watch.Elapsed.TotalMinutes > 0)
            {
                double speed =
                    userText.Length /
                    watch.Elapsed.TotalMinutes;

                txtSpeed.Text =
                    ((int)speed).ToString();
            }

            if (userText.Length >= taskText.Length)
            {
                watch.Stop();

                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;

                MessageBox.Show(
                    "Тренировка завершена!\n\n" +
                    "Ошибок: " + errors +
                    "\nСкорость: " + txtSpeed.Text +
                    " симв/мин");
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateKeyboard();

            if (keyboard.ContainsKey(e.Key))
            {
                keyboard[e.Key].Background =
                    Brushes.LightGray;
            }
        }
    }
}
