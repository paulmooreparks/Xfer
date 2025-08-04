using System;

namespace ParksComputing.Xfer.Lang.Models
{
    /// <summary>
    /// Represents a simple person model used for testing and demonstration purposes.
    /// Contains basic personal information properties for serialization examples.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Gets or sets the person's name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the person's age in years.
        /// </summary>
        public int Age { get; set; }
    }
}
