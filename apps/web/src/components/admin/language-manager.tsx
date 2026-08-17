"use client";

import Link from "next/link";
import { FormEvent, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import Swal from "sweetalert2";
import type { LocaleCatalogItem, ManagedLocale, LocalizationWork } from "@/lib/admin-api";
import type { Locale } from "@/i18n/config";

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

const workCopy={
  "tr-TR":{kicker:"YERELLEŞTİRME OPERASYONU",title:"Kaynak farkı ve SLA kuyruğu",description:"Her çevirinin dayandığı kaynak sürümünü ve değişen alanları görün; işi sorumlu ve son tarihle güvenceye alın.",owner:"Sorumlu",due:"Son tarih",assign:"Ata",all:"Tüm diller",failed:"Atama kaydedilemedi.",empty:"Açık yerelleştirme borcu yok",Missing:"Eksik çeviri",Untracked:"Kaynak sürümü kayıtsız",Stale:"Güncel değil",Review:"İnceleme bekliyor",snapshot:"Kaynak kanıtı",noSnapshot:"Eski kayıt — kaynak snapshot’ı yok",changed:"Değişen alanlar",Title:"Başlık",Summary:"Özet",Body:"Gövde",Seo:"SEO",Unassigned:"Sahipsiz",Overdue:"Gecikmiş",DueSoon:"Yaklaşıyor",OnTrack:"Planlı"},
  "en-US":{kicker:"LOCALIZATION OPERATIONS",title:"Source diff and SLA queue",description:"See the source version behind every translation and the fields that changed, then secure the work with an owner and deadline.",owner:"Owner",due:"Due date",assign:"Assign",all:"All locales",failed:"Assignment could not be saved.",empty:"No open localization debt",Missing:"Missing translation",Untracked:"Source version untracked",Stale:"Out of date",Review:"Review pending",snapshot:"Source evidence",noSnapshot:"Legacy record — no source snapshot",changed:"Changed fields",Title:"Title",Summary:"Summary",Body:"Body",Seo:"SEO",Unassigned:"Unassigned",Overdue:"Overdue",DueSoon:"Due soon",OnTrack:"On track"},
  "de-DE":{kicker:"LOKALISIERUNGSBETRIEB",title:"Quelldifferenz und SLA-Warteschlange",description:"Quellversion und geänderte Felder jeder Übersetzung prüfen und die Arbeit verbindlich zuweisen.",owner:"Verantwortlich",due:"Fällig",assign:"Zuweisen",all:"Alle Sprachen",failed:"Zuweisung konnte nicht gespeichert werden.",empty:"Keine offene Lokalisierungsschuld",Missing:"Übersetzung fehlt",Untracked:"Quellversion nicht erfasst",Stale:"Veraltet",Review:"Prüfung offen",snapshot:"Quellnachweis",noSnapshot:"Alteintrag — kein Quell-Snapshot",changed:"Geänderte Felder",Title:"Titel",Summary:"Zusammenfassung",Body:"Text",Seo:"SEO",Unassigned:"Nicht zugewiesen",Overdue:"Überfällig",DueSoon:"Bald fällig",OnTrack:"Im Plan"},
  "fr-FR":{kicker:"OPÉRATIONS DE LOCALISATION",title:"Écarts source et file SLA",description:"Vérifiez la version source et les champs modifiés de chaque traduction, puis attribuez le travail.",owner:"Responsable",due:"Échéance",assign:"Attribuer",all:"Toutes les langues",failed:"L’attribution n’a pas pu être enregistrée.",empty:"Aucune dette de localisation ouverte",Missing:"Traduction manquante",Untracked:"Version source non suivie",Stale:"Obsolète",Review:"Révision requise",snapshot:"Preuve source",noSnapshot:"Ancien contenu — aucun instantané source",changed:"Champs modifiés",Title:"Titre",Summary:"Résumé",Body:"Corps",Seo:"SEO",Unassigned:"Non attribué",Overdue:"En retard",DueSoon:"Bientôt dû",OnTrack:"Planifié"}
} as const;

export function LocalizationWorkQueue({locale,work}:{locale:Locale;work:LocalizationWork}){
  const c=workCopy[locale],router=useRouter(),[pending,setPending]=useState<string|null>(null),[filter,setFilter]=useState("All"),[error,setError]=useState("");
  const items=filter==="All"?work.items:work.items.filter(item=>item.targetLocale===filter);
  async function assign(event:FormEvent<HTMLFormElement>,item:LocalizationWork["items"][number]){event.preventDefault();setPending(item.articleGroupId+item.targetLocale);setError("");const data=new FormData(event.currentTarget);try{await mutate(`/work/${item.articleGroupId}/${item.targetLocaleId}`,"PUT",{assigneeUserId:data.get("owner"),dueAt:new Date(`${data.get("due")}T12:00:00Z`).toISOString()});router.refresh()}catch{setError(c.failed)}finally{setPending(null)}}
  return <section className="localization-work"><header><div><p className="section-kicker">{c.kicker}</p><h2>{c.title}</h2><p>{c.description}</p></div><select aria-label="Locale" value={filter} onChange={e=>setFilter(e.target.value)}><option value="All">{c.all}</option>{[...new Set(work.items.map(x=>x.targetLocale))].map(code=><option key={code}>{code}</option>)}</select></header>{error&&<p className="form-error" role="alert">{error}</p>}{items.length===0?<div className="admin-panel localization-empty">✓ {c.empty}</div>:<div className="localization-work-list">{items.map(item=><article className={`admin-panel localization-work-card ${item.sla.toLowerCase()}`} key={`${item.articleGroupId}-${item.targetLocale}`}><div><span className="language-state">{item.targetLocale}</span><span className="localization-kind">{c[item.kind]}</span><span className="localization-sla">{c[item.sla]}</span></div><h3>{item.translationTitle??item.sourceTitle}</h3>{item.translationTitle&&<small>{item.sourceTitle}</small>}{item.translationTitle&&<div className="localization-source-evidence"><b>{c.snapshot}</b><span>{item.sourceSnapshotAt?new Intl.DateTimeFormat(locale,{dateStyle:"medium",timeStyle:"short"}).format(new Date(item.sourceSnapshotAt)):c.noSnapshot}</span>{item.changedFields.some(field=>field!=="Untracked")&&<small>{c.changed}: {item.changedFields.filter(field=>field!=="Untracked").map(field=>c[field]).join(" · ")}</small>}</div>}<form onSubmit={e=>void assign(e,item)}><label>{c.owner}<select name="owner" required defaultValue={item.assignment?.assigneeUserId??""}><option value="" disabled>—</option>{work.users.map(user=><option value={user.id} key={user.id}>{user.displayName}</option>)}</select></label><label>{c.due}<input name="due" type="date" required defaultValue={item.assignment?.dueAt.slice(0,10)}/></label><button disabled={pending===item.articleGroupId+item.targetLocale}>{c.assign}</button></form></article>)}</div>}</section>
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
  const missingTags = enabled.reduce((total, item) => total + item.missingTagCount, 0);
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
        <article className={missingTags ? "needs-attention" : ""}><small>Eksik etiket</small><strong>{missingTags}</strong><span>locale geçişi olmayan arşiv</span></article>
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
            {!item.isDefault && <small className={item.missingTagCount ? "taxonomy-debt" : ""}>{item.linkedTagCount}/{item.sourceTagCount} etiket bağlı</small>}
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
