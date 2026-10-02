# Manager intentions and commitments

## Completed
- Analyze the owner-provided 02.10.2026 Alfa Online capture, identify the distinct `v1/loyalty-view` wheel mechanism, generalize the three UI actions to per-drum pipelines, add the `alfa-online-supercashback-wheel-2026-10` module, and release the finalized support as v0.3.10.
- Analyze the owner-supplied Alfa Online Network Recorder capture and implement multi-mechanism drum support through generic per-module action pipelines; add the `alfa-online-supercashback-wheel-2026-10` module and release the implementation as v0.3.10.
- Harden multi-domain session handling with host-scoped reusable headers and keep dynamic security headers request-specific.
- Correct cumulative-update validation for mutable drum definitions: preserve existing user edits while guaranteeing newly introduced built-in modules are seeded.
- Redesign and release v0.3.8 after owner review of v0.3.7: constrain all panes to the main window, move scrolling into data controls, add draggable pane splitters, decode Unicode JSON, add Ctrl+F search, replace the HTTP dropdown with the three-button prize/state/claim workflow, and make prize projection adaptive so missing offerId no longer empties the Result table.
- Redesign the UI and release v0.3.7: scrollable panes, pretty JSON, workflow button on Result, visible log removal, explicit result empty state, smaller raw HTTP response and structured JSON response table.
- Add automatic application version to the main window title and release it as v0.3.6; publish a complete Russian user guide for all current controls.
- Verify `ProjectAlpha-Update-to-v0.3.5.exe` on the owner's real installation that had failed with v0.3.4; owner confirmed successful update on 2026-10-02.
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
- Obtain owner-side confirmation of v0.3.10 on the real installation, including the new Alfa Online prize list/state flow and, only if intentionally requested by the owner, the explicit claim/spin action.
- Live-verify recorder ZIP session restoration on Windows.
- Live-verify the current Alfa-Friday non-mutating request chain from Project Alpha.
- Preserve multi-drum modularity and explicit confirmation semantics in all future changes.
