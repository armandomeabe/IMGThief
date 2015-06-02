using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMGThief.LocalFileThief
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        public static List<string> MODIOCR(string directoryPath)
        {
            var results = new List<string>();
            var files = Directory.GetFiles(directoryPath).GetEnumerator();
            while (files.MoveNext())
            {
                var fileExtension = Path.GetExtension(Convert.ToString(files.Current));
                var fileName = Convert.ToString(files.Current).Replace(fileExtension, string.Empty);
                if (fileExtension == ".jpg" || fileExtension == ".JPG")
                {
                    try
                    {
                        MODI.Document md = new MODI.Document();
                        md.Create(Convert.ToString(files.Current));
                        md.OCR(MODI.MiLANGUAGES.miLANG_ENGLISH, true, true);
                        MODI.Image image = (MODI.Image)md.Images[0];

                        results.Add(image.Layout.Text);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return results;
        }
    }
}
