namespace ParksComputing.Xfer.Lang;

/// <summary>
/// Represents an error encountered during XferLang document parsing.
/// Parse errors indicate syntax or structural problems that prevent successful parsing.
/// </summary>
public class ParseError {
    /// <summary>
    /// Gets or sets the error message describing the issue.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the row number where the error occurred (1-based).
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets the column number where the error occurred (1-based).
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets additional context information related to the error, if available.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseError"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="row">The row number where the error occurred (1-based).</param>
    /// <param name="column">The column number where the error occurred (1-based).</param>
    /// <param name="context">Optional additional context information.</param>
    public ParseError(string message, int row, int column, string? context = null) {
        Message = message;
        Row = row;
        Column = column;
        Context = context;
    }

    /// <summary>
    /// Returns a string representation of the error including location information.
    /// </summary>
    /// <returns>A formatted string describing the error and its location.</returns>
    public override string ToString() => $"Error at row {Row}, column {Column}: {Message}";
}
