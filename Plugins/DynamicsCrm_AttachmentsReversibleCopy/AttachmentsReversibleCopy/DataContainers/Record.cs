using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DMM365.DataContainers
{
    [Serializable()]
    public class Record
    {

        [XmlAttribute("id")]
        public string id { get; set; }

        [XmlElement("field")]
        public List<DataField> fields { get; set; }

    }
}
