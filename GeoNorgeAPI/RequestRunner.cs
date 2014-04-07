using System;
using System.IO;
using System.Net;
using Arkitektum.GIS.Lib.SerializeUtil;
using www.opengis.net;
using System.Xml.Linq;
using System.Collections.Generic;

namespace GeoNorgeAPI
{
    public class RequestRunner
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private const string ContentTypeXml = "application/xml";
        private const string DefaultGeonetworkEndpoint = "http://www.geonorge.no/geonetwork/";
        
        private readonly string _geonetworkEndpoint;
        private readonly string _geonetworkUsername;
        private readonly string _geonetworkPassword;
        
        private readonly HttpRequestExecutor _httpRequestExecutor;

        private Cookie _sessionCookie;

        private RequestRunner(string geonetworkUsername, string geonetworkPassword, string geonetworkEndpoint, HttpRequestExecutor httpRequestExecutor)
        {
            _geonetworkUsername = geonetworkUsername;
            _geonetworkPassword = geonetworkPassword;
            _geonetworkEndpoint = geonetworkEndpoint;
            _httpRequestExecutor = httpRequestExecutor;
        }

        public RequestRunner(string geonetworkUsername = null, string geonetworkPassword = null, string geonetworkEndpoint = DefaultGeonetworkEndpoint)
            : this(geonetworkUsername, geonetworkPassword, geonetworkEndpoint, new HttpRequestExecutor())
        {
            
        }

        public GetRecordsResponseType RunGetRecordsRequest(GetRecordsType getRecordsRequest)
        {
            var requestBody = SerializeUtil.SerializeToString(getRecordsRequest);
            Console.WriteLine(requestBody);
            string responseBody = _httpRequestExecutor.PostRequest(GetUrlForCswService(), ContentTypeXml, ContentTypeXml, requestBody);
            return SerializeUtil.DeserializeFromString<GetRecordsResponseType>(responseBody);
        }

        public MD_Metadata_Type GetRecordById(GetRecordByIdType request)
        {
            var requestBody = SerializeUtil.SerializeToString(request);
            string responseBody = _httpRequestExecutor.PostRequest(GetUrlForCswService(), ContentTypeXml, ContentTypeXml, requestBody);
            GetRecordByIdResponseType response =  SerializeUtil.DeserializeFromString<GetRecordByIdResponseType>(responseBody);

            MD_Metadata_Type metadataRecord = null;
            if (response != null && response.Items != null && response.Items.Length > 0)
            {
                metadataRecord = response.Items[0] as MD_Metadata_Type;
            }
            return metadataRecord;
        }

        public MetadataTransaction RunCswTransaction(TransactionType request)
        {
            var requestBody = SerializeUtil.SerializeToString(request);

            GeoNetworkAuthenticate();

            Log.Info("Running CSW Transaction.");

            string transactionResponse = _httpRequestExecutor.PostRequest(_geonetworkEndpoint + "srv/eng/csw-publication", 
                "application/xml", "application/xml", requestBody, _sessionCookie);

            Log.Debug(transactionResponse);

            Log.Info("CSW transaction complete.");

            XDocument doc = XDocument.Parse(transactionResponse);
            
            XNamespace csw = "http://www.opengis.net/cat/csw/2.0.2";

            string totalInserted = "0";
            string totalUpdated = "0";
            string totalDeleted = "0";
            var summaryElement = doc.Element(csw + "TransactionResponse").Element(csw + "TransactionSummary");
            if (summaryElement != null)
            {
                var insertedElement = summaryElement.Element(csw + "totalInserted");
                if (insertedElement != null)
                {
                    totalInserted = insertedElement.Value;
                }

                var updatedElement = summaryElement.Element(csw + "totalupdated");
                if (updatedElement != null)
                {
                    totalUpdated = updatedElement.Value;
                }

                var deletedElement = summaryElement.Element(csw + "totaldeleted");
                if (deletedElement != null)
                {
                    totalDeleted = deletedElement.Value;
                }
            }

            List<string> identifiers = new List<string>();
            var identifierElements = doc.Element(csw + "TransactionResponse").Element(csw + "InsertResult").Element(csw + "BriefRecord").Elements("identifier");
            if (identifierElements != null)
            {
                foreach(var identifierElement in identifierElements) {
                    identifiers.Add(identifierElement.Value);
                }
            }

            return new MetadataTransaction
            {
                TotalInserted = totalInserted,
                TotalUpdated = totalUpdated,
                TotalDeleted = totalDeleted,
                Identifiers = identifiers
            };            
        }


        private string GetUrlForCswService()
        {
            return _geonetworkEndpoint + "srv/eng/csw";
        }

        private void GeoNetworkAuthenticate()
        {
            HttpWebResponse logoutResponse = null;
            HttpWebResponse loginResponse = null;
            try
            {
                if (_sessionCookie == null)
                {
                    logoutResponse =
                        _httpRequestExecutor.FullGetRequest(_geonetworkEndpoint + "srv/eng/xml.user.logout",
                                                            "application/xml", "text/plain");
                    logoutResponse.Close();

                    loginResponse =
                        _httpRequestExecutor.FullPostRequest(_geonetworkEndpoint + "srv/eng/xml.user.login",
                                                             "application/xml", "application/x-www-form-urlencoded",
                                                             "username=" + _geonetworkUsername + "&password=" +
                                                             _geonetworkPassword);

                    Cookie sessionCookie = loginResponse.Cookies["JSESSIONID"];
                    if (sessionCookie == null)
                    {
                        var stream = loginResponse.GetResponseStream();
                        if (stream != null)
                        {
                            StreamReader reader = new StreamReader(stream);
                            string responseBody = reader.ReadToEnd();
                            loginResponse.Close();
                            Log.Error("Response:" + responseBody);
                        }
                        throw new Exception("Authentication failed - no session cookie available!");
                    }

                    _sessionCookie = sessionCookie;

                    loginResponse.Close();
                    Log.Debug("Authenticated, session cookie: " + _sessionCookie);
                }
                else
                {
                    Log.Info("Already authenticated, using existing session cookie: " + _sessionCookie);
                }
            }
            finally
            {
                if (logoutResponse != null)
                    logoutResponse.Close();

                if (loginResponse != null)
                    loginResponse.Close();
            }
        }
    }
}
