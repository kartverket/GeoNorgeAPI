using System;
using System.IO;
using System.Net;
using System.Text;

namespace GeoNorgeAPI
{
    public class HttpRequestExecutor
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string PostRequest(string url, string accept, string contentType, string postData, Cookie cookie = null)
        {
            HttpWebResponse response = FullPostRequest(url, accept, contentType, postData, cookie);

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responseBody = reader.ReadToEnd();
            response.Close();

            return responseBody;
        }

        public HttpWebResponse FullPostRequest(string url, string accept, string contentType, string postData, Cookie cookie = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                if (accept != null)
                    request.Accept = accept;
                request.ContentType = contentType;
                request.Timeout = 15000;

                Log.Info("HTTP request: [" + request.Method + "] " + url);

                request.CookieContainer = new CookieContainer();
                if (cookie != null)
                {
                    Cookie sessionCookie = new Cookie("JSESSIONID", cookie.Value, "/geonetwork", cookie.Domain);
                    request.CookieContainer.Add(cookie);
                }

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                var response = (HttpWebResponse)request.GetResponse();
                Log.Debug("HTTP response: " + response.StatusCode + " - " + response.StatusDescription);
                return (HttpWebResponse)request.GetResponse();

            }
            catch (Exception e)
            {
                Log.Error("Exception while running HTTP request.", e);
                throw e;
            }
        }

        public HttpWebResponse FullGetRequest(string url, string accept, string contentType)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = accept;
            request.ContentType = contentType;

            Log.Info("HTTP request: [" + request.Method + "] " + url);

            return (HttpWebResponse)request.GetResponse();
        }

    }
}
