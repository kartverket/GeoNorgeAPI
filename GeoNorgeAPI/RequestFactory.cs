using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using www.opengis.net;

namespace GeoNorgeAPI
{
    public class RequestFactory
    {
        public GetRecordsType GetRecordsFreeTextSearch(string searchString, int startPosition = 1, int limit = 20, bool sortByTitle = false, string outputSchema = "csw:Record")
        {
            var words = searchString.Split(' ').ToList();

            if (words.Count < 2)
            {
                searchString = checkString(searchString);
                var filters = new object[]
                {
                    new PropertyIsLikeType
                    {
                        escapeChar = "\\",
                        singleChar = "_",
                        wildCard = "%",
                        PropertyName = new PropertyNameType {Text = new[] {"AnyText"}},
                        Literal = new LiteralType {Text = new[] { searchString }}
                    }
                };
                var filterNames = new ItemsChoiceType23[]
                {
                    ItemsChoiceType23.PropertyIsLike
                };

                return GetRecordsWithFilter(filters, filterNames, startPosition, limit, sortByTitle, outputSchema);
            }
            else 
            {
                var binaryLogicOpTypeObjects = new List<object>();

                var itemsItemsChoiceType22 = new List<ItemsChoiceType22>();

                foreach (var word in words)
                {
                    var search = checkString(word);

                    binaryLogicOpTypeObjects.Add(new PropertyIsLikeType
                    {
                        escapeChar = "\\",
                        singleChar = "_",
                        wildCard = "%",
                        PropertyName = new PropertyNameType { Text = new[] { "AnyText" } },
                        Literal = new LiteralType { Text = new[] { search } }
                    });

                    itemsItemsChoiceType22.Add(ItemsChoiceType22.PropertyIsLike);
                }

                var filters = new object[]
                    {

                        new BinaryLogicOpType()
                            {
                                Items = binaryLogicOpTypeObjects.ToArray(),
                                ItemsElementName = itemsItemsChoiceType22.ToArray()
                            },

                    };

                var filterNames = new ItemsChoiceType23[]
                    {
                        ItemsChoiceType23.And
                    };

                return GetRecordsWithFilter(filters, filterNames, startPosition, limit, sortByTitle, outputSchema);
            }
        }

        public GetRecordByIdType GetRecordById(string uuid)
        {
            GetRecordByIdType getRecordbyId = new GetRecordByIdType();
            getRecordbyId.service = "CSW";
            getRecordbyId.version = "2.0.2";
            getRecordbyId.outputSchema = "csw:IsoRecord";
            getRecordbyId.Id = new[] { uuid };
            getRecordbyId.ElementSetName = new ElementSetNameType { Value = ElementSetType.full };
            return getRecordbyId;
        }

        public GetRecordsType GetRecordsOrganisationNameSearch(string searchString, int startPosition = 1, int limit = 20, bool sortByTitle = false)
        {
            searchString = checkString(searchString);

            var filters = new object[]
                {
                    new PropertyIsLikeType
                        {
                            escapeChar = "\\",
                            singleChar = "_",
                            wildCard = "%",
                            PropertyName = new PropertyNameType {Text = new[] {"OrganisationName"}},
                            Literal = new LiteralType {Text = new[] { searchString }}
                        }
                };
            var filterNames = new ItemsChoiceType23[]
                {
                    ItemsChoiceType23.PropertyIsLike, 
                };

            return GetRecordsWithFilter(filters, filterNames, startPosition, limit, sortByTitle);
        }

        public GetRecordsType GetRecordsOrganisationMetadataPointOfContactSearch(string searchString, int startPosition = 1, int limit = 20, bool sortByTitle = false)
        {
            searchString = checkString(searchString);
            //geonetwork 4 and 2 compability use singleChar ? instead of _
            searchString = searchString.Replace(" ", "?");

            var filters = new object[]
                {
                    new PropertyIsLikeType
                        {
                            escapeChar = "\\",
                            singleChar = "?",
                            wildCard = "%",
                            PropertyName = new PropertyNameType {Text = new[] {"MetadataPointOfContact"}},
                            Literal = new LiteralType {Text = new[] { searchString }}
                        }
                };
            var filterNames = new ItemsChoiceType23[]
                {
                    ItemsChoiceType23.PropertyIsLike,
                };

            return GetRecordsWithFilter(filters, filterNames, startPosition, limit, sortByTitle);
        }

        internal GetRecordsType GetRecordsFreeTextOrganisationNameSearch(string searchString, string organisationName, int startPosition, int limit, bool sortByTitle)
        {
            searchString = checkString(searchString);

            var filters = new object[]
                {
                    new BinaryLogicOpType {
                        ItemsElementName = new ItemsChoiceType22[] { ItemsChoiceType22.PropertyIsLike, ItemsChoiceType22.PropertyIsLike },
                        Items = new object[] {
                            new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "_",
                                wildCard = "%",
                                PropertyName = new PropertyNameType {Text = new[] {"AnyText"}},
                                Literal = new LiteralType {Text = new[] {searchString}}
                            },
                            new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "_",
                                wildCard = "%",
                                PropertyName = new PropertyNameType {Text = new[] {"OrganisationName"}},
                                Literal = new LiteralType {Text = new[] { organisationName }}
                            }
                        }
                    }                    
                };

