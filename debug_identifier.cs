using ParksComputing.Xfer.Lang.Elements;
using System;

var invalidIdent = new IdentifierElement("invalid identifier");
Console.WriteLine($"Invalid identifier style: {invalidIdent.Delimiter.Style}");
Console.WriteLine($"Invalid identifier ToXfer: '{invalidIdent.ToXfer()}'");

var endsWithColon = new IdentifierElement("identifier:");
Console.WriteLine($"Ends with colon style: {endsWithColon.Delimiter.Style}");
Console.WriteLine($"Ends with colon ToXfer: '{endsWithColon.ToXfer()}'");

var validIdent = new IdentifierElement("validIdentifier");
Console.WriteLine($"Valid identifier style: {validIdent.Delimiter.Style}");
Console.WriteLine($"Valid identifier ToXfer: '{validIdent.ToXfer()}'");
