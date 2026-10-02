# Current blockers and open risks

2. Full browser-session ZIP restoration still needs end-to-end runtime verification on the owner's Windows machine with a fresh recorder export.
3. IndexedDB and Cache Storage are not restored. Add them only if live testing proves they are required for session continuity.
4. Captured request-specific dynamic security headers may have server-defined freshness/replay limits. Do not attempt to synthesize or bypass them; prefer fresh WebView2-observed traffic when imported values are rejected.
5. Current Alfa-Friday module still needs live non-mutating workflow verification inside Project Alpha after session restoration.
6. Confirmation/mutating requests must remain explicit and separated from discovery/visualization.
