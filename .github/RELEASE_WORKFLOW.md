# XferLang Release Workflow Setup

This repository includes an automated release workflow that publishes to both NuGet and GitHub releases simultaneously.

## Setup Required

### 1. NuGet API Key (Required for NuGet publishing)

1. Go to [NuGet.org](https://www.nuget.org/account/apikeys)
2. Create a new API key with "Push new packages and package versions" permission
3. In your GitHub repository, go to Settings → Secrets and variables → Actions
4. Add a new repository secret:
   - Name: `NUGET_API_KEY`
   - Value: Your NuGet API key

### 2. Repository Permissions (Automatic)

The workflow uses `GITHUB_TOKEN` which is automatically provided by GitHub Actions.

## How to Use

### Triggering a Release

1. Go to your repository on GitHub
2. Click "Actions" tab
3. Select "Manual Release to NuGet and GitHub" workflow
4. Click "Run workflow"
5. Fill in the parameters:
   - **Version**: e.g., `10.8.0-preview` or `11.0.0`
   - **Pre-release**: Check if this is a pre-release
   - **Release notes**: Optional custom notes (auto-generated if empty)
   - **Publish to NuGet**: Usually checked

### What Happens

The workflow will:
1. ✅ Build and test the solution
2. ✅ Create NuGet packages
3. ✅ Create a Git tag (e.g., `v10.8.0-preview`)
4. ✅ Create a GitHub release with the tag
5. ✅ Upload NuGet packages as release assets
6. ✅ Publish to NuGet.org (if enabled)
7. ✅ Keep everything in perfect sync

### Version Naming

Use semantic versioning:
- **Stable releases**: `11.0.0`, `11.1.0`, `11.1.1`
- **Pre-releases**: `11.0.0-alpha`, `11.0.0-beta`, `11.0.0-rc1`, `10.8.0-preview`

### Safety Features

- ✅ Version format validation
- ✅ All tests must pass before release
- ✅ Manual trigger only (no accidental releases)
- ✅ Option to skip NuGet publishing if needed
- ✅ Symbols packages included automatically

## Example Usage

To release version `10.8.0-preview`:
1. Run the workflow with version `10.8.0-preview`
2. Mark as pre-release: ✅
3. The workflow creates:
   - Git tag: `v10.8.0-preview`
   - GitHub release: "XferLang v10.8.0-preview" (marked as pre-release)
   - NuGet package: `ParksComputing.Xfer.Lang 10.8.0-preview`

Both GitHub and NuGet will have exactly the same version, keeping them perfectly synchronized!
