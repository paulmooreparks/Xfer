﻿using Cliffer;

using System.CommandLine;
using System.Text;
using ParksComputing.Xfer;
using ParksComputing.Xfer.Services;

namespace ParksComputing.Xferc;

[Command("parse", "Parse and display an Xfer document.")]
[Argument(typeof(string), "file", "The path to the Xfer document")]
internal class ParseCommand {
    public int Execute(string file) {
        var inputBytes = File.ReadAllBytes(file);
        var parser = new Parser();
        var document = parser.Parse(inputBytes);

        Console.WriteLine($"Document uses Xfer version {document.Metadata.Version}");
        Console.WriteLine($"Message ID is {document.Metadata.MessageId}");
        Console.WriteLine();

        Console.WriteLine(document);

        return Result.Success;
    }
}

internal abstract class XElement<T> {
    internal T Value { get; set; }

    internal XElement(T value) { Value = value; } 
}

internal class IntegerXElement : XElement<int> {
    internal IntegerXElement(int value) : base(value) {
        Value = value;
    }
}
