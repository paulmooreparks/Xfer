namespace ParksComputing.Xfer.Lang;

public class ParseWarning {
    public string Message { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public WarningType Type { get; set; }
    public string? Context { get; set; }

    public ParseWarning(WarningType type, string message, int row, int column, string? context = null) {
        Type = type;
        Message = message;
        Row = row;
        Column = column;
        Context = context;
    }

    public override string ToString() => $"Warning at row {Row}, column {Column}: {Message}";
}

public enum WarningType {
    CharacterResolutionFailure,
    NumericPrecisionLoss,
    UnregisteredProcessingInstruction,
    EmptyCharacterElement
}
