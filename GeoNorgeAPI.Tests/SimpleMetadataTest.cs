using NUnit.Framework;
using System;
using www.opengis.net;
using GeoNorgeAPI;
using System.Collections.Generic;

namespace GeoNorgeAPI.Tests
{
    [TestFixture]
    public class SimpleMetadataTest
    {
        SimpleMetadata _md;

        [SetUp]
        public void Init()
        {
            _md = new SimpleMetadata(MetadataExample.CreateMetadataExample());
        }

        [Test]
        public void ShouldReturnTitle()
        {
            Assert.AreEqual("Eksempeldatasettet sin tittel.", _md.Title);
        }

        [Test]
        public void ShouldThrowExceptionWhenSettingTitleOnMetadataWithEmptyIdentificationInfoElement()
        {
            _md = new SimpleMetadata(new MD_Metadata_Type());
            Assert.Throws(typeof(NullReferenceException), new TestDelegate(UpdateTitle));
        }

        [Test]
        public void ShouldThrowExceptionWhenSettingTitleOnMetadataWithEmptyIdentificationInfoArrayElement()
        {
            _md = new SimpleMetadata(new MD_Metadata_Type() { 
                identificationInfo = new MD_Identification_PropertyType[] { } 
            });
            Assert.Throws(typeof(NullReferenceException), new TestDelegate(UpdateTitle));
        }

        [Test]
        public void ShouldThrowExceptionWhenSettingTitleOnMetadataWithEmptyAbstractIdentificationElement()
        {
            _md = new SimpleMetadata(new MD_Metadata_Type() { 
                identificationInfo = new MD_Identification_PropertyType[] { 
                    new MD_Identification_PropertyType() 
                } 
            });
            Assert.Throws(typeof(NullReferenceException), new TestDelegate(UpdateTitle));
        }

        [Test]
        public void ShouldSetTitleWhenMissingCitation()
        {
            _md = new SimpleMetadata(new MD_Metadata_Type() { 
                identificationInfo = new MD_Identification_PropertyType[] { 
                    new MD_Identification_PropertyType() { 
                        AbstractMD_Identification = new MD_DataIdentification_Type() 
                    } 
                } 
            });

            UpdateTitle();

            Assert.AreEqual("This is the new title.", _md.Title);
        }

        private void UpdateTitle()
        {
            _md.Title = "This is the new title.";
        }

        [Test]
        public void ShouldReturnUuid()
        {
            Assert.AreEqual("12345-67890-aabbcc-ddeeff-ggffhhjj", _md.Uuid);
        }

        [Test]
        public void ShouldUpdateUuid()
        {
            string newUuid = "aaabbb-cccc-dddd-eeee-ffff-gggg";
            _md.Uuid = newUuid;

            Assert.AreEqual(newUuid, _md.Uuid);
        }

        [Test]
        public void ShouldReturnHierarchyLevel()
        {
            Assert.AreEqual("dataset", _md.HierarchyLevel);
        }

        [Test]
        public void ShouldUpdateHierarchyLevelWhenNoHierarchyLevelExists()
        {
            _md.GetMetadata().hierarchyLevel = null;

            string newHierarchyLevel = "service";
            _md.HierarchyLevel = newHierarchyLevel;
            
            Assert.AreEqual(newHierarchyLevel, _md.HierarchyLevel);
        }

        [Test]
        public void ShouldUpdateHierarchyLevelWhenHierarchyLevelExists()
        {
            string newHierarchyLevel = "service";
            _md.HierarchyLevel = newHierarchyLevel;

            Assert.AreEqual(newHierarchyLevel, _md.HierarchyLevel);
        }

        [Test]
        public void ShouldReturnAbstract()
        {
            Assert.AreEqual("Dette datasettet inneholder ditt og datt. Kan brukes til dette, men må ikke brukes til andre ting som for eksempel dette.", _md.Abstract);
        }

