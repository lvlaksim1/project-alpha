# Authorization profiles and session refresh evidence

Updated: 2026-10-02 MSK

Evidence sources: owner-provided Network Recorder captures:
- `Browser-Network-20261002-045909.zip`
- `Browser-Network-20261002-054552.zip`
- `Browser-Network-20261002-061155.zip`
- `Browser-Network-20261002-132502.zip`

Raw captures are not stored in Git because they contain authenticated session material.

## What is directly observed

Across the captures, authenticated Alfa Online traffic uses a combination of:
- short/runtime auth cookies including `alfa-token`, `GW_SESSION_AO`, `passport-session` and `XSRF-TOKEN`;
- long-lived fast-login/device material, including `fastlogin`, `DEVICE_APP_ID`, `DEVICE_PUBLIC_KEY_ID`, `DEVICE_SECRET`;
- a secured device secret in localStorage;
- request-specific security headers that can change between otherwise identical requests.

The captured `alfa-token` JWT has an approximately 12-hour issued-at to expiry interval. The fast-login/device material lives substantially longer.

The production initial-state feature list contains the enabled feature flag:

`authExpiredRefreshToken`

This is strong evidence that Alfa Online has a built-in path for handling an expired auth token.

## What is not observed

None of the four captures contains the actual token-expiry event.

Therefore there is no runtime evidence yet for:
- a concrete refresh endpoint;
- refresh request method;
- refresh request body;
- response schema;
- retry semantics after refresh.

There is no observed OAuth-style `grant_type=refresh_token` request and no recorded URL containing a dedicated token-refresh operation.

Project Alpha must not invent such an endpoint.

## v0.3.16 persistence strategy

Until the actual expiry/refresh exchange is captured, Project Alpha preserves the inputs the real site appears to rely on:

1. Named authorization profiles store cookies, global/host/request headers, capture provenance and browser storage under DPAPI protection.
2. The legacy single `session.bin` is migrated into the default **Основной** profile.
3. Browser cookies are synchronized after relevant Alfa Online responses, not only after navigation.
4. Current localStorage/sessionStorage are synchronized from the embedded browser.
5. Direct HTTP responses feed observed `Set-Cookie` values back into the active profile.
6. On startup or profile switch, Project Alpha clears the previous browser cookies, restores the selected profile, then loads its last saved Alfa Online URL.
7. Loading the real site gives its own `authExpiredRefreshToken` implementation an opportunity to refresh an expired session using the preserved fast-login/device state.
8. Any cookies rotated by that process are persisted back into the active profile.

This uses Alfa Online's real browser behavior instead of synthesizing an undocumented refresh protocol.

## Next evidence needed for an explicit refresh implementation

Run Network Recorder continuously across the moment when the current auth token actually expires, without manually re-logging in first. If the browser silently recovers, the resulting capture should expose the exact refresh request/response. Only then should Project Alpha consider reproducing that exchange directly for HTTP-only operation.
