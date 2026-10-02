# Current blockers and open risks

1. Full browser-session ZIP restoration still needs end-to-end runtime verification on the owner's Windows machine with a fresh recorder export.
2. IndexedDB and Cache Storage are not restored. Add them only if live testing proves they are required for session continuity.
3. Captured request-specific dynamic security headers may have server-defined freshness/replay limits. Do not attempt to synthesize or bypass them; prefer fresh WebView2-observed traffic when imported values are rejected.
4. Current Alfa-Friday module still needs live non-mutating workflow verification inside Project Alpha after session restoration.
5. Confirmation/mutating requests must remain explicit and separated from discovery/visualization.

6. The new Alfa Online non-mutating wheel GET is capture-verified, but the actual spin/claim request was not present in the supplied capture. The `PUT /api/v1/loyalty-view/accept` + `type: DRUM` path is grounded in captured production JavaScript and the active feature flag, but still requires owner-side runtime verification.
7. The new cabinet may require fresh endpoint-specific security headers for winner/claim endpoints. Preserve the policy: replay only legitimately captured/observed values; if the direct claim is rejected, obtain a real browser capture of that endpoint rather than synthesizing protected headers.
