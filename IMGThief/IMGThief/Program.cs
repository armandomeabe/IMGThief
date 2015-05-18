using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMGThief
{
    class Program
    {
        static void Main(string[] args)
        {
            var htmlcore = new IMGThief.Core.Core();
            var urls = htmlcore.FetchUrlPictures("http://www.ventafe.com.ar/web/default.asp#.VVleoPl_NBc");


        }
    }
}
