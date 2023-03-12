using System.Collections.Generic;
using ComputerGraphics.PictureAnalysis.App.Areas;

namespace ComputerGraphics.PictureAnalysis.App.TreasureFounder
{
    /// <summary>
    /// Класс представляющий итоговый путь
    /// </summary>
    public class Way
    {
        public LinkedList<WayArrow> WayArrows { get; set; }
        public TreasureArea End { get; set; }
    }
}