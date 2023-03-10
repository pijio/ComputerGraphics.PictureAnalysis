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
using Label = System.Windows.Forms.Label;

namespace ComputerGraphics.PictureAnalysis.Gui
{
    public partial class MainForm : Form
    {
        private string _currentImagePath = string.Empty;
        private readonly double _previewRatio;
        private readonly Label[] _labels;

        public MainForm()
        {
            InitializeComponent();
            openFileDialog1.Filter = "Images|*.BMP;*.JPG;*.PNG|All files(*.*)|*.*";
            _previewRatio = (double)pictureBox1.Width / pictureBox1.Height;
            _labels = new[] { label1 };
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
    }
}
