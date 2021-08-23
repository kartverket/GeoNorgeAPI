using NUnit.Framework;
using www.opengis.net;
using GeoNorgeAPI;
using System;
using System.Collections.Generic;
using System.IO;
using Arkitektum.GIS.Lib.SerializeUtil;

namespace GeoNorgeAPI.Tests
{
    
    public class CswTests
    {
        GeoNorge _geonorge;

        [SetUp]
        public void Init()
        {
            _geonorge = new GeoNorge();
        }

        [Test]
        public void ShouldReturnRecordsWhenRunningASimpleSearch()
        {
            var result = _geonorge.Search("wms");

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0, "A search on 'wms' should return records.");
        }
        
        [Test]
        public void ShouldReturnSingleIsoRecord()
        {
            MD_Metadata_Type record = _geonorge.GetRecordByUuid("63c672fa-e180-4601-a176-6bf163e0929d"); // Matrikkelen WMS
            
            Assert.NotNull(record, "Record does not exist.");
        }

        [Test]
        public void ShouldReturnRecordsWhenSearchingWithOrganisationName()
        {
            var result = _geonorge.SearchWithOrganisationName("%Kartverket%");

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0, "An organization name search on '%Kartverket%' should return lots of records.");
        }

        [Test]
        public void ShouldReturnRecordsWhenSearchingWithOrganisationNameIncludingWhitespace()
        {
            var result = _geonorge.SearchWithOrganisationName("Norsk institutt for bioøkonomi");

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0, "An organization name search on 'Norsk institutt for skog og landskap' should return lots of records.");
        }
        [Test]
        public void ShouldReturnServicesFromKartverket()
        {
            var filters = new object[]
                {
                    
                    new BinaryLogicOpType()
                        {
                            Items = new object[]
                                {
                                    new PropertyIsLikeType
                                    {
                                        escapeChar = "\\",
                                        singleChar = "_",
                                        wildCard = "%",
                                        PropertyName = new PropertyNameType {Text = new[] {"OrganisationName"}},
                                        Literal = new LiteralType {Text = new[] { "%Kartverket%" }}
                                    },
                                    new PropertyIsLikeType
                                    {
                                        escapeChar = "\\",
                                        singleChar = "_",
                                        wildCard = "%",
                                        PropertyName = new PropertyNameType {Text = new[] {"Type"}},
                                        Literal = new LiteralType {Text = new[] { "service" }}
                                    }
                                },
                                ItemsElementName = new ItemsChoiceType22[]
                                    {
                                        ItemsChoiceType22.PropertyIsLike, ItemsChoiceType22.PropertyIsLike, 
                                    }
                        },
                    
                };

            var filterNames = new ItemsChoiceType23[]
                {
                    ItemsChoiceType23.And
                };

            var result = _geonorge.SearchWithFilters(filters, filterNames);

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0, "Should have return more than zero datasets from Kartverket.");
        }

        [Test]
        public void ShouldReturnRecordsSpecifiedNumberOfRecords()
        {
            int numberOfRecords = 30;
            var result = _geonorge.Search("data", 1, numberOfRecords);

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0, "A search on 'data' should return records.");
            Assert.AreEqual(numberOfRecords, int.Parse(result.numberOfRecordsReturned), "Should have returned 30 records");
        }

        [Test]
        public void ShouldSearchAndReturnIsoRecords()
        {
            var result = _geonorge.SearchIso("data");

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0);

            MD_Metadata_Type md = result.Items[0] as MD_Metadata_Type;
            Assert.NotNull(md);
        }


        [Test]
        public void ShouldParseCswTransactionAfterUpdateWithoutNullReferenceOnMissingIdentifiers()
        {
            MetadataTransaction transaction = RequestRunner.ParseCswTransactionResponse("<csw:TransactionResponse xmlns:csw=\"http://www.opengis.net/cat/csw/2.0.2\"><csw:TransactionSummary><csw:totalInserted>0</csw:totalInserted><csw:totalUpdated>1</csw:totalUpdated><csw:totalDeleted>0</csw:totalDeleted></csw:TransactionSummary></csw:TransactionResponse>");

            Assert.AreEqual("1", transaction.TotalUpdated);
        }

        /* 
        [Test]
        public void InsertMetadata()
        {
            _geonorge = new GeoNorge("","","https://www.geonorge.no/geonetworkbeta/");

            MD_Metadata_Type metadata = MetadataExample.CreateMetadataExample();
            metadata.fileIdentifier = new CharacterString_PropertyType { CharacterString = Guid.NewGuid().ToString() };

            var transaction = _geonorge.MetadataInsert(metadata, new Dictionary<string, string> { { "GeonorgeUsername", "esk_testbruker" }, { "GeonorgeRole", "nd.metadata_admin" }, { "published", "true" } });

            Assert.NotNull(transaction);
            Assert.AreEqual("1", transaction.TotalInserted);

            Console.WriteLine(transaction.Identifiers);
        }  
        [Test]
        public void UpdateMetadata()
        {
            _geonorge = new GeoNorge("", "", "http://beta.geonorge.no/geonetwork/");

            MD_Metadata_Type metadata = MetadataExample.CreateMetadataExample();

            _geonorge.MetadataUpdate(metadata);
        }

        [Test]
        public void DeleteMetadata()
        {
            _geonorge = new GeoNorge("", "", "http://beta.geonorge.no/geonetwork/");
            _geonorge.MetadataDelete("bcffba00-5396-4f81-ad65-d34d8771eab4");
        }
        */
        [Test]
        public void ShouldReturnConstraints()
        {
            SimpleMetadata metadata = new SimpleMetadata(_geonorge.GetRecordByUuid("e0ce2aae-324c-495c-8740-2adfd20b4386"));
            Assert.NotNull(metadata.Constraints);
        }

        [Test]
        public void ShouldReturnMultipleOnlineElementsForMD_DigitalTransferOptionsAsDistributionsFormats()
        {      
            string xml = File.ReadAllText("xml/multiple-online-transfer-options.xml");
            GetRecordByIdResponseType response = SerializeUtil.DeserializeFromString<GetRecordByIdResponseType>(xml);
            var data = (MD_Metadata_Type)response.Items[0];
            var metadata = new SimpleMetadata(data);
            var distributions = metadata.DistributionsFormats;
            Assert.NotNull(distributions);
        }

        [Test]
        public void ShouldReturnNorwegianAbstractForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var @abstract = metadata.Abstract;
            Assert.AreEqual("Direkte kringkasting beskrivelse", @abstract);
        }

        [Test]
        public void ShouldReturnEnglishAbstractForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var @abstract = metadata.EnglishAbstract;
            Assert.AreEqual("Direct Broadcast data received at MET NORWAY Oslo. Processed by standard processing software to geolocated and calibrated values in satellite swath in received instrument resolution.", @abstract);
        }

        [Test]
        public void ShouldUpdateNorwegianAbstractForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedAbstract = "Abstract norsk";
            metadata.Abstract = expectedAbstract;
            var actualAbstract = metadata.Abstract;
            Assert.AreEqual(expectedAbstract, actualAbstract);
        }

        [Test]
        public void ShouldUpdateEnglishAbstractForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedAbstract = "Abstract english";
            metadata.EnglishAbstract = expectedAbstract;
            var actualAbstract = metadata.EnglishAbstract;
            Assert.AreEqual(expectedAbstract, actualAbstract);
        }

        [Test]
        public void ShouldReturnNorwegianTitleForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var title = metadata.Title;
            Assert.AreEqual("Direkte kringkasting", title);
        }

        [Test]
        public void ShouldReturnEnglishTitleForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var title = metadata.EnglishTitle;
            Assert.AreEqual("Direct Broadcast data processed in satellite swath to L1C.", title);
        }

        [Test]
        public void ShouldUpdateNorwegianTitleForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedTitle = "Title norsk";
            metadata.Title = expectedTitle;
            var actualTitle = metadata.Title;
            Assert.AreEqual(expectedTitle, actualTitle);
        }

        [Test]
        public void ShouldUpdateEnglishTitleForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedTitle = "Title english";
            metadata.EnglishTitle = expectedTitle;
            var actualTitle = metadata.EnglishTitle;
            Assert.AreEqual(expectedTitle, actualTitle);
        }

        [Test]
        public void ShouldReturnNorwegianPurposeForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var purpose = metadata.Purpose;
            Assert.AreEqual("Formål", purpose);
        }

        [Test]
        public void ShouldReturnEnglishPurposeForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var purpose = metadata.EnglishPurpose;
            Assert.AreEqual("Purpose of use", purpose);
        }

        [Test]
        public void ShouldUpdateNorwegianPurposeForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedPurpose = "Purpose norsk";
            metadata.Purpose = expectedPurpose;
            var actualPurpose = metadata.Purpose;
            Assert.AreEqual(expectedPurpose, actualPurpose);
        }

        [Test]
        public void ShouldUpdateEnglishPurposeForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedPurpose = "Purpose english";
            metadata.EnglishPurpose = expectedPurpose;
            var actualPurpose = metadata.EnglishPurpose;
            Assert.AreEqual(expectedPurpose, actualPurpose);
        }

        [Test]
        public void ShouldReturnNorwegianSupplementalDescriptionForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var supplementalDescription = metadata.SupplementalDescription;
            Assert.AreEqual("Hjelp for bruk", supplementalDescription);
        }

        [Test]
        public void ShouldReturnEnglishSupplementalDescriptionForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var supplementalDescription = metadata.EnglishSupplementalDescription;
            Assert.AreEqual("help for use", supplementalDescription);
        }

        [Test]
        public void ShouldUpdateNorwegianSupplementalDescriptionForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedSupplementalDescription = "SupplementalDescription norsk";
            metadata.SupplementalDescription = expectedSupplementalDescription;
            var actualSupplementalDescription = metadata.SupplementalDescription;
            Assert.AreEqual(expectedSupplementalDescription, actualSupplementalDescription);
        }

        [Test]
        public void ShouldUpdateEnglishSupplementalDescriptionForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedSupplementalDescription = "SupplementalDescription english";
            metadata.EnglishSupplementalDescription = expectedSupplementalDescription;
            var actualSupplementalDescription = metadata.EnglishSupplementalDescription;
            Assert.AreEqual(expectedSupplementalDescription, actualSupplementalDescription);
        }

    }
}
