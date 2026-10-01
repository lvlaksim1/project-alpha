# Manager plans

1. Treat `v0.3.4` as the current release baseline.
2. For every future release, create one cumulative `ProjectAlpha-Update-to-vX.Y.Z.exe` plus full Setup only as first-install/recovery fallback.
3. Discover supported bases from Git history and smoke-test every supported base -> target before publishing.
4. Never publish update ZIP/CMD/PS1/standalone manifest files and never create Actions artifacts.
5. Preserve `%LOCALAPPDATA%\Baraban` across updates and ordinary/silent uninstall.
6. Keep interactive uninstall prompt exactly `Удалить также настройки и рабочие данные?`; delete user data only on Yes.
7. Use owner-side v0.3.4 update result as the next runtime checkpoint.
8. Then test a fresh recorder ZIP import and verify WebView2 authenticated-state restoration.
9. Verify the current Alfa-Friday non-mutating chain and result projection.
10. Resolve stale dynamic-header behavior only through legitimate fresh browser-observed session traffic; never synthesize protected headers.
11. Keep confirmation/mutating calls explicit and separate.
12. Keep the 17.09 Alfa module archive-only until the owner supplies new evidence.
