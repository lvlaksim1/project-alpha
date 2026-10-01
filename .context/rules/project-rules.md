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

- Normal upgrades must be delivered as one self-contained delta EXE built against the immediately previous retained release.
- Delta packages must validate the exact installed base by SHA-256, back up changed/deleted files, roll back on failure, verify the result, and restart the application.
- User-editable data must not live in the installation directory. Editable drum JSON belongs under `%LOCALAPPDATA%\Baraban\Drums`.
- Each release may retain a full Setup only as first-install/recovery fallback. The user-facing normal update is exactly one matching `ProjectAlpha-Update-from-vA-to-vB.exe`.
- No ZIP/CMD/PS1 or standalone manifest is published for normal updates. Internal manifest/payload/updater files are embedded inside the single update EXE and exist only temporarily on the CI runner.
- Update EXE must be smoke-tested against a reconstructed copy of the previous published version before it is published.

- Normal uninstall must remove the installed application under `%LOCALAPPDATA%\Programs\Project Alpha` while preserving `%LOCALAPPDATA%\Baraban` by default.
- Interactive uninstall must ask exactly: `Удалить также настройки и рабочие данные?`
- Only an explicit Yes may remove `%LOCALAPPDATA%\Baraban`.
- Silent/service uninstall must preserve user data.
- Incremental Update EXE must share the fixed application AppId so the latest uninstall code is appended to the same Inno uninstall log rather than creating a second installed application.
