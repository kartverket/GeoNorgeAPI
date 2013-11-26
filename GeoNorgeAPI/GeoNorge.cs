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

        public GeoNorge(string geonetworkUsername = null, string geonetworkPassword = null)
            : this(new RequestFactory(), new RequestRunner(geonetworkUsername, geonetworkPassword))
        {
            
        }

        public GeoNorge(string geonetworkUsername, string geonetworkPassword, string geonetworkEndpoint)
            : this(new RequestFactory(), new RequestRunner(geonetworkUsername, geonetworkPassword, geonetworkEndpoint))
        {

        }

        /// <summary>
        /// Free text search for records.
        /// Use numberOfRecordsMatched and nextRecord properties in SearchResults to paginate search. 
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        public SearchResultsType Search(string searchString, int startPosition = 1)
        {
            GetRecordsType request = _requestFactory.GetRecordsFreeTextSearch(searchString, startPosition);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }

        /// <summary>
        /// Return single record in ISO 19139 format.
        /// </summary>
        /// <param name="uuid">Identifier of the metadata record to return</param>
        /// <returns>The record or null when not found.</returns>
        public MD_Metadata_Type GetRecordByUuid(string uuid)
        {
            GetRecordByIdType request = _requestFactory.GetRecordById(uuid);
            return _requestRunner.GetRecordById(request);
        }

        /// <summary>
        /// Return single record in ISO 19139 format wrapped in a SimpleMetadata class. 
        /// </summary>
        /// <param name="uuid">Identifier of the metadata record to return</param>
        /// <returns>The record or null when not found.</returns>
        public SimpleMetadata GetRecordByUuidSimple(string uuid)
        {
            var record = GetRecordByUuid(uuid);
            if (record == null)
                return null;

            return new SimpleMetadata(record);            
        }
        
        /// <summary>
        /// Search and retrieve records by organisation name. 
        /// Results returned in Dublin Core format (www.opengis.net.RecordType objects).
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        public SearchResultsType SearchWithOrganisationName(string searchString, int startPosition = 1)
        {
            GetRecordsType request = _requestFactory.GetRecordsOrganisationNameSearch(searchString, startPosition);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }

        /// <summary>
        /// Search for records with an arbitrary number of filters.
        /// </summary>
        /// <param name="filters">See www.opengis.net.FilterType for the type objects that are accepted</param>
        /// <param name="filterNames">Array of names corresponding to the index in filters, see www.opengis.net.ItemsChoiceType23 for possible values</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        public SearchResultsType SearchWithFilters(object[] filters, ItemsChoiceType23[] filterNames, int startPosition = 1)
        {
            GetRecordsType request = _requestFactory.GetRecordsWithFilter(filters, filterNames, startPosition);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }


        /// <summary>
        /// Insert metadata record in GeoNorge.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public TransactionResponseType MetadataInsert(MD_Metadata_Type metadata)
        {
            TransactionType request = _requestFactory.MetadataInsert(metadata);
            return _requestRunner.RunCswTransaction(request);
        }

        /// <summary>
        /// Update metadata record in GeoNorge.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public TransactionResponseType MetadataUpdate(MD_Metadata_Type metadata)
        {
            TransactionType request = _requestFactory.MetadataUpdate(metadata);
            return _requestRunner.RunCswTransaction(request);
        }

        /// <summary>
        /// Delete metadata record in GeoNorge.
        /// </summary>
        /// <param name="uuid">identifier of the record to delete</param>
        /// <returns></returns>
        public TransactionResponseType MetadataDelete(string uuid)
        {
            TransactionType request = _requestFactory.MetadataDelete(uuid);
            return _requestRunner.RunCswTransaction(request);
        }
        
    }
}
