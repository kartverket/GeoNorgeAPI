using www.opengis.net;

namespace GeoNorgeAPI
{
    public class GeoNorge
    {

        private readonly RequestFactory _requestFactory;
        private readonly RequestRunner _requestRunner;

        private GeoNorge(RequestFactory requestFactory, RequestRunner requestRunner)
        {
            _requestFactory = requestFactory;
            _requestRunner = requestRunner;
        }

        public GeoNorge() : this(new RequestFactory(), new RequestRunner())
        {
            
        }

        /// <summary>
        /// Free text search for records.
        /// Results returned in Dublin Core format (www.opengis.net.RecordType objects).
        /// Use numberOfRecordsMatched and nextRecord properties in SearchResults to paginate search. 
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        public SearchResultsType Search(string searchString, int startPosition = 1)
        {
            GetRecordsType request = _requestFactory.GetRecordsFreeTextSearch(searchString, startPosition);
            GetRecordsResponseType response = _requestRunner.RunGetRecordsRequest(request);
            return response.SearchResults;
        }


    }
}
