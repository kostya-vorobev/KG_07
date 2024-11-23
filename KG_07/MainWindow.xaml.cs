using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KG_07
{
    public partial class MainWindow : Window
    {
        private Bitmap originalBitmap;
        private BitmapImage _originalImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _originalImage = new BitmapImage(new Uri(openFileDialog.FileName));
                OriginalImage.Source = _originalImage;
                originalBitmap = new Bitmap(openFileDialog.FileName); // Загружаем Bitmap для дальнейшего использования
            }
        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        private void NearestNeighbor_Click(object sender, RoutedEventArgs e)
        {
            double scale = ScaleSlider.Value;
            Bitmap scaledBitmap;
            scaledBitmap = ScaleBitmap(originalBitmap, scale, true);
            ScaledImage.Source = BitmapToImageSource(scaledBitmap);
        }

        private void Bilinear_Click(object sender, RoutedEventArgs e)
        {
            double scale = ScaleSlider.Value;
            Bitmap scaledBitmap;
            scaledBitmap = ScaleBitmap(originalBitmap, scale, false);
            ScaledImage.Source = BitmapToImageSource(scaledBitmap);
        }

        private Bitmap ScaleBitmap(Bitmap bitmap, double scale, bool nearestNeighbor)
        {
            int newWidth = (int)(bitmap.Width * scale);
            int newHeight = (int)(bitmap.Height * scale);
            Bitmap scaledBitmap = new Bitmap(newWidth, newHeight);
            var res = new Bitmap((int)(bitmap.Width * scale), (int)(bitmap.Height * scale));
            if (nearestNeighbor)
            {
                
                for (int x1 = 0; x1 < res.Width; x1++)
                    for (int y1 = 0; y1 < res.Height; y1++)
                        res.SetPixel(x1, y1, bitmap.GetPixel((int)(x1 / scale), (int)(y1 / scale)));
            }
            else
            {

                for (int x1 = 0; x1 < newWidth; x1++)
                {
                    for (int y1 = 0; y1 < newHeight; y1++)
                    {
                        // Вычислим исходные координаты
                        double x = (double)x1 / scale;
                        double y = (double)y1 / scale;

                        int x0 = (int)x;
                        int y0 = (int)y;

                        // Получаем значение пикселей из оригинального изображения
                        Color Q11 = bitmap.GetPixel(x0, y0);
                        Color Q12 = bitmap.GetPixel(x0, Math.Min(y0 + 1, bitmap.Height - 1));
                        Color Q21 = bitmap.GetPixel(Math.Min(x0 + 1, bitmap.Width - 1), y0);
                        Color Q22 = bitmap.GetPixel(Math.Min(x0 + 1, bitmap.Width - 1), Math.Min(y0 + 1, bitmap.Height - 1));

                        // Вычисляем расстояния в интервале
                        double xDist = x - x0;
                        double yDist = y - y0;

                        // Интерполяция по горизонтали
                        int r1 = (int)(Q11.R * (1 - xDist) + Q21.R * xDist);
                        int g1 = (int)(Q11.G * (1 - xDist) + Q21.G * xDist);
                        int b1 = (int)(Q11.B * (1 - xDist) + Q21.B * xDist);

                        int r2 = (int)(Q12.R * (1 - xDist) + Q22.R * xDist);
                        int g2 = (int)(Q12.G * (1 - xDist) + Q22.G * xDist);
                        int b2 = (int)(Q12.B * (1 - xDist) + Q22.B * xDist);

                        // Вертикальная интерполяция
                        int rFinal = (int)(r1 * (1 - yDist) + r2 * yDist);
                        int gFinal = (int)(g1 * (1 - yDist) + g2 * yDist);
                        int bFinal = (int)(b1 * (1 - yDist) + b2 * yDist);

                        // Устанавливаем пиксель
                        res.SetPixel(x1, y1, Color.FromArgb(Clamp(rFinal), Clamp(gFinal), Clamp(bFinal)));
                    }
                }

            }


            return res;
        }

        private int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = memory;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        private void Scale_Click(object sender, RoutedEventArgs e)
        {
            //UpdateScaledImage();
        }
    }
}
