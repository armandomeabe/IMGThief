using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IMGThiefCore;
using Tesseract;

namespace IMGBase64URLExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            //CheckFileType(@"E:/TEMP/aaaaa/");
            //return;
            
            var db = new EmailSeekerEntities();
            var core = new Core();
            var rand = new Random(DateTime.Now.Millisecond);

            var URLs = db.URLs.ToList();

            foreach (var url in URLs)
            {
                try
                {
                    if (url.relativepart.EndsWith("jpg") || url.relativepart.EndsWith("png") || url.relativepart.EndsWith("gif") || url.relativepart.Contains("#") || url.relativepart.Contains("javascript"))
                    {
                        Console.WriteLine("Url omitida: " + url.relativepart);
                        continue;
                    }

                    var picUrls = core.FetchUrlBase64Pictures(url.source + url.relativepart);
                    foreach (var picUrl in picUrls)
                    {
                        // Antes de guardar: Eliminar los archivos que acabo de crear porque puede crecer muchísimo esta carpeta.
                        var downloadedMessageInfo = new DirectoryInfo(@"E:/TEMP/imgthief/");
                        foreach (FileInfo file in downloadedMessageInfo.GetFiles())
                        {
                            file.Delete();
                        }


                        var str = picUrl.Replace("data:image/png;base64,", "");

                        var bitmapData = new Byte[str.Length];
                        bitmapData = Convert.FromBase64String(FixBase64ForImage(str));
                        var streamBitmap = new System.IO.MemoryStream(bitmapData);
                        var bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));
                        var path = @"E:/TEMP/imgthief/" + (rand.Next(int.MinValue, int.MaxValue)) + "_" +
                                   (rand.Next(int.MinValue, int.MaxValue)) + ".jpg";

                        var factor = 2.5;
                        bitImage = ResizeBitmap(bitImage, (int)(bitImage.Width * factor), (int)(bitImage.Height * factor));
                        bitImage.Save(path);


                        var results = MODIOCR(@"E:/TEMP/imgthief/");
                        foreach (var result in results)
                        {
                            var r = QuitAccents(
                                    result
                                    .Replace("©", "@")
                                    .Replace("I", "l")
                                    .Replace("\r\n", "")
                                    .Normalize(NormalizationForm.FormC));

                            if (r.Contains("@"))
                            {
                                {
                                    Console.WriteLine("Encontrado: " + r);
                                    var mail = new Email() { email1 = r, source = url.source + url.relativepart, sitioId = url.sitioId };
                                    if (db.Emails.Any(x => x.email1.Equals(mail.email1)))
                                    {
                                        Console.WriteLine(mail.email1 + " YA EXISTÍA EN LA BD");
                                        continue;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Persistiendo: " + mail.email1);
                                    }
                                    db.Emails.Add(mail);
                                    db.SaveChanges();
                                }
                            }
                        }



                        //var result = core.FetchPictureText(path);
                        //if (!string.IsNullOrWhiteSpace(result) && Regex.Matches(result, @"[a-zA-Z]").Count > 0)
                        //{
                        //    //Regex.Replace(result, @"[^a-zA-z0-9 ]+", "");
                        //    result =
                        //        QuitAccents(
                        //            result
                        //            .Replace("©", "@")
                        //            .Replace("I", "l")
                        //            .Normalize(NormalizationForm.FormC));


                        //    if (result.Contains("@"))
                        //    {
                        //        Console.WriteLine("Encontrado: " + result);
                        //        var mail = new Email() { email1 = result, source = url.source + url.relativepart, sitioId = url.sitioId };
                        //        if (db.Emails.Any(x => x.email1.Equals(mail.email1)))
                        //        {
                        //            Console.WriteLine(mail.email1 + " YA EXISTÍA EN LA BD");
                        //            continue;
                        //        }
                        //        else
                        //        {
                        //            Console.WriteLine("Persistiendo: " + mail.email1);
                        //        }
                        //        db.Emails.Add(mail);
                        //        db.SaveChanges();
                        //    }
                        //}
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        public static string FixBase64ForImage(string Image)
        {
            var sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty); sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }

        private static Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            var result = new Bitmap(width, height);
            using (var g = Graphics.FromImage(result))
                g.DrawImage(sourceBMP, 0, 0, width, height);
            return result;
        }

        private static string QuitAccents(string inputString)
        {
            var a = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
            var e = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
            var i = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
            var o = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
            var u = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
            var n = new Regex("[ñ|Ñ]", RegexOptions.Compiled);
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
