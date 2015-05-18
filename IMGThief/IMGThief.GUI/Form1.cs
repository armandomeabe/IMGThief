using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IMGThief.GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var htmlcore = new IMGThief.Core.Core();
            var urls = htmlcore.FetchUrlPictures(textBox1.Text);
            listBox1.Items.Clear();
            listBox1.Items.AddRange(urls.ToArray());


            //pictureBox1.ImageLocation = "http://www.ventafe.com.ar/fotos_up/Inm/Inm-47963_img96096.JPG";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.ImageLocation = textBox1.Text + listBox1.SelectedItem;
        }
    }
}
