using System;
using Arkitektum.GIS.Lib.SerializeUtil;
using www.opengis.net;

namespace GeoNorgeAPI
{
    public class RequestRunner
    {
        private const string GeoNetworkEndpoint = "http://www.geonorge.no/geonetwork/";
        private const string ContentTypeXml = "application/xml";

        private readonly HttpRequestExecutor _httpRequestExecutor;

        public RequestRunner(HttpRequestExecutor httpRequestExecutor)
        {
            _httpRequestExecutor = httpRequestExecutor;
        }

        public RequestRunner() : this(new HttpRequestExecutor())
        {
            
        }

        public GetRecordsResponseType RunGetRecordsRequest(GetRecordsType getRecordsRequest)
        {
            var requestBody = SerializeUtil.SerializeToString(getRecordsRequest);
          //  Console.WriteLine(requestBody);
            string responseBody = _httpRequestExecutor.PostRequest(GetUrlForCswSearch(), ContentTypeXml, ContentTypeXml, requestBody);
          //  Console.WriteLine(responseBody);
            return SerializeUtil.DeserializeFromString<GetRecordsResponseType>(responseBody);
        }

        private string GetUrlForCswSearch()
        {
            return GeoNetworkEndpoint + "srv/eng/csw";
        }
    }
}
