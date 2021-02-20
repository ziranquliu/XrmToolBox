using System;
using System.Xml.Serialization;

namespace DMM365.DataContainers
{
    [Serializable()]
    public class DataField
    {

        [XmlAttribute("name")]
        public string nameAttribute { get; set; }

        [XmlAttribute("value")]
        public string valueAttribute { get; set; }

        [XmlAttribute("lookupentity")]
        public string lookupentity { get; set; }

        [XmlAttribute("lookupentityname")]
        public string lookupentityname { get; set; }

    }
}
