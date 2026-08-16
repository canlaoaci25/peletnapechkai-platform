"use client";

import Link from "next/link";
import { FormEvent, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import Swal from "sweetalert2";
import type { LocaleCatalogItem, ManagedLocale } from "@/lib/admin-api";

async function mutate(path: string, method: "POST" | "PUT", body: object) {
  const csrf = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
  const { token } = (await csrf.json()) as { token: string };
  const response = await fetch(`/api/admin/locales${path}`, {
    method,
    headers: { "content-type": "application/json", "x-csrf-token": token },
    body: JSON.stringify(body),
  });
  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as {
      message?: string;
    } | null;
    throw new Error(problem?.message ?? "İşlem tamamlanamadı.");
  }
}

export function LanguageList({
  locale,
  locales,
}: {
  locale: string;
  locales: ManagedLocale[];
}) {
  const enabled = locales.filter((item) => item.isEnabled);
  const missing = enabled.reduce((total, item) => total + item.missingTranslationCount, 0);
  const pending = enabled.reduce((total, item) => total + item.reviewPendingCount, 0);
  const stale = enabled.reduce((total, item) => total + item.staleTranslationCount, 0);
  const missingCategories = enabled.reduce((total, item) => total + item.missingCategoryCount, 0);
  const translated = enabled.filter((item) => !item.isDefault);
  const coverage = translated.length === 0 ? 100 : Math.round(translated.reduce((total, item) => total + (item.sourcePublishedCount ? item.publishedCount / item.sourcePublishedCount * 100 : 100), 0) / translated.length);
  return (
    <section className="language-health-dashboard">
      <div className="language-health-summary" aria-label="Çeviri sağlığı özeti">
        <article><small>Etkin yayınlar</small><strong>{enabled.length}</strong><span>dil-bölge</span></article>
        <article><small>Ortalama kapsam</small><strong>%{coverage}</strong><span>Türkçe kaynak arşive göre</span></article>
        <article className={missing ? "needs-attention" : ""}><small>Eksik çeviri</small><strong>{missing}</strong><span>henüz oluşturulmamış</span></article>
        <article className={pending ? "needs-attention" : ""}><small>Editör incelemesi</small><strong>{pending}</strong><span>yayın öncesi kontrol</span></article>
        <article className={stale ? "needs-attention" : ""}><small>Güncellik farkı</small><strong>{stale}</strong><span>kaynak yazıdan geride</span></article>
        <article className={missingCategories ? "needs-attention" : ""}><small>Eksik kategori</small><strong>{missingCategories}</strong><span>yerelleştirilmemiş konu yolu</span></article>
      </div>
      <div className="language-list-page">
      {locales.map((item) => (
        <Link
          className="admin-panel language-list-card"
          href={`/${locale}/admin/languages/${item.id}`}
          key={item.id}
        >
          <span>
            <strong>{item.nativeName}</strong>
            <small>
              {item.displayName} · {item.code}
            </small>
          </span>
          <span>
            <span
              className={
                item.isEnabled ? "language-state enabled" : "language-state"
              }
            >
              {item.isEnabled ? "Aktif" : "Pasif"}
            </span>
            <small>{item.publishedCount} yayında · {item.draftCount} taslak · {item.countries.length} ülke</small>
          </span>
          <span className="language-coverage" aria-label={`${item.nativeName} yayın kapsamı`}>
            <b>%{item.isDefault || !item.sourcePublishedCount ? 100 : Math.min(100, Math.round(item.publishedCount / item.sourcePublishedCount * 100))}</b>
            <i><span style={{width:`${item.isDefault || !item.sourcePublishedCount ? 100 : Math.min(100, item.publishedCount / item.sourcePublishedCount * 100)}%`}} /></i>
            <small>{item.isDefault ? `${item.sourceCategoryCount} kaynak kategori` : `${item.missingTranslationCount} eksik · ${item.staleTranslationCount} güncel değil`}</small>
            {!item.isDefault && <small className={item.missingCategoryCount ? "taxonomy-debt" : ""}>{item.linkedCategoryCount}/{item.sourceCategoryCount} kategori bağlı · {item.reviewPendingCount} incelemede</small>}
          </span>
          <b aria-hidden>→</b>
        </Link>
      ))}
      </div>
    </section>
  );
}

