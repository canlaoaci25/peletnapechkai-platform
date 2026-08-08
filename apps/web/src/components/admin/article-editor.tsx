"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { RichTextEditor } from "@/components/admin/rich-text-editor";
import { commercialCopy } from "@/i18n/commercial-copy";
import type { AdminCopy } from "@/i18n/admin-copy";
import type { ArticleDetail } from "@/lib/admin-api";

type Category = { id: string; locale: string; name: string };

function slugify(value: string) {
  return value
    .toLocaleLowerCase("tr-TR")
    .replaceAll("ı", "i")
    .replaceAll("ğ", "g")
    .replaceAll("ü", "u")
    .replaceAll("ş", "s")
    .replaceAll("ö", "o")
    .replaceAll("ç", "c")
    .normalize("NFKD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 160);
}

export function ArticleEditor({
  copy,
  article,
  categories = [],
}: {
  copy: AdminCopy;
  article?: ArticleDetail;
  categories?: Category[];
}) {
  const router = useRouter();
  const [message, setMessage] = useState("");
  const [pending, setPending] = useState(false);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [focusMode, setFocusMode] = useState(false);
  const [words, setWords] = useState(0);
  const [title, setTitle] = useState(article?.title ?? "");
  const [slug, setSlug] = useState(article?.slug ?? "");
  const [slugEdited, setSlugEdited] = useState(Boolean(article));
  const commercial =
    commercialCopy[(article?.locale ?? "tr-TR") as keyof typeof commercialCopy];
  const locale = article?.locale ?? "tr-TR";

  useEffect(() => {
    function close(event: KeyboardEvent) {
      if (event.key !== "Escape") return;
      setSettingsOpen(false);
      setFocusMode(false);
    }
    window.addEventListener("keydown", close);
    return () => window.removeEventListener("keydown", close);
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPending(true);
    setMessage("");
    const data = new FormData(event.currentTarget);
    try {
      const csrf = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
      const { token } = (await csrf.json()) as { token: string };
      const payload: Record<string, unknown> = Object.fromEntries(
        data.entries(),
      );
      payload.categoryIds = data.getAll("categoryIds");
      payload.isSponsored = data.get("isSponsored") === "on";
      payload.hasAffiliateLinks = data.get("hasAffiliateLinks") === "on";
      if (article) payload.expectedUpdatedAt = article.updatedAt;
      const response = await fetch(
        article ? `/api/admin/articles/${article.id}` : "/api/admin/articles/",
        {
          method: article ? "PUT" : "POST",
          headers: {
            "content-type": "application/json",
            "x-csrf-token": token,
          },
          body: JSON.stringify(payload),
        },
      );
      if (!response.ok) throw new Error();
      const result = (await response.json()) as { id?: string };
      if (!article && result.id) {
        router.push(`/${String(payload.locale)}/admin/articles/${result.id}`);
        return;
      }
      setMessage(copy.saved);
      router.refresh();
    } catch {
      setMessage(copy.saveError);
    } finally {
      setPending(false);
    }
  }

  return (
    <form
      className={`editor-form advanced-editor-layout${focusMode ? " editor-focus-mode" : ""}`}
      onSubmit={submit}
    >
      <header className="editor-action-bar">
        <div>
          <strong>{article ? "İçeriği düzenle" : "Yeni Türkçe içerik"}</strong>
          <span>
            {words} kelime · yaklaşık {Math.max(1, Math.ceil(words / 200))}{" "}
            dakika okuma
          </span>
        </div>
        <nav aria-label="Editör işlemleri">
          <button
            type="button"
            className="editor-tool-button"
            onClick={() => setFocusMode((value) => !value)}
          >
            {focusMode ? "Tam ekrandan çık" : "Tam ekran"}
          </button>
          <button
            type="button"
            className="editor-tool-button"
            onClick={() => setSettingsOpen(true)}
          >
            Ayarlar
          </button>
          <button
            type="submit"
            className="editor-save-button"
            disabled={pending}
          >
            {pending ? copy.saving : article ? copy.update : copy.save}
          </button>
        </nav>
      </header>
      <div className="advanced-editor-main">
        <div className="form-grid">
          <label>
            {copy.locale}
            <select
              name="locale"
              defaultValue={locale}
              disabled={Boolean(article)}
            >
              <option>tr-TR</option>
            </select>
          </label>
          <label>
            {copy.type}
            <select
              name="type"
              defaultValue={article?.type ?? "News"}
              disabled={Boolean(article)}
            >
              <option value="News">{copy.news}</option>
              <option value="Guide">{copy.guide}</option>
              <option value="Review">{copy.review}</option>
              <option value="Analysis">{copy.analysis}</option>
            </select>
          </label>
        </div>
        <label>
          {copy.title}
          <input
            name="title"
            value={title}
            onChange={(event) => {
              const next = event.target.value;
              setTitle(next);
              if (!slugEdited) setSlug(slugify(next));
            }}
            required
            maxLength={220}
          />
        </label>
        <label>
          {copy.slug}
          <input
            name="slug"
            value={slug}
            onChange={(event) => {
              setSlugEdited(true);
              setSlug(event.target.value);
            }}
            required
            pattern="[a-z0-9]+(?:-[a-z0-9]+)*"
            placeholder="ornek-makale"
          />
        </label>
        <label>
          {copy.summary}
          <textarea name="summary" defaultValue={article?.summary} rows={3} />
        </label>
        <RichTextEditor
          name="body"
          label={copy.body}
          initialValue={article?.body}
          onMetrics={setWords}
        />
        {message && (
          <p className="form-message" role="status">
            {message}
          </p>
        )}
      </div>
      {settingsOpen && (
        <button
          className="editor-settings-backdrop"
          type="button"
          aria-label="Ayarları kapat"
          onClick={() => setSettingsOpen(false)}
        />
      )}
      <aside
        className={`advanced-editor-settings${settingsOpen ? " is-open" : ""}`}
        aria-label="İçerik ayarları"
        aria-hidden={!settingsOpen}
      >
        <header>
          <div>
            <span>YAYIN AYARLARI</span>
            <h2>İçerik ayarları</h2>
          </div>
          <button
            type="button"
            onClick={() => setSettingsOpen(false)}
            aria-label="Ayarları kapat"
          >
            ×
          </button>
        </header>
        <label>
          Kategori
          <select
            name="categoryIds"
            required
            defaultValue={article?.categoryIds?.[0] ?? ""}
          >
            <option value="" disabled>
              Kategori seçin
            </option>
            {categories
              .filter((item) => item.locale === locale)
              .map((item) => (
                <option key={item.id} value={item.id}>
                  {item.name}
                </option>
              ))}
          </select>
        </label>
        <label>
          {copy.seoTitle}
          <input
            name="seoTitle"
            defaultValue={article?.seoTitle ?? ""}
            maxLength={70}
          />
        </label>
        <label>
          {copy.seoDescription}
          <textarea
            name="seoDescription"
            defaultValue={article?.seoDescription ?? ""}
            rows={4}
            maxLength={170}
          />
        </label>
        <fieldset>
          <legend>{commercial.disclosure}</legend>
          <label className="check-label">
            <input
              name="isSponsored"
              type="checkbox"
              defaultChecked={article?.isSponsored}
            />
            {commercial.sponsored}
          </label>
          <label>
            {commercial.sponsorName}
            <input
              name="sponsorName"
              defaultValue={article?.sponsorName ?? ""}
              maxLength={200}
            />
          </label>
          <label className="check-label">
            <input
              name="hasAffiliateLinks"
              type="checkbox"
              defaultChecked={article?.hasAffiliateLinks}
            />
            {commercial.affiliate}
          </label>
        </fieldset>
      </aside>
    </form>
  );
}
