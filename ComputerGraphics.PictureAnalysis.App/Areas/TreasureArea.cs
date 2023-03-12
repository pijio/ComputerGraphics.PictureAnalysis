using System.Drawing;

namespace ComputerGraphics.PictureAnalysis.App.Areas
{
    /// <summary>
    /// Класс клада
    /// </summary>
    public class TreasureArea : BaseArea
    {
        public TreasureArea(int x, int y)
        {
            this.CenterOfMass = new Point(x, y);
            this.Type = AreaType.Treasure;
        }
    }
}