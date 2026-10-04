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

  - source: legacy-v2-state
- Uninstall preserves user data by default and deletes it only after explicit interactive confirmation.
  - exact prompt: `Удалить также настройки и рабочие данные?`
  - authority: owner-directive + verified-ci

  - source: legacy-v2-state
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

- Owner-confirmed v0.3.7 runtime evidence showed that the request chain itself succeeds (advertCampaignId, available and offerWinId were populated) while the Result table remained empty. Therefore the immediate defect was in result projection/UI interpretation rather than chain execution.
  - source: owner screenshot and runtime report, 2026-10-02
  - authority: owner-observed runtime

- Prize/result projection must not require `offerId` as a prerequisite for displaying a prize row. It should locate the actual prize collection adaptively and use stable prize/drum identifiers plus readable title fields.
  - source: v0.3.7 runtime failure analysis + v0.3.8 remediation
  - authority: verified-code + owner-observed runtime

- The owner's preferred HTTP workflow is action-oriented rather than request-list-oriented: one persistent workspace with explicit buttons for prize options, drum state, and prize claim; the prize list must remain visible while state is fetched, and current prize ID must be resolved to its human-readable name from that list.
  - source: owner directive, 2026-10-02
  - authority: owner-directive

- Scrolling belongs inside data controls, not around the whole application window. Multi-pane data areas should be mouse-resizable with splitters while staying bounded by the main window.
  - source: owner directive after v0.3.7 runtime review, 2026-10-02
  - authority: owner-directive

- The 2026-10-02 owner-supplied capture establishes a second, distinct wheel mechanism on `web.alfabank.ru`, separate from the older `link.alfabank.ru` LoyaltyRouletteService mechanism.
  - observed options/state endpoint: `GET /api/v1/loyalty-view/wheel-of-fortune?basketOfferId=21871`
  - response shape: root `confirmed` plus `offers[]` with `id`, `partner`, `discount`, and `isWinner`
  - authority: owner-supplied runtime capture

  - source: legacy-v2-state
- Captured production JavaScript defines two winner branches. The captured page has `NewClick_NewWheelOfFortune` enabled, so its active mutating branch is `PUT /api/v1/loyalty-view/accept` with `basketOfferId` and `type: DRUM`; the supplied capture itself did not contain an actual spin request.
  - authority: verified captured production code + feature configuration; mutating endpoint not yet owner-runtime-verified

  - source: legacy-v2-state
- Different cabinets must not share host-specific session/security context indiscriminately. Reusable host headers are scoped by host, while dynamic `X-GIB-*`/trace/request headers remain endpoint/request-specific.
  - authority: architecture requirement derived from multi-cabinet capture evidence

  - source: legacy-v2-state
- Existing user-editable drum JSON files are mutable state and may intentionally differ from built-in target files. Update verification must require exact hashes for immutable program files, preserve legitimate existing drum edits, and still ensure newly introduced built-in drum modules are seeded.
  - authority: updater design + verified CI v0.3.10

  - source: legacy-v2-state
- Full owner capture `Browser-Network-20261002-054552.zip` directly records two Alfa Online spins. Both real `PUT /api/v1/loyalty-view/accept` calls use body `{"basketOfferId":"21871","type":"DRUM"}`; the first returned winner 21937 (Цифровые товары 7%), the second 21940 (Такси 7%).
  - source: owner-supplied full runtime capture, 2026-10-02
  - authority: verified-runtime evidence

- `offers[].isWinner` is not an Alfa Online winner authority. It remained false for every offer before, between and after the two observed spins. The winner authority is `winnerOffer.id` from the PUT response, or the dedicated winner GET when reloading an already-confirmed result.
  - source: full runtime capture + captured production frontend behavior
  - authority: verified-runtime evidence

- Free repeat semantics are two-phase: the server's first winner response exposes `reconfirmButton.needPaid=false` / `1 попытка`; clicking `Крутить ещё` performs only a fresh wheel GET/reset, then the user must invoke `Крутить скорее!` again for the second PUT. Starting that repeat forfeits the previous result.
  - source: action timestamps + API chronology in full capture
  - authority: verified-runtime evidence

- After the second observed spin the server offered `needPaid=true`, `за 49 ₽`, with a complete payment `modalView.order`. A later owner capture directly records confirmation: the browser sends `POST /api/v1/loyalty-view/offer` with that exact order object and receives HTTP 200 / `success=true` for a 49 RUR debit.
  - source: owner-supplied paid-repeat capture `Browser-Network-20261002-061155.zip`
  - authority: verified-runtime evidence

