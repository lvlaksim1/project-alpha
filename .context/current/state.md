# Current state

Updated: 2026-09-30 04:09 MSK

- Public repository `lvlaksim1/project-alpha` is the authoritative repository.
- Current code commit `0f41ba66bafee1d2ee10fa7bd9487d42c527ddba` passed Windows CI (run 36653647639).
- Current working drum: `alfa-friday-tasty-coffee-2026-09-30`, reconstructed from owner-supplied browser session captured 2026-09-30.
- Previous `alfa-loyalty-roulette` module is retained as an archive; automatic workflow execution is disabled for archived drums.
- Verified current workflow:
  - GET offer 21898;
  - derive `advertCampaignId` from offer `drumId`;
  - `getCustomerOffersDrum` with `advertCampaignId`;
  - capture `available` and `offerWinId`;
  - `getOfferDrums` with `offerDrumId=available`;
  - winner comparison `offerDrumId == offerWinId`;
  - explicit-only `confirmDrumOffer`.
- Session layer now supports request-specific header profiles because X-GIB values were observed to change between endpoints.
- Recorder ZIP import reads `requests.json`, imports cookies and request-specific reusable headers locally, and persists them through DPAPI-protected SessionStore.
- No live cookies/tokens/security-header values are stored in the public repository.
