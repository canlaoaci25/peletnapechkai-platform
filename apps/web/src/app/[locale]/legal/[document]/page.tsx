import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { SiteFooter } from "@/components/site-footer";
import { hasLocale } from "@/i18n/config";
import { legalDocuments, legalLabels, legalSlugs } from "@/i18n/legal-copy";
import { buildStaticLocaleAlternates } from "@/lib/discovery-alternates";
import { buildDiscoveryStructuredData } from "@/lib/discovery-structured-data";
import { absoluteUrl } from "@/lib/site-url";
import { breadcrumbLabels } from "@/i18n/accessibility-copy";
export async function generateMetadata({
  params,
}: PageProps<"/[locale]/legal/[document]">): Promise<Metadata> {
  const { locale, document } = await params;
  if (!hasLocale(locale) || !legalSlugs.includes(document as never)) return {};
  return {
    title: `${legalDocuments[locale][document].title} — BOECL`,
    alternates: { canonical: `/${locale}/legal/${document}`, languages: buildStaticLocaleAlternates(`/legal/${document}`) },
  };
}
export default async function LegalPage({
  params,
}: PageProps<"/[locale]/legal/[document]">) {
  const { locale, document } = await params;
  if (!hasLocale(locale) || !legalSlugs.includes(document as never)) notFound();
  const item = legalDocuments[locale][document];
  const schema = buildDiscoveryStructuredData({ type: "WebPage", title: item.title, url: absoluteUrl(`/${locale}/legal/${document}`), locale, breadcrumbs: [{ name: item.title, url: absoluteUrl(`/${locale}/legal/${document}`) }] });
  return (
    <div className="site-shell">
      <SiteHeader locale={locale} />
      <main className="legal-page">
        <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(schema).replace(/</g, "\\u003c") }} />
        <nav className="archive-breadcrumbs" aria-label={breadcrumbLabels[locale]}><span aria-current="page">{item.title}</span></nav>
        <h1>{item.title}</h1>
        <p className="muted">
          {legalLabels[locale].updated}: {item.updated}
        </p>
        {item.sections.map((section) => (
          <section key={section.heading}>
            <h2>{section.heading}</h2>
            <p>{section.body}</p>
          </section>
        ))}
      </main>
      <SiteFooter locale={locale} />
    </div>
  );
}
