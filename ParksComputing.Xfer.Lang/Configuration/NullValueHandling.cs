namespace ParksComputing.Xfer.Lang.Configuration {
    /// <summary>
    /// Specifies how null values should be handled during XferLang serialization.
    /// </summary>
    public enum NullValueHandling {
        /// <summary>
        /// Include null values in the serialized output.
        /// </summary>
        Include,

        /// <summary>
        /// Ignore (exclude) null values from the serialized output.
        /// </summary>
        Ignore
    }
}
