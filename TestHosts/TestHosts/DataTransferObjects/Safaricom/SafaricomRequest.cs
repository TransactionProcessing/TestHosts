namespace TestHosts.DataTransferObjects.Safaricom
{
    using System;
    using System.Xml.Serialization;

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://safaricom.co.ke/Pinless/keyaccounts/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://safaricom.co.ke/Pinless/keyaccounts/", IsNullable = false, ElementName = "COMMAND")]
    public class SafaricomRequest
    { 
        [XmlElement(ElementName = "TYPE")]
        public String Type { get; set; }

        [XmlElement(ElementName = "DATE")]
        public String Date { get; set; }

        [XmlElement(ElementName = "EXTNWCODE")]
        public String NetworkCode { get; set; }

        [XmlElement(ElementName = "MSISDN")]
        public String BankMSISDN { get; set; }

        [XmlElement(ElementName = "PIN")]
        public String Pin { get; set; }

        [XmlElement(ElementName = "LOGINID")]
        public String LOGINID { get; set; }

        [XmlElement(ElementName = "PASSWORD")]
        public String PASSWORD { get; set; }

        [XmlElement(ElementName = "EXTCODE")]
        public String ExternalBankCode { get; set; }

        [XmlElement(ElementName = "EXTREFNUM")]
        public String ExternalReferenceNumber { get; set; }

        [XmlElement(ElementName = "MSISDN2")]
        public String CustomerMSISDN { get; set; }

        [XmlElement(ElementName = "AMOUNT")]
        public Int32 Amount { get; set; }

        [XmlElement(ElementName = "LANGUAGE1")]
        public String BankLanguage { get; set; }

        [XmlElement(ElementName = "LANGUAGE2")]
        public String CustomerLanguage { get; set; }

        [XmlElement(ElementName = "SELECTOR")]
        public String Selector { get; set; }
    }
}