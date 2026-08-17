import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { hasLocale, type Locale } from "@/i18n/config";
import { getAdminSession, getLocalizationWorkDetail } from "@/lib/admin-api";

const copy = {
  "tr-TR": {
    kicker: "ÇEVİRİ KARŞILAŞTIRMASI",
    title: "Kaynak ve hedef sürümü birlikte inceleyin",
    intro:
      "Değişen kaynak alanlarını çevirinin mevcut karşılığıyla karşılaştırın. Bu ekran içeriği değiştirmez veya yayımlamaz.",
    source: "Türkçe kaynak",
    target: "Hedef sürüm",
    updated: "Güncellendi",
    snapshot: "Çevirinin kaynak kanıtı",
    untracked: "Kaynak sürümü kayıtsız",
    missing: "Bu dil için çeviri henüz oluşturulmamış.",
    changed: "Değişen kaynak alanları",
    edit: "Çeviriyi düzenle",
    back: "Kuyruğa dön",
    Title: "Başlık",
    Summary: "Özet",
    Body: "Gövde",
    Seo: "SEO",
    Untracked: "Kayıtsız",
    Missing: "Eksik",
    seoTitle: "SEO başlığı",
    seoDescription: "SEO açıklaması",
  },
  "en-US": {
    kicker: "TRANSLATION COMPARISON",
    title: "Review source and target together",
    intro:
      "Compare changed source fields with the current translation. This workspace does not change or publish content.",
    source: "Turkish source",
    target: "Target edition",
    updated: "Updated",
    snapshot: "Translation source evidence",
    untracked: "Source version untracked",
    missing: "No translation has been created for this locale.",
    changed: "Changed source fields",
    edit: "Edit translation",
    back: "Back to queue",
    Title: "Title",
    Summary: "Summary",
    Body: "Body",
    Seo: "SEO",
    Untracked: "Untracked",
    Missing: "Missing",
    seoTitle: "SEO title",
    seoDescription: "SEO description",
  },
  "de-DE": {
    kicker: "ÜBERSETZUNGSVERGLEICH",
    title: "Quelle und Ziel gemeinsam prüfen",
    intro:
      "Geänderte Quellfelder mit der aktuellen Übersetzung vergleichen. Dieser Bereich ändert oder veröffentlicht keine Inhalte.",
    source: "Türkische Quelle",
    target: "Zielausgabe",
    updated: "Aktualisiert",
    snapshot: "Quellnachweis der Übersetzung",
    untracked: "Quellversion nicht erfasst",
    missing: "Für diese Sprache wurde noch keine Übersetzung erstellt.",
    changed: "Geänderte Quellfelder",
    edit: "Übersetzung bearbeiten",
    back: "Zur Warteschlange",
    Title: "Titel",
    Summary: "Zusammenfassung",
    Body: "Text",
    Seo: "SEO",
    Untracked: "Nicht erfasst",
    Missing: "Fehlt",
    seoTitle: "SEO-Titel",
    seoDescription: "SEO-Beschreibung",
  },
  "fr-FR": {
    kicker: "COMPARAISON DE TRADUCTION",
    title: "Examiner la source et la cible ensemble",
    intro:
      "Comparez les champs source modifiés avec la traduction actuelle. Cet espace ne modifie ni ne publie le contenu.",
    source: "Source turque",
    target: "Édition cible",
    updated: "Mis à jour",
    snapshot: "Preuve source de la traduction",
    untracked: "Version source non suivie",
    missing: "Aucune traduction n’a encore été créée pour cette langue.",
    changed: "Champs source modifiés",
    edit: "Modifier la traduction",
    back: "Retour à la file",
    Title: "Titre",
    Summary: "Résumé",
    Body: "Corps",
    Seo: "SEO",
    Untracked: "Non suivi",
    Missing: "Manquant",
    seoTitle: "Titre SEO",
    seoDescription: "Description SEO",
  },
} as const;

