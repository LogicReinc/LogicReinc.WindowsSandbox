using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace LogicReinc.WindowsSandbox
{
    [XmlRoot("Configuration")]
    public class WSB
    {
        public string VGPU { get; set; } = "Enable";
        public string Networking { get; set; } = "Default";
        public List<WSBFolder> MappedFolders { get; set; } = new List<WSBFolder>();
        public WSBLogon LogonCommand { get; set; }


        public string Serialize()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(WSB));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            using (StringWriter writer = new StringWriter())
            using (XmlWriter xml = XmlWriter.Create(writer, new XmlWriterSettings() { Indent = true, OmitXmlDeclaration = true }))
            {
                serializer.Serialize(xml, this, ns);
                return writer.ToString();
            }
        }
    }

    public class WSBLogon
    {
        public string Command { get; set; }
    }

    [XmlType("MappedFolder")]
    public class WSBFolder
    {
        public string HostFolder { get; set; }
        public bool ReadOnly { get; set; }
    }
}
