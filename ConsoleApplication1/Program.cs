using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteGrabber;
using System.IO;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            SGObject sg = new SGObject("http://www.mediatemple.net", 300, 240, 1.33333, 1.0, 1, "png");
            var stream = sg.Capture();
//            FileContentResult result = new FileContentResult(bytes, "image/png");
//            Console.WriteLine("result: " + result);
            
/*
            byte[] bytes = null;
            if (File.Exists(file))
            {
                Console.WriteLine("file exists");
                bytes = File.ReadAllBytes(file);
            }
            Console.WriteLine("content: " + bytes);
*/
        }
    }
}
