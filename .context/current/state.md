# Current state

Updated: 2026-10-02 MSK

## Authority
- Authoritative repository: `lvlaksim1/project-alpha`.
- Project Manager context capsule is installed and is the recovery source for future clean chats.
- Current released application version: `v0.3.15`.
- Owner's currently confirmed installed version: `v0.3.8`.

## Application
- Windows desktop client: .NET 8 WPF + WebView2.
- Installation directory: `%LOCALAPPDATA%\Programs\Project Alpha`.
- User/session/work-data root: `%LOCALAPPDATA%\Baraban`.
- Editable drum definitions live under `%LOCALAPPDATA%\Baraban\Drums`; built-in drum JSON files are defaults only.
- WebView2 profile: `%LOCALAPPDATA%\Baraban\WebView2`.
- DPAPI-protected session state: `%LOCALAPPDATA%\Baraban\session.bin`.

## Drum modules
- Current working module: `alfa-friday-tasty-coffee-2026-09-30`.
- Active Alfa Online modules include `alfa-online-supercashback-wheel-2026-10`, `alfa-online-podruzhka-wheel-2026-10`, and `alfa-online-mvideo-wheel-2026-10-02`, each reconstructed from owner-supplied Network Recorder runtime evidence.
- Drum modules can now define per-action pipelines for prize options, state and explicit claim, so UI buttons are no longer tied to LoyaltyRouletteService request IDs.
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
  `ProjectAlpha-Update-to-v0.3.15.exe` (~2.2 MB).
- Owner-side execution of the v0.3.4 updater failed on the real installation with generic exit code 1 despite reconstructed-base CI success.
- The failure class was traced to Windows PowerShell 5.1 handling of the updater script plus insufficient real-install compatibility assumptions.
- v0.3.5 remediation includes:
  - an ASCII-safe updater script explicitly gated under Windows PowerShell 5.1;
  - persisted exact publish manifests for future cumulative bases;
  - exact SHA-256 delta matching as the preferred path;
  - a version-gated rollback-protected repair path for supported mixed/legacy installations when the exact immutable SHA signature does not match;
  - diagnostic output written and decoded reliably so failures surface a human-readable reason;
  - corrected delta/repair path normalization.
