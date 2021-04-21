using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace GeoNorgeAPI
{


    public class HttpRequestExecutor
    {
        //private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public event LogEventHandlerInfo OnLogEventInfo = delegate { };
        public event LogEventHandlerDebug OnLogEventDebug = delegate { };
        public event LogEventHandlerError OnLogEventError = delegate { };

        private readonly RequestFactory _requestFactory;
        private readonly RequestRunner _requestRunner;

        public string PostRequest(string url, string accept, string contentType, string postData, string username = null, string password = null, Cookie cookie = null, Dictionary<string, string> additionalRequestHeaders = null)
        {
            HttpWebResponse response = FullPostRequest(url, accept, contentType, postData, username, password, cookie, additionalRequestHeaders);

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responseBody = reader.ReadToEnd();
            response.Close();
            //Log.Debug("HTTP response body:\n" + responseBody);
            OnLogEventDebug("HTTP response body:\n" + responseBody);
            return responseBody;
        }

        public HttpWebResponse FullPostRequest(string url, string accept, string contentType, string postData, string username = null, string password = null, Cookie cookie = null, Dictionary<string, string> additionalRequestHeaders = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                if (accept != null)
                    request.Accept = accept;
                request.ContentType = contentType;
                request.Timeout = 15000;

                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    SetBasicAuthHeader(request, username, password);
                }

                if (additionalRequestHeaders != null && additionalRequestHeaders.Count > 0)
                {
                    foreach (KeyValuePair<string,string> element in additionalRequestHeaders)
                    {
                        request.Headers[element.Key] = element.Value;
                    }
                }

                //Log.Debug("HTTP request: [" + request.Method + "] " + url);
                OnLogEventDebug("HTTP request: [" + request.Method + "] " + url);
                //Log.Debug("HTTP request body:\n" + postData);
                OnLogEventDebug("HTTP request body:\n" + postData);
                request.CookieContainer = new CookieContainer();
                if (cookie != null)
                {
                    Cookie sessionCookie = new Cookie("JSESSIONID", cookie.Value, "/geonetwork", cookie.Domain);
                    request.CookieContainer.Add(sessionCookie);
                }

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                var response = (HttpWebResponse)request.GetResponse();
                //Log.Debug("HTTP response: " + response.StatusCode + " - " + response.StatusDescription);
                OnLogEventDebug("HTTP response: " + response.StatusCode + " - " + response.StatusDescription);
                return response;
            }
            catch (Exception e)
            {
                //Log.Error("Exception while running HTTP request.", e);
                OnLogEventError("Exception while running HTTP request.", e);
                throw e;
            }
        }

        public void SetBasicAuthHeader(WebRequest req, String userName, String userPassword)
        {
            string authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;
        }


        public HttpWebResponse FullGetRequest(string url, string accept, string contentType)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = accept;
            request.ContentType = contentType;

            //Log.Debug("HTTP request: [" + request.Method + "] " + url);
            OnLogEventDebug("HTTP request: [" + request.Method + "] " + url);

            return (HttpWebResponse)request.GetResponse();
        }

    }
}
