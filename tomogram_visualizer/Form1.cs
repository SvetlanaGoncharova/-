using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shumihin_tomogram_visualizer
{
    public partial class Form1 : Form
    {
        Bin bin;
        bool loaded = false;
        View view;
        int currentLayer = 0;
        int maxLayer = 0;

        int frameCount;
        DateTime nextFPSUpdate = DateTime.Now.AddSeconds(1);

        bool needReload = false;

        bool useQuads = true;

        public Form1()
        {
            InitializeComponent();
            bin = new Bin();
            view = new View();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                bin.readBIN(str);
                view.setupView(glControl1.Width, glControl1.Height);
                loaded = true;
                maxLayer = bin.getMax();
                glControl1.Invalidate();
                trackBar1.Maximum = maxLayer - 1;
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                if (needReload)
                {
                    if (useQuads)
                        view.drawQuads(currentLayer);
                    else
                    {
                        view.generateTextureImage(currentLayer);
                        view.load2DTexture();
                        needReload = false;
                    }
                }
                if (!useQuads)
                    view.drawTexture();
                glControl1.SwapBuffers();
            }
            //if (loaded)
            //{
            //    view.drawQuads(currentLayer);
            //    glControl1.SwapBuffers();
            //}
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
            textBox1.Text = " " + trackBar1.Value;
            needReload = true;
            //if (loaded)
            //{
            //    view.drawQuads(currentLayer);
            //    glControl1.SwapBuffers();
            //}
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int num = Convert.ToInt32(textBox1.Text);
            if (num < maxLayer && num > 0)
            {
                currentLayer = num;
            }
        }

        void application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                displayFPS();
                glControl1.Invalidate();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Application.Idle += application_Idle;
        }

        void displayFPS()
        {
            if (DateTime.Now >= nextFPSUpdate)
            {
                this.Text = String.Format("CT Visualizer (fps={0})", frameCount);
                nextFPSUpdate = DateTime.Now.AddSeconds(1);
                frameCount = 0;
            }
            frameCount++;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            useQuads = true;
            checkBox2.Checked = false;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            useQuads = false;
            checkBox1.Checked = false;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            view.setTransfMin(trackBar2.Value);
            needReload = true;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            view.setTranfWidth(trackBar3.Value);
            needReload = true;
        }
    }
}
