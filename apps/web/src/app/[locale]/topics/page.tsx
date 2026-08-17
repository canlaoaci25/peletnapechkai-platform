import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { hasLocale, locales } from "@/i18n/config";
import { topicCopy } from "@/i18n/topic-copy";
import { getPublicArchiveIndex } from "@/lib/public-api";

export const dynamic = "force-dynamic";
export async function generateMetadata({ params }: PageProps<"/[locale]/topics">): Promise<Metadata> {
  const { locale } = await params;
  if (!hasLocale(locale)) return {};
  const copy = topicCopy[locale];
  return { title: copy.title, description: copy.description, alternates: { canonical: `/${locale}/topics`, languages: Object.fromEntries(locales.map(code => [code, `/${code}/topics`])) } };
}

export default async function TopicsPage({ params }: PageProps<"/[locale]/topics">) {
  const { locale } = await params;
  if (!hasLocale(locale)) notFound();
  const [archives] = await Promise.all([getPublicArchiveIndex(locale)]);
  const copy = topicCopy[locale];
  const [lead, ...categories] = archives.categories;
  return <div className="site-shell"><SiteHeader locale={locale} /><main id="main-content" className="topics-page">
    <header className="topics-hero"><p className="section-kicker">{copy.eyebrow}</p><h1>{copy.title}</h1><p>{copy.description}</p></header>
    {lead && <section className="topic-lead" aria-labelledby="topic-lead-title">
      <div className="topic-lead-heading"><p className="section-kicker">{copy.lead}</p><p><strong>{lead.articleCount}</strong> {copy.stories}</p></div>
      <div className="topic-lead-grid">
        <div><h2 id="topic-lead-title"><Link href={`/${locale}/categories/${lead.slug}`}>{lead.title}</Link></h2><p>{lead.description}</p><Link className="topic-map-link" href={`/${locale}/categories/${lead.slug}`}>{copy.latest} →</Link></div>
        <div className="topic-lead-stories"><p className="section-kicker">{copy.now}</p>{lead.featured.map(article => <Link key={article.slug} href={`/${locale}/articles/${article.slug}`}>{article.title}<span aria-hidden="true">↗</span></Link>)}</div>
      </div>
    </section>}
    <section aria-labelledby="all-topics-title"><div className="topic-map-heading"><p className="section-kicker" id="all-topics-title">{copy.all}</p><span>{categories.length}</span></div>
    <div className="topic-map">{categories.map((category, index) => {
      const image = category.featured.find(article => article.cover)?.cover;
      return <article className="topic-map-card" key={category.slug}>
        <Link className="topic-map-visual" href={`/${locale}/categories/${category.slug}`} tabIndex={-1} aria-hidden="true">{image ? <Image src={image.url} alt="" fill sizes="(max-width: 700px) 100vw, 45vw" /> : <span>{String(index + 1).padStart(2, "0")}</span>}</Link>
        <div><p className="section-kicker">{category.articleCount} {copy.stories}</p><h2><Link href={`/${locale}/categories/${category.slug}`}>{category.title}</Link></h2>{category.description && <p>{category.description}</p>}<ul className="topic-story-list">{category.featured.slice(0,2).map(article=><li key={article.slug}><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></li>)}</ul><Link className="topic-map-link" href={`/${locale}/categories/${category.slug}`}>{copy.latest} →</Link></div>
      </article>;
    })}</div></section>
  </main></div>;
}
