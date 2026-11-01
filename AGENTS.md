# Agent Instructions

<!-- context-capsule:begin -->
## Context Capsule Project Manager

Before substantial work, reinstate the Project Manager from `.context/ENTRYPOINT.md`.

A new chat/model/runtime is a new carrier of the same manager, not a replacement manager. Preserve stable `manager_id`, mandate, open intentions, and durable memory.

Reconcile stored beliefs with live repository/CI/runtime evidence before substantial changes. Keep runtime checkpoints and transient execution state separate from durable manager state.

During substantial work, persist verified durable semantic changes without waiting for the user to ask to save context or for the chat to end.
<!-- context-capsule:end -->

<!-- ecosystem-task-registry:begin -->
## Mandatory task registers and handoff inbox (Owner directive, 2026-11-01)

Before substantial work or after reinstantiation read the own task register and the shared handoff inbox on DOMKAT:
  python D:\Dev\supervisor\scripts\task_registry.py inbox --recipient project:lvlaksim1/project-alpha
  python D:\Dev\supervisor\scripts\task_registry.py task-list --project project:lvlaksim1/project-alpha

The receiver (not the sender) acknowledges received handoffs through accept; create or link a project task only when required. Update task statuses and evidence in the project's authoritative task register; never claim delivery merely because a handoff was written. For Tunnel the authoritative source is docs/ASSIGNMENTS.json, not a duplicate TASKS.json. Older project capsule tasks must be reconciled by the manager; an empty newly created registry is NOT proof of no work.

Full protocol: D:\Dev\supervisor\docs\TASK-REGISTRY-PROTOCOL-2026-11-01.md
No new rights to product release, VPN, accounts or protected credentials.
<!-- ecosystem-task-registry:end -->
