# Project architecture

- UI: .NET 8 WPF.
- Browser/authentication: Microsoft WebView2 with a persistent user-data directory under `%LOCALAPPDATA%\Baraban\WebView2`.
- SessionStore: app-level cookies, reusable/request-specific headers, imported browser storage and capture provenance persisted in `%LOCALAPPDATA%\Baraban\session.bin`, protected with Windows DPAPI for the current user.
- Session capture ZIP import:
  - validates `session-manifest.json` format `browser-session-capture`, formatVersion 2;
  - imports structured end-state cookies from `browser/end/cookies.json` (start-state fallback);
  - imports `localStorage` and `sessionStorage` from `browser/end/page-snapshot.json` (start-state fallback);
  - imports actual request headers from `requests.json`, preferring `requestExtraInfo.headers` when present;
  - keeps endpoint-specific profiles for API/session-sensitive requests rather than flattening dynamic headers into one global set;
  - restores cookies and browser storage into WebView2 for the recorded origin.
- HTTP execution: `HttpExecutor` combines session headers/cookies with per-request overrides and supports editable method, URL/query, headers and body.
- Module system: `Drums/*.json` definitions contain requests, captures, variables and result mapping. New drums do not require changes to the core workflow engine when the declarative model is sufficient.
- Workflow: `WorkflowRunner` executes non-confirmation requests sequentially and captures JSON fields into variables; `TemplateResolver` expands `{{variable}}` placeholders.
- Result rendering: `ResultProjector` maps response data into a table and marks the winner.
