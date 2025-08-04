using System.Xml;
using System.ComponentModel.DataAnnotations;

namespace Xfer.Data;

public enum TestEnum {
    None, 
    Indented,
    Spaced,
    Pretty
}

public class SampleData {
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    [Range(0, 150)]
    public int Age { get; set; }
    
    public TimeOnly TimeOnly { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public DateTime DateTime { get; set; }
    public TestEnum TestEnum { get; set; } = TestEnum.Pretty;
    
    // Additional properties to showcase XferLang capabilities
    public decimal? Salary { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    public override string ToString() {
        return $"SampleData {{ Name: {Name}, Age: {Age}, TestEnum: {TestEnum}, IsActive: {IsActive} }}";
    }

    public override bool Equals(object? obj) {
        if (obj is not SampleData other) {
            return false;
        }
        
        return Name == other.Name &&
               Age == other.Age &&
               TimeOnly == other.TimeOnly &&
               TimeSpan == other.TimeSpan &&
               DateTime == other.DateTime &&
               TestEnum == other.TestEnum &&
               Salary == other.Salary &&
               IsActive == other.IsActive;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Name, Age, TimeOnly, TimeSpan, DateTime, TestEnum, Salary, IsActive);
    }
}