export function LanguageCreateForm({
  catalog,
  existingCodes,
  locale,
}: {
  catalog: LocaleCatalogItem[];
  existingCodes: string[];
  locale: string;
}) {
  const router = useRouter(),
    [query, setQuery] = useState(""),
    [selected, setSelected] = useState(""),
    [pending, setPending] = useState(false);
  const available = useMemo(
    () => catalog.filter((item) => !existingCodes.includes(item.code)),
    [catalog, existingCodes],
  );
  const filtered = useMemo(() => {
    const value = query.trim().toLocaleLowerCase("tr-TR");
    return available
      .filter(
        (item) =>
          !value ||
          `${item.displayName} ${item.nativeName} ${item.code} ${item.countryName}`
            .toLocaleLowerCase("tr-TR")
            .includes(value),
      )
      .slice(0, 80);
  }, [available, query]);
  async function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selected) return;
    setPending(true);
    try {
      await mutate("/", "POST", {
        code: selected,
        displayName: null,
        nativeName: null,
      });
      await Swal.fire({
        title: "Dil eklendi",
        text: "İlgili ülkeler otomatik bağlandı. Dil, çeviriler hazırlanana kadar pasif tutuldu.",
        icon: "success",
        background: "#151922",
        color: "#f4f6fa",
      });
      router.push(`/${locale}/admin/languages`);
      router.refresh();
    } catch (error) {
      await Swal.fire({
        title: "Dil eklenemedi",
        text: error instanceof Error ? error.message : "İşlem tamamlanamadı.",
        icon: "error",
        background: "#151922",
        color: "#f4f6fa",
      });
    } finally {
      setPending(false);
    }
  }
  const choice = available.find((item) => item.code === selected);
  return (
    <form className="admin-panel admin-form language-picker" onSubmit={create}>
      <label>
        Dil veya ülke ara
        <input
          type="search"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder="Fransızca, France veya fr-FR…"
          autoFocus
        />
      </label>
      <label>
        Dil-bölge seç
        <select
          value={selected}
          onChange={(event) => setSelected(event.target.value)}
          required
          size={Math.min(12, Math.max(5, filtered.length))}
        >
          <option value="" disabled>
            Bir dil seçin
          </option>
          {filtered.map((item) => (
            <option value={item.code} key={item.code}>
              {item.nativeName} — {item.displayName} [{item.code}]
            </option>
          ))}
        </select>
      </label>
      {choice && (
        <aside className="language-choice">
          <strong>{choice.nativeName}</strong>
          <span>{choice.displayName}</span>
          <small>
            Ana ülke: {choice.countryName} ({choice.countryCode})
          </small>
        </aside>
      )}
      <button disabled={pending || !selected}>
        {pending ? "Ekleniyor…" : "Seçili dili ekle"}
      </button>
    </form>
  );
}

export function LanguageEditForm({ locale }: { locale: ManagedLocale }) {
  const router = useRouter(),
    [pending, setPending] = useState(false);
  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPending(true);
    const data = new FormData(event.currentTarget);
    try {
      await mutate(`/${locale.id}`, "PUT", {
        displayName: data.get("displayName"),
        nativeName: data.get("nativeName"),
        isEnabled: data.get("isEnabled") === "on",
      });
      await Swal.fire({
        title: "Dil ayarları kaydedildi",
        icon: "success",
        timer: 1000,
        showConfirmButton: false,
        background: "#151922",
        color: "#f4f6fa",
      });
      router.refresh();
    } catch (error) {
      await Swal.fire({
        title: "Kaydedilemedi",
        text: error instanceof Error ? error.message : "İşlem tamamlanamadı.",
        icon: "error",
        background: "#151922",
        color: "#f4f6fa",
      });
    } finally {
      setPending(false);
    }
  }
  async function country(code: string, isEnabled: boolean) {
    setPending(true);
    try {
      await mutate(`/${locale.id}/countries/${code}`, "PUT", { isEnabled });
      router.refresh();
    } finally {
      setPending(false);
    }
  }
  return (
    <div className="language-edit-layout">
      <form className="admin-panel admin-form" onSubmit={save}>
        <header className="language-edit-header">
          <div>
            <p className="section-kicker">{locale.code}</p>
            <h2>{locale.nativeName}</h2>
            <small>{locale.articleCount} içerik</small>
          </div>
          <span
            className={
              locale.isEnabled ? "language-state enabled" : "language-state"
            }
          >
            {locale.isEnabled ? "Aktif" : "Pasif"}
          </span>
        </header>
        <div className="form-grid">
          <label>
            Yönetim adı
            <input
              name="displayName"
              defaultValue={locale.displayName}
              required
            />
          </label>
          <label>
            Yerel adı
            <input
              name="nativeName"
              defaultValue={locale.nativeName}
              required
            />
          </label>
        </div>
        <label className="check-label">
          <input
            name="isEnabled"
            type="checkbox"
            defaultChecked={locale.isEnabled}
            disabled={locale.isDefault}
          />
          {locale.isDefault
            ? "Varsayılan dil daima aktif"
            : "Site dilini etkinleştir"}
        </label>
        <button disabled={pending}>Dil ayarlarını kaydet</button>
      </form>
      <section className="admin-panel language-countries">
        <header>
          <p className="section-kicker">ÜLKE EŞLEŞMELERİ</p>
          <h2>Otomatik seçilen ülkeler</h2>
          <p className="muted">
            Korunan eşleşmeler silinmez; gerektiğinde pasife alınabilir.
          </p>
        </header>
        {locale.countries.map((item) => (
          <label key={item.code}>
            <span>
              <strong>{item.name}</strong>
              <small>
                {item.code} · {item.currencyCode}
                {item.isPrimary ? " · Ana ülke" : ""}
              </small>
            </span>
            <input
              type="checkbox"
              checked={item.isEnabled}
              disabled={pending}
              onChange={(event) =>
                void country(item.code, event.target.checked)
              }
            />
          </label>
        ))}
      </section>
    </div>
  );
}
