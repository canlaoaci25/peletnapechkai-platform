import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { hasLocale, locales } from "@/i18n/config";
import { topicCopy } from "@/i18n/topic-copy";
import { getPublicArchiveIndex } from "@/lib/public-api";
import { focalPointStyle } from "@/lib/focal-point";

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
  const categories = archives.categories.filter(category => !category.parent);
  const [lead] = categories;
  const tags = archives.tags.filter(tag => tag.articleCount > 0).slice(0, 12);
  return <div className="site-shell"><SiteHeader locale={locale} /><main id="main-content" className="topics-page">
    <header className="topics-hero"><p className="section-kicker">{copy.eyebrow}</p><h1>{copy.title}</h1><p>{copy.description}</p></header>
    {lead && <section className="topic-lead" aria-labelledby="topic-lead-title">
      <div className="topic-lead-heading"><p className="section-kicker">{copy.lead}</p><p><strong>{lead.articleCount}</strong> {copy.stories}</p></div>
      <div className="topic-lead-grid">
        <div><h2 id="topic-lead-title"><Link href={`/${locale}/categories/${lead.slug}`}>{lead.title}</Link></h2><p>{lead.description}</p><Link className="topic-map-link" href={`/${locale}/categories/${lead.slug}`}>{copy.latest} →</Link></div>
        <div className="topic-lead-stories"><p className="section-kicker">{copy.now}</p>{lead.featured.map(article => <Link key={article.slug} href={`/${locale}/articles/${article.slug}`}>{article.title}<span aria-hidden="true">↗</span></Link>)}</div>
      </div>
    </section>}
    <section aria-labelledby="all-topics-title"><div className="topic-map-heading"><div><p className="section-kicker">{copy.paths}</p><h2 id="all-topics-title">{copy.all}</h2></div><span>{categories.length}</span></div>
    <div className="topic-map">{categories.map((category, index) => {
      const image = category.featured.find(article => article.cover)?.cover;
      return <article className="topic-map-card" key={category.slug}>
        <Link className="topic-map-visual" href={`/${locale}/categories/${category.slug}`} tabIndex={-1} aria-hidden="true">{image ? <Image src={image.url} alt="" fill sizes="(max-width: 700px) 100vw, 45vw" style={focalPointStyle(image)} /> : <span>{String(index + 1).padStart(2, "0")}</span>}</Link>
        <div><p className="section-kicker">{category.articleCount} {copy.stories}</p><h2><Link href={`/${locale}/categories/${category.slug}`}>{category.title}</Link></h2>{category.description && <p>{category.description}</p>}{category.children.length>0&&<div className="topic-children"><strong>{copy.subtopics}</strong>{category.children.map(child=><Link key={child.slug} href={`/${locale}/categories/${child.slug}`}><span>{child.title}</span><small>{child.articleCount}</small></Link>)}</div>}<ul className="topic-story-list">{category.featured.slice(0,2).map(article=><li key={article.slug}><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></li>)}</ul><Link className="topic-map-link" href={`/${locale}/categories/${category.slug}`}>{copy.latest} →</Link></div>
      </article>;
    })}</div></section>
    {tags.length > 0 && <section className="tag-atlas" aria-labelledby="tag-atlas-title">
      <header><div><p className="section-kicker">{copy.tagEyebrow}</p><h2 id="tag-atlas-title">{copy.tagTitle}</h2></div><p>{copy.tagDescription}</p></header>
      <div className="tag-atlas-grid">{tags.map((tag, index) => <Link key={tag.slug} href={`/${locale}/tags/${tag.slug}`} aria-label={`${copy.tagAction}: ${tag.title}`}><small>{String(index + 1).padStart(2, "0")}</small><strong>{tag.title}</strong><span>{tag.articleCount} {copy.stories} <b aria-hidden="true">↗</b></span></Link>)}</div>
    </section>}
  </main></div>;
}
