using System.Text;

using ParksComputing.Xfer.Models.Elements;

namespace ParksComputing.Xfer
{
    public class XferDocument
    {
        public MetadataElement Metadata { get; set; } = new();
        public PropertyBagElement Root { get; set; } = new();

        public XferDocument() {
        }

        public XferDocument(MetadataElement metadata, PropertyBagElement root) {
            Metadata = metadata;
            Root = root;
        }

        public XferDocument(MetadataElement metadata) {
            Metadata = metadata;
        }

        public XferDocument(PropertyBagElement root) {
            Root = root;
        }

        public void Add(KeyValuePairElement value) {
            Root.Add(value);
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append(Metadata);
            sb.Append(Root);
            return sb.ToString();
        }

        public byte[] ToByteArray() {
            var stringRepresentation = ToString();
            return Metadata.Encoding switch {
                "UTF-8" => Encoding.UTF8.GetBytes(stringRepresentation),
                "UTF-16" => Encoding.Unicode.GetBytes(stringRepresentation),
                "UTF-32" => Encoding.UTF32.GetBytes(stringRepresentation),
                "Unicode" => Encoding.Unicode.GetBytes(stringRepresentation),
                "ASCII" => Encoding.ASCII.GetBytes(stringRepresentation),
                _ => throw new NotSupportedException($"Encoding '{Metadata.Encoding}' is not supported.")
            };
        }
    }
}
