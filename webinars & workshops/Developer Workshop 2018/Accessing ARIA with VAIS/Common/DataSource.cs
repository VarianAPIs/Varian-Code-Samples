using System;
using System.Xml.Serialization;

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

        public override string ToString()
        {
            return string.Format("Hostname: {0}{1}Port: {2}{3}Database name: {4}"
                , Hostname
                , Environment.NewLine
                , Portnumber
                , Environment.NewLine
                , Databasename);
        }
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

        public override string ToString()
        {
            return Database.ToString();
        }
    }

}
