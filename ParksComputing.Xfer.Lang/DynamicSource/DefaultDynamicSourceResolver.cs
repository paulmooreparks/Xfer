using System;
using System.IO;
using ParksComputing.Xfer.Lang.Elements;

namespace ParksComputing.Xfer.Lang.DynamicSource {
    public class DefaultDynamicSourceResolver : IDynamicSourceResolver {
        public virtual string? Resolve(string key, XferDocument document) {
            // Look for PI directive: dynamicSource
            foreach (var meta in document.Root.Values) {
                if (meta is MetadataElement metaElem && metaElem.ContainsKey("dynamicSource")) {
                    var dsElem = metaElem["dynamicSource"];
                    ObjectElement? obj = null;
                    Element candidate = dsElem;
                    if (candidate is KeyValuePairElement kvElem) {
                        candidate = kvElem.Value;
                    }

                    obj = candidate as ObjectElement;
                    if (obj != null && obj.ContainsKey(key)) {
                        Element? currentElem = obj[key];
                        while (currentElem is KeyValuePairElement kvElem2) {
                            currentElem = kvElem2.Value;
                        }

                        string? sourceStr = null;
                        if (currentElem is StringElement strElem) {
                            sourceStr = strElem.Value;
                        }
                        else {
                            sourceStr = currentElem?.ToString();
                        }

                        if (sourceStr != null) {
                            if (sourceStr.StartsWith("file:")) {
                                var filePath = sourceStr.Substring(5);
                                if (File.Exists(filePath)) {
                                    return File.ReadAllText(filePath);
                                }
                            }
                            if (sourceStr.StartsWith("env:")) {
                                var envVar = sourceStr.Substring(4);
                                return Environment.GetEnvironmentVariable(envVar);
                            }
                            // Add other source types here
                        }
                    }
                    // Legacy single-key support
                    string? piKey = null;
                    string? sourceSingle = null;
                    if (obj != null) {
                        foreach (var kvp in obj.Values) {
                            if (kvp.Key == "key") {
                                piKey = kvp.Value.Value?.ToString();
                            }

                            if (kvp.Key == "source") {
                                sourceSingle = kvp.Value.Value?.ToString();
                            }
                        }
                    }
                    if (piKey == key && sourceSingle != null) {
                        if (sourceSingle.StartsWith("file:")) {
                            var filePath = sourceSingle.Substring(5);
                            if (File.Exists(filePath)) {
                                return File.ReadAllText(filePath);
                            }
                        }
                        if (sourceSingle.StartsWith("env:")) {
                            var envVar = sourceSingle.Substring(4);
                            return Environment.GetEnvironmentVariable(envVar);
                        }
                        // Add other source types here
                    }
                }
            }
            // Fallback: environment variable by key
            return Environment.GetEnvironmentVariable(key);
        }
    }
}
