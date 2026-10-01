# Current state

Updated: 2026-10-02 MSK

## Authority
- Authoritative repository: `lvlaksim1/project-alpha`.
- Project Manager context capsule is installed and is the recovery source for future clean chats.
- Current released application version: `v0.3.5`.

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
  `ProjectAlpha-Update-to-v0.3.5.exe` (2,206,574 bytes, ~2.1 MB).
- v0.3.5 remediates the real-machine v0.3.4 failure caused by Windows PowerShell 5.1 misreading a UTF-8-without-BOM updater script containing Cyrillic text; the executable updater script is now ASCII-safe and is explicitly parsed under Windows PowerShell 5.1 in CI.
- The updater first attempts exact SHA-256 base detection. If a supported legacy installation is in a mixed/non-exact state, a bounded repair path can be selected from the executable version; repair still uses backup/rollback and verifies the complete immutable target publish by SHA-256 before success.
- CI verified every supported legacy base to v0.3.5 and also a deliberately mixed legacy install that forces repair mode.
- The final compiled `ProjectAlpha-Update-to-v0.3.5.exe` was tested against the previous real Setup, including the shared uninstall entry and silent user-data preservation.
- Exact publish manifests are now persisted in Git under `release/manifests/` after successful releases so future cumulative deltas can use actual released hashes rather than reconstructed historical binaries.
- Release run 36938695823 and Repository Hygiene run 36938956222 completed successfully; the release run created no Actions artifacts.
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