- Identical repeated Alfa Online requests can carry different `X-GIB-FGSSCw-...` values. Request path alone is therefore not a sufficient durability scope for all Group-IB headers.
  - source: full capture sequences 25/42/62/64
  - authority: verified-runtime evidence


- Paid Alfa Online repeat semantics are now directly observed: payment POST first, then a separate wheel GET/reset, then a separate normal PUT /accept to determine the new winner. The paid POST must never be bundled into an automatically retried chain with the reset/spin.
  - source: action/API chronology in paid-repeat capture
  - authority: verified-runtime evidence

- The observed payment body is account-specific and must come from `reconfirmButton.modalView.order` at runtime. Public module code must never hard-code the owner's loyalty/account identifiers or the captured order payload.
  - source: paid-repeat request body + public-repository secrecy constraint
  - authority: verified-runtime + architecture constraint

- Payment success is evidenced by HTTP 2xx plus response `success=true`; in the observed capture the response also reports 49 RUR and button `Крутить скорее!`.
  - source: paid-repeat response body
  - authority: verified-runtime evidence

- After the observed paid purchase, the subsequent spin returned winner 21931 (Аптеки 7%) and again exposed a paid repeat offer, so the paid cycle can recur.
  - source: paid-repeat capture
  - authority: verified-runtime evidence

- The owner requires the advanced last-request editor to support a one-shot **Отправить** action for the currently edited Method / URL / Headers / Body, distinct from **Сохранить параметры**. Sending must not silently persist the edits to the module.
  - source: owner directive, 2026-10-02
  - authority: owner-directive

- Manual edited requests may be mutating even when the original request was not, because Method is editable. Therefore v0.3.14 requires explicit confirmation for `IsConfirmation=true` requests and for every manually edited non-GET method.
  - source: v0.3.14 implementation safety boundary
  - authority: implemented-and-verified-ci

- Repair-mode distribution of mutable built-in modules uses seed-if-missing semantics: all target mutable module files are available in the repair payload, but are copied only when absent. Existing user-editable module files are never overwritten by repair seeding.
  - source: v0.3.14 release-gate remediation
  - authority: verified-ci

- The owner-provided capture `Browser-Network-20261002-132502.zip` establishes two additional Alfa Online one-shot partner wheels.
  - Подружка: basketOfferId 22072; five options 20–100%; observed winner 22092 = 50%.
  - М.ВИДЕО: basketOfferId 21837; six options 10–100%; observed winner 21867 = 70%.
  - source: owner-supplied runtime capture, 2026-10-02
  - authority: verified-runtime evidence

- Both Подружка and М.ВИДЕО return `actionButton.type=endActionButton` / `title=Отлично` after their observed PUT /accept. This is terminal evidence: the ordinary action UI must not offer another spin for these modules.
  - source: observed winner responses in the owner capture
  - authority: verified-runtime evidence

- Terminal behavior is module data, not partner-specific UI logic. Project Alpha supports a module-defined terminal variable/value pair so future one-shot wheels can disable their mutating action without hard-coded partner names.
  - source: v0.3.15 architecture
  - authority: implemented-and-verified-ci

- Owner directive supersedes the previous terminal-wheel UI restriction for Подружка and М.ВИДЕО: after `endActionButton`, the normal UI must still expose a user-confirmed second `PUT /accept`. The capture does not establish what the server will return.
  - source: owner directive, 2026-10-02
  - authority: owner-directive

- Authorization must survive Project Alpha restarts and support multiple named profiles. v0.3.16 stores each profile separately under DPAPI, persists the active profile, and migrates the legacy single session into **Основной**.
  - source: owner directive + v0.3.16 implementation
  - authority: owner-directive + verified-ci

- Session durability requires preserving live browser state, not only an imported snapshot. v0.3.16 synchronizes rotating cookies after relevant browser responses, current local/session storage after navigation/manual sync, and `Set-Cookie` values returned to direct HTTP calls.
  - source: v0.3.16 implementation
  - authority: verified-code + verified-ci

- All four supplied Alfa Online captures contain the enabled production feature flag `authExpiredRefreshToken`, plus long-lived fast-login/device credentials and a shorter-lived working auth session, but none records the actual expiry/refresh request. No refresh endpoint/body may be inferred from the flag alone.
  - source: owner-supplied captures 045909, 054552, 061155, 132502
  - authority: verified-capture evidence

- The observed Alfa working token has approximately a 12-hour iat-to-exp interval, while fast-login/device state persists substantially longer. Restoring the real site with this preserved state is the evidence-based way to give its own silent refresh logic a chance to operate until an actual refresh exchange is captured.
  - source: sanitized token metadata + capture cookie/storage metadata
  - authority: verified-capture evidence
