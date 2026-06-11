# Eisenholz Asset Catalog

Editor-only Unity tool to **browse, search, download and import** assets (models, textures, scripts,
audio, prefabs, materials, shaders) from the Eisenholz Asset Catalog HTTP service, plus a helper to
configure the Unity Accelerator (Cache Server) for the team.

## Requirements
- Unity 6 (6000.4+)

## Install via Package Manager (Git URL)

> Replace `ORG/asset-catalog` below with the actual GitHub org/repo this package is hosted in.

**Option A — Package Manager UI**
1. **Window ▸ Package Manager**
2. **+** (top-left) ▸ **Add package from git URL…**
3. Paste a versioned URL (see below) and click **Add**.

**Option B — edit `Packages/manifest.json`** in the consuming project:
```json
{
  "dependencies": {
    "com.eisenholz.assetcatalog": "https://github.com/ORG/asset-catalog.git#v1.0.0"
  }
}
```

### Pinning & updating versions
Always pin to a tag so the whole team resolves the same code:

| URL fragment | Resolves to |
|---|---|
| `…/asset-catalog.git#v1.0.0` | exactly the **v1.0.0** release tag (recommended) |
| `…/asset-catalog.git#main` | latest commit on `main` (moving target — avoid for shared use) |
| `…/asset-catalog.git` | default branch HEAD |

To **update**, change the tag in the URL / `manifest.json` (e.g. `#v1.1.0`) and let Package Manager
re-resolve. Releases follow [Semantic Versioning](https://semver.org/) and are listed in
[`CHANGELOG.md`](CHANGELOG.md).

> Note: if the catalog server uses plain `http://`, set **Project Settings ▸ Player ▸ Other Settings ▸
> Allow downloads over HTTP** to **Always allowed** (Unity 6 blocks insecure HTTP by default).

## Usage
1. **Configure:** set the catalog server URL in **Project Settings ▸ Eisenholz ▸ Asset Catalog**.
2. **Browse:** open **Window ▸ Eisenholz ▸ Asset Catalog**, search and filter by type, and click an
   asset to see its details.
3. **Download & Import:** press **Download & Import** in the details panel. The asset is fetched to a
   temp staging dir, its sha256 is verified, then it is imported under `Assets/` (default
   `Assets/Eisenholz/…`).
4. **Accelerator (optional):** open **Window ▸ Eisenholz ▸ Unity Accelerator** to point Unity's Cache
   Server at the team accelerator and test reachability.

## Status
**v1.0.0 — feature complete.** See [`CHANGELOG.md`](CHANGELOG.md) for the release history and the
project's `Docs/AssetCatalogTool/` (PROGRESS / ROADMAP) for milestone history and the post-1.0 backlog.

## Layout
```
Editor/
  Core/        settings, settings provider, logging, stale-download cleanup
  Http/        async UnityWebRequest client
  Api/         catalog API + endpoints
  Models/      DTOs
  Import/      pluggable importers + safe extractor
  Thumbnails/  two-tier thumbnail cache + service
  UI/          EditorWindow, thumbnail grid, details panel
  Accelerator/ Cache Server config + health check
  Utils/       formatting, paths, sha256, error text, progress
Tests/Editor/  EditMode tests
```
