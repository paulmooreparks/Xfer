using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

var parser = new Parser();
var input = "{ <! id \"testpi\" !> a 'value' }";

try {
    var doc = parser.Parse(input);
    var obj = doc.Root as ObjectElement;

    if (obj != null) {
        foreach (var kvp in obj.Dictionary.Values) {
            Console.WriteLine($"Key: {kvp.Key}, Id: {kvp.Id}");
        }
    }
} catch (Exception ex) {
    Console.WriteLine($"Error: {ex.Message}");
}