        [Test]
        public void ShouldUpdateAbstract()
        {
            string newAbstract = "Nytt sammendrag";
            _md.Abstract = newAbstract;

            Assert.AreEqual(newAbstract, _md.Abstract);
        }

        [Test]
        public void ShouldReturnPurpose()
        {
            Assert.AreEqual("Dette er formålet.", _md.Purpose);
        }

        [Test]
        public void ShouldUpdatePurpose()
        {
            string newPurpose = "Nytt formål";
            _md.Purpose = newPurpose;

            Assert.AreEqual(newPurpose, _md.Purpose);
        }
       
        [Test]
        public void ShouldReturnSupplementalDescription()
        {
            Assert.AreEqual("Dette er den utfyllende informasjonen.", _md.SupplementalDescription);
        }

        [Test]
        public void ShouldUpdateSupplementalDescription()
        {
            string newSupplementalDescription = "Dette er nytt.";
            _md.SupplementalDescription = newSupplementalDescription;

            Assert.AreEqual(newSupplementalDescription, _md.SupplementalDescription);
        }

        [Test]
        public void ShouldReturnKeywords()
        {
            List<SimpleKeyword> keywords = _md.Keywords;

            Assert.AreEqual(1, keywords.Count);
        }

        [Test]
        public void ShouldSetSpatialRepresentationWhenSpatialRepresentationIsNull()
        {
            _md.SpatialRepresentation = "vector";

            MD_Identification_PropertyType identification = (MD_Identification_PropertyType)_md.GetMetadata().identificationInfo[0];
            MD_DataIdentification_Type dataIdentification = (MD_DataIdentification_Type)identification.AbstractMD_Identification;

            Assert.AreEqual("vector", dataIdentification.spatialRepresentationType[0].MD_SpatialRepresentationTypeCode.codeListValue);
        }

        [Test]
        public void ShouldSetSpatialRepresentationWhenSpatialRepresentationExists()
        {
            ((MD_DataIdentification_Type)_md.GetMetadata().identificationInfo[0].AbstractMD_Identification).spatialRepresentationType = new MD_SpatialRepresentationTypeCode_PropertyType[] {
                new MD_SpatialRepresentationTypeCode_PropertyType {
                    MD_SpatialRepresentationTypeCode = new CodeListValue_Type {
                        codeListValue = "annet"
                    }
                }
            };


            _md.SpatialRepresentation = "vector";

            MD_Identification_PropertyType identification = (MD_Identification_PropertyType)_md.GetMetadata().identificationInfo[0];
            MD_DataIdentification_Type dataIdentification = (MD_DataIdentification_Type)identification.AbstractMD_Identification;

            Assert.AreEqual(2, dataIdentification.spatialRepresentationType.Length);
            Assert.AreEqual("annet", dataIdentification.spatialRepresentationType[0].MD_SpatialRepresentationTypeCode.codeListValue);
            Assert.AreEqual("vector", dataIdentification.spatialRepresentationType[1].MD_SpatialRepresentationTypeCode.codeListValue);
        }

        [Test]
        public void ShouldReturnDistributionFormatAsNullWhenNull()
        {
            Assert.IsNull(_md.DistributionFormat);
        }


        [Test]
        public void ShouldReturnDistributionFormatWhenPresent()
        {
            string expectedName = "navn";
            string expectedVersion = "versjon";

            _md.GetMetadata().distributionInfo = new MD_Distribution_PropertyType
            {
                MD_Distribution = new MD_Distribution_Type
                {
                    distributionFormat = new MD_Format_PropertyType[] { 
                        new MD_Format_PropertyType {
                            MD_Format = new MD_Format_Type {
                                name = new CharacterString_PropertyType { CharacterString = expectedName },
                                version = new CharacterString_PropertyType { CharacterString = expectedVersion }
                            }
                        }
                    }
                }
            };

            var format = _md.DistributionFormat;
            Assert.NotNull(format);
            Assert.AreEqual(expectedName, format.Name);
            Assert.AreEqual(expectedVersion, format.Version);
        }

