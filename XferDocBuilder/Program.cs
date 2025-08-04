using Markdig;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace XferDocBuilder;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: XferDocBuilder <source.md> <output.html>");
            Console.WriteLine("  source.md  - Markdown source file with YAML front matter");
            Console.WriteLine("  output.html - HTML output file to generate");
            return;
        }

        var sourceFile = args[0];
        var outputFile = args[1];
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

        nav.AppendLine("        <div class=\"sidebar-header\">");
        nav.AppendLine("            <h2><a href=\"index.html\" class=\"home-link\">XferLang</a></h2>");
        nav.AppendLine("        </div>");
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
