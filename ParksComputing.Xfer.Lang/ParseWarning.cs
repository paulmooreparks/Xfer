namespace ParksComputing.Xfer.Lang;

/// <summary>
/// Represents a warning encountered during XferLang document parsing.
/// Warnings indicate potential issues that don't prevent parsing but may affect document interpretation.
/// </summary>
public class ParseWarning {
    /// <summary>
    /// Gets or sets the warning message describing the issue.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the row number where the warning occurred (1-based).
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets the column number where the warning occurred (1-based).
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the type of warning that occurred.
    /// </summary>
    public WarningType Type { get; set; }

    /// <summary>
    /// Gets or sets additional context information related to the warning, if available.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseWarning"/> class.
    /// </summary>
    /// <param name="type">The type of warning.</param>
    /// <param name="message">The warning message.</param>
    /// <param name="row">The row number where the warning occurred (1-based).</param>
    /// <param name="column">The column number where the warning occurred (1-based).</param>
    /// <param name="context">Optional additional context information.</param>
    public ParseWarning(WarningType type, string message, int row, int column, string? context = null) {
        Type = type;
        Message = message;
        Row = row;
        Column = column;
        Context = context;
    }

    /// <summary>
    /// Returns a string representation of the warning including location information.
    /// </summary>
    /// <returns>A formatted string describing the warning and its location.</returns>
    public override string ToString() => $"Warning at row {Row}, column {Column}: {Message}";
}

/// <summary>
/// Specifies the types of warnings that can occur during XferLang parsing.
/// </summary>
public enum WarningType {
    /// <summary>
    /// A character name could not be resolved to a valid character code point.
    /// </summary>
    CharacterResolutionFailure,

    /// <summary>
    /// Precision may be lost during numeric conversion.
    /// </summary>
    NumericPrecisionLoss,

    /// <summary>
    /// An unregistered processing instruction was encountered.
    /// </summary>
    UnregisteredProcessingInstruction,

    /// <summary>
    /// A character element was empty and a replacement character was used.
    /// </summary>
    EmptyCharacterElement,

    /// <summary>
    /// A character value was invalid and a fallback was used.
    /// </summary>
    InvalidCharacterValue,

    /// <summary>
    /// An if processing instruction referenced an unknown/unregistered operator (condition treated as false).
    /// </summary>
    UnknownConditionalOperator
}