- Release workflow run `36938695823` completed successfully for v0.3.5, and the owner confirmed that updater on the real machine.
- v0.3.6 adds automatic display of the executing assembly version in the main Windows title bar as `Baraban vX.Y.Z`.
- v0.3.6 release rerun `36944055642` completed successfully after hardening exact-manifest persistence against concurrent context commits; exact publish manifest `release/manifests/v0.3.6.json` is persisted.
- v0.3.7 redesigns the main UI: automatic scrollbars across application windows/major panes, pretty JSON rendering for JSON text areas, workflow launch moved onto the Result tab, visible log panel removed, explicit empty-state messaging added to Result, raw HTTP Response reduced, and a structured DataGrid view added for JSON responses.
- v0.3.7 release run `36948995021` completed successfully, exact manifest `release/manifests/v0.3.7.json` is persisted, Repository Hygiene succeeded, and the release produced no Actions artifacts.
- Owner verified v0.3.7 on the real machine and reported three UI/runtime issues: outer-window scrolling caused excessive horizontal width, Result remained empty despite a successful chain, and raw HTTP JSON exposed escaped Unicode / insufficiently structured output.
- v0.3.8 replaces outer-window scrolling with scrollbars inside data fields/tables, keeps all panes constrained to the main window, and adds draggable GridSplitter controls for Result and HTTP pane sizing.
- v0.3.8 adds global Ctrl+F/F3 search across visible text fields, DataGrid rows and the drum list.
- v0.3.8 replaces the HTTP request dropdown workflow with three explicit buttons in one persistent workspace: `Получить варианты призов`, `Состояние барабана`, and explicit `Получить приз`. Prize options remain visible while state is fetched; `offerWinId` is resolved to the displayed prize name; confirmation remains a separate user-confirmed mutating action.
- v0.3.8 changes prize projection to adaptively locate prize arrays/fields in the real JSON shape and no longer discards rows merely because `offerId` is absent. This directly addresses the observed v0.3.7 empty Result table.
- JSON rendering now uses relaxed Unicode output and explicit decoding of literal `\\uXXXX` sequences so Russian text is displayed normally.
- v0.3.8 release run `36951899931` completed successfully; exact manifest `release/manifests/v0.3.8.json` is persisted; no Actions artifacts were produced.
- Owner confirmed the v0.3.8 UI is almost ideal and supplied a second Network Recorder capture from a different Alfa Online cabinet/mechanism: `web.alfabank.ru/marketplace/?loyaltyType=104`.
- The new capture directly observes `GET /api/v1/loyalty-view/wheel-of-fortune?basketOfferId=21871`, whose response contains `offers[]` with `id`, `discount`, `partner`, descriptions, `isWinner`, and top-level `confirmed`.
- Captured production JavaScript defines both legacy and new wheel mutations. The captured page has `NewClick_NewWheelOfFortune` enabled, so its active branch uses `PUT /api/v1/loyalty-view/accept` with `basketOfferId` and `type: "DRUM"`; confirmed panels use `GET /wheel-of-fortune/winner`.
- v0.3.10 introduced the Alfa Online Supercashback module and generic per-drum prize/state/claim pipelines. Subsequent runtime evidence proved that `offers[].isWinner` is not a winner authority for this mechanism.
- The raw owner capture is not stored in Git because it contains authentication/session material. Sanitized evidence is documented in `docs/ALFA_ONLINE_SUPERCASHBACK_WHEEL_2026-10.md`.
- v0.3.10 release run `36955307802` completed successfully after correcting cumulative smoke-test semantics for mutable modules. The updater verifies immutable target files exactly, preserves or refreshes existing mutable modules, and requires newly introduced modules to be seeded into legacy installations. Exact manifest `release/manifests/v0.3.10.json` is persisted and the release produced no Actions artifacts.
- v0.3.10 adds multi-mechanism drum support. The new Alfa Online module uses `GET /api/v1/loyalty-view/wheel-of-fortune?basketOfferId=21871` for offers/state, optional `GET .../wheel-of-fortune/winner` for an already-confirmed result, and an explicit user-confirmed `PUT /api/v1/loyalty-view/accept` with `type: DRUM` for the active `NewClick_NewWheelOfFortune` frontend branch.
- New Alfa Online prize rows come from `offers[]`; the authoritative winner is `winnerOffer.id` returned by `PUT /accept` or the dedicated winner GET. In both observed real spins every `offers[].isWinner` remained `false`.
- Session handling now supports host-scoped reusable headers so credentials/context from `web.alfabank.ru` are not leaked to `link.alfabank.ru`; dynamic `X-GIB-*` headers remain request-specific.
- Cumulative updater tests now model mutable `Drums/*.json` correctly: existing user-edited modules may be preserved, while newly introduced built-in modules must be seeded. This fixed the v0.3.10 release-gate false failure without weakening immutable SHA verification.
- v0.3.10 release run `36955307802` completed successfully and produced no Actions artifacts. Update asset SHA-256: `2701ab6ec8621b01de339cd217f2946c2e100e3cdb94ae72979b6737744f2f57`.
- CI verified all supported exact cumulative base paths, a synthetic mixed-install repair path, the compiled single-file Update EXE, the previous-Setup -> update path, and uninstall user-data preservation.
- Update application retains temporary backup, rollback on failure, full target SHA-256 verification and application restart.
## Installer/uninstaller
- Full Setup and Update EXE share fixed Inno Setup `AppId` `{5C23B63A-5308-42A5-8EAB-68FF65A70D31}`.
- Updates refresh the same installed application/uninstall entry rather than creating a parallel installation.
- Interactive uninstall asks exactly:
  `Удалить также настройки и рабочие данные?`
- Answer No: remove program files but preserve `%LOCALAPPDATA%\Baraban`.
- Answer Yes: also remove `%LOCALAPPDATA%\Baraban`.
- Silent/service uninstall preserves user data.
- CI verified one uninstall entry, correct updated DisplayVersion, and user-data preservation during silent uninstall.

## User documentation
- Complete Russian user guide is maintained at `docs/USER_GUIDE.md` and covers every current button/tab plus the normal operating flow and manual confirmation warning.

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


