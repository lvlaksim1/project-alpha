# Manager intentions and commitments

## Completed
- Analyze the 13:25 Network Recorder capture, add capture-grounded Подружка and М.ВИДЕО one-shot Alfa Online modules, generalize terminal action handling so `endActionButton` disables repeated mutation, and release v0.3.15.
- Add one-shot manual sending of edited requests from the advanced request editor, keep persistence separate, retain confirmation for potentially mutating requests, fix repair-mode seed-if-missing delivery of mutable modules, and release v0.3.14.
- Analyze the 06:11 paid-repeat capture, verify the real POST /loyalty-view/offer payment request and post-payment GET/PUT sequence, implement explicit paid reroll support without hard-coding account-specific order data, and release v0.3.13.
- Analyze the full 05:45 Alfa Online capture with two real spins and the free reroll; replace the incorrect `isWinner` assumption with authoritative `winnerOffer.id`, implement the observed `Крутить скорее! → Крутить ещё → Крутить скорее!` state machine, and release it as v0.3.12.
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
- Obtain owner-side confirmation of v0.3.15 on the real installation: both new modules are seeded, prize lists render correctly, winner IDs map to names, and terminal **Отлично** disables a second spin.
- Verify the v0.3.14 edited-request **Отправить** workflow on the owner's machine.
- Keep paid-repeat testing optional because it causes a real debit; never trigger it automatically.
- Live-verify recorder ZIP session restoration and preserve all existing Alfa-Friday / Alfa Online mechanisms.
