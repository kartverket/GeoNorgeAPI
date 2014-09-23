using System;
using System.Collections.Generic;
using System.Globalization;
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
        public const string LOCALE_ENG = "ENG";
        public const string LOCALE_LINK_ENG = "#" + LOCALE_ENG;

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

        public static SimpleMetadata CreateService()
        {
            SimpleMetadata metadata = CreateSimpleMetadata("service", new SV_ServiceIdentification_Type());
            metadata.GetServiceIdentification().couplingType = new SV_CouplingType_PropertyType 
            { 
                SV_CouplingType = new CodeListValue_Type 
                { 
                    codeList = "",
                    codeListValue = "tight"
                } 
            };
            metadata.GetServiceIdentification().serviceType = new GenericName_PropertyType { Item = new CodeType { Value = "view" } };
            metadata.GetServiceIdentification().containsOperations = new SV_OperationMetadata_PropertyType[] 
            { 
                new SV_OperationMetadata_PropertyType()
            };
            return metadata;
        }

        public static SimpleMetadata CreateDataset()
        {
            return CreateSimpleMetadata("dataset", new MD_DataIdentification_Type());
        }

        private static SimpleMetadata CreateSimpleMetadata(string hierarchyLevel, AbstractMD_Identification_Type identification)
        {
            MD_Metadata_Type md = new MD_Metadata_Type
            {
                fileIdentifier = new CharacterString_PropertyType { CharacterString = Guid.NewGuid().ToString() },
                identificationInfo = new MD_Identification_PropertyType[]
                {
                    new MD_Identification_PropertyType {
                        AbstractMD_Identification = identification
                    }                    
                }
            };
            SimpleMetadata simpleMetadata = new SimpleMetadata(md);
            simpleMetadata.HierarchyLevel = hierarchyLevel;
            return simpleMetadata;
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
                CharacterString_PropertyType titleElement = GetTitleElement();
                if (titleElement != null)
                {
                    title = titleElement.CharacterString;
                } 
                return title;
            }
            set {
                PT_FreeText_PropertyType titleElementWithFreeText = GetTitleElement() as PT_FreeText_PropertyType;
                if (titleElementWithFreeText != null)
                {
                    titleElementWithFreeText.CharacterString = value;
                }
                else
                {
                    SetTitleElement(new CharacterString_PropertyType { CharacterString = value });
                }
                
            }
        }

        private CharacterString_PropertyType GetTitleElement()
        {
            CharacterString_PropertyType title = null;
            var identification = GetIdentification();
            if (identification != null && identification.citation != null && identification.citation.CI_Citation != null && identification.citation.CI_Citation.title != null)
            {
                title = identification.citation.CI_Citation.title;
            }
            return title;
        }

        private void SetTitleElement(CharacterString_PropertyType element)
        {
            var identification = GetIdentificationNotNull();
            
            if (identification.citation == null)
            {
                identification.citation = new CI_Citation_PropertyType();
            }

            if (identification.citation.CI_Citation == null)
            {
                identification.citation.CI_Citation = new CI_Citation_Type();
            }

            identification.citation.CI_Citation.title = element;
        }

        private string GetEnglishValueFromFreeText(CharacterString_PropertyType input)
        {
            string value = null;
            if (input != null)
            {
                PT_FreeText_PropertyType freeText = input as PT_FreeText_PropertyType;
                if (freeText != null && freeText.PT_FreeText != null && freeText.PT_FreeText.textGroup != null)
                {
                    foreach (var localizedStringProperty in freeText.PT_FreeText.textGroup)
                    {
                        if (localizedStringProperty.LocalisedCharacterString != null
                            && localizedStringProperty.LocalisedCharacterString.locale != null
                            && localizedStringProperty.LocalisedCharacterString.locale.ToUpper().Equals(LOCALE_LINK_ENG))
                        {
                            value = localizedStringProperty.LocalisedCharacterString.Value;
                            break;
                        }
                    }
                }
            }
            return value;
        }

        public string EnglishTitle 
        {
            get
            {
                return GetEnglishValueFromFreeText(GetTitleElement());                
            }

            set
            {
                String existingLocalTitle = null;
                CharacterString_PropertyType titleElement = GetTitleElement();
                if (titleElement != null)
                {
                    existingLocalTitle = titleElement.CharacterString;
                }

                SetTitleElement(CreateFreeTextElement(existingLocalTitle, value));
            }
        }

        private PT_FreeText_PropertyType CreateFreeTextElement(string characterString, string englishLocalizedValue) {
            return new PT_FreeText_PropertyType { 
                    CharacterString = characterString,
                    PT_FreeText = new PT_FreeText_Type
                    {
                        textGroup = new LocalisedCharacterString_PropertyType[] { 
                            new LocalisedCharacterString_PropertyType {
                                LocalisedCharacterString = new LocalisedCharacterString_Type {
                                     locale = LOCALE_LINK_ENG,
                                     Value = englishLocalizedValue
                                }
                            }
                        }
                    }
                };
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
            return GetIdentification() as MD_DataIdentification_Type;
        }

        private SV_ServiceIdentification_Type GetServiceIdentification()
        {
            SV_ServiceIdentification_Type identification = null;
            if (IsService())
                identification = GetIdentification() as SV_ServiceIdentification_Type;
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
                CharacterString_PropertyType abstractElement = GetAbstractElement();
                if (abstractElement != null)
                {
                    @abstract = abstractElement.CharacterString;
                }
                return @abstract;
            }

            set
            {
                PT_FreeText_PropertyType abstractElementWithFreeText = GetAbstractElement() as PT_FreeText_PropertyType;
                if (abstractElementWithFreeText != null)
                {
                    abstractElementWithFreeText.CharacterString = value;
                }
                else
                {
                    GetIdentificationNotNull().@abstract = new CharacterString_PropertyType { CharacterString = value };
                }
            }
        }

        public string EnglishAbstract
        {
            get
            {
                return GetEnglishValueFromFreeText(GetAbstractElement());
            }

            set
            {
                String existingLocalAbstract = null;
                CharacterString_PropertyType abstractElement = GetAbstractElement();
                if (abstractElement != null)
                {
                    existingLocalAbstract = abstractElement.CharacterString;
                }
                GetIdentificationNotNull().@abstract = CreateFreeTextElement(existingLocalAbstract, value);
            }
        }

        private CharacterString_PropertyType GetAbstractElement()
        {
            CharacterString_PropertyType @abstract = null;
            var identification = GetIdentification();
            if (identification != null && identification.@abstract != null)
                @abstract = identification.@abstract;
            return @abstract;
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
                if (datasetIdentification != null)
                {
                    datasetIdentification.supplementalInformation = new CharacterString_PropertyType { CharacterString = value };
                }
            }
        }

        public SimpleContact ContactMetadata
        {
            get
            {
                SimpleContact contact = null;
                if (_md.contact != null && _md.contact.Length > 0 && _md.contact[0] != null && _md.contact[0].CI_ResponsibleParty != null)
                {
                    contact = ParseResponsiblePartyToSimpleContact(_md.contact[0].CI_ResponsibleParty);
                }
                return contact;
            }
            set
            {
                CI_ResponsibleParty_Type responsibleParty = CreateResponsiblePartyFromSimpleContact(value);
                _md.contact = new CI_ResponsibleParty_PropertyType[]
                {
                    new CI_ResponsibleParty_PropertyType {
                        CI_ResponsibleParty = responsibleParty    
                    }
                };
            }
        }

        public SimpleContact ContactPublisher
        {
            get { return GetContactWithRole("publisher"); }
            set { CreateOrUpdateContactWithRole("publisher", value); }
        }

        public SimpleContact ContactOwner
        {
            get { return GetContactWithRole("owner"); }
            set { CreateOrUpdateContactWithRole("owner", value); }
        }

        private void CreateOrUpdateContactWithRole(string roleCodeValue, SimpleContact contact)
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

            PopuplateResponsiblePartyFromSimpleContact(responsibleParty, contact);
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
            CI_ResponsibleParty_Type responsibleParty = GetContactInformationResponsiblePartyWithRole(roleCodeValue);
            if (responsibleParty != null)
            {
                contact = ParseResponsiblePartyToSimpleContact(responsibleParty);
            }
            return contact;
        }

        private CI_ResponsibleParty_Type CreateResponsiblePartyFromSimpleContact(SimpleContact contact)
        {
            CI_ResponsibleParty_Type responsibleParty = new CI_ResponsibleParty_Type();
            PopuplateResponsiblePartyFromSimpleContact(responsibleParty, contact);
            return responsibleParty;
        }

        private CI_ResponsibleParty_Type PopuplateResponsiblePartyFromSimpleContact(CI_ResponsibleParty_Type responsibleParty, SimpleContact contact)
        {
            responsibleParty.individualName = new CharacterString_PropertyType { CharacterString = contact.Name };

            if (!string.IsNullOrWhiteSpace(contact.OrganizationEnglish))
            {
                responsibleParty.organisationName = CreateFreeTextElement(contact.Organization, contact.OrganizationEnglish);
            }
            else
            {
                responsibleParty.organisationName = new CharacterString_PropertyType { CharacterString = contact.Organization };
            }
            
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
            return responsibleParty;
        }

        private SimpleContact ParseResponsiblePartyToSimpleContact(CI_ResponsibleParty_Type responsibleParty)
        {
            string email = null;
            if (responsibleParty.contactInfo != null && responsibleParty.contactInfo.CI_Contact != null && responsibleParty.contactInfo.CI_Contact.address != null 
                && responsibleParty.contactInfo.CI_Contact.address.CI_Address != null
                && responsibleParty.contactInfo.CI_Contact.address.CI_Address.electronicMailAddress != null
                && responsibleParty.contactInfo.CI_Contact.address.CI_Address.electronicMailAddress[0] != null
                && responsibleParty.contactInfo.CI_Contact.address.CI_Address.electronicMailAddress[0].CharacterString != null)
            {
                email = responsibleParty.contactInfo.CI_Contact.address.CI_Address.electronicMailAddress[0].CharacterString;
            }

            string role = null;
            if (responsibleParty.role != null && responsibleParty.role.CI_RoleCode != null)
            {
                role = responsibleParty.role.CI_RoleCode.codeListValue;
            }

            return new SimpleContact
            {
                Name = GetStringOrNull(responsibleParty.individualName),
                Organization = GetStringOrNull(responsibleParty.organisationName),
                OrganizationEnglish = GetEnglishValueFromFreeText(responsibleParty.organisationName),
                Email = email,
                Role = role
            };
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
                                string keywordEnglishValue = GetEnglishValueFromFreeText(keywordElement);
                                if (!string.IsNullOrWhiteSpace(keywordValue))
                                {
                                    keywords.Add(new SimpleKeyword
                                    {
                                        Keyword = keywordValue,
                                        Thesaurus = thesaurus,
                                        Type = type,
                                        EnglishKeyword = keywordEnglishValue,
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
                var identification = GetIdentification();
                if (identification != null)
                {
                    List<MD_Keywords_PropertyType> allKeywords = new List<MD_Keywords_PropertyType>();
                    
                    Dictionary<string, bool> processed = new Dictionary<string, bool>();
                    foreach (var simpleKeyword in value)
                    {
                        if (!processed.ContainsKey(createKeywordKey(simpleKeyword.Keyword, simpleKeyword)))
                        {
                            List<SimpleKeyword> filteredKeywords = SimpleKeyword.Filter(value, simpleKeyword.Type, simpleKeyword.Thesaurus);
                            List<CharacterString_PropertyType> keywordsToAdd = new List<CharacterString_PropertyType>();
                            foreach (var fk in filteredKeywords)
                            {
                                string key = createKeywordKey(fk.Keyword, simpleKeyword);
                                if (!processed.ContainsKey(key))
                                {
                                    processed.Add(key, true);

                                    if (!string.IsNullOrWhiteSpace(fk.EnglishKeyword))
                                    {
                                        keywordsToAdd.Add(CreateFreeTextElement(fk.Keyword, fk.EnglishKeyword));
                                    }
                                    else
                                    {
                                        keywordsToAdd.Add(new CharacterString_PropertyType { CharacterString = fk.Keyword });
                                    }                                    
                                }
                            }

                            CI_Citation_PropertyType thesaurus = null; 
                            if (!string.IsNullOrWhiteSpace(simpleKeyword.Thesaurus)) {

                                string date = "";
                                if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE))
                                {
                                    date = "2014-03-20";
                                }
                                else if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_SERVICES_TAXONOMY))
                                {
                                    date = "2010-01-19";
                                }
                                else if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1))
                                {
                                    date = "2008-06-01";
                                }

                                thesaurus = new CI_Citation_PropertyType { 
                                    CI_Citation = new CI_Citation_Type { 
                                        title = toCharString(simpleKeyword.Thesaurus),
                                        date = new CI_Date_PropertyType[] {
                                            new CI_Date_PropertyType {
                                                CI_Date = new CI_Date_Type {
                                                    date = new Date_PropertyType { Item = date },
                                                    dateType = new CI_DateTypeCode_PropertyType { 
                                                        CI_DateTypeCode = new CodeListValue_Type {
                                                            codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#CI_DateTypeCode",
                                                            codeListValue = "publication"
                                                        }
                                                    }                                
                                                }
                                            }
                                            
                                        }
                                    }
                                };
                            }

                            MD_KeywordTypeCode_PropertyType keywordType = null;
                            if (!string.IsNullOrWhiteSpace(simpleKeyword.Type))
                            {
                                keywordType = new MD_KeywordTypeCode_PropertyType
                                {
                                    MD_KeywordTypeCode = new CodeListValue_Type
                                    {
                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_KeywordTypeCode",
                                        codeListValue = simpleKeyword.Type
                                    }
                                };
                            }

                            allKeywords.Add(new MD_Keywords_PropertyType
                            {
                                MD_Keywords = new MD_Keywords_Type
                                {
                                    thesaurusName = thesaurus,
                                    type = keywordType,                                    
                                    keyword = keywordsToAdd.ToArray()
                                }
                            });

                        }

                    }

                    identification.descriptiveKeywords = allKeywords.ToArray();

                }

            }
        }

        private string createKeywordKey(string keyword, SimpleKeyword simpleKeyword)
        {
            return keyword + "_" + simpleKeyword.Type + "_" + simpleKeyword.Thesaurus;
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
                    identification.topicCategory = new MD_TopicCategoryCode_PropertyType[] {
                        new MD_TopicCategoryCode_PropertyType
                        {
                            MD_TopicCategoryCode = (MD_TopicCategoryCode_Type)Enum.Parse(typeof(MD_TopicCategoryCode_Type), value, true)
                        }
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
                        string type = browseGraphic.MD_BrowseGraphic.fileDescription != null ? browseGraphic.MD_BrowseGraphic.fileDescription.CharacterString : null;
                        string url = browseGraphic.MD_BrowseGraphic.fileName != null ? browseGraphic.MD_BrowseGraphic.fileName.CharacterString : null;

                        thumbnails.Add(new SimpleThumbnail
                        {
                            Type = type,
                            URL = url
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
                                    fileDescription = new CharacterString_PropertyType { CharacterString = thumbnail.Type }
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
                    datasetIdentification.spatialRepresentationType = new MD_SpatialRepresentationTypeCode_PropertyType[] {
                        new MD_SpatialRepresentationTypeCode_PropertyType {
                            MD_SpatialRepresentationTypeCode = new CodeListValue_Type {
                                codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/Codelist/ML_gmxCodelists.xml#MD_SpatialRepresentationTypeCode",
                                codeListValue = value
                            }
                        }
                    };
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
                    && _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource != null)
                {
                    var resource = _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource;
                    value = new SimpleDistributionDetails
                    {
                        URL = resource.linkage != null ? resource.linkage.URL : null,
                        Protocol = resource.protocol != null ? resource.protocol.CharacterString : null,
                        Name = resource.name != null ? resource.name.CharacterString : null
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

                URL_PropertyType url = null;
                if (!string.IsNullOrWhiteSpace(value.URL))
                    url = new URL_PropertyType { URL = value.URL };

                CharacterString_PropertyType protocol = null;
                if (!string.IsNullOrWhiteSpace(value.Protocol))
                    protocol = new CharacterString_PropertyType { CharacterString = value.Protocol };

                CharacterString_PropertyType name = null;
                if (!string.IsNullOrWhiteSpace(value.Name))
                    name = new CharacterString_PropertyType { CharacterString = value.Name };

                _md.distributionInfo.MD_Distribution.transferOptions = new MD_DigitalTransferOptions_PropertyType[] {
                    new MD_DigitalTransferOptions_PropertyType {
                        MD_DigitalTransferOptions = new MD_DigitalTransferOptions_Type {
                            onLine = new CI_OnlineResource_PropertyType[] {
                                new CI_OnlineResource_PropertyType {
                                    CI_OnlineResource = new CI_OnlineResource_Type {
                                        linkage = url,
                                        protocol = protocol,
                                        name = name
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
        /// Values from codelist: http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_MaintenanceFrequencyCode
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
                identification.resourceMaintenance = new MD_MaintenanceInformation_PropertyType[] {
                    new MD_MaintenanceInformation_PropertyType {
                        MD_MaintenanceInformation = new MD_MaintenanceInformation_Type {
                            maintenanceAndUpdateFrequency = new MD_MaintenanceFrequencyCode_PropertyType {
                                MD_MaintenanceFrequencyCode = new CodeListValue_Type {
                                    codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_MaintenanceFrequencyCode",
                                    codeListValue = value
                                }
                            }
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Values from codelist: http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ProgressCode
        /// </summary>
        public string Status
        {
            get
            {
                string value = null;

                var identification = GetIdentification();
                if (identification != null
                    && identification.status != null
                    && identification.status.Length > 0
                    && identification.status[0] != null
                    && identification.status[0].MD_ProgressCode != null)
                {
                    value = identification.status[0].MD_ProgressCode.codeListValue;
                }

                return value;
            }

            set
            {
                var identification = GetIdentificationNotNull();
                identification.status = new MD_ProgressCode_PropertyType[] { 
                    new MD_ProgressCode_PropertyType {
                        MD_ProgressCode = new CodeListValue_Type {
                            codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ProgressCode",
                            codeListValue = value
                        }
                    }
                };
            }
        }

        public SimpleQualitySpecification QualitySpecification
        {
            get
            {
                SimpleQualitySpecification value = null;
                
                if (_md.dataQualityInfo != null
                    && _md.dataQualityInfo.Length > 0
                    && _md.dataQualityInfo[0] != null
                    && _md.dataQualityInfo[0].DQ_DataQuality != null
                    && _md.dataQualityInfo[0].DQ_DataQuality.report != null
                    && _md.dataQualityInfo[0].DQ_DataQuality.report.Length > 0
                    && _md.dataQualityInfo[0].DQ_DataQuality.report[0] != null
                    && _md.dataQualityInfo[0].DQ_DataQuality.report[0].AbstractDQ_Element != null)
                {

                    DQ_DomainConsistency_Type domainConsistency = _md.dataQualityInfo[0].DQ_DataQuality.report[0].AbstractDQ_Element as DQ_DomainConsistency_Type;
                    if (domainConsistency != null
                        && domainConsistency.result != null
                        && domainConsistency.result.Length > 0
                        && domainConsistency.result[0] != null
                        && domainConsistency.result[0].AbstractDQ_Result != null)
                    {
                        DQ_ConformanceResult_Type result = domainConsistency.result[0].AbstractDQ_Result as DQ_ConformanceResult_Type;
                        if (result != null) {
                            value = new SimpleQualitySpecification();

                            if (result.specification != null
                            && result.specification.CI_Citation != null) {


                                // title
                                if (result.specification.CI_Citation.title != null) {
                                    value.Title = result.specification.CI_Citation.title.CharacterString;
                                }


                                // date and datetype
                                if (result.specification.CI_Citation.date != null
                                    && result.specification.CI_Citation.date.Length > 0 
                                    && result.specification.CI_Citation.date[0] != null
                                    && result.specification.CI_Citation.date[0].CI_Date != null) {

                                        CI_Date_Type dateType = result.specification.CI_Citation.date[0].CI_Date;

                                        if (dateType.date != null && dateType.date.Item != null)
                                        {
                                            string date = dateType.date.Item as string;
                                            if (date != null)
                                            {
                                                value.Date = date;
                                            }
                                        }

                                        if (dateType.dateType != null && dateType.dateType.CI_DateTypeCode != null) {
                                            value.DateType = dateType.dateType.CI_DateTypeCode.codeListValue;
                                        }
                                }

                                // explanation
                                if (result.explanation != null)
                                {
                                    value.Explanation = result.explanation.CharacterString;
                                }

                                // result
                                if (result.pass != null)
                                {
                                    value.Result = result.pass.Boolean;
                                }
                            }                         
                        }
                    }
                }
                return value;
            }

            set
            {
                DQ_Element_PropertyType[] reports = new DQ_Element_PropertyType[] {
                    new DQ_Element_PropertyType {
                        AbstractDQ_Element = new DQ_DomainConsistency_Type {
                            result = new DQ_Result_PropertyType[] {
                                new DQ_Result_PropertyType {
                                    AbstractDQ_Result = new DQ_ConformanceResult_Type {
                                        specification = new CI_Citation_PropertyType {
                                            CI_Citation = new CI_Citation_Type {
                                                title = toCharString(value.Title),
                                                date = new CI_Date_PropertyType[] {
                                                    new CI_Date_PropertyType {
                                                        CI_Date = new CI_Date_Type {
                                                            date = new Date_PropertyType {
                                                                Item = value.Date  
                                                            },
                                                            dateType = new CI_DateTypeCode_PropertyType {
                                                                CI_DateTypeCode = new CodeListValue_Type {
                                                                    codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#CI_DateTypeCode",
                                                                    codeListValue = value.DateType
                                                                }    
                                                            }
                                                        }
                                                    }
                                                }
                                            }    
                                        },
                                        explanation = toCharString(value.Explanation),
                                        pass = new Boolean_PropertyType { Boolean = value.Result }
                                    }
                                }
                            }
                        }
                    }
                };


                if (_md.dataQualityInfo == null || _md.dataQualityInfo.Length == 0 || _md.dataQualityInfo[0] == null || _md.dataQualityInfo[0].DQ_DataQuality == null)
                {
                    _md.dataQualityInfo = new DQ_DataQuality_PropertyType[] {
                        new DQ_DataQuality_PropertyType {
                            DQ_DataQuality = new DQ_DataQuality_Type {
                                report = reports
                            }
                        }
                    };
                }
                else
                {
                    _md.dataQualityInfo[0].DQ_DataQuality.report = reports;
                }
            }
        }

        public string ProcessHistory 
        {
            get
            {
                string value = null;
                if (_md.dataQualityInfo != null 
                    && _md.dataQualityInfo.Length > 0 
                    && _md.dataQualityInfo[0] != null
                    && _md.dataQualityInfo[0].DQ_DataQuality != null
                    && _md.dataQualityInfo[0].DQ_DataQuality.lineage != null
                    && _md.dataQualityInfo[0].DQ_DataQuality.lineage.LI_Lineage != null
                    && _md.dataQualityInfo[0].DQ_DataQuality.lineage.LI_Lineage.statement != null)
                {
                    value = _md.dataQualityInfo[0].DQ_DataQuality.lineage.LI_Lineage.statement.CharacterString;
                }

                return value;
            }

            set
            {
                if (_md.dataQualityInfo == null || _md.dataQualityInfo.Length == 0 || _md.dataQualityInfo[0] == null || _md.dataQualityInfo[0].DQ_DataQuality == null)
                {
                    _md.dataQualityInfo = new DQ_DataQuality_PropertyType[] {
                        new DQ_DataQuality_PropertyType {
                            DQ_DataQuality = new DQ_DataQuality_Type {
                            }
                        }
                    };
                }

                _md.dataQualityInfo[0].DQ_DataQuality.lineage = new LI_Lineage_PropertyType
                {
                    LI_Lineage = new LI_Lineage_Type
                    {
                        statement = toCharString(value)
                    }
                };

            }
        }

        public DateTime? DateCreated 
        {
            get
            {
                DateTime? value = null;
                var citation = GetIdentificationCitation();
                if (citation != null)
                {
                    value = GetDateFromCitationWithType(citation, "creation");
                }
                return value;
            }

            set
            {
                var citation = GetIdentificationCitation();
                if (citation != null)
                {
                    UpdateCitationDateForType(citation, "creation", value);
                }
            }
        }

       

        public DateTime? DatePublished
        {
            get
            {
                DateTime? value = null;
                var citation = GetIdentificationCitation();
                if (citation != null)
                {
                    value = GetDateFromCitationWithType(citation, "publication");
                }
                return value;
            }

            set
            {
                var citation = GetIdentificationCitation();
                if (citation != null)
                {
                    UpdateCitationDateForType(citation, "publication", value);
                }
            }
        }

        public DateTime? DateUpdated
        {
            get
            {
                DateTime? value = null;
                var citation = GetIdentificationCitation();
                if (citation != null)
                {
                    value = GetDateFromCitationWithType(citation, "revision");
                }
                return value;
            }

            set
            {
                var citation = GetIdentificationCitation();
                if (citation != null)
                {
                    UpdateCitationDateForType(citation, "revision", value);
                }
            }
        }

        public DateTime? DateMetadataUpdated
        {
            get
            {
                DateTime? value = null;
                if (_md.dateStamp != null && _md.dateStamp.Item != null)
                {
                    value = ParseDateProperty(_md.dateStamp);
                }
                return value;
            }

            set
            {
                if (value.HasValue)
                {
                    _md.dateStamp = new Date_PropertyType { Item = value.Value.ToString("yyyy-MM-dd") };
                }
            }
        }

        private CI_Citation_Type GetIdentificationCitation()
        {
            CI_Citation_Type citation = null;
            var identification = GetIdentification();
            if (identification != null && identification.citation != null && identification.citation.CI_Citation != null)
            {
                citation = identification.citation.CI_Citation;
            }
            return citation;
        }

        private DateTime? GetDateFromCitationWithType(CI_Citation_Type citation, string dateType)
        {
            DateTime? date = null;
            if (citation.date != null)
            {
                foreach (var currentDate in citation.date)
                {
                    if (currentDate.CI_Date != null
                        && currentDate.CI_Date.dateType != null
                        && currentDate.CI_Date.dateType.CI_DateTypeCode != null
                        && currentDate.CI_Date.dateType.CI_DateTypeCode.codeListValue == dateType
                        && currentDate.CI_Date.date != null
                        && currentDate.CI_Date.date.Item != null)
                    {
                        date = ParseDateProperty(currentDate.CI_Date.date);
                        break;
                    }
                }
            }            
            return date;
        }


        private DateTime? ParseDateProperty(Date_PropertyType dateProperty)
        {
            DateTime? date = dateProperty.Item as DateTime?;

            if (date == null)
            {
                string dateString = dateProperty.Item as string;
                if (dateString != null)
                {
                    date = DateTime.Parse(dateString);
                }
            }

            return date;
        }

        private void UpdateCitationDateForType(CI_Citation_Type citation, string dateType, DateTime? incomingDateTime)
        {
            
            bool updated = false;
            string updatedValue = null;
            if (incomingDateTime.HasValue)
            {
                updatedValue = incomingDateTime.Value.ToString("yyyy-MM-dd");
            }

            if (citation.date != null)
            {
                foreach (var currentDate in citation.date)
                {
                    if (currentDate.CI_Date != null
                        && currentDate.CI_Date.dateType != null
                        && currentDate.CI_Date.dateType.CI_DateTypeCode != null
                        && currentDate.CI_Date.dateType.CI_DateTypeCode.codeListValue == dateType)
                    {
                        currentDate.CI_Date.date.Item = updatedValue;
                        updated = true;
                    }
                }
            } else {
                citation.date = new CI_Date_PropertyType[0];
            }

            if (!updated)
            {
                CI_Date_PropertyType[] newArray = new CI_Date_PropertyType[] {
                    new CI_Date_PropertyType {
                        CI_Date = new CI_Date_Type {
                            date = new Date_PropertyType {
                                Item = updatedValue
                            },
                            dateType = new CI_DateTypeCode_PropertyType {
                                CI_DateTypeCode = new CodeListValue_Type {
                                    codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#CI_DateTypeCode",
                                    codeListValue = dateType
                                }    
                            }
                        }
                    }
                };
                citation.date = citation.date.Concat(newArray).ToArray();
            }
            
        }

        public string MetadataLanguage
        {
            get
            {
                string value = null;
                if (_md.language != null)
                {
                    value = _md.language.CharacterString;
                }

                return value;
            }

            set
            {
                _md.language = toCharString(value);
            }
        }

        public string MetadataStandard
        {
            get
            {
                string value = null;
                if (_md.metadataStandardName != null)
                {
                    value = _md.metadataStandardName.CharacterString;
                }
                return value;
            }

            set
            {
                _md.metadataStandardName = toCharString(value);
            }
        }

        public string MetadataStandardVersion
        {
            get
            {
                string value = null;
                if (_md.metadataStandardVersion != null)
                {
                    value = _md.metadataStandardVersion.CharacterString;
                }
                return value;
            }

            set
            {
                _md.metadataStandardVersion = toCharString(value);
            }
        }

        public SimpleBoundingBox BoundingBox
        {
            get
            {
                SimpleBoundingBox value = null;


                EX_Extent_PropertyType[] extents = GetIdentificationExtents();
                if (extents != null)
                {
                    foreach (EX_Extent_PropertyType extent in extents)
                    {
                        if (extent.EX_Extent != null 
                            && extent.EX_Extent.geographicElement != null
                            && extent.EX_Extent.geographicElement[0] != null
                            && extent.EX_Extent.geographicElement[0].AbstractEX_GeographicExtent != null)
                        {
                            EX_GeographicBoundingBox_Type exBoundingBox = extent.EX_Extent.geographicElement[0].AbstractEX_GeographicExtent as EX_GeographicBoundingBox_Type;
                            if (exBoundingBox != null)
                            {
                                value = new SimpleBoundingBox
                                {
                                    EastBoundLongitude = exBoundingBox.eastBoundLongitude != null ? exBoundingBox.eastBoundLongitude.Decimal.ToString() : null,
                                    WestBoundLongitude = exBoundingBox.westBoundLongitude != null ? exBoundingBox.westBoundLongitude.Decimal.ToString() : null,
                                    NorthBoundLatitude = exBoundingBox.northBoundLatitude != null ? exBoundingBox.northBoundLatitude.Decimal.ToString() : null,
                                    SouthBoundLatitude = exBoundingBox.southBoundLatitude != null ? exBoundingBox.southBoundLatitude.Decimal.ToString() : null
                                };
                                break;
                            }
                        }
                    }
                }
                return value;
            }

            set
            {
                EX_GeographicExtent_PropertyType[] geographicElement = new EX_GeographicExtent_PropertyType[] 
                {
                    new EX_GeographicExtent_PropertyType
                    {
                        AbstractEX_GeographicExtent = new EX_GeographicBoundingBox_Type
                        {
                            eastBoundLongitude = DecimalFromString(value.EastBoundLongitude),
                            westBoundLongitude = DecimalFromString(value.WestBoundLongitude),
                            northBoundLatitude = DecimalFromString(value.NorthBoundLatitude),
                            southBoundLatitude = DecimalFromString(value.SouthBoundLatitude),
                        }
                    }
                };
                CharacterString_PropertyType description = null;
                EX_TemporalExtent_PropertyType[] temporalElement = null;
                EX_VerticalExtent_PropertyType[] verticalElement = null;

                bool foundDescription = false;
                bool foundTemporalExtent = false;
                bool foundVerticalExtent = false;

                EX_Extent_PropertyType[] extents = GetIdentificationExtents();
                if (extents != null)
                {
                    foreach (var extent in extents)
                    {
                        if (extent.EX_Extent != null)
                        {
                            if (!foundDescription && extent.EX_Extent.description != null && !string.IsNullOrWhiteSpace(extent.EX_Extent.description.CharacterString))
                            {
                                description = extent.EX_Extent.description;
                                foundDescription = true;
                            }
                            if (!foundTemporalExtent && extent.EX_Extent.temporalElement != null)
                            {
                                temporalElement = extent.EX_Extent.temporalElement;
                                foundTemporalExtent = true;
                            }
                            if (!foundVerticalExtent && extent.EX_Extent.verticalElement != null)
                            {
                                verticalElement = extent.EX_Extent.verticalElement;
                                foundVerticalExtent = true;
                            }
                        }
                    }
                }

                EX_Extent_Type newExtent = CreateExtent();

                newExtent.geographicElement = geographicElement;
                newExtent.description = description;
                newExtent.temporalElement = temporalElement;
                newExtent.verticalElement = verticalElement;
            }
        }

        private EX_Extent_Type CreateExtent()
        {
            EX_Extent_Type currentExtent = null;
            if (IsService())
            {
                SV_ServiceIdentification_Type identification = GetServiceIdentification();
                if (identification != null)
                {
                    currentExtent = new EX_Extent_Type();
                    identification.extent = new EX_Extent_PropertyType[] {
                                new EX_Extent_PropertyType {
                                    EX_Extent = currentExtent
                                }
                            };
                }
                
            }
            else
            {
                MD_DataIdentification_Type identification = GetDatasetIdentification();
                if (identification != null)
                {
                    currentExtent = new EX_Extent_Type();
                    identification.extent = new EX_Extent_PropertyType[] {
                                new EX_Extent_PropertyType {
                                    EX_Extent = currentExtent
                                }
                            };
                }
            }
            return currentExtent;
        }

        private EX_Extent_PropertyType[] GetIdentificationExtents()
        {
            EX_Extent_PropertyType[] extent = null;
            if (IsService())
            {
                var identification = GetServiceIdentification();
                if (identification != null
                    && identification.extent != null
                    && identification.extent.Length > 0
                    )
                {
                    extent = identification.extent;
                }
            }
            else
            {
                var identification = GetDatasetIdentification();
                if (identification != null
                    && identification.extent != null
                    && identification.extent.Length > 0)
                {
                    extent = identification.extent;
                }
            }
            return extent;
        }

        public SimpleConstraints Constraints
        {
            get
            {
                SimpleConstraints value = null;
                var identification = GetIdentification();

                if (identification != null
                    && identification.resourceConstraints != null
                    && identification.resourceConstraints.Length > 0)
                {

                    value = new SimpleConstraints();

                    foreach (MD_Constraints_PropertyType constraintProperty in identification.resourceConstraints)
                    {
                        if (constraintProperty.MD_Constraints != null)
                        {
                            MD_SecurityConstraints_Type securityConstraint = constraintProperty.MD_Constraints as MD_SecurityConstraints_Type;
                            if (securityConstraint != null
                                && securityConstraint.classification != null
                                && securityConstraint.classification.MD_ClassificationCode != null)
                            {
                                value.SecurityConstraints = securityConstraint.classification.MD_ClassificationCode.codeListValue;
                            }
                            else
                            {
                                MD_LegalConstraints_Type legalConstraint = constraintProperty.MD_Constraints as MD_LegalConstraints_Type;
                                if (legalConstraint != null) {

                                    if (legalConstraint.accessConstraints != null
                                    && legalConstraint.accessConstraints.Length > 0
                                    && legalConstraint.accessConstraints[0] != null
                                    && legalConstraint.accessConstraints[0].MD_RestrictionCode != null)
                                    {
                                        value.AccessConstraints = legalConstraint.accessConstraints[0].MD_RestrictionCode.codeListValue;
                                    }

                                    if (legalConstraint.otherConstraints != null
                                        && legalConstraint.otherConstraints.Length > 0
                                        && legalConstraint.otherConstraints[0] != null)
                                    {
                                        value.OtherConstraints = legalConstraint.otherConstraints[0].CharacterString;
                                    }

                                    if (legalConstraint.useConstraints != null
                                        && legalConstraint.useConstraints.Length > 0
                                        && legalConstraint.useConstraints[0] != null
                                        && legalConstraint.useConstraints[0].MD_RestrictionCode != null)
                                    {
                                        value.UseConstraints = legalConstraint.useConstraints[0].MD_RestrictionCode.codeListValue;
                                    }

                                }
                                    
                                else
                                {
                                    MD_Constraints_Type regularConstraint = constraintProperty.MD_Constraints as MD_Constraints_Type;
                                    if (regularConstraint != null 
                                        && regularConstraint.useLimitation != null
                                        && regularConstraint.useLimitation.Length > 0
                                        && regularConstraint.useLimitation[0] != null)
                                    {
                                        
                                        value.UseLimitations = regularConstraint.useLimitation[0].CharacterString;
                                    }
                                }
                            }

                        }
                    }

                }
                return value;
            }

            set
            {

                MD_Constraints_PropertyType[] resourceConstraints = new MD_Constraints_PropertyType[] {
                    new MD_Constraints_PropertyType {
                        MD_Constraints = new MD_Constraints_Type {
                            useLimitation = new CharacterString_PropertyType[] { new CharacterString_PropertyType { CharacterString = value.UseLimitations }}
                        }    
                    },
                    new MD_Constraints_PropertyType {
                        MD_Constraints = new MD_LegalConstraints_Type {
                            accessConstraints = new MD_RestrictionCode_PropertyType[] { 
                                new MD_RestrictionCode_PropertyType {
                                    MD_RestrictionCode = new CodeListValue_Type {
                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_RestrictionCode",
                                        codeListValue = value.AccessConstraints
                                    }    
                                }
                            },
                            useConstraints = new MD_RestrictionCode_PropertyType[] { 
                                new MD_RestrictionCode_PropertyType {
                                    MD_RestrictionCode = new CodeListValue_Type {
                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_RestrictionCode",
                                        codeListValue = value.UseConstraints
                                    }    
                                }
                            },
                            otherConstraints = new CharacterString_PropertyType[] { new CharacterString_PropertyType { CharacterString = value.OtherConstraints }}
                        }    
                    },
                    new MD_Constraints_PropertyType {
                        MD_Constraints = new MD_SecurityConstraints_Type {
                            classification = new MD_ClassificationCode_PropertyType {
                                MD_ClassificationCode = new CodeListValue_Type {
                                    codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ClassificationCode",
                                    codeListValue = value.SecurityConstraints
                                }    
                            }
                        }    
                    }
                };

                var identification = GetIdentification();
                if (identification != null)
                {
                    identification.resourceConstraints = resourceConstraints;                    
                }                
            }
        }

        public List<string> OperatesOn
        {
            get { 
                List<string> values = new List<string>();
                var identification = GetServiceIdentification();

                if (identification != null && identification.operatesOn != null && identification.operatesOn.Length > 0)
                {
                    foreach (var element in identification.operatesOn)
                    {
                        if (!string.IsNullOrWhiteSpace(element.uuidref))
                            values.Add(element.uuidref);
                    }
                }
                return values;
            }
            set {
                var identification = GetServiceIdentification();
                if (identification != null)
                {
                    List<MD_DataIdentification_PropertyType> operatesOn = new List<MD_DataIdentification_PropertyType>();
                    foreach(string uuid in value) {
                        operatesOn.Add(new MD_DataIdentification_PropertyType {
                            uuidref = uuid
                        });
                    }

                    identification.operatesOn = operatesOn.ToArray();
                }
            }
        }
       
        public bool IsDataset()
        {
            return HierarchyLevel.Equals("dataset", StringComparison.Ordinal);
        }

        public bool IsService()
        {
            return HierarchyLevel.Equals("service", StringComparison.Ordinal);
        }

        public void SetLocale(string locale)
        {
            _md.locale = new PT_Locale_PropertyType[] {
                new PT_Locale_PropertyType
                {
                    PT_Locale = new PT_Locale_Type
                    {
                        id = locale,
                        languageCode = new LanguageCode_PropertyType
                        {
                            LanguageCode = new CodeListValue_Type
                            {
                                codeList = "http://www.loc.gov/standards/iso639-2/",
                                codeListValue = locale.ToLower()
                            }
                        },
                        characterEncoding = new MD_CharacterSetCode_PropertyType
                        {
                            MD_CharacterSetCode = new CodeListValue_Type
                            {
                                codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/ML_gmxCodelists.xml#MD_CharacterSetCode",
                                codeListValue = "utf8"
                            }
                        }
                    }
                }
            };
        }

        public string SpecificUsage 
        {
            get
            {
                string value = null;
                var identification = GetIdentification();
                if (identification != null && identification.resourceSpecificUsage != null && identification.resourceSpecificUsage.Length > 0
                    && identification.resourceSpecificUsage[0] != null && identification.resourceSpecificUsage[0].MD_Usage != null
                    && identification.resourceSpecificUsage[0].MD_Usage.specificUsage != null)
                {
                    value = identification.resourceSpecificUsage[0].MD_Usage.specificUsage.CharacterString;
                }
                return value;
            }

            set
            {
                var identification = GetIdentification();
                if (identification != null)
                {
                    identification.resourceSpecificUsage = new MD_Usage_PropertyType[]
                    {
                        new MD_Usage_PropertyType {
                            MD_Usage = new MD_Usage_Type
                            {
                                specificUsage = new CharacterString_PropertyType { CharacterString = value }
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

        internal Decimal_PropertyType DecimalFromString(string input)
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
            if (input.Contains(","))
            {
                cultureInfo = new CultureInfo("no");
            }

            return new Decimal_PropertyType { Decimal = Decimal.Parse(input, NumberStyles.Any, cultureInfo) };
        }


        
    }

    public class SimpleContact
    {
        public string Name { get; set; }
        public string Organization { get; set; }
        public string OrganizationEnglish { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class SimpleKeyword
    {
        public const string THESAURUS_GEMET_INSPIRE_V1 = "GEMET - INSPIRE themes, version 1.0";
        public const string THESAURUS_NATIONAL_INITIATIVE = "Nasjonal inndeling i geografiske initiativ og SDI-er";
        public const string THESAURUS_SERVICES_TAXONOMY = "ISO - 19119 geographic services taxonomy";
        public const string TYPE_PLACE = "place";
        public const string TYPE_THEME = "theme";

        public string Keyword { get; set; }
        public string Type { get; set; }
        public string Thesaurus { get; set; }
        public string EnglishKeyword { get; set; }

        public static List<SimpleKeyword> Filter(List<SimpleKeyword> input, string type, string thesaurus)
        {
            List<SimpleKeyword> filteredList = new List<SimpleKeyword>();

            bool filterOnType = !string.IsNullOrWhiteSpace(type);
            bool filterOnThesaurus = !string.IsNullOrWhiteSpace(thesaurus);

            foreach (SimpleKeyword simpleKeyword in input)
            {
                if (filterOnType && !string.IsNullOrWhiteSpace(simpleKeyword.Type) && simpleKeyword.Type.Equals(type))
                {
                    filteredList.Add(simpleKeyword);
                }
                else if (filterOnThesaurus && !string.IsNullOrWhiteSpace(simpleKeyword.Thesaurus) && simpleKeyword.Thesaurus.Equals(thesaurus))
                {
                    filteredList.Add(simpleKeyword);
                }
                else if (!filterOnType && string.IsNullOrWhiteSpace(simpleKeyword.Type) && !filterOnThesaurus && string.IsNullOrWhiteSpace(simpleKeyword.Thesaurus))
                {
                    filteredList.Add(simpleKeyword);
                }
            }
            return filteredList;
        }

        public string GetPrefix()
        {
            if (!string.IsNullOrWhiteSpace(Type))
            {
                if (Type.Equals(TYPE_PLACE))
                {
                    return "Place";
                }
                else if (Type.Equals(TYPE_THEME))
                {
                    return "Theme";
                }
            }
            else if (!string.IsNullOrWhiteSpace(Thesaurus))
            {
                if (Thesaurus.Equals(THESAURUS_GEMET_INSPIRE_V1))
                {
                    return "Inspire";
                }
                else if (Thesaurus.Equals(THESAURUS_NATIONAL_INITIATIVE))
                {
                    return "NationalInitiative";
                }
                else if (Thesaurus.Equals(THESAURUS_SERVICES_TAXONOMY))
                {
                    return "Service";
                }
            }
            return "Other";
        }
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
        public string Name { get; set; }
    }

    public class SimpleQualitySpecification
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string DateType { get; set; }
        public string Explanation { get; set; }
        public bool Result { get; set; }
    }

    public class SimpleBoundingBox
    {
        public string EastBoundLongitude { get; set; }
        public string WestBoundLongitude { get; set; }
        public string NorthBoundLatitude { get; set; }
        public string SouthBoundLatitude { get; set; }
    }

    public class SimpleConstraints
    {
        public string UseLimitations { get; set; }
        public string AccessConstraints { get; set; }
        public string UseConstraints { get; set; }
        public string OtherConstraints { get; set; }
        public string SecurityConstraints { get; set; }
    }
}
