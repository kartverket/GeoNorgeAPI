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

        public SearchResultsType Search(string searchString)
        {
            GetRecordsType request = _requestFactory.GetRecordsFreeTextSearch(searchString);
            GetRecordsResponseType response = _requestRunner.RunGetRecordsRequest(request);
            return response.SearchResults;
        }


    }
}
