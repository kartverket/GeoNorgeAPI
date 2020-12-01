using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using www.opengis.net;

namespace GeoNorgeAPI.Tests
{
    class MetadataExample
    {
        public static MD_Metadata_Type CreateMetadataExample()
        {
            MD_Metadata_Type m = new MD_Metadata_Type();
            m.fileIdentifier = CharString("12345-67890-aabbcc-ddeeff-ggffhhjj");
            m.language = new Language_PropertyType { item = new LanguageCode_PropertyType { LanguageCode = new CodeListValue_Type { codeListValue = "nor", codeList = "http://www.loc.gov/standards/iso639-2/", Value = "Norsk" } } };
            m.metadataStandardName = CharString("ISO19139");
            m.metadataStandardVersion = CharString("1.0");
            m.hierarchyLevel = new[] { new MD_ScopeCode_PropertyType
                {
                    MD_ScopeCode = new CodeListValue_Type
                        {
                            codeList = "http://www.isotc211.org/2005/resources/codeList.xml#MD_ScopeCode",
                            codeListValue = "dataset"
                        }
                } 
            };
            m.hierarchyLevelName = new[] { new CharacterString_PropertyType { CharacterString = "historical" } };

            var responsibleParty = new CI_ResponsibleParty_Type()
            {
                individualName = CharString("Hans Hansen"),
                role = new CI_RoleCode_PropertyType()
                {
                    CI_RoleCode = new CodeListValue_Type
                    {
                        codeList = "http://www.isotc211.org/2005/resources/codeList.xml#CI_RoleCode",
                        codeListValue = "resourceProvider"
                    }
                },
                organisationName = CharString("Statens Kartverk"),
                contactInfo = new CI_Contact_PropertyType()
                {
                    CI_Contact = new CI_Contact_Type()
                    {
                        address = new CI_Address_PropertyType()
                        {
                            CI_Address = new CI_Address_Type()
                            {
                                electronicMailAddress = new[] { CharString("post@kartverket.no") }
                            }
                        }
                    }
                }
            };

            m.contact = new CI_ResponsibleParty_PropertyType[] { new CI_ResponsibleParty_PropertyType()
                {
                    CI_ResponsibleParty = responsibleParty
                }
            };

            m.dateStamp = new Date_PropertyType() { Item = DateTime.Now };

            var mdDataIdentificationType = new MD_DataIdentification_Type();
            mdDataIdentificationType.pointOfContact = new[] {
                new CI_ResponsibleParty_PropertyType { CI_ResponsibleParty = responsibleParty } 
            };
            mdDataIdentificationType.citation = new CI_Citation_PropertyType
            {
                CI_Citation = new CI_Citation_Type
                {
                    title = new CI_Citation_Title { item = CharString("Eksempeldatasettet sin tittel.") },
                    date = new[] { 
                        new CI_Date_PropertyType {
                            CI_Date = new CI_Date_Type { 
                                date = new Date_PropertyType {
                                    Item = DateTime.Now.Date.ToString("yyyy-MM-dd")
                                },
                                dateType = new CI_DateTypeCode_PropertyType { CI_DateTypeCode = new CodeListValue_Type
                                    {
                                        codeList = "http://www.isotc211.org/2005/resources/codeList.xml#CI_DateTypeCode",
                                        codeListValue = "creation"
                                    } 
                                }
                            }
                        }  
                    },
                    alternateTitle = new[] { CharString("Dette er en den alternative tittelen til datasettet som tilbys.") },
                    identifier = new[] { new MD_Identifier_PropertyType
                        {
                            MD_Identifier = new MD_Identifier_Type
                                {
                                    code = new Anchor_PropertyType{  anchor = new CharacterString_PropertyType{ CharacterString ="12345-abcdef-67890-ghijkl"} }
                                }
                        } 
                    }
                }
            };
            mdDataIdentificationType.@abstract = CharString("Dette datasettet inneholder ditt og datt. Kan brukes til dette, men må ikke brukes til andre ting som for eksempel dette.");
            mdDataIdentificationType.purpose = CharString("Dette er formålet.");
            mdDataIdentificationType.resourceSpecificUsage = new[]
                {
                    new MD_Usage_PropertyType() { 
                        MD_Usage = new MD_Usage_Type()
                        {
                            specificUsage = CharString("Skal bare brukes sånn og sånn"),
                            usageDateTime = new DateTime_PropertyType() { DateTime = DateTime.Now },
                            userContactInfo = new []
                                {
                                    new CI_ResponsibleParty_PropertyType
                                        {
                                            CI_ResponsibleParty = responsibleParty
                                        }
                                },
                            userDeterminedLimitations = CharString("Her kommer det begrensninger på bruk av dataene.")
                        } 
                    }
                };
            mdDataIdentificationType.spatialResolution = new MD_Resolution_PropertyType[] { new MD_Resolution_PropertyType()
                {
                    MD_Resolution = new MD_Resolution_Type()
                        {
                            Item = new MD_RepresentativeFraction_PropertyType()
                                {
                                    MD_RepresentativeFraction = new MD_RepresentativeFraction_Type()
                                        {
                                            denominator = new Integer_PropertyType() { Integer = "1000" }
                                        }
                                }
                        }
                } 
            };
            mdDataIdentificationType.language = new LanguageCode_PropertyType[] { new LanguageCode_PropertyType { LanguageCode = new CodeListValue_Type { codeList = "http://www.loc.gov/standards/iso639-2/", codeListValue = "nor", Value = "Norsk" } } };
            mdDataIdentificationType.descriptiveKeywords = new[] { 
                new MD_Keywords_PropertyType {
                    MD_Keywords = new MD_Keywords_Type {
                        keyword = new MD_Keyword[] 
                        { new MD_Keyword
                           { keyword =  new PT_FreeText_PropertyType 
                                { 
                                    CharacterString = "Adresser",
                                    PT_FreeText = new PT_FreeText_Type {
                                            id = "ENG",
                                            textGroup = new LocalisedCharacterString_PropertyType[] 
                                            { 
                                                new LocalisedCharacterString_PropertyType {
                                                    LocalisedCharacterString = new LocalisedCharacterString_Type 
                                                    { 
                                                    locale = "#ENG",
                                                    Value = "Addresses"
                                                    }
                                                }                                                         
                                            }
                                    }
                                }                            
                            }
                        },
                        thesaurusName = new CI_Citation_PropertyType
                            {
                                CI_Citation = new CI_Citation_Type
                                    {
                                        title = new CI_Citation_Title{ item =  new Anchor_Type{ Value = "GEMET - INSPIRE themes, version 1.0", href = "https://register.geonorge.no/subregister/metadata-kodelister/kartverket/inspiretema" } },

                                        date = new [] { new CI_Date_PropertyType
                                            {
                                                CI_Date = new CI_Date_Type
                                                    {
                                                        date = new Date_PropertyType { Item = DateTime.Now },
                                                        dateType = new CI_DateTypeCode_PropertyType
                                                            {
                                                                CI_DateTypeCode = new CodeListValue_Type()
                                                                    {
                                                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/Codelist/ML_gmxCodelists.xml#CI_DateTypeCode",
                                                                        codeListValue = "publication"
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
            mdDataIdentificationType.resourceConstraints = new[]
                {
                    new MD_Constraints_PropertyType
                        {
                            MD_Constraints = new MD_Constraints_Type
                            {

                                 useLimitation = new [] { new PT_FreeText_PropertyType
                                 {
                                    CharacterString = "Gratis å benytte til alle formål.",
                                    PT_FreeText = new PT_FreeText_Type
                                    {
                                            id = "ENG",
                                            textGroup = new LocalisedCharacterString_PropertyType[]
                                            {
                                                new LocalisedCharacterString_PropertyType {
                                                    LocalisedCharacterString = new LocalisedCharacterString_Type
                                                    {
                                                    locale = "#ENG",
                                                    Value = "Free of charge"
                                                    }
                                                }
                                            }
                                    }
                                  }
                                    
                                }
                            }
                        }, 
                    new MD_Constraints_PropertyType
                        {
                            MD_Constraints = new MD_LegalConstraints_Type
                                {
                                    accessConstraints = new MD_RestrictionCode_PropertyType[] 
                                    { 
                                        new MD_RestrictionCode_PropertyType 
                                        { 
                                            MD_RestrictionCode = new CodeListValue_Type { codeListValue = "otherRestrictions" , codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_RestrictionCode" }
                                        }
                                    }
                                    ,
                                     otherConstraints = new MD_RestrictionOther_PropertyType []
                                    {
                                        new MD_RestrictionOther_PropertyType
                                        {
                                            MD_RestrictionOther = new Anchor_Type{href="http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1d",Value="Økonomiske- eller forretningsmessige forhold"}
                                        }
                                    }
                                }
                        },
                        new MD_Constraints_PropertyType
                        {
                            MD_Constraints = new MD_LegalConstraints_Type
                            {
                                    useConstraints = new MD_RestrictionCode_PropertyType[]
                                    {
                                        new MD_RestrictionCode_PropertyType
                                        {
                                            MD_RestrictionCode = new CodeListValue_Type { codeListValue = "otherRestrictions" , codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_RestrictionCode" }
                                        }
                                    },
                                    otherConstraints = new MD_RestrictionOther_PropertyType []
                                    {
                                        new MD_RestrictionOther_PropertyType
                                        {
                                            MD_RestrictionOther = new Anchor_Type{href="https://creativecommons.org/licenses/by/4.0/",Value="Creative Commons BY 4.0 (CC BY 4.0)"}
                                        }
                                    }
                            }
                        },
                        new MD_Constraints_PropertyType
                        {
                            MD_Constraints = new MD_LegalConstraints_Type
                            {
                                    otherConstraints = new MD_RestrictionOther_PropertyType []
                                    {
                                        new MD_RestrictionOther_PropertyType
                                        {
                                            MD_RestrictionOther =
                                            new PT_FreeText_PropertyType
                                                 {
                                                    CharacterString = "Ingen begrensninger på bruk.",
                                                    PT_FreeText = new PT_FreeText_Type
                                                    {
                                                            id = "ENG",
                                                            textGroup = new LocalisedCharacterString_PropertyType[]
                                                            {
                                                                new LocalisedCharacterString_PropertyType {
                                                                    LocalisedCharacterString = new LocalisedCharacterString_Type
                                                                    {
                                                                    locale = "#ENG",
                                                                    Value = "No restrictions"
                                                                    }
                                                                }
                                                            }
                                                    }
                                                  }
                                            }
                                    }
                            },
                        },
                    new MD_Constraints_PropertyType
                        {
                            MD_Constraints = new MD_SecurityConstraints_Type
                                {
                                    userNote = new CharacterString_PropertyType 
                                    {
                                        CharacterString = "Text that describes why it is not freely open"
                                    },
                                    classification = new MD_ClassificationCode_PropertyType
                                        {
                                            MD_ClassificationCode = new CodeListValue_Type
                                                {
                                                    codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ClassificationCode",
                                                    codeListValue = "unclassified"
                                                }
                                        }
                                }
                        }
                };
            mdDataIdentificationType.topicCategory = new[] { new MD_TopicCategoryCode_PropertyType
                {
                    MD_TopicCategoryCode =  MD_TopicCategoryCode_Type.location
                } 
            };
            mdDataIdentificationType.extent = new[]
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
                                                            westBoundLongitude = new Decimal_PropertyType { Decimal = 2 },
                                                            eastBoundLongitude = new Decimal_PropertyType { Decimal = 33 },
                                                            southBoundLatitude = new Decimal_PropertyType { Decimal = 57 },
                                                            northBoundLatitude = new Decimal_PropertyType { Decimal = 72 }
                                                        }
                                                }
                                        },
                                    temporalElement = new EX_TemporalExtent_PropertyType[]
                                    {
                                        new EX_TemporalExtent_PropertyType
                                        {
                                            EX_TemporalExtent = new EX_TemporalExtent_Type
                                            {
                                                extent = new TM_Primitive_PropertyType
                                                {
                                                      AbstractTimePrimitive = new TimePeriodType()
                                                      {
                                                         id="id_1",

                                                            Item = new TimePositionType()
                                                            {
                                                                Value = "0001-01-01"
                                                            },
                                                            Item1 = new TimePositionType()
                                                            {
                                                                Value = "2030-01-01",
                                                                indeterminatePosition = TimeIndeterminateValueType.now
                                                            }
                                                      }        
                                                }
                                            }
                                        }
                                    }

                                }
                        }
                };

            mdDataIdentificationType.supplementalInformation = CharString("Dette er den utfyllende informasjonen.");

            m.identificationInfo = new[] {
                new MD_Identification_PropertyType
                    {
                        AbstractMD_Identification = mdDataIdentificationType
                    }
            };

            m.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier[0].MD_Identifier = new MD_Identifier_Type
            {
                code = new Anchor_PropertyType { anchor = new Anchor_Type { href = "https://registry.gdi-de.org/id/de.nw/inspire-cp-alkis", Value = "inspire-cp-alkis" } }
            };

            m.distributionInfo = new MD_Distribution_PropertyType
            {
                MD_Distribution = new MD_Distribution_Type
                {
                    transferOptions = new[]
                                {
                                    new MD_DigitalTransferOptions_PropertyType
                                        {
                                            MD_DigitalTransferOptions = new MD_DigitalTransferOptions_Type
                                                {
                                                    onLine = new[]
                                                        {
                                                            new CI_OnlineResource_PropertyType
                                                                {
                                                                    CI_OnlineResource = new CI_OnlineResource_Type
                                                                        {
                                                                            linkage = new URL_PropertyType
                                                                                {
                                                                                    URL = "http://www.kartverket.no"
                                                                                }
                                                                        }
                                                                }
                                                        }
                                                }
                                        }
                                },
                    distributionFormat = new[] 
                    {
                        new MD_Format_PropertyType
                        {
                            MD_Format = new MD_Format_Type
                            {
                                name = new CharacterString_PropertyType { CharacterString = "SOSI" },
                                version = new MD_Format_Type_Version {  item = new CharacterString_PropertyType{  CharacterString = "4.5" } },
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
                                                   organisationName = new CharacterString_PropertyType { CharacterString = "Kartverket" },
                                                   role = new CI_RoleCode_PropertyType
                                                   {
                                                       CI_RoleCode = new CodeListValue_Type { codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/ML_gmxCodelists.xml#CI_RoleCode", codeListValue = "distributor" }
                                                   }
                                               }
                                           },
                                           distributorTransferOptions = new MD_DigitalTransferOptions_PropertyType[] 
                                           {
                                               new MD_DigitalTransferOptions_PropertyType
                                               {
                                                   MD_DigitalTransferOptions = new MD_DigitalTransferOptions_Type
                                                   {
                                                       unitsOfDistribution = new CharacterString_PropertyType { CharacterString = "unit1" },
                                                       onLine = new CI_OnlineResource_PropertyType[]
                                                       {
                                                           new CI_OnlineResource_PropertyType
                                                           {
                                                               CI_OnlineResource = new CI_OnlineResource_Type
                                                               {
                                                                   protocol = new CharacterString_PropertyType { CharacterString = "www" },
                                                                   name = new CharacterString_PropertyType { CharacterString = "layer1" },
                                                                   linkage = new URL_PropertyType
                                                                   {
                                                                       URL = "http://www.kartverket.no"
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

            var dqDataQualityType = new DQ_DataQuality_Type();
            dqDataQualityType.scope = new DQ_Scope_PropertyType
            {
                DQ_Scope = new DQ_Scope_Type
                {
                    level = new MD_ScopeCode_PropertyType
                    {
                        MD_ScopeCode = new CodeListValue_Type
                        {
                            codeList = "http://www.isotc211.org/2005/resources/codeList.xml#MD_ScopeCode",
                            codeListValue = "dataset"
                        }
                    }
                }
            };
            dqDataQualityType.lineage = new LI_Lineage_PropertyType
            {
                LI_Lineage = new LI_Lineage_Type
                {
                    statement = CharString("Her kommer det en tekst om prosesshistorien.")
                }
            };
            dqDataQualityType.report = new[]
                {
                    new DQ_Element_PropertyType
                        {
                            AbstractDQ_Element = new DQ_DomainConsistency_Type
                                {
                                    result = new [] {
                                        new DQ_Result_PropertyType
                                        {
                                            AbstractDQ_Result = new DQ_ConformanceResult_Type
                                                {
                                                    specification = new CI_Citation_PropertyType
                                                        {
                                                            CI_Citation = new CI_Citation_Type
                                                                {
                                                                    title = new CI_Citation_Title{ item = CharString("COMMISSION REGULATION (EU) No 1089/2010 of 23 November 2010 implementing Directive 2007/2/EC of the European Parliament and of the Council as regards interoperability of spatial data sets and services") },
                                                                    date = new CI_Date_PropertyType[]
                                                                        {
                                                                            new CI_Date_PropertyType
                                                                                {
                                                                                    CI_Date = new CI_Date_Type
                                                                                        {
                                                                                            date = new Date_PropertyType
                                                                                                {
                                                                                                   Item = "2010-12-08"
                                                                                                },
                                                                                            dateType = new CI_DateTypeCode_PropertyType
                                                                                                {
                                                                                                    CI_DateTypeCode = new CodeListValue_Type
                                                                                                        {
                                                                                                            codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/Codelist/ML_gmxCodelists.xml#CI_DateTypeCode", 
                                                                                                            codeListValue = "publication"
                                                                                                        }
                                                                                                }
                                                                                        }
                                                                                }
                                                                        },
                                                                identifier = new MD_Identifier_PropertyType[]
                                                                {
                                                                    new MD_Identifier_PropertyType
                                                                    {
                                                                        MD_Identifier = new MD_Identifier_Type
                                                                        {
                                                                         authority = new CI_Citation_PropertyType
                                                                            {
                                                                                CI_Citation = new CI_Citation_Type
                                                                                {
                                                                                    title = new CI_Citation_Title
                                                                                    {
                                                                                        item = new CharacterString_PropertyType
                                                                                        {
                                                                                            CharacterString = "inspire"
                                                                                        }
                                                                                    }, date = new CI_Date_PropertyType[]{ new CI_Date_PropertyType() }
                                                                                }
                                                                            },
                                                                            code = new Anchor_PropertyType{ anchor = new CharacterString_PropertyType() }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                    },
                                                    explanation = CharString("See the referenced specification"),
                                                    pass = new Boolean_PropertyType { Boolean = true }
                                                }
                                        }   
                                    }
                                }
                        }
                };

            m.dataQualityInfo = new[] { new DQ_DataQuality_PropertyType { DQ_DataQuality = dqDataQualityType } };

            m.referenceSystemInfo = new MD_ReferenceSystem_PropertyType[] 
            {
                new MD_ReferenceSystem_PropertyType 
                { 
                    MD_ReferenceSystem = new MD_ReferenceSystem_Type 
                    { 
                        referenceSystemIdentifier = new RS_Identifier_PropertyType 
                        { 
                            RS_Identifier = new RS_Identifier_Type 
                            { 
                                code = new Anchor_PropertyType
                                {
                                  anchor =  new Anchor_Type{  Value = "http://www.opengis.net/def/crs/EPSG/0/25831", href = "http://www.opengis.net/def/crs/EPSG/0/25831" }
                                }, 
                                codeSpace = new CharacterString_PropertyType 
                                { 
                                    CharacterString = "EPSG" 
                                } 
                            } 
                        } 
                    } 
                } 
            };

            return m;
        }




        private static CharacterString_PropertyType CharString(string s)
        {
            var sp = new CharacterString_PropertyType();
            sp.CharacterString = s;
            return sp;
        }
    }
}
