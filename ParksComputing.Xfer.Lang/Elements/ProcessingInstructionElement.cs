using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang.Elements {
    // Base class for all processing instructions
    public class ProcessingInstructionElement : MetadataElement {
        public static readonly HashSet<string> KnownPIKeywords = new() {
            DeserializeKeyword,
            IncludeKeyword
        };
        public const string DeserializeKeyword = "deserialize";
        public const string IncludeKeyword = "include";
        public const string IdKeyword = "id";
        public string PIType { get; }
        public ProcessingInstructionElement(string piType) : base() {
            PIType = piType;
        }
    }

    // Example: Deserialize PI
    public class DeserializePIElement : ProcessingInstructionElement {
        public DeserializePIElement() : base(DeserializeKeyword) { }
    }

    // Example: Include PI
    public class IncludePIElement : ProcessingInstructionElement {
        public IncludePIElement() : base(IncludeKeyword) { }
    }
}
