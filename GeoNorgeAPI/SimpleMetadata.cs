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

        private MD_DataIdentification_Type GetDatasetIdentification()
        {
            MD_DataIdentification_Type identification = null;
            if (HierarchyLevel == "dataset")
                identification = GetIdentification() as MD_DataIdentification_Type;
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

        public string SupplementalDescription
        {
            get
            {
                string desc = null;
                var datasetIdentification = GetDatasetIdentification();
                if (datasetIdentification != null && datasetIdentification.supplementalInformation != null) {
                    desc = datasetIdentification.supplementalInformation.CharacterString;
                }
                return desc;
            }

            set
            {
                var datasetIdentification = GetDatasetIdentification();
                if (datasetIdentification != null && datasetIdentification.supplementalInformation != null)
                {
                    datasetIdentification.supplementalInformation = new CharacterString_PropertyType { CharacterString = value };
                }
            }
        }

        public SimpleContact ContactPublisher
        {
            get { return GetContactWithRole("publisher"); }
            set { CreatOrUpdateContactWithRole("publisher", value); }
        }

        public SimpleContact ContactPointOfContact
        {
            get { return GetContactWithRole("pointOfContact"); }
            set { CreatOrUpdateContactWithRole("pointOfContact", value); }
        }

        private void CreatOrUpdateContactWithRole(string roleCodeValue, SimpleContact contact)
        {
            CI_ResponsibleParty_Type responsibleParty = GetContactInformationResponsiblePartyWithRole(roleCodeValue);

            if (responsibleParty == null)
            {
                responsibleParty = new CI_ResponsibleParty_Type();
                
                var newPointOfContactArray = new CI_ResponsibleParty_PropertyType[] {
                        new CI_ResponsibleParty_PropertyType {
                            CI_ResponsibleParty = responsibleParty
                        }                        
                    };

                var identification = GetIdentificationNotNull();
                if (identification.pointOfContact == null)
                {
                    identification.pointOfContact = newPointOfContactArray;
                }
                else
                {
                    identification.pointOfContact = identification.pointOfContact.Concat(newPointOfContactArray).ToArray();
                }
            }

            responsibleParty.individualName = new CharacterString_PropertyType { CharacterString = contact.Name };
            responsibleParty.organisationName = new CharacterString_PropertyType { CharacterString = contact.Organization };
            if (responsibleParty.contactInfo == null)
            {
                responsibleParty.contactInfo = new CI_Contact_PropertyType
                {
                    CI_Contact = new CI_Contact_Type
                    {
                        address = new CI_Address_PropertyType
                        {
                            CI_Address = new CI_Address_Type()
                        }
                    }
                };
            }
            if (responsibleParty.contactInfo.CI_Contact == null)
            {
                responsibleParty.contactInfo.CI_Contact = new CI_Contact_Type
                    {
                        address = new CI_Address_PropertyType
                        {
                            CI_Address = new CI_Address_Type()
                        }
                    };
            }
            if (responsibleParty.contactInfo.CI_Contact.address == null)
            {
                responsibleParty.contactInfo.CI_Contact.address = new CI_Address_PropertyType
                {
                    CI_Address = new CI_Address_Type()
                };
            }
            if (responsibleParty.contactInfo.CI_Contact.address.CI_Address == null)
            {
                responsibleParty.contactInfo.CI_Contact.address.CI_Address = new CI_Address_Type();
            }


            responsibleParty.contactInfo.CI_Contact.address.CI_Address.electronicMailAddress = new CharacterString_PropertyType[] { 
                    new CharacterString_PropertyType { CharacterString = contact.Email }
                };

            responsibleParty.role = new CI_RoleCode_PropertyType
            {
                CI_RoleCode = new CodeListValue_Type
                {
                    codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/Codelist/ML_gmxCodelists.xml#CI_RoleCode",
                    codeListValue = contact.Role
                }
            };
        }

        private CI_ResponsibleParty_Type GetContactInformationResponsiblePartyWithRole(string roleCodeValue) 
        {
            CI_ResponsibleParty_Type contact = null;
            var identification = GetIdentification();
            if (identification != null && identification.pointOfContact != null)
            {
                foreach (var responsibleParty in identification.pointOfContact)
                {
                    if (responsibleParty.CI_ResponsibleParty != null)
                    {
                        if (responsibleParty.CI_ResponsibleParty.role != null && responsibleParty.CI_ResponsibleParty.role.CI_RoleCode != null
                            && responsibleParty.CI_ResponsibleParty.role.CI_RoleCode.codeListValue != null
                            && responsibleParty.CI_ResponsibleParty.role.CI_RoleCode.codeListValue.ToLower() == roleCodeValue.ToLower())
                        {
                            contact = responsibleParty.CI_ResponsibleParty;
                            break;
                        }
                    }
                }
            }
            return contact;
        }

        private SimpleContact GetContactWithRole(string roleCodeValue)
        {
            SimpleContact contact = null;

            var identification = GetIdentification();
            if (identification != null && identification.pointOfContact != null)
            {
                foreach (var responsibleParty in identification.pointOfContact)
                {
                    if (responsibleParty.CI_ResponsibleParty != null)
                    {
                        if (responsibleParty.CI_ResponsibleParty.role != null && responsibleParty.CI_ResponsibleParty.role.CI_RoleCode != null
                            && responsibleParty.CI_ResponsibleParty.role.CI_RoleCode.codeListValue != null
                            && responsibleParty.CI_ResponsibleParty.role.CI_RoleCode.codeListValue.ToLower() == roleCodeValue.ToLower())
                        {
                            var p = responsibleParty.CI_ResponsibleParty;

                            string email = null;
                            if (p.contactInfo != null && p.contactInfo.CI_Contact != null && p.contactInfo.CI_Contact.address != null 
                                && p.contactInfo.CI_Contact.address.CI_Address != null
                                && p.contactInfo.CI_Contact.address.CI_Address.electronicMailAddress != null
                                && p.contactInfo.CI_Contact.address.CI_Address.electronicMailAddress[0] != null
                                && p.contactInfo.CI_Contact.address.CI_Address.electronicMailAddress[0].CharacterString != null)
                            {
                                email = p.contactInfo.CI_Contact.address.CI_Address.electronicMailAddress[0].CharacterString;
                            }

                            string role = null;
                            if (p.role != null && p.role.CI_RoleCode != null)
                            {
                                role = p.role.CI_RoleCode.codeListValue;
                            }

                            contact = new SimpleContact
                            {
                                Name = GetStringOrNull(p.individualName),
                                Organization = GetStringOrNull(p.organisationName),
                                Email = email,
                                Role = role
                            };
                            break;
                        }
                    }
                }
            }
            return contact;
        }

        private string GetStringOrNull(CharacterString_PropertyType input)
        {
            return input != null ? input.CharacterString : null;
        }
    }

    public class SimpleContact
    {
        public string Name { get; set; }
        public string Organization { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
