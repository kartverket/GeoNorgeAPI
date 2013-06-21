using NUnit.Framework;
using www.opengis.net;

namespace GeoNorgeAPI.Tests
{
    
    public class CswTests
    {

        [Test]
        public void ShouldReturnRecordsWhenRunningASimpleSearch()
        {
            var geonorge = new GeoNorge();
            var result = geonorge.Search("wms");

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0, "A search on 'wms' should return records.");
        }
        
        [Test]
        public void ShouldReturnSingleIsoRecord()
        {
            var geonorge = new GeoNorge();
            MD_Metadata_Type record = geonorge.GetRecordByUuid("63c672fa-e180-4601-a176-6bf163e0929d"); // Matrikkelen WMS
            
            Assert.NotNull(record, "Record does not exist.");
        }

        [Test]
        public void ShouldReturnRecordsWhenSearchingWithOrganisationName()
        {
            var geonorge = new GeoNorge();
            var result = geonorge.SearchWithOrganisationName("%kartverk%");

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0, "An organization name search on '%kartverk%' should return lots of records.");
        }

    }
}