        [Test]
        public void ShouldHandleNullValueInDistributionFormatVersion()
        {
            string expectedName = "navn";

            _md.GetMetadata().distributionInfo = new MD_Distribution_PropertyType
            {
                MD_Distribution = new MD_Distribution_Type
                {
                    distributionFormat = new MD_Format_PropertyType[] { 
                        new MD_Format_PropertyType {
                            MD_Format = new MD_Format_Type {
                                name = new CharacterString_PropertyType { CharacterString = expectedName },
                                version = null
                            }
                        }
                    }
                }
            };

            var format = _md.DistributionFormat;
            Assert.NotNull(format);
            Assert.AreEqual(expectedName, format.Name);
            Assert.IsNull(format.Version);
        }

        [Test]
        public void ShouldReturnNullWhenReferenceSystemIsNull()
        {
            Assert.IsNull(_md.ReferenceSystem);
        }

        [Test]
        public void ShouldReturnReferenceSystemWhenPresent()
        {
            string expectedCode = "code";
            string expectedCodeSpace = "codespace";
            _md.GetMetadata().referenceSystemInfo = new MD_ReferenceSystem_PropertyType[] {
                new MD_ReferenceSystem_PropertyType {
                    MD_ReferenceSystem = new MD_ReferenceSystem_Type {
                        referenceSystemIdentifier = new RS_Identifier_PropertyType {
                            RS_Identifier = new RS_Identifier_Type {
                                code = new CharacterString_PropertyType { CharacterString = expectedCode },
                                codeSpace = new CharacterString_PropertyType { CharacterString = expectedCodeSpace }
                            }
                        }
                    }
                }
                
            };

            var rs = _md.ReferenceSystem;
            Assert.NotNull(rs);
            Assert.AreEqual(expectedCode, rs.CoordinateSystem);
            Assert.AreEqual(expectedCodeSpace, rs.Namespace);
        }

        [Test]
        public void ShouldUpdateReferenceSystem()
        {
            string expectedCoordinateSystem = "system";
            string expectedNamespace = "namespace";
            _md.ReferenceSystem = new SimpleReferenceSystem
            {
                CoordinateSystem = expectedCoordinateSystem,
                Namespace = expectedNamespace
            };

            var identifier = _md.GetMetadata().referenceSystemInfo[0].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier;

            Assert.AreEqual(expectedCoordinateSystem, identifier.code.CharacterString);
            Assert.AreEqual(expectedNamespace, identifier.codeSpace.CharacterString);
        }

        [Test]
        public void ShouldReturnNullWhenDistributionDetailsIsNull()
        {
            _md.GetMetadata().distributionInfo = null;

            Assert.IsNull(_md.DistributionDetails);
        }

        [Test]
        public void ShouldReturnDistributionDetails()
        {
            string expectedURL = "http://example.com";
            string expectedProtocol = "www";
            _md.GetMetadata().distributionInfo = new MD_Distribution_PropertyType {
                MD_Distribution = new MD_Distribution_Type
                {
                    transferOptions = new MD_DigitalTransferOptions_PropertyType[]
                    {
                        new MD_DigitalTransferOptions_PropertyType {
                            MD_DigitalTransferOptions = new MD_DigitalTransferOptions_Type {
                                onLine = new CI_OnlineResource_PropertyType[] 
                                {
                                    new CI_OnlineResource_PropertyType 
                                    {
                                        CI_OnlineResource = new CI_OnlineResource_Type 
                                        { 
                                            linkage = new URL_PropertyType { URL = expectedURL },
                                            protocol = new CharacterString_PropertyType { CharacterString = expectedProtocol }
                                        }
                                    }
                                }
                            }    
                        }
                    }
                }
            };

            var details = _md.DistributionDetails;

            Assert.NotNull(details);
            Assert.AreEqual(expectedURL, details.URL);
            Assert.AreEqual(expectedProtocol, details.Protocol);

        }

