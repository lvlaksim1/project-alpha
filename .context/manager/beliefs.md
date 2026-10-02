# Manager beliefs

- `lvlaksim1/project-alpha` is the authoritative repository and its capsule is the recovery source for clean chats.
  - source: owner workflow / repository state
  - authority: owner-directive

- Project Alpha is a modular Windows client; it must not be architected as a one-off client for a single drum.
  - source: owner directive
  - authority: owner-directive

- Browser login and saved recorder-session ZIP import are both first-class session paths.
  - source: owner directive
  - authority: owner-directive

- Session lifetime must follow server validity; Project Alpha must not impose an artificial shorter cookie lifetime.
  - source: owner directive
  - authority: owner-directive

- Material HTTP parameters must remain inspectable/editable by the owner.
  - source: owner directive
  - authority: owner-directive

- The old Alfa Loyalty Roulette module remains archive-only until the owner provides additional evidence/data.
  - source: owner directive
  - authority: owner-directive

- The current Alfa-Friday capture and production JavaScript establish the observed chain around `advertCampaignId`, `available`, `offerWinId`, `getCustomerOffersDrum`, `getOfferDrums` and explicit `confirmDrumOffer`.
  - source: owner-supplied capture + captured production bundle
  - authority: verified-runtime evidence

- Request-specific X-GIB/security headers vary by endpoint/request; one global copied header set is not an adequate replay model.
  - source: owner-supplied capture
  - authority: verified-runtime evidence

- Normal software updates must be one EXE, not ZIP/CMD/PS1 bundles.
  - source: owner directive, 2026-10-01
  - authority: owner-directive

- Normal updates must be cumulative: the current Update EXE must directly upgrade every supported installed Project Alpha version from v0.3.0 onward without requiring intermediate updates.
  - source: correction after owner-observed v0.3.3 failure
  - authority: owner-directive + verified-ci

- Program files and user data are intentionally separated:
  - program: `%LOCALAPPDATA%\Programs\Project Alpha`;
  - user data: `%LOCALAPPDATA%\Baraban`.
  - authority: implemented-and-verified

- Uninstall preserves user data by default and deletes it only after explicit interactive confirmation.
  - exact prompt: `Удалить также настройки и рабочие данные?`
  - authority: owner-directive + verified-ci

- CI/repository policy is minimal-storage: no Actions artifacts, no unnecessary caches, latest Release only.
  - source: owner directive
  - authority: owner-directive

- v0.3.4 cumulative updater is verified in CI from v0.3.0, v0.3.1, v0.3.2 and v0.3.3 directly to v0.3.4.
  - source: GitHub Actions release run 36935517796
  - authority: verified-ci

- The v0.3.4 real-machine updater failure was caused by a Windows PowerShell 5.1 compatibility defect: `Apply-Update.ps1` was UTF-8 without BOM and contained Cyrillic text, so Windows PowerShell 5.1 could misdecode and fail during parsing before the script's try/catch and diagnostic-file write.
  - source: owner screenshot + reproduced Windows PowerShell 5.1 CI parse failure, 2026-10-02
  - authority: verified-runtime + verified-ci

- v0.3.5 fixes the updater failure class by keeping the executable updater PowerShell ASCII-safe, gating it explicitly under Windows PowerShell 5.1, adding an exact-SHA path plus a rollback-protected repair path for mixed legacy installations, and persisting exact publish manifests in Git for future cumulative bases.
  - source: GitHub Actions release run 36938695823 and release v0.3.5
  - authority: verified-ci

- Release v0.3.5 is verified end-to-end in CI: cumulative legacy-base tests pass, the synthetic mixed-install repair test passes, the compiled Update EXE passes previous-Setup update/uninstall-preservation testing, Repository Hygiene succeeds, and the release run produced no Actions artifacts.
  - source: GitHub Actions release run 36938695823; Repository Hygiene run 36938956222
  - authority: verified-ci

- The corrected `ProjectAlpha-Update-to-v0.3.5.exe` successfully updated the owner's real installation that had failed with v0.3.4.
  - source: owner runtime confirmation, 2026-10-02
  - authority: owner-observed runtime

- v0.3.6 displays the executing application version in the Windows title bar using the assembly version, so release version changes flow into the UI without a separately hard-coded UI version string.
  - source: implementation commit 3c98c20f28b449ff6c7ef7bd4fbee1f4f3543a5e + release run 36944055642
  - authority: verified-code + verified-ci

- The release workflow now persists exact publish manifests by first rebasing that persistence step onto the latest `origin/main`, preventing unrelated concurrent context/documentation commits from making the final manifest push non-fast-forward.
  - source: workflow commit fb9c3cd901307cfcd9afe0314c2d07ae496ef220 + successful release run 36944055642
  - authority: verified-ci
