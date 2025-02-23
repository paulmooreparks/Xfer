using Microsoft.VisualStudio.TestTools.UnitTesting;

using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Attributes;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Services;

namespace Xfer.Tests;

[TestClass]
public class ParserTests {
    public TestContext? TestContext { get; set; }

    private static TestContext? _testContext;

    [ClassInitialize]
    public static void SetupTests(TestContext testContext) {
        _testContext = testContext;
    }

    [TestMethod]
    [DeploymentItem("Valid/sample.xfer")]
    public void BigParse() {
        var filePath = Path.Combine(TestContext?.DeploymentDirectory!, "sample.xfer");
        var parser = new Parser();
        var result = parser.Parse(File.ReadAllText(filePath));
    }

    [TestMethod]
    public void Serialize() {
        var data = new SampleData {
            Person = new Person { Name = null, Age = 42 },
            CreatedAt = DateTime.UtcNow,
            Description = "Serializing Xfer makes me <\\$1F600\\>",
            ints = [1, 2, 3],
            strings = [ "one", "two", "three" ],
            bag_o_bits1 = ["one", 2, 3.14]
        };

        string xferContent = XferConvert.Serialize(data, Formatting.Pretty);
        Console.WriteLine(xferContent);

        var deserializedData = XferConvert.Deserialize<SampleData>(xferContent);
        Console.WriteLine(deserializedData.Person.Name);
        Console.WriteLine(deserializedData.Person.Age);
        Console.WriteLine(deserializedData.CreatedAt);
        Console.WriteLine(deserializedData.Description);
        Console.WriteLine(string.Join(", ", deserializedData.ints));
        Console.WriteLine(string.Join(", ", deserializedData.strings));
        Console.WriteLine(string.Join(", ", deserializedData.bag_o_bits1));

        var parser = new Parser();
        var document = parser.Parse(xferContent);
        Console.WriteLine(document.ToXfer(Formatting.Pretty));

        var x = document.Root[0];

        if (x is ObjectElement o) {
            if (o.TryGetElement("Person", out ObjectElement? person)) {
                Console.WriteLine(person?.ToXfer(Formatting.Pretty));
            }
        }
    }
}

public class Person {
    [XferProperty("Full name:")]
    public string? Name { get; set; } = string.Empty;

    public int Age { get; set; }
}

public class SampleData {
    public Person Person { get; set; } = new Person { Name = "Alice", Age = 30 };

    public DateTime CreatedAt { get; set; }

    [XferEvaluated]
    public string? Description { get; set; }

    public int[] ints { get; set; } = [];
    public string[] strings { get; set; } = [];
    public List<object> bag_o_bits1 { get; set; } = new List<object>() { };
}