## Alfa Online two-spin runtime evidence
- Full owner capture `Browser-Network-20261002-054552.zip` directly records two real spins and one free repeat transition.
- Initial GET `/wheel-of-fortune?basketOfferId=21871`: `confirmed=false`, ten offers, all `isWinner=false`, button `Крутить скорее!`.
- First `PUT /loyalty-view/accept` with `{"basketOfferId":"21871","type":"DRUM"}` returned `winnerOffer.id=21937` = `Цифровые товары — 7%`, plus `Крутить ещё`, `needPaid=false`, `1 попытка`.
- Clicking `Крутить ещё` did not call a special mutation. It triggered a fresh wheel GET; that GET returned `confirmed=true` while all offer flags still remained false.
- Second `PUT /accept` returned `winnerOffer.id=21940` = `Такси — 7%`.
- After the second spin the server offered another repeat with `needPaid=true`, subtitle `за 49 ₽`, and a payment `modalView.order`.
- A later full paid-repeat capture directly records the payment confirmation request: `POST /api/v1/loyalty-view/offer` with the exact server-supplied `reconfirmButton.modalView.order` object. The response is HTTP 200, `success=true`, amount 49 RUR, and button `Крутить скорее!`.
- The two observed identical PUT requests used different `X-GIB-FGSSCw-...` values, confirming that at least that Group-IB header is occurrence-specific rather than a durable request-profile token.
- v0.3.12 implements dynamic action labels, the observed free reroll state machine, safe loss-warning before forfeiting the first result, automatic return to `Крутить скорее!`, and migration of already-installed Alfa Online module JSON without overwriting unrelated user edits.
- v0.3.12 release run `36957895354` completed successfully; cumulative base tests, hybrid repair, compiled Update EXE smoke-test and uninstall preservation all passed; the release produced no Actions artifacts.


## Alfa Online paid-repeat runtime evidence
- Owner capture `Browser-Network-20261002-061155.zip` directly records **Крутить ещё — за 49 ₽ → Подтвердить → POST /api/v1/loyalty-view/offer**.
- The payment request body is the full account-specific `reconfirmButton.modalView.order` object supplied by the previous winner response. Project Alpha does not hard-code the owner's loyalty/account identifiers into the public module.
- Payment response: HTTP 200, `type=buy-offer`, `direction=EXPENSE`, `success=true`, 49 RUR, button `Крутить скорее!`.
- After payment, the next `Крутить скорее!` produces a wheel GET/reset; the following `Крутить скорее!` performs normal `PUT /accept`.
- The observed paid spin returned `winnerOffer.id=21931` = `Аптеки — 7%` and again advertised another paid repeat.
- v0.3.13 implements a generic paid-repeat pipeline. It extracts the entire payment order at runtime, requires explicit Yes/No confirmation before the real debit, requires both HTTP 2xx and `success=true`, and never retries the POST automatically as part of the subsequent wheel-reset step.
- v0.3.13 migrates already-installed Alfa Online module JSON by adding only the required paid-repeat variables/request/action mappings while preserving unrelated user edits.
- Release run `36959665060` completed successfully; cumulative bases, hybrid repair, final Update EXE smoke test and uninstall preservation passed; no Actions artifacts were produced. Repository Hygiene run `36959917957` completed successfully.


## Manual request editor
- v0.3.14 adds an **Отправить** button to **Параметры последнего запроса (расширенный режим)**.
- Manual send constructs a temporary request from the currently edited Method / URL / Headers / Body and sends it once without persisting those edits to the drum module. **Сохранить параметры** remains the separate persistence action.
- The manual response is rendered in the normal raw/table response areas and the request's capture mappings update runtime variables.
- Manual requests marked `IsConfirmation=true`, and every manually edited method other than GET, require a separate Yes/No confirmation before network transmission.
- The first v0.3.14 release attempt exposed a repair-mode packaging gap: new mutable drum files could be omitted when an old reconstructed base fell back to repair mode. Repair manifests now package all mutable target files as `seedIfMissing`: absent modules are added, existing user-editable modules are never overwritten.
- v0.3.14 release run `36962085875` passed cumulative base tests, hybrid repair, compiled Update EXE smoke-test and uninstall preservation. No Actions artifacts were produced.


## Podruzhka and MVideo one-shot wheels
- Owner capture `Browser-Network-20261002-132502.zip` directly records both partner wheels on `web.alfabank.ru/partner-offers/`.
- Podruzhka: `basketOfferId=22072`; observed options are 100/70/50/30/20%; real PUT /accept returned `winnerOffer.id=22092` = Подружка 50%.
- MVideo: `basketOfferId=21837`; observed options are 10/20/30/50/70/100%; real PUT /accept returned `winnerOffer.id=21867` = М.ВИДЕО 70%.
- Both winner responses return `actionButton.title="Отлично"` and `actionButton.type="endActionButton"`; these are terminal one-shot wheels with no repeat path in the capture.
- v0.3.15 adds both modules and a generic terminal-action contract. When a module-defined terminal variable matches its terminal value, the normal mutating action button is disabled, preventing accidental repeated PUT /accept.
- Sanitized evidence is stored in `docs/ALFA_ONLINE_PARTNER_WHEELS_2026-10-02.md`; the raw capture is not committed because it contains authenticated session/request material.
- v0.3.15 release run `36996176531` completed successfully; cumulative base tests, hybrid repair, final Update EXE smoke-test and uninstall preservation passed; no Actions artifacts were produced.
