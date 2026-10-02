# Current blockers and open risks

1. Full browser-session ZIP restoration still needs end-to-end runtime verification on the owner's Windows machine with a fresh recorder export.
2. IndexedDB and Cache Storage are not restored. Add them only if live testing proves they are required for session continuity.
3. Captured request-specific dynamic security headers may have server-defined freshness/replay limits. Do not attempt to synthesize or bypass them; prefer fresh WebView2-observed traffic when imported values are rejected.
4. Current Alfa-Friday module still needs live non-mutating workflow verification inside Project Alpha after session restoration.
5. Confirmation/mutating requests must remain explicit and separated from discovery/visualization.
