using System.Collections.Generic;
using System.Drawing;

namespace ComputerGraphics.PictureAnalysis.App.Areas
{
    /// <summary>
    /// Базовый класс для всех связанных областей
    /// </summary>
    public abstract class BaseArea
    {
        /// <summary>
        /// Центр масс
        /// </summary>
        public Point CenterOfMass { get; set; }
        public AreaType Type { get; set; }

    }
}