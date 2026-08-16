import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { SiteFooter } from "@/components/site-footer";
import { hasLocale } from "@/i18n/config";
import { legalDocuments, legalLabels, legalSlugs } from "@/i18n/legal-copy";
export async function generateMetadata({
  params,
}: PageProps<"/[locale]/legal/[document]">): Promise<Metadata> {
  const { locale, document } = await params;
  if (!hasLocale(locale) || !legalSlugs.includes(document as never)) return {};
  return {
    title: `${legalDocuments[locale][document].title} — BOECL`,
    alternates: { canonical: `/${locale}/legal/${document}` },
  };
}
export default async function LegalPage({
  params,
}: PageProps<"/[locale]/legal/[document]">) {
  const { locale, document } = await params;
  if (!hasLocale(locale) || !legalSlugs.includes(document as never)) notFound();
  const item = legalDocuments[locale][document];
  return (
    <div className="site-shell">
      <SiteHeader locale={locale} />
      <main className="legal-page">
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
