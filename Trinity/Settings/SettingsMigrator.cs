using System.Xml;

namespace Trinity.Config
{
    //http://stackoverflow.com/questions/11092274/datacontractserializer-change-namespace-and-deserialize-file-bound-to-old-name
    public class SettingsMigrator : XmlReader
    {
        public static string GilesTrinityNamespace { get { return "http://schemas.datacontract.org/2004/07/GilesTrinity.Settings"; } }

        XmlReader inner;
        public SettingsMigrator(XmlReader inner)
        {
            this.inner = inner;
        }

        public override int AttributeCount
        {
            get { return inner.AttributeCount; }
        }

        public override string BaseURI
        {
            get { return inner.BaseURI; }
        }

        public override void Close()
        {
            inner.Close();
        }

        public override int Depth
        {
            get { return inner.Depth; }
        }

        public override bool EOF
        {
            get { return inner.EOF; }
        }

        public override string GetAttribute(int i)
        {
            return inner.GetAttribute(i);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return inner.GetAttribute(name, namespaceURI);
        }

        public override string GetAttribute(string name)
        {
            return inner.GetAttribute(name);
        }

        public override bool IsEmptyElement
        {
            get { return inner.IsEmptyElement; }
        }

        public override string LocalName
        {
            get { return inner.LocalName; }
        }

        public override string LookupNamespace(string prefix)
        {
            return inner.LookupNamespace(prefix);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return inner.MoveToAttribute(name, ns);
        }

        public override bool MoveToAttribute(string name)
        {
            return inner.MoveToAttribute(name);
        }

        public override bool MoveToElement()
        {
            return inner.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return inner.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return inner.MoveToNextAttribute();
        }

        public override XmlNameTable NameTable
        {
            get { return inner.NameTable; }
        }

        public override string NamespaceURI
        {
            get
            {
                if (inner.NamespaceURI.StartsWith(GilesTrinityNamespace))
                {
                    return "";
                }
                else
                {
                    return inner.NamespaceURI;
                }
            }
        }

        public override XmlNodeType NodeType
        {
            get { return inner.NodeType; }
        }

        public override string Prefix
        {
            get { return inner.Prefix; }
        }

        public override bool Read()
        {
            return inner.Read();
        }

        public override bool ReadAttributeValue()
        {
            return inner.ReadAttributeValue();
        }

        public override ReadState ReadState
        {
            get { return inner.ReadState; }
        }

        public override void ResolveEntity()
        {
            inner.ResolveEntity();
        }

        public override string Value
        {
            get { return inner.Value; }
        }
    }
}
