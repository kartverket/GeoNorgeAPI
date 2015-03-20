using System;
using Arkitektum.GIS.Lib.SerializeUtil;
using www.opengis.net;
using System.Xml.Linq;
using System.Collections.Generic;

namespace GeoNorgeAPI
{
    public class RequestRunner
    {
        //private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public event LogEventHandlerInfo OnLogEventInfo = delegate { };
        public event LogEventHandlerDebug OnLogEventDebug = delegate { };
        public event LogEventHandlerError OnLogEventError = delegate { };

        private readonly RequestFactory _requestFactory;
        private readonly RequestRunner _requestRunner;
  
        private const string ContentTypeXml = "application/xml";
        private const string DefaultGeonetworkEndpoint = "https://www.geonorge.no/geonetwork/";
        
        private readonly string _geonetworkEndpoint;
        private readonly string _geonetworkUsername;
        private readonly string _geonetworkPassword;
        
        private readonly HttpRequestExecutor _httpRequestExecutor;

        private RequestRunner(string geonetworkUsername, string geonetworkPassword, string geonetworkEndpoint, HttpRequestExecutor httpRequestExecutor)
        {
            _geonetworkUsername = geonetworkUsername;
            _geonetworkPassword = geonetworkPassword;
            _geonetworkEndpoint = geonetworkEndpoint;
            _httpRequestExecutor = httpRequestExecutor;

            _httpRequestExecutor.OnLogEventInfo += new GeoNorgeAPI.LogEventHandlerInfo(LogEventsInfo);
            _httpRequestExecutor.OnLogEventDebug += new GeoNorgeAPI.LogEventHandlerDebug(LogEventsDebug);
            _httpRequestExecutor.OnLogEventError += new GeoNorgeAPI.LogEventHandlerError(LogEventsError);
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
            responseBody = FixInvalidXml(responseBody);
            return SerializeUtil.DeserializeFromString<GetRecordsResponseType>(responseBody);
        }

        private string FixInvalidXml(string input)
        {
            string fixedInput = FixInvalidDateTimeElementInXml(input);
            fixedInput = FixInvalidRealElement(fixedInput);
            return fixedInput;
        }

        private string FixInvalidDateTimeElementInXml(string input)
        {
            return input.Replace(@"<gco:DateTime xmlns:gco=""http://www.isotc211.org/2005/gco"" />", "");
        }

        private string FixInvalidRealElement(string input)
        {
            return input.Replace(@"<gco:Real />", "");
        }

        public MD_Metadata_Type GetRecordById(GetRecordByIdType request)
        {
            var requestBody = SerializeUtil.SerializeToString(request);
            string responseBody = _httpRequestExecutor.PostRequest(GetUrlForCswService(), ContentTypeXml, ContentTypeXml, requestBody);
            responseBody = FixInvalidXml(responseBody);
            GetRecordByIdResponseType response =  SerializeUtil.DeserializeFromString<GetRecordByIdResponseType>(responseBody);

            MD_Metadata_Type metadataRecord = null;
            if (response != null && response.Items != null && response.Items.Length > 0)
            {
                metadataRecord = response.Items[0] as MD_Metadata_Type;
            }


            return metadataRecord;
        }

        public MetadataTransaction RunCswTransaction(TransactionType request, Dictionary<string, string> additionalRequestHeaders)
        {
            var requestBody = SerializeUtil.SerializeToString(request);

            //Log.Info("Running CSW Transaction.");
            OnLogEventInfo("Running CSW Transaction.");

            string transactionResponse = _httpRequestExecutor.PostRequest(_geonetworkEndpoint + "srv/nor/csw-publication", 
                "application/xml", "application/xml", requestBody, _geonetworkUsername, _geonetworkPassword, null, additionalRequestHeaders);

            //Log.Debug(transactionResponse);
            OnLogEventDebug(transactionResponse);

            //Log.Info("CSW transaction complete.");
            OnLogEventInfo("CSW transaction complete.");
            return ParseCswTransactionResponse(transactionResponse);      
        }

        internal static MetadataTransaction ParseCswTransactionResponse(string transactionResponse)
        {

            XDocument doc = XDocument.Parse(transactionResponse);

            XNamespace csw = "http://www.opengis.net/cat/csw/2.0.2";

            string totalInserted = "0";
            string totalUpdated = "0";
            string totalDeleted = "0";
            List<string> identifiers = new List<string>();

            var responseElement = doc.Element(csw + "TransactionResponse");
            if (responseElement != null)
            {
                var summaryElement = responseElement.Element(csw + "TransactionSummary");
                if (summaryElement != null)
                {
                    var insertedElement = summaryElement.Element(csw + "totalInserted");
                    if (insertedElement != null)
                    {
                        totalInserted = insertedElement.Value;
                    }

                    var updatedElement = summaryElement.Element(csw + "totalUpdated");
                    if (updatedElement != null)
                    {
                        totalUpdated = updatedElement.Value;
                    }

                    var deletedElement = summaryElement.Element(csw + "totalDeleted");
                    if (deletedElement != null)
                    {
                        totalDeleted = deletedElement.Value;
                    }
                }

                var insertResult = responseElement.Element(csw + "InsertResult");
                if (insertResult != null)
                {
                    var identifierElements = insertResult.Element(csw + "BriefRecord").Elements("identifier");
                    if (identifierElements != null)
                    {
                        foreach (var identifierElement in identifierElements)
                        {
                            identifiers.Add(identifierElement.Value);
                        }
                    }
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
            return _geonetworkEndpoint + "srv/nor/csw";
        }

        private void LogEventsInfo(string log)
        {
            System.Diagnostics.Debug.WriteLine(log);
            if (OnLogEventInfo != null)
                OnLogEventInfo(log);

        }

        private void LogEventsDebug(string log)
        {
            System.Diagnostics.Debug.WriteLine(log);
            if (OnLogEventDebug != null)
                OnLogEventDebug(log);
        }

        private void LogEventsError(string log, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(log);
            if (OnLogEventError != null)
                OnLogEventError(log, ex);
        }

    }
}
