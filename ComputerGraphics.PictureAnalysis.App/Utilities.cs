using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ComputerGraphics.PictureAnalysis.App
{
    /// <summary>
    /// Класс различных математических утилит
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Количество знаков после запятой при округлении при получении гистограммы
        /// </summary>
        private const int _afterDot = 5;

        /// <summary>
        /// Получение гистограммы изображения по параметру L из пространства HSL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Dictionary<double, int> GetGistagram(IEnumerable<double> source)
        {
            var result = new Dictionary<double, int>();
            foreach (var item in source)
            {
                try
                {
                    var itemRound = Math.Round(item, _afterDot);
                    if (result.ContainsKey(itemRound))
                    {
                        result[itemRound]++;
                    }
                    else
                    {
                        result.Add(itemRound, 1);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return result;
        }

        /// <summary>
        /// Гауссово ядро для сглаживания гистограммы по алгоритму Керна
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double GaussianKernel(double x)
        {
            return 1d / Math.Sqrt(2 * Math.PI) * Math.Exp(-0.5 * x * x);
        }

        /// <summary>
        /// Сглаживание гистограммы по методу Керна с Гауссовым ядром в качестве функции-ядра
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bandwidth"></param>
        /// <returns></returns>
        public static Dictionary<double, int> KernelSmooth(Dictionary<double, int> data, double bandwidth)
        {
            double[] x = data.Keys.ToArray();
            int[] y = data.Values.ToArray();

            int n = x.Length;
            double[] smoothedY = new double[n];
            for (int i = 0; i < n; i++)
            {
                double[] weights = new double[n];
                for (int j = 0; j < n; j++)
                {
                    weights[j] = GaussianKernel((x[j] - x[i]) / bandwidth);
                }
                double sumWeights = weights.Sum();
                smoothedY[i] = weights.Zip(y, (a, b) => a * b).Sum() / sumWeights;
            }

            var smoothedData = new Dictionary<double, int>();
            for (int i = 0; i < x.Length; i++)
            {
                smoothedData.Add(x[i], (int)Math.Round(smoothedY[i], 0));
            }

            return smoothedData;
        }

        /// <summary>
        /// Среднеквадратичное отклонение коллекции чисел
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double StandartDeviation(ICollection<double> target)
        {
            var average = target.Average();
            return Math.Sqrt(target.Select(x => Math.Pow(x - average, 2)).Sum() / target.Count);
        }

        /// <summary>
        /// Метод Шотта для выбора оптимальной ширины ядра
        /// </summary>
        /// <returns></returns>
        public static double ScottMethodCoreWidth(double deviation, double count)
        {
            return 3.5 * deviation * Math.Pow(count, -1d / 3);
        }

        /// <summary>
        /// Порог яркости для бинаризации изображения
        /// </summary>
        /// <param name="gist"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public static double BinarizationThreshold(Dictionary<double, int> gist, double percentage = 0.05)
        {
            var hmax = gist.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;

            int total = gist.Where(kvp => kvp.Key >= hmax).Sum(kvp => kvp.Value);
            int threshold = (int)(total * percentage);

            int count = 0;
            double brightness = hmax;
            foreach (var kvp in gist.OrderByDescending(kvp => kvp.Key))
            {
                if (kvp.Key < hmax) break;
                count += kvp.Value;
                if (count >= threshold)
                {
                    brightness = kvp.Key;
                    break;
                }
            }

            return Math.Abs(hmax - (brightness - hmax));
        }

        /// <summary>
        /// Проверяем, лежит ли точка на линии либо близка к ней
        /// </summary>
        /// <param name="lineDot"></param>
        /// <param name="dotForVerify"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static bool LocateOnLine(Point lineDot, Point dotForVerify, double angle)
        {
            var x1 = lineDot.X; // координата x начальной точки луча
            var y1 = lineDot.Y; // координата y начальной точки луча
            var x2 = dotForVerify.X; // координата x проверяемой точки
            var y2 = dotForVerify.Y; // координата y проверяемой точки
            var deviation = 20d * (Math.PI / 180.0); // максимальный допустимый угол отклонения

            double distanceX = (x2 - x1) * Math.Cos(angle) + (y2 - y1) * Math.Sin(angle);
            double distanceY = (y2 - y1) * Math.Cos(angle) - (x2 - x1) * Math.Sin(angle);
            double distance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            return Math.Abs(distanceY / distance) <= Math.Tan(deviation);
        }

        /// <summary>
        /// Расстояние от точки до точки
        /// </summary>
        /// <param name="dot"></param>
        /// <param name="dotForVerify"></param>
        /// <returns></returns>
        public static double DistanceToPoint(Point dot, Point dotForVerify)
        {
            return Math.Sqrt(
                Math.Pow(dot.X - dotForVerify.X, 2) +
                Math.Pow(dot.Y - dotForVerify.Y, 2));
        }
    }
}