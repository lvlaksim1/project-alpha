# Next actions

1. Owner: apply `ProjectAlpha-Update-to-v0.3.8.exe` to the confirmed v0.3.7 installation.
2. Verify that the main window no longer scrolls horizontally as a whole; scrollbars must exist only inside data fields/tables, and the Result/HTTP splitters must resize pane heights by mouse.
3. Verify `Ctrl+F` search in JSON fields and result/HTTP tables.
4. On the HTTP tab, press `Получить варианты призов` and confirm that the prize list is populated with readable Russian text.
5. Press `Состояние барабана`; confirm that the prize list remains visible and the current prize is shown by name by matching `offerWinId` to the prize list.
6. Verify the Result tab / `Запустить цепочку` now populates the same prize table. If it is still empty, use the now-readable real `getOfferDrums` response to refine the adaptive projection against observed runtime data.
7. Test `Получить приз` only when the owner intentionally wants to send the explicit confirmation request; keep the Yes/No confirmation barrier.
8. Continue end-to-end recorder ZIP / authenticated WebView2 verification after the v0.3.8 UI/runtime checkpoint.
