using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing instruction for assigning unique identifiers to elements in XferLang.
/// The ID processing instruction associates a string identifier with an element,
/// enabling element referencing and uniqueness validation within the document.
/// </summary>
public class IdProcessingInstruction : ProcessingInstruction {
    /// <summary>
    /// The keyword used to identify ID processing instructions.
    /// </summary>
    public const string Keyword = "id";

    /// <summary>
    /// Initializes a new instance of the IdProcessingInstruction class with the specified ID value.
    /// </summary>
    /// <param name="value">The text element containing the ID value to assign to the target element.</param>
    public IdProcessingInstruction(TextElement value) : base(value, Keyword) { }

    /// <summary>
    /// Handles element processing by assigning the ID value to the target element.
    /// Sets the element's Id property to the string representation of the processing instruction's value.
    /// </summary>
    /// <param name="element">The element to assign the ID to.</param>
    public override void ElementHandler(Element element) {
        element.Id = Value.ToString();
    }
}