function Field({
  label,
  value,
  changed,
}: {
  label: string;
  value: string | null;
  changed?: boolean;
}) {
  return (
    <section
      className={
        changed
          ? "localization-compare-field changed"
          : "localization-compare-field"
      }
    >
      <h2>{label}</h2>
      <pre>{value?.trim() || "—"}</pre>
    </section>
  );
}

export default async function LocalizationComparisonPage({
  params,
}: PageProps<"/[locale]/admin/languages/work/[articleGroupId]/[targetLocaleId]">) {
  const { locale, articleGroupId, targetLocaleId } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  if (!session.roles.some((role) => ["Owner", "Admin"].includes(role)))
    redirect(`/${locale}/admin`);
  const detail = await getLocalizationWorkDetail(
    articleGroupId,
    targetLocaleId,
  );
  if (!detail) notFound();
  const c = copy[locale as Locale],
    changed = new Set(detail.changedFields);
  const date = (value: string) =>
    new Intl.DateTimeFormat(locale, {
      dateStyle: "medium",
      timeStyle: "short",
    }).format(new Date(value));
  return (
    <main className="admin-shell localization-compare-shell">
      <Link className="back-link" href={`/${locale}/admin/languages`}>
        ← {c.back}
      </Link>
      <header className="admin-command-header">
        <div>
          <p className="section-kicker">{c.kicker}</p>
          <h1>{c.title}</h1>
          <p>{c.intro}</p>
        </div>
        {detail.translation && (
          <Link
            className="primary-link"
            href={`/${locale}/admin/articles/${detail.translation.id}`}
          >
            {c.edit}
          </Link>
        )}
      </header>
      <aside className="admin-panel localization-compare-summary">
        <div>
          <b>{c.changed}</b>
          <span>
            {detail.changedFields.map((field) => c[field]).join(" · ")}
          </span>
        </div>
        <div>
          <b>{c.snapshot}</b>
          <span>
            {detail.translation?.sourceSnapshotUpdatedAt
              ? date(detail.translation.sourceSnapshotUpdatedAt)
              : c.untracked}
          </span>
        </div>
      </aside>
      <div className="localization-compare-grid">
        <article className="admin-panel localization-compare-column">
          <header>
            <span className="language-state">{detail.source.locale}</span>
            <h2>{c.source}</h2>
            <small>
              {c.updated}: {date(detail.source.updatedAt)}
            </small>
          </header>
          <Field
            label={c.Title}
            value={detail.source.title}
            changed={changed.has("Title")}
          />
          <Field
            label={c.Summary}
            value={detail.source.summary}
            changed={changed.has("Summary")}
          />
          <Field
            label={c.Body}
            value={detail.source.body}
            changed={changed.has("Body")}
          />
          <Field
            label={c.seoTitle}
            value={detail.source.seoTitle}
            changed={changed.has("Seo")}
          />
          <Field
            label={c.seoDescription}
            value={detail.source.seoDescription}
            changed={changed.has("Seo")}
          />
        </article>
        <article className="admin-panel localization-compare-column">
          <header>
            <span className="language-state">{detail.targetLocale}</span>
            <h2>{c.target}</h2>
            {detail.translation && (
              <small>
                {c.updated}: {date(detail.translation.updatedAt)} ·{" "}
                {detail.translation.status}
              </small>
            )}
          </header>
          {detail.translation ? (
            <>
              <Field
                label={c.Title}
                value={detail.translation.title}
                changed={changed.has("Title")}
              />
              <Field
                label={c.Summary}
                value={detail.translation.summary}
                changed={changed.has("Summary")}
              />
              <Field
                label={c.Body}
                value={detail.translation.body}
                changed={changed.has("Body")}
              />
              <Field
                label={c.seoTitle}
                value={detail.translation.seoTitle}
                changed={changed.has("Seo")}
              />
              <Field
                label={c.seoDescription}
                value={detail.translation.seoDescription}
                changed={changed.has("Seo")}
              />
            </>
          ) : (
            <p className="localization-compare-missing">{c.missing}</p>
          )}
        </article>
      </div>
    </main>
  );
}
