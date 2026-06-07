using System;
using System.Runtime.InteropServices;
using static System.Console;

namespace MonitorDiagonal
{
    public class NativeMethods
    {
        // Получает контекст устройства для всего экрана
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        // Освобождает контекст устройства
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        // Получает параметры устройства (горизонтальный/вертикальный размер в мм)
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
    }

    // Константы для GetDeviceCaps
    public enum DeviceCaps
    {
        HORZSIZE = 4,   // физическая ширина экрана в миллиметрах
        VERTSIZE = 6    // физическая высота экрана в миллиметрах
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Получаем контекст устройства для рабочего стола
                IntPtr hdc = NativeMethods.GetDC(IntPtr.Zero);
                if (hdc == IntPtr.Zero)
                {
                    WriteLine("Не удалось получить контекст устройства.");
                    return;
                }

                // Получаем физические размеры экрана в миллиметрах
                int widthMm = NativeMethods.GetDeviceCaps(hdc, (int)DeviceCaps.HORZSIZE);
                int heightMm = NativeMethods.GetDeviceCaps(hdc, (int)DeviceCaps.VERTSIZE);

                // Освобождаем контекст
                NativeMethods.ReleaseDC(IntPtr.Zero, hdc);

                if (widthMm == 0 || heightMm == 0)
                {
                    WriteLine("Не удалось определить физические размеры экрана.");
                    return;
                }

                // Вычисляем диагональ в миллиметрах и переводим в дюймы (1 дюйм = 25.4 мм)
                double diagonalMm = Math.Sqrt(widthMm * widthMm + heightMm * heightMm);
                double diagonalInches = diagonalMm / 25.4;

                WriteLine($"Физическая ширина экрана: {widthMm} мм");
                WriteLine($"Физическая высота экрана: {heightMm} мм");
                WriteLine($"Диагональ монитора: {diagonalInches:F2} дюймов");
            }
            catch (Exception ex)
            {
                WriteLine($"Ошибка: {ex.Message}");
            }

            WriteLine("\nНажмите любую клавишу для выхода...");
            ReadKey();
        }
    }
}