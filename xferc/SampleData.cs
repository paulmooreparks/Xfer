using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParksComputing.Xferc;
public class SampleData {
    public DateTime DateTime { get; set; } = DateTime.Now;
    public DateTimeOffset DateTimeOffset { get; set; }
    public TimeOnly TimeOnly { get; set; } = new TimeOnly(11, 39, 44);
    public DateOnly DateOnly { get; set; } = new DateOnly(2024, 12, 5);
}
