# Current blockers and open risks

1. Named authorization profiles and strengthened browser-state persistence are release-verified but still need owner-side runtime verification across real close/reopen and account switching.
2. The actual Alfa Online refresh-token exchange is not present in any supplied capture. Only the enabled `authExpiredRefreshToken` feature flag and long-lived fast-login/device inputs are observed. A capture spanning real token expiry is required before implementing a direct refresh API.
3. IndexedDB and Cache Storage are still not copied into SessionProfile. Add them only if runtime profile/refresh testing proves they are required.
4. Multiple captures prove request-specific Group-IB/security values change between otherwise identical requests. Do not synthesize or bypass them; obtain fresh legitimate browser evidence when replay is rejected.
5. A second PUT after `endActionButton` for Подружка/М.ВИДЕО is intentionally exposed by owner directive, but the server outcome is unknown. It remains an explicit confirmed action and must never auto-run.
6. Paid Supercashback retry has a real monetary effect and must remain explicit.
7. Existing Alfa-Friday and Supercashback flows need regression verification after the session/profile changes.
