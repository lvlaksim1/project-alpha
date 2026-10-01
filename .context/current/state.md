# Current state

Updated: 2026-10-02 MSK

## Authority
- Authoritative repository: `lvlaksim1/project-alpha`.
- Project Manager context capsule is installed and is the recovery source for future clean chats.
- Current released application version: `v0.3.4`.

## Application
- Windows desktop client: .NET 8 WPF + WebView2.
- Installation directory: `%LOCALAPPDATA%\Programs\Project Alpha`.
- User/session/work-data root: `%LOCALAPPDATA%\Baraban`.
- Editable drum definitions live under `%LOCALAPPDATA%\Baraban\Drums`; built-in drum JSON files are defaults only.
- WebView2 profile: `%LOCALAPPDATA%\Baraban\WebView2`.
- DPAPI-protected session state: `%LOCALAPPDATA%\Baraban\session.bin`.

## Drum modules
- Current working module: `alfa-friday-tasty-coffee-2026-09-30`.
- Previous `alfa-loyalty-roulette` module is retained as archive/history only; automatic workflow execution is disabled for archived drums.
- Mutating confirmation remains an explicit user action and must never be auto-run.

## Browser-session ZIP import
- Recorder technology was inspected from the supplied extension source.
- Supported capture format: `browser-session-capture`, `formatVersion: 2`.
- Import currently restores:
  - structured cookies from `browser/end/cookies.json` with start-state fallback;
  - `localStorage` and `sessionStorage` from page snapshot data;
  - endpoint/request-specific headers from `requests.json` and `requestExtraInfo.headers`;
  - capture provenance/metadata;
  - WebView2 state for the captured origin.
- Cookie-header parsing is only a fallback when structured cookies are absent.
- IndexedDB and Cache Storage are recorded by the extension but are not restored yet.
- Dynamic security headers are observed/replayed only; Project Alpha does not synthesize them.

## Distribution and updates
- First/full installation is delivered as `ProjectAlpha-Setup-vX.Y.Z-win-x64.exe`.
- Normal updates are delivered as one self-contained cumulative EXE:
  `ProjectAlpha-Update-to-vX.Y.Z.exe`.
- Current normal update:
  `ProjectAlpha-Update-to-v0.3.4.exe` (~2.1 MB).
- The v0.3.4 updater supports direct update from every installed Project Alpha version `v0.3.0` through `v0.3.3`; intermediate updates are not required.
- CI reconstructs all supported historical publishes from Git history, builds a SHA-256 delta for each base, embeds them into the single EXE, and smoke-tests every supported base to the target publish before release.
- Verified by CI for v0.3.4:
  - v0.3.0 -> v0.3.4;
  - v0.3.1 -> v0.3.4;
  - v0.3.2 -> v0.3.4;
  - v0.3.3 -> v0.3.4.
- Update application uses exact SHA-256 base detection, temporary backup, rollback on failure, post-update hash verification and application restart.
- Update failures expose a human-readable diagnostic reason instead of only an exit code.

## Installer/uninstaller
- Full Setup and Update EXE share fixed Inno Setup `AppId` `{5C23B63A-5308-42A5-8EAB-68FF65A70D31}`.
- Updates refresh the same installed application/uninstall entry rather than creating a parallel installation.
- Interactive uninstall asks exactly:
  `Удалить также настройки и рабочие данные?`
- Answer No: remove program files but preserve `%LOCALAPPDATA%\Baraban`.
- Answer Yes: also remove `%LOCALAPPDATA%\Baraban`.
- Silent/service uninstall preserves user data.
- CI verified one uninstall entry, correct updated DisplayVersion, and user-data preservation during silent uninstall.

## Repository hygiene
- No GitHub Actions artifacts are used or retained.
- Build/update intermediates exist only on the ephemeral runner.
- Generated binaries, archives, captures, session files and local secrets are excluded from Git.
- Build cache is disabled by default.
- Only the latest GitHub Release is retained automatically.
- Repository Hygiene removes Actions artifacts if any appear and keeps only a short diagnostic workflow-run history.
- Current Release contains only:
  - cumulative Update EXE;
  - full Setup EXE fallback.
