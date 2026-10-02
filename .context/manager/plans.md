# Manager plans

1. Treat `v0.3.12` as the current release baseline.
2. For every future release, create one cumulative `ProjectAlpha-Update-to-vX.Y.Z.exe` plus full Setup only as first-install/recovery fallback.
3. Use persisted exact publish manifests for released bases when available; legacy bases may be reconstructed only as a compatibility fallback. Smoke-test every supported base -> target before publishing.
4. Never publish update ZIP/CMD/PS1/standalone manifest files and never create Actions artifacts.
5. Preserve `%LOCALAPPDATA%\Baraban` across updates and ordinary/silent uninstall.
6. Keep interactive uninstall prompt exactly `Удалить также настройки и рабочие данные?`; delete user data only on Yes.
7. Have the owner apply `ProjectAlpha-Update-to-v0.3.12.exe`; verify the full Alfa Online first-spin/free-repeat/second-spin state machine, then verify the existing Alfa-Friday flow was not regressed.
8. Keep an explicit Windows PowerShell 5.1 syntax gate for `Apply-Update.ps1`; keep that executable script ASCII-safe unless a BOM-safe packaging mechanism is deliberately introduced.
9. Then test a fresh recorder ZIP import and verify WebView2 authenticated-state restoration.
10. Keep the three UI actions driven by generic per-drum action pipelines rather than hard-coded request IDs, so different cabinets/mechanics can coexist.
11. Verify Alfa Online against the full runtime capture: `PUT /accept` is now directly observed twice and `winnerOffer.id` is authoritative. Free repeat is GET-reset then another explicit PUT. Paid repeat remains runtime-unverified at the purchase step and must not auto-pay.
12. Verify the current Alfa-Friday non-mutating chain and adaptive result projection remains compatible.
13. Resolve stale dynamic-header behavior only through legitimate fresh browser-observed session traffic; never synthesize protected headers.
14. Keep confirmation/mutating calls explicit and separate.
15. Keep the 17.09 Alfa module archive-only until the owner supplies new evidence.
