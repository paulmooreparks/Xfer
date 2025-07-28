# This script automates publishing the XferLang VS Code extension to the Marketplace.
# Usage: pwsh ./publish-extension.ps1

# Ensure vsce is installed
Write-Output "Checking for vsce..."
if (-not (Get-Command vsce -ErrorAction SilentlyContinue)) {
    Write-Output "vsce not found. Installing..."
    npm install -g @vscode/vsce
}

# Prompt for publisher login if needed
Write-Output "Checking for VSCE publisher login..."
$token = vsce ls-publishers 2>&1 | Select-String 'No publishers found'
if ($token) {
    Write-Output "No publisher found. Run 'vsce create-publisher <name>' and follow the prompts."
    exit 1
}

# Publish the extension
Write-Output "Publishing the extension..."
vsce publish

Write-Output "Done."
