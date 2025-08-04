using Markdig;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using System.Xml.Linq;
using System.Reflection;

namespace XferDocBuilder;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        var command = args[0].ToLower();

        switch (command)
        {
            case "md":
            case "markdown":
                await HandleMarkdownGeneration(args);
                break;
            case "api":
                await HandleApiDocumentationGeneration(args);
                break;
            default:
                // Legacy support - if first arg doesn't match a command, treat as markdown
                await HandleMarkdownGeneration(args);
                break;
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("XferDocBuilder - Documentation Generation Tool");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  md <source.md> <output.html>     Generate HTML from Markdown");
        Console.WriteLine("  api <assembly.dll> <output.html> Generate API documentation from XML comments");
        Console.WriteLine();
        Console.WriteLine("Legacy usage (markdown):");
        Console.WriteLine("  XferDocBuilder <source.md> <output.html>");
    }

    private static async Task HandleMarkdownGeneration(string[] args)
    {
        string sourceFile, outputFile;

        if (args.Length == 3 && args[0].ToLower() == "md")
        {
            sourceFile = args[1];
            outputFile = args[2];
        }
        else if (args.Length == 2)
        {
            sourceFile = args[0];
            outputFile = args[1];
        }
        else
        {
            Console.WriteLine("Usage: XferDocBuilder md <source.md> <output.html>");
            Console.WriteLine("  source.md  - Markdown source file with YAML front matter");
            Console.WriteLine("  output.html - HTML output file to generate");
            return;
        }

        var templateFile = Path.Combine("docs", "template.html");

        Console.WriteLine($"Building documentation from {sourceFile} to {outputFile}");

        if (!File.Exists(sourceFile))
        {
            Console.WriteLine($"Source file not found: {sourceFile}");
            Console.WriteLine("Creating example source file...");
            await CreateExampleSourceFile(sourceFile);
            return;
        }

        if (!File.Exists(templateFile))
        {
            Console.WriteLine($"Template file not found: {templateFile}");
            Console.WriteLine("Creating default template...");
            await CreateDefaultTemplate(templateFile);
        }

        try
        {
            await BuildDocumentation(sourceFile, templateFile, outputFile);
            Console.WriteLine($"Documentation built successfully: {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error building documentation: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task HandleApiDocumentationGeneration(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: XferDocBuilder api <assembly.dll> <output.html>");
            Console.WriteLine("  assembly.dll - .NET assembly with XML documentation");
            Console.WriteLine("  output.html  - HTML output file to generate");
            return;
        }

        var assemblyPath = args[1];
        var outputFile = args[2];
        var templateFile = Path.Combine("docs", "template.html");

        Console.WriteLine($"Generating API documentation from {assemblyPath} to {outputFile}");

        if (!File.Exists(assemblyPath))
        {
            Console.WriteLine($"Assembly file not found: {assemblyPath}");
            return;
        }

        if (!File.Exists(templateFile))
        {
            Console.WriteLine($"Template file not found: {templateFile}");
            Console.WriteLine("Creating default template...");
            await CreateDefaultTemplate(templateFile);
        }

        try
        {
            await BuildApiDocumentation(assemblyPath, templateFile, outputFile);
            Console.WriteLine($"API documentation built successfully: {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error building API documentation: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task BuildDocumentation(string sourceFile, string templateFile, string outputFile)
    {
        // Read source markdown
        var markdownContent = await File.ReadAllTextAsync(sourceFile);

        // Extract front matter if present
        var (frontMatter, content) = ExtractFrontMatter(markdownContent);

        // Configure Markdig pipeline
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseCustomContainers()
            .UseSyntaxHighlighting()
            .Build();

        // Convert markdown to HTML
        var htmlContent = Markdown.ToHtml(content, pipeline);

        // Process sections and build navigation
        var processedHtml = ProcessSections(htmlContent);
        var navigation = BuildNavigation(processedHtml);

        // Read template
        var template = await File.ReadAllTextAsync(templateFile);

        // Replace placeholders in template
        var finalHtml = template
            .Replace("{{TITLE}}", frontMatter.ContainsKey("title") ? frontMatter["title"].ToString() : "XferLang Documentation")
            .Replace("{{NAVIGATION}}", navigation)
            .Replace("{{CONTENT}}", processedHtml)
            .Replace("{{GENERATED_DATE}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));

        // Ensure output directory exists
        var outputDir = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // Write final HTML
        await File.WriteAllTextAsync(outputFile, finalHtml);
    }

    private static async Task BuildApiDocumentation(string assemblyPath, string templateFile, string outputFile)
    {
        // Load the assembly
        var assembly = Assembly.LoadFrom(assemblyPath);

        // Find the XML documentation file
        var xmlDocPath = Path.ChangeExtension(assemblyPath, ".xml");
        XDocument? xmlDoc = null;

        if (File.Exists(xmlDocPath))
        {
            xmlDoc = XDocument.Load(xmlDocPath);
        }

        // Generate API documentation
        var apiDoc = new ApiDocumentationGenerator();
        var htmlContent = apiDoc.GenerateDocumentation(assembly, xmlDoc);

        // Build navigation
        var navigation = apiDoc.GenerateNavigation(assembly);

        // Read template
        var template = await File.ReadAllTextAsync(templateFile);

        // Replace placeholders in template
        var finalHtml = template
            .Replace("{{TITLE}}", $"{assembly.GetName().Name} API Documentation")
            .Replace("{{NAVIGATION}}", navigation)
            .Replace("{{CONTENT}}", htmlContent)
            .Replace("{{GENERATED_DATE}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));

        // Ensure output directory exists
        var outputDir = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // Write final HTML
        await File.WriteAllTextAsync(outputFile, finalHtml);
    }

    private static (Dictionary<string, object> frontMatter, string content) ExtractFrontMatter(string markdown)
    {
        var frontMatter = new Dictionary<string, object>();
        var content = markdown;

        if (markdown.StartsWith("---"))
        {
            var endIndex = markdown.IndexOf("\n---\n", 3);
            if (endIndex > 0)
            {
                var yamlContent = markdown.Substring(3, endIndex - 3);
                content = markdown.Substring(endIndex + 5);

                try
                {
                    var deserializer = new DeserializerBuilder().Build();
                    frontMatter = deserializer.Deserialize<Dictionary<string, object>>(yamlContent) ?? new();
                }
                catch
                {
                    // If YAML parsing fails, just use the entire content
                    content = markdown;
                }
            }
        }

        return (frontMatter, content);
    }

    private static string ProcessSections(string html)
    {
        // Add section wrappers and IDs based on headings
        var processed = html;

        // Pattern to match h2, h3, and h4 headings (allowing HTML tags inside)
        var headingPattern = @"<(h[234])([^>]*)>(.*?)</\1>";

        processed = Regex.Replace(processed, headingPattern, match =>
        {
            var tag = match.Groups[1].Value;
            var attributes = match.Groups[2].Value;
            var text = match.Groups[3].Value;
            var id = GenerateId(text);

            return $"<section id=\"{id}\"><{tag}{attributes}>{text}</{tag}>";
        });

        // Close sections (this is a simple approach - you might want more sophisticated section handling)
        processed = processed.Replace("</section><section", "</section>\n<section");

        return processed + "</section>"; // Close the last section
    }

    private static string GenerateId(string text)
    {
        var cleanText = StripHtmlTags(text);
        return Regex.Replace(cleanText.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and"), @"[^a-z0-9\-]", "")
            .Trim('-');
    }

    private static string StripHtmlTags(string text)
    {
        return Regex.Replace(text, @"<[^>]+>", "");
    }

    private static string BuildNavigation(string html)
    {
        var nav = new StringBuilder();

        // Extract all headings from the HTML (allowing HTML tags inside headings)
        var headingPattern = @"<section id=""([^""]*)""><(h[234])[^>]*>(.*?)</\2>";
        var matches = Regex.Matches(html, headingPattern);

        var navItems = new List<NavItem>();

        foreach (Match match in matches)
        {
            var id = match.Groups[1].Value;
            var level = int.Parse(match.Groups[2].Value.Substring(1)); // Extract number from h2, h3, h4
            var text = StripHtmlTags(match.Groups[3].Value);

            // Skip "Table of Contents" from navigation as it's redundant
            if (text.ToLowerInvariant().Contains("table of contents"))
            {
                continue;
            }

            navItems.Add(new NavItem { Id = id, Level = level, Text = text });
        }

        nav.AppendLine("        <ul>");

        BuildNavigationRecursive(nav, navItems, 0, 2); // Start with level 2 (h2 headings)

        nav.AppendLine("        </ul>");
        return nav.ToString();
    }

    private static void BuildNavigationRecursive(StringBuilder nav, List<NavItem> items, int startIndex, int currentLevel)
    {
        for (int i = startIndex; i < items.Count; i++)
        {
            var item = items[i];

            // If we've gone back to a higher level, stop processing this level
            if (item.Level < currentLevel)
            {
                return;
            }

            // If this is a deeper level, skip it (will be processed recursively)
            if (item.Level > currentLevel)
            {
                continue;
            }

            // Check if this item has children (any subsequent items with level > current)
            // Only check against items that are actually in the navigation
            bool hasChildren = false;
            for (int j = i + 1; j < items.Count; j++)
            {
                if (items[j].Level <= item.Level)
                {
                    // We've hit another item at the same level or higher, stop looking
                    break;
                }
                if (items[j].Level == item.Level + 1)
                {
                    hasChildren = true;
                    break;
                }
            }

            if (currentLevel == 2)
            {
                if (hasChildren)
                {
                    // Top-level items (h2) with children become accordions - summary is just text, not clickable
                    nav.AppendLine("            <li>");
                    nav.AppendLine("                <details>");
                    nav.AppendLine($"                    <summary>{item.Text}</summary>");
                    nav.AppendLine("                    <ul>");
                    BuildNavigationRecursive(nav, items, i + 1, currentLevel + 1);
                    nav.AppendLine("                    </ul>");
                    nav.AppendLine("                </details>");
                    nav.AppendLine("            </li>");
                }
                else
                {
                    // Top-level items (h2) without children are simple links
                    nav.AppendLine($"            <li><a href=\"#{item.Id}\">{item.Text}</a></li>");
                }
            }
            else if (currentLevel > 2)
            {
                // Nested items (h3, h4, etc.) are always simple links
                nav.AppendLine($"                <li><a href=\"#{item.Id}\">{item.Text}</a></li>");
            }
        }
    }

    private class NavItem
    {
        public string Id { get; set; } = "";
        public int Level { get; set; }
        public string Text { get; set; } = "";
    }

    private static async Task CreateExampleSourceFile(string filePath)
    {
        var exampleContent = @"---
title: XferLang Documentation
description: Documentation for the XferLang data-interchange format
---

# XferLang Documentation

## Introduction

XferLang is a data-interchange format designed to support data serialization, data transmission, and offline use cases such as configuration management.

```xfer
{
    name ""Alice""
    age 30
    isActive ~true
}
```

## Design Philosophy

XferLang is built around four core principles:

### 1. Clarity and Readability
The syntax is designed to be human-readable without requiring separators like commas.

### 2. Explicit Typing
All values are explicitly typed using prefixes.

## Basic Syntax

XferLang documents consist of elements separated by whitespace.

```xfer
</ This is a comment />
name ""John Doe""
age 30
```
";

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, exampleContent);
    }

    private static async Task CreateDefaultTemplate(string filePath)
    {
        var templateContent = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{{TITLE}}</title>
    <link rel=""stylesheet"" href=""style.css"">
    <link rel=""stylesheet"" href=""highlightjs/styles/github.min.css"">
    <script src=""highlightjs/highlight.min.js""></script>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            hljs.highlightAll();
        });
    </script>
</head>
<body>
    <div class=""mobile-nav-toggle"">
        ☰
    </div>
    <nav class=""sidebar"">
{{NAVIGATION}}
    </nav>
    <main class=""content"">
        <header>
            <img src=""XferLang-lg.png"" alt=""XferLang Logo"" class=""logo"">
            <h1>XferLang</h1>
            <p class=""subtitle"">A Modern Data-Interchange Format</p>
        </header>

{{CONTENT}}

        <footer>
            <p>Documentation generated on {{GENERATED_DATE}}</p>
        </footer>
    </main>
</body>
</html>";

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, templateContent);
    }
}

public static class MarkdownPipelineBuilderExtensions
{
    public static MarkdownPipelineBuilder UseSyntaxHighlighting(this MarkdownPipelineBuilder builder)
    {
        // Add syntax highlighting support for XferLang
        return builder.UseGenericAttributes();
    }
}

public class ApiDocumentationGenerator
{
    public string GenerateDocumentation(Assembly assembly, XDocument? xmlDoc)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"<section id=\"api-overview\"><h1>{assembly.GetName().Name} API Documentation</h1>");
        sb.AppendLine($"<p>API documentation for {assembly.GetName().Name} version {assembly.GetName().Version}</p></section>");

        // Get all public types
        var publicTypes = assembly.GetTypes()
            .Where(t => t.IsPublic)
            .OrderBy(t => t.Namespace)
            .ThenBy(t => t.Name)
            .ToList();

        // Group by namespace
        var namespaceGroups = publicTypes.GroupBy(t => t.Namespace ?? "Global").OrderBy(g => g.Key);

        foreach (var namespaceGroup in namespaceGroups)
        {
            var namespaceId = SanitizeId(namespaceGroup.Key);
            sb.AppendLine($"<section id=\"namespace-{namespaceId}\"><h2>Namespace: {namespaceGroup.Key}</h2>");

            foreach (var type in namespaceGroup)
            {
                GenerateTypeDocumentation(sb, type, xmlDoc);
            }

            sb.AppendLine("</section>");
        }

        return sb.ToString();
    }

    public string GenerateNavigation(Assembly assembly)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<ul>");
        sb.AppendLine($"<li><a href=\"#api-overview\">{assembly.GetName().Name} API</a></li>");

        // Get all public types grouped by namespace
        var publicTypes = assembly.GetTypes()
            .Where(t => t.IsPublic)
            .OrderBy(t => t.Namespace)
            .ThenBy(t => t.Name)
            .ToList();

        var namespaceGroups = publicTypes.GroupBy(t => t.Namespace ?? "Global").OrderBy(g => g.Key);

        foreach (var namespaceGroup in namespaceGroups)
        {
            var namespaceId = SanitizeId(namespaceGroup.Key);
            sb.AppendLine("            <li>");
            sb.AppendLine("                <details>");
            sb.AppendLine($"                    <summary>{namespaceGroup.Key}</summary>");
            sb.AppendLine("                    <ul>");

            foreach (var type in namespaceGroup)
            {
                var typeId = SanitizeId($"{type.Namespace}.{type.Name}");
                var typeName = GetTypeName(type);
                sb.AppendLine($"                        <li><a href=\"#type-{typeId}\">{typeName}</a></li>");
            }

            sb.AppendLine("                    </ul>");
            sb.AppendLine("                </details>");
            sb.AppendLine("            </li>");
        }

        sb.AppendLine("</ul>");
        return sb.ToString();
    }

    private void GenerateTypeDocumentation(StringBuilder sb, Type type, XDocument? xmlDoc)
    {
        var typeId = SanitizeId($"{type.Namespace}.{type.Name}");
        var typeName = GetTypeName(type);
        var typeKind = GetTypeKind(type);

        sb.AppendLine($"<section id=\"type-{typeId}\"><h3>{typeKind}: {typeName}</h3>");

        // Get XML documentation for the type
        var xmlMemberName = $"T:{type.FullName}";
        var xmlSummary = GetXmlDocumentation(xmlDoc, xmlMemberName);
        if (!string.IsNullOrEmpty(xmlSummary))
        {
            sb.AppendLine($"<p>{xmlSummary}</p>");
        }

        // Generate enum members documentation if this is an enum
        if (type.IsEnum)
        {
            var enumFields = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.DeclaringType == type)
                .OrderBy(f => f.Name)
                .ToList();

            if (enumFields.Any())
            {
                sb.AppendLine("<h4>Members</h4>");
                sb.AppendLine("<table class=\"api-table\">");
                sb.AppendLine("<thead><tr><th>Name</th><th>Value</th><th>Description</th></tr></thead>");
                sb.AppendLine("<tbody>");

                foreach (var field in enumFields)
                {
                    var fieldXmlName = $"F:{field.DeclaringType!.FullName}.{field.Name}";
                    var fieldDoc = GetXmlDocumentation(xmlDoc, fieldXmlName);
                    var fieldValue = Convert.ToInt32(field.GetValue(null));

                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td><code>{field.Name}</code></td>");
                    sb.AppendLine($"<td><code>{fieldValue}</code></td>");
                    sb.AppendLine($"<td>{fieldDoc}</td>");
                    sb.AppendLine("</tr>");
                }

                sb.AppendLine("</tbody>");
                sb.AppendLine("</table>");
            }
        }

        // Generate properties documentation
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(p => p.DeclaringType == type)
            .OrderBy(p => p.Name)
            .ToList();

        if (properties.Any())
        {
            sb.AppendLine("<h4>Properties</h4>");
            sb.AppendLine("<table class=\"api-table\">");
            sb.AppendLine("<thead><tr><th>Name</th><th>Type</th><th>Description</th></tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var prop in properties)
            {
                var propXmlName = $"P:{prop.DeclaringType!.FullName}.{prop.Name}";
                var propDoc = GetXmlDocumentation(xmlDoc, propXmlName);
                var propTypeName = GetTypeName(prop.PropertyType);

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td><code>{prop.Name}</code></td>");
                sb.AppendLine($"<td><code>{propTypeName}</code></td>");
                sb.AppendLine($"<td>{propDoc}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
        }

        // Generate methods documentation (constructors and public methods)
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.DeclaringType == type && !m.IsSpecialName)
            .OrderBy(m => m.Name)
            .ToList();

        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Where(c => c.DeclaringType == type)
            .ToList();

        if (constructors.Any() || methods.Any())
        {
            sb.AppendLine("<h4>Methods</h4>");

            foreach (var ctor in constructors)
            {
                GenerateMethodDocumentation(sb, ctor, xmlDoc, true);
            }

            foreach (var method in methods)
            {
                GenerateMethodDocumentation(sb, method, xmlDoc, false);
            }
        }

        sb.AppendLine("</section>");
    }

    private void GenerateMethodDocumentation(StringBuilder sb, MethodBase method, XDocument? xmlDoc, bool isConstructor)
    {
        var parameters = method.GetParameters();
        var parameterTypes = parameters.Select(p => GetTypeName(p.ParameterType)).ToArray();

        string xmlMemberName;
        if (isConstructor)
        {
            xmlMemberName = $"M:{method.DeclaringType!.FullName}.#ctor";
        }
        else
        {
            xmlMemberName = $"M:{method.DeclaringType!.FullName}.{method.Name}";

            // Handle generic methods
            if (method.IsGenericMethod)
            {
                var genericArgs = method.GetGenericArguments();
                xmlMemberName += $"``{genericArgs.Length}";
            }
        }

        if (parameters.Length > 0)
        {
            var parameterTypeNames = parameters.Select(p => GetXmlTypeName(p.ParameterType, method));
            xmlMemberName += $"({string.Join(",", parameterTypeNames)})";
        }

        var methodDocumentation = GetMethodDocumentation(xmlDoc, xmlMemberName);

        sb.AppendLine("<div class=\"method-doc\">");

        if (isConstructor)
        {
            sb.AppendLine($"<h5>Constructor</h5>");
            sb.AppendLine($"<code>{method.DeclaringType!.Name}({string.Join(", ", parameters.Select(p => $"{GetTypeName(p.ParameterType)} {p.Name}"))})</code>");
        }
        else
        {
            var returnType = ((MethodInfo)method).ReturnType;
            sb.AppendLine($"<h5>{method.Name}</h5>");
            sb.AppendLine($"<code>{GetTypeName(returnType)} {method.Name}({string.Join(", ", parameters.Select(p => $"{GetTypeName(p.ParameterType)} {p.Name}"))})</code>");
        }

        // Add method summary
        if (!string.IsNullOrEmpty(methodDocumentation.Summary))
        {
            sb.AppendLine($"<p>{methodDocumentation.Summary}</p>");
        }

        // Add parameter documentation
        if (methodDocumentation.Parameters.Any())
        {
            sb.AppendLine("<h6>Parameters</h6>");
            sb.AppendLine("<table class=\"api-table\">");
            sb.AppendLine("<thead><tr><th>Name</th><th>Type</th><th>Description</th></tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var param in parameters)
            {
                var paramDoc = methodDocumentation.Parameters.TryGetValue(param.Name!, out var doc) ? doc : "";
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td><code>{param.Name}</code></td>");
                sb.AppendLine($"<td><code>{GetTypeName(param.ParameterType)}</code></td>");
                sb.AppendLine($"<td>{paramDoc}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
        }

        // Add return value documentation
        if (!isConstructor && !string.IsNullOrEmpty(methodDocumentation.Returns))
        {
            sb.AppendLine("<h6>Returns</h6>");
            sb.AppendLine($"<p>{methodDocumentation.Returns}</p>");
        }

        sb.AppendLine("</div>");
    }

    public class MethodDocumentation
    {
        public string Summary { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new();
        public string Returns { get; set; } = string.Empty;
    }

    private MethodDocumentation GetMethodDocumentation(XDocument? xmlDoc, string memberName)
    {
        var result = new MethodDocumentation();

        if (xmlDoc == null)
        {
            return result;
        }

        var member = xmlDoc.Descendants("member")
            .FirstOrDefault(m => m.Attribute("name")?.Value == memberName);

        if (member == null)
        {
            return result;
        }

        // Get summary
        var summary = member.Element("summary");
        if (summary != null)
        {
            result.Summary = CleanXmlDocText(summary.Value);
        }

        // Get parameters
        var paramElements = member.Elements("param");
        foreach (var param in paramElements)
        {
            var nameAttr = param.Attribute("name");
            if (nameAttr != null)
            {
                result.Parameters[nameAttr.Value] = CleanXmlDocText(param.Value);
            }
        }

        // Get returns
        var returns = member.Element("returns");
        if (returns != null)
        {
            result.Returns = CleanXmlDocText(returns.Value);
        }

        return result;
    }

    private string CleanXmlDocText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Clean up the XML documentation formatting
        // Remove leading/trailing whitespace and normalize line breaks
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var cleanedLines = lines.Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line));

        return string.Join(" ", cleanedLines);
    }

    private string GetXmlTypeName(Type type, MethodBase? method = null)
    {
        // Handle generic type parameters for methods
        if (type.IsGenericParameter && method?.IsGenericMethod == true)
        {
            var genericArgs = method.GetGenericArguments();
            for (int i = 0; i < genericArgs.Length; i++)
            {
                if (genericArgs[i] == type)
                {
                    return $"``{i}";
                }
            }
        }

        // Handle array types
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            return GetXmlTypeName(elementType, method) + "[]";
        }

        // Handle generic types
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            var args = type.GetGenericArguments();
            var argNames = args.Select(arg => GetXmlTypeName(arg, method));

            return $"{genericTypeDef.FullName!.Split('`')[0]}{{{string.Join(",", argNames)}}}";
        }

        // For regular types, use full name
        return type.FullName ?? type.Name;
    }

    private string GetXmlDocumentation(XDocument? xmlDoc, string memberName)
    {
        if (xmlDoc == null)
        {
            return string.Empty;
        }

        var member = xmlDoc.Descendants("member")
            .FirstOrDefault(m => m.Attribute("name")?.Value == memberName);

        if (member == null)
        {
            return string.Empty;
        }

        var summary = member.Element("summary");
        if (summary == null)
        {
            return string.Empty;
        }

        // Get the inner text and clean up whitespace
        var text = summary.Value;
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Clean up the XML documentation formatting
        // Remove leading/trailing whitespace and normalize line breaks
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var cleanedLines = lines.Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line));

        return string.Join(" ", cleanedLines);
    }

    private string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericName = type.Name.Substring(0, type.Name.IndexOf('`'));
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
            return $"{genericName}&lt;{genericArgs}&gt;";
        }

        return type.Name switch
        {
            "String" => "string",
            "Int32" => "int",
            "Int64" => "long",
            "Boolean" => "bool",
            "Double" => "double",
            "Decimal" => "decimal",
            "DateTime" => "DateTime",
            "Void" => "void",
            _ => type.Name
        };
    }

    private string GetTypeKind(Type type)
    {
        if (type.IsInterface)
        {
            return "Interface";
        }
        if (type.IsEnum)
        {
            return "Enum";
        }
        if (type.IsValueType)
        {
            return "Struct";
        }
        if (type.IsAbstract && type.IsSealed)
        {
            return "Static Class";
        }
        if (type.IsAbstract)
        {
            return "Abstract Class";
        }
        return "Class";
    }

    private string SanitizeId(string input)
    {
        return Regex.Replace(input, @"[^a-zA-Z0-9-_]", "-").ToLower();
    }
}
