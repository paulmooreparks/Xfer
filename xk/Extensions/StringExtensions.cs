using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.VisualBasic;

using ParksComputing.XferKit.Scripting.Services;
using ParksComputing.XferKit.Workspace.Services;

namespace ParksComputing.XferKit.Cli.Extensions;

internal static class StringExtensions {
    public static string ReplaceXferKitPlaceholders(
        this string template,
        IXferScriptEngine scriptEngine,
        IPropertyResolver propertyResolver,
        string? workspaceName,
        string? requestName,
        Dictionary<string, object?>? args
        ) 
    {
        // Regex pattern matches placeholders like "{{[env]::VariableName}}", "{{[prop]::VariableName}}", or "{{VariableName::DefaultValue}}"
        string placeholderPattern = @"\{\{(\[env\]::|\[prop\]::|\[file\]::|\[arg\]::)?([^:{}]+(?::[^:{}]+)*)(?:::([^}]+))?\}\}";

        // Find all matches in the template
        var matches = Regex.Matches(template, placeholderPattern);

        foreach (Match match in matches) {
            string namespacePrefix = match.Groups[1].Value;
            var variable = match.Groups[2].Value;
            var defaultValue = match.Groups[3].Success ? match.Groups[3].Value : null;
            string? value = null;

            if (namespacePrefix == "[env]::" || string.IsNullOrEmpty(namespacePrefix)) {
                value = Environment.GetEnvironmentVariable(variable);
            }
            else if (namespacePrefix == "[arg]::") {
                if (args is not null && args.TryGetValue(variable, out var argValue) && argValue is not null) {
                    value = argValue.ToString();
                }
            }
            else if (namespacePrefix == "[prop]::") {
                value = propertyResolver.ResolveProperty(variable, workspaceName, requestName, defaultValue);
            }
            else if (namespacePrefix == "[file]::") {
                string filePath = variable;

                // Check if the file exists
                if (!File.Exists(filePath)) {
                    Console.Error.WriteLine($"{Workspace.Constants.ErrorChar} File not found: {filePath}");
                    value = defaultValue;
                }

                try {
                    value = File.ReadAllText(filePath, Encoding.UTF8);
                }
                catch (Exception ex) {
                    Console.Error.WriteLine($"{Workspace.Constants.ErrorChar} Error reading file '{filePath}': {ex.Message}");
                    value = defaultValue;
                }
            }

            // Use defaultValue if value is still null
            value = value ?? defaultValue;

            // If a value (including a default value) is found, replace the placeholder in the template with the actual value
            if (value != null) {
                template = template.Replace(match.Value, value);
            }
        }

        return template;
    }
}
