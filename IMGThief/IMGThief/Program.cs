using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IMGThiefCore;
using System.Drawing;

namespace IMGThief
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = "http://www.maspocovendo.com"; // /rafaela-santa-fe/autos/passat/388232";
            var mailconunter = 0;

            var visitar = new List<string>();
            var visitado = new List<string>();
            var escanear = new List<string>();
            var rand = new Random(DateTime.Now.Millisecond);
            var db = new EmailSeekerEntities();
            var core = new Core();

            visitar.Add(source);

            //var urls = core.FetchUrls(source).Where(url => url.StartsWith("/") && url != "/").Distinct();
            //foreach (var url in urls)
            //{
            //    Console.WriteLine(url);
            //    visitar.Add(url);
            //}

            while (visitar.Any())
            {
                try
                {
                    //Console.Clear();
                    Console.WriteLine("Visitar: " + visitar.Count);
                    Console.WriteLine("Visitado: " + visitado.Count);
                    //Console.WriteLine("Mails encontrados: " + mailconunter);

                    var actual = visitar.First();
                    visitar.Remove(visitar.First());
                    visitado.Add(actual);
                    actual = actual == source ? "/" : actual;

                    Console.WriteLine("Visitando: " + actual);
                    Console.WriteLine("##############################################");

                    if (true)//actual.EndsWith("5")): Para acelerar la cosa y solo procesar los posts reales
                    {
                        if (actual.EndsWith("jpg") || actual.EndsWith("png") || actual.EndsWith("gif") || actual.Contains("#") || actual.Contains("javascript"))
                        {
                            Console.WriteLine("Url omitida: " + actual);
                            continue;
                        }

                        var picUrls = core.FetchUrlBase64Pictures(source + actual);
                        foreach (var url in picUrls)
                        {


                            Console.WriteLine("Persistiendo: " + actual);
                            if (!db.URLs.Any(x => x.source.Equals(source) && x.relativepart.Equals(actual)))
                            {
                                var newurl = new URL()
                                {
                                    source = source,
                                    relativepart = actual,
                                    visited = false
                                };
                                db.URLs.Add(newurl);
                                db.SaveChanges();
                            }
                            else
                            {
                                Console.WriteLine("Ya existía en la BD: " + url);
                            }

                            // Procesar las imágenes acá:
                            //Console.WriteLine("Se encontró una imagen base64 en: " + source + actual);
                            ////escanear.Add(url.Replace("data:image/png;base64,", ""));
                            //var str = url.Replace("data:image/png;base64,", "");

                            //var bitmapData = new Byte[str.Length];
                            //bitmapData = Convert.FromBase64String(FixBase64ForImage(str));
                            //var streamBitmap = new System.IO.MemoryStream(bitmapData);
                            //var bitImage = new Bitmap((Bitmap) Image.FromStream(streamBitmap));
                            //var path = @"E:/TEMP/imgthief/" + (rand.Next(int.MinValue, int.MaxValue)) + "_" +
                            //           (rand.Next(int.MinValue, int.MaxValue)) + ".jpg";

                            //int factor = 4;
                            //bitImage = ResizeBitmap(bitImage, bitImage.Width*factor, bitImage.Height*factor);

                            //bitImage.Save(path);

                            //var result = core.FetchPictureText(path);
                            //if (!string.IsNullOrWhiteSpace(result) && Regex.Matches(result, @"[a-zA-Z]").Count > 0)
                            //{
                            //    //Regex.Replace(result, @"[^a-zA-z0-9 ]+", "");
                            //    result =
                            //        QuitAccents(
                            //            result.Replace("©", "@")
                            //                .Normalize(NormalizationForm.FormC));


                            //    if (result.Contains("@"))
                            //    {
                            //        Console.WriteLine("Encontrado: " + result);
                            //        mailconunter++;
                            //        var mail = new Email() {email1 = result, source = source + actual};
                            //        if (db.Emails.Any(x => x.email1.Equals(mail.email1)))
                            //        {
                            //            Console.WriteLine(mail.email1 + " YA EXISTÍA EN LA BD");
                            //            continue;
                            //        }
                            //        db.Emails.Add(mail);
                            //        db.SaveChanges();
                            //    }
                            //}
                        }
                    }
                    var nuevasUrls = core.FetchUrls(source + actual).Where(url => url.StartsWith("/") && url != "/").Distinct();
                    foreach (var url in nuevasUrls)
                    {
                        if (!visitado.Contains(url) && !visitar.Contains(url)) visitar.Add(url);
                    }
                }
                catch
                {
                    continue;
                }
            }

            Console.ReadLine();
        }

        public static string FixBase64ForImage(string Image)
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

        private static string QuitAccents(string inputString)
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
