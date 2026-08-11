# Ready content automation

`AI Hazır > Hazır içerik oluştur` creates a durable `ReadyContentGeneration` job. An
Owner or Admin selects a Turkish category, article type, count (1–50), cover-image
mode, automatic translation, and automatic SEO.

The worker processes the same job through persisted phases: research and planning,
Turkish generation, optional covers, optional translations, optional locale SEO, and
final verification. The report screen polls every three seconds and displays the current
phase plus real remaining counts. Candidate counts are recalculated from
`generated_by_automation_job_id`, so a service restart does not lose the checkpoint.

Generation uses Codex live web search and a strict JSON schema. Every article must carry
two to eight absolute research-source URLs, a detailed Turkish body, and unique metadata.
The API sanitizes HTML and rejects invalid fields, duplicate slugs, duplicate source URLs,
and title/summary token similarity at or above 0.52 against recent BOECL content and the
same batch. Source URLs are stored as article-group relationships; researched prose must
be original rather than copied.

Validated Turkish articles are published directly. If selected, a branded 1200×675 WebP
cover is generated locally and attached through the existing media model. Translations
are restricted to content created by the same job and are published through the validated
translation endpoint. SEO can update only published localizations linked to that job.
Completion is rejected while any requested article, translation, or SEO candidate remains.

All mutations create audit records. Raw model output never receives database access, and
the worker remains restricted to the loopback token-protected candidate/delivery APIs.
