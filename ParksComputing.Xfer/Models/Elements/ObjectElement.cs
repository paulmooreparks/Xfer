﻿using System.Text;

using ParksComputing.Xfer.Extensions;

namespace ParksComputing.Xfer.Models.Elements;

public class ObjectElement : Element {
    public const char OpeningMarker = '{';
    public const char ClosingMarker = '}';

    private Dictionary<string, Tuple<Element, Element>> _values = new();
    public IReadOnlyDictionary<string, Tuple<Element, Element>> Values => _values;

    public Element this[string index] {
        get {
            return _values[index].Item2;
        }
        set {
            SetOrUpdateValue(index, value);
        }
    }

    public ObjectElement() : base("object", new(OpeningMarker, ClosingMarker)) { }

    private void SetOrUpdateValue<TElement>(string key, TElement element) where TElement : Element {
        if (_values.TryGetValue(key, out Tuple<Element, Element>? tuple)) {
            _values[key] = new Tuple<Element, Element>(tuple.Item1, element);
        }
        else {
            Element keyElement;

            if (key.IsKeywordString()) {
                keyElement = new KeywordElement(key);
            }
            else {
                keyElement = new StringElement(key);
            }

            _values.Add(key, new Tuple<Element, Element>(keyElement, element));
        }
    }

    public void AddOrUpdate(KeyValuePairElement value) {
        this[value.Key] = value.Value;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Delimiter.Opening);
        foreach (var value in _values.Values) {
            sb.Append($"{value.Item1}{value.Item2}");
        }
        sb.Append(Delimiter.Closing);
        return sb.ToString();
    }
}
