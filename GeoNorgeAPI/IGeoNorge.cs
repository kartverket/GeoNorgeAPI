using System.Collections.Generic;
using www.opengis.net;

namespace GeoNorgeAPI
{
    public interface IGeoNorge
    {
        /// <summary>
        /// Free text search for records.
        /// Use numberOfRecordsMatched and nextRecord properties in SearchResults to paginate search. 
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="startPosition">Offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is false</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        SearchResultsType Search(string searchString, int startPosition = 1, int limit = 20, bool sortByTitle = false);

        /// <summary>
        /// Free text search for records, with ISO 19139 response.
        /// Use numberOfRecordsMatched and nextRecord properties in SearchResults to paginate search. 
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="startPosition">Offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is false</param>
        /// <returns>Results returned in ISO 19139 format (www.opengis.net.RecordType objects).</returns>
        SearchResultsType SearchIso(string searchString, int startPosition = 1, int limit = 20, bool sortByTitle = false);

        /// <summary>
        /// Return single record in ISO 19139 format.
        /// </summary>
        /// <param name="uuid">Identifier of the metadata record to return</param>
        /// <returns>The record or null when not found.</returns>
        MD_Metadata_Type GetRecordByUuid(string uuid);

        /// <summary>
        /// Search and retrieve records by organisation name. 
        /// Results returned in Dublin Core format (www.opengis.net.RecordType objects).
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is false</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        SearchResultsType SearchWithOrganisationName(string searchString, int startPosition = 1, int limit = 20, bool sortByTitle = false);

        /// <summary>
        /// Search and retrieve records by free text together with organisation name.
        /// </summary>
        /// <param name="searchString">Search string, use % as wildcard</param>
        /// <param name="organisationName">Organisation name</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is true</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        SearchResultsType SearchFreeTextWithOrganisationName(string searchString, string organisationName, int startPosition = 1, int limit = 20, bool sortByTitle = true);

        /// <summary>
        /// Search for records with an arbitrary number of filters.
        /// </summary>
        /// <param name="filters">See www.opengis.net.FilterType for the type objects that are accepted</param>
        /// <param name="filterNames">Array of names corresponding to the index in filters, see www.opengis.net.ItemsChoiceType23 for possible values</param>
        /// <param name="startPosition">Search offset for pagination of results</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="sortByTitle">Sort results by title, default value is false</param>
        /// <returns>Results returned in Dublin Core format (www.opengis.net.RecordType objects).</returns>
        SearchResultsType SearchWithFilters(object[] filters, ItemsChoiceType23[] filterNames, int startPosition = 1, int limit = 20, bool sortByTitle = false);

        /// <summary>
        /// Insert metadata record in GeoNorge.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        MetadataTransaction MetadataInsert(MD_Metadata_Type metadata, Dictionary<string,string> additionalRequestHeaders = null);

        /// <summary>
        /// Update metadata record in GeoNorge.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        MetadataTransaction MetadataUpdate(MD_Metadata_Type metadata, Dictionary<string, string> additionalRequestHeaders = null);

        /// <summary>
        /// Delete metadata record in GeoNorge.
        /// </summary>
        /// <param name="uuid">identifier of the record to delete</param>
        /// <returns></returns>
        MetadataTransaction MetadataDelete(string uuid, Dictionary<string, string> additionalRequestHeaders = null);
    }
}