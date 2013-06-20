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

        private string GetUrlForCswService()
        {
            return GeoNetworkEndpoint + "srv/eng/csw";
        }

    }
}
