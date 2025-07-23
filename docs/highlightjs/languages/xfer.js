/*
Language: XferLang
Description: XferLang is a human-readable data interchange format with explicit typing
Author: Parks Computing
Website: https://github.com/paulmooreparks/Xfer
*/

(function() {
    'use strict';

    function xferLanguage(hljs) {
        // Common patterns
        const IDENTIFIER = /[a-zA-Z_][a-zA-Z0-9_]*/;
        const DECIMAL_NUMBER = /[+-]?\d+(\.\d+)?([eE][+-]?\d+)?/;
        const HEX_NUMBER = /0[xX][0-9a-fA-F]+/;
        const BINARY_NUMBER = /0[bB][01]+/;

        return {
            name: 'XferLang',
            aliases: ['xfer'],
            case_insensitive: false,

            contains: [
                // STRINGS MUST COME FIRST - highest priority to prevent internal highlighting
                // String literals with explicit delimiters: <"...">
                {
                    className: 'string',
                    begin: /<"/,
                    end: /">/,
                    relevance: 10
                },

                // Regular string literals: "..."
                {
                    className: 'string',
                    begin: /"/,
                    end: /"/,
                    relevance: 10
                },

                // Evaluated strings: '...'
                {
                    className: 'string',
                    begin: /'/,
                    end: /'/,
                    relevance: 10
                },

                // Comments: </ ... />
                {
                    className: 'comment',
                    begin: /<\//,
                    end: /\/>/,
                    relevance: 5
                },

                // Metadata: <! ... !>
                {
                    className: 'meta',
                    begin: /<!/,
                    end: /!>/,
                    relevance: 5
                },

                // ISO 8601 dates and times - must come before operators to catch @ symbols
                {
                    className: 'number',
                    begin: /@\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2}(\.\d{1,3})?(Z|[+-]\d{2}:\d{2})?)?@/,
                    relevance: 5
                },

                // Time spans - must come before operators
                {
                    className: 'number',
                    begin: /@P(?:\d+Y)?(?:\d+M)?(?:\d+D)?(?:T(?:\d+H)?(?:\d+M)?(?:\d+(?:\.\d+)?S)?)?@/,
                    relevance: 5
                },

                // Boolean literals - specific patterns before general operators
                {
                    className: 'literal',
                    begin: /\b(true|false)\b/,
                    relevance: 4
                },

                // Character keywords - specific patterns before general operators
                {
                    className: 'built_in',
                    begin: /\b(newline|tab|space|cr|lf|crlf)\b/,
                    relevance: 4
                },

                // Hexadecimal numbers - must come before # operator
                {
                    className: 'number',
                    begin: HEX_NUMBER,
                    relevance: 3
                },

                // Binary numbers - must come before operators
                {
                    className: 'number',
                    begin: BINARY_NUMBER,
                    relevance: 3
                },

                // Decimal numbers
                {
                    className: 'number',
                    begin: DECIMAL_NUMBER,
                    relevance: 2
                },

                // Type prefixes and operators - LOWER priority so they don't override strings
                {
                    className: 'operator',
                    begin: /[#*~?@\\$]/,
                    relevance: 1
                },

                // Delimiters - also lower priority
                {
                    className: 'punctuation',
                    begin: /[\[\]{}()<>]/,
                    relevance: 1
                },

                // Identifiers and property names - lowest priority
                {
                    className: 'attr',
                    begin: IDENTIFIER,
                    relevance: 0
                }
            ]
        };
    }

    // Register the language
    if (typeof hljs !== 'undefined') {
        hljs.registerLanguage('xfer', xferLanguage);
    }
})();
