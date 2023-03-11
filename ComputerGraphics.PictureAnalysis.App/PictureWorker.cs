﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ComputerGraphics.PictureAnalysis.App
{
    /// <summary>
    /// Класс предназначенный для работы с изображениями (методы улучшения, очистки и т.д)
    /// </summary>
    public static partial class PictureWorker
    {
        private static Random _rand = new Random();

        /// <summary>
        /// Метод бинаризации (для HSL пространства)
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static List<Matrix<double>> BinarizePicture(List<Matrix<double>> image, double threshold)
        {
            var newPicture = new List<Matrix<double>>(image.Count);
            var blackHsl = Matrix<double>.Build.DenseOfColumnArrays(new[] { 0d, 0d, 0d });
            var whiteHsl = Matrix<double>.Build.DenseOfColumnArrays(new[] { 0d, 0d, 1d });
            foreach (var pixel in image)
            {
                if (pixel[2, 0] < threshold) 
                    newPicture.Add(blackHsl);
                if (pixel[2, 0] >= threshold) 
                    newPicture.Add(whiteHsl);
            }
            return newPicture;
        }

        /// <summary>
        /// Метод для раскраски выбранных областей
        /// </summary>
        /// <returns></returns>
        public static Bitmap ColorizeSelectedAreas(int[,] areas)
        {

            var colorsDict = new ConcurrentDictionary<int, Color>();

            var listGroups = areas.Cast<int>().ToArray();
            const int colorsCount = 40;
            var colors = new ConcurrentBag<Color>(GenerateColors(colorsCount));
            listGroups.AsParallel().ForAll(x =>
            {
                if (x == 0 || colorsDict.ContainsKey(x)) return;
                bool res = colors.TryTake(out var color);
                if (res)
                {
                    colorsDict.TryAdd(x, color);
                }
            });

            //добавим черный для необьектов
            colorsDict.TryAdd(0, Color.Black);

            var selectedBitmap = new Bitmap(areas.GetLength(0), areas.GetLength(1));
            for (var y = 0; y < selectedBitmap.Height; y++)
            {
                for (int x = 0; x < selectedBitmap.Width; x++)
                {
                    var objectNo = areas[x, y];
                    var color = colorsDict[objectNo];
                    selectedBitmap.SetPixel(x, y, color);
                }
            }
            return selectedBitmap;
        }

        public static int[,] SelectRelatedAreas(Bitmap binarizedSource)
        {
            var areas = new int[binarizedSource.Width, binarizedSource.Height];
            for (var y = 0; y < binarizedSource.Height; y++)
            {
                for (int x = 0; x < binarizedSource.Width; x++)
                {
                    areas[x, y] = 0;
                }
            }
            int group = 1;
            for (var y = 0; y < binarizedSource.Height; y++)
            {
                for (int x = 0; x < binarizedSource.Width; x++)
                {
                    Fill(binarizedSource, areas, x, y, group++);
                }
            }
            return areas;
        }

        private static void Fill(Bitmap bitmap, int[,] areas, int x, int y, int group)
        {
            var pixel = bitmap.GetPixel(x, y);
            if (pixel.IsWhite() && areas[x, y] == 0)
            {
                areas[x, y] = group;
                if (x > 0)
                {
                    Fill(bitmap, areas, x - 1, y, group);
                }

                if (x < bitmap.Width - 1)
                {
                    Fill(bitmap, areas, x + 1, y, group);
                }

                if (y < bitmap.Height - 1)
                {
                    Fill(bitmap, areas, x, y + 1, group);
                }

                if (y > 0)
                {
                    Fill(bitmap, areas, x, y - 1, group);
                }
            }
        }

        private static bool IsWhite(this Color color)
        {
            return color.R == 255 && color.G == 255 && color.B == 255;
        }

        /// <summary>
        /// Выделяем области и получаем информацию о всех пикселях в области
        /// </summary>
        /// <param name="areas"></param>
        /// <returns></returns>
        public static Dictionary<int, LinkedList<int[]>> ExcludeBackgroundFromPicture(int[,] areas)
        {
            var groupsInfo = new Dictionary<int, LinkedList<int[]>>();
            for (int y = 0; y < areas.GetLength(1); y++)
            {
                for (int x = 0; x < areas.GetLength(0); x++)
                {
                    if (areas[x, y] == 0) continue;
                    if (groupsInfo.ContainsKey(areas[x, y]))
                    {
                        groupsInfo[areas[x, y]].AddLast(new[] { x, y });
                    }
                    else
                    {
                        groupsInfo.Add(areas[x, y], new LinkedList<int[]>());
                        groupsInfo[areas[x, y]].AddLast(new[] { x, y });
                    }
                }
            }

            return groupsInfo;
        }

        public static List<int> DiscreteCentralMoment(Dictionary<int, LinkedList<int[]>> areas,
            List<Tuple<int, int, int>> centersOfMass, int i, int j)
        {
            var result = new List<int>(areas.Keys.Count);
            foreach (var area in areas)
            {
                // area mass center
                var amc = centersOfMass.Where(x => x.Item1 == area.Key)
                    .Select(p => new { X = p.Item2, Y = p.Item3}).FirstOrDefault();
                if(amc == null) continue;

                var sum = (int)area.Value.AsParallel().Sum(p => Math.Pow(p[0] - amc.X, i) * Math.Pow(p[1] - amc.Y, j));
                result.Add(sum);
            }
            return result;
        }

        /// <summary>
        /// Центры масс связанных областей
        /// </summary>
        /// <param name="areas"></param>
        /// <returns></returns>
        public static List<Tuple<int, int, int>> CenterOfMass(int[,] areas)
        {
            var groupsInfo = ExcludeBackgroundFromPicture(areas);
            var result = new List<Tuple<int, int, int>>(groupsInfo.Keys.Count);
            int currentSumX = 0, currentSumY = 0;
            foreach (var group in groupsInfo)
            {
                foreach (var pixels in group.Value)
                {
                    currentSumX += pixels[0];
                    currentSumY += pixels[1];
                }

                var centerMassX = (int)Math.Round((double)currentSumX / group.Value.Count, 0);
                var centerMassY = (int)Math.Round((double)currentSumY / group.Value.Count, 0);
                result.Add(new Tuple<int, int, int>(group.Key, centerMassX, centerMassY));
                currentSumX = 0;
                currentSumY = 0;
            }

            return result;
        }

        public static IEnumerable<Color> GenerateColors(int count)
        {
            HashSet<Color> colors = new HashSet<Color>();

            while (colors.Count < count)
            {
                Color color = Color.FromArgb(_rand.Next(256), _rand.Next(256), _rand.Next(256));
                colors.Add(color);
            }

            return colors;
        }
    }
}
