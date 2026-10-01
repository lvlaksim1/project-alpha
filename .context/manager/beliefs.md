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
