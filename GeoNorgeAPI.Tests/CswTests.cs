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
            Assert.AreEqual("Direktesendte satellittdata mottatt ved Meteorologisk Institutt Oslo. Prosessert med standard prosesseringssoftware til geolokaliserte og kalibrerte verdier i satellitsveip i mottatt instrument oppløsning.", @abstract);
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
            Assert.AreEqual("Direktesendte satellittdata prosessert i satellittsveip til L1C. Test", title);
        }

        [Test]
        public void ShouldReturnEnglishTitleForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var title = metadata.EnglishTitle;
            Assert.AreEqual("Direct Broadcast data processed in satellite swath to L1C. TEST", title);
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

        [Test]
        public void ShouldReturnNorwegianProcessHistoryForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var processHistory = metadata.ProcessHistory;
            Assert.AreEqual("Ingen prosesshistorie", processHistory);
        }

        [Test]
        public void ShouldReturnEnglishProcessHistoryForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var processHistory = metadata.EnglishProcessHistory;
            Assert.AreEqual("No lineage statement has been provided", processHistory);
        }

        [Test]
        public void ShouldUpdateNorwegianProcessHistoryForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedProcessHistory = "ProcessHistory norsk";
            metadata.ProcessHistory = expectedProcessHistory;
            var actualProcessHistory = metadata.ProcessHistory;
            Assert.AreEqual(expectedProcessHistory, actualProcessHistory);
        }

        [Test]
        public void ShouldUpdateEnglishProcessHistoryForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedProcessHistory = "ProcessHistory english";
            metadata.EnglishProcessHistory = expectedProcessHistory;
            var actualProcessHistory = metadata.EnglishProcessHistory;
            Assert.AreEqual(expectedProcessHistory, actualProcessHistory);
        }

        [Test]
        public void ShouldReturnNorwegianContactForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var organization = metadata.ContactOwner.Organization;
            Assert.AreEqual("Meteorologisk institutt", organization);
        }

        [Test]
        public void ShouldReturnEnglishContactForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var organization = metadata.ContactOwner.OrganizationEnglish;
            Assert.AreEqual("Norwegian Meteorological Institute", organization);
        }

        [Test]
        public void ShouldUpdateNorwegianContactForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedContactOrganization = "Organisasjon norsk";
            metadata.ContactOwner = new SimpleContact { Organization = expectedContactOrganization, Role = "owner", OrganizationEnglish = "Organization English" };
            var actualContactOrganization = metadata.ContactOwner.Organization;
            Assert.AreEqual(expectedContactOrganization, actualContactOrganization);
        }

        [Test]
        public void ShouldUpdateEnglishContactForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedContactOrganization = "Organisasjon engelsk";
            metadata.ContactOwner = new SimpleContact { Role = "owner", OrganizationEnglish = expectedContactOrganization };
            var actualContactOrganization = metadata.ContactOwner.OrganizationEnglish;
            Assert.AreEqual(expectedContactOrganization, actualContactOrganization);
        }

        [Test]
        public void ShouldReturnNorwegianKeywordsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var keywords = metadata.Keywords;
            Assert.AreEqual("Jordvitenskap > atmosfære > stråling", keywords[0].Keyword);
        }

        [Test]
        public void ShouldReturnEnglishKeywordsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var keywords = metadata.Keywords;
            Assert.AreEqual("Oceanographic geographical features", keywords[5].EnglishKeyword);
        }

        [Test]
        public void ShouldUpdateNorwegianKeywordsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedKeyword = "Nøkkelord norsk";
            var keywords = new List<SimpleKeyword> {
                new SimpleKeyword {
                    Keyword = expectedKeyword,
                    Thesaurus = SimpleKeyword.TYPE_THEME,
                    EnglishKeyword = "Broadcast data"
                }
            };
            metadata.Keywords = keywords;
            var actualKeyword = metadata.Keywords[0].Keyword;
            Assert.AreEqual(expectedKeyword, actualKeyword);
        }

        [Test]
        public void ShouldUpdateEnglishKeywordsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedKeyword = "Broadcast data2";
            var keywords = new List<SimpleKeyword> {
                new SimpleKeyword {
                    Keyword = "Kringkasting",
                    Thesaurus = SimpleKeyword.TYPE_THEME,
                    EnglishKeyword = expectedKeyword
                }
            };
            metadata.Keywords = keywords;
            var actualKeyword = metadata.Keywords[0].EnglishKeyword;
            Assert.AreEqual(expectedKeyword, actualKeyword);
        }

        [Test]
        public void ShouldReturnNorwegianDistributionsFormatsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var distributionsformats = metadata.DistributionsFormats;
            Assert.AreEqual("kommunevis, fylkesvis, landsfiler, region, celle, kartblad", 
                distributionsformats[0].UnitsOfDistribution);
        }

        [Test]
        public void ShouldReturnEnglishDistributionsFormatsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var distributionsformats = metadata.DistributionsFormats;
            Assert.AreEqual("municipality, county, entire country, region, cell, map tile",
                distributionsformats[0].EnglishUnitsOfDistribution);
        }

        [Test]
        public void ShouldUpdateNorwegianDistributionsFormatsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedUnitsOfDistribution = "UnitsOfDistribution norsk";
            metadata.DistributionsFormats = new List<SimpleDistribution> { new SimpleDistribution { UnitsOfDistribution = expectedUnitsOfDistribution, EnglishUnitsOfDistribution = "UnitsOfDistribution engelsk", FormatName = "GML", FormatVersion = "3.2.1", Organization = "Kartverket", Protocol = "GEONORGE:DOWNLOAD", URL= "https://nedlasting.test.geonorge.no/api/capabilities/" } };
            var actualUnitsOfDistribution = metadata.DistributionsFormats[0].UnitsOfDistribution;
            Assert.AreEqual(expectedUnitsOfDistribution, actualUnitsOfDistribution);
        }

        [Test]
        public void ShouldUpdateEnglishDistributionsFormatsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedUnitsOfDistribution = "UnitsOfDistribution engelsk";
            metadata.DistributionsFormats = metadata.DistributionsFormats = new List<SimpleDistribution> { new SimpleDistribution { UnitsOfDistribution = "UnitsOfDistribution norsk", EnglishUnitsOfDistribution = expectedUnitsOfDistribution, FormatName = "GML", FormatVersion = "3.2.1", Organization = "Kartverket", Protocol = "GEONORGE:DOWNLOAD", URL = "https://nedlasting.test.geonorge.no/api/capabilities/" } };
            var actualUnitsOfDistribution = metadata.DistributionsFormats[0].EnglishUnitsOfDistribution; ;
            Assert.AreEqual(expectedUnitsOfDistribution, actualUnitsOfDistribution);
        }

        [Test]
        public void ShouldReturnNorwegianUseLimitationsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var useLimitations = metadata.Constraints.UseLimitations;
            Assert.AreEqual("Ingen begrensninger på bruk er oppgitt. Se forøvrig lisens.", useLimitations);
        }

        [Test]
        public void ShouldReturnEnglishUseLimitationsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var useLimitations = metadata.Constraints.EnglishUseLimitations;
            Assert.AreEqual("No conditions apply", useLimitations);
        }

        [Test]
        public void ShouldUpdateNorwegianUseLimitationsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedUseLimitations = "UseLimitations norsk";
            metadata.Constraints = new SimpleConstraints { UseLimitations = expectedUseLimitations };
            var actualUseLimitations = metadata.Constraints.UseLimitations;
            Assert.AreEqual(expectedUseLimitations, actualUseLimitations);
        }

        [Test]
        public void ShouldUpdateEnglishUseLimitationsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedUseLimitations = "UseLimitations engelsk";
            metadata.Constraints = new SimpleConstraints { EnglishUseLimitations = expectedUseLimitations };
            var actualUseLimitations = metadata.Constraints.EnglishUseLimitations;
            Assert.AreEqual(expectedUseLimitations, actualUseLimitations);
        }

        public void ShouldReturnNorwegianOtherConstraintsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var otherConstraints = metadata.Constraints.OtherConstraints;
            Assert.AreEqual("Ingen restriksjoner", otherConstraints);
        }

        [Test]
        public void ShouldReturnEnglishOtherConstraintsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var otherConstraints = metadata.Constraints.EnglishOtherConstraints;
            Assert.AreEqual("conditionsUnknown", otherConstraints);
        }

        [Test]
        public void ShouldUpdateNorwegianOtherConstraintsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedOtherConstraints = "OtherConstraints norsk";
            metadata.Constraints = new SimpleConstraints { EnglishOtherConstraints = "OtherConstraints English", OtherConstraints = expectedOtherConstraints };
            var actualOtherConstraints = metadata.Constraints.OtherConstraints;
            Assert.AreEqual(expectedOtherConstraints, actualOtherConstraints);
        }

        [Test]
        public void ShouldUpdateEnglishOtherConstraintsForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedOtherConstraints = "OtherConstraints engelsk";
            metadata.Constraints = new SimpleConstraints { EnglishOtherConstraints = expectedOtherConstraints };
            var actualOtherConstraints = metadata.Constraints.EnglishOtherConstraints;
            Assert.AreEqual(expectedOtherConstraints, actualOtherConstraints);
        }

        [Test]
        public void ShouldReturnNorwegianSpecificUsageForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var specificUsage = metadata.SpecificUsage;
            Assert.AreEqual("Bruksområde", specificUsage);
        }

        [Test]
        public void ShouldReturnEnglishSpecificUsageForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var specificUsage = metadata.EnglishSpecificUsage;
            Assert.AreEqual("Usage", specificUsage);
        }

        [Test]
        public void ShouldUpdateNorwegianSpecificUsageForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedSpecificUsage = "SpecificUsage norsk";
            metadata.SpecificUsage = expectedSpecificUsage;
            var actualSpecificUsage = metadata.SpecificUsage;
            Assert.AreEqual(expectedSpecificUsage, actualSpecificUsage);
        }

        [Test]
        public void ShouldUpdateEnglishSpecificUsageForEnglishMetadata()
        {
            string xml = File.ReadAllText("xml/english-main-language.xml");
            MD_Metadata_Type data = SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml);
            var metadata = new SimpleMetadata(data);
            var expectedSpecificUsage = "SpecificUsage english";
            metadata.EnglishSpecificUsage = expectedSpecificUsage;
            var actualSpecificUsage = metadata.EnglishSpecificUsage;
            Assert.AreEqual(expectedSpecificUsage, actualSpecificUsage);
        }


        [Test]
        public void ShouldReturnFromMets()
        {
            _geonorge = new GeoNorge("", "", "https://data.csw.met.no/?");
            var filters = new object[]
{
                new PropertyIsLikeType
                    {
                        escapeChar = "\\",
                        singleChar = "_",
                        wildCard = "%",
                        PropertyName = new PropertyNameType {Text = new[] {"apiso:ParentIdentifier"}},
                        Literal = new LiteralType {Text = new[] {"no.met:64db6102-14ce-41e9-b93b-61dbb2cb8b4e"}}
                    }
};

            var filterNames = new ItemsChoiceType23[]
            {
                 ItemsChoiceType23.PropertyIsLike,
            };


            var result = _geonorge.SearchWithFilters(filters, filterNames);

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0, "Should have return more than zero datasets from Mets.");
        }

        [Test]
        public void ShouldReturnDateIntervalFromMets()
        {
            _geonorge = new GeoNorge("", "", "https://data.csw.met.no/?");

            ExpressionType[] expressionTypesFromDate = new ExpressionType[2];
            expressionTypesFromDate[0] = new PropertyNameType { Text = new[] { "apiso:TempExtent_begin" } };
            expressionTypesFromDate[1] = new LiteralType { Text = new[] { "2023-05-01" } };

            ExpressionType[] expressionTypesToDate = new ExpressionType[2];
            expressionTypesToDate[0] = new PropertyNameType { Text = new[] { "apiso:TempExtent_end" } };
            expressionTypesToDate[1] = new LiteralType { Text = new[] { "2023-05-31" } };

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
                                        PropertyName = new PropertyNameType {Text = new[] {"apiso:ParentIdentifier"}},
                                        Literal = new LiteralType {Text = new[] { "no.met:64db6102-14ce-41e9-b93b-61dbb2cb8b4e" }}
                                    },
                                    new BinaryComparisonOpType
                                    {                                     
                                       expression = expressionTypesFromDate
                                    }
                                    ,
                                    new BinaryComparisonOpType
                                    {
                                        expression = expressionTypesToDate
                                    }
                                },
      
                                ItemsElementName = new ItemsChoiceType22[]
                                    {
                                        ItemsChoiceType22.PropertyIsLike, ItemsChoiceType22.PropertyIsGreaterThanOrEqualTo,ItemsChoiceType22.PropertyIsLessThanOrEqualTo
                                    }
                        },

                      };

            var filterNames = new ItemsChoiceType23[]
                {
                    ItemsChoiceType23.And
                };


            var result = _geonorge.SearchWithFilters(filters, filterNames,1,20,false, true);

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0, "Should have return more than zero datasets from Mets.");
        }

        [Test]
        public void ShouldReturnSeriesFromMets()
        {
            _geonorge = new GeoNorge("", "", "https://data.csw.met.no/?");

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
                                        PropertyName = new PropertyNameType {Text = new[] {"apiso:Title"}},
                                        Literal = new LiteralType {Text = new[] { "%satellite%" }}
                                    },
                                    new PropertyIsLikeType
                                    {
                                        escapeChar = "\\",
                                        singleChar = "_",
                                        wildCard = "%",
                                        PropertyName = new PropertyNameType {Text = new[] {"apiso:Type"}},
                                        Literal = new LiteralType {Text = new[] { "series" }}
                                    }
                                },

                                ItemsElementName = new ItemsChoiceType22[]
                                    {
                                        ItemsChoiceType22.PropertyIsLike, ItemsChoiceType22.PropertyIsLike
                                    }
                        },

                      };

            var filterNames = new ItemsChoiceType23[]
                {
                    ItemsChoiceType23.And
                };


            var result = _geonorge.SearchWithFilters(filters, filterNames, 1, 20, false, true);

            Assert.Greater(int.Parse(result.numberOfRecordsMatched), 0, "Should have return more than zero datasets from Mets.");
        }

    }
}