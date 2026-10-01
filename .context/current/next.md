# Next actions

1. Owner: retest the real installed application with `ProjectAlpha-Update-to-v0.3.4.exe`. This updater supports installed v0.3.0-v0.3.3 directly.
2. If update fails, use the new detailed on-screen diagnostic as evidence; do not guess or silently fall back to full Setup.
3. Import a fresh ZIP produced by the recorder extension through the Session tab.
4. Verify imported counts/state: cookies, request profiles, localStorage, sessionStorage and restored origin.
5. Verify WebView2 opens in the restored authenticated state.
6. Run the current Alfa-Friday non-confirming chain and compare variables/result table with the captured browser sequence.
7. Keep `confirmDrumOffer` explicit; test it only after the non-mutating chain is verified.
