using NUnit.Framework;

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

    }
}
