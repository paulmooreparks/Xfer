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
    [DeploymentItem("Valid/sample.xfer")]
    public void Parse_UnderLoad_ShouldRemainConsistent() {
        var filePath = Path.Combine(TestContext!.DeploymentDirectory!, "sample.xfer");
        string xferText = File.ReadAllText(filePath);

        const int iterations = 1000;
        var parser = new Parser();

        for (int i = 0; i < iterations; i++) {
            var document = parser.Parse(xferText);
            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsTrue(document.Root.Count > 0, $"Document root empty on iteration {i}");
        }
    }

    [TestMethod]
    [DeploymentItem("Valid/sample.xfer")]
    public void Parse_MultiThreaded_ShouldRemainIsolated() {
        var filePath = Path.Combine(TestContext!.DeploymentDirectory!, "sample.xfer");
        string xferText = File.ReadAllText(filePath);

        int numThreads = 10;
        int iterationsPerThread = 100;

        Parallel.For(0, numThreads, i => {
            var parser = new Parser();
            for (int j = 0; j < iterationsPerThread; j++) {
                var doc = parser.Parse(xferText);
                Assert.IsNotNull(doc);
                Assert.IsTrue(doc.Root.Count > 0);
            }
        });
    }

    [TestMethod]
    [DeploymentItem("Valid/sample.xfer")]
    public void Parse_PerformanceBenchmark() {
        var filePath = Path.Combine(TestContext!.DeploymentDirectory!, "sample.xfer");
        string xferText = File.ReadAllText(filePath);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var parser = new Parser();

        for (int i = 0; i < 1000; i++) {
            var doc = parser.Parse(xferText);
        }

        sw.Stop();
        Console.WriteLine($"Total time: {sw.ElapsedMilliseconds}ms");
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
        if (deserializedData != null && deserializedData.Person != null)
        {
            Console.WriteLine(deserializedData.Person.Name);
            Console.WriteLine(deserializedData.Person.Age);
            Console.WriteLine(deserializedData.CreatedAt);
            Console.WriteLine(deserializedData.Description);
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
