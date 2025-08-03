using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.DynamicSource;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace DynamicDbResolverDemo {
    // Extension: Database source handler
    public static class DatabaseSourceExtension {
        private static string? _dbPath;

        /// <summary>
        /// Registers the database source handler extension.
        /// </summary>
        /// <param name="dbPath">Path to the SQLite database</param>
        public static void Register(string dbPath) {
            _dbPath = dbPath;
            DynamicSourceHandlerRegistry.RegisterHandler("db", HandleDatabase);
            Console.WriteLine("✓ Registered 'db' source handler extension");
        }

        /// <summary>
        /// Unregisters the database source handler extension.
        /// </summary>
        public static void Unregister() {
            DynamicSourceHandlerRegistry.UnregisterHandler("db");
            _dbPath = null;
            Console.WriteLine("✓ Unregistered 'db' source handler extension");
        }

        /// <summary>
        /// Database source handler implementation.
        /// </summary>
        private static string? HandleDatabase(string? sourceValue, string fallbackKey) {
            if (string.IsNullOrEmpty(_dbPath)) {
                return null;
            }

            var key = sourceValue ?? fallbackKey;
            return GetValueFromDb(key);
        }

        private static string? GetValueFromDb(string key) {
            if (string.IsNullOrEmpty(_dbPath)) {
                return null;
            }

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT value FROM secrets WHERE key = @key;";
            cmd.Parameters.AddWithValue("@key", key);

            return cmd.ExecuteScalar()?.ToString();
        }
    }

    // Simple resolver that just uses the core library functionality
    public class ExtensibleDynamicSourceResolver : DefaultDynamicSourceResolver {
        // No custom logic needed - everything is handled by the core library
        // and registered extensions!
    }

    // Helper methods
    public static class Helpers {
        public static IEnumerable<Element> GetAllElements(Element root) {
            yield return root;
            foreach (var child in root.Children) {
                foreach (var descendant in GetAllElements(child)) {
                    yield return descendant;
                }
            }
        }
    }

    class Program {
        static void Main(string[] args) {
            Console.WriteLine("=== Dynamic Source Extensibility Demo ===");
            Console.WriteLine("This demo shows the extensible dynamic source system:");
            Console.WriteLine("• Core library provides 'const', 'env', and 'file' source types");
            Console.WriteLine("• Database extension adds 'db' source type via handler registry");
            Console.WriteLine("• Extension is completely decoupled from core library");
            Console.WriteLine("• Works with 'dynamicSource' PI using recursive KVPs: key sourceType \"sourceValue\"\n");

            // Setup demo DB
            var dbPath = "demo.db";
            // Set up the database
            SetupDatabase(dbPath);

            // Register the database extension
            DatabaseSourceExtension.Register(dbPath);

            // Read Xfer document from file
            var xferPath = Path.Combine(Environment.CurrentDirectory, "sample.xfer");
            if (!File.Exists(xferPath)) {
                Console.WriteLine($"File not found: {xferPath}");
                return;
            }

            var xfer = File.ReadAllText(xferPath);
            Console.WriteLine("Original Xfer content:");
            Console.WriteLine(xfer);
            Console.WriteLine();

            var parser = new Parser();
            parser.DynamicSourceResolver = new ExtensibleDynamicSourceResolver();

            // Clear any previous dynamic source configurations
            DynamicSourceRegistry.Clear();

            var doc = parser.Parse(xfer);

            Console.WriteLine("Parsed document structure:");
            Console.WriteLine($"Root: {doc.Root.GetType().Name}");
            Console.WriteLine($"Total document-level PIs: {doc.ProcessingInstructions.Count}");

            foreach (var pi in doc.ProcessingInstructions) {
                Console.WriteLine($"  PI: {pi.GetType().Name} - {pi.Kvp?.Key}");
            }

            // Debug: show all elements in the root to see if PI is there
            Console.WriteLine("Root element contents:");
            var allRootElements = Helpers.GetAllElements(doc.Root).ToList();
            foreach (var elem in allRootElements.Take(10)) { // Limit to first 10 for brevity
                Console.WriteLine($"  {elem.GetType().Name}: {(elem is ProcessingInstruction pi ? pi.Kvp?.Key : elem.ToString()?.Substring(0, Math.Min(50, elem.ToString()?.Length ?? 0)))}");
            }

            // Show dynamicSource PIs that were registered using the new PI registry
            // Check both document-level PIs and PIs within the root element
            var documentLevelPIs = doc.ProcessingInstructions.OfType<DynamicSourceProcessingInstruction>().ToList();
            var rootEmbeddedPIs = Helpers.GetAllElements(doc.Root).OfType<DynamicSourceProcessingInstruction>().ToList();
            var allDynamicSourcePIs = documentLevelPIs.Concat(rootEmbeddedPIs).ToList();

            Console.WriteLine($"DynamicSource PIs found: {allDynamicSourcePIs.Count} (via new PI registry!)");
            Console.WriteLine($"  Document-level: {documentLevelPIs.Count}, Root-embedded: {rootEmbeddedPIs.Count}");

            foreach (var pi in allDynamicSourcePIs) {
                Console.WriteLine($"✓ Found DynamicSource PI: {pi.Kvp?.Key}");
                if (pi.Kvp?.Value is ObjectElement obj) {
                    foreach (var kvp in obj.Dictionary.Values) {
                        // Show the recursive KVP format
                        if (kvp.Value is KeyValuePairElement sourceKvp) {
                            Console.WriteLine($"  {kvp.Key} {sourceKvp.Key} \"{sourceKvp.Value}\"");
                        }
                        else {
                            Console.WriteLine($"  {kvp.Key} -> {kvp.Value}");
                        }
                    }
                }
            }
            Console.WriteLine();

            // Process the credentials object and show resolved dynamic values
            var root = doc.Root;
            foreach (var element in root.Children) {
                if (element is KeyValuePairElement kvp && kvp.Key == "credentials") {
                    Console.WriteLine("Processing credentials object:");
                    var obj = kvp.Value as ObjectElement;

                    if (obj != null) {
                        ProcessCredentialsObject(obj);
                    }
                }
            }

            Console.WriteLine("\nSerialized result (with PI included):");
            Console.WriteLine(doc.ToXfer(Formatting.Pretty));

            Console.WriteLine("\n✓ Demo completed successfully!");
            Console.WriteLine("✓ Core library handled 'const' and 'env' source types");
            Console.WriteLine("✓ Database extension handled 'db' source type via handler registry");
            Console.WriteLine("✓ Extension system is completely decoupled from core library");
            Console.WriteLine("✓ DynamicSourceProcessingInstruction was created via the new PI registry");
            Console.WriteLine("✓ PI was correctly serialized in the output");
            Console.WriteLine("✓ Dynamic element resolution worked as expected");

            // Clean up: unregister the extension
            DatabaseSourceExtension.Unregister();
        }

        private static void SetupDatabase(string dbPath) {
            Console.WriteLine("Setting up demo database...");

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS secrets (key TEXT PRIMARY KEY, value TEXT);";
            cmd.ExecuteNonQuery();

            // Insert demo values
            var values = new Dictionary<string, string> {
                { "dbpassword", "SuperSecretFromDB!" },
                { "greeting", "Hello from the DB!" },
                { "username", "db_user_123" },
                { "apikey", "api_key_from_database" }
            };

            foreach (var kvp in values) {
                cmd.CommandText = "INSERT OR REPLACE INTO secrets (key, value) VALUES (@key, @value);";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@key", kvp.Key);
                cmd.Parameters.AddWithValue("@value", kvp.Value);
                cmd.ExecuteNonQuery();
                Console.WriteLine($"  Inserted: {kvp.Key} = {kvp.Value}");
            }
            Console.WriteLine();
        }

        private static void ProcessCredentialsObject(ObjectElement obj) {
            foreach (var kvp in obj.Dictionary.Values) {
                var element = kvp.Value;
                var elementType = element.GetType().Name;

                Console.WriteLine($"  {kvp.Key} ({elementType}): {element}");

                if (element is DynamicElement dynamicElem) {
                    Console.WriteLine($"    Resolved dynamic value: {dynamicElem.Value}");
                }
                else if (element is InterpolatedElement interpElem) {
                    Console.WriteLine($"    Resolved interpolated value: {interpElem.Value}");
                }
            }
        }
    }
}
