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

        public const string LOCALE_NOR = "NOR";
        public const string LOCALE_LINK_NOR = "#" + LOCALE_NOR;

        public const string METADATA_LANG_NOR = "nor";

        private const string APPLICATION_PROFILE_PRODUCTSPEC = "produktspesifikasjon";
        private const string APPLICATION_PROFILE_PRODUCTSPEC_OTHER = "annen produktspesifikasjon";
        private const string APPLICATION_PROFILE_PRODUCTSHEET = "produktark";
        private const string APPLICATION_PROFILE_LEGEND = "tegnforklaring";
        private const string APPLICATION_PROFILE_PRODUCTPAGE = "produktside";
        private const string APPLICATION_PROFILE_COVERAGE = "dekningsoversikt";
        private const string APPLICATION_PROFILE_COVERAGE_GRID = "dekningsoversikt rutenett";
        private const string APPLICATION_PROFILE_COVERAGE_CELL = "dekningsoversikt celle";
        private const string APPLICATION_PROFILE_HELP = "hjelp";
        private const string RESOURCE_PROTOCOL_WWW = "WWW:LINK-1.0-http--related";

        private const string ENGLISH_APPLICATION_PROFILE_PRODUCTSPEC = "data product specification";
        private const string ENGLISH_APPLICATION_PROFILE_PRODUCTSHEET = "product sheet";
        private const string ENGLISH_APPLICATION_PROFILE_LEGEND = "cartography";
        private const string ENGLISH_APPLICATION_PROFILE_PRODUCTPAGE = "website";
        private const string ENGLISH_APPLICATION_PROFILE_COVERAGE = "coverage map";
        private const string ENGLISH_APPLICATION_PROFILE_COVERAGE_GRID = "grid coverage map";
        private const string ENGLISH_APPLICATION_PROFILE_COVERAGE_CELL = "cell coverage map";
        private const string ENGLISH_APPLICATION_PROFILE_HELP = "help";

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

        public static SimpleMetadata CreateDataset(string fileIdentifier = null)
        {
            return CreateSimpleMetadata("dataset", new MD_DataIdentification_Type(), fileIdentifier);
        }

        private static SimpleMetadata CreateSimpleMetadata(string hierarchyLevel, AbstractMD_Identification_Type identification, string fileIdentifier = null)
        {
            if (string.IsNullOrEmpty(fileIdentifier))
                fileIdentifier = Guid.NewGuid().ToString();

            MD_Metadata_Type md = new MD_Metadata_Type
            {
                fileIdentifier = new CharacterString_PropertyType { CharacterString = fileIdentifier },
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

        public void RemoveUnnecessaryElements()
        {
            try
            {
                if(_md.identificationInfo != null && _md.identificationInfo.Length > 0
                    && _md.identificationInfo[0].AbstractMD_Identification != null 
                    && _md.identificationInfo[0].AbstractMD_Identification.citation != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.citedResponsibleParty != null)
                        _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.citedResponsibleParty = null;
            }
            catch (Exception ex) { }

            try
            {
                if (_md.identificationInfo != null && _md.identificationInfo.Length > 0
                    && _md.identificationInfo[0].AbstractMD_Identification != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.edition != null)
                        _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.edition = null;
            }
            catch (Exception ex) { }

            try
            {
                if (_md.identificationInfo != null && _md.identificationInfo.Length > 0
                    && _md.identificationInfo[0].AbstractMD_Identification != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.presentationForm != null)
                        _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.presentationForm = null;
            }
            catch (Exception ex) { }
            
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

                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    var norwegianTitle = GetNorwegianValueFromFreeText(GetTitleElement());
                    if (!string.IsNullOrEmpty(norwegianTitle))
                        title = norwegianTitle;
                }

                return title;
            }
            set {

                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    String existingLocalTitle = null;
                    CharacterString_PropertyType titleElement = GetTitleElement();
                    if (titleElement != null)
                    {
                        existingLocalTitle = titleElement.CharacterString;
                    }

                    var identification = GetIdentificationNotNull();
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
                        identification.citation.CI_Citation.title = new CI_Citation_Title();
                    }

                    identification.citation.CI_Citation.title.item = CreateFreeTextElementNorwegian(existingLocalTitle, value);
                }
                else
                {
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
        }

        private CharacterString_PropertyType GetTitleElement()
        {
            CharacterString_PropertyType title = null;
            var identification = GetIdentification();
            if (identification != null && identification.citation != null && identification.citation.CI_Citation != null && identification.citation.CI_Citation.title != null)
            {
                Anchor_Type titleObject = identification.citation.CI_Citation.title.item as Anchor_Type;
                if (titleObject != null)
                    title = toCharString(titleObject.Value);
                else
                    title = identification.citation.CI_Citation.title.item as CharacterString_PropertyType;
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
            if (identification.citation.CI_Citation.title == null)
            {
                identification.citation.CI_Citation.title = new CI_Citation_Title();
            }

            identification.citation.CI_Citation.title.item = element;
        }

        public string GetEnglishValueFromFreeText(CharacterString_PropertyType input)
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

        public string GetNorwegianValueFromFreeText(CharacterString_PropertyType input)
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
                            && localizedStringProperty.LocalisedCharacterString.locale.Equals(LOCALE_LINK_NOR))
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
                string title = null;
                CharacterString_PropertyType titleElement = GetTitleElement();
                if (titleElement != null)
                {
                    title = titleElement.CharacterString;
                }
                if (MetadataLanguage == METADATA_LANG_NOR)
                {
                    var englishTitle = GetEnglishValueFromFreeText(GetTitleElement());

                    if (!string.IsNullOrEmpty(englishTitle))
                        title = englishTitle;
                    else
                        return null;
                }

                return title;
            }

            set
            {
                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    PT_FreeText_PropertyType titleElementWithFreeText = GetTitleElement() as PT_FreeText_PropertyType;
                    if (titleElementWithFreeText != null)
                    {
                        titleElementWithFreeText.CharacterString = value;
                    }
                    else
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
                        if (identification.citation.CI_Citation.title == null)
                        {
                            identification.citation.CI_Citation.title = new CI_Citation_Title();
                        }

                        identification.citation.CI_Citation.title.item = new CharacterString_PropertyType { CharacterString = value };

                    }
                }
                else
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

        private PT_FreeText_PropertyType CreateFreeTextElementNorwegian(string characterString, string norwegianLocalizedValue)
        {
            return new PT_FreeText_PropertyType
            {
                CharacterString = characterString,
                PT_FreeText = new PT_FreeText_Type
                {
                    textGroup = new LocalisedCharacterString_PropertyType[] {
                            new LocalisedCharacterString_PropertyType {
                                LocalisedCharacterString = new LocalisedCharacterString_Type {
                                     locale = LOCALE_LINK_NOR,
                                     Value = norwegianLocalizedValue
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
            if (IsService() || IsDimensionGroup())
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
        public string ParentIdentifier
        {
            get
            {
                string parentIdentifier = null;
                if (_md.parentIdentifier != null)
                    parentIdentifier = _md.parentIdentifier.CharacterString;
                return parentIdentifier;
            }

            set
            {
                _md.parentIdentifier = new CharacterString_PropertyType { CharacterString = value };
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

        /// <summary>
        /// Note: Only supporting one hierarchyLevelName element. Array is overwritten with an array of one element when value is updated.
        /// </summary>
        public string HierarchyLevelName
        {
            get
            {
                string hierarchyLevelName = null;
                if (_md.hierarchyLevelName != null && _md.hierarchyLevelName.Count() > 0 && _md.hierarchyLevelName[0] != null && !string.IsNullOrEmpty(_md.hierarchyLevelName[0].CharacterString))
                    hierarchyLevelName = _md.hierarchyLevelName[0].CharacterString;
                return hierarchyLevelName;
            }

            set
            {
                _md.hierarchyLevelName = new CharacterString_PropertyType[] { new CharacterString_PropertyType { CharacterString = value } };        
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

                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    var norwegianAbstract = GetNorwegianValueFromFreeText(GetAbstractElement());
                    if (!string.IsNullOrEmpty(norwegianAbstract))
                        @abstract = norwegianAbstract;
                }

                return @abstract;
            }

            set
            {
                if(MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    String existingLocalAbstract = null;
                    CharacterString_PropertyType abstractElement = GetAbstractElement();
                    if (abstractElement != null)
                    {
                        existingLocalAbstract = abstractElement.CharacterString;
                    }
                    GetIdentificationNotNull().@abstract = CreateFreeTextElementNorwegian(existingLocalAbstract, value);
                }
                else
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
        }

        public string EnglishAbstract
        {
            get
            {
                string @abstract = null;
                CharacterString_PropertyType abstractElement = GetAbstractElement();
                if (abstractElement != null)
                {
                    @abstract = abstractElement.CharacterString;
                }
                if (MetadataLanguage == METADATA_LANG_NOR)
                {
                    var englishAbstract = GetEnglishValueFromFreeText(GetAbstractElement());

                    if (!string.IsNullOrEmpty(englishAbstract))
                        @abstract = englishAbstract;
                    else
                        return null;
                }

                return @abstract;
            }

            set
            {
                if(MetadataLanguage == LOCALE_ENG.ToLower())
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
                else
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

                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    var norwegianPurpose = GetNorwegianValueFromFreeText(GetPurposeElement());
                    if (!string.IsNullOrEmpty(norwegianPurpose))
                        purpose = norwegianPurpose;
                }

                return purpose;
            }
            set
            {
                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    String existingLocalPurpose = null;
                    CharacterString_PropertyType purposeElement = GetPurposeElement();
                    if (purposeElement != null)
                    {
                        existingLocalPurpose = purposeElement.CharacterString;
                    }
                    GetIdentificationNotNull().purpose = CreateFreeTextElementNorwegian(existingLocalPurpose, value);
                }
                else
                {
                    PT_FreeText_PropertyType purposeElementWithFreeText = GetPurposeElement() as PT_FreeText_PropertyType;
                    if (purposeElementWithFreeText != null)
                    {
                        purposeElementWithFreeText.CharacterString = value;
                    }
                    else
                    {
                        GetIdentificationNotNull().purpose = new CharacterString_PropertyType { CharacterString = value };
                    }
                }
            }
        }

        public string EnglishPurpose
        {
            get
            {
                string purpose = null;
                CharacterString_PropertyType purposeElement = GetPurposeElement();
                if (purposeElement != null)
                {
                    purpose = purposeElement.CharacterString;
                }

                if (MetadataLanguage == METADATA_LANG_NOR)
                {
                    var englishPurpose = GetEnglishValueFromFreeText(GetPurposeElement());

                    if (!string.IsNullOrEmpty(englishPurpose))
                        purpose = englishPurpose;
                    else
                        return null;
                }

                return purpose;
            }

            set
            {
                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    PT_FreeText_PropertyType purposeElementWithFreeText = GetPurposeElement() as PT_FreeText_PropertyType;
                    if (purposeElementWithFreeText != null)
                    {
                        purposeElementWithFreeText.CharacterString = value;
                    }
                    else
                    {
                        GetIdentificationNotNull().purpose = new CharacterString_PropertyType { CharacterString = value };
                    }
                }
                else
                {
                    String existingLocalPurpose = null;
                    CharacterString_PropertyType purposeElement = GetPurposeElement();
                    if (purposeElement != null)
                    {
                        existingLocalPurpose = purposeElement.CharacterString;
                    }
                    GetIdentificationNotNull().purpose = CreateFreeTextElement(existingLocalPurpose, value);
                }
            }
        }

        private CharacterString_PropertyType GetPurposeElement()
        {
            CharacterString_PropertyType purpose = null;
            var identification = GetIdentification();
            if (identification != null && identification.purpose != null)
                purpose = identification.purpose;
            return purpose;
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

                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    var norwegianSupplementalDescription = GetNorwegianValueFromFreeText(GetSupplementalDescriptionElement());
                    if (!string.IsNullOrEmpty(norwegianSupplementalDescription))
                        desc = norwegianSupplementalDescription;
                }

                return desc;
            }

            set
            {
                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    String existingLocalSupplementalDescription = null;
                    CharacterString_PropertyType supplementalDescriptionElement = GetSupplementalDescriptionElement();
                    if (supplementalDescriptionElement != null)
                    {
                        existingLocalSupplementalDescription = supplementalDescriptionElement.CharacterString;
                    }
                    GetDatasetIdentification().supplementalInformation = CreateFreeTextElementNorwegian(existingLocalSupplementalDescription, value);
                }
                else
                {
                    PT_FreeText_PropertyType supplementalDescriptionElementWithFreeText = GetSupplementalDescriptionElement() as PT_FreeText_PropertyType;
                    if (supplementalDescriptionElementWithFreeText != null)
                    {
                        supplementalDescriptionElementWithFreeText.CharacterString = value;
                    }
                    else
                    {
                        GetDatasetIdentification().supplementalInformation = new CharacterString_PropertyType { CharacterString = value };
                    }
                }
            }
        }

        public string EnglishSupplementalDescription
        {
            get
            {
                string supplementalDescription = null;
                CharacterString_PropertyType supplementalDescriptionElement = GetSupplementalDescriptionElement();
                if (supplementalDescriptionElement != null)
                {
                    supplementalDescription = supplementalDescriptionElement.CharacterString;
                }


                if (MetadataLanguage == METADATA_LANG_NOR)
                {
                    var englishSupplementalDescription = GetEnglishValueFromFreeText(GetSupplementalDescriptionElement());

                    if (!string.IsNullOrEmpty(englishSupplementalDescription))
                        supplementalDescription = englishSupplementalDescription;
                    else
                        return null;
                }

                return supplementalDescription;
            }

            set
            {
                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    PT_FreeText_PropertyType supplementalDescriptionElementWithFreeText = GetSupplementalDescriptionElement() as PT_FreeText_PropertyType;
                    if (supplementalDescriptionElementWithFreeText != null)
                    {
                        supplementalDescriptionElementWithFreeText.CharacterString = value;
                    }
                    else
                    {
                        GetDatasetIdentification().supplementalInformation = new CharacterString_PropertyType { CharacterString = value };
                    }
                }
                else
                {
                    String existingLocalSupplementalDescription = null;
                    CharacterString_PropertyType supplementalDescriptionElement = GetSupplementalDescriptionElement();
                    if (supplementalDescriptionElement != null)
                    {
                        existingLocalSupplementalDescription = supplementalDescriptionElement.CharacterString;
                    }
                    GetDatasetIdentification().supplementalInformation = CreateFreeTextElement(existingLocalSupplementalDescription, value);
                }
            }
        }

        private CharacterString_PropertyType GetSupplementalDescriptionElement()
        {
            CharacterString_PropertyType supplementalDescription = null;
            var datasetIdentification = GetDatasetIdentification();
            if (datasetIdentification != null && datasetIdentification.supplementalInformation != null)
                supplementalDescription = datasetIdentification.supplementalInformation;
            return supplementalDescription;
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

        public SimpleContact ContactCustodian
        {
            get { return GetContactWithRole("custodian"); }
            set { CreateOrUpdateContactWithRole("custodian", value); }
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

            if (!string.IsNullOrEmpty(contact.PositionName))
                responsibleParty.positionName = new CharacterString_PropertyType { CharacterString = contact.PositionName };

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
                Role = role,
                PositionName = GetStringOrNull(responsibleParty.positionName)
            };
        }

        private string GetStringOrNull(CharacterString_PropertyType input)
        {
            return input != null ? input.CharacterString : null;
        }

        public string Language
        {
            get
            {
                string language = null;
                var datasetIdentification = GetDatasetIdentification();
                if (datasetIdentification != null && datasetIdentification.language != null && datasetIdentification.language.Length > 0
                    && datasetIdentification.language[0].LanguageCode != null)
                {
                    language = datasetIdentification.language[0].LanguageCode.codeListValue;
                }
                return language;
            }

            set
            {
                string languageCode = "nor";
                string languageValue = "Norsk";

                if (value == "eng")
                {
                    languageCode = "eng";
                    languageValue = "English";
                }

                GetDatasetIdentification().language = 
                    new LanguageCode_PropertyType[] 
                    {
                        new LanguageCode_PropertyType
                        {
                            LanguageCode = new CodeListValue_Type { codeList = "http://www.loc.gov/standards/iso639-2/", codeListValue = languageCode, Value = languageValue }
                        }
                    };
            }
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
                                Anchor_Type titleObject = descriptiveKeyword.MD_Keywords.thesaurusName.CI_Citation.title.item as Anchor_Type;
                                CharacterString_PropertyType titleCharacterString = descriptiveKeyword.MD_Keywords.thesaurusName.CI_Citation.title.item as CharacterString_PropertyType;
                                if (titleObject != null)
                                    thesaurus = GetStringOrNull(new CharacterString_PropertyType { CharacterString = titleObject.Value });
                                else
                                    thesaurus = GetStringOrNull(new CharacterString_PropertyType { CharacterString = titleCharacterString.CharacterString });
                            }
                            
                            foreach (var keywordElement in descriptiveKeyword.MD_Keywords.keyword)
                            {
                                string keywordValue = GetStringOrNull(keywordElement);
                                string keywordLink = GetLinkOrNull(keywordElement);
                                string keywordEnglishValue = GetEnglishValueFromFreeText(keywordElement);
                                if (!string.IsNullOrWhiteSpace(keywordValue))
                                {
                                    keywords.Add(new SimpleKeyword
                                    {
                                        Keyword = keywordValue,
                                        KeywordLink = keywordLink,
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
                            List<MD_Keyword> keywordsToAdd = new List<MD_Keyword>();
                            foreach (var fk in filteredKeywords)
                            {
                                string key = createKeywordKey(fk.Keyword, simpleKeyword);
                                if (!processed.ContainsKey(key))
                                {
                                    processed.Add(key, true);

                                    if (!string.IsNullOrWhiteSpace(fk.EnglishKeyword))
                                    {
                                        keywordsToAdd.Add(new MD_Keyword { keyword = CreateFreeTextElement(fk.Keyword, fk.EnglishKeyword) });
                                    }
                                    else
                                    {
                                        if(!string.IsNullOrEmpty(fk.KeywordLink))
                                            keywordsToAdd.Add(new MD_Keyword { keyword = new Anchor_Type {  Value = fk.Keyword, href = fk.KeywordLink } });
                                        else
                                            keywordsToAdd.Add(new MD_Keyword { keyword = new CharacterString_PropertyType { CharacterString = fk.Keyword } });
                                    }                                    
                                }
                            }

                            CI_Citation_PropertyType thesaurus = null; 
                            if (!string.IsNullOrWhiteSpace(simpleKeyword.Thesaurus)) {

                                object title = toCharString(simpleKeyword.Thesaurus);
                                string date = "";
                                if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE))
                                {
                                    date = "2014-03-20";
                                    title = new Anchor_Type { Value = simpleKeyword.Thesaurus, href = SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE_LINK };
                                }
                                else if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_SERVICES_TAXONOMY))
                                {
                                    date = "2010-01-19";
                                }
                                else if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1))
                                {
                                    date = "2008-06-01";
                                    title = new Anchor_Type { Value = simpleKeyword.Thesaurus, href = SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1_LINK };
                                }
                                else if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_INSPIRE_PRIORITY_DATASET))
                                {
                                    date = "2018-04-04";
                                    title = new Anchor_Type { Value = simpleKeyword.Thesaurus, href = SimpleKeyword.THESAURUS_INSPIRE_PRIORITY_DATASET_LINK };
                                }
                                else if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_SPATIAL_SCOPE))
                                {
                                    date = "2019-05-22";
                                    title = new Anchor_Type { Value = simpleKeyword.Thesaurus, href = SimpleKeyword.THESAURUS_SPATIAL_SCOPE_LINK };
                                }
                                else if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_NATIONAL_THEME))
                                {
                                    date = "2014-10-28";
                                    title = new Anchor_Type { Value = simpleKeyword.Thesaurus, href = SimpleKeyword.THESAURUS_NATIONAL_THEME_LINK };
                                }
                                else if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_CONCEPT))
                                {
                                    date = "2008-06-01";
                                    title = new Anchor_Type { Value = simpleKeyword.Thesaurus, href = SimpleKeyword.THESAURUS_CONCEPT_LINK };
                                }
                                else if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_ADMIN_UNITS))
                                {
                                    date = "2018-01-01";
                                    title = new Anchor_Type { Value = simpleKeyword.Thesaurus, href = SimpleKeyword.THESAURUS_ADMIN_UNITS_LINK };
                                }
                                else if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_SERVICE_TYPE))
                                {
                                    date = "2008-12-03";
                                }

                                var dateType = "publication";
                                if (simpleKeyword.Thesaurus.Equals(SimpleKeyword.THESAURUS_INSPIRE_PRIORITY_DATASET))
                                    dateType = "publication";

                                    thesaurus = new CI_Citation_PropertyType { 
                                    CI_Citation = new CI_Citation_Type { 
                                        title = new CI_Citation_Title { item = title },
                                        date = new CI_Date_PropertyType[] {
                                            new CI_Date_PropertyType {
                                                CI_Date = new CI_Date_Type {
                                                    date = new Date_PropertyType { Item = date },
                                                    dateType = new CI_DateTypeCode_PropertyType { 
                                                        CI_DateTypeCode = new CodeListValue_Type {
                                                            codeList = "http://standards.iso.org/iso/19139/resources/gmxCodelists.xml#CI_DateTypeCode",
                                                            codeListValue = dateType
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

        private string GetEnglishValueFromFreeText(MD_Keyword input)
        {
            string value = null;

            var keyword = input.keyword;
            if (keyword != null)
            {
                PT_FreeText_PropertyType freeText = keyword as PT_FreeText_PropertyType;
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

        private string GetLinkOrNull(MD_Keyword keywordElement)
        {
            string keywordLink = null;

            var keyword = keywordElement.keyword;

            if (keyword.GetType() == typeof(Anchor_Type))
            {
                Anchor_Type anchor = keyword as Anchor_Type;
                if (anchor != null)
                {
                    keywordLink = anchor.href;
                }
            }

            return keywordLink;
        }

        private string GetStringOrNull(MD_Keyword keywordElement)
        {
            string keywordValue = null;

            var keyword = keywordElement.keyword;

            if (keyword.GetType() == typeof(PT_FreeText_PropertyType))
            {
                CharacterString_PropertyType charString = keyword as CharacterString_PropertyType;
                if (charString != null)
                {
                    keywordValue = charString.CharacterString;
                }
            }
            else if (keyword.GetType() == typeof(CharacterString_PropertyType))
            {
                CharacterString_PropertyType charString = keyword as CharacterString_PropertyType;
                if (charString != null)
                {
                    keywordValue = charString.CharacterString;
                }
            }
            else if (keyword.GetType() == typeof(Anchor_Type))
            {
                Anchor_Type anchor = keyword as Anchor_Type;
                if (anchor != null)
                {
                    keywordValue = anchor.Value;
                }
            }

            return keywordValue;
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
                    if (topic != null)
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
                if (MetadataLanguage == METADATA_LANG_NOR)
                    onlineResource.name = CreateFreeTextElement(APPLICATION_PROFILE_PRODUCTSPEC, ENGLISH_APPLICATION_PROFILE_PRODUCTSPEC);
                else
                    onlineResource.name = CreateFreeTextElementNorwegian(ENGLISH_APPLICATION_PROFILE_PRODUCTSPEC, APPLICATION_PROFILE_PRODUCTSPEC);
                onlineResource.protocol = new CharacterString_PropertyType { CharacterString = RESOURCE_PROTOCOL_WWW };
            }
        }

        public string ApplicationSchema
        {
            get
            {
                string applicationSchema = null;
                if (_md.applicationSchemaInfo != null &&
                    _md.applicationSchemaInfo.Length > 0 &&
                    _md.applicationSchemaInfo[0].MD_ApplicationSchemaInformation != null &&
                    _md.applicationSchemaInfo[0].MD_ApplicationSchemaInformation.name != null &&
                    _md.applicationSchemaInfo[0].MD_ApplicationSchemaInformation.name.CI_Citation != null &&
                    _md.applicationSchemaInfo[0].MD_ApplicationSchemaInformation.name.CI_Citation.title != null)
                {
                    applicationSchema = GetStringFromObject(_md.applicationSchemaInfo[0].MD_ApplicationSchemaInformation.name.CI_Citation.title);
                }
                return applicationSchema;
            }
            set
            {
                // Remove ApplicationSchemaInfo if value is null or empty
                if (string.IsNullOrWhiteSpace(value))
                {
                    _md.applicationSchemaInfo = null;
                    return;
                }

                if (_md.applicationSchemaInfo == null)
                {
                    _md.applicationSchemaInfo = new MD_ApplicationSchemaInformation_PropertyType[1];
                }

                _md.applicationSchemaInfo[0] = new MD_ApplicationSchemaInformation_PropertyType
                {
                    MD_ApplicationSchemaInformation = new MD_ApplicationSchemaInformation_Type
                    {
                        name = new CI_Citation_PropertyType
                        {
                            CI_Citation = new CI_Citation_Type
                            {
                                title = new CI_Citation_Title
                                {
                                    item = new CharacterString_PropertyType
                                    {
                                        CharacterString = value
                                    }
                                },
                                date = new CI_Date_PropertyType[]
                                {
                                       new CI_Date_PropertyType
                                       {
                                           CI_Date = new CI_Date_Type
                                           {
                                               date = new Date_PropertyType
                                               {
                                                   Item = DateTime.Now.ToString("yyyy-MM-dd")
                                               },
                                               dateType = new CI_DateTypeCode_PropertyType
                                               {
                                                   CI_DateTypeCode = new CodeListValue_Type
                                                   {
                                                       codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/ML_gmxCodelists.xml#CI_DateTypeCode",
                                                       codeListValue = "publication"
                                                   }
                                                }
                                           }
                                       }
                               }
                            }
                        },
                        schemaLanguage = new CharacterString_PropertyType
                        {
                            CharacterString = "UML"
                        },
                        constraintLanguage = new CharacterString_PropertyType
                        {
                            CharacterString = "OCL"
                        }
                    }
                };
            }
        }

        private string GetStringFromObject(object o)
        {
            Anchor_Type titleObject = o as Anchor_Type;
            CI_Citation_Title citation = o as CI_Citation_Title;

            if (titleObject != null)
                return titleObject.Value;
            else if (citation != null)
            {              
                CharacterString_PropertyType title = citation.item as CharacterString_PropertyType;
                return title.CharacterString;
            }
            else
            {
                CharacterString_PropertyType title = o as CharacterString_PropertyType;
                return title.CharacterString;
            }
        }

        public SimpleOnlineResource ProductSpecificationOther
        {
            get
            {
                SimpleOnlineResource spec = new SimpleOnlineResource();
                spec.URL = GetMetadataExtensionInfoURLWithApplicationProfile(APPLICATION_PROFILE_PRODUCTSPEC_OTHER);
                spec.Name = GetMetadataExtensionInfoNameWithApplicationProfile(APPLICATION_PROFILE_PRODUCTSPEC_OTHER);
                return spec;
            }
            set
            {
                CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(APPLICATION_PROFILE_PRODUCTSPEC_OTHER);

                if (string.IsNullOrEmpty(value.Name) || string.IsNullOrEmpty(value.URL))
                {
                    if (onlineResource != null) 
                    {
                        RemoveOnlineResourceMetadataExtensionInfoWithApplicationProfile(APPLICATION_PROFILE_PRODUCTSPEC_OTHER);
                    }
                }

                else 
                {

                    if (onlineResource == null)
                    {
                        onlineResource = new CI_OnlineResource_Type();
                        AddOnlineResourceToMetadataExtensionInfo(onlineResource);
                    }
                    onlineResource.linkage = new URL_PropertyType { URL = value.URL };
                    onlineResource.applicationProfile = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_PRODUCTSPEC_OTHER };
                    onlineResource.name = new CharacterString_PropertyType { CharacterString = value.Name };
                    onlineResource.protocol = new CharacterString_PropertyType { CharacterString = RESOURCE_PROTOCOL_WWW };
                
                }
            }
        }

        private string GetMetadataExtensionInfoNameWithApplicationProfile(string applicationProfile)
        {
            string name = null;
            CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(applicationProfile);
            if (onlineResource != null && onlineResource.name != null)
            {
                name = onlineResource.name.CharacterString;
            }
            return name;
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

        private void RemoveOnlineResourceMetadataExtensionInfoWithApplicationProfile(string applicationProfile)
        {

            if (_md.metadataExtensionInfo != null && _md.metadataExtensionInfo.Length > 0)
            {
                int counter = 0;

                foreach (MD_MetadataExtensionInformation_PropertyType ext in _md.metadataExtensionInfo)
                {
                    if (ext.MD_MetadataExtensionInformation != null && ext.MD_MetadataExtensionInformation.extensionOnLineResource != null
                        && ext.MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource != null
                        && ext.MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource.applicationProfile != null
                        && ext.MD_MetadataExtensionInformation.extensionOnLineResource.CI_OnlineResource.applicationProfile.CharacterString == applicationProfile)
                    {
                        int indexToRemove = counter;
                        _md.metadataExtensionInfo = _md.metadataExtensionInfo.Where((source, index) => index != indexToRemove).ToArray();
                        break;
                    }

                    counter++;
                }
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
                if (MetadataLanguage == METADATA_LANG_NOR)
                    onlineResource.name = CreateFreeTextElement(APPLICATION_PROFILE_PRODUCTSHEET, ENGLISH_APPLICATION_PROFILE_PRODUCTSHEET);
                else
                    onlineResource.name = CreateFreeTextElementNorwegian(ENGLISH_APPLICATION_PROFILE_PRODUCTSHEET, APPLICATION_PROFILE_PRODUCTSHEET);
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
                if (MetadataLanguage == METADATA_LANG_NOR)
                    onlineResource.name = CreateFreeTextElement(APPLICATION_PROFILE_LEGEND, ENGLISH_APPLICATION_PROFILE_LEGEND);
                else
                    onlineResource.name = CreateFreeTextElementNorwegian(ENGLISH_APPLICATION_PROFILE_LEGEND, APPLICATION_PROFILE_LEGEND);
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
                onlineResource.name = CreateFreeTextElement(APPLICATION_PROFILE_PRODUCTPAGE, ENGLISH_APPLICATION_PROFILE_PRODUCTPAGE);
                onlineResource.protocol = new CharacterString_PropertyType { CharacterString = RESOURCE_PROTOCOL_WWW };
            }
        }

        public string CoverageUrl
        {
            get
            {
                return GetMetadataExtensionInfoURLWithApplicationProfile(APPLICATION_PROFILE_COVERAGE);
            }
            set
            {
                CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(APPLICATION_PROFILE_COVERAGE);
                if (onlineResource == null)
                {
                    onlineResource = new CI_OnlineResource_Type();
                    AddOnlineResourceToMetadataExtensionInfo(onlineResource);
                }
                onlineResource.linkage = new URL_PropertyType { URL = value };
                onlineResource.applicationProfile = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_COVERAGE };
                onlineResource.name = CreateFreeTextElement(APPLICATION_PROFILE_COVERAGE, ENGLISH_APPLICATION_PROFILE_COVERAGE);
                onlineResource.protocol = new CharacterString_PropertyType { CharacterString = RESOURCE_PROTOCOL_WWW };
            }
        }

        public string CoverageGridUrl
        {
            get
            {
                return GetMetadataExtensionInfoURLWithApplicationProfile(APPLICATION_PROFILE_COVERAGE_GRID);
            }
            set
            {
                CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(APPLICATION_PROFILE_COVERAGE_GRID);
                if (onlineResource == null)
                {
                    onlineResource = new CI_OnlineResource_Type();
                    AddOnlineResourceToMetadataExtensionInfo(onlineResource);
                }
                onlineResource.linkage = new URL_PropertyType { URL = value };
                onlineResource.applicationProfile = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_COVERAGE_GRID };
                onlineResource.name = CreateFreeTextElement(APPLICATION_PROFILE_COVERAGE_GRID, ENGLISH_APPLICATION_PROFILE_COVERAGE_GRID);
                onlineResource.protocol = new CharacterString_PropertyType { CharacterString = RESOURCE_PROTOCOL_WWW };
            }
        }

        public string CoverageCellUrl
        {
            get
            {
                return GetMetadataExtensionInfoURLWithApplicationProfile(APPLICATION_PROFILE_COVERAGE_CELL);
            }
            set
            {
                CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(APPLICATION_PROFILE_COVERAGE_CELL);
                if (onlineResource == null)
                {
                    onlineResource = new CI_OnlineResource_Type();
                    AddOnlineResourceToMetadataExtensionInfo(onlineResource);
                }
                onlineResource.linkage = new URL_PropertyType { URL = value };
                onlineResource.applicationProfile = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_COVERAGE_CELL };
                onlineResource.name = CreateFreeTextElement(APPLICATION_PROFILE_COVERAGE_CELL, ENGLISH_APPLICATION_PROFILE_COVERAGE_CELL);
                onlineResource.protocol = new CharacterString_PropertyType { CharacterString = RESOURCE_PROTOCOL_WWW };
            }
        }

        public string HelpUrl
        {
            get
            {
                return GetMetadataExtensionInfoURLWithApplicationProfile(APPLICATION_PROFILE_HELP);
            }
            set
            {
                CI_OnlineResource_Type onlineResource = GetMetadataExtensionInfoWithApplicationProfile(APPLICATION_PROFILE_HELP);
                if (onlineResource == null)
                {
                    onlineResource = new CI_OnlineResource_Type();
                    AddOnlineResourceToMetadataExtensionInfo(onlineResource);
                }
                onlineResource.linkage = new URL_PropertyType { URL = value };
                onlineResource.applicationProfile = new CharacterString_PropertyType { CharacterString = APPLICATION_PROFILE_HELP };
                onlineResource.name = CreateFreeTextElement(APPLICATION_PROFILE_HELP, ENGLISH_APPLICATION_PROFILE_HELP);
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
                    var version = df.version?.item as CharacterString_PropertyType;
                    format = new SimpleDistributionFormat {
                        Name = df.name.CharacterString,
                        Version = version != null ? version.CharacterString : null
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
                            version = new MD_Format_Type_Version
                            {
                                item = !string.IsNullOrEmpty(value.Version) ? toCharString(value.Version) : null
                            }
                        }
                    }
                };

            }
        }


        public List<SimpleDistributionFormat> DistributionFormats
        {
            get
            {
                List<SimpleDistributionFormat> formats = null;
                
                if (_md.distributionInfo != null && _md.distributionInfo.MD_Distribution != null
                    && _md.distributionInfo.MD_Distribution.distributionFormat != null
                    && _md.distributionInfo.MD_Distribution.distributionFormat.Length > 0 )
                {
                    formats = new List<SimpleDistributionFormat>();

                    foreach (var mdFormat in _md.distributionInfo.MD_Distribution.distributionFormat) 
                    {
                        SimpleDistributionFormat format = null;

                        if (mdFormat.MD_Format != null && mdFormat.MD_Format.name != null) 
                        {
                            var df = mdFormat.MD_Format;
                            var version = df?.version?.item as CharacterString_PropertyType;
                            format = new SimpleDistributionFormat
                            {
                                Name = df.name.CharacterString,
                                Version = version != null ? version.CharacterString : null
                            };

                            formats.Add(format);
                        }
                    }
                }
                
                return formats;
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

                List<MD_Format_PropertyType> dsFormats = new List<MD_Format_PropertyType>();
                
                
                foreach (var dsFormat in value)
                    {
                     MD_Format_PropertyType mdFormatPropertyType = new MD_Format_PropertyType
                        {
                            MD_Format = new MD_Format_Type
                            {
                                name = toCharString(dsFormat.Name),
                                version = new MD_Format_Type_Version
                                {
                                    item = !string.IsNullOrEmpty(dsFormat.Version) ? toCharString(dsFormat.Version) : null
                                }
                            }
                        };
                     dsFormats.Add(mdFormatPropertyType);
                    }
                
            _md.distributionInfo.MD_Distribution.distributionFormat = dsFormats.ToArray();
                
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
                    && _md.referenceSystemInfo[0].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier.code != null)
                {
                    RS_Identifier_Type identifier = _md.referenceSystemInfo[0].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier;

                    var anchor = identifier.code.anchor as Anchor_Type;
                    string text = "";
                    string link = null;
                    if (anchor != null)
                    {
                        text = anchor.Value;
                        link = anchor.href;
                    }
                    else
                    {
                        var code = identifier.code.anchor as CharacterString_PropertyType;
                        text = code.CharacterString;
                    }

                    value = new SimpleReferenceSystem
                    {
                        CoordinateSystem = text,
                        CoordinateSystemLink = link,
                        Namespace = !string.IsNullOrEmpty(identifier?.codeSpace?.CharacterString) ? identifier.codeSpace.CharacterString : null
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
                                     code = !string.IsNullOrEmpty(value.CoordinateSystemLink) 
                                     ? new Anchor_PropertyType{ anchor = new Anchor_Type { Value = value.CoordinateSystem, href = value.CoordinateSystemLink } }
                                     : new Anchor_PropertyType{ anchor = new CharacterString_PropertyType{ CharacterString = value.CoordinateSystem } },
                                     codeSpace = !string.IsNullOrEmpty(value.Namespace) ? toCharString(value.Namespace) : null
                                 }
                            }
                        }
                    }
                };
            }
        }

        //Multiple
        public List<SimpleReferenceSystem> ReferenceSystems
        {
            get
            {
                List<SimpleReferenceSystem> referenceSystems = null;

                if (_md.referenceSystemInfo != null && _md.referenceSystemInfo.Length > 0)
                {
                    referenceSystems = new List<SimpleReferenceSystem>();

                    for (int r = 0; r < _md.referenceSystemInfo.Length; r++ )
                    {
                        if( _md.referenceSystemInfo[r].MD_ReferenceSystem != null 
                            && _md.referenceSystemInfo[r].MD_ReferenceSystem.referenceSystemIdentifier != null
                            && _md.referenceSystemInfo[r].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier != null
                            && _md.referenceSystemInfo[r].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier.code != null) 
                            {
                                RS_Identifier_Type identifier = _md.referenceSystemInfo[r].MD_ReferenceSystem.referenceSystemIdentifier.RS_Identifier;

                            var anchor = identifier.code.anchor as Anchor_Type;
                            string text = "";
                            string link = null;
                            if (anchor != null)
                            {
                                text = anchor.Value;
                                link = anchor.href;
                            }
                            else
                            {
                                var code = identifier.code.anchor as CharacterString_PropertyType;
                                text = code.CharacterString;
                            }
                            SimpleReferenceSystem referenceSystem = new SimpleReferenceSystem
                            {
                                CoordinateSystem = text,
                                CoordinateSystemLink = link,
                                Namespace = !string.IsNullOrEmpty(identifier?.codeSpace?.CharacterString) ? identifier.codeSpace.CharacterString : null
                            };

                                referenceSystems.Add(referenceSystem);
                            }         
                    }

                }
                return referenceSystems;
            }

            set
            {
                List<MD_ReferenceSystem_PropertyType> refSystems = new List<MD_ReferenceSystem_PropertyType>();

                foreach (var refSystem in value)
                {
                   MD_ReferenceSystem_PropertyType refSystemProperty = new MD_ReferenceSystem_PropertyType 
                    {
                        MD_ReferenceSystem = new MD_ReferenceSystem_Type 
                        {
                            referenceSystemIdentifier = new RS_Identifier_PropertyType 
                            {
                                RS_Identifier = new RS_Identifier_Type 
                                {
                                    code = !string.IsNullOrEmpty(refSystem.CoordinateSystemLink)
                                     ? new Anchor_PropertyType { anchor = new Anchor_Type { Value = refSystem.CoordinateSystem, href = refSystem.CoordinateSystemLink } }
                                     : new Anchor_PropertyType { anchor = new CharacterString_PropertyType { CharacterString = refSystem.CoordinateSystem } },
                                    codeSpace = !string.IsNullOrEmpty(refSystem.Namespace) ? toCharString(refSystem.Namespace) : null
                                }
                            }
                        }
                    };

                  refSystems.Add(refSystemProperty);
                }
                _md.referenceSystemInfo = refSystems.ToArray();
            }
        }


        public SimpleResourceReference ResourceReference
        {
            get
            {
                SimpleResourceReference value = null;

                if (_md.identificationInfo != null && _md.identificationInfo.Length > 0 && _md.identificationInfo[0] != null
                    && _md.identificationInfo[0].AbstractMD_Identification != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier[0] != null)
                {
                    RS_Identifier_Type identifier = _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier[0].MD_Identifier as RS_Identifier_Type;

                    if (identifier != null) 
                    {
                        var anchor = identifier.code.anchor as Anchor_Type;
                        string text = null;
                        if (anchor != null)
                        {
                            text = anchor.Value;
                        }
                        else
                        {
                            var code = identifier.code.anchor as CharacterString_PropertyType;
                            if(code != null)
                                text = code.CharacterString;
                        }

                        value = new SimpleResourceReference
                        {
                            Code = text,
                            Codespace = identifier.codeSpace != null ? identifier.codeSpace.CharacterString : null
                        };
                    }

                    if (identifier == null)
                    {
                        MD_Identifier_Type identifierType = _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier[0].MD_Identifier as MD_Identifier_Type;

                        if (identifierType != null)
                        {
                            var anchor = identifierType.code.anchor as Anchor_Type;
                            string code = null;
                            string codespace = null;
                            if (anchor != null)
                            {
                                code = anchor.Value;
                                codespace = anchor.href;

                                if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(codespace))
                                {
                                    codespace = codespace.Replace("/" + code, "");
                                }

                                value = new SimpleResourceReference
                                {
                                    Code = code,
                                    Codespace = codespace
                                };
                            }
                        }
                    }


                }
                return value;
            }

            set
            {
                if (_md.identificationInfo != null && _md.identificationInfo.Length > 0 && _md.identificationInfo[0] != null
                    && _md.identificationInfo[0].AbstractMD_Identification != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation != null
                    && _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation != null)
                {
                    if (_md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier == null)
                    {
                        _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier = new MD_Identifier_PropertyType[]
                            {
                                new MD_Identifier_PropertyType()
                               
                            };
                    }

                    if (_md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier[0] == null)
                    {
                        _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier[0] = new MD_Identifier_PropertyType();
                    }

                    Anchor_PropertyType code = new Anchor_PropertyType { anchor = new Anchor_Type { Value = value.Code, href = value.Codespace + "/" + value.Code } };

                    _md.identificationInfo[0].AbstractMD_Identification.citation.CI_Citation.identifier[0].MD_Identifier = new MD_Identifier_Type
                    {
                                        code = code,
                                    };

                }
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
                    var tranferOptions = _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions;
                    var englishUnitsOfDistribution = tranferOptions.unitsOfDistribution as PT_FreeText_PropertyType;
            
                    value = new SimpleDistributionDetails
                    {
                        URL = resource.linkage != null ? resource.linkage.URL : null,
                        Protocol = resource.protocol != null ? resource.protocol.CharacterString : null,
                        Name = resource.name != null ? resource.name.CharacterString : null,
                        UnitsOfDistribution = tranferOptions.unitsOfDistribution != null ? tranferOptions.unitsOfDistribution.CharacterString : null
                    };
                    if (englishUnitsOfDistribution != null)
                        value.EnglishUnitsOfDistribution = GetEnglishValueFromFreeText(englishUnitsOfDistribution);
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

                URL_PropertyType url = new URL_PropertyType { URL = string.IsNullOrEmpty(value.URL) ? null : value.URL };

                CharacterString_PropertyType protocol = null;
                if (!string.IsNullOrWhiteSpace(value.Protocol))
                    protocol = new CharacterString_PropertyType { CharacterString = value.Protocol };

                CharacterString_PropertyType name = null;
                if (!string.IsNullOrWhiteSpace(value.Name))
                    name = new CharacterString_PropertyType { CharacterString = value.Name };

                PT_FreeText_PropertyType UnitsOfDistribution = null;
                if (!string.IsNullOrWhiteSpace(value.UnitsOfDistribution))
                    UnitsOfDistribution = CreateFreeTextElement( value.UnitsOfDistribution, value.EnglishUnitsOfDistribution);

                CI_OnlineResource_Type.Description_Type descriptionOption = null;
                CI_OnLineFunctionCode_PropertyType functionOption = null;

                if (IsService() && !string.IsNullOrEmpty(value.Protocol) && IsAccessPoint(value.Protocol))
                {
                    descriptionOption = GetAccessPointDescription();
                    functionOption = GetOnlineFunction();
                }

                _md.distributionInfo.MD_Distribution.transferOptions = new MD_DigitalTransferOptions_PropertyType[] {
                    new MD_DigitalTransferOptions_PropertyType {
                        MD_DigitalTransferOptions = new MD_DigitalTransferOptions_Type { 
                            unitsOfDistribution = UnitsOfDistribution ,
                            onLine = new CI_OnlineResource_PropertyType[] {
                                new CI_OnlineResource_PropertyType {
                                    CI_OnlineResource = new CI_OnlineResource_Type {
                                        linkage = url,
                                        protocol = protocol,
                                        name = name,
                                        description = descriptionOption,
                                        function = functionOption
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
                                    denominator = string.IsNullOrWhiteSpace(value) ?  new Integer_PropertyType { Integer = null } : new Integer_PropertyType { Integer = value }
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
                                if (result.specification.CI_Citation.title.item != null) {
                                    value.Title = GetStringFromObject(result.specification.CI_Citation.title.item);
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
                                    var englishExplanation = result.explanation as PT_FreeText_PropertyType;
                                    if (englishExplanation != null)
                                        value.EnglishExplanation = GetEnglishValueFromFreeText(englishExplanation);
                                }

                                // result
                                if (result.pass != null && result.pass.Boolean.HasValue)
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
                                                title = new CI_Citation_Title{ item =  toCharString(value.Title) },
                                                date = new CI_Date_PropertyType[] {
                                                    new CI_Date_PropertyType {
                                                        CI_Date = new CI_Date_Type {
                                                            date = new Date_PropertyType {
                                                                Item = value.Date  
                                                            },
                                                            dateType = new CI_DateTypeCode_PropertyType {
                                                                CI_DateTypeCode = new CodeListValue_Type {
                                                                    codeList = "http://standards.iso.org/iso/19139/resources/gmxCodelists.xml#CI_DateTypeCode",
                                                                    codeListValue = value.DateType
                                                                }    
                                                            }
                                                        }
                                                    }
                                                }
                                            }    
                                        },
                                        explanation = CreateFreeTextElement(value.Explanation, value.EnglishExplanation),
                                        pass = value.Result != null
                                               ? new Boolean_PropertyType { Boolean = value.Result.Value }
                                               : new Boolean_PropertyType { nilReason = "unknown" }
                                    }
                                }
                            }
                        }
                    }
                };


                _md.dataQualityInfo = new DQ_DataQuality_PropertyType[] {
                    new DQ_DataQuality_PropertyType {
                        DQ_DataQuality = new DQ_DataQuality_Type {
                            scope = new DQ_Scope_PropertyType
                            {
                                DQ_Scope = new DQ_Scope_Type
                                {
                                    level = new MD_ScopeCode_PropertyType
                                    {
                                        MD_ScopeCode = new CodeListValue_Type
                                        {
                                            codeList="http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/ML_gmxCodelists.xml#MD_ScopeCode",
                                            codeListValue= _md?.hierarchyLevel?[0]?.MD_ScopeCode?.codeListValue
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
                
  
                _md.dataQualityInfo[0].DQ_DataQuality.report = reports;
            }
        }

        //Multiple QualitySpecification
        public List<SimpleQualitySpecification> QualitySpecifications
        {
            get
            {
                List<SimpleQualitySpecification> value = null;

                if (_md.dataQualityInfo != null
                    && _md.dataQualityInfo.Length > 0
                    && _md.dataQualityInfo[0] != null
                    && _md.dataQualityInfo[0].DQ_DataQuality != null
                    && _md.dataQualityInfo[0].DQ_DataQuality.report != null
                    && _md.dataQualityInfo[0].DQ_DataQuality.report.Length > 0)
                {
                    var numberOfReports = _md.dataQualityInfo[0].DQ_DataQuality.report.Length;

                    value = new List<SimpleQualitySpecification>();

                    for (int r = 0; r < numberOfReports; r++)
                    {
                        if (_md.dataQualityInfo[0].DQ_DataQuality.report[r] != null
                        && _md.dataQualityInfo[0].DQ_DataQuality.report[r].AbstractDQ_Element != null)
                        {
                            DQ_DomainConsistency_Type domainConsistency = _md.dataQualityInfo[0].DQ_DataQuality.report[r].AbstractDQ_Element as DQ_DomainConsistency_Type;
                            if (domainConsistency != null
                                && domainConsistency.result != null
                                && domainConsistency.result.Length > 0
                                && domainConsistency.result[0] != null
                                && domainConsistency.result[0].AbstractDQ_Result != null)
                            {

                                foreach (var mdResult in domainConsistency.result)
                                {
                                    DQ_ConformanceResult_Type result = mdResult.AbstractDQ_Result as DQ_ConformanceResult_Type;
                                    if (result != null)
                                    {
                                        SimpleQualitySpecification resultItem = null;

                                        if (result.specification != null
                                        && result.specification.CI_Citation != null)
                                        {
                                            resultItem = new SimpleQualitySpecification();

                                            if (!string.IsNullOrEmpty(result.specification.href))
                                                resultItem.SpecificationLink = result.specification.href;
                                            // title
                                            if (result.specification.CI_Citation.title != null)
                                            {
                                                var anchorTitle = result.specification.CI_Citation.title.item as Anchor_Type;
                                                if (anchorTitle != null)
                                                {
                                                    resultItem.Title = anchorTitle.Value;
                                                    resultItem.TitleLink = anchorTitle.href;
                                                    resultItem.TitleLinkDescription = anchorTitle.title;
                                                }
                                                else
                                                    resultItem.Title = GetStringFromObject(result.specification.CI_Citation.title);
                                            }


                                            // date and datetype
                                            if (result.specification.CI_Citation.date != null
                                                && result.specification.CI_Citation.date.Length > 0
                                                && result.specification.CI_Citation.date[0] != null
                                                && result.specification.CI_Citation.date[0].CI_Date != null)
                                            {

                                                CI_Date_Type dateType = result.specification.CI_Citation.date[0].CI_Date;

                                                if (dateType.date != null && dateType.date.Item != null)
                                                {
                                                    string date = dateType.date.Item as string;
                                                    if (date != null)
                                                    {
                                                        resultItem.Date = date;
                                                    }
                                                }

                                                if (dateType.dateType != null && dateType.dateType.CI_DateTypeCode != null)
                                                {
                                                    resultItem.DateType = dateType.dateType.CI_DateTypeCode.codeListValue;
                                                }
                                            }

                                            // authority
                                            if (result.specification.CI_Citation.identifier != null
                                                && result.specification.CI_Citation.identifier.Length > 0
                                                && result.specification.CI_Citation.identifier[0] != null
                                                && result.specification.CI_Citation.identifier[0].MD_Identifier != null
                                                && result.specification.CI_Citation.identifier[0].MD_Identifier.authority != null
                                                && result.specification.CI_Citation.identifier[0].MD_Identifier.authority.CI_Citation != null
                                                && result.specification.CI_Citation.identifier[0].MD_Identifier.authority.CI_Citation.title != null
                                                )
                                            {
                                                resultItem.Responsible = GetStringFromObject(result.specification.CI_Citation.identifier[0].MD_Identifier.authority.CI_Citation.title);
                                            }


                                            // explanation
                                            if (result.explanation != null)
                                            {
                                                resultItem.Explanation = result.explanation.CharacterString;
                                                var englishExplanation = result.explanation as PT_FreeText_PropertyType;
                                                if (englishExplanation != null)
                                                    resultItem.EnglishExplanation = GetEnglishValueFromFreeText(englishExplanation);
                                            }

                                            // result
                                            if (result.pass != null && result.pass.Boolean.HasValue)
                                            {
                                                resultItem.Result = result.pass.Boolean.Value;
                                            }

                                            value.Add(resultItem);
                                        }
                                    }
                                }
                            }
                            else
                            { 
                                DQ_ConceptualConsistency_Type conceptualConsistency = _md.dataQualityInfo[0].DQ_DataQuality.report[r].AbstractDQ_Element as DQ_ConceptualConsistency_Type;
                                if (conceptualConsistency != null
                                    && conceptualConsistency.result != null
                                    && conceptualConsistency.result.Length > 0
                                    && conceptualConsistency.result[0] != null
                                    && conceptualConsistency.result[0].AbstractDQ_Result != null)
                                {
                                    DQ_QuantitativeResult_Type resultQuantitative = conceptualConsistency.result[0].AbstractDQ_Result as DQ_QuantitativeResult_Type;
                                    if(resultQuantitative != null)
                                    {
                                        SimpleQualitySpecification resultItem = new SimpleQualitySpecification();
                                        resultItem.Title = conceptualConsistency.nameOfMeasure?[0]?.type?.Value;
                                        resultItem.Explanation = conceptualConsistency.measureDescription.CharacterString;
                                        resultItem.QuantitativeResult = resultQuantitative?.value?[0]?.Record?.ToString();
                                        resultItem.QuantitativeResultValueUnit = GetSimpleValueUnit(resultQuantitative?.valueUnit?.href);
                                        resultItem.Responsible = "sds-" + conceptualConsistency.nameOfMeasure?[0]?.type?.Value?.ToLower();

                                        value.Add(resultItem);
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
                List<DQ_Element_PropertyType> reports = new List<DQ_Element_PropertyType>();

                List<DQ_Result_PropertyType> mdResults = new List<DQ_Result_PropertyType>();
                const int numberOfResultsForEachReport = 2;
                int reportCounter = 0;
                int resultCounter = 0;

                var conformanceResults = value.Where(r => r.Responsible != "sds-performance"
                && r.Responsible != "sds-availability" && r.Responsible != "sds-capacity").ToList();

                foreach (var mdResult in conformanceResults)
                {
                    string specificationLink = null;
                    if (!string.IsNullOrEmpty(mdResult.SpecificationLink))
                        specificationLink = mdResult.SpecificationLink;

                    object title = toCharString(mdResult.Title);

                    if (!string.IsNullOrEmpty(mdResult.TitleLink))
                    {
                        title = new Anchor_Type
                        {
                            href = mdResult.TitleLink,
                            title = mdResult.TitleLinkDescription,
                            Value = mdResult.Title
                        };
                    }

                    DQ_Result_PropertyType   DQResult = new DQ_Result_PropertyType
                        {
                            AbstractDQ_Result = new DQ_ConformanceResult_Type
                            {
                                specification = new CI_Citation_PropertyType
                                {
                                    href = specificationLink,
                                    CI_Citation = new CI_Citation_Type
                                    {
                                        title = new CI_Citation_Title { item = title },
                                        date = new CI_Date_PropertyType[] {
                                                new CI_Date_PropertyType {
                                                    CI_Date = new CI_Date_Type {
                                                        date = new Date_PropertyType {
                                                            Item = mdResult.Date
                                                        },
                                                        dateType = new CI_DateTypeCode_PropertyType {
                                                            CI_DateTypeCode = new CodeListValue_Type {
                                                                codeList = "http://standards.iso.org/iso/19139/resources/gmxCodelists.xml#CI_DateTypeCode",
                                                                codeListValue = mdResult.DateType
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
                                                            title = new CI_Citation_Title{ item =  new CharacterString_PropertyType { CharacterString = mdResult.Responsible } },
                                                            date = new CI_Date_PropertyType[]{ new CI_Date_PropertyType() }
                                                        }
                                                    }, code = new Anchor_PropertyType{ anchor =  new CharacterString_PropertyType() }
                                                }
                                              }
                                            }
                                    }
                                },
                                explanation = CreateFreeTextElement(mdResult.Explanation, mdResult.EnglishExplanation),
                                pass = mdResult.Result != null 
                                ? new Boolean_PropertyType { Boolean = mdResult.Result.Value } 
                                : new Boolean_PropertyType { nilReason = "unknown" }
                            }
                        };

                        mdResults.Add(DQResult);
                        resultCounter++;

                        if (resultCounter == numberOfResultsForEachReport)
                        {
                            reports.Add(
                                new DQ_Element_PropertyType
                                {
                                    AbstractDQ_Element = new DQ_DomainConsistency_Type
                                    {
                                        result = new DQ_Result_PropertyType[] { }
                                    }
                                });

                            reports[reportCounter].AbstractDQ_Element.result = mdResults.ToArray();
                            mdResults = new List<DQ_Result_PropertyType>();
                            resultCounter = 0;
                            reportCounter++;
                        }
                }

                if (mdResults.Count > 0)
                {
                    reports.Add(
                    new DQ_Element_PropertyType
                    {
                        AbstractDQ_Element = new DQ_DomainConsistency_Type
                        {
                            result = new DQ_Result_PropertyType[] { }
                        }
                    });
                    reports[reportCounter].AbstractDQ_Element.result = mdResults.ToArray();
                }


                var conceptualConsistencyResult = value.Where(r => r.Responsible == "sds-performance"
                            || r.Responsible == "sds-availability" || r.Responsible == "sds-capacity").ToList();

                foreach(var conceptualConsistency in conceptualConsistencyResult) { 

                    if (conceptualConsistency.Responsible.ToLower() == "sds-performance")
                    {
                        DQ_Element_PropertyType sdsPerformance = new DQ_Element_PropertyType
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
                                            valueUnit = new UnitOfMeasure_PropertyType{ href = "http://www.opengis.net/def/uom/SI/second" } ,
                                            value = new Record_PropertyType[]
                                            {
                                                new Record_PropertyType
                                                {
                                                    Record = Convert.ToDouble(conceptualConsistency.QuantitativeResult) 
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        };

                        reports.Add(sdsPerformance);
                    }

                    else if (conceptualConsistency.Responsible.ToLower() == "sds-availability")
                    {
                        DQ_Element_PropertyType sdsAvailability = new DQ_Element_PropertyType
                        {
                            AbstractDQ_Element = new DQ_ConceptualConsistency_Type
                            {
                                nameOfMeasure = new Measure_Type[]
                                {
                                    new Measure_Type
                                    {
                                        type = new Anchor_Type{  href = "http://inspire.ec.europa.eu/metadata-codelist/QualityOfServiceCriteriaCode/availability",  Value = "availability" }
                                    }
                                },
                                measureDescription = new CharacterString_PropertyType
                                {
                                    CharacterString = "Lower limit of the percentage of time the service is estimated to be available on a yearly basis"
                                },
                                result = new DQ_Result_PropertyType[]
                                {
                                    new DQ_Result_PropertyType
                                    {
                                        AbstractDQ_Result = new DQ_QuantitativeResult_Type
                                        {
                                            valueUnit = new UnitOfMeasure_PropertyType{ href = "urn:ogc:def:uom:OGC::percent" } ,
                                            value = new Record_PropertyType[]
                                            {
                                                new Record_PropertyType
                                                {
                                                    Record = Convert.ToDouble(conceptualConsistency.QuantitativeResult)
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        };

                        reports.Add(sdsAvailability);
                    }

                    else if (conceptualConsistency.Responsible.ToLower() == "sds-capacity")
                    {
                        DQ_Element_PropertyType sdsCapacity = new DQ_Element_PropertyType
                        {
                            AbstractDQ_Element = new DQ_ConceptualConsistency_Type
                            {
                                nameOfMeasure = new Measure_Type[]
                                {
                                    new Measure_Type
                                    {
                                        type = new Anchor_Type{  href = "http://inspire.ec.europa.eu/metadata-codelist/QualityOfServiceCriteriaCode/capacity",  Value = "capacity" }
                                    }
                                },
                                measureDescription = new CharacterString_PropertyType
                                {
                                    CharacterString = "Lower limit of the maximum number of simultaneous requests that can be completed within the limits of the declared performance"
                                },
                                result = new DQ_Result_PropertyType[]
                                {
                                    new DQ_Result_PropertyType
                                    {
                                        AbstractDQ_Result = new DQ_QuantitativeResult_Type
                                        {
                                            valueUnit = new UnitOfMeasure_PropertyType{ href = "http://www.opengis.net/def/uom/OGC/1.0/unity" } ,
                                            value = new Record_PropertyType[]
                                            {
                                                new Record_PropertyType
                                                {
                                                    Record = new integer{ Value = conceptualConsistency.QuantitativeResult }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        };

                        reports.Add(sdsCapacity);
                    }
                }

                MD_ScopeDescription_PropertyType[] levelDescriptionType = null;
                if (_md?.hierarchyLevel?[0]?.MD_ScopeCode?.codeListValue == "service")
                {
                    var other = new object[] { new CharacterString_PropertyType { CharacterString = "service" } };
                    levelDescriptionType = new MD_ScopeDescription_PropertyType[] { new MD_ScopeDescription_PropertyType { MD_ScopeDescription = new MD_ScopeDescription_Type { ItemsElementName = new ItemsChoiceType11[] { ItemsChoiceType11.other }, Items = other } } };
                }

                _md.dataQualityInfo = new DQ_DataQuality_PropertyType[] {
                    new DQ_DataQuality_PropertyType {
                        DQ_DataQuality = new DQ_DataQuality_Type {
                            scope = new DQ_Scope_PropertyType
                            { 
                                DQ_Scope = new DQ_Scope_Type
                                { 
                                    level = new MD_ScopeCode_PropertyType
                                    { 
                                        MD_ScopeCode = new CodeListValue_Type
                                        {
                                            codeList="http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/ML_gmxCodelists.xml#MD_ScopeCode",
                                            codeListValue = _md?.hierarchyLevel?[0]?.MD_ScopeCode?.codeListValue
                                        }
                                    },
                                    levelDescription = levelDescriptionType
                                 }
                            }
                        }
                    }
                };

                _md.dataQualityInfo[0].DQ_DataQuality.report = reports.ToArray();

            }
        }

        private string GetSimpleValueUnit(string value)
        {
            if (value == "http://www.opengis.net/def/uom/SI/second" 
                || (!string.IsNullOrEmpty(value) && value.Contains("second")))
                return "second";
            else if (value == "urn:ogc:def:uom:OGC::percent")
                return "percent";
            else if (value == "http://www.opengis.net/def/uom/OGC/1.0/unity")
                return "integer";
            else
                return value;
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

                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    var norwegianProcessHistory = GetNorwegianValueFromFreeText(GetProcessHistoryElement());
                    if (!string.IsNullOrEmpty(norwegianProcessHistory))
                        value = norwegianProcessHistory;
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

                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    String existingLocalProcessHistory = null;
                    CharacterString_PropertyType processHistoryElement = GetProcessHistoryElement();
                    if (processHistoryElement != null)
                    {
                        existingLocalProcessHistory = processHistoryElement.CharacterString;
                    }
                    _md.dataQualityInfo[0].DQ_DataQuality.lineage = new LI_Lineage_PropertyType
                    {
                        LI_Lineage = new LI_Lineage_Type
                        {
                            statement = CreateFreeTextElementNorwegian(existingLocalProcessHistory, value)
                        }
                    };
                }
                else
                { 
                    PT_FreeText_PropertyType processHistoryElementWithFreeText = GetProcessHistoryElement() as PT_FreeText_PropertyType;
                    if (processHistoryElementWithFreeText != null)
                    {
                        processHistoryElementWithFreeText.CharacterString = value;
                        _md.dataQualityInfo[0].DQ_DataQuality.lineage = new LI_Lineage_PropertyType
                        {
                            LI_Lineage = new LI_Lineage_Type
                            {
                                statement = processHistoryElementWithFreeText
                            }
                        };
                    }
                    else
                    { 
                        _md.dataQualityInfo[0].DQ_DataQuality.lineage = new LI_Lineage_PropertyType
                        {
                            LI_Lineage = new LI_Lineage_Type
                            {
                                statement = toCharString(value)
                            }
                        };
                    }
                }
            }
        }

        public string EnglishProcessHistory
        {
            get
            {
                var processHistory = GetProcessHistoryElement()?.CharacterString;

                if (MetadataLanguage == METADATA_LANG_NOR)
                {
                    var englishProcessHistory = GetEnglishValueFromFreeText(GetProcessHistoryElement());

                    if (!string.IsNullOrEmpty(englishProcessHistory))
                        processHistory = englishProcessHistory;
                    else
                        return null;
                }

                return processHistory;
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

                if (MetadataLanguage == LOCALE_ENG.ToLower())
                {
                    CharacterString_PropertyType processHistoryElement = GetProcessHistoryElement();
                    _md.dataQualityInfo[0].DQ_DataQuality.lineage = new LI_Lineage_PropertyType
                    {
                        LI_Lineage = new LI_Lineage_Type
                        {
                            statement = new CharacterString_PropertyType { CharacterString = value }
                        }
                    };
                }
                else
                { 
                    String existingLocalProcessHistory = null;
                    CharacterString_PropertyType processHistoryElement = GetProcessHistoryElement();
                    if (processHistoryElement != null)
                    {
                        existingLocalProcessHistory = processHistoryElement.CharacterString;
                    }
                    _md.dataQualityInfo[0].DQ_DataQuality.lineage = new LI_Lineage_PropertyType
                    {
                        LI_Lineage = new LI_Lineage_Type
                        {
                            statement = CreateFreeTextElement(existingLocalProcessHistory, value)
                        }
                    };
                }
            }
        }

        private CharacterString_PropertyType GetProcessHistoryElement()
        {
            CharacterString_PropertyType processHistory = null;
            if (_md.dataQualityInfo != null && _md.dataQualityInfo.Count() > 0
                && _md.dataQualityInfo[0].DQ_DataQuality != null && _md.dataQualityInfo[0].DQ_DataQuality.lineage !=null
                && _md.dataQualityInfo[0].DQ_DataQuality.lineage.LI_Lineage != null && _md.dataQualityInfo[0].DQ_DataQuality.lineage.LI_Lineage.statement != null)
                processHistory = _md.dataQualityInfo[0].DQ_DataQuality.lineage.LI_Lineage.statement;
            return processHistory;
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

        public SimpleValidTimePeriod ValidTimePeriod
        {

            get 
            {
                SimpleValidTimePeriod value = new SimpleValidTimePeriod();
                EX_Extent_PropertyType[] extents = GetIdentificationExtents();
                if (extents != null && extents.Length > 0)
                {
                    foreach (EX_Extent_PropertyType extent in extents)
                    {
                        if (extent.EX_Extent != null && extent.EX_Extent.temporalElement != null && extent.EX_Extent.temporalElement.Length > 0)
                        {
                            var temporalElement = extent.EX_Extent.temporalElement[0];
                            if (temporalElement.EX_TemporalExtent != null && temporalElement.EX_TemporalExtent.extent != null && temporalElement.EX_TemporalExtent.extent.AbstractTimePrimitive != null)
                            {
                                TimePeriodType timePeriodType = temporalElement.EX_TemporalExtent.extent.AbstractTimePrimitive as TimePeriodType;
                                if (timePeriodType != null)
                                {
                                    TimePositionType validFrom = null;
                                    TimePositionType validTo = null;

                                    if (timePeriodType.Item != null)
                                        validFrom = timePeriodType.Item as TimePositionType;
                                    if (timePeriodType.Item1 != null)
                                        validTo = timePeriodType.Item1 as TimePositionType;

                                    if (validFrom != null && validTo != null && validFrom.Value != "0001-01-01")
                                    {
                                        value = new SimpleValidTimePeriod
                                        {
                                            ValidFrom = validFrom.Value,
                                            ValidTo = validTo.Value
                                        };
                                        
                                        break;
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
                var validFrom = value.ValidFrom;
                if (string.IsNullOrEmpty(validFrom))
                    validFrom = "0001-01-01";

                var validTo = value.ValidTo;
                TimePositionType timePositionType = new TimePositionType(){ Value = value.ValidTo };
                if (string.IsNullOrEmpty(validTo))
                    timePositionType = new TimePositionType() { indeterminatePosition = TimeIndeterminateValueType.now, indeterminatePositionSpecified = true };


                EX_TemporalExtent_PropertyType[]  temporalElement= new EX_TemporalExtent_PropertyType[]
                    {
                        new EX_TemporalExtent_PropertyType()
                        {
                            EX_TemporalExtent= new EX_TemporalExtent_Type()
                            {
                                extent = new TM_Primitive_PropertyType()
                                {
                                    AbstractTimePrimitive = new TimePeriodType()
                                    {
                                        id = "id_1",

                                        Item = new TimePositionType()
                                        {
                                            Value = validFrom
                                        },
                                        Item1 = timePositionType
                                    }
                                } 
                            }
                        }
                    };


                CharacterString_PropertyType description = null;
                EX_GeographicExtent_PropertyType[] geographichElement = null;
                EX_VerticalExtent_PropertyType[] verticalElement = null;

                bool foundDescription = false;
                bool foundGeoGraphicExtent = false;
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
                            if (!foundGeoGraphicExtent && extent.EX_Extent.geographicElement != null)
                            {
                                geographichElement = extent.EX_Extent.geographicElement;
                                foundGeoGraphicExtent = true;
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

                newExtent.geographicElement = geographichElement;
                newExtent.description = description;
                newExtent.temporalElement = temporalElement;
                newExtent.verticalElement = verticalElement;
  
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
            if (dateProperty != null && dateProperty.Item != null
                && dateProperty.Item.ToString().StartsWith("0001-01-01"))
                return null;

            DateTime? date = dateProperty.Item as DateTime?;

            if (date == null)
            {
                string dateString = dateProperty.Item as string;
                if (!string.IsNullOrEmpty(dateString))
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
            else { updatedValue = "0001-01-01"; }

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
                                    codeList = "http://standards.iso.org/iso/19139/resources/gmxCodelists.xml#CI_DateTypeCode",
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
                    if (_md.language.item is LanguageCode_PropertyType)
                    {
                        var language = _md.language.item as LanguageCode_PropertyType;
                        value = language.LanguageCode.codeListValue;
                    }
                    else 
                        value = _md.language.item.ToString();
                }

                return value;
            }

            set
            {
                _md.language = new Language_PropertyType
                {
                    item = new LanguageCode_PropertyType
                    {
                        LanguageCode = new CodeListValue_Type
                        {
                        codeList = "http://www.loc.gov/standards/iso639-2/",
                        codeListValue = value,
                        Value = (value == "nor" ? "Norsk" : "English")
                        }
                    }
                };
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
                //Populate data default from old constraints structure
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
                                if (securityConstraint.userNote != null) 
                                {
                                    value.SecurityConstraintsNote = securityConstraint.userNote.CharacterString;
                                }
                            }
                            else
                            {
                                MD_LegalConstraints_Type legalConstraint = constraintProperty.MD_Constraints as MD_LegalConstraints_Type;
                                if (legalConstraint != null)
                                {

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

                                        var otherConstraint = legalConstraint.otherConstraints[0].MD_RestrictionOther as CharacterString_PropertyType;
                                        if (otherConstraint != null)
                                        {
                                            value.OtherConstraints = otherConstraint.CharacterString;
                                            var englishOtherConstraint = legalConstraint.otherConstraints[0].MD_RestrictionOther as PT_FreeText_PropertyType;
                                            if(englishOtherConstraint != null)
                                                value.EnglishOtherConstraints = GetEnglishValueFromFreeText(englishOtherConstraint);
                                        }


                                        if (otherConstraint == null)
                                        {
                                            var otherConstrainAnchor = legalConstraint.otherConstraints[0].MD_RestrictionOther as Anchor_Type;
                                            if (otherConstrainAnchor != null)
                                            {
                                                value.OtherConstraintsLink = otherConstrainAnchor.href;
                                                value.UseConstraintsLicenseLink = otherConstrainAnchor.href;
                                                value.OtherConstraintsLinkText = otherConstrainAnchor.Value;
                                                value.UseConstraintsLicenseLinkText = otherConstrainAnchor.Value;
                                            }
                                        }

                                        if (legalConstraint.otherConstraints.Length > 1 && legalConstraint.otherConstraints[1] != null)
                                        {

                                            var otherConstrainAnchor2 = legalConstraint.otherConstraints[1].MD_RestrictionOther as Anchor_Type;
                                            if (otherConstrainAnchor2 != null)
                                            {
                                                value.OtherConstraintsLink = otherConstrainAnchor2.href;
                                                value.UseConstraintsLicenseLink = otherConstrainAnchor2.href;
                                                value.OtherConstraintsLinkText = otherConstrainAnchor2.Value;
                                                value.UseConstraintsLicenseLinkText = otherConstrainAnchor2.Value;
                                            }
                                        }

                                        for (int a = 0; a < legalConstraint.otherConstraints.Length; a++)
                                        {
                                            var access = legalConstraint.otherConstraints[a].MD_RestrictionOther as CharacterString_PropertyType;
                                            if(access != null)
                                            { 
                                                if (access.CharacterString == "no restrictions" || access.CharacterString == "norway digital restricted")
                                                { 
                                                    value.OtherConstraintsAccess = access.CharacterString;
                                                    value.AccessConstraints = access.CharacterString;
                                                    break;
                                                }
                                            }
                                        }


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
                                        var useLimit = regularConstraint.useLimitation[0] as PT_FreeText_PropertyType;
                                        if (useLimit != null)
                                            value.EnglishUseLimitations = GetEnglishValueFromFreeText(useLimit);

                                        value.UseLimitations = regularConstraint.useLimitation[0].CharacterString;
                                    }
                                }
                            }

                        }
                    }

                }

                //Get new constraint structure
                if (identification != null
                    && identification.resourceConstraints != null
                    && identification.resourceConstraints.Length > 0)
                {
                    foreach (var constraint in identification.resourceConstraints)
                    {
                        MD_SecurityConstraints_Type securityConstraint = constraint.MD_Constraints as MD_SecurityConstraints_Type;
                        MD_LegalConstraints_Type legalConstraint = constraint.MD_Constraints as MD_LegalConstraints_Type;

                        if (securityConstraint != null)
                        {
                            value.SecurityConstraints = securityConstraint.classification.MD_ClassificationCode.codeListValue;
                            if(securityConstraint.userNote != null)
                                value.SecurityConstraintsNote = securityConstraint.userNote.CharacterString;
                        }
                        else if (legalConstraint?.useConstraints != null)
                        {
                            value.UseConstraints = legalConstraint.useConstraints[0].MD_RestrictionCode.codeListValue;
                            var useConstraintsType = legalConstraint.otherConstraints[0].MD_RestrictionOther as Anchor_Type;

                            if (useConstraintsType != null)
                            { 
                                value.OtherConstraintsLinkText = useConstraintsType.Value;
                                value.UseConstraintsLicenseLinkText = useConstraintsType.Value;
                                value.OtherConstraintsLink = useConstraintsType.href;
                                value.UseConstraintsLicenseLink = useConstraintsType.href;
                            }

                        }
                        else if (constraint?.MD_Constraints?.useLimitation != null)
                        {
                            var useLimit = constraint.MD_Constraints.useLimitation[0] as PT_FreeText_PropertyType;{
                                if (useLimit != null)
                                {
                                    value.UseLimitations = useLimit.CharacterString;
                                    value.EnglishUseLimitations = GetEnglishValueFromFreeText(useLimit);
                                }
                            }
                        }
                        else if (legalConstraint?.accessConstraints != null)
                        {
                            var accessConstraintType = legalConstraint.otherConstraints[0].MD_RestrictionOther as Anchor_Type;

                            if (accessConstraintType != null)
                            { 
                                value.AccessConstraints = accessConstraintType.Value;
                                value.OtherConstraintsAccess = accessConstraintType.href;
                                value.AccessConstraintsLink = accessConstraintType.href;
                            }
                        }
                        else if (legalConstraint?.otherConstraints != null)
                        {
                            var otherConstraintString = legalConstraint.otherConstraints[0].MD_RestrictionOther as CharacterString_PropertyType;
                            if (otherConstraintString != null)
                                value.OtherConstraints = otherConstraintString.CharacterString;

                            var englishOtherConstraint = legalConstraint.otherConstraints[0].MD_RestrictionOther as PT_FreeText_PropertyType;
                            if (englishOtherConstraint != null)
                                value.EnglishOtherConstraints = GetEnglishValueFromFreeText(englishOtherConstraint);
                        }
                    }

                }

                value = FixConstraints(value);

                return value;
            }

            set
            {
                MD_RestrictionOther_PropertyType otherConstraint = null;
                if (!string.IsNullOrEmpty(value.EnglishOtherConstraints))
                    otherConstraint = new MD_RestrictionOther_PropertyType { MD_RestrictionOther = CreateFreeTextElement(value.OtherConstraints, value.EnglishOtherConstraints ) };
                else
                    otherConstraint = new MD_RestrictionOther_PropertyType { MD_RestrictionOther = new CharacterString_PropertyType { CharacterString = value.OtherConstraints } };


                MD_Constraints_PropertyType[] resourceConstraints = new MD_Constraints_PropertyType[] {
                    new MD_Constraints_PropertyType {
                        MD_Constraints = new MD_Constraints_Type {
                            useLimitation = new CharacterString_PropertyType[] { CreateFreeTextElement(value.UseLimitations, value.EnglishUseLimitations) }
                        }
                    },
                    new MD_Constraints_PropertyType {
                        MD_Constraints = new MD_LegalConstraints_Type {
                            accessConstraints = new MD_RestrictionCode_PropertyType[] {
                                new MD_RestrictionCode_PropertyType {
                                    MD_RestrictionCode = new CodeListValue_Type {
                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_RestrictionCode",
                                        codeListValue = "otherRestrictions"
                                    }
                                }
                            },
                            otherConstraints = new MD_RestrictionOther_PropertyType[]
                            {
                                new MD_RestrictionOther_PropertyType
                                {
                                    MD_RestrictionOther =  new Anchor_Type
                                    {
                                        Value = value.AccessConstraints,
                                        href = value.AccessConstraintsLink
                                    }
                                }
                            }
                        }    
                    },
                    new MD_Constraints_PropertyType {
                        MD_Constraints = new MD_LegalConstraints_Type {
                            useConstraints = new MD_RestrictionCode_PropertyType[] {
                                new MD_RestrictionCode_PropertyType {
                                    MD_RestrictionCode = new CodeListValue_Type {
                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_RestrictionCode",
                                        codeListValue = "otherRestrictions"
                                    }
                                }
                            },
                            otherConstraints = new MD_RestrictionOther_PropertyType[]
                            {
                                new MD_RestrictionOther_PropertyType
                                {
                                    MD_RestrictionOther =  new Anchor_Type
                                    {
                                        Value = value.UseConstraintsLicenseLinkText,
                                        href = value.UseConstraintsLicenseLink
                                    }
                                }
                            }
                        }
                    },
                    new MD_Constraints_PropertyType {
                        MD_Constraints = new MD_LegalConstraints_Type {
                            otherConstraints = new MD_RestrictionOther_PropertyType[]
                            {
                                otherConstraint
                            }
                        }
                    },
                    new MD_Constraints_PropertyType {
                        MD_Constraints = new MD_SecurityConstraints_Type {
                            userNote = new CharacterString_PropertyType {
                                CharacterString = value.SecurityConstraintsNote
                            },
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

        private SimpleConstraints FixConstraints(SimpleConstraints value)
        {
            if (value != null && !string.IsNullOrEmpty(value.AccessConstraints))
                value.AccessConstraintsLinkText = value.AccessConstraints;

            if (value != null && !string.IsNullOrEmpty(value.AccessConstraintsLink))
            {
                if (value.AccessConstraintsLink.Contains("noLimitations"))
                    value.AccessConstraints = "no restrictions";
                else if (value.AccessConstraintsLink.Contains("INSPIRE_Directive_Article13_1d"))
                    value.AccessConstraints = "norway digital restricted";
                else if (value.AccessConstraintsLink.Contains("INSPIRE_Directive_Article13_1b"))
                    value.AccessConstraints = "restricted";
            }

            return value;

        }

        public List<string> CrossReference
        {
            get
            {
                List<string> values = new List<string>();
                var identification = GetIdentification();

                if (identification != null && identification.aggregationInfo != null)
                {
                    foreach (var element in identification.aggregationInfo)
                    {
                        if (element.MD_AggregateInformation != null && element.MD_AggregateInformation.aggregateDataSetIdentifier != null
                           && element.MD_AggregateInformation.aggregateDataSetIdentifier.MD_Identifier != null
                           && element.MD_AggregateInformation.aggregateDataSetIdentifier.MD_Identifier.code != null)
                        {
                            var code = element.MD_AggregateInformation.aggregateDataSetIdentifier.MD_Identifier.code.anchor as CharacterString_PropertyType;
                            if(code != null)
                                values.Add(code.CharacterString);
                        }
                    }
                }

                return values;
            }

            set
            {
                var identification = GetIdentification();
                if (identification != null)
                {
                    List<MD_AggregateInformation_PropertyType> crossReference = new List<MD_AggregateInformation_PropertyType>();
                    foreach (string uuid in value)
                    {
                        crossReference.Add( new MD_AggregateInformation_PropertyType
                        {
                            MD_AggregateInformation = new MD_AggregateInformation_Type
                            {
                                aggregateDataSetIdentifier = new MD_Identifier_PropertyType
                                {
                                    MD_Identifier = new MD_Identifier_Type
                                    {
                                        code = new Anchor_PropertyType { anchor = new CharacterString_PropertyType { CharacterString = uuid } }
                                    }
                                },
                                associationType = new DS_AssociationTypeCode_PropertyType {
                                    DS_AssociationTypeCode = new CodeListValue_Type
                                    {
                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml",
                                        codeListValue = "crossReference"
                                    }
                                },
                                initiativeType = new DS_InitiativeTypeCode_PropertyType
                                {
                                    DS_InitiativeTypeCode = new CodeListValue_Type
                                    {
                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml",
                                        codeListValue = "program"

                                    }
                                }
                            }
                        });
                    }

                    identification.aggregationInfo = crossReference.ToArray();
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
                            uuidref = uuid,
                            href = "https://www.geonorge.no/geonetwork/srv/nor/xml_iso19139?uuid=" + uuid
                    });
                    }

                    identification.operatesOn = operatesOn.ToArray();
                }
            }
        }

        public List<SimpleDistribution> DistributionsFormats
        {
            get
            {
                List<SimpleDistribution> formats = new List<SimpleDistribution>();

                List<SimpleDistributionDetails> transferOptions = GetDistributionTransferOptions();

                if (_md.distributionInfo != null && _md.distributionInfo.MD_Distribution != null
                    && _md.distributionInfo.MD_Distribution.distributionFormat != null
                    && _md.distributionInfo.MD_Distribution.distributionFormat.Length > 0)
                {

                    foreach (var mdFormat in _md.distributionInfo.MD_Distribution.distributionFormat)
                    {
                        SimpleDistribution format = null;

                        if (mdFormat.MD_Format != null && mdFormat.MD_Format.name != null)
                        {
                            var df = mdFormat.MD_Format;
                            var version = df?.version?.item as CharacterString_PropertyType;
                            format = new SimpleDistribution
                            {
                                FormatName = df.name.CharacterString,
                                FormatVersion = version != null ? version.CharacterString : null
                            };
                            if (df.formatDistributor != null && df.formatDistributor.Length > 0
                                && df.formatDistributor[0] != null && df.formatDistributor[0].MD_Distributor != null)
                            {
                                if (df.formatDistributor[0].MD_Distributor.distributorContact != null
                                    && df.formatDistributor[0].MD_Distributor.distributorContact.CI_ResponsibleParty != null
                                    && df.formatDistributor[0].MD_Distributor.distributorContact.CI_ResponsibleParty.organisationName != null)
                                {
                                    format.Organization = df.formatDistributor[0].MD_Distributor.distributorContact.CI_ResponsibleParty.organisationName.CharacterString;
                                }
                                if (df.formatDistributor[0].MD_Distributor.distributorTransferOptions != null
                                    && df.formatDistributor[0].MD_Distributor.distributorTransferOptions.Length > 0
                                    && df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0] != null
                                    && df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions != null)
                                {
                                    if (df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.unitsOfDistribution != null)
                                    {
                                        format.UnitsOfDistribution = df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.unitsOfDistribution.CharacterString;
                                        var englishUnitsOfDistribution = df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.unitsOfDistribution as PT_FreeText_PropertyType;
                                        if (englishUnitsOfDistribution != null)
                                        {
                                            format.EnglishUnitsOfDistribution = GetEnglishValueFromFreeText(englishUnitsOfDistribution);
                                        }
                                    }
                                    if (df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine != null
                                        && df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine.Length > 0
                                        && df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine[0] != null
                                        && df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource != null)
                                    {
                                        format.Name = df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource.name.CharacterString;
                                        format.Protocol = df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource.protocol.CharacterString;
                                        if (df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource.linkage != null)
                                        {
                                            format.URL = df.formatDistributor[0].MD_Distributor.distributorTransferOptions[0].MD_DigitalTransferOptions.onLine[0].CI_OnlineResource.linkage.URL;
                                        }
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(format.Protocol) && transferOptions != null)
                            {
                                format.Protocol = transferOptions[0].Protocol;
                                format.URL = transferOptions[0].URL;
                                format.Name = transferOptions[0].Name;
                                format.UnitsOfDistribution = transferOptions[0].UnitsOfDistribution;
                                format.EnglishUnitsOfDistribution = transferOptions[0].EnglishUnitsOfDistribution;
                                format.Organization = ContactOwner?.Organization;
                            }

                            formats.Add(format);

                            if (transferOptions != null && transferOptions.Count > 1)
                            {

                                for (int o = 1; o < transferOptions.Count; o++)
                                {
                                    SimpleDistribution simpleDistribution = new SimpleDistribution();
                                    simpleDistribution.FormatName = format.FormatName;
                                    simpleDistribution.FormatVersion = format.FormatVersion;
                                    simpleDistribution.Protocol = transferOptions[o].Protocol;
                                    simpleDistribution.URL = transferOptions[o].URL;
                                    simpleDistribution.Name = transferOptions[o].Name;
                                    simpleDistribution.UnitsOfDistribution = transferOptions[o].UnitsOfDistribution;
                                    simpleDistribution.EnglishUnitsOfDistribution = transferOptions[o].EnglishUnitsOfDistribution;
                                    simpleDistribution.Organization = ContactOwner?.Organization;
                                    formats.Add(simpleDistribution);
                                }

                            }
                        }
                    }

                }

                return formats;
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

                List<MD_Format_PropertyType> dsFormats = new List<MD_Format_PropertyType>();


                foreach (var dsFormat in value)
                {
                    CI_OnlineResource_Type.Description_Type descriptionOption = null;
                    CI_OnLineFunctionCode_PropertyType functionOption = null;

                    if (IsService() && !string.IsNullOrEmpty(dsFormat.Protocol) && IsAccessPoint(dsFormat.Protocol))
                    {
                        descriptionOption = GetAccessPointDescription();
                        functionOption = GetOnlineFunction();
                    }

                    MD_Format_PropertyType mdFormatPropertyType = new MD_Format_PropertyType
                    {
                        MD_Format = new MD_Format_Type
                        {
                            name = toCharString(dsFormat.FormatName),
                            version = new MD_Format_Type_Version
                            {
                                item = !string.IsNullOrEmpty(dsFormat.FormatVersion) ? toCharString(dsFormat.FormatVersion) : null
                            },
                            formatDistributor = new MD_Distributor_PropertyType[]
                           {
                                new MD_Distributor_PropertyType
                                {
                                    MD_Distributor = new MD_Distributor_Type
                                    {
                                        distributorContact =  new CI_ResponsibleParty_PropertyType
                                        {
                                            CI_ResponsibleParty = new CI_ResponsibleParty_Type
                                            {
                                                organisationName = new CharacterString_PropertyType
                                                {
                                                    CharacterString = dsFormat.Organization
                                                },
                                                role = new CI_RoleCode_PropertyType { CI_RoleCode = new CodeListValue_Type { codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/ML_gmxCodelists.xml#CI_RoleCode", codeListValue = "distributor" } }
                                            }
                                        },
                                        distributorTransferOptions = new MD_DigitalTransferOptions_PropertyType[]
                                        {
                                            new MD_DigitalTransferOptions_PropertyType
                                            {
                                                MD_DigitalTransferOptions = new MD_DigitalTransferOptions_Type
                                                {
                                                    unitsOfDistribution = CreateFreeTextElement(dsFormat.UnitsOfDistribution, dsFormat.EnglishUnitsOfDistribution),
                                                    onLine = new CI_OnlineResource_PropertyType[]
                                                    {
                                                        new CI_OnlineResource_PropertyType
                                                        {
                                                            CI_OnlineResource = new CI_OnlineResource_Type
                                                            {
                                                                protocol = new CharacterString_PropertyType { CharacterString = dsFormat.Protocol },
                                                                name = new CharacterString_PropertyType { CharacterString = dsFormat.Name },
                                                                linkage = new URL_PropertyType { URL = dsFormat.URL },
                                                                description = descriptionOption,
                                                                function = functionOption
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
                    dsFormats.Add(mdFormatPropertyType);
                }

                _md.distributionInfo.MD_Distribution.distributionFormat = dsFormats.ToArray();

            }
        }

        public List<SimpleOperation> ContainOperations
        {
            get
            {
                List<SimpleOperation> operations = new List<SimpleOperation>();
                var identification = GetServiceIdentification();
                if (identification.containsOperations != null && identification.containsOperations.Length > 0)
                {
                    foreach(var containOperation in identification.containsOperations)
                    { 
                        SimpleOperation operation = new SimpleOperation();
                        var operationMetadata = containOperation.SV_OperationMetadata;
                        operation.Name = operationMetadata?.operationName?.CharacterString;
                        operation.Platform = operationMetadata?.DCP?.FirstOrDefault()?.DCPList?.codeListValue;
                        operation.URL = operationMetadata?.connectPoint?.FirstOrDefault()?.CI_OnlineResource?.linkage?.URL;
                        operation.Description = operationMetadata?.operationDescription?.CharacterString;

                        operations.Add(operation);
                    }
                }
                return operations;
            }
            set
            {
                var identification = GetServiceIdentification();
                if (identification != null)
                {
                    List<SV_OperationMetadata_PropertyType> operationMetadata = new List<SV_OperationMetadata_PropertyType>();

                    foreach (var operation in value)
                    {
                        string descriptionNilReason = null;
                        if (string.IsNullOrEmpty(operation.Description))
                            descriptionNilReason = "missing";

                        operationMetadata.Add(new SV_OperationMetadata_PropertyType
                        {
                            SV_OperationMetadata = new SV_OperationMetadata_Type
                            {
                                operationName = new CharacterString_PropertyType { CharacterString = operation.Name },
                                operationDescription = new CharacterString_PropertyType { CharacterString = operation.Description, nilReason = descriptionNilReason },
                                DCP = new DCPList_PropertyType[]{ new DCPList_PropertyType { DCPList = new CodeListValue_Type
                                    {
                                        codeList = "http://www.isotc211.org/2005/iso19119/resources/Codelist/gmxCodelists.xml#DCPList" , codeListValue = operation.Platform } } },
                                connectPoint = new CI_OnlineResource_PropertyType[] { new CI_OnlineResource_PropertyType { CI_OnlineResource = new CI_OnlineResource_Type { linkage = new URL_PropertyType { URL = operation.URL } } } }
                            }
                        });
                    }

                    if(operationMetadata.Count == 0)
                        identification.containsOperations = new SV_OperationMetadata_PropertyType[]
                            {
                                new SV_OperationMetadata_PropertyType()
                            };
                    else
                        identification.containsOperations = operationMetadata.ToArray();
                }
            }
        }

        private static CI_OnLineFunctionCode_PropertyType GetOnlineFunction()
        {
            return new CI_OnLineFunctionCode_PropertyType
            {
                CI_OnLineFunctionCode = new CodeListValue_Type
                {
                    codeList = "http://standards.iso.org/iso/19139/resources/gmxCodelists.xml#CI_OnLineFunctionCode",
                    codeListValue = "information"
                }
            };
        }

        private static CI_OnlineResource_Type.Description_Type GetAccessPointDescription()
        {
            return new CI_OnlineResource_Type.Description_Type
            {
                description = new Anchor_Type
                {
                    href = "http://inspire.ec.europa.eu/metadata-codelist/OnLineDescriptionCode/accessPoint",
                    Value = "accessPoint"
                }
            };
        }

        public List<SimpleDistributionDetails> GetDistributionTransferOptions()
        {
            List<SimpleDistributionDetails> distributions = null;

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
                distributions = new List<SimpleDistributionDetails>();

                foreach(var option in _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions.onLine)
                { 
                    var resource = option.CI_OnlineResource;
                    var tranferOptions = _md.distributionInfo.MD_Distribution.transferOptions[0].MD_DigitalTransferOptions;
                    var englishUnitsOfDistribution = tranferOptions.unitsOfDistribution as PT_FreeText_PropertyType;

                    var distribution = new SimpleDistributionDetails
                    {
                        URL = resource.linkage != null ? resource.linkage.URL : null,
                        Protocol = resource.protocol != null ? resource.protocol.CharacterString : null,
                        Name = resource.name != null ? resource.name.CharacterString : null,
                        UnitsOfDistribution = tranferOptions.unitsOfDistribution != null ? tranferOptions.unitsOfDistribution.CharacterString : null,
                    };

                    if (englishUnitsOfDistribution != null)
                        distribution.EnglishUnitsOfDistribution = GetEnglishValueFromFreeText(englishUnitsOfDistribution);

                    distributions.Add(distribution);
                }
            }
            return distributions;
        }


        public string ServiceType
        {
            get
            {
                string value = "other";
                var identification = GetServiceIdentification();

                if (identification != null && identification.serviceType != null 
                    && identification.serviceType.Item !=null && identification.serviceType.Item.Value !=null)
                {
                    value = identification.serviceType.Item.Value;
                }
                return value;
            }
            set
            {
                var identification = GetServiceIdentification();
                if (identification != null)
                {
                    var serviceType = new GenericName_PropertyType { Item = new CodeType { Value = value } };
                    identification.serviceType = serviceType;
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

        public bool IsDimensionGroup()
        {
            return HierarchyLevel.Equals("dimensionGroup", StringComparison.Ordinal);
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
                    PT_FreeText_PropertyType specificUsageElementWithFreeText = GetSpecificUsageElement() as PT_FreeText_PropertyType;

                    if (specificUsageElementWithFreeText != null)
                    {
                        specificUsageElementWithFreeText.CharacterString = value;

                        identification.resourceSpecificUsage = new MD_Usage_PropertyType[]
                        {
                            new MD_Usage_PropertyType {
                                MD_Usage = new MD_Usage_Type
                                {
                                    specificUsage = specificUsageElementWithFreeText,
                                    userContactInfo = new CI_ResponsibleParty_PropertyType[] {
                                        new CI_ResponsibleParty_PropertyType
                                        {
                                            CI_ResponsibleParty = new CI_ResponsibleParty_Type
                                            {
                                                role = new CI_RoleCode_PropertyType
                                                {
                                                    CI_RoleCode = new CodeListValue_Type
                                                    {
                                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/Codelist/ML_gmxCodelists.xml#CI_RoleCode",
                                                        codeListValue = "owner"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        };
                    }
                    else
                    {
                        identification.resourceSpecificUsage = new MD_Usage_PropertyType[]
                        {
                            new MD_Usage_PropertyType {
                                MD_Usage = new MD_Usage_Type
                                {
                                    specificUsage = new CharacterString_PropertyType { CharacterString = value },
                                    userContactInfo = new CI_ResponsibleParty_PropertyType[] {
                                        new CI_ResponsibleParty_PropertyType
                                        {
                                            CI_ResponsibleParty = new CI_ResponsibleParty_Type
                                            {
                                                role = new CI_RoleCode_PropertyType
                                                {
                                                    CI_RoleCode = new CodeListValue_Type
                                                    {
                                                        codeList = "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/Codelist/ML_gmxCodelists.xml#CI_RoleCode",
                                                        codeListValue = "owner"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        };
                    }
                }
            }
        }

        public string EnglishSpecificUsage
        {
            get
            {
                return GetEnglishValueFromFreeText(GetSpecificUsageElement());
            }

            set
            {
                String existingLocalSpecificUsage = null;
                CharacterString_PropertyType specificUsageElement = GetSpecificUsageElement();
                if (specificUsageElement != null)
                {
                    existingLocalSpecificUsage = specificUsageElement.CharacterString;
                }
                GetIdentificationNotNull().resourceSpecificUsage[0].MD_Usage.specificUsage = CreateFreeTextElement(existingLocalSpecificUsage, value);
            }
        }

        private CharacterString_PropertyType GetSpecificUsageElement()
        {
            CharacterString_PropertyType specificUsage = null;
            var identification = GetIdentification();
            if (identification != null && identification.resourceSpecificUsage != null && identification.resourceSpecificUsage.Count() > 0
                && identification.resourceSpecificUsage[0].MD_Usage != null && identification.resourceSpecificUsage[0].MD_Usage.specificUsage != null
                &&  !string.IsNullOrEmpty(identification.resourceSpecificUsage[0].MD_Usage.specificUsage.CharacterString))
                specificUsage = identification.resourceSpecificUsage[0].MD_Usage.specificUsage;
            return specificUsage;
        }

        public SimpleAccessProperties AccessProperties
        {
            get
            {
                SimpleAccessProperties values = null;
                var identification = GetServiceIdentification();

                if (identification != null && identification.accessProperties != null && identification.accessProperties.MD_StandardOrderProcess != null
                    && identification.accessProperties.MD_StandardOrderProcess.orderingInstructions != null 
                    && !string.IsNullOrEmpty(identification.accessProperties.MD_StandardOrderProcess.orderingInstructions.CharacterString) )
                    {
                        values = new SimpleAccessProperties();
                        values.OrderingInstructions = identification.accessProperties.MD_StandardOrderProcess.orderingInstructions.CharacterString;
                    }   
                return values;
            }
            set
            {
                var identification = GetServiceIdentification();
                if (identification != null)
                {
                    identification.accessProperties = new MD_StandardOrderProcess_PropertyType
                    {
                        MD_StandardOrderProcess = new MD_StandardOrderProcess_Type
                        {
                            orderingInstructions = new CharacterString_PropertyType
                            {
                                CharacterString = value.OrderingInstructions
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

        public static bool IsAccessPoint(string protocol)
        {
            return protocol == "W3C:REST" || protocol == "W3C:WS" || protocol == "OGC:WPS" || protocol == "OGC:SOS";
        }

        public static bool IsNetworkService(string protocol)
        {
            return protocol == "OGC:WMS" || protocol == "OGC:WFS" || protocol == "W3C:AtomFeed" || protocol == "OGC:CSW" ||
                   protocol == "OGC:WCS" || protocol == "OGC:WMTS" || protocol == "OGC-WMS-C";
        }

    }

    public class SimpleContact
    {
        public string Name { get; set; }
        public string Organization { get; set; }
        public string OrganizationEnglish { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string PositionName { get; set; }
    }

    public class SimpleKeyword
    {
        public const string THESAURUS_GEMET_INSPIRE_V1 = "GEMET - INSPIRE themes, version 1.0";
        public const string THESAURUS_GEMET_INSPIRE_V1_LINK = "http://inspire.ec.europa.eu/theme";
        public const string THESAURUS_INSPIRE_PRIORITY_DATASET = "INSPIRE priority data set";
        public const string THESAURUS_INSPIRE_PRIORITY_DATASET_LINK = "http://inspire.ec.europa.eu/metadata-codelist/PriorityDataset";
        public const string THESAURUS_NATIONAL_INITIATIVE = "Nasjonal inndeling i geografiske initiativ og SDI-er";
        public const string THESAURUS_NATIONAL_INITIATIVE_LINK = "https://register.geonorge.no/subregister/metadata-kodelister/kartverket/samarbeid-og-lover";
        public const string THESAURUS_SERVICES_TAXONOMY = "ISO - 19119 geographic services taxonomy";
        public const string THESAURUS_NATIONAL_THEME = "Nasjonal tematisk inndeling (DOK-kategori)";
        public const string THESAURUS_NATIONAL_THEME_LINK = "https://register.geonorge.no/subregister/metadata-kodelister/kartverket/nasjonal-temainndeling";
        public const string THESAURUS_CONCEPT = "SOSI produktspesifikasjon";
        public const string THESAURUS_CONCEPT_LINK = "https://objektkatalog.geonorge.no/";
        public const string THESAURUS_ADMIN_UNITS = "Administrative enheter i Norge";
        public const string THESAURUS_ADMIN_UNITS_LINK = "https://data.geonorge.no/administrativeEnheter";
        public const string THESAURUS_SERVICE_TYPE = "COMMISSION REGULATION (EC) No 1205/2008 of 3 December 2008 implementing Directive 2007/2/EC of the European Parliament and of the Council as regards metadata, Part D 4, Classification of Spatial Data Services";
        public const string THESAURUS_SPATIAL_SCOPE = "Spatial scope";
        public const string THESAURUS_SPATIAL_SCOPE_LINK = "http://inspire.ec.europa.eu/metadata-codelist/SpatialScope";
        public const string TYPE_PLACE = "place";
        public const string TYPE_THEME = "theme";

        public string Keyword { get; set; }
        public string KeywordLink { get; set; }
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
                else if (Thesaurus.Equals(THESAURUS_INSPIRE_PRIORITY_DATASET))
                {
                    return "InspirePriorityDataset";
                }
                else if (Thesaurus.Equals(THESAURUS_NATIONAL_INITIATIVE))
                {
                    return "NationalInitiative";
                }
                else if (Thesaurus.Equals(THESAURUS_NATIONAL_THEME))
                {
                    return "NationalTheme";
                }
                else if (Thesaurus.Equals(THESAURUS_CONCEPT))
                {
                    return "Concept";
                }
                else if (Thesaurus.Equals(THESAURUS_ADMIN_UNITS))
                {
                    return "AdminUnit";
                }
                else if (Thesaurus.Equals(THESAURUS_SPATIAL_SCOPE))
                {
                    return "SpatialScope";
                }
                else if (Thesaurus.Equals(THESAURUS_SERVICE_TYPE))
                {
                    return "ServiceType";
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

    public class SimpleDistribution
    {
        public string FormatName { get; set; }
        public string FormatVersion { get; set; }
        public string URL { get; set; }
        public string Protocol { get; set; }
        public string Name { get; set; }
        public string Organization { get; set; }
        public string UnitsOfDistribution { get; set; }
        public string EnglishUnitsOfDistribution { get; set; }
    }

    public class SimpleReferenceSystem
    {
        public string CoordinateSystem { get; set; }
        public string CoordinateSystemLink { get; set; }
        public string Namespace { get; set; }
    }

    public class SimpleDistributionDetails
    {
        public string URL { get; set; }
        public string Protocol { get; set; }
        public string Name { get; set; }
        public string UnitsOfDistribution { get; set; }
        public string EnglishUnitsOfDistribution { get; set; }
    }

    public class SimpleQualitySpecification
    {
        public string SpecificationLink { get; set; }
        public string Title { get; set; }
        public string TitleLink { get; set; }
        public string TitleLinkDescription { get; set; }   
        public string Date { get; set; }
        public string DateType { get; set; }
        public string Explanation { get; set; }
        public string EnglishExplanation { get; set; }
        public bool? Result { get; set; }
        public string QuantitativeResult { get; set; }
        public string QuantitativeResultValueUnit { get; set; }
        public string Responsible { get; set; }
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
        public string EnglishUseLimitations { get; set; }
        public string AccessConstraints { get; set; } // "no restrictions" , "restricted", "norway digital restricted"
        [ObsoleteAttribute("UseConstraints will soon be deprecated. Use UseConstraintsLicenseLinkText and UseConstraintsLicenseLink instead.")]
        public string UseConstraints { get; set; } // egentlig fast verdi "otherRestrictions", det er OtherConstraintsLinkText og OtherConstraintsLink som har info lisens.
        public string OtherConstraints { get; set; }
        public string EnglishOtherConstraints { get; set; }
        [ObsoleteAttribute("OtherConstraintsLink will soon be deprecated. Use UseConstraintsLicenseLink instead.")]
        public string OtherConstraintsLink { get; set; } // https://creativecommons.org/licenses/by/4.0/
        [ObsoleteAttribute("OtherConstraintsLinkText  will soon be deprecated. Use UseConstraintsLicenseLinkText instead.")]
        public string OtherConstraintsLinkText { get; set; } // Creative Commons BY 4.0 (CC BY 4.0)
        [ObsoleteAttribute("OtherConstraintsAccess  will soon be deprecated. Use AccessConstraintsLink instead.")]
        public string OtherConstraintsAccess { get; set; } // http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1d
        public string SecurityConstraints { get; set; }
        public string SecurityConstraintsNote { get; set; }
        public string UseConstraintsLicenseLinkText { get; set; } // Creative Commons BY 4.0 (CC BY 4.0)
        public string UseConstraintsLicenseLink { get; set; } // https://creativecommons.org/licenses/by/4.0/
        public string AccessConstraintsLink { get; set; } // http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1d
        public string AccessConstraintsLinkText { get; set; } // Økonomiske- eller forretningsmessige forhold
    }


    public class SimpleValidTimePeriod 
    {
        public string ValidFrom { get; set; }
        public string ValidTo { get; set; }
    
    }

    public class SimpleResourceReference
    {
        public string Code { get; set; }
        public string Codespace { get; set; }
    }

    public class SimpleOnlineResource
    {
        public string URL { get; set; }
        public string Name { get; set; }
    }

    public class SimpleAccessProperties
    {
        public string OrderingInstructions { get; set; }
    }

    public class SimpleOperation
    {
        public string Name { get; set; }
        public string Platform { get; set; }
        public string URL { get; set; }
        public string Description { get; set; }
    }

}
