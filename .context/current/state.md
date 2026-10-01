# Current state

Updated: 2026-10-01 18:50 MSK

- Public repository `lvlaksim1/project-alpha` is the authoritative repository.
- Project Manager capsule is installed and populated with the current project state.
- Current application source has been migrated into this repository.
- Current working drum: `alfa-friday-tasty-coffee-2026-09-30`.
- Previous `alfa-loyalty-roulette` module is retained as an archive; automatic workflow execution is disabled for archived drums.
- Session layer supports request-specific header profiles and DPAPI-protected local persistence.
- No live cookies/tokens/security-header values are stored in the public repository.
- Repository storage hygiene is enforced:
  - no Actions build artifacts;
  - no build cache by default;
  - binary/archive/session/capture outputs ignored by Git;
  - only the latest GitHub Release is retained;
  - only the eight most recent completed workflow runs are retained;
  - build CI does not run for context/documentation-only changes.
- At policy introduction, Git contained no large blobs; the largest tracked source file was about 12 KB. The only large object was the current release ZIP (~79 MB), which is intentionally retained as the current downloadable build.
