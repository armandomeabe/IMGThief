using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IMGThiefCore;

namespace IMGThief.GUI
{
    public partial class Form1 : Form
    {
        private List<string> filePaths { get; set; } // Este es para archivos

        private List<string> visit { get; set; }
        private List<string> visited { get; set; }

        private IMGThiefCore.Core core { get; set; }
        public Random Rand { get; set; }

        private EmailSeekerEntities db { get; set; }

        public Form1()
        {
            InitializeComponent();
            core = new Core();
            filePaths = new List<string>();
            visited = new List<string>();
            visit = new List<string>();
            Rand = new Random(DateTime.Now.Millisecond);
            db = new EmailSeekerEntities();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.DoEvents();
            //if (!textBox1.Text.EndsWith("/")) textBox1.Text += "/";
            textBox2.Text = textBox1.Text.Remove(textBox1.Text.IndexOf("/", 8, System.StringComparison.Ordinal));

            listBox1.Items.Clear();
            var picurls = core.FetchUrlPictures(textBox1.Text);
            listBox1.Items.AddRange(picurls.Distinct().ToArray());

            listBox3.Items.Clear();
            var urls = core.FetchUrls(textBox1.Text);
            visit.AddRange(urls.Distinct());
            visit = visit.Distinct().Where(x => !visited.Contains(x)).ToList();
            listBox3.Items.AddRange(visit.ToArray());

            Application.DoEvents();
            button2_Click(sender, e);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.ImageLocation = textBox1.Text + listBox1.SelectedItem;
        }

        public string FixBase64ForImage(string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty); sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }

        private static Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(sourceBMP, 0, 0, width, height);
            return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (listBox1.Items.Count > 0)
            {
                var item = listBox1.Items[0];
                listBox1.Items.RemoveAt(0);

                if (!item.ToString().Contains("base64")) continue;
                var str = item.ToString().Replace("data:image/png;base64,", "");
                
                var bitmapData = new Byte[str.Length];
                bitmapData = Convert.FromBase64String(FixBase64ForImage(str));
                var streamBitmap = new System.IO.MemoryStream(bitmapData);
                var bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));
                var path = @"E:/TEMP/imgthief/" + (Rand.Next(int.MinValue, int.MaxValue)) + "_" + (Rand.Next(int.MinValue, int.MaxValue)) + ".jpg";

                int factor = 4;
                bitImage = ResizeBitmap(bitImage, bitImage.Width * factor, bitImage.Height * factor);

                bitImage.Save(path);
                filePaths.Add(path);

                var result = core.FetchPictureText(path);
                if (!string.IsNullOrWhiteSpace(result) && !listBox2.Items.Contains(result) && Regex.Matches(result, @"[a-zA-Z]").Count > 0)
                {
                    //Regex.Replace(result, @"[^a-zA-z0-9 ]+", "");
                    result =
                        QuitAccents(
                            result.Replace("©", "@")
                                .Normalize(NormalizationForm.FormC));
                    

                    if (result.Contains("@"))
                    {
                        listBox2.Items.Add(result);
                        var mail = new Email() { email1 = result };
                        db.Emails.Add(mail);
                    }
                    
                    // ARREGLAR EL REDIMENSIONADO DE IMAGENES Y SALE CON FRITAS


                }

                db.SaveChanges();

                Application.DoEvents();

                // Con el siguiente código, ADEMÁS de guardar las imágenes base64, puedo guardar las imagenes (todas)
                //var itemUrl = textBox1.Text + listBox1.Items[0];
                //var path = @"E:/TEMP/imgthief/" + (Rand.Next(int.MinValue, int.MaxValue)) + "_" + (Rand.Next(int.MinValue, int.MaxValue)) + ".jpg";
                //filePaths.Add(path);
                //listBox1.Items.RemoveAt(0);

                //var client = new WebClient();
                //client.DownloadProgressChanged += client_DownloadProgressChanged;
                //client.DownloadFileCompleted += client_DownloadFileCompleted;
                //client.DownloadFileAsync(new Uri(itemUrl), path);
            }

            if (listBox3.Items.Count > 0)
            {
                while (listBox3.Items[0].ToString().Contains("#") || listBox3.Items[0].ToString().Contains("http") || !Regex.IsMatch(listBox3.Items[0].ToString(), @"\d$")) listBox3.Items.RemoveAt(0);

                textBox1.Text = textBox2.Text + listBox3.Items[0];
                listBox3.Items.RemoveAt(0);


                visited.Add(textBox1.Text.Replace(textBox2.Text, ""));

                Application.DoEvents();
                button1_Click(sender, e);
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ProcessPathList();
        }

        void ProcessPathList()
        {
            if (listBox1.Items.Count > 0) return;

            while (filePaths.Any())
            {
                var result = core.FetchPictureText(filePaths.First());
                if (!string.IsNullOrWhiteSpace(result))
                    listBox2.Items.Add(result);
            }
        }

        public string QuitAccents(string inputString)
        {
            Regex a = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
            Regex e = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
            Regex i = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
            Regex o = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
            Regex u = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
            Regex n = new Regex("[ñ|Ñ]", RegexOptions.Compiled);
            inputString = a.Replace(inputString, "a");
            inputString = e.Replace(inputString, "e");
            inputString = i.Replace(inputString, "i");
            inputString = o.Replace(inputString, "o");
            inputString = u.Replace(inputString, "u");
            inputString = n.Replace(inputString, "n");

            inputString = inputString.Replace("ì", "i"); // No parece funcionar en el regex
            inputString = inputString.Replace("í", "i"); // No parece funcionar en el regex
            inputString = inputString.Replace(" ", "_");
            inputString = inputString.Replace("hotmaìl", "hotmail");

            return inputString;
        }
    }
}
