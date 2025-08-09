using ParksComputing.Xfer.Lang.Elements;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var invalidIdent = new IdentifierElement("invalid identifier");
            Console.WriteLine($"Invalid identifier style: {invalidIdent.Delimiter.Style}");
            Console.WriteLine($"Invalid identifier ToXfer: '{invalidIdent.ToXfer()}'");

            var endsWithColon = new IdentifierElement("identifier:");
            Console.WriteLine($"Ends with colon style: {endsWithColon.Delimiter.Style}");
            Console.WriteLine($"Ends with colon ToXfer: '{endsWithColon.ToXfer()}'");

            var validIdent = new IdentifierElement("validIdentifier");
            Console.WriteLine($"Valid identifier style: {validIdent.Delimiter.Style}");
            Console.WriteLine($"Valid identifier ToXfer: '{validIdent.ToXfer()}'");

            var withMultipleColons = new IdentifierElement("te::st");
            Console.WriteLine($"With double colons style: {withMultipleColons.Delimiter.Style}");
            Console.WriteLine($"With double colons ToXfer: '{withMultipleColons.ToXfer()}'");
            Console.WriteLine($"With double colons SpecifierCount: {withMultipleColons.Delimiter.SpecifierCount}");
        }
    }
}