            var filterNames = new ItemsChoiceType23[]
                {
                    ItemsChoiceType23.And
                };

            return GetRecordsWithFilter(filters, filterNames, startPosition, limit, sortByTitle);
        }

        internal GetRecordsType GetRecordsFreeTextOrganisationMetadataPointOfContactSearch(string searchString, string organisationName, int startPosition, int limit, bool sortByTitle)
        {
            searchString = checkString(searchString);
            organisationName = organisationName.Replace(" ", "?");

            var filters = new object[]
                {
                    new BinaryLogicOpType {
                        ItemsElementName = new ItemsChoiceType22[] { ItemsChoiceType22.PropertyIsLike, ItemsChoiceType22.PropertyIsLike },
                        Items = new object[] {
                            new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "?",
                                wildCard = "%",
                                PropertyName = new PropertyNameType {Text = new[] {"AnyText"}},
                                Literal = new LiteralType {Text = new[] {searchString}}
                            },
                            new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "_",
                                wildCard = "%",
                                PropertyName = new PropertyNameType {Text = new[] {"MetadataPointOfContact"}},
                                Literal = new LiteralType {Text = new[] { organisationName }}
                            }
                        }
                    }
                };

            var filterNames = new ItemsChoiceType23[]
                {
                    ItemsChoiceType23.And
                };

            return GetRecordsWithFilter(filters, filterNames, startPosition, limit, sortByTitle);
        }

        public GetRecordsType GetRecordsWithFilter(object[] filters, ItemsChoiceType23[] filterNames, int startPosition = 1, int limit = 20, bool sortByTitle = false, string outputSchema = "csw:Record", bool sortByDate = false)
        {
            var getRecords = new GetRecordsType();
            getRecords.resultType = ResultType1.results;
            getRecords.startPosition = startPosition.ToString();
            getRecords.maxRecords = limit.ToString();
            getRecords.outputSchema = outputSchema;

            var query = new QueryType();
            query.typeNames = new[] { new XmlQualifiedName("Record", "http://www.opengis.net/cat/csw/2.0.2") };
            var queryConstraint = new QueryConstraintType();
            queryConstraint.version = "1.1.0";
            queryConstraint.Item = new FilterType
            {
                Items = filters,
                ItemsElementName = filterNames
            };
            query.Constraint = queryConstraint;
            query.Items = new object[] { new ElementSetNameType { Value = ElementSetType.full } };

            if (sortByTitle)
            {
                query.SortBy = new SortPropertyType[]
                {
                    new SortPropertyType {
                        PropertyName = new PropertyNameType { Text = new string[] { "Title" } },
                        SortOrder = SortOrderType.ASC
                    }
                };
            }
            if (sortByDate)
            {
                query.SortBy = new SortPropertyType[]
                {
                    new SortPropertyType {
                        PropertyName = new PropertyNameType { Text = new string[] { "dc:date" } },
                        SortOrder = SortOrderType.DESC,
                        SortOrderSpecified = true
                    }
                };
            }
            getRecords.Item = query;

            return getRecords;
        }

        public TransactionType MetadataInsert(MD_Metadata_Type metadata)
        {
            TransactionType cswTransaction = new TransactionType
            {
                service = "CSW",
                version = "2.0.2",
                Items = new object[]
                    {
                        new InsertType()
                            {
                                Items = new object[]
                                    {
                                        metadata
                                    }
                            }
                    }
            };

            return cswTransaction;
        }

        public TransactionType MetadataUpdate(MD_Metadata_Type metadata)
        {
            TransactionType cswTransaction = new TransactionType
            {
                service = "CSW",
                version = "2.0.2",
                Items = new object[]
                    {
                        new UpdateType()
                            {
                                Items = new object[]
                                    {
                                        metadata
                                    }
                            }
                    }
            };
            return cswTransaction;

        }

        public TransactionType MetadataDelete(string uuid)
        {
            TransactionType cswTransaction = new TransactionType
                {
                    service = "CSW",
                    version = "2.0.2",
                    Items = new object[]
                        {
                            new DeleteType()
                                {
                                    Constraint = new QueryConstraintType()
                                        {
                                            version = "1.1.0",
                                            Item = new FilterType()
                                                {
                                                    Items = new object[]
                                                        {
                                                            new PropertyIsLikeType()
                                                            {
                                                                wildCard = "%",
                                                                singleChar = "_",
                                                                escapeChar = "/",
                                                                PropertyName = new PropertyNameType()
                                                                    {
                                                                        Text = new []
                                                                            {
                                                                                "apiso:identifier"
                                                                            }
                                                                    },
                                                                Literal = new LiteralType()
                                                                    {
                                                                        Text = new []
                                                                            {
                                                                                uuid
                                                                            }
                                                                    }
                                                            }   
                                                        },
                                                        ItemsElementName = new ItemsChoiceType23[]
                                                            {
                                                                ItemsChoiceType23.PropertyIsLike,
                                                            }
                                                }
                                        }
                                }
                        }
                };
            return cswTransaction;
        }

        private string checkString(string searchString)
        {
            //geonetwork 4 needs to escape wildCard %, alternativly use * as wildCard?

            if(string.IsNullOrEmpty(searchString))
                searchString = "%";

            if (searchString.Contains("%"))
            {
                searchString = searchString.Replace("%", "\\%");
            }

            return searchString;
        }

    }
}
