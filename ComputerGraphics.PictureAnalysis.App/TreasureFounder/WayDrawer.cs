using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ComputerGraphics.PictureAnalysis.App.TreasureFounder
{
    public class WayDrawer
    {
        /// <summary>
        /// Рисуем путь по полученной информации
        /// </summary>
        /// <param name="endArea"></param>
        /// <param name="way"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Bitmap DrawWay(LinkedList<int[]> endArea, Way way, Bitmap bitmap)
        {
            var minX = endArea.Min(x => x[0]);
            var minY = endArea.Min(x => x[1]);
            var maxX = endArea.Max(x => x[0]);
            var maxY = endArea.Max(x => x[1]);

            using (var paint = Graphics.FromImage(bitmap))
            {
                var pen = new Pen(Color.Yellow, 4);

                var current = way.WayArrows.First;

                while (current != null)
                {
                    var next = current.Next;

                    paint.DrawLine(pen, current.Value.CenterOfMass, next == null ? way.End.CenterOfMass : next.Value.CenterOfMass);

                    current = next;
                }

                paint.DrawLine(pen, maxX, minY, minX, minY);
                paint.DrawLine(pen, maxX, maxY, minX, maxY);
                paint.DrawLine(pen, minX, minY, minX, maxY);
                paint.DrawLine(pen, maxX, minY, maxX, maxY);
                return bitmap;
            }
        }
    }
}