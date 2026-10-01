# Current blockers and open risks

- The ZIP format implementation is verified against the supplied extension v1.6.0 source and compiles successfully, but restoration with a fresh real exported ZIP must still be runtime-tested on Windows.
- The recorder captures additional data (HAR, bodies, IndexedDB, Cache Storage, downloads, DOM/MHTML, tracing), but Project Alpha currently imports only the state needed for session restoration: cookies, request headers, localStorage/sessionStorage and capture metadata.
- IndexedDB and Cache Storage restoration is intentionally not attempted yet; if a target site proves to require either for authentication/session continuity, that can be added based on observed need.
- Captured dynamic security headers are never synthesized. Imported request-specific values may be rejected by the server if their own freshness rules have expired; live WebView2 traffic remains the preferred refresh path.
