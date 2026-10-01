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
- Full Setup remains a fallback/first-install asset; normal upgrades use one `ProjectAlpha-Update-from-vA-to-vB.exe`.

- First production delta release is verified: `v0.3.0 -> v0.3.1`.
  - update asset: `ProjectAlpha-Update-from-v0.3.0-to-v0.3.1.zip`;
  - size: 131,018 bytes (~128 KB);
  - release workflow completed successfully;
  - Repository Hygiene completed successfully and removed the older release.
- The current release contains the delta ZIP as the normal upgrade path, a small exact publish manifest for the next delta, and a full Setup only as first-install/recovery fallback.

- Update packaging policy changed by owner directive: normal updates must be a single EXE, never a ZIP/CMD bundle.
- Release pipeline now builds a temporary delta directory, smoke-tests it against a reconstructed previous publish, then embeds manifest + payload + updater into one Inno Setup self-extracting update EXE.
- No update ZIP, CMD, standalone PowerShell, or standalone publish manifest is intended to be published from v0.3.2 onward.

- First verified single-file incremental update is `v0.3.1 -> v0.3.2`.
  - normal update asset: `ProjectAlpha-Update-from-v0.3.1-to-v0.3.2.exe`;
  - size: 2,203,533 bytes (~2.1 MB);
  - delta was smoke-tested against a reconstructed v0.3.1 publish before packaging;
  - release workflow and Repository Hygiene both completed successfully;
  - the build and release runs created no GitHub Actions artifacts;
  - current release exposes only two executable assets: normal Update EXE and fallback full Setup EXE.

- Uninstall policy updated:
  - application files: `%LOCALAPPDATA%\Programs\Project Alpha`;
  - user/session/work data: `%LOCALAPPDATA%\Baraban`;
  - interactive uninstall asks `Удалить также настройки и рабочие данные?`;
  - No preserves the entire user-data directory;
  - Yes deletes it after application uninstall;
  - silent uninstall preserves user data.
- Incremental update installer now shares the application's fixed Inno AppId and carries the full uninstall code so normal small updates also refresh the installed uninstaller.

- v0.3.3 verified:
  - incremental update from v0.3.2 is a single EXE;
  - Update EXE uses the same fixed Inno AppId and refreshes the shared uninstall log;
  - CI installed the previous full Setup, applied the incremental Update EXE, verified exactly one Project Alpha uninstall entry at version 0.3.3, then silently uninstalled it;
  - silent uninstall preserved a sentinel under `%LOCALAPPDATA%\Baraban`;
  - interactive uninstall code asks `Удалить также настройки и рабочие данные?` and only deletes `%LOCALAPPDATA%\Baraban` on Yes.

- Release `v0.3.3` verified the uninstall-data policy end-to-end in CI.
  - Update path `v0.3.2 -> v0.3.3` uses one EXE: `ProjectAlpha-Update-from-v0.3.2-to-v0.3.3.exe`.
  - The incremental updater shares the fixed Inno AppId with the full installer and updates the existing uninstall entry instead of creating a parallel application.
  - CI verified exactly one `Project Alpha` uninstall entry after applying the update, with DisplayVersion `0.3.3`.
  - CI performed a silent uninstall and verified that `%LOCALAPPDATA%\Baraban` remained intact.
  - Interactive uninstall code asks exactly `Удалить также настройки и рабочие данные?`; only an explicit Yes deletes `%LOCALAPPDATA%\Baraban`.
  - Release and Repository Hygiene workflows both completed successfully and created no Actions artifacts.

- A real user update attempt exposed a design flaw in v0.3.3: the updater only accepted exactly v0.3.2 and surfaced failures as generic exit code 1.
- Updater architecture has been changed to cumulative multi-base detection:
  - supported installed bases are discovered from Git history starting at v0.3.0;
  - a separate SHA-verified delta is embedded for each supported base inside one EXE;
  - updater selects the correct delta by matching actual installed program hashes;
  - the user no longer needs to install intermediate updates;
  - failures write a detailed reason that the Inno wrapper displays to the user.
- New user-facing naming is `ProjectAlpha-Update-to-vX.Y.Z.exe`, because one update EXE supports multiple source versions.

- Cumulative updater v0.3.4 is verified end-to-end.
  - single user-facing asset: `ProjectAlpha-Update-to-v0.3.4.exe`;
  - size: 2,205,274 bytes (~2.1 MB);
  - CI discovered supported installed bases automatically from Git history: v0.3.0, v0.3.1, v0.3.2, v0.3.3;
  - CI reconstructed each historical publish and successfully updated every base directly to v0.3.4;
  - cumulative Update EXE also passed same-AppId/uninstall-entry/silent-user-data-preservation checks;
  - updater failures now surface the actual diagnostic reason instead of only numeric exit code 1;
  - Release v0.3.4 contains only Update EXE and fallback full Setup EXE;
  - no Actions artifacts were created;
  - Repository Hygiene completed successfully after release.
