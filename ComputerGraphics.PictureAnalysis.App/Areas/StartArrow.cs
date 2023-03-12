using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ComputerGraphics.PictureAnalysis.App.Areas
{
    public class StartArrow : WayArrow
    {
        /// <summary>
        /// Установленные опытным путем минимальные и максимальные значения яркости для начальной стрелки
        /// </summary>
        private static readonly (double, double) BrightnessCriterion = (0.45d, 0.52d);
        protected StartArrow(int x, int y) : base(x, y) {}

        /// <summary>
        /// Метод который формирует экземпляр области если он принадлежит к классу
        /// </summary>
        /// <param name="area"></param>
        /// <param name="sourceBitmap"></param>
        /// <returns>null если область не принадлежит к классу</returns>
        public new static StartArrow GetOnValidate(LinkedList<int[]> area, Bitmap sourceBitmap)
        {
            if (!ArrowClassificator(area)) return null;
            if (!StartArrowClassificator(area, sourceBitmap)) return null;
            var com = AreaAnalyzer.CenterOfMass(area);
            return new StartArrow(com.Item1, com.Item2)
            {
                Type = AreaType.StartArrow,
                Angle = CalculateAngle(area, com)

            };
        }

        /// <summary>
        /// Оцениваем область по критерию яркости
        /// </summary>
        /// <param name="area"></param>
        /// <param name="sourceBitmap"></param>
        /// <returns></returns>
        private static bool StartArrowClassificator(LinkedList<int[]> area, Bitmap sourceBitmap)
        {
            var brightness = area.Sum(pixel => sourceBitmap.GetPixel(pixel[0], pixel[1]).RgbPixelToHsl()[2, 0]) /
                             area.Count;

            if (brightness < BrightnessCriterion.Item1 || brightness > BrightnessCriterion.Item2)
                return false;

            return true;
        }
    }
}