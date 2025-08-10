using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang.Scripting;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class ScriptingLogicalOperatorsTests {
    private ScriptingEngine CreateEngine() {
        OperatorRegistry.RegisterBuiltInOperators();
        return new ScriptingEngine(new ScriptingContext());
    }

    [TestMethod]
    public void AndOperator_AllTrue_ReturnsTrue() {
        var engine = CreateEngine();
        var result = engine.Evaluate("and", new BooleanElement(true), new BooleanElement(true), new StringElement("nonempty"));
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void AndOperator_AnyFalse_ReturnsFalse() {
        var engine = CreateEngine();
        var result = engine.Evaluate("and", new BooleanElement(true), new BooleanElement(false), new BooleanElement(true));
        Assert.IsFalse((bool)result!);
    }

    [TestMethod]
    public void OrOperator_AnyTrue_ReturnsTrue() {
        var engine = CreateEngine();
        var result = engine.Evaluate("or", new BooleanElement(false), new BooleanElement(false), new BooleanElement(true));
        Assert.IsTrue((bool)result!);
    }

    [TestMethod]
    public void OrOperator_AllFalse_ReturnsFalse() {
        var engine = CreateEngine();
        var result = engine.Evaluate("or", new BooleanElement(false), new BooleanElement(false));
        Assert.IsFalse((bool)result!);
    }

    [TestMethod]
    public void NotOperator_Inverts() {
        var engine = CreateEngine();
        Assert.IsTrue((bool)engine.Evaluate("not", new BooleanElement(false))!);
        Assert.IsFalse((bool)engine.Evaluate("not", new BooleanElement(true))!);
    }

    [TestMethod]
    public void XorOperator_ExclusiveTruthTable() {
        var engine = CreateEngine();
        Assert.IsFalse((bool)engine.Evaluate("xor", new BooleanElement(false), new BooleanElement(false))!);
        Assert.IsTrue((bool)engine.Evaluate("xor", new BooleanElement(true), new BooleanElement(false))!);
        Assert.IsTrue((bool)engine.Evaluate("xor", new BooleanElement(false), new BooleanElement(true))!);
        Assert.IsFalse((bool)engine.Evaluate("xor", new BooleanElement(true), new BooleanElement(true))!);
    }

    [TestMethod]
    public void LogicalOperators_StringAndNumericTruthiness() {
        var engine = CreateEngine();
        Assert.IsTrue((bool)engine.Evaluate("and", new StringElement("x"), new IntegerElement(1))!);
        Assert.IsFalse((bool)engine.Evaluate("and", new StringElement("x"), new IntegerElement(0))!);
        Assert.IsTrue((bool)engine.Evaluate("or", new IntegerElement(0), new StringElement("y"))!);
    }
}
