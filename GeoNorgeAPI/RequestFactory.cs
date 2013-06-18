using System.Xml;
using www.opengis.net;

namespace GeoNorgeAPI
{
    public class RequestFactory
    {
        public GetRecordsType GetRecordsFreeTextSearch(string searchString, int startPosition = 1)
        {
            var getRecords = new GetRecordsType();
            getRecords.resultType = ResultType1.results;
            getRecords.startPosition = startPosition.ToString();

            var query = new QueryType();
            query.typeNames = new[] { new XmlQualifiedName("Record", "http://www.opengis.net/cat/csw/2.0.2") };
            var queryConstraint = new QueryConstraintType();
            queryConstraint.version = "1.1.0";
            queryConstraint.Item = new FilterType
                {
                    Items = new object[]
                        {
                            new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "_",
                                wildCard = "%",
                                PropertyName = new PropertyNameType { Text = new [] {"AnyText"} },
                                Literal = new LiteralType { Text = new [] { searchString } }
                            }  
                        },
                        ItemsElementName = new [] { ItemsChoiceType23.PropertyIsLike }
                };

            query.Constraint = queryConstraint;
            getRecords.Item = query;

            return getRecords;
        }
    }
}
