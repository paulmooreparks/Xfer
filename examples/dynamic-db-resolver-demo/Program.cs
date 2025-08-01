using System;
using Microsoft.Data.Sqlite;
using ParksComputing.Xfer.Lang;
using ParksComputing.Xfer.Lang.Services;
using ParksComputing.Xfer.Lang.Elements;
using ParksComputing.Xfer.Lang.DynamicSource;
using ParksComputing.Xfer.Lang.ProcessingInstructions;

namespace DynamicDbResolverDemo {
    // Custom resolver: supports "db:" source type, expects key/value pairs directly in dynamicSource
    public class DbDynamicSourceResolver : DefaultDynamicSourceResolver {
        public override string? Resolve(string key, XferDocument document) {
            // 1. Check for PI override
            foreach (var meta in document.Root.Values) {
                if (meta is ProcessingInstruction metaElem && metaElem.Kvp?.Key == "dynamicSource") {
                    if (metaElem.Kvp.Value is ObjectElement obj && obj.ContainsKey(key)) {
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
                            if (sourceStr.StartsWith("db:")) {
                                var dbKey = sourceStr.Substring(3);
                                return GetValueFromDb(dbKey);
                            }
                            else if (sourceStr.StartsWith("env:")) {
                                var envKey = sourceStr.Substring(4);
                                return Environment.GetEnvironmentVariable(envKey);
                            }
                            else {
                                // Hard-coded value
                                return sourceStr;
                            }
                        }
                    }
                }
            }

            // 2. If no PI override, use DB as default
            return GetValueFromDb(key);
        }

        private string? GetValueFromDb(string dbKey) {
            // Use a local SQLite DB file
            var dbPath = "demo.db";
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT value FROM secrets WHERE key = @key LIMIT 1";
            cmd.Parameters.AddWithValue("@key", dbKey);
            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }
    }

    class Program {
        // ...existing code...
        static void Main(string[] args) {
            // Setup demo DB
            var dbPath = "demo.db";
            using (var connection = new SqliteConnection($"Data Source={dbPath}")) {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS secrets (key TEXT PRIMARY KEY, value TEXT);";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "INSERT OR REPLACE INTO secrets (key, value) VALUES (@key, @value);";
                cmd.Parameters.AddWithValue("@key", "dbpassword");
                cmd.Parameters.AddWithValue("@value", "SuperSecretFromDB!");
                cmd.ExecuteNonQuery();
            }

            // Insert greeting value for demo
            using (var connection = new SqliteConnection($"Data Source={dbPath}")) {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT OR REPLACE INTO secrets (key, value) VALUES (@key, @value);";
                cmd.Parameters.AddWithValue("@key", "greeting");
                cmd.Parameters.AddWithValue("@value", "Hello from the DB!");
                cmd.ExecuteNonQuery();
            }

            // Read Xfer document from file
            var xferPath = Path.Combine(Environment.CurrentDirectory, "sample.xfer");
            var xfer = System.IO.File.ReadAllText(xferPath);

            var parser = new Parser();
            parser.DynamicSourceResolver = new DbDynamicSourceResolver();
            var doc = parser.Parse(xfer);
            var root = doc.Root;

            foreach (var element in root.Values) {
                if (element is KeyValuePairElement kvp && kvp.Key == "credentials") {
                    var obj = kvp.Value as ObjectElement;

                    if (obj != null) {
                        var passwordRaw = obj["password"];
                        var greetingRaw = obj["greeting"];
                        Element? passwordValue = passwordRaw is KeyValuePairElement pwKvp ? pwKvp.Value : passwordRaw;
                        Element? greetingValue = greetingRaw is KeyValuePairElement grKvp ? grKvp.Value : greetingRaw;

                        if (passwordValue is InterpolatedElement pwElement) {
                            Console.WriteLine($"Resolved password: {pwElement.Value}");
                        }
                        else if (passwordValue is StringElement pwStrElem) {
                            Console.WriteLine($"Resolved password (string): {pwStrElem.Value}");
                        }
                        else {
                            Console.WriteLine($"Resolved password (raw): {passwordValue}");
                        }

                        if (greetingValue is InterpolatedElement grElement) {
                            Console.WriteLine($"Resolved greeting: {grElement.Value}");
                        }
                        else if (greetingValue is StringElement grStrElem) {
                            Console.WriteLine($"Resolved greeting (string): {grStrElem.Value}");
                        }
                        else {
                            Console.WriteLine($"Resolved greeting (raw): {greetingValue}");
                        }
                    }
                }
            }
        }
    }
}
