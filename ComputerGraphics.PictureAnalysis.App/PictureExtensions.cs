using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;

namespace ComputerGraphics.PictureAnalysis.App
{
    /// <summary>
    /// Класс для методов расширения (и не только) для других классов представляющих картинки
    /// </summary>
    public static class PictureExtensions
    {
        /// <summary>
        /// Метод преобразования битмапа в лист матриц вещественного RGB
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<Matrix<double>> ToMatrixList(this Bitmap source)
        {
            var rect = new Rectangle(0, 0, source.Width, source.Height);
            var picData = source.LockBits(rect, ImageLockMode.ReadOnly, source.PixelFormat);

            var size = Math.Abs(picData.Stride) * source.Height;
            var rgbs = new byte[size];

            Marshal.Copy(picData.Scan0, rgbs, 0, size);

            source.UnlockBits(picData);
            var result = new List<Matrix<double>>(source.Width * source.Height);
            for (var i = 0; i < rgbs.Length / 3; i++)
            {
                result.Add(Matrix<double>.Build.DenseOfColumnArrays(new[]
                {
                    rgbs[i * 3 + 2] / 255d,
                    rgbs[i * 3 + 1] / 255d,
                    rgbs[i * 3] / 255d
                }));
            }
            return result;
        }

        /// <summary>
        /// Метод преобразования листа матриц вещественного RGB в битмап
        /// </summary>
        /// <param name="rgbSpace"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this ICollection<Matrix<double>> rgbSpace, int width, int height)
        {
            byte[] rgbs = new byte[rgbSpace.Count * 3];

            int byteIndex = 0;
            foreach (var color in rgbSpace)
            {
                rgbs[byteIndex++] = (byte)(color[2, 0] * 255.0); // red
                rgbs[byteIndex++] = (byte)(color[1, 0] * 255.0); // green
                rgbs[byteIndex++] = (byte)(color[0, 0] * 255.0); // blue
            }

            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            try
            {
                Marshal.Copy(rgbs, 0, bmpData.Scan0, rgbs.Length);
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            return bmp;
        }

        public static IEnumerable<Matrix<double>> RgbToHslMatrices(this List<Matrix<double>> source)
        {
            return source.Select(item => item.RgbPixelToHsl());
        }

        /// <summary>
        /// Пиксель rgb в hsl
        /// </summary>
        /// <returns></returns>
        public static Matrix<double> RgbPixelToHsl(this Matrix<double> rgb)
        {
            if (rgb.RowCount != 3 || rgb.ColumnCount != 1)
                throw new ArgumentException("Входная матрица должна быть размерностью 3х1.");

            double r = rgb[0, 0];
            double g = rgb[1, 0];
            double b = rgb[2, 0];
            double max = Math.Max(Math.Max(r, g), b);
            double min = Math.Min(Math.Min(r, g), b);
            double h = 0, s = 0, l = (max + min) / 2;

            const double eps = 0.00001;

            if (Math.Abs(max - min) < eps)
            {
                double d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
                if (Math.Abs(max - r) < eps) h = (g - b) / d + (g < b ? 6 : 0);
                else if (Math.Abs(max - g) < eps) h = (b - r) / d + 2;
                else if (Math.Abs(max - b) < eps) h = (r - g) / d + 4;
                h /= 6;
            }

            return Matrix<double>.Build.DenseOfArray(new[,] { { h }, { s }, { l } });
        }

        
        /// <summary>
        /// Пиксель rgb в hsl
        /// </summary>
        /// <returns></returns>
        public static Matrix<double> RgbPixelToHsl(this Color rgb)
        {

            var r = (double)rgb.R / 255;
            var g = (double)rgb.G / 255;
            var b = (double)rgb.B / 255;
            var max = Math.Max(Math.Max(r, g), b);
            var min = Math.Min(Math.Min(r, g), b);
            double h = 0, s = 0, l = (max + min) / 2;

            const double eps = 0.00001;

            if (Math.Abs(max - min) < eps)
            {
                var d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
                if (Math.Abs(max - r) < eps) h = (g - b) / d + (g < b ? 6 : 0);
                else if (Math.Abs(max - g) < eps) h = (b - r) / d + 2;
                else if (Math.Abs(max - b) < eps) h = (r - g) / d + 4;
                h /= 6;
            }

            return Matrix<double>.Build.DenseOfArray(new[,] { { h }, { s }, { l } });
        }

        /// <summary>
        /// Пиксель hsl в rgb
        /// </summary>
        /// <param name="hsl"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Matrix<double> HslPixelToRgb(this Matrix<double> hsl)
        {
            if (hsl.RowCount != 3 || hsl.ColumnCount != 1)
                throw new ArgumentException("Входная матрица должна быть размерностью 3х1.");

            double h = hsl[0, 0];
            double s = hsl[1, 0];
            double l = hsl[2, 0];


            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs(h * 6 % 2 - 1));
            double m = l - c / 2;
            double r, g, b;
            if (s == 0)
            {
                r = g = b = l;
                return Matrix<double>.Build.DenseOfArray(new[,]
                {
                    { (r) },
                    { (g) },
                    { (b) }
                });
            }
            if (h < 1.0 / 6.0)
            {
                r = c; g = x; b = 0;
            }
            else if (h < 2.0 / 6.0)
            {
                r = x; g = c; b = 0;
            }
            else if (h < 3.0 / 6.0)
            {
                r = 0; g = c; b = x;
            }
            else if (h < 4.0 / 6.0)
            {
                r = 0; g = x; b = c;
            }
            else if (h < 5.0 / 6.0)
            {
                r = x; g = 0; b = c;
            }
            else
            {
                r = c; g = 0; b = x;
            }

            return Matrix<double>.Build.DenseOfArray(new[,] {
                { (r + m) },
                { (g + m) },
                { (b + m) }
            });
        }
    }
}