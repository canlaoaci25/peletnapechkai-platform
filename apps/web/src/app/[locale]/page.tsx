import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";

import { AdSlot } from "@/components/ad-slot";
import { SiteHeader } from "@/components/site-header";
import { siteConfig } from "@/config/site";
import { hasLocale, localeLabels } from "@/i18n/config";
import { getDictionary } from "@/i18n/get-dictionary";
import { getPublicHomepage, type PublicArticleSummary } from "@/lib/public-api";

export const dynamic = "force-dynamic";

function ArticleImage({ article, priority = false }: { article: PublicArticleSummary; priority?: boolean }) {
  return article.cover ? (
    <Image src={article.cover.url} alt={article.cover.altText} fill priority={priority} sizes="(max-width: 760px) 100vw, 50vw" unoptimized />
  ) : <span className="home-image-fallback" aria-hidden="true">BOECL</span>;
}

export default async function LocaleHome({ params }: PageProps<"/[locale]">) {
  const { locale } = await params;
  if (!hasLocale(locale)) notFound();

  const [dictionary, homepage] = await Promise.all([getDictionary(locale), getPublicHomepage(locale)]);
  const copy = dictionary.home;
  const lead = homepage.lead;
  const secondary = homepage.secondary;
  const trending = homepage.trending;
  const picks = homepage.editors;
  const latest = homepage.latest;
  const articles = [lead,...secondary,...trending,...picks,...latest].filter((item):item is PublicArticleSummary=>item!==null).filter((item,index,all)=>all.findIndex(candidate=>candidate.slug===item.slug)===index);
  const types = [...new Set(articles.map(article => article.type))].slice(0, 3);
  const formatDate = (date: string) => new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(date));

  return (
    <div className="site-shell home-shell">
      <SiteHeader locale={locale} />
      <main id="main-content">
        <nav className="topic-strip" aria-label={copy.currentTopics}>
          <strong>{copy.currentTopics}</strong>
          {[dictionary.navigation.technology, dictionary.navigation.ai, dictionary.navigation.science, dictionary.navigation.software, dictionary.navigation.mobile].map(topic => (
            <Link key={topic} href={`/${locale}/search?q=${encodeURIComponent(topic)}`}>{topic}</Link>
          ))}
        </nav>

        {lead ? (
          <section className="headline-grid" aria-labelledby="headline-title">
            <article className="lead-story">
              <Link className="lead-image" href={`/${locale}/articles/${lead.slug}`}><ArticleImage article={lead} priority /></Link>
              <div className="lead-copy">
                <p className="section-kicker">{copy.featured}</p>
                <h1 id="headline-title"><Link href={`/${locale}/articles/${lead.slug}`}>{lead.title}</Link></h1>
                <p>{lead.summary}</p>
                <span>{lead.type} · {formatDate(lead.publishedAt)}</span>
              </div>
            </article>
            <div className="secondary-stories">
              {secondary.map(article => (
                <article key={article.slug}>
                  <Link className="secondary-image" href={`/${locale}/articles/${article.slug}`}><ArticleImage article={article} /></Link>
                  <div><p className="section-kicker">{article.type}</p><h2><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h2><time dateTime={article.publishedAt}>{formatDate(article.publishedAt)}</time></div>
                </article>
              ))}
            </div>
          </section>
        ) : (
          <section className="hero"><p className="eyebrow">{copy.eyebrow}</p><h1>{copy.title}</h1><p className="hero-description">{copy.description}</p><p className="muted">{copy.noArticles}</p></section>
        )}

        {trending.length > 0 && <section className="trending-section" aria-labelledby="trending-title">
          <header className="home-section-header"><div><p className="section-kicker">01 / {copy.discover}</p><h2 id="trending-title">{copy.trending}</h2></div><span>{copy.trendingHint}</span></header>
          <ol>{trending.map((article, index) => <li key={article.slug}><span>{String(index + 1).padStart(2, "0")}</span><div><small>{article.type}</small><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3></div></li>)}</ol>
        </section>}

        {picks.length > 0 && <section className="picks-section" aria-labelledby="picks-title">
          <header className="home-section-header"><div><p className="section-kicker">02 / {copy.curated}</p><h2 id="picks-title">{copy.editorsPicks}</h2></div></header>
          <div className="pick-grid">{picks.map(article => <article key={article.slug}><Link className="pick-image" href={`/${locale}/articles/${article.slug}`}><ArticleImage article={article} /></Link><p className="section-kicker">{article.type}</p><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3><p>{article.summary}</p></article>)}</div>
        </section>}

        <AdSlot label={copy.advertisement} />

        {types.map((type, typeIndex) => {
          const items = articles.filter(article => article.type === type).slice(0, 4);
          if (items.length < 2) return null;
          return <section className="category-showcase" key={type} aria-labelledby={`type-${typeIndex}`}>
            <header className="home-section-header"><div><p className="section-kicker">{String(typeIndex + 3).padStart(2, "0")} / {copy.coverage}</p><h2 id={`type-${typeIndex}`}>{type}</h2></div><Link href={`/${locale}/search?q=${encodeURIComponent(type)}`}>{copy.seeAll} →</Link></header>
            <div className="category-grid">{items.map((article, index) => <article className={index === 0 ? "category-feature" : ""} key={article.slug}>{index === 0 && <Link className="category-image" href={`/${locale}/articles/${article.slug}`}><ArticleImage article={article} /></Link>}<div><small>{formatDate(article.publishedAt)}</small><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3>{index === 0 && <p>{article.summary}</p>}</div></article>)}</div>
          </section>;
        })}

        {latest.length > 0 && <section className="latest-feed" aria-labelledby="latest-title">
          <header className="home-section-header"><div><p className="section-kicker">{copy.justIn}</p><h2 id="latest-title">{copy.latestTitle}</h2></div></header>
          <div>{latest.map(article => <article key={article.slug}>{article.cover && <Link className="latest-image" href={`/${locale}/articles/${article.slug}`}><ArticleImage article={article} /></Link>}<div><p className="section-kicker">{article.type}</p><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3><p>{article.summary}</p><time dateTime={article.publishedAt}>{formatDate(article.publishedAt)}</time></div></article>)}</div>
        </section>}

        <section className="home-search"><div><p className="section-kicker">{copy.explore}</p><h2>{dictionary.search.title}</h2><p>{copy.searchDescription}</p></div><form action={`/${locale}/search`} role="search"><label htmlFor="home-search">{dictionary.search.label}</label><div><input id="home-search" name="q" minLength={2} required /><button>{dictionary.search.submit}</button></div></form></section>
      </main>
      <footer className="site-footer"><span>© {new Date().getUTCFullYear()} {siteConfig.legalName}</span><span>{localeLabels[locale]}</span></footer>
    </div>
  );
}
