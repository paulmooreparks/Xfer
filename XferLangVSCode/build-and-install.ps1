// This script automates packaging and installing the XferLang VS Code extension.
// Usage: pwsh ./build-and-install.ps1

# Ensure vsce is installed
Write-Output "Checking for vsce..."
if (-not (Get-Command vsce -ErrorAction SilentlyContinue)) {
    Write-Output "vsce not found. Installing..."
    npm install -g @vscode/vsce
}

# Install dependencies
Write-Output "Installing npm dependencies..."
npm install

# Package the extension
Write-Output "Packaging the extension..."
$vsix = (vsce package | Select-String -Pattern '\.vsix' | ForEach-Object { $_.Line.Trim() })
if (-not $vsix) {
    $vsix = Get-ChildItem *.vsix | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Select-Object -ExpandProperty Name
}

if (-not $vsix) {
    Write-Output "Failed to package extension."
    exit 1
}

# Install the extension
Write-Output "Installing the extension: $vsix ..."
code --install-extension $vsix

Write-Output "Done."
