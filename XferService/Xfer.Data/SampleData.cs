using System.Xml;

namespace Xfer.Data;

public enum TestEnum {
    None, 
    Indented,
    Spaced,
    Pretty
}

public class SampleData {
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public TimeOnly TimeOnly { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public DateTime DateTime { get; set; }
    public TestEnum TestEnum { get; set; } = TestEnum.Pretty;
}
