using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

var parser = new Parser();

// Test case 1: PI for array
Console.WriteLine("=== Test 1: PI for array ===");
var input1 = "<! id \"array\" !> [<! id \"one\"!> 1 2 3]";
try {
    var doc1 = parser.Parse(input1);
    var tuple1 = doc1.Root as TupleElement;
    if (tuple1 != null) {
        foreach (var child in tuple1.Children) {
            Console.WriteLine($"Type: {child.GetType().Name}, Id: '{child.Id}'");
            if (child is ArrayElement arr) {
                foreach (var arrChild in arr.Children) {
                    Console.WriteLine($"  Array child - Type: {arrChild.GetType().Name}, Id: '{arrChild.Id}'");
                }
            }
        }
    }
} catch (Exception ex) {
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine();

// Test case 2: PI for key-value pair in object
Console.WriteLine("=== Test 2: PI for key-value pair in object ===");
var input2 = "{ <! id \"testpi\" !> a 'value' }";
try {
    var doc2 = parser.Parse(input2);
    var obj = doc2.Root as ObjectElement;
    if (obj != null) {
        foreach (var kvp in obj.Dictionary.Values) {
            Console.WriteLine($"Key: {kvp.Key}, Id: '{kvp.Id}', Type: {kvp.GetType().Name}");
        }
    }
} catch (Exception ex) {
    Console.WriteLine($"Error: {ex.Message}");
}
