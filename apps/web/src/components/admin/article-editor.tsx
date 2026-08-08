"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import type { AdminCopy } from "@/i18n/admin-copy";
import type { ArticleDetail } from "@/lib/admin-api";
import {commercialCopy}from"@/i18n/commercial-copy";

export function ArticleEditor({ copy, article }: { copy: AdminCopy; article?: ArticleDetail }) {
  const router = useRouter();
  const [message, setMessage] = useState("");
  const [pending, setPending] = useState(false);
  const commercial=commercialCopy[(article?.locale??"tr-TR") as keyof typeof commercialCopy];

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPending(true);
    setMessage("");
    const form = event.currentTarget;
    const data = new FormData(form);
    try {
      const csrf = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
      const { token } = (await csrf.json()) as { token: string };
      const payload:Record<string,unknown> = Object.fromEntries(data.entries());
      payload.isSponsored=data.get("isSponsored")==="on";payload.hasAffiliateLinks=data.get("hasAffiliateLinks")==="on";
      if (article) payload.expectedUpdatedAt = article.updatedAt;
      const response = await fetch(article ? `/api/admin/articles/${article.id}` : "/api/admin/articles/", {
        method: article ? "PUT" : "POST",
        headers: { "content-type": "application/json", "x-csrf-token": token },
        body: JSON.stringify(payload),
      });
      if (!response.ok) throw new Error();
      const result = (await response.json()) as { id?: string };
      if (!article && result.id) {
        router.push(`/${String(payload.locale)}/admin/articles/${result.id}`);
        return;
      }
      form.reset();
      setMessage(copy.saved);
      router.refresh();
    } catch {
      setMessage(copy.saveError);
    } finally {
      setPending(false);
    }
  }

  return (
    <form className="admin-form editor-form" onSubmit={submit}>
      <div className="form-grid">
        <label>{copy.locale}<select name="locale" defaultValue={article?.locale ?? "tr-TR"} disabled={Boolean(article)}><option>tr-TR</option><option>en-US</option><option>de-DE</option></select></label>
        <label>{copy.type}<select name="type" defaultValue={article?.type ?? "News"} disabled={Boolean(article)}><option value="News">{copy.news}</option><option value="Guide">{copy.guide}</option><option value="Review">{copy.review}</option><option value="Analysis">{copy.analysis}</option></select></label>
      </div>
      <label>{copy.title}<input name="title" defaultValue={article?.title} required maxLength={220} /></label>
      <label>{copy.slug}<input name="slug" defaultValue={article?.slug} required pattern="[a-z0-9]+(?:-[a-z0-9]+)*" placeholder="example-article" /></label>
      <label>{copy.summary}<textarea name="summary" defaultValue={article?.summary} rows={3} /></label>
      <label>{copy.body}<textarea name="body" defaultValue={article?.body} rows={12} /></label>
      <div className="form-grid">
        <label>{copy.seoTitle}<input name="seoTitle" defaultValue={article?.seoTitle ?? ""} maxLength={70} /></label>
        <label>{copy.seoDescription}<textarea name="seoDescription" defaultValue={article?.seoDescription ?? ""} rows={2} maxLength={170} /></label>
      </div>
      <fieldset><legend>{commercial.disclosure}</legend><label className="check-label"><input name="isSponsored" type="checkbox" defaultChecked={article?.isSponsored}/>{commercial.sponsored}</label><label>{commercial.sponsorName}<input name="sponsorName" defaultValue={article?.sponsorName??""} maxLength={200}/></label><label className="check-label"><input name="hasAffiliateLinks" type="checkbox" defaultChecked={article?.hasAffiliateLinks}/>{commercial.affiliate}</label></fieldset>
      {message && <p className="form-message" role="status">{message}</p>}
      <button type="submit" disabled={pending}>{pending ? copy.saving : article ? copy.update : copy.save}</button>
    </form>
  );
}
