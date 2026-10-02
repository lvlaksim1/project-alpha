# Next actions

1. Owner: update the confirmed v0.3.8 installation with `ProjectAlpha-Update-to-v0.3.10.exe`.
2. Confirm that the new module **«Альфа-Онлайн — Суперкэшбэк (октябрь 2026)»** appears in the left drum list. The cumulative updater is specifically tested to seed newly introduced modules into older installations.
3. Import the supplied/fresh Alfa Online Network Recorder ZIP through **Сессия → Импорт capture ZIP** so the `web.alfabank.ru` cookies, exact GET-wheel request profile, XSRF/device context and browser storage become the active session.
4. Select the new Alfa Online module and press **Получить варианты призов**. Verify the 10 observed offers render by human-readable category + discount and that Unicode/emoji are readable.
5. Press **Состояние барабана**. Before a spin, `confirmed=false` may legitimately mean no current prize. If already confirmed, resolve the winner via winner ID/`isWinner` and show its human-readable name.
6. Test **Получить приз** only when the owner intentionally wants the server-side wheel mutation. v0.3.10 maps the captured active frontend branch to `PUT /api/v1/loyalty-view/accept` with `type: "DRUM"`, but this exact request was not present in the supplied capture, so its first real execution is the validation checkpoint.
7. If the claim fails because of security headers, capture the real browser click on **«Крутить скорее!»** with Network Recorder and adapt from that evidence; do not synthesize dynamic `X-GIB` values.
8. Keep the existing Alfa-Friday module regression-safe while validating the new cabinet.
