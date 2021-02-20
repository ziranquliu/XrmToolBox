using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DMM365.DataContainers
{
    [Serializable()]
    public class Records
    {
        [XmlElement("record")]
        public List<Record> record { get; set; }

    }
}
