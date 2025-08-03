using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;
using System.Text;

namespace ParksComputing.Xfer.Lang.Tests;

[TestClass]
public class XferLangSyntaxTests
{
    private Parser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new Parser();
    }

    [TestMethod]
    public void Parse_BooleanWithTildePrefix_ShouldSucceed()
    {
        // Arrange
        var input = """{ active ~true inactive ~false }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        var active = obj["active"] as BooleanElement;
        var inactive = obj["inactive"] as BooleanElement;

        Assert.IsNotNull(active);
        Assert.IsNotNull(inactive);
        Assert.AreEqual(true, active.Value);
        Assert.AreEqual(false, inactive.Value);
    }

    [TestMethod]
    public void Parse_DecimalWithAsteriskPrefix_ShouldSucceed()
    {
        // Arrange
        var input = """{ price *99.99 rate *0.05 }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        var price = obj["price"] as DecimalElement;
        var rate = obj["rate"] as DecimalElement;

        Assert.IsNotNull(price);
        Assert.IsNotNull(rate);
        Assert.AreEqual(99.99m, price.Value);
        Assert.AreEqual(0.05m, rate.Value);
    }

    [TestMethod]
    public void Parse_IntegerWithHashPrefix_ShouldSucceed()
    {
        // Arrange
        var input = """{ count #42 negative #-10 }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        var count = obj["count"] as IntegerElement;
        var negative = obj["negative"] as IntegerElement;

        Assert.IsNotNull(count);
        Assert.IsNotNull(negative);
        Assert.AreEqual(42, count.Value);
        Assert.AreEqual(-10, negative.Value);
    }

    [TestMethod]
    public void Parse_IntegerImplicitSyntax_ShouldSucceed()
    {
        // Arrange - integers can be written without prefix if unambiguous
        var input = """{ age 30 year 2023 }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        var age = obj["age"] as IntegerElement;
        var year = obj["year"] as IntegerElement;

        Assert.IsNotNull(age);
        Assert.IsNotNull(year);
        Assert.AreEqual(30, age.Value);
        Assert.AreEqual(2023, year.Value);
    }

    [TestMethod]
    public void Parse_LongWithAmpersandPrefix_ShouldSucceed()
    {
        // Arrange
        var input = """{ bigNumber &5000000000 }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        var bigNumber = obj["bigNumber"] as LongElement;

        Assert.IsNotNull(bigNumber);
        Assert.AreEqual(5000000000L, bigNumber.Value);
    }

    [TestMethod]
    public void Parse_DoubleWithCaretPrefix_ShouldSucceed()
    {
        // Arrange
        var input = """{ pi ^3.14159 e ^2.71828 }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        var pi = obj["pi"] as DoubleElement;
        var e = obj["e"] as DoubleElement;

        Assert.IsNotNull(pi);
        Assert.IsNotNull(e);
        Assert.AreEqual(3.14159, pi.Value, 0.00001);
        Assert.AreEqual(2.71828, e.Value, 0.00001);
    }

    [TestMethod]
    public void Parse_StringWithQuotes_ShouldSucceed()
    {
        // Arrange
        var input = """{ name "Alice" greeting "Hello, World!" }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        var name = obj["name"] as StringElement;
        var greeting = obj["greeting"] as StringElement;

        Assert.IsNotNull(name);
        Assert.IsNotNull(greeting);
        Assert.AreEqual("Alice", name.Value);
        Assert.AreEqual("Hello, World!", greeting.Value);
    }

    [TestMethod]
    public void Parse_ArrayWithSpaceSeparatedElements_ShouldSucceed()
    {
        // Arrange - Arrays use space-separated elements
        var input = """{ numbers [ 1 2 3 4 5 ] }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        var numbers = obj["numbers"] as ArrayElement;

        Assert.IsNotNull(numbers);
        Assert.AreEqual(5, numbers.Count);

        // All elements should be integers
        for (int i = 0; i < 5; i++)
        {
            var element = numbers[i] as IntegerElement;
            Assert.IsNotNull(element);
            Assert.AreEqual(i + 1, element.Value);
        }
    }

    [TestMethod]
    public void Parse_ArrayWithTypedElements_ShouldSucceed()
    {
        // Arrange - Array with explicit type prefixes
        var input = """{ decimals [ *1.1 *2.2 *3.3 ] }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        var decimals = obj["decimals"] as ArrayElement;

        Assert.IsNotNull(decimals);
        Assert.AreEqual(3, decimals.Count);

        var elem1 = decimals[0] as DecimalElement;
        var elem2 = decimals[1] as DecimalElement;
        var elem3 = decimals[2] as DecimalElement;

        Assert.IsNotNull(elem1);
        Assert.IsNotNull(elem2);
        Assert.IsNotNull(elem3);
        Assert.AreEqual(1.1m, elem1.Value);
        Assert.AreEqual(2.2m, elem2.Value);
        Assert.AreEqual(3.3m, elem3.Value);
    }

    [TestMethod]
    public void Parse_TupleWithMixedTypes_ShouldSucceed()
    {
        // Arrange - Tuples can contain mixed types
        var input = """{ mixed ( "Alice" 30 ~true *99.99 ) }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        var mixed = obj["mixed"] as TupleElement;

        Assert.IsNotNull(mixed);
        Assert.AreEqual(4, mixed.Count);

        var str = mixed[0] as StringElement;
        var age = mixed[1] as IntegerElement;
        var active = mixed[2] as BooleanElement;
        var price = mixed[3] as DecimalElement;

        Assert.IsNotNull(str);
        Assert.IsNotNull(age);
        Assert.IsNotNull(active);
        Assert.IsNotNull(price);

        Assert.AreEqual("Alice", str.Value);
        Assert.AreEqual(30, age.Value);
        Assert.AreEqual(true, active.Value);
        Assert.AreEqual(99.99m, price.Value);
    }

    [TestMethod]
    public void Parse_NestedObjects_ShouldSucceed()
    {
        // Arrange
        var input = """
        {
            person {
                name "Alice"
                age 30
                address {
                    street "123 Main St"
                    city "Springfield"
                }
            }
        }
        """;
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var root = (ObjectElement)document.Root;

        var person = root["person"] as ObjectElement;
        Assert.IsNotNull(person);

        var name = person["name"] as StringElement;
        var age = person["age"] as IntegerElement;
        var address = person["address"] as ObjectElement;

        Assert.IsNotNull(name);
        Assert.IsNotNull(age);
        Assert.IsNotNull(address);

        Assert.AreEqual("Alice", name.Value);
        Assert.AreEqual(30, age.Value);

        var street = address["street"] as StringElement;
        var city = address["city"] as StringElement;

        Assert.IsNotNull(street);
        Assert.IsNotNull(city);
        Assert.AreEqual("123 Main St", street.Value);
        Assert.AreEqual("Springfield", city.Value);
    }

    [TestMethod]
    public void Parse_NullElement_ShouldSucceed()
    {
        // Arrange
        var input = """{ optional ? }""";
        var bytes = Encoding.UTF8.GetBytes(input);

        // Act
        var document = _parser.Parse(bytes);

        // Assert
        Assert.IsNotNull(document);
        Assert.IsFalse(document.HasError);
        var obj = (ObjectElement)document.Root;

        // NullElement is internal, but we can verify it exists and parses
        Assert.IsTrue(obj.Dictionary.ContainsKey("optional"));
        var optional = obj["optional"];
        Assert.IsNotNull(optional);
        // The element should have ToXfer() return "?"
        Assert.AreEqual("?", optional.ToXfer());
    }
}
