using System;
using System.Diagnostics;
using System.IO;
using ParksComputing.Xfer.Lang;

// Test the fast ID lookup implementation
var xferContent = """
{
    <!id "root"!>
    name "Test Document"

    users [
        <!id "user1"!> { name "Alice" role "admin" }
        <!id "user2"!> { name "Bob" role "user" }
        <!id "user3"!> { name "Charlie" role "moderator" }
    ]

    config {
        <!id "app-config"!>
        debug ~true
        version "1.0.0"

        database {
            <!id "db-config"!>
            host "localhost"
            port *5432
        }
    }
}
""";

var document = XferParser.Parse(xferContent);

Console.WriteLine("Testing Fast ID Lookup Implementation");
Console.WriteLine("=====================================");

// Test all the elements with IDs
var testIds = new[] { "root", "user1", "user2", "user3", "app-config", "db-config" };

Console.WriteLine("\n1. Testing GetElementById (fast lookup):");
foreach (var id in testIds)
{
    var element = document.GetElementById(id);
    Console.WriteLine($"   {id}: {(element != null ? $"Found ({element.GetType().Name})" : "Not found")}");
}

Console.WriteLine("\n2. Testing FindElementById (tree traversal):");
foreach (var id in testIds)
{
    var element = document.FindElementById(id);
    Console.WriteLine($"   {id}: {(element != null ? $"Found ({element.GetType().Name})" : "Not found")}");
}

// Test ContainsElementId
Console.WriteLine("\n3. Testing ContainsElementId:");
foreach (var id in testIds)
{
    var exists = document.ContainsElementId(id);
    Console.WriteLine($"   {id}: {exists}");
}

Console.WriteLine($"\n   nonexistent-id: {document.ContainsElementId("nonexistent-id")}");

// Test GetAllElementsWithIds
Console.WriteLine("\n4. All elements with IDs:");
var allElements = document.GetAllElementsWithIds();
foreach (var kvp in allElements)
{
    Console.WriteLine($"   {kvp.Key}: {kvp.Value.GetType().Name}");
}

// Performance comparison
Console.WriteLine("\n5. Performance Comparison (1000 lookups each):");
const int iterations = 1000;

// Test fast lookup performance
var sw = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
{
    foreach (var id in testIds)
    {
        document.GetElementById(id);
    }
}
sw.Stop();
var fastTime = sw.ElapsedMilliseconds;

// Test tree traversal performance
sw.Restart();
for (int i = 0; i < iterations; i++)
{
    foreach (var id in testIds)
    {
        document.FindElementById(id);
    }
}
sw.Stop();
var slowTime = sw.ElapsedMilliseconds;

Console.WriteLine($"   Fast lookup (GetElementById):     {fastTime}ms");
Console.WriteLine($"   Tree traversal (FindElementById): {slowTime}ms");
if (fastTime > 0)
{
    Console.WriteLine($"   Speed improvement: {(double)slowTime / fastTime:F1}x faster");
}
else
{
    Console.WriteLine($"   Fast lookup was too fast to measure accurately!");
}

Console.WriteLine("\n6. Testing invalid IDs:");
Console.WriteLine($"   GetElementById(null): {document.GetElementById(null)}");
Console.WriteLine($"   GetElementById(\"\"): {document.GetElementById("")}");
Console.WriteLine($"   GetElementById(\"invalid\"): {document.GetElementById("invalid")}");

// Test that both methods return the same results
Console.WriteLine("\n7. Verifying consistency between fast and slow lookup:");
bool allConsistent = true;
foreach (var id in testIds)
{
    var fastResult = document.GetElementById(id);
    var slowResult = document.FindElementById(id);
    bool consistent = ReferenceEquals(fastResult, slowResult);
    if (!consistent)
    {
        Console.WriteLine($"   INCONSISTENCY for {id}: fast={fastResult?.GetType().Name}, slow={slowResult?.GetType().Name}");
        allConsistent = false;
    }
}

if (allConsistent)
{
    Console.WriteLine("   ✓ All lookups are consistent between fast and slow methods");
}

Console.WriteLine("\nFast ID lookup test completed successfully!");
