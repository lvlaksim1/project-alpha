# Next actions

1. User should retest with ProjectAlpha-Update-to-v0.3.4.exe; it supports direct update from any installed Project Alpha v0.3.0-v0.3.3.
2. On Windows, import a real ZIP produced by the recorder extension through the Session tab.
3. Verify import summary counts for cookies, request profiles, localStorage and sessionStorage.
4. Navigate WebView2 to the captured origin and verify the restored authenticated state.
5. Run only the non-confirming workflow and compare behavior with the recorded browser session.
6. Test confirmation only as an explicit owner action after session restoration is verified.
