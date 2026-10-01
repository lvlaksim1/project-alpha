# Current state

Updated: 2026-10-01 19:26 MSK

- Public repository `lvlaksim1/project-alpha` is the authoritative repository.
- Project Manager capsule is installed and populated with the current project state.
- Current working drum: `alfa-friday-tasty-coffee-2026-09-30`.
- Previous `alfa-loyalty-roulette` module is retained as an archive; automatic workflow execution is disabled for archived drums.
- Browser session ZIP technology from the supplied recorder extension v1.6.0 has been inspected directly from its source.
- Recorder export format is verified as `browser-session-capture`, formatVersion 2.
- Full session ZIP import is implemented:
  - structured cookies with expiry/domain/path/HttpOnly/Secure/SameSite;
  - end-state localStorage and sessionStorage;
  - request-specific captured headers from requests.json/requestExtraInfo;
  - capture metadata/provenance;
  - WebView2 restoration for the captured origin.
- Cookie-header parsing remains only a fallback when a capture lacks structured cookies.
- Commit `5558858f4e388e95d6746a7ad9916003520092aa` passed Windows CI (run 36891659555).
- No live cookies/tokens/security-header values are stored in the public repository.
- Repository storage hygiene remains enforced: no Actions artifacts/caches, generated captures/binaries excluded from Git, latest release only.

- Windows distribution has been converted to an installer pipeline:
  - fixed Inno Setup AppId for in-place upgrades;
  - per-user installation under %LOCALAPPDATA%\Programs\Project Alpha;
  - self-contained win-x64 publish;
  - no Actions artifacts or retained intermediate packages;
  - final Setup.exe uploaded directly to GitHub Release.

- Incremental updater architecture now mirrors `chatgpt-desktop-local-bridge`:
  - exact SHA-256 publish manifest;
  - delta package contains only changed/added files plus delete list and updater;
  - strict base-version validation;
  - temporary backup and rollback on failure;
  - post-update hash verification and automatic application restart.
- Editable drum definitions are now seeded into `%LOCALAPPDATA%\Baraban\Drums` and edited there, outside the installation directory.
- The first delta updater migrates any existing installed `Drums/*.json` into the user data directory before replacing program files.
- Full Setup remains a fallback/first-install asset; normal upgrades use `ProjectAlpha-Update-from-vA-to-vB.zip`.

- First production delta release is verified: `v0.3.0 -> v0.3.1`.
  - update asset: `ProjectAlpha-Update-from-v0.3.0-to-v0.3.1.zip`;
  - size: 131,018 bytes (~128 KB);
  - release workflow completed successfully;
  - Repository Hygiene completed successfully and removed the older release.
- The current release contains the delta ZIP as the normal upgrade path, a small exact publish manifest for the next delta, and a full Setup only as first-install/recovery fallback.
