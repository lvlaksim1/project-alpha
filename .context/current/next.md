# Next actions

1. Owner: apply `ProjectAlpha-Update-to-v0.3.7.exe` to the currently installed Project Alpha and verify the redesigned UI: title version, scrollbars, moved `Запустить цепочку` button, no visible log panel, and the new HTTP response table.
2. Import a fresh ZIP produced by the recorder extension through the Session tab.
3. Verify restored cookies, request profiles, localStorage/sessionStorage and authenticated WebView2 state.
4. Run the current Alfa-Friday non-confirming chain and verify that the Result tab now produces the intended table rather than an unexplained empty area.
5. If Result remains empty after a successful chain, inspect the actual `getOfferDrums` JSON response and correct the module's result mapping from observed runtime data.
6. Keep `confirmDrumOffer` explicit; test it only after the non-mutating chain is verified.