        [Test]
        public void ShouldUpdateDistributionDetails()
        {
            string expectedURL = "http://example.com";
            string expectedProtocol = "www";
            
            _md.DistributionDetails = new SimpleDistributionDetails { Protocol = expectedProtocol, URL = expectedURL };

            var resource = _md.GetMetadata().distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource;
            Assert.AreEqual(expectedURL, resource.linkage.URL);
            Assert.AreEqual(expectedProtocol, resource.protocol.CharacterString);
        }

        [Test]
        public void ShouldReturnNullWhenResolutionScaleDoesNotExist()
        {
            ((MD_DataIdentification_Type)_md.GetMetadata().identificationInfo[0].AbstractMD_Identification).spatialResolution = null;

            Assert.IsNull(_md.ResolutionScale);
        }

        [Test]
        public void ShouldReturnNullWhenSpatialResolutionContainsOtherItem()
        {
            ((MD_DataIdentification_Type)_md.GetMetadata().identificationInfo[0].AbstractMD_Identification).spatialResolution = new MD_Resolution_PropertyType[] {
                new MD_Resolution_PropertyType {
                    MD_Resolution = new MD_Resolution_Type {
                        Item = new Distance_PropertyType()
                    }
                }
            };
            Assert.IsNull(_md.ResolutionScale);
        }

        [Test]
        public void ShouldReturnResolutionScale()
        {
            string scale = "1000";

            ((MD_DataIdentification_Type)_md.GetMetadata().identificationInfo[0].AbstractMD_Identification).spatialResolution = new MD_Resolution_PropertyType[] {
                new MD_Resolution_PropertyType {
                    MD_Resolution = new MD_Resolution_Type {
                        Item = new MD_RepresentativeFraction_PropertyType { 
                            MD_RepresentativeFraction = new MD_RepresentativeFraction_Type {
                                denominator = new Integer_PropertyType { Integer = scale }
                            }
                        }
                    }
                }
            };

            Assert.IsNotNull(_md.ResolutionScale);
        }

        [Test]
        public void ShouldSetResolutionScale()
        {
            _md.ResolutionScale = "5000";

            var item = ((MD_DataIdentification_Type)_md.GetMetadata().identificationInfo[0].AbstractMD_Identification).spatialResolution[0].MD_Resolution.Item as MD_RepresentativeFraction_PropertyType;

            Assert.AreEqual("5000", item.MD_RepresentativeFraction.denominator.Integer);
        }

        [Test]
        public void ShouldReturnNullWhenNoMaintenanceFrequencyExists()
        {
            string frequency = _md.MaintenanceFrequency;

            Assert.IsNull(frequency);
        }

        
        [Test]
        public void ShouldReturnMaintenanceFrequency()
        {
            string expectedFrequency = "daily";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.resourceMaintenance = new MD_MaintenanceInformation_PropertyType[]
            {
                new MD_MaintenanceInformation_PropertyType {
                    MD_MaintenanceInformation = new MD_MaintenanceInformation_Type {
                        maintenanceAndUpdateFrequency = new MD_MaintenanceFrequencyCode_PropertyType {
                            MD_MaintenanceFrequencyCode = new CodeListValue_Type {
                                codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ProgressCode", 
                                codeListValue = expectedFrequency
                            }
                        }
                    }
                }
            };

            string frequency = _md.MaintenanceFrequency;

            Assert.AreEqual(expectedFrequency, frequency);
        }

        [Test]
        public void ShouldUpdateMaintenanceFrequency()
        {
            string expectedFrequency = "weekly";

            _md.MaintenanceFrequency = expectedFrequency;

            string value = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.resourceMaintenance[0].MD_MaintenanceInformation.maintenanceAndUpdateFrequency.MD_MaintenanceFrequencyCode.codeListValue;

            Assert.AreEqual(expectedFrequency, value);
        }



