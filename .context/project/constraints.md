# Project constraints

- Repository is public; never commit live cookies, authentication tokens, CSRF values, X-GIB values or other session secrets.
- Session lifetime must not be artificially shortened by Baraban. Server expiry/rejection remains authoritative.
- Both browser-based login and manual cookies/headers injection must remain supported.
- Material HTTP parameters must remain user-visible and editable.
- The current Alfa Loyalty Roulette is only one module; core architecture must remain multi-drum.
- Mutating/confirmation requests such as `confirmDrumOffer` must not auto-run.
- Do not hard-code current `advertCampaignId` or `offerWinId`; derive live values from workflow responses or user-visible runtime variables.
- Exact request contracts not verified from source traffic must be labelled as templates rather than asserted as fact.
