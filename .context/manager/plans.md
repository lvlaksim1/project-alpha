# Manager plans

1. Treat `v0.3.8` as the current release baseline.
2. For every future release, create one cumulative `ProjectAlpha-Update-to-vX.Y.Z.exe` plus full Setup only as first-install/recovery fallback.
3. Use persisted exact publish manifests for released bases when available; legacy bases may be reconstructed only as a compatibility fallback. Smoke-test every supported base -> target before publishing.
4. Never publish update ZIP/CMD/PS1/standalone manifest files and never create Actions artifacts.
5. Preserve `%LOCALAPPDATA%\Baraban` across updates and ordinary/silent uninstall.
6. Keep interactive uninstall prompt exactly `Удалить также настройки и рабочие данные?`; delete user data only on Yes.
7. Have the owner apply `ProjectAlpha-Update-to-v0.3.8.exe` and verify the constrained/resizable Result/HTTP UI, Ctrl+F search, Unicode decoding, and three-button prize workflow on the real installation.
8. Keep an explicit Windows PowerShell 5.1 syntax gate for `Apply-Update.ps1`; keep that executable script ASCII-safe unless a BOM-safe packaging mechanism is deliberately introduced.
9. Then test a fresh recorder ZIP import and verify WebView2 authenticated-state restoration.
10. Verify the current Alfa-Friday non-mutating chain and adaptive result projection. v0.3.8 must accept prize rows without `offerId` and search nested response arrays/fields; if runtime evidence still differs, refine only from the readable actual `getOfferDrums` response.
11. Resolve stale dynamic-header behavior only through legitimate fresh browser-observed session traffic; never synthesize protected headers.
12. Keep confirmation/mutating calls explicit and separate.
13. Keep the 17.09 Alfa module archive-only until the owner supplies new evidence.
