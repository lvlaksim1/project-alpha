# Procedural memory

Record learned procedures, reliable workflows, verification strategies, and operational techniques that should improve future manager performance.

## Windows updater compatibility and release verification

- Treat Windows PowerShell 5.1 as the actual execution environment for embedded `.ps1` updater code, not merely `pwsh` used by modern CI steps.
- A UTF-8-without-BOM PowerShell script containing non-ASCII text can be misdecoded by Windows PowerShell 5.1 and may fail at parse time before runtime error handling executes. Keep embedded updater scripts ASCII-safe or deliberately package them with an encoding Windows PowerShell 5.1 recognizes.
- Every release must parse `Apply-Update.ps1` under both PowerShell 7 and Windows PowerShell 5.1 before packaging.
- Preserve exact SHA-256 publish manifests in Git for released versions. Future deltas should compare against those exact manifests rather than relying on a later reconstruction of an old self-contained publish.
- Test two updater paths before release: exact historical-base update and a deliberately mixed legacy installation that forces repair mode. Repair mode must back up touched files, apply only its bounded payload, verify the complete immutable target manifest, and roll back on any mismatch.
- Compile and execute the final single-file Inno Update EXE in CI against a real previous Setup; script-level tests alone are insufficient.
