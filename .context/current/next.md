# Next actions

1. Owner: update the current Project Alpha installation with `ProjectAlpha-Update-to-v0.3.14.exe`.
2. Verify the new **Отправить** button in **Параметры последнего запроса (расширенный режим)**: edit a harmless GET request, send it without saving, and confirm the response appears while the module file remains unchanged unless **Сохранить параметры** is pressed.
10. Import the newest capture `Browser-Network-20261002-061155.zip` when testing the paid-repeat endpoint, because it contains the observed POST /offer request profile in addition to the wheel GET/PUT profiles.
3. Verify the Alfa Online module still shows the correct ten prize options and resolves `winnerOffer.id` to the human-readable prize name.
4. Verify free repeat remains: **Крутить ещё — 1 попытка → GET reset → Крутить скорее! → PUT /accept**.
5. When a paid repeat is offered, Project Alpha should show **Крутить ещё — за 49 ₽** and a separate warning that confirmation causes a real debit.
6. Do not perform the app-side paid-repeat validation unless the owner intentionally wants to spend another 49 ₽. If performed: after Yes, require POST /offer HTTP 2xx + `success=true`; then the button must become **Крутить скорее!** without a second payment.
7. The next click after a successful payment must prepare the paid attempt via GET only. The following click performs PUT /accept and displays the new `winnerOffer`.
8. If any direct request is rejected, compare fresh browser traffic/request-specific headers. Never synthesize changing Group-IB headers.
9. Regression-check the existing Alfa-Friday module after Alfa Online validation.
