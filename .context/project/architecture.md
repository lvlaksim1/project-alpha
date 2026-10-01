# Project architecture

- UI: .NET 8 WPF.
- Browser/authentication: Microsoft WebView2 with a persistent user-data directory under `%LOCALAPPDATA%\Baraban\WebView2`.
- SessionStore: app-level cookies + reusable headers persisted in `%LOCALAPPDATA%\Baraban\session.bin`, protected with Windows DPAPI for the current user.
- HTTP execution: `HttpExecutor` combines session headers/cookies with per-request overrides and supports editable method, URL/query, headers and body.
- Module system: `Drums/*.json` definitions contain requests, captures, variables and result mapping. New drums do not require changes to the core workflow engine when the declarative model is sufficient.
- Workflow: `WorkflowRunner` executes non-confirmation requests sequentially and captures JSON fields into variables; `TemplateResolver` expands `{{variable}}` placeholders.
- Result rendering: `ResultProjector` maps response data into a table and marks the winner.
- First module: `alfa-loyalty-roulette.json`.
