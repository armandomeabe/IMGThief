using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Drawing;

namespace IMGThiefCore
{
    public static class NetUtils
    {
        public static Image GetImageFromUrl(string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            using (var httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream stream = httpWebReponse.GetResponseStream())
                {
                    if (stream != null) return Image.FromStream(stream);
                }
            }
            return null;
        }
    }
}
