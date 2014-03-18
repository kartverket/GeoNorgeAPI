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

    }
}
