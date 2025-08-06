#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Generates documentation for the XferLang project
.DESCRIPTION
    This script builds the project with XML documentation generation enabled,
    then uses XferDocBuilder to generate both HTML documentation and API documentation.
.PARAMETER Clean
    Performs a clean build before generating documentation
.PARAMETER SkipBuild
    Skips the build step and only runs documentation generation (assumes project is already built)
.EXAMPLE
    .\generate-docs.ps1
    Generates documentation with default settings
.EXAMPLE
    .\generate-docs.ps1 -Clean
    Performs a clean build before generating documentation
.EXAMPLE
    .\generate-docs.ps1 -SkipBuild
    Only generates documentation without building
#>

param(
    [switch]$Clean,
    [switch]$SkipBuild
)

# Get script directory (solution root)
$SolutionDir = $PSScriptRoot
$XferDocBuilderPath = Join-Path $SolutionDir "XferDocBuilder\XferDocBuilder.csproj"
$ProjectPath = Join-Path $SolutionDir "ParksComputing.Xfer.Lang\ParksComputing.Xfer.Lang.csproj"
$DocsDir = Join-Path $SolutionDir "docs"

Write-Host "=== XferLang Documentation Generator ===" -ForegroundColor Cyan
Write-Host "Solution Directory: $SolutionDir" -ForegroundColor Gray

# Check if XferDocBuilder exists
if (-not (Test-Path $XferDocBuilderPath)) {
    Write-Host "❌ XferDocBuilder not found at: $XferDocBuilderPath" -ForegroundColor Red
    Write-Host "Cannot generate documentation without XferDocBuilder." -ForegroundColor Red
    exit 1
}

# Check if main project exists
if (-not (Test-Path $ProjectPath)) {
    Write-Host "❌ Main project not found at: $ProjectPath" -ForegroundColor Red
    exit 1
}

# Build the project with XML documentation if not skipping build
if (-not $SkipBuild) {
    Write-Host "🔨 Building project with XML documentation..." -ForegroundColor Yellow

    if ($Clean) {
        Write-Host "🧹 Cleaning project..." -ForegroundColor Yellow
        dotnet clean $ProjectPath --configuration Release
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Clean failed" -ForegroundColor Red
            exit 1
        }
    }

    # Build with XML documentation generation enabled
    dotnet build $ProjectPath --configuration Release -p:GenerateDocumentationFile=true
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Build completed successfully" -ForegroundColor Green
} else {
    Write-Host "⏭️  Skipping build step" -ForegroundColor Yellow
}

# Ensure docs directory exists
if (-not (Test-Path $DocsDir)) {
    New-Item -ItemType Directory -Path $DocsDir -Force | Out-Null
    Write-Host "📁 Created docs directory" -ForegroundColor Green
}

# Generate main documentation (README.md -> docs/index.html)
Write-Host "📝 Generating main documentation..." -ForegroundColor Yellow
$ReadmePath = Join-Path $SolutionDir "README.md"
$IndexPath = Join-Path $DocsDir "index.html"

if (-not (Test-Path $ReadmePath)) {
    Write-Host "❌ README.md not found at: $ReadmePath" -ForegroundColor Red
    exit 1
}

dotnet run --project $XferDocBuilderPath md $ReadmePath $IndexPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Main documentation generation failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Main documentation generated: $IndexPath" -ForegroundColor Green

# Generate API documentation
Write-Host "📚 Generating API documentation..." -ForegroundColor Yellow
$AssemblyPath = Join-Path $SolutionDir "ParksComputing.Xfer.Lang\bin\Release\net8.0\ParksComputing.Xfer.Lang.dll"
$ApiDocsPath = Join-Path $DocsDir "api.html"

if (-not (Test-Path $AssemblyPath)) {
    Write-Host "❌ Assembly not found at: $AssemblyPath" -ForegroundColor Red
    Write-Host "Make sure the project has been built successfully." -ForegroundColor Red
    exit 1
}

dotnet run --project $XferDocBuilderPath api $AssemblyPath $ApiDocsPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ API documentation generation failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ API documentation generated: $ApiDocsPath" -ForegroundColor Green

Write-Host ""
Write-Host "🎉 Documentation generation completed successfully!" -ForegroundColor Green
Write-Host "📄 Main docs: $IndexPath" -ForegroundColor Gray
Write-Host "📚 API docs:  $ApiDocsPath" -ForegroundColor Gray
