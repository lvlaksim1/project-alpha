# Next actions

1. Owner: retest the real installed application with `ProjectAlpha-Update-to-v0.3.5.exe`. It supports supported legacy bases directly and includes a rollback-protected repair path when the exact legacy SHA signature does not match.
2. If update fails, capture the detailed on-screen diagnostic. The v0.3.5 wrapper reads the updater's UTF-8 diagnostic file reliably; do not guess or silently fall back to full Setup.
3. Import a fresh ZIP produced by the recorder extension through the Session tab.
4. Verify imported counts/state: cookies, request profiles, localStorage, sessionStorage and restored origin.
5. Verify WebView2 opens in the restored authenticated state.
6. Run the current Alfa-Friday non-confirming chain and compare variables/result table with the captured browser sequence.
7. Keep `confirmDrumOffer` explicit; test it only after the non-mutating chain is verified.
