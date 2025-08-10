using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Scripting;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class ScriptingComparisonOperatorsTests {
    private ScriptingEngine CreateEngine() {
        OperatorRegistry.RegisterBuiltInOperators();
        return new ScriptingEngine(new ScriptingContext());
    }

    [TestMethod]
    public void NeOperator_BasicInequality_Works() {
        var engine = CreateEngine();
        var resultTrue = engine.Evaluate("ne", new IntegerElement(1), new IntegerElement(2));
        var resultFalse = engine.Evaluate("ne", new IntegerElement(3), new IntegerElement(3));
        Assert.IsTrue((bool)resultTrue!);
        Assert.IsFalse((bool)resultFalse!);
    }

    [TestMethod]
    public void LessThan_LessThanOrEqual_NumericAndString() {
        var engine = CreateEngine();
        Assert.IsTrue((bool)engine.Evaluate("lt", new IntegerElement(1), new IntegerElement(2))!);
        Assert.IsFalse((bool)engine.Evaluate("lt", new IntegerElement(2), new IntegerElement(2))!);
        Assert.IsTrue((bool)engine.Evaluate("lte", new IntegerElement(2), new IntegerElement(2))!);
        Assert.IsTrue((bool)engine.Evaluate("lt", new StringElement("apple"), new StringElement("banana"))!);
        Assert.IsFalse((bool)engine.Evaluate("lt", new StringElement("banana"), new StringElement("apple"))!);
    }

    [TestMethod]
    public void GreaterThanOrEqual_DateTimeAndNumeric() {
        var engine = CreateEngine();
        var now = DateTime.UtcNow;
        Assert.IsTrue((bool)engine.Evaluate("gte", new DateTimeElement(now), new DateTimeElement(now))!);
        Assert.IsTrue((bool)engine.Evaluate("gte", new IntegerElement(10), new IntegerElement(2))!);
        Assert.IsFalse((bool)engine.Evaluate("gte", new IntegerElement(1), new IntegerElement(5))!);
    }

    [TestMethod]
    public void MixedNumericTypes_Comparison_Coerces() {
        var engine = CreateEngine();
        Assert.IsTrue((bool)engine.Evaluate("lt", new IntegerElement(5), new DecimalElement(5.5m))!);
        Assert.IsTrue((bool)engine.Evaluate("gte", new DecimalElement(5.0m), new IntegerElement(5))!);
    }

    [TestMethod]
    public void NullComparisons_GteAndLteBehavior() {
        var engine = CreateEngine();
        Assert.IsTrue((bool)engine.Evaluate("gte", new NullElement(), new NullElement())!);
        Assert.IsTrue((bool)engine.Evaluate("lte", new NullElement(), new NullElement())!);
        Assert.IsFalse((bool)engine.Evaluate("lt", new NullElement(), new IntegerElement(1))!);
        Assert.IsFalse((bool)engine.Evaluate("gt", new NullElement(), new IntegerElement(1))!);
    }
}
