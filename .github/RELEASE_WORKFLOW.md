# XferLang Release Workflow

## Versioning System

XferLang uses a hybrid versioning approach that combines manual control with automatic build increments:

**Version Format**: `X.Y.BUILD-prerelease` (e.g., `0.10.123-prerelease`)

- **X.Y** (Base Version): Manually controlled in `version.txt` (e.g., `0.10`, `0.11`, `1.0`)
- **BUILD**: Auto-increments with each master branch merge
- **Suffix**: `-prerelease` for development, removed for stable releases

### Updating Base Version (X.Y)
When you want to bump the major/minor version:

1. Edit the `version.txt` file in the repository root
2. Change from `0.10` to `0.11` (or `1.0` for major release)
3. Commit and push to master
4. Next CI build will use the new base version

### Automatic Build Increment
- Every push to master auto-increments the build number
- CI builds packages like `0.10.456-prerelease` (not published to NuGet)
- Build number resets when you change the base version

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
   - **✅ Use auto-generated version** (recommended): Uses current `X.Y.BUILD-prerelease`
   - **Custom version**: Override with your own version string
   - **Pre-release**: Check for development releases, uncheck for stable
   - **Release notes**: Optional custom notes (auto-generated if empty)
   - **Publish to NuGet**: Usually checked

### What Happens

The workflow will:
1. ✅ Build and test the solution
2. ✅ Create NuGet packages
3. ✅ Create a Git tag (e.g., `v0.10.9-prerelease`)
4. ✅ Create a GitHub release with the tag
5. ✅ Upload NuGet packages as release assets
6. ✅ Publish to NuGet.org (if enabled)
7. ✅ Keep everything in perfect sync

### Version Naming

Use the same pattern as your current NuGet versions:
- **Pre-releases**: `0.10.9-prerelease`, `0.11.0-prerelease`, `1.0.0-prerelease`
- **Stable releases**: `1.0.0`, `1.1.0`, `1.1.1`

### Safety Features

- ✅ Version format validation
- ✅ All tests must pass before release
- ✅ Manual trigger only (no accidental releases)
- ✅ Option to skip NuGet publishing if needed
- ✅ Symbols packages included automatically

## Example Usage

To release version `10.8.0-prerelease`:
1. Run the workflow with version `10.8.0-prerelease`
2. Mark as pre-release: ✅
3. The workflow creates:
   - Git tag: `v10.8.0-prerelease`
   - GitHub release: "XferLang v10.8.0-prerelease" (marked as pre-release)
   - NuGet package: `ParksComputing.Xfer.Lang 10.8.0-prerelease`

Both GitHub and NuGet will have exactly the same version, keeping them perfectly synchronized!

## CI vs Release Workflows

- **`.NET CI` workflow**: Runs on every push/PR to build, test, and create `10.X.0-prerelease` packages (no publishing)
- **`Manual Release` workflow**: Only way to publish releases to NuGet and GitHub using the same versioning pattern
