namespace TestHosts.DataTransferObjects.Safaricom
{
    using System;
    using System.Xml.Serialization;

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "COMMAND")]
    public partial class SafaricomResponse
    {
        [XmlElement(ElementName = "TYPE")]
        public String TYPE { get; set; }

        [XmlElement(ElementName = "TXNSTATUS")]
        public Int32 TXNSTATUS { get; set; }

        [XmlElement(ElementName = "DATE")]
        public String DATE { get; set; }

        [XmlElement(ElementName = "EXTREFNUM")]
        public String EXTREFNUM { get; set; }

        [XmlElement(ElementName = "TXNID")]
        public string TXNID { get; set; }

        [XmlElement(ElementName = "MESSAGE")]
        public String MESSAGE { get; set; }
    }
}