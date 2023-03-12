using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;

namespace ComputerGraphics.PictureAnalysis.App.Areas
{
    public class WayArrow : BaseArea
    {
        public bool IsStartArrow => Type == AreaType.StartArrow;

        public double Angle { get; set; }

        /// <summary>
        /// Установленный опытным путем критерий удлиненности всех стрелок
        /// </summary>
        private static readonly (double, double) ElongCriterion = (3.7d, 4.2d);

        /// <summary>
        /// Установленный опытным путем критерий яркости путевых стрелок
        /// </summary>
        private static readonly (double, double) BrightnessCriterion = (0.92d, 1d);

        protected WayArrow(int x, int y)
        {
            CenterOfMass = new Point(x, y);
        }

        /// <summary>
        /// Расчет угла главной оси инерции стрелки
        /// </summary>
        /// <param name="area"></param>
        /// <param name="cetnerOfMass"></param>
        /// <returns></returns>
        protected static double CalculateAngle(LinkedList<int[]> area, Tuple<int, int> cetnerOfMass)
        {
            var dcm11 = AreaAnalyzer.DiscreteCentralMoment(area, cetnerOfMass, 1, 1);
            var dcm20 = AreaAnalyzer.DiscreteCentralMoment(area, cetnerOfMass, 2, 0);
            var dcm02 = AreaAnalyzer.DiscreteCentralMoment(area, cetnerOfMass, 0, 2);

            return AreaAnalyzer.MainAxisOrientation(dcm11, dcm20, dcm02);
        }

        /// <summary>
        /// Метод который формирует экземпляр области если он принадлежит к классу
        /// </summary>
        /// <param name="area"></param>
        /// <param name="sourceBitmap"></param>
        /// <returns>null если область не принадлежит к классу</returns>
        public static WayArrow GetOnValidate(LinkedList<int[]> area, Bitmap sourceBitmap)
        {
            if (!ArrowClassificator(area)) return null;
            if (!WayArrowClassificator(area, sourceBitmap)) return null;
            var com = AreaAnalyzer.CenterOfMass(area);

            return new WayArrow(com.Item1, com.Item2)
            {
                Type = AreaType.WayArrow,

                Angle = CalculateAngle(area, com)
            };
        }

        /// <summary>
        /// Классификатор всех стрелок
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        protected static bool ArrowClassificator(LinkedList<int[]> area)
        {
            var com = AreaAnalyzer.CenterOfMass(area);
            var dcm11 = AreaAnalyzer.DiscreteCentralMoment(area, com, 1, 1);
            var dcm20 = AreaAnalyzer.DiscreteCentralMoment(area, com, 2, 0);
            var dcm02 = AreaAnalyzer.DiscreteCentralMoment(area, com, 0, 2);

            //критерий по удлиненности
            var elong = AreaAnalyzer.AreaElongation(dcm11, dcm20, dcm02);
            if (elong < ElongCriterion.Item1 || elong > ElongCriterion.Item2 || elong == Double.NaN)
                return false;

            // TODO остальные инвариантные критерии
            return true;
        }

        /// <summary>
        /// Классификатор путевых стрелок по яркости
        /// </summary>
        /// <param name="area"></param>
        /// <param name="sourceBitmap"></param>
        /// <returns></returns>
        private static bool WayArrowClassificator(LinkedList<int[]> area, Bitmap sourceBitmap)
        {
            var brightness = area.Sum(pixel => sourceBitmap.GetPixel(pixel[0], pixel[1]).RgbPixelToHsl()[2, 0]) /
                             area.Count;

            if (brightness < BrightnessCriterion.Item1 || brightness > BrightnessCriterion.Item2)
                return false;

            return true;
        }


    }
}