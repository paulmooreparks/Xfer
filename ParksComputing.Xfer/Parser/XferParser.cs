using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using ParksComputing.Xfer.Services;

namespace ParksComputing.Xfer.Parser {
    public class XferParser {
        private string _input = string.Empty;
        private int _position = 0;

        public XferParser() {
        }

        // Entry point for parsing
        public XferDocument Parse(string input) {
            _input = input;
            _position = 0;
            var parser = new Services.Parser();
            var tokens = parser.Parse(input);

            /*
            SkipWhitespace();
            var metadata = ParseMetadata();
            SkipWhitespace();
            var content = ParsePropertyBag();
            return new XferDocument { Metadata = metadata, Content = content };
            */
            return new XferDocument();
        }

        private Dictionary<string, object> ParseMetadata() {
            if (Match("<@")) {
                var metadata = ParseObject("@>");
                return metadata;
            }
            return new Dictionary<string, object>();
        }

        private List<object> ParsePropertyBag() {
            Expect("<(");
            var elements = new List<object>();
            while (!Match(")>")) {
                elements.Add(ParseElement());
                SkipWhitespace();
            }
            return elements;
        }

        private object ParseElement() {
            SkipWhitespace();

            if (Match("<{"))
                return ParseObject("}>");
            if (Match("<["))
                return ParseArray("]>");
            if (Match("<\""))
                return ParseString("\">");
            if (Match("<<"))
                return ParseUntypedText(">>");
            if (Match("<#"))
                return ParseNumeric("#>");
            if (Match("<&"))
                return ParseIsoDate("&>");
            if (Match("<\\")) {
                return ParseUtf8Character("\\>");
            }

            throw new Exception($"Unexpected token at position {_position}");
        }

        private Dictionary<string, object> ParseObject(string terminator) {
            var obj = new Dictionary<string, object>();
            while (!Match(terminator)) {
                SkipWhitespace();
                var key = ParseString("\">");
                SkipWhitespace();
                var value = ParseElement();
                obj[key] = value;
                SkipWhitespace();
            }
            return obj;
        }

        private List<object> ParseArray(string terminator) {
            var array = new List<object>();
            while (!Match(terminator)) {
                SkipWhitespace();
                array.Add(ParseElement());
                SkipWhitespace();
            }
            return array;
        }

        private string ParseString(string terminator) {
            var value = new StringBuilder();
            while (!Match(terminator)) {
                value.Append(Current());
                Advance();
            }
            return value.ToString();
        }

        private string ParseUntypedText(string terminator) {
            var value = new StringBuilder();
            while (!Match(terminator)) {
                value.Append(Current());
                Advance();
            }
            return value.ToString();
        }

        private int ParseNumeric(string terminator) {
            var value = new StringBuilder();
            while (!Match(terminator)) {
                value.Append(Current());
                Advance();
            }
            return int.Parse(value.ToString());
        }

        private DateTime ParseIsoDate(string terminator) {
            var value = new StringBuilder();
            while (!Match(terminator)) {
                value.Append(Current());
                Advance();
            }
            return DateTime.Parse(value.ToString());
        }

        private char ParseUtf8Character(string terminator) {
            var value = new StringBuilder();
            while (!Match(terminator)) {
                value.Append(Current());
                Advance();
            }
            return (char)int.Parse(value.ToString(), System.Globalization.NumberStyles.HexNumber);
        }

        // Utility Methods

        private void SkipWhitespace() {
            while (char.IsWhiteSpace(Current()))
                Advance();
        }

        private bool Match(string expected) {
            if (_input.Substring(_position).StartsWith(expected)) {
                _position += expected.Length;
                return true;
            }
            return false;
        }

        private void Expect(string expected) {
            if (!Match(expected))
                throw new Exception($"Expected '{expected}' at position {_position}");
        }

        private char Current() {
            return _position < _input.Length ? _input[_position] : '\0';
        }

        private void Advance() {
            _position++;
        }
    }
}
