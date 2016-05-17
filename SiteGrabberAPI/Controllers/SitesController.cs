// TODO: debug modifiedsince not getting inside compareto if branch
// TIMEOUT HANDLING
// TODO: implement timeout

// ERROR HANDLING
// TODO: catch errors
// TODO: catch sg.capture errors and return 500 errors
// TODO: parameter error handling (i.e. bad fileType, etc.)
// TODO: return different status codes

// CACHE HANDLING
// TODO: configure Expires header
// TODO: Add Action Filters (OutputCache)

// API AUTH
// TODO: Add Action Filters (Authorize)

// OTHER
// TODO: allow url with query params (i.e. with & embedded)
// TODO: remove headers from hackers http://www.dhuvelle.com/2012/10/programmatically-remove-http-response.html

// NOTES:
// Server.Mappath valus http://stackoverflow.com/questions/275781/server-mappath-server-mappath-server-mappath-server-mappath
// return image: http://stackoverflow.com/questions/186062/can-an-asp-net-mvc-controller-return-an-image, http://www.dotnetperls.com/image-aspnet
// Expires header: http://stackoverflow.com/questions/8406377/http-response-header-format-for-expires

// REFERENCE
// http://stackoverflow.com/questions/9541351/returning-binary-file-from-controller-in-asp-net-web-api

using System;
using System.IO;
using SiteGrabber;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Web;
using System.Web.UI;

namespace SitesApp.Controllers
{
    public class SitesController : ApiController
    {
        private int CACHE_TIME_SECONDS = 3600;

        // GET|POST api/sites
        // TODO: enable api auth
        //[System.Web.Http.Authorize]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
//        public HttpResponseMessage Sites(HttpContext context)
        public HttpResponseMessage Sites(string url, short width = 800, short height = 600, double aspectRatio = 1.333, double scale = 1.0, int crop = 1, string fileType = "png")
//        public IHttpActionResult Sites(string url, short width = 800, short height = 600, double aspectRatio = 1.333, double scale = 1.0, int crop = 1, string fileType = "png")
        {
            HttpResponseMessage result = new HttpResponseMessage();

            // Check request for If-Modified-Since header
            HttpRequest request = HttpContext.Current.Request;
            string modifiedSince = request.Headers.Get("If-Modified-Since");

            if (modifiedSince != null)
            {
                // convert modifiedSince string to datetime object
                DateTime clientContentTimestamp;
                if (DateTime.TryParse(modifiedSince, out clientContentTimestamp))
                {
                    // Check if client's resource is within the cache period.
                    // If so, then it's "not modified" and no need for server to fetch.
                    TimeSpan cache = new TimeSpan(0, 0, CACHE_TIME_SECONDS);
                    if (DateTime.UtcNow.Subtract(clientContentTimestamp).CompareTo(cache) < 0)
                    {
                        // return status 304
                        result.StatusCode = HttpStatusCode.NotModified;
                        result.ReasonPhrase = "Not Modified";
//                        return Ok("not modified");
                    }
                    result.StatusCode = HttpStatusCode.OK;
//                    return Ok("cache expired");
                }
                else
                {
                    result.StatusCode = HttpStatusCode.BadRequest;
//                    return Ok("failed to parse If-Modified-Since string");
                }

                return result;
            }
            //            return Ok("modifiedsince is null");

            // Grab site
            SGObject sg = new SGObject(url, width, height, aspectRatio, scale, crop, fileType, Path.Combine(HttpContext.Current.Server.MapPath("~"), @"bin"));
            var stream = sg.Capture();
            result.Content = new StreamContent(stream);

            // Set Content-Type response header
            string contentType = MimeMapping.GetMimeMapping(Path.GetExtension("." + fileType));
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            // Set Expires response header
            DateTimeOffset dto = DateTimeOffset.UtcNow;
            DateTimeOffset dtoNew = dto.AddHours(10);
            result.Content.Headers.Expires = DateTime.UtcNow.AddHours(10);
            //result.Content.Headers.Expires = dto.AddHours(1);
            //context.Response.Cache.SetCacheability(HttpCacheability.Public);
            //context.Response.Cache.SetExpires(DateTime.UtcNow.AddSeconds(3600));

            // Set Last-Modified response header
            result.Content.Headers.LastModified = DateTime.UtcNow;

            result.StatusCode = HttpStatusCode.OK;
            return result;

        }
    }
}