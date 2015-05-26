using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

        public IEnumerable<string> FetchUrlBase64Pictures(string url)
        {
            var document = new HtmlWeb().Load(url);
            var urls = document.DocumentNode.Descendants("img")
                                            .Select(e => e.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s) && s.Contains("base64")).ToList();
            
            var result = new List<string>(urls.Count());
            result.AddRange(urls.Select(url1 => url1.Replace("data:image/png;base64,", "")));

            return result;
        }

        public IEnumerable<string> FetchUrls(string url)
        {
            var document = new HtmlWeb().Load(url);
            var urls = document.DocumentNode.Descendants("a")
                                            .Select(e => e.GetAttributeValue("href", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

            return urls;
        }

        public IEnumerable<string> FetchEmailsFromText(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            string data = "";

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var receiveStream = response.GetResponseStream();
                StreamReader readStream = null;
                readStream = response.CharacterSet == null ? new StreamReader(receiveStream) : new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }

            //var document = new HtmlWeb().Load(url);
            return MailExtracter.ExtractEmails(data);
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

    class MailExtracter
    {
        public static List<string> ExtractEmails(string data)
        {
            var emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);
            var emailMatches = emailRegex.Matches(data);
            return (from Match emailMatch in emailMatches select emailMatch.Value).ToList();
        }
    }
}
