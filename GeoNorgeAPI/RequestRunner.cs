using System;
using Arkitektum.GIS.Lib.SerializeUtil;
using www.opengis.net;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace GeoNorgeAPI
{
    public class RequestRunner
    {
        //private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public event LogEventHandlerInfo OnLogEventInfo = delegate { };
        public event LogEventHandlerDebug OnLogEventDebug = delegate { };
        public event LogEventHandlerError OnLogEventError = delegate { };

        //private readonly RequestFactory _requestFactory;
        //private readonly RequestRunner _requestRunner;
  
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
            requestBody = FixRequest(requestBody);
            Console.WriteLine(requestBody);
            OnLogEventDebug(requestBody);
            string responseBody = _httpRequestExecutor.PostRequest(GetUrlForCswService(), ContentTypeXml, ContentTypeXml, requestBody);
            responseBody = FixInvalidXml(responseBody);

            // Check if the response is an ExceptionReport before trying to deserialize
            if (IsExceptionReport(responseBody))
            {
                var exceptionMessage = ExtractExceptionMessage(responseBody);
                throw new InvalidOperationException($"CSW service returned an error: {exceptionMessage}");
            }

            return SerializeUtil.DeserializeFromString<GetRecordsResponseType>(responseBody);
        }

        public GetRecordsResponseType RunGetRecordsRequest()
        {
            var response = _httpRequestExecutor.FullGetRequest(GetUrlForCswService(), "application/xml", ContentTypeXml);
            var responseBody = new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd();
            responseBody = FixInvalidXml(responseBody);

            // Check if the response is an ExceptionReport before trying to deserialize
            if (IsExceptionReport(responseBody))
            {
                var exceptionMessage = ExtractExceptionMessage(responseBody);
                throw new InvalidOperationException($"CSW service returned an error: {exceptionMessage}");
            }

            return SerializeUtil.DeserializeFromString<GetRecordsResponseType>(responseBody);
        }

        private string FixRequest(string requestBody)
        {
            if (_geonetworkEndpoint.Contains("met.no"))
            {
                requestBody = FixAbstractExpressionElements(requestBody);
                requestBody = requestBody.Replace(@"outputSchema=""csw:Record""", @"outputSchema=""http://www.isotc211.org/2005/gmd""");
                requestBody = requestBody.Replace(@"outputSchema=""csw:IsoRecord""", @"outputSchema=""http://www.isotc211.org/2005/gmd""");
            }

            return requestBody;
        }

        private string FixAbstractExpressionElements(string requestBody)
        {
            try
            {
                var doc = XDocument.Parse(requestBody);
                var xsiNs = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                var ogcNs = XNamespace.Get("http://www.opengis.net/ogc");

                // Find all expression elements - both with and without namespaces
                var expressionElements = doc.Descendants()
                    .Where(e => e.Name.LocalName == "expression")
                    .ToList();

                foreach (var element in expressionElements)
                {
                    var typeAttr = element.Attribute(xsiNs + "type");

                    // Handle elements with xsi:type attributes
                    if (typeAttr != null)
                    {
                        if (typeAttr.Value == "PropertyNameType" || typeAttr.Value.EndsWith(":PropertyNameType"))
                        {
                            var newElement = new XElement(ogcNs + "PropertyName", element.Value);
                            CopyAttributesExceptType(element, newElement, xsiNs);
                            element.ReplaceWith(newElement);
                        }
                        else if (typeAttr.Value == "LiteralType" || typeAttr.Value.EndsWith(":LiteralType"))
                        {
                            var newElement = new XElement(ogcNs + "Literal", element.Value);
                            CopyAttributesExceptType(element, newElement, xsiNs);
                            element.ReplaceWith(newElement);
                        }
                    }
                    else
                    {
                        // Handle abstract expression elements without xsi:type
                        // Try to determine the type from context or content
                        var parent = element.Parent;
                        if (parent?.Name.LocalName == "PropertyIsGreaterThanOrEqualTo" || 
                            parent?.Name.LocalName == "PropertyIsLessThanOrEqualTo" ||
                            parent?.Name.LocalName == "PropertyIsEqualTo" ||
                            parent?.Name.LocalName == "PropertyIsNotEqualTo" ||
                            parent?.Name.LocalName == "PropertyIsLike")
                        {
                            // In binary comparison operations, first expression is usually PropertyName, second is Literal
                            var siblings = parent.Elements().Where(e => e.Name.LocalName == "expression").ToList();
                            var index = siblings.IndexOf(element);

                            if (index == 0)
                            {
                                // First expression is typically PropertyName
                                var newElement = new XElement("PropertyName", element.Value);
                                CopyAttributesExceptType(element, newElement, xsiNs);
                                element.ReplaceWith(newElement);
                            }
                            else if (index == 1)
                            {
                                // Second expression is typically Literal
                                var newElement = new XElement("Literal", element.Value);
                                CopyAttributesExceptType(element, newElement, xsiNs);
                                element.ReplaceWith(newElement);
                            }
                        }
                    }
                }

                return doc.ToString();
            }
            catch (XmlException)
            {
                // Fallback to string replacement if XML parsing fails
                return FixAbstractExpressionElementsLegacy(requestBody);
            }
        }

        private void CopyAttributesExceptType(XElement source, XElement target, XNamespace xsiNs)
        {
            foreach (var attr in source.Attributes().Where(a => a.Name != xsiNs + "type"))
            {
                target.SetAttributeValue(attr.Name, attr.Value);
            }
        }

        private string FixAbstractExpressionElementsLegacy(string requestBody)
        {
            // Legacy string-based approach for fallback
            // Fix PropertyName expressions - specific cases first
            requestBody = requestBody.Replace(@"<expression xsi:type=""PropertyNameType"">apiso:TempExtent_begin</expression>", "<PropertyName>apiso:TempExtent_begin</PropertyName>");
            requestBody = requestBody.Replace(@"<expression xsi:type=""PropertyNameType"">apiso:TempExtent_end</expression>", "<PropertyName>apiso:TempExtent_end</PropertyName>");

            // Generic fix for any remaining PropertyName expressions with xsi:type
            requestBody = System.Text.RegularExpressions.Regex.Replace(
                requestBody,
                @"<expression xsi:type=""[^""]*PropertyNameType[^""]*"">([^<]+)</expression>",
                "<PropertyName>$1</PropertyName>");

            // Generic fix for Literal expressions with xsi:type
            requestBody = System.Text.RegularExpressions.Regex.Replace(
                requestBody,
                @"<expression xsi:type=""[^""]*LiteralType[^""]*"">([^<]*)</expression>",
                "<Literal>$1</Literal>");

            // Handle abstract expression elements without xsi:type - more aggressive approach
            // This pattern looks for expression elements within comparison operators
            requestBody = System.Text.RegularExpressions.Regex.Replace(
                requestBody,
                @"(<(PropertyIs[^>]*|BinaryComparison[^>]*)>[^<]*)<expression>([^<]+)</expression>",
                "$1<PropertyName>$3</PropertyName>");

            requestBody = System.Text.RegularExpressions.Regex.Replace(
                requestBody,
                @"(<PropertyName>[^<]+</PropertyName>[^<]*)<expression>([^<]*)</expression>",
                "$1<Literal>$2</Literal>");

            return requestBody;
        }

        private bool IsExceptionReport(string responseBody)
        {
            return responseBody.Contains("<ows:ExceptionReport") || responseBody.Contains(":ExceptionReport");
        }

        private string ExtractExceptionMessage(string responseBody)
        {
            try
            {
                var doc = XDocument.Parse(responseBody);
                var owsNs = XNamespace.Get("http://www.opengis.net/ows");

                var exceptionText = doc.Descendants(owsNs + "ExceptionText").FirstOrDefault()?.Value;
                if (exceptionText != null)
                {
                    return exceptionText;
                }

                // Fallback: look for ExceptionText without namespace
                exceptionText = doc.Descendants().Where(e => e.Name.LocalName == "ExceptionText").FirstOrDefault()?.Value;
                return exceptionText ?? "Unknown CSW service error";
            }
            catch
            {
                return "Error parsing exception response";
            }
        }

        private string FixInvalidXml(string input)
        {
            string fixedInput = FixInvalidDateTimeElementInXml(input);
            fixedInput = FixInvalidRealElement(fixedInput);
            fixedInput = FixInvalidBooleanElement(fixedInput);
            fixedInput = FixInvalidTopicCategoryCodeElement(fixedInput);
            fixedInput = FixInvalidDateElement(fixedInput);
            return fixedInput;
        }

        private string FixInvalidDateElement(string input)
        {
            input = input.Replace("<gco:Date/>", $"<gco:Date>{DateTime.Now.ToString("yyyy-MM-dd")}</gco:Date>");
            return input;
        }

        private string FixInvalidTopicCategoryCodeElement(string input)
        {
            input = input.Replace(@"<gmd:MD_TopicCategoryCode>Not available</gmd:MD_TopicCategoryCode>", "<gmd:MD_TopicCategoryCode>environment</gmd:MD_TopicCategoryCode>"); 
            return input;
        }

        private string FixInvalidBooleanElement(string input)
        {
            input = input.Replace(@"<gco:Boolean/>", "");
            input = input.Replace(@"<gco:Boolean />", "");
            return input;
        }

        private string FixInvalidDateTimeElementInXml(string input)
        {
            return input.Replace(@"<gco:DateTime xmlns:gco=""http://www.isotc211.org/2005/gco"" />", "");
        }

        private string FixInvalidRealElement(string input)
        {
            input = input.Replace(@"<gco:Real />", "");
            input = input.Replace(@"<gco:Real xmlns:gco=""http://www.isotc211.org/2005/gco"" />", "");
            return input;
        }

        public MD_Metadata_Type GetRecordById(GetRecordByIdType request)
        {
            var requestBody = SerializeUtil.SerializeToString(request);
            requestBody = FixRequest(requestBody);
            string responseBody = _httpRequestExecutor.PostRequest(GetUrlForCswService(), ContentTypeXml, ContentTypeXml, requestBody);
            responseBody = FixInvalidXml(responseBody);

            // Check if the response is an ExceptionReport before trying to deserialize
            if (IsExceptionReport(responseBody))
            {
                var exceptionMessage = ExtractExceptionMessage(responseBody);
                throw new InvalidOperationException($"CSW service returned an error: {exceptionMessage}");
            }

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
            requestBody = SetNameSpace(requestBody);
            System.Diagnostics.Debug.Write(requestBody);

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

        private string SetNameSpace(string requestBody)
        {
            if (requestBody.Contains(@"codeListValue=""service"""))
                requestBody = requestBody.Replace("<gmd:MD_Metadata>", @"<gmd:MD_Metadata xmlns:gmd=""http://www.isotc211.org/2005/gmd"" xmlns:gco=""http://www.isotc211.org/2005/gco"" xmlns:geonet=""http://www.fao.org/geonetwork"" xmlns:gml=""http://www.opengis.net/gml/3.2"" xmlns:gmx=""http://www.isotc211.org/2005/gmx"" xmlns:gts=""http://www.isotc211.org/2005/gts"" xmlns:srv=""http://www.isotc211.org/2005/srv"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.isotc211.org/2005/gmd https://inspire.ec.europa.eu/draft-schemas/inspire-md-schemas-temp/apiso-inspire/apiso-inspire.xsd"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">");
            else
                requestBody = requestBody.Replace( "<gmd:MD_Metadata>", @"<gmd:MD_Metadata xmlns:gmd=""http://www.isotc211.org/2005/gmd"" xmlns:gco=""http://www.isotc211.org/2005/gco"" xmlns:gmx=""http://www.isotc211.org/2005/gmx"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:gml=""http://www.opengis.net/gml"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xsi:schemaLocation=""http://www.isotc211.org/2005/gmd http://schemas.opengis.net/iso/19139/20060504/gmd/gmd.xsd http://www.isotc211.org/2005/gmx http://schemas.opengis.net/iso/19139/20060504/gmx/gmx.xsd"">");

            return requestBody;
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
                    var briefRecord = insertResult.Element(csw + "BriefRecord");
                    if(briefRecord != null)
                    {
                        var identifierElements = briefRecord.Elements("identifier");
                        if (identifierElements != null)
                        {
                            foreach (var identifierElement in identifierElements)
                            {
                                identifiers.Add(identifierElement.Value);
                            }
                        }
                    }
                    else 
                    {
                        var record = insertResult.Element(csw + "Record");
                        if (record != null)
                        {
                            var identifierElements = record.Elements("identifier");
                            if (identifierElements != null)
                            {
                                foreach (var identifierElement in identifierElements)
                                {
                                    identifiers.Add(identifierElement.Value);
                                }
                            }
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
            if(_geonetworkEndpoint.Contains("csw.nina.no"))
                return _geonetworkEndpoint;
            else
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
