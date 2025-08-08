using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions;

/// <summary>
/// Processing instruction for assigning a single tag to elements in XferLang.
/// The tag processing instruction associates a string tag with an element,
/// enabling element categorization and group selection within the document.
/// Multiple elements can share the same tag, but each element can have only one tag.
/// </summary>
public class TagProcessingInstruction : ProcessingInstruction {
    /// <summary>
    /// The keyword used to identify tag processing instructions.
    /// </summary>
    public const string Keyword = "tag";

    /// <summary>
    /// Initializes a new instance of the TagProcessingInstruction class with the specified tag value.
    /// </summary>
    /// <param name="value">The text element containing the tag value to assign to the target element.</param>
    public TagProcessingInstruction(TextElement value) : base(value, Keyword) { }

    /// <summary>
    /// Handles element processing by setting the tag value on the target element.
    /// Throws an exception if the element already has a tag assigned.
    /// </summary>
    /// <param name="element">The element to assign the tag to.</param>
    /// <exception cref="InvalidOperationException">Thrown when the element already has a tag assigned.</exception>
    public override void ElementHandler(Element element) {
        var tagName = Value.ToString();
        if (string.IsNullOrEmpty(tagName)) {
            return; // Ignore empty tags
        }

        if (!string.IsNullOrEmpty(element.Tag)) {
            throw new InvalidOperationException($"Element already has tag '{element.Tag}'. Cannot assign tag '{tagName}' to the same element.");
        }

        element.Tag = tagName;
    }
}
