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

        [Test]
        public void ShouldReturnNullForEnglishTitleWhenTitleIsRegularCharacterStringObject()
        {
            string title = _md.EnglishTitle;
            Assert.IsNull(title);
        }

        [Test]
        public void ShouldReturnEnglishTitleWhenTitleIsLocalized()
        {
            string expectedEnglishTitle = "This is english.";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.title = new PT_FreeText_PropertyType
            {
                CharacterString = "Dette er norsk",
                PT_FreeText = new PT_FreeText_Type
                {
                    textGroup = new LocalisedCharacterString_PropertyType[] { 
                        new LocalisedCharacterString_PropertyType {
                            LocalisedCharacterString = new LocalisedCharacterString_Type {
                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                Value = expectedEnglishTitle
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(expectedEnglishTitle, _md.EnglishTitle);            
        }

        [Test]
        public void ShouldUpdateEnglishTitleWithoutDestroyingExistingLocalTitle()
        {
            string expectedEnglishTitle = "This is english.";

            _md.EnglishTitle = expectedEnglishTitle;

            CharacterString_PropertyType titleElement = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.title;
            PT_FreeText_PropertyType freeTextElement = titleElement as PT_FreeText_PropertyType;
            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual("Eksempeldatasettet sin tittel.", freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishTitle, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
        }

        [Test]
        public void ShouldUpdateTitleWithoutDestroyingEnglishTitle()
        {
            string expectedEnglishTitle = "This is english.";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.title = new PT_FreeText_PropertyType
            {
                CharacterString = "Dette er norsk",
                PT_FreeText = new PT_FreeText_Type
                {
                    textGroup = new LocalisedCharacterString_PropertyType[] { 
                        new LocalisedCharacterString_PropertyType {
                            LocalisedCharacterString = new LocalisedCharacterString_Type {
                                locale = SimpleMetadata.LOCALE_ENG,
                                Value = expectedEnglishTitle
                            }
                        }
                    }
                }
            };

            _md.Title = "Oppdatert norsk tittel";

            CharacterString_PropertyType titleElement = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.title;
            PT_FreeText_PropertyType freeTextElement = titleElement as PT_FreeText_PropertyType;
            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual("Oppdatert norsk tittel", freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishTitle, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
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
        public void ShouldReturnNullForEnglishAbstractWhenAbstractIsRegularCharacterStringObject()
        {
            string @abstract = _md.EnglishAbstract;
            Assert.IsNull(@abstract);
        }

        [Test]
        public void ShouldReturnEnglishAbstractWhenAbstractIsLocalized()
        {
            string expectedEnglishAbstract = "This is english.";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.@abstract = new PT_FreeText_PropertyType
            {
                CharacterString = "Dette er norsk",
                PT_FreeText = new PT_FreeText_Type
                {
                    textGroup = new LocalisedCharacterString_PropertyType[] { 
                        new LocalisedCharacterString_PropertyType {
                            LocalisedCharacterString = new LocalisedCharacterString_Type {
                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                Value = expectedEnglishAbstract
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(expectedEnglishAbstract, _md.EnglishAbstract);
        }

        [Test]
        public void ShouldUpdateEnglishAbstractWithoutDestroyingExistingLocalAbstract()
        {
            string expectedNorwegianAbstract = "Dette er sammendraget.";
            string expectedEnglishAbstract = "This is english.";

            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.@abstract = new CharacterString_PropertyType { CharacterString = expectedNorwegianAbstract };

            _md.EnglishAbstract = expectedEnglishAbstract;

            CharacterString_PropertyType abstractElement = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.@abstract;
            PT_FreeText_PropertyType freeTextElement = abstractElement as PT_FreeText_PropertyType;

            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual(expectedNorwegianAbstract, freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishAbstract, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
        }

        [Test]
        public void ShouldUpdateAbstractWithoutDestroyingEnglishAbstract()
        {
            string expectedNorwegianAbstract = "Oppdatert norsk sammendrag";
            string expectedEnglishAbstract = "This is english.";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.@abstract = new PT_FreeText_PropertyType
            {
                CharacterString = "Dette er norsk",
                PT_FreeText = new PT_FreeText_Type
                {
                    textGroup = new LocalisedCharacterString_PropertyType[] { 
                        new LocalisedCharacterString_PropertyType {
                            LocalisedCharacterString = new LocalisedCharacterString_Type {
                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                Value = expectedEnglishAbstract
                            }
                        }
                    }
                }
            };

            _md.Abstract = expectedNorwegianAbstract;

            CharacterString_PropertyType abstractElement = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.@abstract;
            PT_FreeText_PropertyType freeTextElement = abstractElement as PT_FreeText_PropertyType;

            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual(expectedNorwegianAbstract, freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishAbstract, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
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
        public void ShouldUpdateKeywords()
        {
            _md.Keywords = new List<SimpleKeyword> {
                new SimpleKeyword {
                    Keyword = "Addresses",
                    Thesaurus = SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1
                },
                new SimpleKeyword {
                    Keyword = "Oslo",
                    Type = SimpleKeyword.TYPE_PLACE
                },
                new SimpleKeyword {
                    Keyword = "Akershus",
                    Type = SimpleKeyword.TYPE_PLACE
                },
                new SimpleKeyword {
                    Keyword = "Bygninger",
                    Type = SimpleKeyword.TYPE_THEME
                },
                new SimpleKeyword {
                    Keyword = "Buildings",
                    Thesaurus = SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1
                },
                new SimpleKeyword {
                    Keyword = "Eksempeldata",
                },
                new SimpleKeyword {
                    Keyword = "testing"
                },
                new SimpleKeyword {
                    Keyword = "Det offentlige kartgrunnlaget",
                    Thesaurus = SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE
                },
            };

            int numberOfInspireKeywords = 0;
            int numberOfNationalKeywords = 0;
            int numberOfThemeKeywords = 0;
            int numberOfPlaceKeywords = 0;
            int numberOfOtherKeywords = 0;
            bool inspireAddressesFound = false;
            bool inspireBuildingsFound = false;
            bool nationalDOKfound = false;
            bool placeOsloFound = false;
            bool placeAkershusFound = false;
            bool themeBygningFound = false;
            bool otherEksempeldataFound = false;
            bool otherTestingFound = false;

            MD_Keywords_PropertyType[] descriptiveKeywords = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.descriptiveKeywords;

            foreach (MD_Keywords_PropertyType descriptiveKeyword in descriptiveKeywords)
            {
                int numberOfKeywords = descriptiveKeyword.MD_Keywords.keyword.Length;

                if (descriptiveKeyword.MD_Keywords.thesaurusName != null)
                {
                    if (descriptiveKeyword.MD_Keywords.thesaurusName.CI_Citation.title.CharacterString.Equals(SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1)) {
                        numberOfInspireKeywords = numberOfKeywords;
                        foreach (var k in descriptiveKeyword.MD_Keywords.keyword)
                        {
                            if (k.CharacterString.Equals("Addresses"))
                            {
                                inspireAddressesFound = true;
                            }
                            else if (k.CharacterString.Equals("Buildings"))
                            {
                                inspireBuildingsFound = true;
                            }
                        }
                    }
                    else if (descriptiveKeyword.MD_Keywords.thesaurusName.CI_Citation.title.CharacterString.Equals(SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE))
                    {
                        numberOfNationalKeywords = numberOfKeywords;
                        if (descriptiveKeyword.MD_Keywords.keyword[0].CharacterString.Equals("Det offentlige kartgrunnlaget"))
                        {
                            nationalDOKfound = true;
                        }
                    }
                }
                else if (descriptiveKeyword.MD_Keywords.type != null && descriptiveKeyword.MD_Keywords.type.MD_KeywordTypeCode.codeListValue.Equals(SimpleKeyword.TYPE_PLACE)) 
                {
                    numberOfPlaceKeywords = numberOfKeywords;
                    foreach (var k in descriptiveKeyword.MD_Keywords.keyword)
                    {
                        if (k.CharacterString.Equals("Oslo"))
                        {
                            placeOsloFound = true;
                        }
                        else if (k.CharacterString.Equals("Akershus"))
                        {
                            placeAkershusFound = true;
                        }
                    }
                }
                else if (descriptiveKeyword.MD_Keywords.type != null && descriptiveKeyword.MD_Keywords.type.MD_KeywordTypeCode.codeListValue.Equals(SimpleKeyword.TYPE_THEME))
                {
                    numberOfThemeKeywords = numberOfKeywords;
                    if (descriptiveKeyword.MD_Keywords.keyword[0].CharacterString.Equals("Bygninger"))
                    {
                        themeBygningFound = true;
                    }
                }
                else
                {
                    numberOfOtherKeywords = numberOfKeywords;
                    foreach (var k in descriptiveKeyword.MD_Keywords.keyword)
                    {
                        if (k.CharacterString.Equals("Eksempeldata"))
                        {
                            otherEksempeldataFound = true;
                        }
                        else if (k.CharacterString.Equals("testing"))
                        {
                            otherTestingFound = true;
                        }
                    }
                }
            }

            Assert.AreEqual(2, numberOfInspireKeywords, "Expected two inspire keywords in same wrapper element");
            Assert.AreEqual(1, numberOfNationalKeywords, "Expected one national keyword in same wrapper element");
            Assert.AreEqual(1, numberOfThemeKeywords, "Expected one theme keyword in same wrapper element");
            Assert.AreEqual(2, numberOfPlaceKeywords, "Expected two place keywords in same wrapper element");
            Assert.AreEqual(2, numberOfOtherKeywords, "Expected two other keywords in same wrapper element");

            Assert.True(inspireAddressesFound);
            Assert.True(inspireBuildingsFound);
            Assert.True(nationalDOKfound);
            Assert.True(placeOsloFound);
            Assert.True(placeAkershusFound);
            Assert.True(themeBygningFound);
            Assert.True(otherEksempeldataFound);
            Assert.True(otherTestingFound);
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
            string expectedName = "navn";
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
                                            protocol = new CharacterString_PropertyType { CharacterString = expectedProtocol },
                                            name = new CharacterString_PropertyType { CharacterString = expectedName }
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
            Assert.AreEqual(expectedName, details.Name);

        }

        [Test]
        public void ShouldUpdateDistributionDetails()
        {
            string expectedURL = "http://example.com";
            string expectedProtocol = "www";
            string expectedName = "navn";
            _md.DistributionDetails = new SimpleDistributionDetails { Protocol = expectedProtocol, URL = expectedURL, Name = expectedName };

            var resource = _md.GetMetadata().distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource;
            Assert.AreEqual(expectedURL, resource.linkage.URL);
            Assert.AreEqual(expectedProtocol, resource.protocol.CharacterString);
            Assert.AreEqual(expectedName, resource.name.CharacterString);
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
            DateTime expectedDate = DateTime.Parse("2014-01-01T12:00:00");

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

            Assert.AreEqual(expectedDateString, (string)GetCitationDateWithType("creation"));
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

            Assert.AreEqual(expectedDateString, (string) GetCitationDateWithType("publication"));
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

            Assert.AreEqual(expectedDateString, (string)GetCitationDateWithType("revision"));
        }

        [Test]
        public void ShouldReturnMetadataUpdatedDate()
        {
            Assert.IsNotNull(_md.DateMetadataUpdated);
        }

        [Test]
        public void ShouldUpdateMetadataUpdatedDate()
        {
            string expectedDateString = "2014-01-15";
            DateTime expectedDate = DateTime.Parse(expectedDateString);
            _md.DateMetadataUpdated = expectedDate;

            Assert.AreEqual(expectedDateString, (string)_md.GetMetadata().dateStamp.Item);
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

        [Test]
        public void ShouldReturnBoundingBoxForDataset()
        {
            SimpleBoundingBox boundingBox = _md.BoundingBox;

            Assert.IsNotNull(boundingBox);

            Assert.AreEqual("33", boundingBox.EastBoundLongitude);
            Assert.AreEqual("2", boundingBox.WestBoundLongitude);
            Assert.AreEqual("72", boundingBox.NorthBoundLatitude);
            Assert.AreEqual("57", boundingBox.SouthBoundLatitude);
        }

        [Test]
        public void ShouldReturnBoundingBoxForService()
        {
            _md.GetMetadata().identificationInfo = new MD_Identification_PropertyType[]
                {
                    new MD_Identification_PropertyType 
                    {
                        AbstractMD_Identification = new SV_ServiceIdentification_Type
                        {
                            extent = new[]
                            {
                                new EX_Extent_PropertyType
                                    {
                                        EX_Extent = new EX_Extent_Type
                                            {
                                                geographicElement = new []
                                                    {
                                                        new EX_GeographicExtent_PropertyType
                                                            {
                                                                AbstractEX_GeographicExtent = new EX_GeographicBoundingBox_Type
                                                                    {
                                                                        eastBoundLongitude = new Decimal_PropertyType { Decimal = 44 },
                                                                        westBoundLongitude = new Decimal_PropertyType { Decimal = 10 },                                                                        
                                                                        northBoundLatitude = new Decimal_PropertyType { Decimal = 77 },
                                                                        southBoundLatitude = new Decimal_PropertyType { Decimal = 55 }                                                                        
                                                                    }
                                                            }
                                                    }
                                            }
                                    }
                            }
                        }
                    }
                };
            _md.GetMetadata().hierarchyLevel = new MD_ScopeCode_PropertyType[] { new MD_ScopeCode_PropertyType { MD_ScopeCode = new CodeListValue_Type { codeListValue = "service" } } };

            SimpleBoundingBox boundingBox = _md.BoundingBox;

            Assert.IsNotNull(boundingBox);

            Assert.AreEqual("44", boundingBox.EastBoundLongitude);
            Assert.AreEqual("10", boundingBox.WestBoundLongitude);
            Assert.AreEqual("77", boundingBox.NorthBoundLatitude);
            Assert.AreEqual("55", boundingBox.SouthBoundLatitude);
        }

        [Test]
        public void ShouldReturnNullWhenHierarchyLevelIsNotDatasetOrService()
        {
            _md.GetMetadata().hierarchyLevel = new MD_ScopeCode_PropertyType[] { new MD_ScopeCode_PropertyType { MD_ScopeCode = new CodeListValue_Type { codeListValue = "application" } } };
            Assert.IsNull(_md.BoundingBox);
        }

        [Test]
        public void ShouldUpdateBoundingBoxForDataset()
        {
            _md.BoundingBox = new SimpleBoundingBox 
            {
                EastBoundLongitude = "12",
                WestBoundLongitude = "22",
                NorthBoundLatitude = "44",
                SouthBoundLatitude = "33"
            };

            var identification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;
            var bbox = identification.extent[0].EX_Extent.geographicElement[0].AbstractEX_GeographicExtent as EX_GeographicBoundingBox_Type;

            Assert.AreEqual("12", bbox.eastBoundLongitude.Decimal.ToString());
            Assert.AreEqual("22", bbox.westBoundLongitude.Decimal.ToString());
            Assert.AreEqual("44", bbox.northBoundLatitude.Decimal.ToString());
            Assert.AreEqual("33", bbox.southBoundLatitude.Decimal.ToString());
        }

        [Test]
        public void ShouldUpdateBoundingBoxForService()
        {
            _md.GetMetadata().hierarchyLevel = new MD_ScopeCode_PropertyType[] { new MD_ScopeCode_PropertyType { MD_ScopeCode = new CodeListValue_Type { codeListValue = "service" } } };
            _md.GetMetadata().identificationInfo = new MD_Identification_PropertyType[]
                {
                    new MD_Identification_PropertyType 
                    {
                        AbstractMD_Identification = new SV_ServiceIdentification_Type()
                    }
                };
                        
            _md.BoundingBox = new SimpleBoundingBox
            {
                EastBoundLongitude = "12",
                WestBoundLongitude = "22",
                NorthBoundLatitude = "44",
                SouthBoundLatitude = "33"
            };

            var identification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as SV_ServiceIdentification_Type;
            var bbox = identification.extent[0].EX_Extent.geographicElement[0].AbstractEX_GeographicExtent as EX_GeographicBoundingBox_Type;

            Assert.AreEqual("12", bbox.eastBoundLongitude.Decimal.ToString());
            Assert.AreEqual("22", bbox.westBoundLongitude.Decimal.ToString());
            Assert.AreEqual("44", bbox.northBoundLatitude.Decimal.ToString());
            Assert.AreEqual("33", bbox.southBoundLatitude.Decimal.ToString());
        }

        [Test]
        public void ShouldReturnConstraints()
        {
            SimpleConstraints constraints = _md.Constraints;

            Assert.AreEqual("Gratis å benytte til alle formål.", constraints.UseLimitations);
            Assert.AreEqual("Ingen begrensninger på bruk.", constraints.OtherConstraints);
            Assert.AreEqual("unclassified", constraints.SecurityConstraints);
            Assert.AreEqual("none", constraints.AccessConstraints);
            Assert.AreEqual("free", constraints.UseConstraints);
        }

        [Test]
        public void ShouldUpdateConstraints()
        {
            string expectedUseLimitations = "ingen begrensninger";
            string expectedOtherConstraints = "ingen andre begrensninger";
            string expectedSecurityConstraints = "classified";
            string expectedAccessConstraints = "restricted";
            string expectedUseConstraints = "license";

            SimpleConstraints constraints = new SimpleConstraints
            {
                UseLimitations = expectedUseLimitations,
                OtherConstraints = expectedOtherConstraints,
                SecurityConstraints = expectedSecurityConstraints,
                AccessConstraints = expectedAccessConstraints,
                UseConstraints = expectedUseConstraints,

            };
            _md.Constraints = constraints;

            string actualUseLimitation = null;
            string actualSecurityConstraint = null;
            string actualAccessConstraint = null;
            string actualOtherConstraint = null;
            string actualUseConstraint = null;
            MD_DataIdentification_Type identification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;

            foreach (var constraint in identification.resourceConstraints)
            {
                MD_SecurityConstraints_Type securityConstraint = constraint.MD_Constraints as MD_SecurityConstraints_Type;
                if (securityConstraint != null)
                {
                    actualSecurityConstraint = securityConstraint.classification.MD_ClassificationCode.codeListValue;
                }
                else
                {
                    MD_LegalConstraints_Type legalConstraint = constraint.MD_Constraints as MD_LegalConstraints_Type;
                    if (legalConstraint != null)
                    {
                        actualAccessConstraint = legalConstraint.accessConstraints[0].MD_RestrictionCode.codeListValue;
                        actualOtherConstraint = legalConstraint.otherConstraints[0].CharacterString;
                        actualUseConstraint = legalConstraint.useConstraints[0].MD_RestrictionCode.codeListValue;
                    }
                    else
                    {
                        MD_Constraints_Type regularConstraint = constraint.MD_Constraints as MD_Constraints_Type;
                        actualUseLimitation = regularConstraint.useLimitation[0].CharacterString;
                    }                    
                }                
            }

            Assert.AreEqual(expectedUseLimitations, actualUseLimitation);
            Assert.AreEqual(expectedSecurityConstraints, actualSecurityConstraint);
            Assert.AreEqual(expectedAccessConstraints, actualAccessConstraint);
            Assert.AreEqual(expectedOtherConstraints, actualOtherConstraint);
            Assert.AreEqual(expectedUseConstraints, actualUseConstraint);
        }

        [Test]
        public void ShouldSetOperatesOn()
        {
            string uuid1 = "dddbb667-1303-4ac5-8640-7ec04c0e3918";
            string uuid2 = "595e47d9-d201-479c-a77d-cbc1f573a76b";
            List<string> uuids = new List<string> { uuid1, uuid2 };
            _md.HierarchyLevel = "service";
            _md.GetMetadata().identificationInfo = new MD_Identification_PropertyType[] { new MD_Identification_PropertyType { AbstractMD_Identification = new SV_ServiceIdentification_Type() } };
            _md.OperatesOn = uuids;

            bool foundUuid1 = false;
            bool foundUuid2 = false;

            var serviceIdentification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as SV_ServiceIdentification_Type;
            var operatesOnList = serviceIdentification.operatesOn;
            foreach (MD_DataIdentification_PropertyType element in operatesOnList)
            {
                if (element.uuidref.Equals(uuid1))
                {
                    foundUuid1 = true;
                }
                else if (element.uuidref.Equals(uuid2))
                {
                    foundUuid2 = true;
                }
            }

            Assert.True(foundUuid1);
            Assert.True(foundUuid2);
        }

        [Test]
        public void ShouldReturnOperatesOn()
        {
            string uuid1 = "dddbb667-1303-4ac5-8640-7ec04c0e3918";
            string uuid2 = "595e47d9-d201-479c-a77d-cbc1f573a76b";
            _md.HierarchyLevel = "service";
            _md.GetMetadata().identificationInfo = new MD_Identification_PropertyType[] 
            { 
                new MD_Identification_PropertyType 
                { 
                    AbstractMD_Identification = new SV_ServiceIdentification_Type 
                    {
                        operatesOn = new MD_DataIdentification_PropertyType [] 
                        {
                            new MD_DataIdentification_PropertyType {
                                uuidref = uuid1
                            },
                            new MD_DataIdentification_PropertyType {
                                uuidref = uuid2
                            }
                        }
                    }
                } 
            };

            List<string> operatesOn = _md.OperatesOn;
            Assert.True(operatesOn.Contains(uuid1));
            Assert.True(operatesOn.Contains(uuid2));
        }

        [Test]
        public void CreateService()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            Assert.NotNull(metadata);

            Console.WriteLine(Arkitektum.GIS.Lib.SerializeUtil.SerializeUtil.SerializeToString(metadata.GetMetadata()));
        }

        [Test]
        public void ShouldAddKeywordToMetadataWithNoExistingKeywords()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            metadata.Keywords = new List<SimpleKeyword> { new SimpleKeyword { Keyword = "testkeyword", Type = "theme" } };

            Assert.AreEqual(1, metadata.Keywords.Count);
        }

        [Test]
        public void ShouldAddThumbnailToMetadataWithNoExistingThumbnails()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            metadata.Thumbnails = new List<SimpleThumbnail> { new SimpleThumbnail { Type = "thumbnail", URL = "http://www.geonorge.no/image.png" } };

            Assert.AreEqual(1, metadata.Thumbnails.Count);
        }
        
        [Test]
        public void ShouldIgnoreDuplicateKeywords()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            metadata.Keywords = new List<SimpleKeyword> { new SimpleKeyword { Keyword = "hello" } };

            List<SimpleKeyword> existingKeywords = metadata.Keywords;

            existingKeywords.Add(new SimpleKeyword { Keyword = "hello" });
            metadata.Keywords = existingKeywords;

        }

        [Test]
        public void ShouldParseDecimalStringWithCommaAsSeparator()
        {
            string input = "23,45";

            SimpleMetadata metadata = SimpleMetadata.CreateService();
            Decimal_PropertyType output = metadata.DecimalFromString(input);

            Assert.AreEqual(23.45m, output.Decimal);
        }

        [Test]
        public void ShouldParseDecimalStringWithDotAsSeparator()
        {
            string input = "23.45";

            SimpleMetadata metadata = SimpleMetadata.CreateService();
            Decimal_PropertyType output = metadata.DecimalFromString(input);

            Assert.AreEqual(23.45m, output.Decimal);
        }

        [Test]
        public void ShouldParseDecimalStringWithExponentialComponent()
        {
            string input = "1.41125505E-8";

            SimpleMetadata metadata = SimpleMetadata.CreateService();
            Decimal_PropertyType output = metadata.DecimalFromString(input);

            Assert.AreEqual(1.41125505E-8m, output.Decimal);
        }

        [Test]
        public void ShouldAddTopicCategoryToMetadataWhenNewlyCreated()
        {
            string expectedTopicCategory = "elevation";
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            metadata.TopicCategory = expectedTopicCategory;

            Assert.AreEqual(expectedTopicCategory, metadata.TopicCategory);
        }

        [Test]
        public void ShouldAddSupplementalDescriptionToMetadataWhenNewlyCreated()
        {
            string expectedDescription = "hello world";
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            metadata.SupplementalDescription = expectedDescription;

            Assert.AreEqual(expectedDescription, metadata.SupplementalDescription);
        }

        [Test]
        public void ShouldUpdateSpecificUsage()
        {
            string expected = "THis is the specific usage details.";
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            metadata.SpecificUsage = expected;

            string actual = metadata.GetMetadata().identificationInfo[0].AbstractMD_Identification.resourceSpecificUsage[0].MD_Usage.specificUsage.CharacterString;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ShouldReturnSpecificUsageWhenNotNull()
        {
            string expected = "hello";
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            metadata.GetMetadata().identificationInfo[0].AbstractMD_Identification.resourceSpecificUsage = new MD_Usage_PropertyType[] {
                new MD_Usage_PropertyType {
                    MD_Usage = new MD_Usage_Type {
                        specificUsage = toCharString(expected)
                    }
                }
            };

            Assert.AreEqual(expected, metadata.SpecificUsage);

        }

        [Test]
        public void ShouldReturnNullWhenSpecificUsageIsNotDefined()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            Assert.IsNull(metadata.SpecificUsage);
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
