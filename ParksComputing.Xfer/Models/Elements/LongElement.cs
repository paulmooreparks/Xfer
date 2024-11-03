﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xfer.Models.Elements;

public class LongIntegerElement : Element {
    public static readonly string ElementName = "longInteger";
    public const char OpeningMarker = '+';
    public const char ClosingMarker = OpeningMarker;

    public long Value { get; set; }

    public LongIntegerElement(long value)
        : base(ElementName, new Delimiter(OpeningMarker, ClosingMarker)) {
        Value = value;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        sb.Append(Value);
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}