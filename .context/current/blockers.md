# Current blockers and open risks

1. Full browser-session ZIP restoration still needs end-to-end runtime verification on the owner's Windows machine with the new full Alfa Online capture.
2. IndexedDB and Cache Storage are not restored. Add them only if live testing proves they are required for session continuity.
3. The full Alfa Online capture proves `X-GIB-FGSSCw-...` changes between identical repeated requests. Captured dynamic security headers may have server freshness/replay limits. Do not synthesize or bypass them; use fresh legitimate browser evidence if replay is rejected.
4. Alfa Online free repeat is now capture-grounded and implemented, but the owner still needs to verify the two-step flow in Project Alpha on the real account.
5. Paid repeat is only partially grounded: the response and production JS expose `POST /api/v1/loyalty-view/offer` plus the exact `modalView.order`, but the owner did not execute payment in the capture. Project Alpha must not automatically submit this paid action yet.
6. Current Alfa-Friday module still needs live regression verification after the multi-mechanism changes.
7. All mutating requests must remain explicit and separated from discovery/visualization.
