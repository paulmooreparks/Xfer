using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.ProcessingInstructions {
    /// <summary>
    /// Processing instruction for storing metadata information in XferLang documents.
    /// Provides a way to attach arbitrary metadata such as authorship, creation dates,
    /// versioning information, and other descriptive properties to elements or documents.
    /// </summary>
    public class MetaProcessingInstruction : ProcessingInstruction {
        /// <summary>
        /// The keyword used to identify metadata processing instructions.
        /// </summary>
        public const string Keyword = "meta";

        /// <summary>
        /// Gets or sets the metadata object containing structured metadata information.
        /// This property shadows the base Metadata property to provide strongly-typed access.
        /// </summary>
        public new XferMetadata Metadata { get; set; } = new XferMetadata();

        /// <summary>
        /// Initializes a new instance of the MetaProcessingInstruction class with the specified metadata object.
        /// </summary>
        /// <param name="value">The object element containing metadata key-value pairs.</param>
        public MetaProcessingInstruction(ObjectElement value) : base(value, Keyword) { }

    }
}
