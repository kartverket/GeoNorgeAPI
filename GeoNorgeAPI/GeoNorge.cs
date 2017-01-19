using System;
using System.Collections.Generic;
using www.opengis.net;

namespace GeoNorgeAPI
{
    /// <summary>
    /// API for communicating with the CSW services available on www.geonorge.no. 
    /// </summary>
    /// 

    public delegate void LogEventHandlerInfo(string msg);
    public delegate void LogEventHandlerDebug(string msg);
    public delegate void LogEventHandlerError(string msg, Exception ex);

    public class GeoNorge : IGeoNorge
    {

        private readonly RequestFactory _requestFactory;
        private readonly RequestRunner _requestRunner;

        public event LogEventHandlerInfo OnLogEventInfo = delegate { };
        public event LogEventHandlerDebug OnLogEventDebug = delegate { };
        public event LogEventHandlerError OnLogEventError = delegate { };

        private GeoNorge(RequestFactory requestFactory, RequestRunner requestRunner)
        {
            _requestFactory = requestFactory;
            _requestRunner = requestRunner;

            _requestRunner.OnLogEventInfo += new GeoNorgeAPI.LogEventHandlerInfo(LogEventsInfo);
            _requestRunner.OnLogEventDebug += new GeoNorgeAPI.LogEventHandlerDebug(LogEventsDebug);
            _requestRunner.OnLogEventError += new GeoNorgeAPI.LogEventHandlerError(LogEventsError);
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
        /// <param name="startPosition">Offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is false</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        public SearchResultsType Search(string searchString, int startPosition = 1, int limit = 20, bool sortByTitle = false)
        {
            GetRecordsType request = _requestFactory.GetRecordsFreeTextSearch(searchString, startPosition, limit, sortByTitle);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }
        /// <summary>
        /// Free text search for records, with ISO 19139 response.
        /// Use numberOfRecordsMatched and nextRecord properties in SearchResults to paginate search. 
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="startPosition">Offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is false</param>
        /// <returns>Results returned in ISO 19139 format (www.opengis.net.RecordType objects).</returns>
        public SearchResultsType SearchIso(string searchString, int startPosition = 1, int limit = 20, bool sortByTitle = false)
        {
            GetRecordsType request = _requestFactory.GetRecordsFreeTextSearch(searchString, startPosition, limit, sortByTitle, "csw:IsoRecord");
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
        /// Search and retrieve records by organisation name. 
        /// Results returned in Dublin Core format (www.opengis.net.RecordType objects).
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is false</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        public SearchResultsType SearchWithOrganisationName(string searchString, int startPosition = 1, int limit = 20, bool sortByTitle = false)
        {
            GetRecordsType request = _requestFactory.GetRecordsOrganisationNameSearch(searchString, startPosition, limit, sortByTitle);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }

        /// <summary>
        /// Search and retrieve records by MetadataPointOfContact. 
        /// Results returned in Dublin Core format (www.opengis.net.RecordType objects).
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is false</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        public SearchResultsType SearchWithOrganisationMetadataPointOfContact(string searchString, int startPosition = 1, int limit = 20, bool sortByTitle = false)
        {
            GetRecordsType request = _requestFactory.GetRecordsOrganisationMetadataPointOfContactSearch(searchString, startPosition, limit, sortByTitle);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }

        /// <summary>
        /// Search and retrieve records by free text together with organisation name.
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="organisationName">Organisation name</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is true</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        public SearchResultsType SearchFreeTextWithOrganisationName(string searchString, string organisationName, int startPosition = 1, int limit = 20, bool sortByTitle = true)
        {
            GetRecordsType request = _requestFactory.GetRecordsFreeTextOrganisationNameSearch(searchString, organisationName, startPosition, limit, sortByTitle);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }

        /// <summary>
        /// Search and retrieve records by free text together with organisation MetadataPointOfContact.
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="organisationName">Organisation name</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is true</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        public SearchResultsType SearchFreeTextWithOrganisationMetadataPointOfContact(string searchString, string organisationName, int startPosition = 1, int limit = 20, bool sortByTitle = true)
        {
            GetRecordsType request = _requestFactory.GetRecordsFreeTextOrganisationMetadataPointOfContactSearch(searchString, organisationName, startPosition, limit, sortByTitle);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }


        /// <summary>
        /// Search for records with an arbitrary number of filters.
        /// </summary>
        /// <param name="filters">See www.opengis.net.FilterType for the type objects that are accepted</param>
        /// <param name="filterNames">Array of names corresponding to the index in filters, see www.opengis.net.ItemsChoiceType23 for possible values</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is false</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        public SearchResultsType SearchWithFilters(object[] filters, ItemsChoiceType23[] filterNames, int startPosition = 1, int limit = 20, bool sortByTitle = false)
        {
            GetRecordsType request = _requestFactory.GetRecordsWithFilter(filters, filterNames, startPosition, limit, sortByTitle);
            return _requestRunner.RunGetRecordsRequest(request).SearchResults;
        }


        /// <summary>
        /// Insert metadata record in GeoNorge.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public MetadataTransaction MetadataInsert(MD_Metadata_Type metadata, Dictionary<string,string> additionalRequestHeaders = null)
        {
            TransactionType request = _requestFactory.MetadataInsert(metadata);
            return _requestRunner.RunCswTransaction(request, additionalRequestHeaders);
        }

        /// <summary>
        /// Update metadata record in GeoNorge.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public MetadataTransaction MetadataUpdate(MD_Metadata_Type metadata, Dictionary<string, string> additionalRequestHeaders = null)
        {
            TransactionType request = _requestFactory.MetadataUpdate(metadata);
            return _requestRunner.RunCswTransaction(request, additionalRequestHeaders);
        }

        /// <summary>
        /// Delete metadata record in GeoNorge.
        /// </summary>
        /// <param name="uuid">identifier of the record to delete</param>
        /// <returns></returns>
        public MetadataTransaction MetadataDelete(string uuid, Dictionary<string, string> additionalRequestHeaders = null)
        {
            TransactionType request = _requestFactory.MetadataDelete(uuid);
            return _requestRunner.RunCswTransaction(request, additionalRequestHeaders);
        }

        private void LogEventsInfo(string log)
        {
            if (OnLogEventInfo != null)
                OnLogEventInfo(log);
        }

        private void LogEventsDebug(string log)
        {
            if (OnLogEventDebug != null)
                OnLogEventDebug(log);
        }

        private void LogEventsError(string log, Exception ex)
        {
            if (OnLogEventError != null)
                OnLogEventError(log, ex);
        }

        
    }
}
