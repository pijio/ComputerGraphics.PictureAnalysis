using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ComputerGraphics.PictureAnalysis.App
{
    public static class AreaAnalyzer
    {
        /// <summary>
        /// Расчет дискретный центральных моментов областей
        /// </summary>
        /// <param name="areas"></param>
        /// <param name="centersOfMass"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static List<int> DiscreteCentralMoment(Dictionary<int, LinkedList<int[]>> areas,
            List<Tuple<int, int, int>> centersOfMass, int i, int j)
        {
            var result = new List<int>(areas.Keys.Count);
            foreach (var area in areas)
            {
                // area mass center
                var amc = centersOfMass.Where(x => x.Item1 == area.Key)
                    .Select(x => new Tuple<int, int>(x.Item2, x.Item3)).FirstOrDefault();
                if (amc == null) continue;

                var dms = DiscreteCentralMoment(area.Value, amc, i, j);
                result.Add(dms);
            }
            return result;
        }

        /// <summary>
        /// Расчет дискретного центрального момента одной области
        /// </summary>
        /// <param name="area"></param>
        /// <param name="centerOfMass"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static int DiscreteCentralMoment(LinkedList<int[]> area, Tuple<int, int> centerOfMass, int i, int j)
        {
            var amc = new { X = centerOfMass.Item1, Y = centerOfMass.Item2 };

            var sum = (int)area.AsParallel().Sum(p => Math.Pow(p[0] - amc.X, i) * Math.Pow(p[1] - amc.Y, j));

            return sum;
        }

        /// <summary>
        /// Центры масс связанных областей
        /// </summary>
        /// <param name="areas"></param>
        /// <returns></returns>
        public static List<Tuple<int, int, int>> CentersOfMass(Dictionary<int, LinkedList<int[]>> areas)
        {
            var result = new List<Tuple<int, int, int>>(areas.Keys.Count);
            foreach (var group in areas)
            {
                var com = CenterOfMass(group.Value);
                result.Add(new Tuple<int, int, int>(group.Key, com.Item1, com.Item2));
            }

            return result;
        }

        /// <summary>
        /// Центр масс одной из областей
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public static Tuple<int, int> CenterOfMass(LinkedList<int[]> area)
        {
            int currentSumX = area.AsParallel().Sum(x => x[0]);
            int currentSumY = area.AsParallel().Sum(x => x[1]);

            var centerMassX = (int)Math.Round((double)currentSumX / area.Count, 0);
            var centerMassY = (int)Math.Round((double)currentSumY / area.Count, 0);
            return new Tuple<int, int>(centerMassX, centerMassY);
        }
        
        /// <summary>
        /// Ориентация главной оси инерции (в радианах)
        /// </summary>
        /// <param name="dmc11"></param>
        /// <param name="dmc20"></param>
        /// <param name="dmc02"></param>
        /// <returns></returns>

        public static double MainAxisOrientation(double dmc11, double dmc20, double dmc02)
        {
            return 1d / 2 * Math.Atan2(2 * dmc11, (dmc20 - dmc02));
        }

        /// <summary>
        /// Вытянутость области
        /// </summary>
        /// <param name="dmc11"></param>
        /// <param name="dmc20"></param>
        /// <param name="dmc02"></param>
        /// <returns></returns>

        public static double AreaElongation(double dmc11, double dmc20, double dmc02)
        {
            double chislitel = dmc20 + dmc02 + Math.Sqrt(Math.Pow(dmc20 - dmc02, 2) + 4 * Math.Pow(dmc11, 2));
            double znamenatel = dmc20 + dmc02 - Math.Sqrt(Math.Pow(dmc20 - dmc02, 2) + 4 * Math.Pow(dmc11, 2));
            return chislitel / znamenatel;
        }
    }
}