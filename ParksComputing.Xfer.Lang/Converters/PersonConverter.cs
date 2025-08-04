using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.Models;

namespace ParksComputing.Xfer.Lang.Converters
{
    /// <summary>
    /// Example custom converter for Person objects.
    /// Demonstrates how to implement specialized serialization logic
    /// by converting Person objects to/from comma-separated string format.
    /// </summary>
    public class PersonConverter : XferConverter<Person>
    {
        /// <summary>
        /// Converts a Person object to a XferLang StringElement in "Name,Age" format.
        /// </summary>
        /// <param name="value">The Person object to convert.</param>
        /// <param name="settings">Serializer settings (not used in this implementation).</param>
        /// <returns>A StringElement containing the comma-separated name and age.</returns>
        public override Element WriteXfer(Person value, XferSerializerSettings settings)
        {
            return new StringElement($"{value.Name},{value.Age}");
        }

        /// <summary>
        /// Converts a XferLang element back to a Person object from "Name,Age" format.
        /// </summary>
        /// <param name="element">The XferLang element to convert (expected to be StringElement).</param>
        /// <param name="settings">Serializer settings (not used in this implementation).</param>
        /// <returns>A Person object with parsed name and age.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the element cannot be converted to Person.</exception>
        public override Person ReadXfer(Element element, XferSerializerSettings settings)
        {
            if (element is StringElement stringElement)
            {
                var parts = stringElement.Value.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[1], out int age))
                {
                    return new Person { Name = parts[0], Age = age };
                }
            }
            throw new InvalidOperationException("Cannot convert element to Person.");
        }
    }
}
