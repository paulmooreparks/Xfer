using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang.Elements;

// Base class for all processing instructions
public class ProcessingInstructionElement : MetadataElement
{
    public static readonly HashSet<string> KnownPIKeywords = new() {
        DeserializeKeyword,
        IncludeKeyword,
        CharDefKeyword
    };
    public const string DeserializeKeyword = "deserialize";
    public const string IncludeKeyword = "include";
    public const string IdKeyword = "id";
    public const string CharDefKeyword = "chardef";

    public string PIType { get; }
    public ProcessingInstructionElement(string piType) : base()
    {
        PIType = piType;
    }
}

// Example: Deserialize PI
public class DeserializePIElement : ProcessingInstructionElement {
    public DeserializePIElement() : base(DeserializeKeyword) { }
}

// Example: Include PI
public class IncludePIElement : ProcessingInstructionElement
{
    public IncludePIElement() : base(IncludeKeyword) { }
}

// Example: charDef PI
public class CharDefPIElement : ProcessingInstructionElement {
    public Dictionary<string, int> CustomCharIds { get; } = [];
    public CharDefPIElement() : base(CharDefKeyword) {
    }
}
