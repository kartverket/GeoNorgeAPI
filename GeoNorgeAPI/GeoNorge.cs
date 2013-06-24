using www.opengis.net;

namespace GeoNorgeAPI
{
    /// <summary>
    /// API for communicating with the CSW services available on www.geonorge.no. 
    /// </summary>
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
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }

        /// <summary>
        /// Return single record in ISO 19139 format.
        /// </summary>
        /// <param name="uuid">identifier of the metadata record to return</param>
        /// <returns>The record or null when not found.</returns>
        public MD_Metadata_Type GetRecordByUuid(string uuid)
        {
            GetRecordByIdType request = _requestFactory.GetRecordById(uuid);
            return _requestRunner.GetRecordById(request);
        }
        
        /// <summary>
        /// Search and retrieve records by organisation name. 
        /// Results returned in Dublin Core format (www.opengis.net.RecordType objects).
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        public SearchResultsType SearchWithOrganisationName(string searchString, int startPosition = 1)
        {
            GetRecordsType request = _requestFactory.GetRecordsOrganisationNameSearch(searchString, startPosition);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }

        /// <summary>
        /// Search for records with an arbitrary number of filters.
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="filterNames"></param>
        /// <param name="startPosition"></param>
        /// <returns></returns>
        public SearchResultsType SearchWithFilters(object[] filters, ItemsChoiceType23[] filterNames, int startPosition = 1)
        {
            GetRecordsType request = _requestFactory.GetRecordsWithFilter(filters, filterNames, startPosition);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }

    }
}
