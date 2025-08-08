using System;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using System.Linq;

// Test our new ParsedValue implementation
var parser = new Parser();

// Test 1: Environment variable
Environment.SetEnvironmentVariable("DYNAMIC_TEST_VAR", "test_value");

var xfer1 = @"<! defined <|DYNAMIC_TEST_VAR|> !>";
var doc1 = parser.Parse(xfer1);
var dynamicElem1 = doc1.ProcessingInstructions
    .OfType<DefinedProcessingInstruction>().FirstOrDefault()?.SourceElement as DynamicElement;

Console.WriteLine($"Dynamic element value: '{dynamicElem1?.Value}'");
Console.WriteLine($"Dynamic element ParsedValue: '{dynamicElem1?.ParsedValue}'");
Console.WriteLine($"ParsedValue is null: {dynamicElem1?.ParsedValue == null}");

// Test 2: DynamicSource
var xfer2 = @"
<! dynamicSource {
    testkey const ""test_value""
} !>
<! defined <|testkey|> !>";

var doc2 = parser.Parse(xfer2);
var dynamicElem2 = doc2.ProcessingInstructions
    .OfType<DefinedProcessingInstruction>().FirstOrDefault()?.SourceElement as DynamicElement;

Console.WriteLine($"Dynamic source element value: '{dynamicElem2?.Value}'");
Console.WriteLine($"Dynamic source element ParsedValue: '{dynamicElem2?.ParsedValue}'");
Console.WriteLine($"ParsedValue is null: {dynamicElem2?.ParsedValue == null}");

Environment.SetEnvironmentVariable("DYNAMIC_TEST_VAR", null);
