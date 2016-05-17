// TODO: width/height auto calculations (pass thru for now)
// TODO: integrate image processing for scaling, cropping
// zoom factor only upscales. It does not downscale even if value < 1.0
// TODO: implement is_valid_web_url
// TODO: implement timeout

using System;
using System.IO;
using System.Text.RegularExpressions;
using NReco.PhantomJS;

namespace SiteGrabber
{
    // SiteGrabber utilizes the PhantomJS package to take a screenshot
    // of the specified url with the specified dimensions.
    // Possible output file types are: png, jpg, jpeg, pdf

    public class SGObject
    {
        // Constants
        private short timeToExit = 15;
        private short MAX_URL_CHARS = 2000;
        private const short DEFAULT_IMG_WIDTH = 800;
        private const double DEFAULT_IMG_ASPECT_RATIO = 1.333; // width/height
        private short CACHE_TIME = 300;   // seconds
        private short TIMEOUT = 15;    // seconds
                                       //        private static string OUTPUT_PATH = Path.Combine(Path.GetTempPath(), "thumbs");
                                       //        private static string OUTPUT_PATH = String.Format("{0}thumbs{1}", Directory.GetAccessControl(Path.GetTempPath) Path.DirectorySeparatorChar); // NEEDS WORK!
                                       //        private static string OUTPUT_PATH = String.Format("{0}{1}{2}", DriveInfo.GetDrives Path.VolumeSeparatorChar, Path.Combine("temp", "SiteGrabber", "thumbs"));
                                       //        private string OUTPUT_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
                                       //                                       private string OUTPUT_PATH = AppDomain.CurrentDomain.BaseDirectory;
        //private string OUTPUT_PATH = "";

        // Constructor
        public SGObject(string url, short width = DEFAULT_IMG_WIDTH, double aspectRatio = DEFAULT_IMG_ASPECT_RATIO, double zoom = 1.0, int crop = 1, string fileType = "png", string targetDir = "", short height)
        {
            SiteUrl = url;
            Width = width;
            Height = height;
            AspectRatio = aspectRatio;
            ZoomFactor = zoom;
            Crop = crop;
            FileType = fileType;
            TargetDir = targetDir;
            // nocache = nocache // 0;

            if ( (Height != null ) || ( AspectRatio != null ) )
            {
                // if both specified, give priority to aspect_ratio
                // override height based on aspect_ratio
                if (AspectRatio != null)
                {
                    Height = Width / AspectRatio;
                }
                else if (Height != null)
                {
                    AspectRatio = Width / Height;
                }
            }
            else
            {
                Height = Width / AspectRatio;
            }
        }

        // Public Access Members
        public string SiteUrl
        {
            get;
            set;
        }

        public short Width
        {
            get;
            set;
        }

        public short Height
        {
            get;
            set;
        }

        public double AspectRatio
        {
            get;
            set;
        }

        public double ZoomFactor
        {
            get;
            set;
        }

        public int Crop
        {
            get;
            set;
        }

        public string FileType
        {
            get;
            set;
        }

        public string TargetDir
        {
            get;
            set;
        }

        // Core Logic
        public Stream Capture()
        {
            // disk space available: https://msdn.microsoft.com/en-us/library/system.io.driveinfo.volumelabel(v=vs.110).aspx
            string targetDir = TargetDir;
            string siteUrl = SiteUrl;
            short pageWidth = Width;
            short pageHeight = Height;
            double zoomFactor = ZoomFactor;
            int crop = Crop;

            // Perform width/height/aspect ratio calculations
            short finalPageWidth = pageWidth;
            short finalPageHeight = pageHeight;

            // Generate a filename for saving the image
            string outFile = generateFileName(siteUrl, finalPageWidth, finalPageHeight, this.FileType);
//            Console.WriteLine("filename: " + outFile);

            // Check cache for image
            string cwd = Directory.GetCurrentDirectory();
            changeDir(targetDir);
            Stream stream;
            if (File.Exists(outFile))
            {
                stream = new FileStream(outFile, FileMode.Open);
                return stream;
            }

            // Lets go grabbing!
            PhantomJS phantomJS = new PhantomJS();
            phantomJS.OutputReceived += (sender, e) =>
            {
                Console.WriteLine("PhantomJS output: {0}", e.Data);
            };
            phantomJS.RunScript("var page = require('webpage').create(); page.viewportSize = { width: " + pageWidth + ", height: " + pageHeight + "}; page.clipRect = { top: 0, left: 0, width: " + pageWidth + ", height: " + pageHeight + "}; page.zoomFactor = " + zoomFactor + "; page.open('" + siteUrl + "', function() { page.render('" + outFile + "'); phantom.exit();}); ", null);

            // Image processing (crop/scale) goes here
            // outFile = processImage(outFile);

            stream = new FileStream(outFile, FileMode.Open);
            changeDir(cwd);
            return stream;
        }

        /*
            VALIDATE URL
sub is_valid_web_url {
    my $url = shift;
    if ( $url eq '' ) {
        return;
    }
    my ( undef, $scheme, $u ) = ( $url =~ /^((\w+):\/\/)?(.+)/ );
    $scheme ||= 'http';
    my $uri = URI->new( sprintf( '%s://%s', $scheme, $u ) );
    return is_web_uri( $uri->as_string );
}
            * */

        private void changeDir(string dir)
        {
            try
            {
                // Get the current directory.
                string path = Directory.GetCurrentDirectory();
                Console.WriteLine("The current directory is {0}", path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Change the current directory.
                Environment.CurrentDirectory = (dir);
                if (path.Equals(Directory.GetCurrentDirectory()))
                {
                    Console.WriteLine("You are in the target directory.");
                }
                else
                {
                    Console.WriteLine("You are not in the target directory.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }

        private string slugifyUrl(string url)
        {
            Console.WriteLine("pre: " + url);
            Uri uri = new Uri(url);

            // Extract Host+Path url components
            string sanitized = uri.Host;
            string[] pathAndQuery = uri.PathAndQuery.Split('?');
            string path = pathAndQuery[0];
            if (path.Length > 1)
            {
                sanitized += path;
            }
            Console.WriteLine("host+path: " + sanitized);

            // Remove www.
            String pattern = @"^(www\.)?(.+)";
            Match m = Regex.Match(sanitized, pattern);
            string match = (string)m.Groups[2].Value;
            sanitized = match;

            // Remove any trailing slashes
            pattern = @"(.+)/$";
            string lastString;
            string loopString = sanitized;
            do
            {
                lastString = loopString;
                m = Regex.Match(loopString, pattern);
                loopString = (string)m.Groups[1].Value;
            }
            while (loopString.Length > 1);
            sanitized = lastString;

            // Replace slashes with !
            sanitized = sanitized.Replace('/', '!');

            // Truncate to max length
            if (sanitized.Length > MAX_URL_CHARS)
            {
                sanitized = sanitized.Substring(0, MAX_URL_CHARS);
            }
            Console.WriteLine("post: " + sanitized);
            return sanitized;
        }

        private string generateFileName(string url, int width = 800, int height = 600, string fileType = "png")
        {
            return string.Format("{0}-{1}x{2}.{3}", slugifyUrl(url), width, height, fileType);
        }
    }
}
