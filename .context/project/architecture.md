# Project architecture

## Runtime
- UI: .NET 8 WPF.
- Embedded browser: Microsoft WebView2.
- Program install root: `%LOCALAPPDATA%\Programs\Project Alpha`.
- User-data root: `%LOCALAPPDATA%\Baraban`.
- WebView2 persistent profile: `%LOCALAPPDATA%\Baraban\WebView2`.
- DPAPI-protected app session: `%LOCALAPPDATA%\Baraban\session.bin`.
- Editable drum definitions: `%LOCALAPPDATA%\Baraban\Drums`.

## Session import
- Supported recorder format: `browser-session-capture` formatVersion 2.
- Structured cookies: `browser/end/cookies.json` with start-state fallback.
- Browser storage: localStorage/sessionStorage from end page snapshot with start fallback.
- Request profiles: `requests.json`, preferring actual `requestExtraInfo.headers`.
- Profiles are keyed per method + scheme/host/path so endpoint-specific security headers are not flattened into one global set.
- Imported storage is restored into WebView2 for the captured origin.
- IndexedDB/Cache Storage restoration is not implemented yet.

## HTTP/workflow
- `HttpExecutor` combines session cookies/global headers, request-specific captured profiles and editable request overrides.
- `Drums/*.json` definitions describe requests, captures, variables and result mappings.
- `WorkflowRunner` executes non-confirming requests sequentially.
- `TemplateResolver` expands `{{variable}}` values.
- `ResultProjector` renders result rows and winner mapping.
- Confirmation requests are explicit and excluded from automatic workflow execution.

## Distribution
- Full install/recovery: `ProjectAlpha-Setup-vX.Y.Z-win-x64.exe`.
- Normal update: cumulative `ProjectAlpha-Update-to-vX.Y.Z.exe`.
- Fixed Inno Setup AppId: `{5C23B63A-5308-42A5-8EAB-68FF65A70D31}`.
- Update EXE embeds one SHA-verified delta for each supported historical base, detects the installed base by file hashes, backs up affected files, applies the matching delta, verifies target hashes, rolls back on failure and restarts the app.
- CI reconstructs historical publishes from Git history and smoke-tests every supported upgrade path before publication.
- Update/full installer share the same AppId so the installed application/uninstaller remains one logical product.

## Repository/release hygiene
- No `actions/upload-artifact`.
- No persistent build cache by default.
- Build/publish/delta staging exists only on ephemeral GitHub runners.
- GitHub Release keeps only the current cumulative Update EXE and full Setup fallback.
- Repository Hygiene deletes stale Releases, artifacts and old workflow-run history according to project policy.
