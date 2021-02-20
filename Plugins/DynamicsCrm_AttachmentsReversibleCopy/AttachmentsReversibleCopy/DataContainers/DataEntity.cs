using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DMM365.DataContainers
{
    [Serializable()]
    public class DataEntity
    {
        [XmlAttribute("displayname")]
        public string displayname { get; set; }

        [XmlAttribute("name")]
        public string name { get; set; }

        [XmlArray("records")]
        [XmlArrayItem("record")]
        public List<Record> RecordsCollection { get; set; }


    }
}
