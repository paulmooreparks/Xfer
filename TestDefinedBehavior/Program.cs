using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Scripting;
using ParksComputing.Xfer.Lang.Scripting.Utility;

var context = new ScriptingContext();
context.Variables["emptyVar"] = "";
context.Variables["nullVar"] = null;

var engine = new ScriptingEngine(context);
var definedOp = new DefinedOperator();

// Test direct string element with empty string
var emptyStringElement = new StringElement("");
var result1 = definedOp.Evaluate(context, emptyStringElement);
Console.WriteLine($"defined(StringElement(\"\")) = {result1}");

// Test EmptyElement
var emptyElement = new EmptyElement();
var result2 = definedOp.Evaluate(context, emptyElement);
Console.WriteLine($"defined(EmptyElement) = {result2}");

// Test dynamic element resolving to empty string
var emptyDynamic = new DynamicElement("emptyVar");
var result4 = definedOp.Evaluate(context, emptyDynamic);
Console.WriteLine($"defined(DynamicElement(\"emptyVar\")) = {result4}");

// Test dynamic element resolving to null
var nullDynamic = new DynamicElement("nullVar");
var result5 = definedOp.Evaluate(context, nullDynamic);
Console.WriteLine($"defined(DynamicElement(\"nullVar\")) = {result5}");

// Test undefined dynamic element
var undefinedDynamic = new DynamicElement("undefinedVar");
var result6 = definedOp.Evaluate(context, undefinedDynamic);
Console.WriteLine($"defined(DynamicElement(\"undefinedVar\")) = {result6}");
