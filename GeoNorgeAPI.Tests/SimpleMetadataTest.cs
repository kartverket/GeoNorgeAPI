using NUnit.Framework;
using System;
using www.opengis.net;
using System.Collections.Generic;
using System.Diagnostics;
using Arkitektum.GIS.Lib.SerializeUtil;
using System.IO;

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
        public void TestSample()
        {
            Trace.WriteLine(SerializeUtil.SerializeToString(MetadataExample.CreateMetadataExample()));
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
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.title.item = new PT_FreeText_PropertyType
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

            CharacterString_PropertyType titleElement = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.title.item as CharacterString_PropertyType;
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
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.title = new CI_Citation_Title
            {
                item = new PT_FreeText_PropertyType
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
                }
            };

            _md.Title = "Oppdatert norsk tittel";

            var titleElement = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.title.item;
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
        public void ShouldReturnHierarchyLevelName()
        {
            Assert.AreEqual("historical", _md.HierarchyLevelName);
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
        public void ShouldUpdateHierarchyLevelNameWhenNoHierarchyLevelNameExists()
        {
            _md.GetMetadata().hierarchyLevelName = null;

            string newHierarchyLevelName = "historical";
            _md.HierarchyLevelName = newHierarchyLevelName;

            Assert.AreEqual(newHierarchyLevelName, _md.HierarchyLevelName);
        }

        [Test]
        public void ShouldUpdateHierarchyLevelWhenHierarchyLevelExists()
        {
            string newHierarchyLevel = "service";
            _md.HierarchyLevel = newHierarchyLevel;

            Assert.AreEqual(newHierarchyLevel, _md.HierarchyLevel);
        }

        [Test]
        public void ShouldUpdateHierarchyLevelNameWhenHierarchyLevelNameExists()
        {
            string newHierarchyLevelName = "historical";
            _md.HierarchyLevelName = newHierarchyLevelName;

            Assert.AreEqual(newHierarchyLevelName, _md.HierarchyLevelName);
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
        public void ShouldReturnNullForEnglishPurposeWhenPurposeIsRegularCharacterStringObject()
        {
            string purpose = _md.EnglishPurpose;
            Assert.IsNull(purpose);
        }

        [Test]
        public void ShouldReturnEnglishPurposeWhenPurposeIsLocalized()
        {
            string expectedEnglishPurpose = "This is english.";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.purpose = new PT_FreeText_PropertyType
            {
                CharacterString = "Dette er norsk",
                PT_FreeText = new PT_FreeText_Type
                {
                    textGroup = new LocalisedCharacterString_PropertyType[] {
                        new LocalisedCharacterString_PropertyType {
                            LocalisedCharacterString = new LocalisedCharacterString_Type {
                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                Value = expectedEnglishPurpose
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(expectedEnglishPurpose, _md.EnglishPurpose);
        }

        [Test]
        public void ShouldUpdateEnglishPurposeWithoutDestroyingExistingLocalPurpose()
        {
            string expectedNorwegianPurpose = "Dette er formålet.";
            string expectedEnglishPurpose = "This is english.";

            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.purpose = new CharacterString_PropertyType { CharacterString = expectedNorwegianPurpose };

            _md.EnglishPurpose = expectedEnglishPurpose;

            CharacterString_PropertyType purposeElement = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.purpose;
            PT_FreeText_PropertyType freeTextElement = purposeElement as PT_FreeText_PropertyType;

            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual(expectedNorwegianPurpose, freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishPurpose, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
        }

        [Test]
        public void ShouldUpdatePurposeWithoutDestroyingEnglishPurpose()
        {
            string expectedNorwegianPurpose = "Oppdatert norsk formål";
            string expectedEnglishPurpose = "This is english.";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.purpose = new PT_FreeText_PropertyType
            {
                CharacterString = "Dette er norsk",
                PT_FreeText = new PT_FreeText_Type
                {
                    textGroup = new LocalisedCharacterString_PropertyType[] {
                        new LocalisedCharacterString_PropertyType {
                            LocalisedCharacterString = new LocalisedCharacterString_Type {
                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                Value = expectedEnglishPurpose
                            }
                        }
                    }
                }
            };

            _md.Purpose = expectedNorwegianPurpose;

            CharacterString_PropertyType purposeElement = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.purpose;
            PT_FreeText_PropertyType freeTextElement = purposeElement as PT_FreeText_PropertyType;

            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual(expectedNorwegianPurpose, freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishPurpose, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
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
        public void ShouldReturnNullForEnglishSupplementalDescriptionWhenSupplementalDescriptionIsRegularCharacterStringObject()
        {
            string supplementalDescription = _md.EnglishSupplementalDescription;
            Assert.IsNull(supplementalDescription);
        }

        [Test]
        public void ShouldReturnEnglishSupplementalDescriptionWhenSupplementalDescriptionIsLocalized()
        {
            string expectedEnglishSupplementalDescription = "This is english.";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification = new MD_DataIdentification_Type
            {
                supplementalInformation = new PT_FreeText_PropertyType
                {
                    CharacterString = "Dette er norsk",
                    PT_FreeText = new PT_FreeText_Type
                    {
                        textGroup = new LocalisedCharacterString_PropertyType[] {
                        new LocalisedCharacterString_PropertyType {
                            LocalisedCharacterString = new LocalisedCharacterString_Type {
                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                Value = expectedEnglishSupplementalDescription
                            }
                        }
                    }
                    }
                }
            };

            Assert.AreEqual(expectedEnglishSupplementalDescription, _md.EnglishSupplementalDescription);
        }

        [Test]
        public void ShouldUpdateEnglishSupplementalDescriptionWithoutDestroyingExistingLocalSupplementalDescription()
        {
            string expectedNorwegianSupplementalDescription = "Dette er utfyllende informasjonen.";
            string expectedEnglishSupplementalDescription = "This is english.";

            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification = new MD_DataIdentification_Type { supplementalInformation = new CharacterString_PropertyType { CharacterString = expectedNorwegianSupplementalDescription } };

            _md.EnglishSupplementalDescription = expectedEnglishSupplementalDescription;

            var dataIdentification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;
            var supplementalDescriptionElement = dataIdentification.supplementalInformation;
            PT_FreeText_PropertyType freeTextElement = supplementalDescriptionElement as PT_FreeText_PropertyType;

            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual(expectedNorwegianSupplementalDescription, freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishSupplementalDescription, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
        }

        [Test]
        public void ShouldUpdateSupplementalDescriptionWithoutDestroyingEnglishSupplementalDescription()
        {
            string expectedNorwegianSupplementalDescription = "Oppdatert norsk utfyllende informasjon";
            string expectedEnglishSupplementalDescription = "This is english.";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification = new MD_DataIdentification_Type
            {
                supplementalInformation = new PT_FreeText_PropertyType
                {
                    CharacterString = "Dette er norsk",
                    PT_FreeText = new PT_FreeText_Type
                    {
                        textGroup = new LocalisedCharacterString_PropertyType[] {
                            new LocalisedCharacterString_PropertyType {
                                LocalisedCharacterString = new LocalisedCharacterString_Type {
                                    locale = SimpleMetadata.LOCALE_LINK_ENG,
                                    Value = expectedEnglishSupplementalDescription
                                }
                            }
                        }
                    }
                }
            };

            _md.SupplementalDescription = expectedNorwegianSupplementalDescription;

            var dataIdentification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;
            var supplementalDescriptionElement = dataIdentification.supplementalInformation;

            PT_FreeText_PropertyType freeTextElement = supplementalDescriptionElement as PT_FreeText_PropertyType;

            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual(expectedNorwegianSupplementalDescription, freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishSupplementalDescription, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
        }

        [Test]
        public void ShouldReturnKeywords()
        {
            List<SimpleKeyword> keywords = _md.Keywords;

            Assert.AreEqual(1, keywords.Count);
        }

        [Test]
        public void ShouldReturnKeywordsWithEnglishTranslation()
        {
            List<SimpleKeyword> keywords = _md.Keywords;

            Assert.NotNull(keywords[0].EnglishKeyword, "Engelsk oversetting av nøkkelord mangler.");
            Assert.AreEqual("Addresses", keywords[0].EnglishKeyword);
        }

        [Test]
        public void ShouldUpdateKeywords()
        {
            _md.Keywords = new List<SimpleKeyword> {
                new SimpleKeyword {
                    Keyword = "Adresser",
                    Thesaurus = SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1,
                    EnglishKeyword = "Addresses"
                },
                new SimpleKeyword {
                    Keyword = "Eu direktiv 1",
                    Thesaurus = SimpleKeyword.THESAURUS_INSPIRE_PRIORITY_DATASET,
                    KeywordLink = "http://inspire.ec.europa.eu/metadata-codelist/PriorityDataset/dir-1991-31"
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
                    Keyword = "Akvakulturgrense",
                    Type = SimpleKeyword.THESAURUS_CONCEPT,
                    KeywordLink = "http://objektkatalog.geonorge.no"
                },
                new SimpleKeyword {
                    Keyword = "Norge",
                    Thesaurus = SimpleKeyword.THESAURUS_ADMIN_UNITS,
                    KeywordLink = "https://data.geonorge.no/administrativeEnheter/nasjon/id/173163"
                },
                new SimpleKeyword {
                    Keyword = "National",
                    Thesaurus = SimpleKeyword.THESAURUS_SPATIAL_SCOPE,
                    KeywordLink = SimpleKeyword.THESAURUS_SPATIAL_SCOPE_LINK
                },
                new SimpleKeyword {
                    Keyword = "Buildings",
                    Thesaurus = SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1
                },
                new SimpleKeyword {
                    Keyword = "Eksempeldata",
                    EnglishKeyword = "Example data"
                },
                new SimpleKeyword {
                    Keyword = "testing"
                },
                new SimpleKeyword {
                    Keyword = "Det offentlige kartgrunnlaget",
                    Thesaurus = SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE
                },
                new SimpleKeyword {
                    Keyword = "infoFeatureAccessService",
                    Thesaurus = SimpleKeyword.THESAURUS_SERVICE_TYPE
                },
            };

            int numberOfInspireKeywords = 0;
            int numberOfInspirePriorityDatasetKeywords = 0;
            int numberOfNationalKeywords = 0;
            int numberOfThemeKeywords = 0;
            int numberOfPlaceKeywords = 0;
            int numberOfConceptKeywords = 0;
            int numberOfAdminUnitKeywords = 0;
            int numberOfServiceTypeKeywords = 0;
            int numberOfOtherKeywords = 0;
            int numberOfSpatialScopeKeywords = 0;
            bool inspireAddressesFound = false;
            bool inspirePriorityDatasetsFound = false;
            bool inspireBuildingsFound = false;
            bool nationalDOKfound = false;
            bool placeOsloFound = false;
            bool placeAkershusFound = false;
            bool themeBygningFound = false;
            bool conceptAkvakulturgrenseFound = false;
            bool adminUnitNorwayFound = false;
            bool serviceTypeFound = false;
            bool otherEksempeldataFound = false;
            bool otherTestingFound = false;
            bool englishKeywordFound = false;
            bool englishKeywordFoundInOtherGroup = false;
            bool spatialScopeKeywordFound = false;

            MD_Keywords_PropertyType[] descriptiveKeywords = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.descriptiveKeywords;

            foreach (MD_Keywords_PropertyType descriptiveKeyword in descriptiveKeywords)
            {
                int numberOfKeywords = descriptiveKeyword.MD_Keywords.keyword.Length;
                var titleCharacterString = descriptiveKeyword.MD_Keywords?.thesaurusName?.CI_Citation.title?.item as CharacterString_PropertyType;
                var titleAnchor = descriptiveKeyword.MD_Keywords?.thesaurusName?.CI_Citation.title?.item as Anchor_Type;

                CharacterString_PropertyType title = null;
                if (titleAnchor != null)
                    title = new CharacterString_PropertyType { CharacterString = titleAnchor.Value };
                else
                    title = titleCharacterString;

                if (descriptiveKeyword.MD_Keywords.thesaurusName != null)
                {
                    if (title.CharacterString.Equals(SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1)) {
                        numberOfInspireKeywords = numberOfKeywords;
                        foreach (var k in descriptiveKeyword.MD_Keywords.keyword)
                        {
                            var keyword = k.keyword as CharacterString_PropertyType;

                            if (keyword.CharacterString.Equals("Adresser"))
                            {
                                inspireAddressesFound = true;

                                var freeText = k.keyword as PT_FreeText_PropertyType;
                                if (freeText != null && freeText.PT_FreeText.textGroup[0].LocalisedCharacterString.Value.Equals("Addresses"))
                                {
                                    englishKeywordFound = true;
                                }
                            }
                            else if (keyword.CharacterString.Equals("Buildings"))
                            {
                                inspireBuildingsFound = true;
                            }
                        }
                    }
                    else if (title.CharacterString.Equals(SimpleKeyword.THESAURUS_INSPIRE_PRIORITY_DATASET))
                    {
                        numberOfInspirePriorityDatasetKeywords = numberOfKeywords;
                        var keyword = descriptiveKeyword.MD_Keywords.keyword[0].keyword as Anchor_Type;
                        if (keyword.Value.Equals("Eu direktiv 1") && keyword.href.Equals("http://inspire.ec.europa.eu/metadata-codelist/PriorityDataset/dir-1991-31"))
                        {
                            inspirePriorityDatasetsFound = true;
                        }
                    }
                    else if (title.CharacterString.Equals(SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE))
                    {
                        numberOfNationalKeywords = numberOfKeywords;
                        var keyword = descriptiveKeyword.MD_Keywords.keyword[0].keyword as CharacterString_PropertyType;
                        if (keyword.CharacterString.Equals("Det offentlige kartgrunnlaget"))
                        {
                            nationalDOKfound = true;
                        }
                    }
                    else if (title.CharacterString.Equals(SimpleKeyword.THESAURUS_ADMIN_UNITS))
                    {
                        numberOfAdminUnitKeywords = numberOfKeywords;
                        var keyword = descriptiveKeyword.MD_Keywords.keyword[0].keyword as Anchor_Type;
                        if (keyword.Value.Equals("Norge") && keyword.href.Equals("https://data.geonorge.no/administrativeEnheter/nasjon/id/173163"))
                        {
                            adminUnitNorwayFound = true;
                        }
                    }


                    else if (title.CharacterString.Equals(SimpleKeyword.THESAURUS_SPATIAL_SCOPE))
                    {
                        numberOfSpatialScopeKeywords = numberOfKeywords;
                        var keyword = descriptiveKeyword.MD_Keywords.keyword[0].keyword as Anchor_Type;
                        if (keyword.Value.Equals("National") && keyword.href.Equals(SimpleKeyword.THESAURUS_SPATIAL_SCOPE_LINK))
                        {
                            spatialScopeKeywordFound = true;
                        }
                    }

                    else if (title.CharacterString.Equals(SimpleKeyword.THESAURUS_SERVICE_TYPE))
                    {
                        numberOfServiceTypeKeywords = numberOfKeywords;
                        var keyword = descriptiveKeyword.MD_Keywords.keyword[0].keyword as CharacterString_PropertyType;
                        if (keyword.CharacterString.Equals("infoFeatureAccessService"))
                        {
                            serviceTypeFound = true;
                        }
                    }
                }
                else if (descriptiveKeyword.MD_Keywords.type != null && descriptiveKeyword.MD_Keywords.type.MD_KeywordTypeCode.codeListValue.Equals(SimpleKeyword.TYPE_PLACE))
                {
                    numberOfPlaceKeywords = numberOfKeywords;
                    foreach (var k in descriptiveKeyword.MD_Keywords.keyword)
                    {
                        var keyword = k.keyword as CharacterString_PropertyType;

                        if (keyword.CharacterString.Equals("Oslo"))
                        {
                            placeOsloFound = true;
                        }
                        else if (keyword.CharacterString.Equals("Akershus"))
                        {
                            placeAkershusFound = true;
                        }
                    }
                }
                else if (descriptiveKeyword.MD_Keywords.type != null && descriptiveKeyword.MD_Keywords.type.MD_KeywordTypeCode.codeListValue.Equals(SimpleKeyword.TYPE_THEME))
                {
                    numberOfThemeKeywords = numberOfKeywords;
                    var keyword = descriptiveKeyword.MD_Keywords.keyword[0].keyword as CharacterString_PropertyType;
                    if (keyword.CharacterString.Equals("Bygninger"))
                    {
                        themeBygningFound = true;
                    }
                }
                else if (descriptiveKeyword.MD_Keywords.type != null && descriptiveKeyword.MD_Keywords.type.MD_KeywordTypeCode.codeListValue.Equals(SimpleKeyword.THESAURUS_CONCEPT))
                {
                    numberOfConceptKeywords = numberOfKeywords;
                    var keyword = descriptiveKeyword.MD_Keywords.keyword[0].keyword as Anchor_Type;
                    if (keyword.Value.Equals("Akvakulturgrense") && keyword.href.Equals("http://objektkatalog.geonorge.no"))
                    {
                        conceptAkvakulturgrenseFound = true;
                    }
                }
                else
                {
                    numberOfOtherKeywords = numberOfKeywords;
                    foreach (var k in descriptiveKeyword.MD_Keywords.keyword)
                    {
                        var keyword = k.keyword as CharacterString_PropertyType;

                        if (keyword.CharacterString.Equals("Eksempeldata"))
                        {
                            otherEksempeldataFound = true;
                            var freeText = k.keyword as PT_FreeText_PropertyType;
                            if (freeText != null && freeText.PT_FreeText.textGroup[0].LocalisedCharacterString.Value.Equals("Example data"))
                            {
                                englishKeywordFoundInOtherGroup = true;
                            }
                        }
                        else if (keyword.CharacterString.Equals("testing"))
                        {
                            otherTestingFound = true;
                        }
                    }
                }
            }

            Assert.AreEqual(2, numberOfInspireKeywords, "Expected two inspire keywords in same wrapper element");
            Assert.AreEqual(1, numberOfInspirePriorityDatasetKeywords, "Expected one inspire priority dataset keyword in same wrapper element");
            Assert.AreEqual(1, numberOfNationalKeywords, "Expected one national keyword in same wrapper element");
            Assert.AreEqual(1, numberOfThemeKeywords, "Expected one theme keyword in same wrapper element");
            Assert.AreEqual(2, numberOfPlaceKeywords, "Expected two place keywords in same wrapper element");
            Assert.AreEqual(1, numberOfConceptKeywords, "Expected one concept keywords in same wrapper element");
            Assert.AreEqual(1, numberOfAdminUnitKeywords, "Expected one admin unit keyword in same wrapper element");
            Assert.AreEqual(1, numberOfServiceTypeKeywords, "Expected one service type keyword in same wrapper element");
            Assert.AreEqual(2, numberOfOtherKeywords, "Expected two other keywords in same wrapper element");
            Assert.AreEqual(1, numberOfSpatialScopeKeywords, "Expected one service type keyword in same wrapper element");

            Assert.True(inspireAddressesFound);
            Assert.True(inspirePriorityDatasetsFound);
            Assert.True(inspireBuildingsFound);
            Assert.True(nationalDOKfound);
            Assert.True(placeOsloFound);
            Assert.True(placeAkershusFound);
            Assert.True(themeBygningFound);
            Assert.True(conceptAkvakulturgrenseFound);
            Assert.True(adminUnitNorwayFound);
            Assert.True(serviceTypeFound);
            Assert.True(otherEksempeldataFound);
            Assert.True(otherTestingFound);
            Assert.True(englishKeywordFound, "Mangler engelsk oversetting av nøkkelord");
            Assert.True(englishKeywordFoundInOtherGroup, "Engelsk nøkkelord mangler for ikke-gruppert nøkkelord");
            Assert.True(spatialScopeKeywordFound);

            Trace.WriteLine(SerializeUtil.SerializeToString(_md.GetMetadata()));

        }

        [Test]
        public void ShouldReturnThesaurusForKeyword()
        {
            _md.Keywords = new List<SimpleKeyword> {

                new SimpleKeyword {
                    Keyword = "Eu direktiv 1",
                    Thesaurus = SimpleKeyword.THESAURUS_INSPIRE_PRIORITY_DATASET,
                    KeywordLink = "http://inspire.ec.europa.eu/metadata-codelist/PriorityDataset/dir-1991-31"
                }
            };

            var keyword = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.descriptiveKeywords[0];

            var thesaurus = keyword.MD_Keywords.thesaurusName.CI_Citation.title.item as Anchor_Type;
            Assert.AreEqual(SimpleKeyword.THESAURUS_INSPIRE_PRIORITY_DATASET, thesaurus.Value);
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
        public void ShouldReturnDistributionFormatsWhenPresent()
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

            var format = _md.DistributionFormats;
            Assert.NotNull(format);
            Assert.AreEqual(expectedName, format[0].Name);
            Assert.AreEqual(expectedVersion, format[0].Version);
        }

        [Test]
        public void ShouldHandleNullValueInDistributionFormatsVersion()
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

            var format = _md.DistributionFormats;
            Assert.NotNull(format);
            Assert.AreEqual(expectedName, format[0].Name);
            Assert.IsNull(format[0].Version);
        }


        //[Test]
        //public void ShouldReturnNullWhenReferenceSystemIsNull()
        //{
        //    Assert.IsNull(_md.ReferenceSystem);
        //}

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

        //[Test]
        //public void ShouldReturnNullWhenReferenceSystemsIsNull()
        //{
        //    Assert.IsNull(_md.ReferenceSystems);
        //}

        [Test]
        public void ShouldReturnReferenceSystemsWhenPresent()
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

            var rs = _md.ReferenceSystems;
            Assert.NotNull(rs);
            Assert.AreEqual(expectedCode, rs[0].CoordinateSystem);
            Assert.AreEqual(expectedCodeSpace, rs[0].Namespace);
        }

        [Test]
        public void ShouldUpdateReferenceSystems()
        {
            string expectedCoordinateSystem = "system";
            string expectedNamespace = "namespace";
            _md.ReferenceSystems = new List<SimpleReferenceSystem>
            {
                new SimpleReferenceSystem
                {
                    CoordinateSystem = expectedCoordinateSystem,
                    Namespace = expectedNamespace
                }

            };

            var identifier = _md.GetMetadata().referenceSystemInfo[0].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier;

            Assert.AreEqual(expectedCoordinateSystem, identifier.code.CharacterString);
            Assert.AreEqual(expectedNamespace, identifier.codeSpace.CharacterString);
        }

        [Test]
        public void ShouldReturnNullWhenResourceReferenceIsNull()
        {
            Assert.IsNull(_md.ResourceReference);
        }

        [Test]
        public void ShouldReturnResourceReferenceWhenPresent()
        {
            string expectedCode = "code";
            string expectedCodeSpace = "codespace";
            _md.GetMetadata().identificationInfo = new MD_Identification_PropertyType[]
            {
               new MD_Identification_PropertyType
               {
                   AbstractMD_Identification = new SV_ServiceIdentification_Type
                   {
                       citation = new CI_Citation_PropertyType
                       {
                           CI_Citation = new CI_Citation_Type
                           {
                               identifier = new MD_Identifier_PropertyType[]
                               {
                                   new MD_Identifier_PropertyType
                                   {
                                    MD_Identifier = new RS_Identifier_Type
                                        {
                                        code = new CharacterString_PropertyType { CharacterString = expectedCode },
                                        codeSpace = new CharacterString_PropertyType { CharacterString = expectedCodeSpace }
                                        }
                                   }
                               }
                           }
                       }
                   }

               }
            };


            var rs = _md.ResourceReference;
            Assert.NotNull(rs);
            Assert.AreEqual(expectedCode, rs.Code);
            Assert.AreEqual(expectedCodeSpace, rs.Codespace);
        }

        [Test]
        public void ShouldUpdateResourceReference()
        {
            string expectedResourceReference = "system";
            string expectedNamespace = "namespace";
            _md.ResourceReference = new SimpleResourceReference
            {
                Code = expectedResourceReference,
                Codespace = expectedNamespace
            };

            var identifier = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier[0].MD_Identifier as RS_Identifier_Type;

            Assert.AreEqual(expectedResourceReference, identifier.code.CharacterString);
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
            string expectedUnitsOfDistribution = "unit1";
            string expectedEnglishUnitsOfDistribution = "unit1eng";
            _md.GetMetadata().distributionInfo = new MD_Distribution_PropertyType {
                MD_Distribution = new MD_Distribution_Type
                {
                    transferOptions = new MD_DigitalTransferOptions_PropertyType[]
                    {
                        new MD_DigitalTransferOptions_PropertyType {
                            MD_DigitalTransferOptions = new MD_DigitalTransferOptions_Type {
                                unitsOfDistribution = new PT_FreeText_PropertyType
                                {
                                    CharacterString = expectedUnitsOfDistribution,
                                    PT_FreeText = new PT_FreeText_Type
                                    {
                                        textGroup = new LocalisedCharacterString_PropertyType[]
                                        {
                                            new LocalisedCharacterString_PropertyType
                                            {
                                            LocalisedCharacterString = new LocalisedCharacterString_Type
                                                {
                                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                                Value = expectedEnglishUnitsOfDistribution
                                                }
                                            }
                                        }
                                    }
                                },
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
            Assert.AreEqual(expectedUnitsOfDistribution, details.UnitsOfDistribution);
            Assert.AreEqual(expectedEnglishUnitsOfDistribution, details.EnglishUnitsOfDistribution);

        }

        [Test]
        public void ShouldUpdateDistributionDetails()
        {
            string expectedURL = "http://example.com";
            string expectedProtocol = "www";
            string expectedName = "navn";
            string expectedUnitsOfDistribution = "unit1";
            string expectedEnglishUnitsOfDistribution = "unit1eng";
            _md.DistributionDetails = new SimpleDistributionDetails { Protocol = expectedProtocol, URL = expectedURL, Name = expectedName, UnitsOfDistribution = expectedUnitsOfDistribution, EnglishUnitsOfDistribution = expectedEnglishUnitsOfDistribution };

            var resource = _md.GetMetadata().distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource;
            var tranferOptions = _md.GetMetadata().distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions;
            var actualEnglishUnitsOfDistribution = tranferOptions.unitsOfDistribution as PT_FreeText_PropertyType;
            Assert.AreEqual(expectedURL, resource.linkage.URL);
            Assert.AreEqual(expectedProtocol, resource.protocol.CharacterString);
            Assert.AreEqual(expectedName, resource.name.CharacterString);
            Assert.AreEqual(expectedUnitsOfDistribution, tranferOptions.unitsOfDistribution.CharacterString);
            Assert.AreEqual(expectedEnglishUnitsOfDistribution, actualEnglishUnitsOfDistribution.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
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
            string expectedEnglishExplanation = "explained english";
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
                                                        title = new CI_Citation_Title{ item = toCharString(expectedTitle) },
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
                                                explanation = new PT_FreeText_PropertyType
                                                {
                                                    CharacterString = expectedExplanation,
                                                    PT_FreeText = new PT_FreeText_Type
                                                    {
                                                        textGroup = new LocalisedCharacterString_PropertyType[]
                                                        {
                                                            new LocalisedCharacterString_PropertyType
                                                            {
                                                            LocalisedCharacterString = new LocalisedCharacterString_Type
                                                                {
                                                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                                                Value = expectedEnglishExplanation
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
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
            Assert.AreEqual(expectedEnglishExplanation, spec.EnglishExplanation);
            Assert.AreEqual(expectedResult, spec.Result);
        }

        [Test]
        public void ShouldUpdateQualitySpecification()
        {
            string expectedTitle = "title";
            string expectedDate = "2014-01-01";
            string expectedDateType = "creation";
            string expectedExplanation = "explained";
            string expectedEnglishExplanation = "explained english";
            bool expectedResult = true;

            _md.QualitySpecification = new SimpleQualitySpecification
            {
                Title = expectedTitle,
                Date = expectedDate,
                DateType = expectedDateType,
                Explanation = expectedExplanation,
                EnglishExplanation = expectedEnglishExplanation,
                Result = expectedResult
            };

            DQ_DomainConsistency_Type domainConsistency = _md.GetMetadata().dataQualityInfo[0].DQ_DataQuality.report[0].AbstractDQ_Element as DQ_DomainConsistency_Type;
            DQ_ConformanceResult_Type conformanceResult = domainConsistency.result[0].AbstractDQ_Result as DQ_ConformanceResult_Type;

            var actualExplanation = conformanceResult.explanation as PT_FreeText_PropertyType;
            var title = conformanceResult.specification.CI_Citation.title.item as CharacterString_PropertyType;

            Assert.AreEqual(expectedTitle, title.CharacterString);
            Assert.AreEqual(expectedDate, (string)conformanceResult.specification.CI_Citation.date[0].CI_Date.date.Item);
            Assert.AreEqual(expectedDateType, conformanceResult.specification.CI_Citation.date[0].CI_Date.dateType.CI_DateTypeCode.codeListValue);
            Assert.AreEqual(expectedExplanation, conformanceResult.explanation.CharacterString);
            Assert.AreEqual(expectedEnglishExplanation, actualExplanation.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
            Assert.AreEqual(expectedResult, conformanceResult.pass.Boolean);
        }


        [Test]
        public void ShouldReturnNullWhenQualitySpecificationsDataIsNull()
        {
            _md.GetMetadata().dataQualityInfo = null;
            Assert.IsNull(_md.QualitySpecifications);
        }

        [Test]
        public void ShouldReturnQualitySpecificationsData()
        {
            string expectedTitle = "title";
            string expectedDate = "2014-01-01";
            string expectedDateType = "creation";
            string expectedExplanation = "explained";
            string expectedEnglishExplanation = "explained english";
            bool expectedResult = true;
            string expectedResponsible = "SOSI";

            string expectedTitle2 = "title2";
            string expectedDate2 = "2014-01-01";
            string expectedDateType2 = "creation";
            string expectedExplanation2 = "explained";
            string expectedEnglishExplanation2 = "explained english";
            bool expectedResult2 = false;
            string expectedResponsible2 = "Inspire";

            string expectedTitle3 = "title3";
            string expectedDate3 = "2014-01-01";
            string expectedDateType3 = "creation";
            string expectedExplanation3 = "explained";
            string expectedEnglishExplanation3 = "explained english";
            bool expectedResult3 = false;
            string expectedResponsible3 = "OtherSpec";

            string expectedInteroperabelQuantitativeResultPerformance = "1,56";


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
                                                        title = new CI_Citation_Title{ item = toCharString(expectedTitle) },
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
                                                        },
                                                        identifier = new MD_Identifier_PropertyType[]{
                                                            new MD_Identifier_PropertyType{
                                                                MD_Identifier = new MD_Identifier_Type{
                                                                    authority = new CI_Citation_PropertyType{
                                                                        CI_Citation = new CI_Citation_Type{
                                                                            title = new CI_Citation_Title{ item = new CharacterString_PropertyType{ CharacterString = expectedResponsible } }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                explanation = new PT_FreeText_PropertyType
                                                {
                                                    CharacterString = expectedExplanation,
                                                    PT_FreeText = new PT_FreeText_Type
                                                    {
                                                        textGroup = new LocalisedCharacterString_PropertyType[]
                                                        {
                                                            new LocalisedCharacterString_PropertyType
                                                            {
                                                            LocalisedCharacterString = new LocalisedCharacterString_Type
                                                                {
                                                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                                                Value = expectedEnglishExplanation
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                pass = new Boolean_PropertyType { Boolean = expectedResult }
                                            }
                                        },
                                        new DQ_Result_PropertyType {
                                            AbstractDQ_Result = new DQ_ConformanceResult_Type {
                                                specification = new CI_Citation_PropertyType {
                                                    CI_Citation = new CI_Citation_Type {
                                                        title = new CI_Citation_Title{ item = toCharString(expectedTitle2) },
                                                        date = new CI_Date_PropertyType[] {
                                                            new CI_Date_PropertyType {
                                                                CI_Date = new CI_Date_Type {
                                                                    date = new Date_PropertyType {
                                                                        Item = expectedDate2
                                                                    },
                                                                    dateType = new CI_DateTypeCode_PropertyType {
                                                                        CI_DateTypeCode = new CodeListValue_Type {
                                                                            codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#CI_DateTypeCode",
                                                                            codeListValue = expectedDateType2
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        },
                                                         identifier = new MD_Identifier_PropertyType[]{
                                                            new MD_Identifier_PropertyType{
                                                                MD_Identifier = new MD_Identifier_Type{
                                                                    authority = new CI_Citation_PropertyType{
                                                                        CI_Citation = new CI_Citation_Type{
                                                                            title = new CI_Citation_Title { item =  new CharacterString_PropertyType{ CharacterString = expectedResponsible2 } }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                explanation = new PT_FreeText_PropertyType
                                                {
                                                    CharacterString = expectedExplanation2,
                                                    PT_FreeText = new PT_FreeText_Type
                                                    {
                                                        textGroup = new LocalisedCharacterString_PropertyType[]
                                                        {
                                                            new LocalisedCharacterString_PropertyType
                                                            {
                                                            LocalisedCharacterString = new LocalisedCharacterString_Type
                                                                {
                                                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                                                Value = expectedEnglishExplanation2
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                pass = new Boolean_PropertyType { Boolean = expectedResult2 }
                                            }
                                        }
                                    }
                                }
                            },
                            new DQ_Element_PropertyType
                            {
                                AbstractDQ_Element = new DQ_DomainConsistency_Type {
                                    result = new DQ_Result_PropertyType[] {
                                        new DQ_Result_PropertyType {
                                            AbstractDQ_Result = new DQ_ConformanceResult_Type {
                                                specification = new CI_Citation_PropertyType {
                                                    CI_Citation = new CI_Citation_Type {
                                                        title = new CI_Citation_Title{ item = toCharString(expectedTitle3) },
                                                        date = new CI_Date_PropertyType[] {
                                                            new CI_Date_PropertyType {
                                                                CI_Date = new CI_Date_Type {
                                                                    date = new Date_PropertyType {
                                                                        Item = expectedDate3
                                                                    },
                                                                    dateType = new CI_DateTypeCode_PropertyType {
                                                                        CI_DateTypeCode = new CodeListValue_Type {
                                                                            codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#CI_DateTypeCode",
                                                                            codeListValue = expectedDateType3
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        },
                                                        identifier = new MD_Identifier_PropertyType[]{
                                                            new MD_Identifier_PropertyType{
                                                                MD_Identifier = new MD_Identifier_Type{
                                                                    authority = new CI_Citation_PropertyType{
                                                                        CI_Citation = new CI_Citation_Type{
                                                                            title = new CI_Citation_Title{ item = new CharacterString_PropertyType{ CharacterString = expectedResponsible3 } }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                explanation = new PT_FreeText_PropertyType
                                                {
                                                    CharacterString = expectedExplanation3,
                                                    PT_FreeText = new PT_FreeText_Type
                                                    {
                                                        textGroup = new LocalisedCharacterString_PropertyType[]
                                                        {
                                                            new LocalisedCharacterString_PropertyType
                                                            {
                                                            LocalisedCharacterString = new LocalisedCharacterString_Type
                                                                {
                                                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                                                Value = expectedEnglishExplanation3
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                pass = new Boolean_PropertyType { Boolean = expectedResult3 }
                                            }
                                        }
                                    }
                                }
                            },
                                new DQ_Element_PropertyType
                                {
                                    AbstractDQ_Element = new DQ_ConceptualConsistency_Type
                                    {
                                        nameOfMeasure = new Measure_Type[]
                                        {
                                            new Measure_Type
                                            {
                                                type = new Anchor_Type{  href = "http://inspire.ec.europa.eu/metadata-codelist/QualityOfServiceCriteriaCode/performance",  Value = "performance" }
                                            }
                                        },
                                        measureDescription = new CharacterString_PropertyType
                                        {
                                            CharacterString = "The maximum time in which a typical request to the Spatial Data Service can be carried out in a off-peak load situation"
                                        },
                                        result = new DQ_Result_PropertyType[]
                                        {
                                            new DQ_Result_PropertyType
                                            {
                                                AbstractDQ_Result = new DQ_QuantitativeResult_Type
                                                {
                                                    valueUnit = new UnitOfMeasure_PropertyType{ href = "*http://www.opengis.net/def/uom/SI/second*" } ,
                                                    value = new Record_PropertyType[]
                                                    {
                                                        new Record_PropertyType
                                                        {
                                                            Record = Convert.ToDouble(expectedInteroperabelQuantitativeResultPerformance)
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                        }
                    }
                }
            };


            List<SimpleQualitySpecification> spec = _md.QualitySpecifications;
            Assert.AreEqual(4, spec.Count);

            Assert.IsNotNull(spec);
            Assert.AreEqual(expectedTitle, spec[0].Title);
            Assert.AreEqual(expectedDate, spec[0].Date);
            Assert.AreEqual(expectedDateType, spec[0].DateType);
            Assert.AreEqual(expectedExplanation, spec[0].Explanation);
            Assert.AreEqual(expectedEnglishExplanation, spec[0].EnglishExplanation);
            Assert.AreEqual(expectedResult, spec[0].Result);
            Assert.AreEqual(expectedResponsible, spec[0].Responsible);

            Assert.AreEqual(expectedTitle2, spec[1].Title);
            Assert.AreEqual(expectedDate2, spec[1].Date);
            Assert.AreEqual(expectedDateType2, spec[1].DateType);
            Assert.AreEqual(expectedExplanation2, spec[1].Explanation);
            Assert.AreEqual(expectedEnglishExplanation2, spec[1].EnglishExplanation);
            Assert.AreEqual(expectedResult2, spec[1].Result);
            Assert.AreEqual(expectedResponsible2, spec[1].Responsible);

            Assert.AreEqual(expectedInteroperabelQuantitativeResultPerformance, spec[3].QuantitativeResult);

        }


        [Test]
        public void ShouldUpdateQualitySpecifications()
        {
            string expectedTitle = "title";
            string expectedDate = "2014-01-01";
            string expectedDateType = "creation";
            string expectedExplanation = "explained";
            string expectedEnglishExplanation = "explained english";
            bool expectedResult = true;
            string expectedResponsible = "SOSI";

            string expectedTitle2 = "title2";
            string expectedDate2 = "2014-01-01";
            string expectedDateType2 = "creation";
            string expectedExplanation2 = "explained";
            string expectedEnglishExplanation2 = "explained english";
            bool expectedResult2 = false;
            string expectedResponsible2 = "Inspire";

            string expectedSpecificationLink3 = "http://inspire.ec.europa.eu/id/citation/ir/reg-976-2009";
            string expectedTitle3 = "title3";
            string expectedTitleLink3 = "http://inspire.ec.europa.eu/id/ats/metadata/2.0/sds-invocable";
            string expectedTitleLinkDescription3 = "INSPIRE Invocable Spatial Data Services metadata";
            string expectedDate3 = "2014-01-01";
            string expectedDateType3 = "creation";
            string expectedExplanation3 = "explained";
            string expectedEnglishExplanation3 = "explained english";
            bool expectedResult3 = false;
            string expectedResponsible3 = "OtherSpec";

            string expectedResponsibleInteroperabelPerformance = "sds-performance";
            string expectedInteroperabelQuantitativeResultPerformance = "1,56";

            string expectedResponsibleInteroperabelAvailability = "sds-availability";
            string expectedInteroperabelQuantitativeResultAvailability = "98,9";

            string expectedResponsibleInteroperabelCapacity = "sds-capacity";
            string expectedInteroperabelQuantitativeResultCapacity = "1000";

            List<SimpleQualitySpecification> QualityList = new List<SimpleQualitySpecification>();

            QualityList.Add(new SimpleQualitySpecification
            {
                Title = expectedTitle,
                Date = expectedDate,
                DateType = expectedDateType,
                Explanation = expectedExplanation,
                EnglishExplanation = expectedEnglishExplanation,
                Result = expectedResult,
                Responsible = expectedResponsible
            });

            QualityList.Add(new SimpleQualitySpecification
            {
                Title = expectedTitle2,
                Date = expectedDate2,
                DateType = expectedDateType2,
                Explanation = expectedExplanation2,
                EnglishExplanation = expectedEnglishExplanation2,
                Result = expectedResult2,
                Responsible = expectedResponsible2
            });

            QualityList.Add(new SimpleQualitySpecification
            {
                SpecificationLink = expectedSpecificationLink3,
                Title = expectedTitle3,
                TitleLink = expectedTitleLink3,
                TitleLinkDescription = expectedTitleLinkDescription3,
                Date = expectedDate3,
                DateType = expectedDateType3,
                Explanation = expectedExplanation3,
                EnglishExplanation = expectedEnglishExplanation3,
                Result = expectedResult3,
                Responsible = expectedResponsible3
            });

            QualityList.Add(new SimpleQualitySpecification
            {
                Responsible = expectedResponsibleInteroperabelPerformance,
                QuantitativeResult = expectedInteroperabelQuantitativeResultPerformance
            });

            QualityList.Add(new SimpleQualitySpecification
            {
                Responsible = expectedResponsibleInteroperabelAvailability,
                QuantitativeResult = expectedInteroperabelQuantitativeResultAvailability
            });

            QualityList.Add(new SimpleQualitySpecification
            {
                Responsible = expectedResponsibleInteroperabelCapacity,
                QuantitativeResult = expectedInteroperabelQuantitativeResultCapacity
            });


            _md.QualitySpecifications = QualityList;

            Trace.WriteLine(SerializeUtil.SerializeToString(_md.GetMetadata()));

            DQ_DomainConsistency_Type domainConsistency = _md.GetMetadata().dataQualityInfo[0].DQ_DataQuality.report[0].AbstractDQ_Element as DQ_DomainConsistency_Type;
            DQ_ConformanceResult_Type conformanceResult = domainConsistency.result[0].AbstractDQ_Result as DQ_ConformanceResult_Type;
            DQ_ConformanceResult_Type conformanceResult2 = domainConsistency.result[1].AbstractDQ_Result as DQ_ConformanceResult_Type;
            DQ_DomainConsistency_Type domainConsistency2 = _md.GetMetadata().dataQualityInfo[0].DQ_DataQuality.report[1].AbstractDQ_Element as DQ_DomainConsistency_Type;
            DQ_ConformanceResult_Type conformanceResult3 = domainConsistency2.result[0].AbstractDQ_Result as DQ_ConformanceResult_Type;
            var explanation = conformanceResult.explanation as PT_FreeText_PropertyType;
            var explanation2 = conformanceResult2.explanation as PT_FreeText_PropertyType;
            var title = conformanceResult.specification.CI_Citation.title.item as CharacterString_PropertyType;
            var responsible = conformanceResult.specification.CI_Citation.identifier[0].MD_Identifier.authority.CI_Citation.title.item as CharacterString_PropertyType;
            var title2 = conformanceResult2.specification.CI_Citation.title.item as CharacterString_PropertyType;
            var responsible2 = conformanceResult2.specification.CI_Citation.identifier[0].MD_Identifier.authority.CI_Citation.title.item as CharacterString_PropertyType;

            Assert.AreEqual(expectedTitle, title.CharacterString);
            Assert.AreEqual(expectedDate, (string)conformanceResult.specification.CI_Citation.date[0].CI_Date.date.Item);
            Assert.AreEqual(expectedDateType, conformanceResult.specification.CI_Citation.date[0].CI_Date.dateType.CI_DateTypeCode.codeListValue);
            Assert.AreEqual(expectedExplanation, conformanceResult.explanation.CharacterString);
            Assert.AreEqual(expectedEnglishExplanation, explanation.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
            Assert.AreEqual(expectedResult, conformanceResult.pass.Boolean);
            Assert.AreEqual(expectedResponsible, responsible.CharacterString);

            Assert.AreEqual(expectedTitle2, title2.CharacterString);
            Assert.AreEqual(expectedDate2, (string)conformanceResult2.specification.CI_Citation.date[0].CI_Date.date.Item);
            Assert.AreEqual(expectedDateType2, conformanceResult2.specification.CI_Citation.date[0].CI_Date.dateType.CI_DateTypeCode.codeListValue);
            Assert.AreEqual(expectedExplanation2, conformanceResult2.explanation.CharacterString);
            Assert.AreEqual(expectedEnglishExplanation2, explanation2.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
            Assert.AreEqual(expectedResult2, conformanceResult2.pass.Boolean);
            Assert.AreEqual(expectedResponsible2, responsible2.CharacterString);

            var anchorTitle3 = conformanceResult3.specification.CI_Citation.title.item as Anchor_Type;
            Assert.AreEqual(expectedTitle3, anchorTitle3.Value);
            Assert.AreEqual(expectedSpecificationLink3, conformanceResult3.specification.href);
            Assert.AreEqual(expectedTitleLink3, anchorTitle3.href);
            Assert.AreEqual(expectedTitleLinkDescription3, anchorTitle3.title);

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
        public void ShouldReturnNullForEnglishProcessHistoryWhenProcessHistoryIsRegularCharacterStringObject()
        {
            string processHistory = _md.EnglishProcessHistory;
            Assert.IsNull(processHistory);
        }

        [Test]
        public void ShouldReturnEnglishProcessHistoryWhenProcessHistoryIsLocalized()
        {
            string expectedEnglishProcessHistory = "This is english.";
            _md.GetMetadata().dataQualityInfo[0].DQ_DataQuality = new DQ_DataQuality_Type
            {
                lineage = new LI_Lineage_PropertyType
                {
                    LI_Lineage = new LI_Lineage_Type
                    {
                        statement = new PT_FreeText_PropertyType
                        {
                            CharacterString = "Dette er norsk",
                            PT_FreeText = new PT_FreeText_Type
                            {
                                textGroup = new LocalisedCharacterString_PropertyType[]
                                {
                                    new LocalisedCharacterString_PropertyType
                                    {
                                    LocalisedCharacterString = new LocalisedCharacterString_Type
                                        {
                                        locale = SimpleMetadata.LOCALE_LINK_ENG,
                                        Value = expectedEnglishProcessHistory
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };        

            Assert.AreEqual(expectedEnglishProcessHistory, _md.EnglishProcessHistory);
        }

        [Test]
        public void ShouldUpdateProcessHistoryWithoutDestroyingExistingLocalEnglishProcessHistory()
        {
            string expectedNorwegianProcessHistory = "Dette er prosesshistorie.";
            string expectedEnglishProcessHistory = "This is english.";

            _md.GetMetadata().dataQualityInfo[0].DQ_DataQuality = new DQ_DataQuality_Type { lineage = new LI_Lineage_PropertyType { LI_Lineage = new LI_Lineage_Type { statement = toCharString(expectedNorwegianProcessHistory) } } };

            _md.EnglishProcessHistory = expectedEnglishProcessHistory;

            var processHistoryElement = _md.GetMetadata().dataQualityInfo[0].DQ_DataQuality.lineage.LI_Lineage.statement;
            PT_FreeText_PropertyType freeTextElement = processHistoryElement as PT_FreeText_PropertyType;

            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual(expectedNorwegianProcessHistory, freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishProcessHistory, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
        }

        [Test]
        public void ShouldUpdateProcessHistoryWithoutDestroyingEnglishProcessHistory()
        {
            string expectedNorwegianProcessHistory = "Oppdatert norsk prosesshistorie";
            string expectedEnglishProcessHistory = "This is english.";
            _md.GetMetadata().dataQualityInfo[0].DQ_DataQuality = new DQ_DataQuality_Type
            {
                lineage = new LI_Lineage_PropertyType
                {
                    LI_Lineage = new LI_Lineage_Type
                    {
                        statement = new PT_FreeText_PropertyType
                        {
                            CharacterString = "Dette er norsk",
                            PT_FreeText = new PT_FreeText_Type
                            {
                                textGroup = new LocalisedCharacterString_PropertyType[] {
                                        new LocalisedCharacterString_PropertyType {
                                            LocalisedCharacterString = new LocalisedCharacterString_Type {
                                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                                Value = expectedEnglishProcessHistory
                                            }
                                        }
                                    }
                            }
                        }
                    }
                }
            };

            _md.ProcessHistory = expectedNorwegianProcessHistory;

            var processHistoryElement = _md.GetMetadata().dataQualityInfo[0].DQ_DataQuality.lineage.LI_Lineage.statement;

            PT_FreeText_PropertyType freeTextElement = processHistoryElement as PT_FreeText_PropertyType;

            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual(expectedNorwegianProcessHistory, freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishProcessHistory, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
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

            Assert.AreEqual(expectedDateString, (string)GetCitationDateWithType("publication"));
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
        public void ShouldReturnBoundingBoxWhenTwoExtentElementExistsAndOnlyTheSecondOneHasGeographicElement()
        {
            var dataIdentification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;

            EX_Extent_PropertyType extentWithGeographicElement = dataIdentification.extent[0];

            dataIdentification.extent = new EX_Extent_PropertyType[] {
                new EX_Extent_PropertyType
                {
                    EX_Extent = new EX_Extent_Type { description = toCharString("hello World!") }
                },
                extentWithGeographicElement
            };

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
        public void ShouldUpdateBoundingBoxWithoutDestroyingOtherInformationInExtentElement()
        {
            var dataIdentification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;

            EX_Extent_Type extent = dataIdentification.extent[0].EX_Extent;
            extent.description = toCharString("Hello World");
            extent.temporalElement = new EX_TemporalExtent_PropertyType[] { };
            extent.verticalElement = new EX_VerticalExtent_PropertyType[] { };

            _md.BoundingBox = new SimpleBoundingBox
            {
                EastBoundLongitude = "12",
                WestBoundLongitude = "22",
                NorthBoundLatitude = "44",
                SouthBoundLatitude = "33"
            };

            var identification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;
            var actualExtent = identification.extent[0].EX_Extent;
            var bbox = actualExtent.geographicElement[0].AbstractEX_GeographicExtent as EX_GeographicBoundingBox_Type;

            Assert.AreEqual("12", bbox.eastBoundLongitude.Decimal.ToString());
            Assert.AreEqual("22", bbox.westBoundLongitude.Decimal.ToString());
            Assert.AreEqual("44", bbox.northBoundLatitude.Decimal.ToString());
            Assert.AreEqual("33", bbox.southBoundLatitude.Decimal.ToString());

            Assert.NotNull(actualExtent.description);
            Assert.NotNull(actualExtent.temporalElement);
            Assert.NotNull(actualExtent.verticalElement);
        }

        [Test]
        public void ShouldMoveExtentInformationIntoOneElement()
        {
            var dataIdentification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;

            dataIdentification.extent = new EX_Extent_PropertyType[] {
                new EX_Extent_PropertyType {
                    EX_Extent = new EX_Extent_Type {
                        description = new CharacterString_PropertyType { CharacterString = "Hello world" },
                        temporalElement = new EX_TemporalExtent_PropertyType[] { }
                    }
                },
                new EX_Extent_PropertyType {
                    EX_Extent = new EX_Extent_Type {
                        geographicElement = new EX_GeographicExtent_PropertyType[] { },
                        verticalElement = new EX_VerticalExtent_PropertyType[] { }
                    }
                }
            };

            _md.BoundingBox = new SimpleBoundingBox
            {
                EastBoundLongitude = "12",
                WestBoundLongitude = "22",
                NorthBoundLatitude = "44",
                SouthBoundLatitude = "33"
            };

            var identification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;

            Assert.AreEqual(1, identification.extent.Length, "More than one extent!");
            var actualExtent = identification.extent[0].EX_Extent;

            Assert.NotNull(actualExtent.description);
            Assert.NotNull(actualExtent.geographicElement);

            var bbox = actualExtent.geographicElement[0].AbstractEX_GeographicExtent as EX_GeographicBoundingBox_Type;

            Assert.AreEqual("12", bbox.eastBoundLongitude.Decimal.ToString());
            Assert.AreEqual("22", bbox.westBoundLongitude.Decimal.ToString());
            Assert.AreEqual("44", bbox.northBoundLatitude.Decimal.ToString());
            Assert.AreEqual("33", bbox.southBoundLatitude.Decimal.ToString());

            Assert.NotNull(actualExtent.temporalElement);
            Assert.NotNull(actualExtent.verticalElement);
        }


        [Test]
        public void ShouldReturnConstraints()
        {
            SimpleConstraints constraints = _md.Constraints;

            Assert.AreEqual("Gratis å benytte til alle formål.", constraints.UseLimitations);
            Assert.AreEqual("Ingen begrensninger på bruk.", constraints.OtherConstraints);
            Assert.AreEqual("http://test.no", constraints.OtherConstraintsLink);
            Assert.AreEqual("Link", constraints.OtherConstraintsLinkText);
            Assert.AreEqual("unclassified", constraints.SecurityConstraints);
            Assert.AreEqual("Text that describes why it is not freely open", constraints.SecurityConstraintsNote);
            Assert.AreEqual("none", constraints.AccessConstraints);
            Assert.AreEqual("free", constraints.UseConstraints);
            Assert.AreEqual("norway digital restricted", constraints.OtherConstraintsAccess);
            Assert.AreEqual("Free of charge", constraints.EnglishUseLimitations);
            Assert.AreEqual("No restrictions", constraints.EnglishOtherConstraints);
        }

        [Test]
        public void ShouldUpdateConstraints()
        {
            string expectedUseLimitations = "ingen begrensninger";
            string expectedOtherConstraints = "ingen andre begrensninger";
            string expectedOtherConstraintsLink = "http://test.no";
            string expectedOtherConstraintsLinkText = "Link";
            string expectedSecurityConstraints = "classified";
            string expectedSecurityConstraintsNote = "Text that describes why it is not freely open";
            string expectedAccessConstraints = "restricted";
            string expectedUseConstraints = "license";
            string expectedOtherConstraintsAccess = "norway digital restricted";
            string expectedEnglishUseLimitations = "no limitations";
            string expectedEnglishOtherConstraints = "no limitations";

            SimpleConstraints constraints = new SimpleConstraints
            {
                UseLimitations = expectedUseLimitations,
                OtherConstraints = expectedOtherConstraints,
                OtherConstraintsLink = expectedOtherConstraintsLink,
                OtherConstraintsLinkText = expectedOtherConstraintsLinkText,
                SecurityConstraints = expectedSecurityConstraints,
                SecurityConstraintsNote = expectedSecurityConstraintsNote,
                AccessConstraints = expectedAccessConstraints,
                UseConstraints = expectedUseConstraints,
                OtherConstraintsAccess = expectedOtherConstraintsAccess,
                EnglishUseLimitations = expectedEnglishUseLimitations,
                EnglishOtherConstraints = expectedEnglishOtherConstraints

            };
            _md.Constraints = constraints;

            string actualUseLimitation = null;
            string actualSecurityConstraint = null;
            string actualSecurityConstraintNote = null;
            string actualAccessConstraint = null;
            string actualOtherConstraint = null;
            string actualOtherConstraintLink = null;
            string actualOtherConstraintLinkText = null;
            string actualUseConstraint = null;
            string actualOtherConstraintsAccess = null;
            string actualEnglishUseLimitation = null;
            string actualEnglishOtherConstraints = null;
            MD_DataIdentification_Type identification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;

            foreach (var constraint in identification.resourceConstraints)
            {
                MD_SecurityConstraints_Type securityConstraint = constraint.MD_Constraints as MD_SecurityConstraints_Type;
                if (securityConstraint != null)
                {
                    actualSecurityConstraint = securityConstraint.classification.MD_ClassificationCode.codeListValue;
                    actualSecurityConstraintNote = securityConstraint.userNote.CharacterString;
                }
                else
                {
                    MD_LegalConstraints_Type legalConstraint = constraint.MD_Constraints as MD_LegalConstraints_Type;
                    if (legalConstraint != null)
                    {
                        actualAccessConstraint = legalConstraint.accessConstraints[0].MD_RestrictionCode.codeListValue;

                        var otherConstraintString = legalConstraint.otherConstraints[0].MD_RestrictionOther as CharacterString_PropertyType;
                        if (otherConstraintString != null)
                            actualOtherConstraint = otherConstraintString.CharacterString;

                        var englishOtherConstraint = legalConstraint.otherConstraints[0].MD_RestrictionOther as PT_FreeText_PropertyType;
                        if (englishOtherConstraint != null)
                            actualEnglishOtherConstraints = _md.GetEnglishValueFromFreeText(englishOtherConstraint);

                        var otherConstraintAnchor = legalConstraint.otherConstraints[1].MD_RestrictionOther as Anchor_Type;
                        if (otherConstraintAnchor != null)
                        {
                            actualOtherConstraintLink = otherConstraintAnchor.href;
                            actualOtherConstraintLinkText = otherConstraintAnchor.Value;
                        }

                        actualUseConstraint = legalConstraint.useConstraints[0].MD_RestrictionCode.codeListValue;

                        for (int a = 0; a < legalConstraint.otherConstraints.Length; a++)
                        {
                            var access = legalConstraint.otherConstraints[a].MD_RestrictionOther as CharacterString_PropertyType;
                            if (access != null)
                            {
                                if (access.CharacterString == "no restrictions" || access.CharacterString == "norway digital restricted")
                                {
                                    actualOtherConstraintsAccess = access.CharacterString;
                                    break;
                                }
                            }
                        }

                    }
                    else
                    {
                        var useLimit = constraint.MD_Constraints.useLimitation[0] as PT_FreeText_PropertyType;
                        actualUseLimitation = useLimit.CharacterString;
                        actualEnglishUseLimitation = _md.GetEnglishValueFromFreeText(useLimit);
                    }

                }
            }

            Assert.AreEqual(expectedUseLimitations, actualUseLimitation);
            Assert.AreEqual(expectedSecurityConstraints, actualSecurityConstraint);
            Assert.AreEqual(expectedSecurityConstraintsNote, actualSecurityConstraintNote);
            Assert.AreEqual(expectedAccessConstraints, actualAccessConstraint);
            Assert.AreEqual(expectedOtherConstraints, actualOtherConstraint);
            Assert.AreEqual(expectedOtherConstraintsLink, actualOtherConstraintLink);
            Assert.AreEqual(expectedOtherConstraintsLinkText, actualOtherConstraintLinkText);
            Assert.AreEqual(expectedUseConstraints, actualUseConstraint);
            Assert.AreEqual(expectedOtherConstraintsAccess, actualOtherConstraintsAccess);
            Assert.AreEqual(expectedEnglishUseLimitations, actualEnglishUseLimitation);
            Assert.AreEqual(expectedEnglishOtherConstraints, actualEnglishOtherConstraints);
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
        public void ShouldReturnCrossReference()
        {
            string uuid1 = "dddbb667-1303-4ac5-8640-7ec04c0e3918";
            string uuid2 = "595e47d9-d201-479c-a77d-cbc1f573a76b";
            _md.HierarchyLevel = "software";
            MD_DataIdentification_Type identification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;

            identification.aggregationInfo = new MD_AggregateInformation_PropertyType[]
            {
              new MD_AggregateInformation_PropertyType
              {
                  MD_AggregateInformation = new MD_AggregateInformation_Type
                  {
                      aggregateDataSetIdentifier = new MD_Identifier_PropertyType
                      {
                          MD_Identifier = new MD_Identifier_Type
                          {
                              code = new CharacterString_PropertyType { CharacterString = uuid1 }
                          }
                      }
                  }
              },
              new MD_AggregateInformation_PropertyType
              {
                  MD_AggregateInformation = new MD_AggregateInformation_Type
                  {
                      aggregateDataSetIdentifier = new MD_Identifier_PropertyType
                      {
                          MD_Identifier = new MD_Identifier_Type
                          {
                              code = new CharacterString_PropertyType { CharacterString = uuid2 }
                          }
                      }
                  }
              }
            };

            List<string> references = _md.CrossReference;
            Assert.True(references.Contains(uuid1));
            Assert.True(references.Contains(uuid2));
        }

        [Test]
        public void ShouldSetCrossReference()
        {
            string uuid1 = "dddbb667-1303-4ac5-8640-7ec04c0e3918";
            string uuid2 = "595e47d9-d201-479c-a77d-cbc1f573a76b";
            List<string> uuids = new List<string> { uuid1, uuid2 };
            _md.HierarchyLevel = "software";
            _md.GetMetadata().identificationInfo = new MD_Identification_PropertyType[] { new MD_Identification_PropertyType { AbstractMD_Identification = new MD_DataIdentification_Type() } };
            _md.CrossReference = uuids;

            bool foundUuid1 = false;
            bool foundUuid2 = false;

            var identification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;
            var crossReferences = identification.aggregationInfo;
            foreach (MD_AggregateInformation_PropertyType element in crossReferences)
            {
                if (element.MD_AggregateInformation.aggregateDataSetIdentifier.MD_Identifier.code.CharacterString.Equals(uuid1))
                {
                    foundUuid1 = true;
                }
                if (element.MD_AggregateInformation.aggregateDataSetIdentifier.MD_Identifier.code.CharacterString.Equals(uuid2))
                {
                    foundUuid2 = true;
                }
            }

            Assert.True(foundUuid1);
            Assert.True(foundUuid2);
        }

        [Test]
        public void ShouldReturnServiceType()
        {
            string identifier = "view";
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            Assert.AreEqual(identifier, metadata.ServiceType);
        }

        [Test]
        public void ShouldUpdateServiceType()
        {
            string expectedServiceType = "download";
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            metadata.ServiceType = expectedServiceType;
            Assert.AreEqual(expectedServiceType, metadata.ServiceType);
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
        public void ShouldUpdateSpecificUsageService()
        {
            string expected = "THis is the specific usage details.";
            string expectedEnglish = "THis is the specific usage details english.";
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            metadata.SpecificUsage = expected;
            metadata.EnglishSpecificUsage = expectedEnglish;

            string actual = metadata.GetMetadata().identificationInfo[0].AbstractMD_Identification.resourceSpecificUsage[0].MD_Usage.specificUsage.CharacterString;
            var english = metadata.GetMetadata().identificationInfo[0].AbstractMD_Identification.resourceSpecificUsage[0].MD_Usage.specificUsage as PT_FreeText_PropertyType;
            string actualEnglish = english.PT_FreeText.textGroup[0].LocalisedCharacterString.Value;
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expectedEnglish, actualEnglish);
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

        [Test]
        public void ShouldReturnNullForEnglishSpecificUsageWhenSpecificUsageIsRegularCharacterStringObject()
        {
            string specificUsage = _md.EnglishSpecificUsage;
            Assert.IsNull(specificUsage);
        }

        [Test]
        public void ShouldReturnEnglishSpecificUsageWhenSpecificUsageIsLocalized()
        {
            string expectedEnglishSpecificUsage = "This is english.";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification = new MD_DataIdentification_Type
            {
                resourceSpecificUsage = new MD_Usage_PropertyType[]
                {
                  new MD_Usage_PropertyType
                  {
                   MD_Usage = new MD_Usage_Type
                   {
                        specificUsage = new PT_FreeText_PropertyType
                        {
                            CharacterString = "Dette er norsk",
                            PT_FreeText = new PT_FreeText_Type
                            {
                                textGroup = new LocalisedCharacterString_PropertyType[]
                                {
                                    new LocalisedCharacterString_PropertyType
                                    {
                                    LocalisedCharacterString = new LocalisedCharacterString_Type
                                        {
                                        locale = SimpleMetadata.LOCALE_LINK_ENG,
                                        Value = expectedEnglishSpecificUsage
                                        }
                                    }
                                }
                            }
                        }
                    }
                  }
                }
            };

            Assert.AreEqual(expectedEnglishSpecificUsage, _md.EnglishSpecificUsage);
        }

        [Test]
        public void ShouldUpdateEnglishEnglishSpecificUsageWithoutDestroyingExistingLocalEnglishSpecificUsage()
        {
            string expectedNorwegianSpecificUsage = "Dette er bruksområder.";
            string expectedEnglishSpecificUsage = "This is english.";

            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification = new MD_DataIdentification_Type { resourceSpecificUsage = new MD_Usage_PropertyType[] { new MD_Usage_PropertyType { MD_Usage = new MD_Usage_Type { specificUsage = new CharacterString_PropertyType { CharacterString = expectedNorwegianSpecificUsage } } } } };

            _md.EnglishSpecificUsage = expectedEnglishSpecificUsage;

            var dataIdentification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;
            var specificUsageElement = dataIdentification.resourceSpecificUsage[0].MD_Usage.specificUsage;
            PT_FreeText_PropertyType freeTextElement = specificUsageElement as PT_FreeText_PropertyType;

            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual(expectedNorwegianSpecificUsage, freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishSpecificUsage, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
        }

        [Test]
        public void ShouldUpdateSpecificUsageWithoutDestroyingEnglishSpecificUsage()
        {
            string expectedNorwegianSpecificUsage = "Oppdatert norsk bruksområde";
            string expectedEnglishSpecificUsage = "This is english.";
            _md.GetMetadata().identificationInfo[0].AbstractMD_Identification = new MD_DataIdentification_Type
            {
               resourceSpecificUsage = new MD_Usage_PropertyType[] 
               {
                   new MD_Usage_PropertyType
                   {
                       MD_Usage = new MD_Usage_Type
                       {
                       specificUsage  = new PT_FreeText_PropertyType
                            {
                                CharacterString = "Dette er norsk",
                                PT_FreeText = new PT_FreeText_Type
                                {
                                    textGroup = new LocalisedCharacterString_PropertyType[] {
                                        new LocalisedCharacterString_PropertyType {
                                            LocalisedCharacterString = new LocalisedCharacterString_Type {
                                                locale = SimpleMetadata.LOCALE_LINK_ENG,
                                                Value = expectedEnglishSpecificUsage
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
               }
            };

            _md.SpecificUsage = expectedNorwegianSpecificUsage;

            var dataIdentification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as MD_DataIdentification_Type;
            var specificUsageElement = dataIdentification.resourceSpecificUsage[0].MD_Usage.specificUsage;

            PT_FreeText_PropertyType freeTextElement = specificUsageElement as PT_FreeText_PropertyType;

            Assert.IsNotNull(freeTextElement, "PT_FreeText_PropertyType does not exist");
            Assert.AreEqual(expectedNorwegianSpecificUsage, freeTextElement.CharacterString);
            Assert.AreEqual(SimpleMetadata.LOCALE_LINK_ENG, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.locale);
            Assert.AreEqual(expectedEnglishSpecificUsage, freeTextElement.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
        }

        [Test]
        public void ShouldUpdateParentIdentifier()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            string identifier = "testIdentifier";
            metadata.ParentIdentifier = identifier;
            Assert.AreEqual(identifier, metadata.GetMetadata().parentIdentifier.CharacterString);
        }

        [Test]
        public void ShouldReturnParentIdentifier()
        {
            string identifier = "testIdentifier";
            MD_Metadata_Type metadata = new MD_Metadata_Type();
            metadata.parentIdentifier = new CharacterString_PropertyType{ CharacterString = identifier};

            SimpleMetadata simpleMetadata = new SimpleMetadata(metadata);

            Assert.AreEqual(identifier, simpleMetadata.ParentIdentifier);
        }

        [Test]
        public void ShouldReturnValidTimePeriod()
        {
            MD_Metadata_Type metadata = new MD_Metadata_Type();
            metadata.hierarchyLevel = new MD_ScopeCode_PropertyType[]
            { 
                new MD_ScopeCode_PropertyType()
                { 
                    MD_ScopeCode = new CodeListValue_Type()
                    { 
                        codeListValue = "service"
                    }
                } 

            };

            metadata.identificationInfo = new MD_Identification_PropertyType[] 
            { 
             new MD_Identification_PropertyType{
              AbstractMD_Identification =  new SV_ServiceIdentification_Type()
              {
                extent = new EX_Extent_PropertyType[]
                {
                    new EX_Extent_PropertyType
                    {
                        EX_Extent = new EX_Extent_Type()
                        {
                         temporalElement= new EX_TemporalExtent_PropertyType[]
                         {
                            new EX_TemporalExtent_PropertyType()
                            {
                                EX_TemporalExtent = new EX_TemporalExtent_Type()
                                {
                                   extent = new TM_Primitive_PropertyType()
                                   {
                                       AbstractTimePrimitive =  new TimePeriodType()
                                       {
                                           Item= new TimePositionType()
                                           {
                                               Value = "2010-01-01T12:00:00"
                                           },
                                          Item1= new TimePositionType()
                                          {
                                              Value = "2018-01-01T12:00:00"
                                          }
                                        }
                                    }
                                }
                            }
                         }
                        }
                    }
                }
              }
             }
            };

            SimpleMetadata simpleMetadata = new SimpleMetadata(metadata);
            Assert.NotNull(simpleMetadata.ValidTimePeriod);
            Assert.AreEqual("2010-01-01T12:00:00", simpleMetadata.ValidTimePeriod.ValidFrom);
            Assert.AreEqual("2018-01-01T12:00:00", simpleMetadata.ValidTimePeriod.ValidTo);


        }

        [Test]
        public void ShouldUpdateValidTimePeriodWhenExtentNotSet()
        {
            string expectedValidFrom = "2010-01-01T12:00:00";
            string expectedValidTo = "2018-01-01T12:00:00";
            SimpleMetadata simpleMetadata = SimpleMetadata.CreateService();
            simpleMetadata.ValidTimePeriod = new SimpleValidTimePeriod
            {
                ValidFrom = expectedValidFrom,
                ValidTo = expectedValidTo
            };

            SV_ServiceIdentification_Type serviceIdentificationType = simpleMetadata.GetMetadata().identificationInfo[0].AbstractMD_Identification as SV_ServiceIdentification_Type;
            TimePeriodType timePeriodType = serviceIdentificationType.extent[0].EX_Extent.temporalElement[0].EX_TemporalExtent.extent.AbstractTimePrimitive as TimePeriodType;
            TimePositionType actualTimePositionType = timePeriodType.Item as TimePositionType;
            TimePositionType actualTimePositionType1 = timePeriodType.Item1 as TimePositionType;
            string actualValidFrom = actualTimePositionType.Value;
            string actualValidTo = actualTimePositionType1.Value;

            Assert.AreEqual(expectedValidFrom, actualValidFrom);
            Assert.AreEqual(expectedValidTo, actualValidTo);
            
        }

        [Test]
        public void ShouldUpdateValidTimePeriodWithoutDestroyingOtherInformationInExtentElement()
        {
            SimpleMetadata simpleMetadata = SimpleMetadata.CreateService(); 
            
            SV_ServiceIdentification_Type serviceIdentificationType = simpleMetadata.GetMetadata().identificationInfo[0].AbstractMD_Identification as SV_ServiceIdentification_Type;

            serviceIdentificationType.extent = new EX_Extent_PropertyType[]
            {
                new EX_Extent_PropertyType()
                {
                    EX_Extent = new EX_Extent_Type()
                    {
                        description = new CharacterString_PropertyType()
                        {
                            CharacterString = "Testing"
                        }                                      
                    }
                }
            };

            EX_Extent_Type extent = serviceIdentificationType.extent[0].EX_Extent;

            extent.geographicElement = new EX_GeographicExtent_PropertyType[] { };
            extent.verticalElement = new EX_VerticalExtent_PropertyType[] { };

            simpleMetadata.ValidTimePeriod = new SimpleValidTimePeriod
            {
                ValidFrom = "2010-01-01T12:00:00",
                ValidTo = "2018-01-01T12:00:00"
            };

            var identification = simpleMetadata.GetMetadata().identificationInfo[0].AbstractMD_Identification as SV_ServiceIdentification_Type;
            var actualExtent = identification.extent[0].EX_Extent;
            TimePeriodType timePeriodType = actualExtent.temporalElement[0].EX_TemporalExtent.extent.AbstractTimePrimitive as TimePeriodType;

            TimePositionType actualTimePositionType = timePeriodType.Item as TimePositionType;
            TimePositionType actualTimePositionType1 = timePeriodType.Item1 as TimePositionType;
            string actualValidFrom = actualTimePositionType.Value;
            string actualValidTo = actualTimePositionType1.Value;

            Assert.AreEqual("2010-01-01T12:00:00", actualValidFrom);
            Assert.AreEqual("2018-01-01T12:00:00", actualValidTo);
            
            Assert.NotNull(actualExtent.description);
            Assert.NotNull(actualExtent.geographicElement);
            Assert.NotNull(actualExtent.verticalElement);            

        }

        [Test]
        public void ShouldReturnNullWhenValidTimePeriodNotSet() 
        {
            SimpleMetadata simpleMetadata = SimpleMetadata.CreateService();

            Assert.Null(simpleMetadata.ValidTimePeriod.ValidFrom);
            Assert.Null(simpleMetadata.ValidTimePeriod.ValidTo);
        }

        [Test]
        public void ShouldReturnNullWhenProductSpecificationOtherNotSet()
        {
            SimpleMetadata simpleMetadata = SimpleMetadata.CreateDataset();

            Assert.Null(simpleMetadata.ProductSpecificationOther.Name);
            Assert.Null(simpleMetadata.ProductSpecificationOther.URL);
        }

        [Test]
        public void ShouldReturnProductSpecificationOther()
        {
            string expectedURL = "http://example.com";
            string expectedName = "navn";
            _md.GetMetadata().metadataExtensionInfo = new MD_MetadataExtensionInformation_PropertyType[] {
                new MD_MetadataExtensionInformation_PropertyType {
                MD_MetadataExtensionInformation = new MD_MetadataExtensionInformation_Type{
                    extensionOnLineResource = new CI_OnlineResource_PropertyType{ 
                        CI_OnlineResource = new CI_OnlineResource_Type{ 
                            name = new CharacterString_PropertyType{ CharacterString = expectedName},
                            linkage = new URL_PropertyType{ URL = expectedURL},
                            applicationProfile = new CharacterString_PropertyType{ CharacterString = "annen produktspesifikasjon"}
                        }
                    }
                }
              }
            };

            var specsOther = _md.ProductSpecificationOther;

            Assert.NotNull(specsOther);
            Assert.AreEqual(expectedURL, specsOther.URL);
            Assert.AreEqual(expectedName, specsOther.Name);

        }

        [Test]
        public void ShouldUpdateProductSpecificationOther()
        {
            string expectedURL = "http://example.com";
            string expectedName = "navn";
            SimpleMetadata simpleMetadata = SimpleMetadata.CreateService();
            simpleMetadata.ProductSpecificationOther = new SimpleOnlineResource
            {
                Name = expectedName,
                URL = expectedURL
            };

            var mdExt = simpleMetadata.GetMetadata().metadataExtensionInfo[0].MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource;

            string actualURL = mdExt.linkage.URL;
            string actualName = mdExt.name.CharacterString;

            Assert.AreEqual(expectedURL, actualURL);
            Assert.AreEqual(expectedName, actualName);

        }


        [Test]
        public void ShouldDeleteProductSpecificationOtherWhenSetNullAndPreserveOtherResources()
        {
            string url = "http://example.com";
            string name = "navn";
            SimpleMetadata simpleMetadata = SimpleMetadata.CreateService();

            simpleMetadata.ProductSpecificationUrl = url;

            simpleMetadata.ProductSpecificationOther = new SimpleOnlineResource
            {
                Name = name,
                URL = url
            };

            Assert.IsNotNull(simpleMetadata.ProductSpecificationOther.Name);
            Assert.IsNotNull(simpleMetadata.ProductSpecificationOther.URL);

            simpleMetadata.ProductPageUrl = url;

            simpleMetadata.ProductSpecificationOther = new SimpleOnlineResource
            {
                Name = null,
                URL = null
            };

            Assert.IsNull(simpleMetadata.ProductSpecificationOther.Name);
            Assert.IsNull(simpleMetadata.ProductSpecificationOther.URL);

            Assert.IsNotNull(simpleMetadata.ProductSpecificationUrl);
            Assert.IsNotNull(simpleMetadata.ProductPageUrl);

        }

        [Test]
        public void ShouldReturnHelpUrl()
        {
            string expectedURL = "http://example.com";
            _md.GetMetadata().metadataExtensionInfo = new MD_MetadataExtensionInformation_PropertyType[] {
                new MD_MetadataExtensionInformation_PropertyType {
                MD_MetadataExtensionInformation = new MD_MetadataExtensionInformation_Type{
                    extensionOnLineResource = new CI_OnlineResource_PropertyType{
                        CI_OnlineResource = new CI_OnlineResource_Type{
                            linkage = new URL_PropertyType{ URL = expectedURL},
                            applicationProfile = new CharacterString_PropertyType{ CharacterString = "hjelp"}
                        }
                    }
                }
              }
            };

            var help = _md.HelpUrl;

            Assert.NotNull(help);
            Assert.AreEqual(expectedURL, help);

        }

        [Test]
        public void ShouldUpdateHelpUrl()
        {
            string expectedURL = "http://example.com";
            SimpleMetadata simpleMetadata = SimpleMetadata.CreateDataset();
            simpleMetadata.HelpUrl = expectedURL;

            var md = simpleMetadata.GetMetadata().metadataExtensionInfo[0].MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource;

            string actualURL = md.linkage.URL;

            Assert.AreEqual(expectedURL, actualURL);

        }

        [Test]
        public void ShouldUpdateCoverageUrls()
        {
            string expectedCovarageGridMapURL = "TYPE:GEONORGE-WMS@PATH:https://wms.geonorge.no/skwms/wms.geonorge_dekningskart?@LAYER:dsb_brannstasjon";
            string expectedCovarageMapURL = "TYPE:GEONORGE-WMS@PATH:https://wms.geonorge.no/wms?@LAYER:dsb_brannstasjon";
            SimpleMetadata simpleMetadata = SimpleMetadata.CreateDataset();
            simpleMetadata.CoverageGridUrl = expectedCovarageGridMapURL;
            simpleMetadata.CoverageUrl = expectedCovarageMapURL;

            var gridMap = simpleMetadata.GetMetadata().metadataExtensionInfo[0].MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource;
            var map = simpleMetadata.GetMetadata().metadataExtensionInfo[1].MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource;

            string actualCovarageGridMapURL = gridMap.linkage.URL;
            string actualCovarageMapURL = map.linkage.URL;

            Assert.AreEqual(expectedCovarageGridMapURL, actualCovarageGridMapURL);
            Assert.AreEqual(expectedCovarageMapURL, actualCovarageMapURL);

        }


        [Test]
        public void ShouldReturnNullWhenAccessPropertiesIsNotDefined()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            Assert.IsNull(metadata.AccessProperties);
        }

        [Test]
        public void ShouldUpdateAccessProperties()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            string orderingInstructions = "Norge digitalt tjenesteerklæring A";
            metadata.AccessProperties = new SimpleAccessProperties { OrderingInstructions = orderingInstructions } ;
            Assert.AreEqual(orderingInstructions, metadata.AccessProperties.OrderingInstructions);
        }

        [Test]
        public void ShouldSetAndGetApplicationSchema()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            string applicationSchema = "application schema";
            metadata.ApplicationSchema = applicationSchema;

            Assert.AreEqual(applicationSchema, metadata.ApplicationSchema);
        }

        [Test]
        public void ShouldSetDefaultValuesInApplicationSchemaInformation()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            metadata.ApplicationSchema = "application schema";

            Assert.AreEqual(DateTime.Now.ToString("yyyy-MM-dd"), metadata.GetMetadata().applicationSchemaInfo[0].MD_ApplicationSchemaInformation.name.CI_Citation.date[0].CI_Date.date.Item);
            Assert.AreEqual("UML", metadata.GetMetadata().applicationSchemaInfo[0].MD_ApplicationSchemaInformation.schemaLanguage.CharacterString);
            Assert.AreEqual("OCL", metadata.GetMetadata().applicationSchemaInfo[0].MD_ApplicationSchemaInformation.constraintLanguage.CharacterString);
        }

        [Test]
        public void ShouldReturnNullIfApplicationSchemaIsNotSet()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateService();
            Assert.IsNull(metadata.ApplicationSchema);
            Assert.IsNull(metadata.GetMetadata().applicationSchemaInfo);
        }

        [Test]
        public void ShouldRemoveApplicationSchemaIfSetToNull()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            metadata.ApplicationSchema = "application schema";

            metadata.ApplicationSchema = null;

            Assert.IsNull(metadata.GetMetadata().applicationSchemaInfo);
        }

        [Test]
        public void ShouldRemoveApplicationSchemaIfSetToEmptyString()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            metadata.ApplicationSchema = "application schema";

            metadata.ApplicationSchema = "";

            Assert.IsNull(metadata.GetMetadata().applicationSchemaInfo);
        }


        [Test]
        public void ShouldReturnDistributionsFormats()
        {
            string expectedName = "SOSI";
            string expectedVersion = "4.5";
            string expectedOrganization = "kartverket";
            string expectedURL = "http://example.com";
            string expectedProtocol = "www";
            string expectedResourceName = "navn";
            string expectedUnitsOfDistribution = "unit1";
            string expectedEnglishUnitsOfDistribution = "unit1eng";
            _md.GetMetadata().distributionInfo = new MD_Distribution_PropertyType
            {
                MD_Distribution = new MD_Distribution_Type
                {
                    distributionFormat = new[]
                    {
                        new MD_Format_PropertyType
                        {
                            MD_Format = new MD_Format_Type
                            {
                                name = new CharacterString_PropertyType { CharacterString = expectedName },
                                version = new CharacterString_PropertyType { CharacterString = expectedVersion },
                                formatDistributor = new MD_Distributor_PropertyType[]
                                {
                                   new MD_Distributor_PropertyType
                                   {
                                       MD_Distributor = new MD_Distributor_Type
                                       {
                                           distributorContact = new CI_ResponsibleParty_PropertyType
                                           {
                                               CI_ResponsibleParty = new CI_ResponsibleParty_Type
                                               {
                                                   organisationName = new CharacterString_PropertyType { CharacterString = expectedOrganization },
                                                   role = new CI_RoleCode_PropertyType
                                                   {
                                                       CI_RoleCode = new CodeListValue_Type { codeListValue = "distributor" }
                                                   }
                                               }
                                           },
                                           distributorTransferOptions = new MD_DigitalTransferOptions_PropertyType[]
                                           {
                                               new MD_DigitalTransferOptions_PropertyType
                                               {
                                                   MD_DigitalTransferOptions = new MD_DigitalTransferOptions_Type
                                                   {
                                                       unitsOfDistribution = new PT_FreeText_PropertyType
                                                        {
                                                            CharacterString = expectedUnitsOfDistribution,
                                                            PT_FreeText = new PT_FreeText_Type
                                                            {
                                                                textGroup = new LocalisedCharacterString_PropertyType[]
                                                                {
                                                                    new LocalisedCharacterString_PropertyType
                                                                    {
                                                                    LocalisedCharacterString = new LocalisedCharacterString_Type
                                                                        {
                                                                        locale = SimpleMetadata.LOCALE_LINK_ENG,
                                                                        Value = expectedEnglishUnitsOfDistribution
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        },
                                                       onLine = new CI_OnlineResource_PropertyType[]
                                                       {
                                                           new CI_OnlineResource_PropertyType
                                                           {
                                                               CI_OnlineResource = new CI_OnlineResource_Type
                                                               {
                                                                   protocol = new CharacterString_PropertyType { CharacterString = expectedProtocol },
                                                                   name = new CharacterString_PropertyType { CharacterString = expectedResourceName },
                                                                   linkage = new URL_PropertyType
                                                                   {
                                                                       URL = expectedURL
                                                                   }
                                                               }
                                                           }
                                                       }
                                                   }
                                               }
                                           }
                                       }
                                   }
                                }
                            }
                        }
                    }
                }
            };


            var details = _md.DistributionsFormats;

            Assert.NotNull(details);
            Assert.AreEqual(expectedName, details[0].FormatName);
            Assert.AreEqual(expectedVersion, details[0].FormatVersion);
            Assert.AreEqual(expectedOrganization, details[0].Organization);
            Assert.AreEqual(expectedURL, details[0].URL);
            Assert.AreEqual(expectedProtocol, details[0].Protocol);
            Assert.AreEqual(expectedResourceName, details[0].Name);
            Assert.AreEqual(expectedUnitsOfDistribution, details[0].UnitsOfDistribution);
            Assert.AreEqual(expectedEnglishUnitsOfDistribution, details[0].EnglishUnitsOfDistribution);

        }

        [Test]
        public void ShouldUpdateDistributionsFormats()
        {
            string expectedName = "SOSI";
            string expectedVersion = "4.5";
            string expectedOrganization = "kartverket";
            string expectedURL = "http://example.com";
            string expectedProtocol = "www";
            string expectedResourceName = "navn";
            string expectedUnitsOfDistribution = "unit1";
            string expectedEnglishUnitsOfDistribution = "unit1eng";

            var formats = new List<SimpleDistribution>();
            var format = new SimpleDistribution { FormatName = expectedName, FormatVersion = expectedVersion, Organization = expectedOrganization, Protocol = expectedProtocol, URL = expectedURL, Name = expectedResourceName, UnitsOfDistribution = expectedUnitsOfDistribution, EnglishUnitsOfDistribution = expectedEnglishUnitsOfDistribution };
            formats.Add(format);
            _md.DistributionsFormats = formats;

            var mdFormat = _md.GetMetadata().distributionInfo.MD_Distribution.distributionFormat[0].MD_Format;
            var actualEnglishUnitsOfDistribution = mdFormat.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.unitsOfDistribution as PT_FreeText_PropertyType;
            Assert.AreEqual(expectedName, mdFormat.name.CharacterString);
            Assert.AreEqual(expectedVersion, mdFormat.version.CharacterString);
            Assert.AreEqual(expectedOrganization, mdFormat.formatDistributor[0].MD_Distributor.distributorContact.CI_ResponsibleParty.organisationName.CharacterString);
            Assert.AreEqual(expectedURL, mdFormat.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource.linkage.URL);
            Assert.AreEqual(expectedProtocol, mdFormat.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource.protocol.CharacterString);
            Assert.AreEqual(expectedResourceName, mdFormat.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource.name.CharacterString);
            Assert.AreEqual(expectedUnitsOfDistribution, mdFormat.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.unitsOfDistribution.CharacterString);
            Assert.AreEqual(expectedEnglishUnitsOfDistribution, actualEnglishUnitsOfDistribution.PT_FreeText.textGroup[0].LocalisedCharacterString.Value);
        }

        [Test]
        public void ShouldRemoveUnnecessaryElements()
        {
            string xml = File.ReadAllText("xml/unnecessary.xml");
            var metadata = new SimpleMetadata(SerializeUtil.DeserializeFromString<MD_Metadata_Type>(xml));
            metadata.RemoveUnnecessaryElements();
            MD_Metadata_Type metadataUpdated = metadata.GetMetadata();
            var citation = metadataUpdated.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation;
            Assert.IsNull(citation.citedResponsibleParty);
            Assert.IsNull(citation.edition);
            Assert.IsNull(citation.presentationForm);
        }

        [Test]
        public void ShouldCheckProtocolSpatialDataServices()
        {
            Assert.True(SimpleMetadata.IsAccessPoint("W3C:REST"));
            Assert.False(SimpleMetadata.IsAccessPoint("OGC:WFS"));
            Assert.True(SimpleMetadata.IsNetworkService("OGC:WFS"));
            Assert.False(SimpleMetadata.IsNetworkService("W3C:REST"));
        }

        [Test]
        public void ShouldReturnSimpleOperationsForService()
        {
            _md.GetMetadata().identificationInfo = new MD_Identification_PropertyType[]
                {
                    new MD_Identification_PropertyType
                    {
                        AbstractMD_Identification = new SV_ServiceIdentification_Type
                        {
                            containsOperations = new SV_OperationMetadata_PropertyType[]{ new SV_OperationMetadata_PropertyType { SV_OperationMetadata = new SV_OperationMetadata_Type
                            {
                                operationName = new CharacterString_PropertyType { CharacterString = "Operation1" },
                                operationDescription = new CharacterString_PropertyType{ CharacterString = "Description1" },
                                DCP = new DCPList_PropertyType[]{ new DCPList_PropertyType { DCPList = new CodeListValue_Type { codeListValue = "Platform1" } } },
                                connectPoint = new CI_OnlineResource_PropertyType[]{ new CI_OnlineResource_PropertyType { CI_OnlineResource = new CI_OnlineResource_Type { linkage = new URL_PropertyType { URL = "http://service1.geonorge.no" } } } }
                            } } }
                        }
                    }
                };
            _md.GetMetadata().hierarchyLevel = new MD_ScopeCode_PropertyType[] { new MD_ScopeCode_PropertyType { MD_ScopeCode = new CodeListValue_Type { codeListValue = "service" } } };

            List<SimpleOperation> operations = _md.ContainOperations;

            Assert.IsNotNull(operations);

            Assert.AreEqual("Operation1", operations[0].Name);
            Assert.AreEqual("Description1", operations[0].Description);
            Assert.AreEqual("Platform1", operations[0].Platform);
            Assert.AreEqual("http://service1.geonorge.no", operations[0].URL);
        }
        [Test]
        public void ShouldUpdateSimpleOperationsForService()
        {
            _md.GetMetadata().hierarchyLevel = new MD_ScopeCode_PropertyType[] { new MD_ScopeCode_PropertyType { MD_ScopeCode = new CodeListValue_Type { codeListValue = "service" } } };
            _md.GetMetadata().identificationInfo = new MD_Identification_PropertyType[]
                {
                    new MD_Identification_PropertyType
                    {
                        AbstractMD_Identification = new SV_ServiceIdentification_Type()
                    }
                };

            List<SimpleOperation> operations = new List<SimpleOperation>();
            operations.Add(new SimpleOperation
            {
                Name = "Name1",
                Description = "Description1",
                Platform = "Platform1",
                URL = "www.example.com"
            });

            _md.ContainOperations = operations;

            var identification = _md.GetMetadata().identificationInfo[0].AbstractMD_Identification as SV_ServiceIdentification_Type;
            var operation = identification.containsOperations[0].SV_OperationMetadata;

            Assert.AreEqual("Name1", operation.operationName.CharacterString);
            Assert.AreEqual("Description1", operation.operationDescription.CharacterString);
            Assert.AreEqual("Platform1", operation.DCP[0].DCPList.codeListValue);
            Assert.AreEqual("www.example.com", operation.connectPoint[0].CI_OnlineResource.linkage.URL);
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

        private CharacterString_PropertyType toCharString(object input)
        {

            Anchor_Type titleObject = input as Anchor_Type;
            PT_FreeText_PropertyType freeText = input as PT_FreeText_PropertyType;
            if (titleObject != null)
                return new CharacterString_PropertyType { CharacterString = titleObject.Value };
            else if (freeText != null)
                return new CharacterString_PropertyType { CharacterString = freeText.CharacterString};
            else
                return new CharacterString_PropertyType { CharacterString = input.ToString() };

        }
    }
}
