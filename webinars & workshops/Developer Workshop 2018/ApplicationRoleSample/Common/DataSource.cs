using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Common
{
    [XmlRoot(ElementName = "database", Namespace = "http://www.varian.com/vmsos/schema/vms.osp/DataSource")]
    public class Database
    {
        [XmlElement(ElementName = "hostname", Namespace = "http://www.varian.com/vmsos/schema/vms.osp/DataSource")]
        public string Hostname { get; set; }
        [XmlElement(ElementName = "portnumber", Namespace = "http://www.varian.com/vmsos/schema/vms.osp/DataSource")]
        public string Portnumber { get; set; }
        [XmlElement(ElementName = "databasename", Namespace = "http://www.varian.com/vmsos/schema/vms.osp/DataSource")]
        public string Databasename { get; set; }
    }

    [XmlRoot(ElementName = "datasource", Namespace = "http://www.varian.com/vmsos/schema/vms.osp/DataSource")]
    public class DataSource
    {
        [XmlElement(ElementName = "database", Namespace = "http://www.varian.com/vmsos/schema/vms.osp/DataSource")]
        public Database Database { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

}
