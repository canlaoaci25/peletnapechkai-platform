"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";

import type { AdminCopy } from "@/i18n/admin-copy";
import type { ArticleDetail } from "@/lib/admin-api";
import type { ArticleCollaboration } from "@/lib/admin-api";
import type { Locale } from "@/i18n/config";

type Action = "submit" | "editorial-approve" | "return-to-draft" | "publish" | "archive";

export function WorkflowActions({ article, roles, copy, checklist, locale }: { article: ArticleDetail; roles: string[]; copy: AdminCopy; checklist: ArticleCollaboration["checklist"]; locale: Locale }) {
  const router = useRouter();
  const [pending, setPending] = useState(false);
  const [message, setMessage] = useState("");
  const canWrite = roles.some((role) => ["Owner", "Admin", "Editor", "Author"].includes(role));
  const canManageEditorial = roles.some((role) => ["Owner", "Admin", "Editor"].includes(role));
  const canManageSeo = roles.some((role) => ["Owner", "Admin", "Editor", "SEO"].includes(role));
  const gateCopy = {
    "tr-TR": { title: "Yayın kalite kapısı", ready: "8/8 kontrol tamamlandı", blocked: "Yayınlama ve planlama için tüm kontrolleri tamamlayın", progress: "kontrol tamamlandı" },
    "en-US": { title: "Publication quality gate", ready: "8/8 checks complete", blocked: "Complete every check before publishing or scheduling", progress: "checks complete" },
    "de-DE": { title: "Qualitätsschranke", ready: "8/8 Prüfungen abgeschlossen", blocked: "Vor Planung oder Veröffentlichung alle Prüfungen abschließen", progress: "Prüfungen abgeschlossen" },
    "fr-FR": { title: "Contrôle qualité", ready: "8/8 contrôles terminés", blocked: "Terminez tous les contrôles avant de planifier ou publier", progress: "contrôles terminés" },
  }[locale];
  const completed = checklist ? Object.entries(checklist).filter(([key, value]) => key !== "isComplete" && value).length : 0;
  const qualityReady = checklist?.isComplete === true;

  async function transition(action: Action | "schedule", payload?: object) {
    setPending(true);
    setMessage("");
    try {
      const csrfResponse = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
      if (!csrfResponse.ok) throw new Error(copy.workflowError);
      const { token } = (await csrfResponse.json()) as { token: string };
      const response = await fetch(`/api/admin/articles/${article.id}/${action}`, {
        method: "POST",
        headers: { "content-type": "application/json", "x-csrf-token": token },
        body: payload ? JSON.stringify(payload) : undefined,
      });
      if (!response.ok) {
        const problem = (await response.json().catch(() => null)) as { message?: string } | null;
        throw new Error(problem?.message ?? copy.workflowError);
      }
      setMessage(copy.workflowSuccess);
      router.refresh();
    } catch (error) {
      setMessage(error instanceof Error ? error.message : copy.workflowError);
    } finally {
      setPending(false);
    }
  }

  function schedule(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const value = new FormData(event.currentTarget).get("scheduledAt");
    if (typeof value !== "string" || !value) return;
    void transition("schedule", { scheduledAt: new Date(value).toISOString() });
  }

  return (
    <section className="workflow-panel" aria-labelledby="workflow-title">
      <div><p className="section-kicker">{copy.status}</p><h2 id="workflow-title">{copy[`status${article.status}` as keyof AdminCopy] ?? article.status}</h2></div>
      <div className={`publication-readiness ${qualityReady ? "is-ready" : "is-blocked"}`} role="status"><span>{gateCopy.title}</span><strong>{qualityReady ? gateCopy.ready : `${completed}/8 ${gateCopy.progress}`}</strong><small>{qualityReady ? copy.publish : gateCopy.blocked}</small><div aria-hidden="true"><i style={{ width: `${completed * 12.5}%` }} /></div></div>
      <div className="workflow-actions">
        {article.status === "Draft" && canWrite && <button disabled={pending} onClick={() => void transition("submit")}>{copy.submitReview}</button>}
        {article.status === "InEditorialReview" && canManageEditorial && <button disabled={pending} onClick={() => void transition("editorial-approve")}>{copy.approveEditorial}</button>}
        {["InEditorialReview", "InSeoReview"].includes(article.status) && canManageEditorial && <button className="button-secondary" disabled={pending} onClick={() => void transition("return-to-draft")}>{copy.returnDraft}</button>}
        {article.status === "InSeoReview" && canManageSeo && <form className="schedule-form" onSubmit={schedule}><label>{copy.scheduleAt}<input name="scheduledAt" type="datetime-local" required /></label><button disabled={pending || !qualityReady}>{copy.schedule}</button></form>}
        {["InSeoReview", "Scheduled"].includes(article.status) && canManageSeo && <button disabled={pending || !qualityReady} onClick={() => void transition("publish")}>{copy.publish}</button>}
        {article.status === "Published" && canManageEditorial && <button className="button-secondary" disabled={pending} onClick={() => void transition("archive")}>{copy.archive}</button>}
      </div>
      {message && <p className="form-message" role="status">{message}</p>}
    </section>
  );
}
