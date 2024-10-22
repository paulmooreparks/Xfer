namespace ParksComputing.Xfer
{
    public class XferDocument
    {
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<object> Content { get; set; } = new();
    }
}
