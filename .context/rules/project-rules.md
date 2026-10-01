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
