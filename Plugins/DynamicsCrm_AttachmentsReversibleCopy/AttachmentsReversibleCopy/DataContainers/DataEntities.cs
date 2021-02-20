using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DMM365.DataContainers
{
    [Serializable()]
    [XmlRoot("entities")]

    public class DataEntities
    {
        [XmlElement("entity")]
        public List<DataEntity> entities { get; set; }

    }
}
