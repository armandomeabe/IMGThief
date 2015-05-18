using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using journeyofcode.Images.OnenoteOCR;

namespace IMGThiefCore
{
    public class Core
    {
        public IEnumerable<string> FetchUrlPictures(string url)
        {
            var document = new HtmlWeb().Load(url);
            var urls = document.DocumentNode.Descendants("img")
                                            .Select(e => e.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

            return urls;
        }

        public IEnumerable<string> FetchUrls(string url)
        {
            var document = new HtmlWeb().Load(url);
            var urls = document.DocumentNode.Descendants("a")
                                            .Select(e => e.GetAttributeValue("href", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

            return urls;
        }

        public string FetchPictureText(string filenamefull)
        {
            if (String.IsNullOrWhiteSpace(filenamefull) || !File.Exists(filenamefull))
            {
                return "SIN PARAMETRO DE ENTRADA";
            }

            try
            {
                using (var ocrEngine = new OnenoteOcrEngine())
                using (var image = Image.FromFile(filenamefull))
                {
                    var text = ocrEngine.Recognize(image);
                    return text ?? "";
                }
            }
            catch (OcrException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
