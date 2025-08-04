using System;
using System.IO;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.ProcessingInstructions;
using ParksComputing.Xfer.Lang.Services;

namespace ParksComputing.Xfer.Lang.DynamicSource {
    /// <summary>
    /// Default implementation of dynamic source resolution for XferLang.
    /// Resolves dynamic values from files, environment variables, constants,
    /// and the dynamic source registry. Provides backward compatibility
    /// with legacy dynamic source processing instructions.
    /// </summary>
    public class DefaultDynamicSourceResolver : IDynamicSourceResolver {
        // Legacy constants for backward compatibility
        private const string DynamicSourceKey = "dynamicSource";
        private const string FileKeyword = "file";
        private const string EnvKeyword = "env";
        private const string ConstKeyword = "const";

        public virtual string? Resolve(string key, XferDocument document) {
            // First, try the new DynamicSourceRegistry (from dynamicSource PIs)
            var result = DynamicSourceRegistry.Resolve(key);
            if (result != null) {
                return result;
            }

            // Fallback to legacy PI scanning (for backward compatibility)
            return ResolveLegacy(key, document);
        }

        /// <summary>
        /// Legacy resolution method for backward compatibility with old PI format.
        /// </summary>
        protected virtual string? ResolveLegacy(string key, XferDocument document) {
            // Look for PI directive: dynamicSource
            foreach (var meta in document.Root.Values) {
                if (meta is ProcessingInstruction metaElem && metaElem.Kvp?.Key == DynamicSourceKey) {
                    // The value for the dynamicSource PI should be an ObjectElement
                    if (metaElem.Kvp.Value is ObjectElement obj && obj.ContainsKey(key)) {
                        Element? currentElem = obj[key];
                        // If the value is a KeyValuePairElement, use its key as the keyword and its value as the argument
                        if (currentElem is KeyValuePairElement innerKvp) {
                            var keyword = innerKvp.Key;
                            var valueElem = innerKvp.Value;
                            string? valueStr = valueElem is StringElement se ? se.Value : valueElem?.ToString();
                            if (!string.IsNullOrEmpty(keyword) && !string.IsNullOrEmpty(valueStr)) {
                                if (string.Equals(keyword, FileKeyword, StringComparison.OrdinalIgnoreCase)) {
                                    if (File.Exists(valueStr)) {
                                        return File.ReadAllText(valueStr);
                                    }
                                }
                                else if (string.Equals(keyword, EnvKeyword, StringComparison.OrdinalIgnoreCase)) {
                                    return Environment.GetEnvironmentVariable(valueStr);
                                }
                                else if (string.Equals(keyword, ConstKeyword, StringComparison.OrdinalIgnoreCase)) {
                                    return valueStr;
                                }
                                // Add other keyword-based sources here
                            }
                        }
                        else if (currentElem is StringElement strElem) {
                            // Fallback: treat as const
                            return strElem.Value;
                        }
                        else if (currentElem != null) {
                            // Fallback: treat as const
                            return currentElem.ToString();
                        }
                    }
                }
            }
            // Fallback: environment variable by key
            return Environment.GetEnvironmentVariable(key);
        }
    }
}
