using NUnit.Framework;
using System;
using www.opengis.net;
using GeoNorgeAPI;

namespace GeoNorgeAPI.Tests
{
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
    }
}
