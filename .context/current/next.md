# Next actions

1. Owner: update the current Project Alpha installation with `ProjectAlpha-Update-to-v0.3.15.exe`.
2. Verify the new modules appear in the left list:
   - **Альфа-Онлайн — Подружка (01–04.10.2026)**;
   - **Альфа-Онлайн — М.ВИДЕО (02.10.2026)**.
3. Import the fresh capture `Browser-Network-20261002-132502.zip` when validating these modules so the current partner-wheel GET/PUT request profiles and session context are available.
4. For Подружка, press **Получить варианты призов** and verify five options: 100/70/50/30/20%.
5. For М.ВИДЕО, press **Получить варианты призов** and verify six options: 10/20/30/50/70/100%.
6. Only on intentional real use, press **Крутить скорее!**. After a successful one-shot spin, verify `winnerOffer.id` maps to the displayed prize and the action becomes disabled **Отлично** rather than permitting a second PUT.
7. Verify the advanced request editor's **Отправить** button from v0.3.14 still sends edited requests without persisting them unless **Сохранить параметры** is pressed.
8. Keep Alfa Online Supercashback free/paid repeat behavior regression-safe and keep Alfa-Friday behavior regression-safe.
9. If any direct request is rejected, compare fresh browser traffic/request-specific headers; never synthesize changing Group-IB headers.
