namespace ParksComputing.Xfer.Lang;

public class ParseError {
    public string Message { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public string? Context { get; set; }

    public ParseError(string message, int row, int column, string? context = null) {
        Message = message;
        Row = row;
        Column = column;
        Context = context;
    }

    public override string ToString() => $"Error at row {Row}, column {Column}: {Message}";
}
