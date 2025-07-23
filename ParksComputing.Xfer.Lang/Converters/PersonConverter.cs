using System;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.Configuration;
using ParksComputing.Xfer.Lang.Models;

namespace ParksComputing.Xfer.Lang.Converters
{
    public class PersonConverter : XferConverter<Person>
    {
        public override Element WriteXfer(Person value, XferSerializerSettings settings)
        {
            return new StringElement($"{value.Name},{value.Age}");
        }

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
