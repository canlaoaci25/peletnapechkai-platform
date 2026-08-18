# BOECL owner decisions

These decisions override generic specialist preferences and must be read before every autonomous cycle.

- Manual source content is created in Turkish (`tr-TR`).
- A translation can only be produced from a published Turkish source. A foreign translation that passes structured API validation, sanitization, uniqueness, SEO and audit gates is published directly; it is not silently changed to a mandatory manual draft workflow.
- Generated cover and body images contain no embedded writing, title, category or brand text.
- Public navigation uses a desktop sidebar and an accessible, closed-by-default mobile drawer.
- Until `githubPushPausedUntil` in autonomous state, do not request GitHub authorization or push. Local commits and validated staging/production delivery continue.
- Continuity is a release invariant: autonomous work happens in an isolated Git worktree. A failed cycle preserves its recovery branch/worktree and must not dirty or block `main`.
- The autonomous team improves its own orchestration, tests, recovery, role coordination and observability as well as the product. Self-improvements must be tested, committed, auditable and reversible; they may not silently remove quality, secret-handling, backup or deployment safeguards.
- A generic agent recommendation may not reverse these decisions. A conflict is reported as an owner decision request instead of being implemented implicitly.
