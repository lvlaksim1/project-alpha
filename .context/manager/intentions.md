# Manager intentions and commitments

## Completed
- Create public `lvlaksim1/project-alpha` and install the Project Manager capsule.
- Migrate the current application into the new neutral repository.
- Keep the old Alfa module as an archive.
- Implement the current Alfa-Friday/Tasty Coffee module from captured evidence.
- Implement request-specific session/header profiles and full recorder ZIP import for cookies + local/session storage.
- Separate editable/user state from the installation directory.
- Establish full Windows Setup with fixed AppId.
- Establish one-file incremental updates.
- Replace strict previous-version updater with cumulative multi-base updater after the owner's real v0.3.3 failure.
- Verify cumulative v0.3.4 updates from v0.3.0-v0.3.3 in CI.
- Diagnose the owner-observed v0.3.4 updater failure, reproduce the Windows PowerShell 5.1 encoding/parser defect in CI, and release the corrected v0.3.5 updater with repair fallback and exact manifest persistence.
- Implement uninstall user-data choice and verify silent preservation.
- Enforce minimal GitHub storage / no Actions-artifact policy.

## Active
- Obtain owner-side confirmation that `ProjectAlpha-Update-to-v0.3.5.exe` succeeds on the real installation where v0.3.4 failed with exit code 1.
- Live-verify recorder ZIP session restoration on Windows.
- Live-verify the current Alfa-Friday non-mutating request chain from Project Alpha.
- Preserve multi-drum modularity and explicit confirmation semantics in all future changes.
