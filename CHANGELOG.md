# Changelog

All notable changes to this package are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.0.0] - 2026-06-10
First feature-complete release: browse → download → import end to end, plus the Accelerator helper.
### Added
- **M2 — networking & models.** Async `CatalogHttpClient` over `UnityWebRequest` (retry + backoff,
  progress, cancellation), `HttpResult`/`HttpOutcome`, `IAuthProvider` seam, `JsonUtilitySerializer`
  behind `IJsonSerializer`, camelCase DTOs with wrapped list responses, and the `CatalogApi` client.
- **M3 — browser UI.** Rebuilt `AssetCatalogWindow` as a full browser: debounced search, type filter,
  paged wrapping thumbnail grid, and a `DetailsPanel`. Two-tier `ThumbnailCache` (memory LRU + disk
  cache under `Library/AssetCatalog/Thumbnails`) with `ThumbnailService`.
- **M4 — download & import.** Pluggable `IAssetImporter` strategy (`unitypackage`, `zip+meta`, raw),
  zip-slip / zip-bomb–guarded `SafeExtractor`, sha256 verification, and `AssetInstaller` orchestration
  with scaled progress. Downloads land under `Assets/` only.
- **M5 — Unity Accelerator helper.** `AcceleratorSetupWindow` to configure the Cache Server endpoint
  and probe reachability, fully decoupled from the catalog.
### Changed
- Type-filter dropdown now lists the full taxonomy (model/texture/script/audio/prefab/material/shader).
- Error messages are routed through a single `ErrorText` helper for consistent, friendly wording.
### Fixed
- Texture leak in `ThumbnailCache` when two concurrent decodes raced the same key.
- Orphaned temp downloads left behind when a domain reload tore down an in-flight install are now
  swept on editor load (`AssetInstaller.CleanStaleDownloads`).
- `CatalogHttpClient` no longer lets a synchronous `SendWebRequest` exception escape (e.g. Unity
  blocking insecure HTTP); it is converted to a connection-error result with an actionable message.

## [0.1.0] - 2026-06-05
### Added
- M1: Embedded UPM package skeleton.
- `AssetCatalogSettings` ScriptableObject persisted under `ProjectSettings/` (shared/committed).
- Project Settings page at **Project Settings ▸ Eisenholz ▸ Asset Catalog**.
- `CatalogLog` prefixed logging wrapper.
- `AssetCatalogWindow` (UIToolkit) opened via **Window ▸ Eisenholz ▸ Asset Catalog** — placeholder until M3.
