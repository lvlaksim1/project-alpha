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

- Windows distribution is installer-only: the release workflow must build `ProjectAlpha-Setup-vX.Y.Z-win-x64.exe` and upload it directly to GitHub Release.
- Never use `actions/upload-artifact` or retain intermediate publish/installer files. Runner-local build output is ephemeral and must disappear with the job.
- Installer identity (`AppId`) is fixed across releases so every newer installer updates the existing per-user installation instead of creating a parallel installation.
- Application session data under `%LOCALAPPDATA%\Baraban` is outside the installation directory and must survive normal application updates.
