# Project rules

- Never commit user session secrets to the public repository.
- Do not hard-code current campaign/winner IDs.
- Do not auto-run confirmation/mutating requests.
- Keep browser login and manual HTTP-session import as first-class paths.
- Keep all material request fields user-visible and editable.
- Treat unverified API details as templates/unknowns, not facts.
- Preserve multi-drum modularity when extending the first Alfa module.
- Repository storage policy: source/configuration only in Git; generated binaries, archives, captures, session files and build outputs are forbidden.
- CI must not upload Actions artifacts unless the owner explicitly approves a concrete need.
- Do not enable dependency/build caches by default; any future cache must have a documented benefit and bounded retention.
- Release distribution may use a GitHub Release asset, but only the latest release is retained automatically.
- Repository Hygiene deletes all Actions artifacts after successful releases and keeps only the eight most recent completed workflow runs for diagnostics.
- Build CI should trigger only for code/build-system changes, not routine context/documentation edits.

- Never use `actions/upload-artifact` or retain intermediate publish/installer files. Runner-local build output is ephemeral and must disappear with the job.
- Installer identity (`AppId`) is fixed across releases so every newer installer updates the existing per-user installation instead of creating a parallel installation.
- Application session data under `%LOCALAPPDATA%\Baraban` is outside the installation directory and must survive normal application updates.

- Normal upgrades must be delivered as one self-contained cumulative delta EXE that can update every supported installed Project Alpha version from v0.3.0 onward directly to the target version.
- Delta packages must validate the exact installed base by SHA-256, back up changed/deleted files, roll back on failure, verify the result, and restart the application.
- User-editable data must not live in the installation directory. Editable drum JSON belongs under `%LOCALAPPDATA%\Baraban\Drums`.
- Each release may retain a full Setup only as first-install/recovery fallback. The user-facing normal update is exactly one `ProjectAlpha-Update-to-vX.Y.Z.exe`.
- No ZIP/CMD/PS1 or standalone manifest is published for normal updates. Internal manifest/payload/updater files are embedded inside the single update EXE and exist only temporarily on the CI runner.
- Update EXE must be smoke-tested against a reconstructed copy of the previous published version before it is published.

- Normal uninstall must remove the installed application under `%LOCALAPPDATA%\Programs\Project Alpha` while preserving `%LOCALAPPDATA%\Baraban` by default.
- Interactive uninstall must ask exactly: `Удалить также настройки и рабочие данные?`
- Only an explicit Yes may remove `%LOCALAPPDATA%\Baraban`.
- Silent/service uninstall must preserve user data.
- Incremental Update EXE must share the fixed application AppId so the latest uninstall code is appended to the same Inno uninstall log rather than creating a second installed application.
- CI must reconstruct every supported historical publish from Git history and smoke-test cumulative delta application from each base before publishing the Update EXE.
- Update failures must expose a meaningful reason to the user; a bare numeric exit code is insufficient.

## Правила общения с владельцем и оценки решений
- Любое предложение, идея или техническое решение владельца рассматривается как гипотеза, а не как заведомо правильное указание по реализации.
- Менеджер обязан критически оценивать предложения владельца по целям проекта, проверенным данным, ограничениям платформы, рискам, стоимости и наличию лучших вариантов.
- Если предложение технически плохое, избыточное, противоречит цели, создаёт лишний риск или хуже доступной альтернативы, менеджер обязан сказать об этом прямо и объяснить причины.
- Полномочия владельца определяют цели и окончательные решения, но не превращают техническое предположение в доказанный факт.
- Оценки результатов должны быть консервативными. Нельзя приукрашивать неопределённость, повышать степень доказанности или использовать оптимистичную трактовку ради успокоения владельца.
- Доказанным считается только то, что подтверждено наблюдаемыми авторитетными данными. При существенной неопределённости использовать формулировки «не доказано», «неопределённо», «заблокировано» или «ошибка» по фактическому состоянию.
- Промежуточный успешный результат не означает успех всей архитектуры. Отсутствие наблюдаемой ошибки не является доказательством работоспособности.
- Существенные риски, отрицательные результаты, неизвестные факторы и обнаруженные ошибки сообщать владельцу сразу.
- Все объяснения владельцу давать на русском языке.
- Не использовать английские слова и англицизмы, если существует понятный русский эквивалент.
- Устоявшийся английский технический термин допускается только вместе с русским переводом и кратким объяснением смысла.
- Сокращения и специальные обозначения при первом употреблении расшифровывать по-русски, если их смысл не очевиден из контекста.
- Приоритет — понятное русское объяснение сути, а не профессиональный жаргон или калька с английского.

