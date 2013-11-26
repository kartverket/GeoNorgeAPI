using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.opengis.net;

namespace GeoNorgeAPI
{
    /// <summary>
    /// Simple abstraction of the opengis metadata. Provides convenience methods for extracting information from the metadata object.
    /// </summary>
    public class SimpleMetadata
    {
        private MD_Metadata_Type _md;

        /// <summary>
        /// Construct simple metadata object based on the opengis metadata.
        /// </summary>
        /// <param name="md">The original metadata object</param>
        public SimpleMetadata(MD_Metadata_Type md) {
            if (md == null) 
                throw new ArgumentNullException("md", "Metadata cannot be null.");
            _md = md;
        }

        public MD_Metadata_Type GetMetadata()
        {
            return _md;
        }

        public string Title
        {
            get
            {
                string title = null;
                var identification = GetIdentification();
                if (identification != null && identification.citation != null && identification.citation.CI_Citation != null && identification.citation.CI_Citation.title != null)
                {
                    title = identification.citation.CI_Citation.title.CharacterString;
                }
                return title;
            }
            set {
                var identification = GetIdentification();
                if (identification == null)
                {
                    throw new NullReferenceException("Identification element is null.");
                }

                if (identification.citation == null)
                {
                    identification.citation = new CI_Citation_PropertyType();
                }

                if (identification.citation.CI_Citation == null)
                {
                    identification.citation.CI_Citation = new CI_Citation_Type();
                }

                if (identification.citation.CI_Citation.title == null)
                {
                    identification.citation.CI_Citation.title = new CharacterString_PropertyType();
                }

                identification.citation.CI_Citation.title.CharacterString = value;                    
            }
        }

        private AbstractMD_Identification_Type GetIdentification()
        {
            AbstractMD_Identification_Type identification = null; 
            if (_md.identificationInfo != null && _md.identificationInfo.Count() > 0 && _md.identificationInfo[0].AbstractMD_Identification != null)
                identification = _md.identificationInfo[0].AbstractMD_Identification;
            return identification;
        }

        private AbstractMD_Identification_Type GetIdentificationNotNull()
        {
            var identification = GetIdentification();
            if (identification == null)
                throw new NullReferenceException("Identification element is null");
            return identification;
        }


        public string Uuid
        {
            get
            {
                string uuid = null;
                if (_md.fileIdentifier != null)
                    uuid = _md.fileIdentifier.CharacterString;
                return uuid;
            }

            set
            {
                _md.fileIdentifier = new CharacterString_PropertyType { CharacterString = value };
            }
        }
        /// <summary>
        /// Note: Only supporting one hierarchyLevel element. Array is overwritten with an array of one element when value is updated.
        /// </summary>
        public string HierarchyLevel
        {
            get
            {
                string hierarchyLevel = null;
                if (_md.hierarchyLevel != null && _md.hierarchyLevel.Count() > 0 && _md.hierarchyLevel[0] != null && _md.hierarchyLevel[0].MD_ScopeCode != null)
                    hierarchyLevel = _md.hierarchyLevel[0].MD_ScopeCode.codeListValue;
                return hierarchyLevel;
            }

            set
            {
                _md.hierarchyLevel = new MD_ScopeCode_PropertyType[] { 
                    new MD_ScopeCode_PropertyType { 
                        MD_ScopeCode = new CodeListValue_Type { 
                            codeList = "http://www.isotc211.org/2005/resources/Codelist/ML_gmxCodelists.xml#MD_ScopeCode",
                            codeListValue = value
                        }
                    }
                };
            }
        }

        public string Abstract
        {
            get 
            {
                string @abstract = null;
                var identification = GetIdentification();
                if (identification != null && identification.@abstract != null)
                    @abstract = identification.@abstract.CharacterString;
                return @abstract;
            }

            set
            {
                var identification = GetIdentificationNotNull();
                identification.@abstract = new CharacterString_PropertyType { CharacterString = value };                
            }
        }

        public string Purpose {
            get 
            {
                string purpose = null;
                var identification = GetIdentification();
                if (identification != null && identification.purpose != null)
                    purpose = identification.purpose.CharacterString;
                return purpose;
            }
            set
            {
                var identification = GetIdentificationNotNull();
                identification.purpose = new CharacterString_PropertyType { CharacterString = value };
            }
        }
    }
}
