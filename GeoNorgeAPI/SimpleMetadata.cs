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
        private const string APPLICATION_PROFILE_PRODUCTSPEC = "produktspesifikasjon";
        private const string APPLICATION_PROFILE_PRODUCTSHEET = "produktark";
        private const string APPLICATION_PROFILE_LEGEND = "tegnforklaring";
        private const string APPLICATION_PROFILE_PRODUCTPAGE = "produktside";
        private const string RESOURCE_PROTOCOL_WWW = "WWW:LINK-1.0-http--related";

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


        public List<SimpleKeyword> Keywords
        {
            get
            {
                var keywords = new List<SimpleKeyword>();
                var identification = GetIdentification();
                if (identification != null && identification.descriptiveKeywords != null)
                {
                    foreach (var descriptiveKeyword in identification.descriptiveKeywords)
                    {
                        if (descriptiveKeyword.MD_Keywords != null && descriptiveKeyword.MD_Keywords.keyword != null)
                        {
                            string type = "";
                            string thesaurus = null;
                            if (descriptiveKeyword.MD_Keywords.type != null && descriptiveKeyword.MD_Keywords.type.MD_KeywordTypeCode != null
                                && descriptiveKeyword.MD_Keywords.type.MD_KeywordTypeCode.codeListValue != null)
                            {
                                type = descriptiveKeyword.MD_Keywords.type.MD_KeywordTypeCode.codeListValue;
                            }

                            if (descriptiveKeyword.MD_Keywords.thesaurusName != null && descriptiveKeyword.MD_Keywords.thesaurusName.CI_Citation != null
                                && descriptiveKeyword.MD_Keywords.thesaurusName.CI_Citation.title != null)
                            {
                                thesaurus = GetStringOrNull(descriptiveKeyword.MD_Keywords.thesaurusName.CI_Citation.title);
                            }

                            foreach (var keywordElement in descriptiveKeyword.MD_Keywords.keyword)
                            {
                                string keywordValue = GetStringOrNull(keywordElement);
                                if (!string.IsNullOrWhiteSpace(keywordValue))
                                {
                                    keywords.Add(new SimpleKeyword
                                    {
                                        Keyword = keywordValue,
                                        Thesaurus = thesaurus,
                                        Type = type
                                    });
                                }
                            }
                        }
                    }
                }
                return keywords;
            }

            set
            {

            }
        }

        // dataset
        public string TopicCategory
        {
            get {
                string topicCategory = null;
                var identification = GetDatasetIdentification();
                if (identification != null && identification.topicCategory != null && identification.topicCategory.Length > 0
                    && identification.topicCategory[0] != null)
                {
                    var topic = identification.topicCategory[0];
                    if (topic.MD_TopicCategoryCode != null)
                    {
                        topicCategory = topic.MD_TopicCategoryCode.ToString();
                    }
                }

                return topicCategory;
            }
            set {
                var identification = GetDatasetIdentification();
                if (identification != null)
                {
                    if (identification.topicCategory == null)
                    {
                        identification.topicCategory = new MD_TopicCategoryCode_PropertyType [1];
                    }

                    identification.topicCategory[0] =
                        new MD_TopicCategoryCode_PropertyType
                        {
                            MD_TopicCategoryCode = (MD_TopicCategoryCode_Type)Enum.Parse(typeof(MD_TopicCategoryCode_Type), value, true)
                        };
                }
            }
        }

        public List<SimpleThumbnail> Thumbnails
        {
            get {
                List<SimpleThumbnail> thumbnails = new List<SimpleThumbnail>();
                
                var identification = GetIdentification();
                if (identification != null && identification.graphicOverview != null && identification.graphicOverview.Length > 0
                    && identification.graphicOverview[0] != null)
                {

                    foreach (MD_BrowseGraphic_PropertyType browseGraphic in identification.graphicOverview)
                    {
                        thumbnails.Add(new SimpleThumbnail
                        {
                            Type = browseGraphic.MD_BrowseGraphic.fileDescription.CharacterString,
                            URL = browseGraphic.MD_BrowseGraphic.fileName.CharacterString
                        });
                    }
                    
                }
                return thumbnails;
            }
            set {
                if (value != null) {
                    var identification = GetIdentification();
                    if (identification != null)
                    {
                        List<MD_BrowseGraphic_PropertyType> graphics = new List<MD_BrowseGraphic_PropertyType>();
                        foreach(SimpleThumbnail thumbnail in value) {
                            MD_BrowseGraphic_PropertyType graphic = new MD_BrowseGraphic_PropertyType
                            {
                                MD_BrowseGraphic = new MD_BrowseGraphic_Type
                                {
                                    fileName = new CharacterString_PropertyType { CharacterString = thumbnail.URL },
                                    fileType = new CharacterString_PropertyType { CharacterString = thumbnail.Type }
                                }
                            };
                            graphics.Add(graphic);
                        }

                        identification.graphicOverview = graphics.ToArray();
                    }
                }
            }
        }

        private CI_OnlineResource_Type GetMetadataExtensionInfoWithApplicationProfile(string applicationProfile)
        {
            CI_OnlineResource_Type onlineResource = null;
            if (_md.metadataExtensionInfo != null && _md.metadataExtensionInfo.Length > 0)
            {
                foreach (MD_MetadataExtensionInformation_PropertyType ext in _md.metadataExtensionInfo)
                {
                    if (ext.MD_MetadataExtensionInformation != null && ext.MD_MetadataExtensionInformation.extensionOnLineResource != null
                        && ext.MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource != null
                        && ext.MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource.applicationProfile != null
                        && ext.MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource.applicationProfile.CharacterString == applicationProfile)
                    {
                        onlineResource = ext.MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource;
                    }
                }
            }
            return onlineResource;
        }

        private string GetMetadataExtensionInfoURLWithApplicationProfile(string applicationProfile)
        {
            string url = null;
            CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(applicationProfile);
            if (onlineResource != null && onlineResource.linkage != null)
            {
                url = onlineResource.linkage.URL;
            }
            return url;
        }

        public string ProductSpecificationUrl
        {
            get {
                return GetMetadataExtensionInfoURLWithApplicationProfile(APPLICATION_PROFILE_PRODUCTSPEC);
            }
            set {
                CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(APPLICATION_PROFILE_PRODUCTSPEC);
                if (onlineResource == null)
                {
                    onlineResource = new CI_OnlineResource_Type();
                    AddOnlineResourceToMetadataExtensionInfo(onlineResource);
                }
                onlineResource.linkage = new URL_PropertyType { URL = value };
                onlineResource.applicationProfile = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_PRODUCTSPEC };
                onlineResource.name = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_PRODUCTSPEC };
                onlineResource.protocol = new CharacterString_PropertyType { CharacterString = RESOURCE_PROTOCOL_WWW };
            }
        }

        private void AddOnlineResourceToMetadataExtensionInfo(CI_OnlineResource_Type onlineResource)
        {
            MD_MetadataExtensionInformation_PropertyType extensionInfo = new MD_MetadataExtensionInformation_PropertyType();
            extensionInfo.MD_MetadataExtensionInformation = new MD_MetadataExtensionInformation_Type
            {
                extensionOnLineResource = new CI_OnlineResource_PropertyType
                {
                    CI_OnlineResource = onlineResource
                }
            };

            MD_MetadataExtensionInformation_PropertyType[] newExtensionInfo = new MD_MetadataExtensionInformation_PropertyType[] {
                    extensionInfo
                };
                
            if (_md.metadataExtensionInfo == null)
            {
                _md.metadataExtensionInfo = newExtensionInfo;
            }
            else
            {
                _md.metadataExtensionInfo = _md.metadataExtensionInfo.Concat(newExtensionInfo).ToArray();
            }
        }


        public string ProductSheetUrl
        {
            get
            {
                return GetMetadataExtensionInfoURLWithApplicationProfile(APPLICATION_PROFILE_PRODUCTSHEET);
            }
            set
            {
                CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(APPLICATION_PROFILE_PRODUCTSHEET);
                if (onlineResource == null)
                {
                    onlineResource = new CI_OnlineResource_Type();
                    AddOnlineResourceToMetadataExtensionInfo(onlineResource);
                }
                onlineResource.linkage = new URL_PropertyType { URL = value };
                onlineResource.applicationProfile = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_PRODUCTSHEET };
                onlineResource.name = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_PRODUCTSHEET };
                onlineResource.protocol = new CharacterString_PropertyType { CharacterString = RESOURCE_PROTOCOL_WWW };
            }
        }

        public string LegendDescriptionUrl
        {
            get
            {
                return GetMetadataExtensionInfoURLWithApplicationProfile(APPLICATION_PROFILE_LEGEND);
            }
            set
            {
                CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(APPLICATION_PROFILE_LEGEND);
                if (onlineResource == null)
                {
                    onlineResource = new CI_OnlineResource_Type();
                    AddOnlineResourceToMetadataExtensionInfo(onlineResource);
                }
                onlineResource.linkage = new URL_PropertyType { URL = value };
                onlineResource.applicationProfile = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_LEGEND };
                onlineResource.name = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_LEGEND };
                onlineResource.protocol = new CharacterString_PropertyType { CharacterString = RESOURCE_PROTOCOL_WWW };
            }
        }


        public string ProductPageUrl
        {
            get
            {
                return GetMetadataExtensionInfoURLWithApplicationProfile(APPLICATION_PROFILE_PRODUCTPAGE);
            }
            set
            {
                CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(APPLICATION_PROFILE_PRODUCTPAGE);
                if (onlineResource == null)
                {
                    onlineResource = new CI_OnlineResource_Type();
                    AddOnlineResourceToMetadataExtensionInfo(onlineResource);
                }
                onlineResource.linkage = new URL_PropertyType { URL = value };
                onlineResource.applicationProfile = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_PRODUCTPAGE };
                onlineResource.name = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_PRODUCTPAGE };
                onlineResource.protocol = new CharacterString_PropertyType { CharacterString = RESOURCE_PROTOCOL_WWW };
            }
        }

        public string SpatialRepresentation
        {
            get
            {
                string value = null;
                var datasetIdentification = GetDatasetIdentification();
                if (datasetIdentification != null && datasetIdentification.spatialRepresentationType != null 
                    && datasetIdentification.spatialRepresentationType.Length > 0)
                {
                    MD_SpatialRepresentationTypeCode_PropertyType spatialRepType = datasetIdentification.spatialRepresentationType[0];
                    if (spatialRepType != null && spatialRepType.MD_SpatialRepresentationTypeCode != null)
                    {
                        value = spatialRepType.MD_SpatialRepresentationTypeCode.codeListValue;
                    }
                }
                return value;
            }

            set
            {
                MD_DataIdentification_Type datasetIdentification = GetDatasetIdentification();
                if (datasetIdentification != null)
                {
                    MD_SpatialRepresentationTypeCode_PropertyType[] newSpatialRepresentationArray = new MD_SpatialRepresentationTypeCode_PropertyType[] {
                        new MD_SpatialRepresentationTypeCode_PropertyType {
                            MD_SpatialRepresentationTypeCode = new CodeListValue_Type {
                                codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/Codelist/ML_gmxCodelists.xml#MD_SpatialRepresentationTypeCode",
                                codeListValue = value
                            }
                        }
                    };

                    MD_SpatialRepresentationTypeCode_PropertyType[] spatialRepresentationArray = datasetIdentification.spatialRepresentationType;

                    if (spatialRepresentationArray == null)
                    {
                        spatialRepresentationArray = new MD_SpatialRepresentationTypeCode_PropertyType[0];
                    }
                    datasetIdentification.spatialRepresentationType = spatialRepresentationArray.Concat(newSpatialRepresentationArray).ToArray();
                }
            }
        }

        // TODO: Handle multiple distribution formats
        public SimpleDistributionFormat DistributionFormat
        {
            get
            {
                SimpleDistributionFormat format = null;
                if (_md.distributionInfo != null && _md.distributionInfo.MD_Distribution != null 
                    && _md.distributionInfo.MD_Distribution.distributionFormat != null
                    && _md.distributionInfo.MD_Distribution.distributionFormat.Length > 0
                    && _md.distributionInfo.MD_Distribution.distributionFormat[0] != null
                    && _md.distributionInfo.MD_Distribution.distributionFormat[0].MD_Format != null
                    && _md.distributionInfo.MD_Distribution.distributionFormat[0].MD_Format.name != null
                    && _md.distributionInfo.MD_Distribution.distributionFormat[0].MD_Format.name != null)
                {
                    var df = _md.distributionInfo.MD_Distribution.distributionFormat[0].MD_Format;
                    format = new SimpleDistributionFormat {
                        Name = df.name.CharacterString,
                        Version = df.version != null ? df.version.CharacterString : null
                    };
                }
                return format;
            }

            set
            {
                if (_md.distributionInfo == null)
                {
                    _md.distributionInfo = new MD_Distribution_PropertyType { MD_Distribution = new MD_Distribution_Type() };
                }
                if (_md.distributionInfo.MD_Distribution == null)
                {
                    _md.distributionInfo.MD_Distribution = new MD_Distribution_Type();
                }

                _md.distributionInfo.MD_Distribution.distributionFormat = new MD_Format_PropertyType[]
                {
                    new MD_Format_PropertyType {
                        MD_Format = new MD_Format_Type
                        {
                            name = toCharString(value.Name),
                            version = toCharString(value.Version)
                        }
                    }
                };

            }
        }

        public SimpleReferenceSystem ReferenceSystem
        {
            get
            {
                SimpleReferenceSystem value = null;

                if (_md.referenceSystemInfo != null && _md.referenceSystemInfo.Length > 0 && _md.referenceSystemInfo[0] != null
                    && _md.referenceSystemInfo[0].MD_ReferenceSystem != null 
                    && _md.referenceSystemInfo[0].MD_ReferenceSystem.referenceSystemIdentifier != null
                    && _md.referenceSystemInfo[0].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier != null
                    && _md.referenceSystemInfo[0].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier.code != null
                    && _md.referenceSystemInfo[0].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier.codeSpace != null)
                {
                    RS_Identifier_Type identifier = _md.referenceSystemInfo[0].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier;

                    value = new SimpleReferenceSystem
                    {
                        CoordinateSystem = identifier.code.CharacterString,
                        Namespace = identifier.codeSpace.CharacterString
                    };
                }
                return value;
            }

            set
            {
                _md.referenceSystemInfo = new MD_ReferenceSystem_PropertyType[] {
                    new MD_ReferenceSystem_PropertyType {
                        MD_ReferenceSystem = new MD_ReferenceSystem_Type {
                            referenceSystemIdentifier = new RS_Identifier_PropertyType {
                                 RS_Identifier = new RS_Identifier_Type { 
                                     code = toCharString(value.CoordinateSystem),
                                     codeSpace = toCharString(value.Namespace)
                                 }
                            }
                        }
                    }
                };
            }
        }

        public SimpleDistributionDetails DistributionDetails
        {
            get
            {
                SimpleDistributionDetails value = null;

                if (_md.distributionInfo != null && _md.distributionInfo.MD_Distribution != null
                    && _md.distributionInfo.MD_Distribution.transferOptions != null
                    && _md.distributionInfo.MD_Distribution.transferOptions.Length > 0
                    && _md.distributionInfo.MD_Distribution.transferOptions[0] != null
                    && _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions != null
                    && _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine != null
                    && _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine.Length > 0
                    && _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine[0] != null
                    && _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource != null
                    && _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource.linkage != null)
                {
                    var resource = _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource;
                    value = new SimpleDistributionDetails
                    {
                        URL = resource.linkage.URL,
                        Protocol = resource.protocol != null ? resource.protocol.CharacterString : null
                    };
                }
                return value;
            }

            set
            {
                if (_md.distributionInfo == null)
                {
                    _md.distributionInfo = new MD_Distribution_PropertyType();
                }
                if (_md.distributionInfo.MD_Distribution == null)
                {
                    _md.distributionInfo.MD_Distribution = new MD_Distribution_Type();
                }

                _md.distributionInfo.MD_Distribution.transferOptions = new MD_DigitalTransferOptions_PropertyType[] {
                    new MD_DigitalTransferOptions_PropertyType {
                        MD_DigitalTransferOptions = new MD_DigitalTransferOptions_Type {
                            onLine = new CI_OnlineResource_PropertyType[] {
                                new CI_OnlineResource_PropertyType {
                                    CI_OnlineResource = new CI_OnlineResource_Type {
                                         linkage = new URL_PropertyType { URL = value.URL },
                                         protocol = new CharacterString_PropertyType { CharacterString = value.Protocol }
                                    }
                                }
                            }
                        }
                    }
                };

            }
        }

        public string ResolutionScale
        {
            get
            {
                string value = null;
                var dataIdentification = GetDatasetIdentification();
                if (dataIdentification != null
                    && dataIdentification.spatialResolution != null
                    && dataIdentification.spatialResolution.Length > 0
                    && dataIdentification.spatialResolution[0] != null
                    && dataIdentification.spatialResolution[0].MD_Resolution != null
                    && dataIdentification.spatialResolution[0].MD_Resolution.Item != null)
                {
                    MD_RepresentativeFraction_PropertyType item = dataIdentification.spatialResolution[0].MD_Resolution.Item as MD_RepresentativeFraction_PropertyType;
                    if (item != null
                        && item.MD_RepresentativeFraction != null
                        && item.MD_RepresentativeFraction.denominator != null)
                    {
                        value = item.MD_RepresentativeFraction.denominator.Integer;
                    }
                }
                return value;
            }

            set
            {
                var dataIdentification = GetDatasetIdentification();
                if (dataIdentification != null)
                {
                    MD_Resolution_PropertyType resolution = new MD_Resolution_PropertyType
                    {
                        MD_Resolution = new MD_Resolution_Type
                        {
                            Item = new MD_RepresentativeFraction_PropertyType
                            {
                                MD_RepresentativeFraction = new MD_RepresentativeFraction_Type
                                {
                                    denominator = new Integer_PropertyType { Integer = value }
                                }
                            }
                        }
                    };

                    dataIdentification.spatialResolution = new MD_Resolution_PropertyType[] { resolution };
                }
            }
        }

        /// <summary>
        /// Values from codelist: http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ProgressCode
        /// </summary>
        public string MaintenanceFrequency
        {
            get 
            {
                string value = null;
                var identification = GetIdentification();
                if (identification != null
                    && identification.resourceMaintenance != null
                    && identification.resourceMaintenance.Length > 0
                    && identification.resourceMaintenance[0] != null
                    && identification.resourceMaintenance[0].MD_MaintenanceInformation != null
                    && identification.resourceMaintenance[0].MD_MaintenanceInformation.maintenanceAndUpdateFrequency != null
                    && identification.resourceMaintenance[0].MD_MaintenanceInformation.maintenanceAndUpdateFrequency.MD_MaintenanceFrequencyCode != null)
                {
                    var codelist = identification.resourceMaintenance[0].MD_MaintenanceInformation.maintenanceAndUpdateFrequency.MD_MaintenanceFrequencyCode;
                    value = codelist.codeListValue;
                }

                return value;
            }

            set
            {
                var identification = GetIdentificationNotNull();
                if (identification != null)
                {
                    identification.resourceMaintenance = new MD_MaintenanceInformation_PropertyType[] {
                        new MD_MaintenanceInformation_PropertyType {
                            MD_MaintenanceInformation = new MD_MaintenanceInformation_Type {
                                maintenanceAndUpdateFrequency = new MD_MaintenanceFrequencyCode_PropertyType {
                                    MD_MaintenanceFrequencyCode = new CodeListValue_Type {
                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ProgressCode",
                                        codeListValue = value
                                    }
                                }
                            }
                        }
                    };
                }
            }
        }

        private CharacterString_PropertyType toCharString(string input)
        {
            return new CharacterString_PropertyType { CharacterString = input };
        }

    }

    public class SimpleContact
    {
        public string Name { get; set; }
        public string Organization { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class SimpleKeyword
    {
        public string Keyword { get; set; }
        public string Type { get; set; }
        public string Thesaurus { get; set; }
    }

    public class SimpleThumbnail
    {
        public string URL { get; set; }
        public string Type { get; set; }
    }

    public class SimpleDistributionFormat
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }

    public class SimpleReferenceSystem
    {
        public string CoordinateSystem { get; set; }
        public string Namespace { get; set; }
    }

    public class SimpleDistributionDetails
    {
        public string URL { get; set; }
        public string Protocol { get; set; }
    }

}
