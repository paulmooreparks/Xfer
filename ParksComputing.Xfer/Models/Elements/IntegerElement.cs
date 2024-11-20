﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class IntegerElement : TypedElement<int> {
    public static readonly string ElementName = "integer";
    public const char OpeningMarker = '#';
    public const char ClosingMarker = OpeningMarker;
    public static readonly Delimiter ElementDelimiter = new Delimiter(OpeningMarker, ClosingMarker);

    public IntegerElement(int value)
        : base(value, ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        sb.Append(Value);
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