        [Test]
        public void ShouldReturnNullWhenStatusIsNull()
        {
            Assert.IsNull(_md.Status);
        }

        [Test]
        public void ShouldReturnStatus()
        {
            string expectedStatus = "completed";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.status = new MD_ProgressCode_PropertyType[] { new MD_ProgressCode_PropertyType { 
                MD_ProgressCode =  new CodeListValue_Type { codeListValue = expectedStatus }
            }};

            Assert.AreEqual(expectedStatus, _md.Status);
        }

        [Test]
        public void ShouldUpdateStatus()
        {
            string expectedStatus = "completed";
            _md.Status = expectedStatus;

            string value = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.status[0].MD_ProgressCode.codeListValue;

            Assert.AreEqual(expectedStatus, value);
        }

        [Test]
        public void ShouldReturnNullWhenQualitySpecificationDataIsNull()
        {
            _md.GetMetadata().dataQualityInfo = null;
            Assert.IsNull(_md.QualitySpecification);
        }

        [Test]
        public void ShouldReturnQualitySpecificationData()
        {
            string expectedTitle = "title";
            string expectedDate = "2014-01-01";
            string expectedDateType = "creation";
            string expectedExplanation = "explained";
            bool expectedResult = true;

            _md.GetMetadata().dataQualityInfo = new DQ_DataQuality_PropertyType[] 
            {
                new DQ_DataQuality_PropertyType {
                    DQ_DataQuality = new DQ_DataQuality_Type {
                        report = new DQ_Element_PropertyType[] {
                            new DQ_Element_PropertyType {
                                AbstractDQ_Element = new DQ_DomainConsistency_Type {
                                    result = new DQ_Result_PropertyType[] {
                                        new DQ_Result_PropertyType {
                                            AbstractDQ_Result = new DQ_ConformanceResult_Type {
                                                specification = new CI_Citation_PropertyType {
                                                    CI_Citation = new CI_Citation_Type {
                                                        title = toCharString(expectedTitle),
                                                        date = new CI_Date_PropertyType[] {
                                                            new CI_Date_PropertyType {
                                                                CI_Date = new CI_Date_Type {
                                                                    date = new Date_PropertyType {
                                                                        Item = expectedDate  
                                                                    },
                                                                    dateType = new CI_DateTypeCode_PropertyType {
                                                                        CI_DateTypeCode = new CodeListValue_Type {
                                                                            codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#CI_DateTypeCode",
                                                                            codeListValue = expectedDateType
                                                                        }    
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }    
                                                },
                                                explanation = toCharString(expectedExplanation),
                                                pass = new Boolean_PropertyType { Boolean = expectedResult }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };


            SimpleQualitySpecification spec = _md.QualitySpecification;

            Assert.IsNotNull(spec);
            Assert.AreEqual(expectedTitle, spec.Title);
            Assert.AreEqual(expectedDate, spec.Date);
            Assert.AreEqual(expectedDateType, spec.DateType);
            Assert.AreEqual(expectedExplanation, spec.Explanation);
            Assert.AreEqual(expectedResult, spec.Result);
        }

        [Test]
        public void ShouldUpdateQualitySpecification()
        {
            string expectedTitle = "title";
            string expectedDate = "2014-01-01";
            string expectedDateType = "creation";
            string expectedExplanation = "explained";
            bool expectedResult = true;

            _md.QualitySpecification = new SimpleQualitySpecification
            {
                Title = expectedTitle,
                Date = expectedDate,
                DateType = expectedDateType,
                Explanation = expectedExplanation,
                Result = expectedResult
            };

            DQ_DomainConsistency_Type domainConsistency = _md.GetMetadata().dataQualityInfo[0].DQ_DataQuality.report[0].AbstractDQ_Element as DQ_DomainConsistency_Type;
            DQ_ConformanceResult_Type conformanceResult = domainConsistency.result[0].AbstractDQ_Result as DQ_ConformanceResult_Type;

            Assert.AreEqual(expectedTitle, conformanceResult.specification.CI_Citation.title.CharacterString);
            Assert.AreEqual(expectedDate, (string)conformanceResult.specification.CI_Citation.date[0].CI_Date.date.Item);
            Assert.AreEqual(expectedDateType, conformanceResult.specification.CI_Citation.date[0].CI_Date.dateType.CI_DateTypeCode.codeListValue);
            Assert.AreEqual(expectedExplanation, conformanceResult.explanation.CharacterString);
            Assert.AreEqual(expectedResult, conformanceResult.pass.Boolean);
        }
        
        [Test]
        public void ShouldReturnNullWhenProcessHistoryIsNull()
        {
            _md.GetMetadata().dataQualityInfo = null;
            Assert.IsNull(_md.ProcessHistory);
        }

        [Test]
        public void ShouldReturnProcessHistory()
        {
            string expectedProcessHistory = "history";

            _md.GetMetadata().dataQualityInfo = new DQ_DataQuality_PropertyType[] {
                new DQ_DataQuality_PropertyType {
                    DQ_DataQuality = new DQ_DataQuality_Type {
                        lineage = new LI_Lineage_PropertyType {
                            LI_Lineage = new LI_Lineage_Type {
                                statement = toCharString(expectedProcessHistory)
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(expectedProcessHistory, _md.ProcessHistory);
        }

        [Test]
        public void ShouldUpdateProcessHistory()
        {
            string expectedProcessHistory = "updated history";
            _md.ProcessHistory = expectedProcessHistory;

            Assert.AreEqual(expectedProcessHistory, _md.GetMetadata().dataQualityInfo[0].DQ_DataQuality.lineage.LI_Lineage.statement.CharacterString);
        }

        [Test]
        public void ShouldReturnNullWhenCreationDateIsNull()
        {
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.date = null;
            Assert.IsNull(_md.DateCreated);
        }

        [Test]
        public void ShouldReturnCreationDateWhenTypeIsDateTime()
        {
            DateTime expectedDate = DateTime.Parse("2014-01-01");

            SetDateOnCitationDateType(expectedDate, "creation");

            Assert.AreEqual(expectedDate, _md.DateCreated);
        }
        [Test]
        public void ShouldReturnCreationDateWhenTypeIsString()
        {
            string expectedDateString = "2014-01-01";
            DateTime expectedDate = DateTime.Parse(expectedDateString);

            SetDateOnCitationDateType(expectedDateString, "creation");

            Assert.AreEqual(expectedDate, _md.DateCreated);
        }

        [Test]
        public void ShouldUpdateCreatedDate()
        {
            string expectedDateString = "2014-03-04";
            DateTime expectedDate = DateTime.Parse(expectedDateString);

            _md.DateCreated = expectedDate;

            Assert.AreEqual(expectedDate, GetCitationDateWithType("creation"));
        }

        [Test]
        public void ShouldReturnNullWhenPublishedDateIsNull()
        {
            Assert.IsNull(_md.DatePublished);
        }

        [Test]
        public void ShouldReturnPublishedDateWhenTypeIsDateTime()
        {
            DateTime expectedDate = DateTime.Parse("2014-04-05");

            SetDateOnCitationDateType(expectedDate, "publication");

            Assert.AreEqual(expectedDate, _md.DatePublished);
        }
        [Test]
        public void ShouldReturnPublishedDateWhenTypeIsString()
        {
            string expectedDateString = "2014-03-04";
            DateTime expectedDate = DateTime.Parse(expectedDateString);

            SetDateOnCitationDateType(expectedDateString, "publication");

            Assert.AreEqual(expectedDate, _md.DatePublished);
        }

        [Test]
        public void ShouldUpdatePublishedDate()
        {
            string expectedDateString = "2014-03-04";
            DateTime expectedDate = DateTime.Parse(expectedDateString);

            _md.DatePublished = expectedDate;

            Assert.AreEqual(expectedDate, GetCitationDateWithType("publication"));
        }

        [Test]
        public void ShouldReturnNullWhenUpdatedDateIsNull()
        {
            Assert.IsNull(_md.DateUpdated);
        }

        [Test]
        public void ShouldReturnUpdatedDateWhenTypeIsDateTime()
        {
            DateTime expectedDate = DateTime.Parse("2014-04-05");

            SetDateOnCitationDateType(expectedDate, "revision");

            Assert.AreEqual(expectedDate, _md.DateUpdated);
        }

        [Test]
        public void ShouldReturnUpdatedDateWhenTypeIsString()
        {
            string expectedDateString = "2014-03-04";
            DateTime expectedDate = DateTime.Parse(expectedDateString);

            SetDateOnCitationDateType(expectedDateString, "revision");

            Assert.AreEqual(expectedDate, _md.DateUpdated);
        }

        [Test]
        public void ShouldUpdateUpdatedDate()
        {
            string expectedDateString = "2014-03-04";
            DateTime expectedDate = DateTime.Parse(expectedDateString);

            _md.DateUpdated = expectedDate;

            Assert.AreEqual(expectedDate, GetCitationDateWithType("revision"));
        }

        [Test]
        public void ShouldReturnMetadataUpdatedDate()
        {
            Assert.IsNotNull(_md.DateMetadataUpdated);
        }

        [Test]
        public void ShouldUpdateMetadataUpdatedDate()
        {
            DateTime expectedDate = DateTime.Parse("2014-01-15");
            _md.DateMetadataUpdated = expectedDate;

            Assert.AreEqual(expectedDate, (DateTime)_md.GetMetadata().dateStamp.Item);
        }

        [Test]
        public void ShouldReturnMetadataLanguage()
        {
            Assert.AreEqual("nor", _md.MetadataLanguage);
        }

        [Test]
        public void ShouldUpdateMetadataLanguage()
        {
            _md.MetadataLanguage = "eng";

            Assert.AreEqual("eng", _md.GetMetadata().language.CharacterString);
        }

        [Test]
        public void ShouldReturnMetadataStandardName()
        {
            Assert.AreEqual("ISO19139", _md.MetadataStandard);
        }

        [Test]
        public void ShouldUpdateMetadataStandard()
        {
            _md.MetadataStandard = "iso19115";

            Assert.AreEqual("iso19115", _md.GetMetadata().metadataStandardName.CharacterString);
        }

        [Test]
        public void ShouldReturnMetadataStandardVersion()
        {
            Assert.AreEqual("1.0", _md.MetadataStandardVersion);
        }

        [Test]
        public void ShouldUpdateMetadataStandardVersion()
        {
            _md.MetadataStandardVersion = "2.0";

            Assert.AreEqual("2.0", _md.GetMetadata().metadataStandardVersion.CharacterString);
        }

        private void SetDateOnCitationDateType(object date, string dateType)
        {
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.date = new CI_Date_PropertyType[] {
                new CI_Date_PropertyType {
                    CI_Date = new CI_Date_Type {
                        date = new Date_PropertyType { Item = date },
                        dateType = new CI_DateTypeCode_PropertyType { CI_DateTypeCode = new CodeListValue_Type { codeListValue = dateType }}
                    }
                }
            };
        }

        private object GetCitationDateWithType(string dateType)
        {
            var dates = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.date;
            foreach (var date in dates)
            {
                if (date.CI_Date.dateType.CI_DateTypeCode.codeListValue == dateType)
                {
                    return date.CI_Date.date.Item;
                }
            }
            return null;
        }

        private CharacterString_PropertyType toCharString(string input)
        {
            return new CharacterString_PropertyType { CharacterString = input };
        }
    }
}
