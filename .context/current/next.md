# Next actions

1. Owner: update the current v0.3.8 installation with `ProjectAlpha-Update-to-v0.3.10.exe`.
2. Import the supplied/new fresh Network Recorder ZIP for the Alfa Online cabinet and select **«Альфа-Онлайн — Суперкэшбэк (октябрь 2026)»**.
3. Press **«Получить варианты призов»** and verify that the ten `offers[]` rows render with readable partner/discount names.
4. Press **«Состояние барабана»**. If `confirmed=false`, the UI should report that the current prize is not yet determined; if already confirmed, it should resolve the winner by `winnerOfferId` or `isWinner`.
5. Only when the owner intentionally wants to spin/claim, press **«Получить приз»** and confirm Yes. This sends the new-cabinet mutating action only after explicit confirmation.
6. Treat the `PUT /api/v1/loyalty-view/accept` path as runtime-unverified until this real-machine test succeeds: the capture did not contain an actual spin request, although the endpoint/body were recovered from the captured production JavaScript and the active `NewClick_NewWheelOfFortune` feature flag.
7. If the mutating request is rejected because fresh endpoint-specific security headers are required, capture the real browser spin request and adapt only from that runtime evidence; do not synthesize protected headers.
8. Recheck the existing Alfa-Friday module after v0.3.10 to confirm the generic action-pipeline fallback preserved its behavior.
