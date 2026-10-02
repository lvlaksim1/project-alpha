# Current blockers and open risks

1. Project Alpha still needs owner-side end-to-end runtime verification with the latest Alfa Online captures after the v0.3.13 implementation.
2. IndexedDB and Cache Storage are not restored. Add them only if live testing proves they are required for session continuity.
3. Multiple captures prove `X-GIB-FGSSCw-...` changes between otherwise identical requests, including GET, PUT and the paid POST. Imported request-specific security values may therefore have server freshness/replay limits. Do not synthesize or bypass them; obtain fresh legitimate browser evidence if replay is rejected.
4. Free and paid Alfa Online repeat mechanics are now directly capture-grounded. The remaining risk is whether Project Alpha's replay with imported session/request profiles is accepted by the live server.
5. App-side testing of the paid path has a real monetary effect (49 ₽ in the observed flow). It must remain explicit and must never be invoked automatically or as part of a non-mutating workflow.
6. Current Alfa-Friday module still needs live regression verification after the multi-mechanism changes.
7. All mutating/payment requests must remain explicit and separated from discovery/visualization.
