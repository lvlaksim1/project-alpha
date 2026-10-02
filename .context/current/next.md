# Next actions

1. Owner: update the current installation with `ProjectAlpha-Update-to-v0.3.12.exe`.
2. Import the new full capture `Browser-Network-20261002-054552.zip` through **Сессия → Импорт capture ZIP**.
3. Select **«Альфа-Онлайн — Суперкэшбэк (октябрь 2026)»** and verify **Получить варианты призов** still renders the ten observed offers.
4. Verify the primary action button is **«Крутить скорее!»**, not generic **«Получить приз»**.
5. On an intentional real spin, verify `winnerOffer.id` is captured and the matching prize row/name is shown.
6. If the response offers the observed free repeat, verify the button changes to **«Крутить ещё — 1 попытка»**. Confirming it must warn that the previous result will be lost, perform the observed wheel GET reset, clear the previous winner in the UI, and return the button to **«Крутить скорее!»**.
7. A second real spin should replace the previous winner. If the response offers **«Крутить ещё — за 49 ₽»**, Project Alpha must only display/explain the paid option; no payment request is sent automatically.
8. If Alfa Online PUT replay fails because of freshness/security headers, capture that failure/current browser request. Do not synthesize dynamic Group-IB values; the full capture already proves `X-GIB-FGSSCw-...` changes between identical PUT requests.
9. Regression-check the existing Alfa-Friday module after the new-cabinet validation.
