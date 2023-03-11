using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComputerGraphics.PictureAnalysis.App;
using MathNet.Numerics.Statistics;
using Label = System.Windows.Forms.Label;

namespace ComputerGraphics.PictureAnalysis.Gui
{
    public partial class MainForm : Form
    {
        private string _currentImagePath = string.Empty;
        private readonly double _previewRatio;
        private readonly Label[] _labels;
        private double p = 5d / 100;
        private Bitmap _binarized = null;

        public MainForm()
        {
            InitializeComponent();
            trackBar1.Scroll += trackbar1_Scroll;
            openFileDialog1.Filter = "Images|*.BMP;*.JPG;*.PNG|All files(*.*)|*.*";
            _previewRatio = (double)pictureBox1.Width / pictureBox1.Height;
            _labels = new[] { label1, label2, label3 };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Диалог для выбора фото
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            ResetInfoBox(1);
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            _currentImagePath = openFileDialog1.FileName;
            try
            {
                var image = ScalePictureForPictureBox(Image.FromFile(_currentImagePath));
                pictureBox1.Image = image;
            }
            catch (Exception ex)
            {
                label1.ForeColor = Color.Red;
                label1.Text = ex.Message;
            }
        }

        private Image ScalePictureForPictureBox(Image image)
        {
            if (image == null)
                return null;
            double imageRatio = (double)image.Width / image.Height;

            // Вычисляем новые размеры изображения с сохранением пропорций
            int newWidth, newHeight;
            if (imageRatio > _previewRatio)
            {
                newWidth = pictureBox1.Width;
                newHeight = (int)(pictureBox1.Width / imageRatio);
            }
            else
            {
                newWidth = (int)(pictureBox1.Height * imageRatio);
                newHeight = pictureBox1.Height;
            }
            return new Bitmap(image, newWidth, newHeight);
        }

        private void ResetInfoBox(int no)
        {
            var labelTochange = _labels[no - 1];
            labelTochange.ForeColor = Color.Black;
            labelTochange.Text = string.Empty;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var image = Image.FromFile(_currentImagePath);
                var matrices = ((Bitmap)image).ToMatrixList().RgbToHslMatrices().ToList();
                var gist = Utilities.GetGistagram(matrices.Select(x => x[2, 0]));
                var deviaton = Utilities.StandartDeviation(gist.Keys);
                var smothedGist =
                    Utilities.KernelSmooth(gist, Utilities.ScottMethodCoreWidth(deviaton, gist.Keys.Count));

                var t = Utilities.BinarizationThreshold(smothedGist, p);

                var binarized = PictureWorker.BinarizePicture(matrices.ToList(), t)
                    .Select(x => x.HslPixelToRgb()).ToArray()
                    .ToBitmap(image.Width, image.Height);
                _binarized = binarized;
                pictureBox2.Image = ScalePictureForPictureBox(binarized);
            }
            catch (Exception ex)
            {
                label1.ForeColor = Color.Red;
                label1.Text = ex.Message;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ResetInfoBox(3);
            try
            {
                if (_binarized == null)
                    throw new InvalidOperationException("Исходник не бинаризован!");

                var selectedAreas = PictureWorker.SelectRelatedAreas(_binarized);

                var colorizedBitmap = PictureWorker.ColorizeSelectedAreas(selectedAreas);

                pictureBox2.Image = ScalePictureForPictureBox(colorizedBitmap);
                var centersOfMass = PictureWorker.CenterOfMass(selectedAreas);
                var areasWithoutBackg = PictureWorker.ExcludeBackgroundFromPicture(selectedAreas);
                var dcm11 = PictureWorker.DiscreteCentralMoment(areasWithoutBackg, centersOfMass, 1, 1);
                var dcm20 = PictureWorker.DiscreteCentralMoment(areasWithoutBackg, centersOfMass, 2, 0);
                var dcm02 = PictureWorker.DiscreteCentralMoment(areasWithoutBackg, centersOfMass, 0, 2);
            }
            catch(Exception ex)
            {
                label3.ForeColor = Color.Red;
                label3.Text = ex.Message;
            }
        }

        private void trackbar1_Scroll(object sender, EventArgs e)
        {
            label2.Text = $@"p% = {trackBar1.Value}%";
            p = (double)trackBar1.Value / 100;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }
    }
}
