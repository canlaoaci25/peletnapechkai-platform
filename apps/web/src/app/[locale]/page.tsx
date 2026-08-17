import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";

import { AdSlot } from "@/components/ad-slot";
import { SiteHeader } from "@/components/site-header";
import { siteConfig } from "@/config/site";
import { hasLocale, localeLabels } from "@/i18n/config";
import { getDictionary } from "@/i18n/get-dictionary";
import { getPublicArchiveIndex, getPublicHomepage, type PublicArticleSummary } from "@/lib/public-api";
import { homeImageSizes } from "@/lib/responsive-images";
import { absoluteUrl } from "@/lib/site-url";

export const dynamic = "force-dynamic";

function ArticleImage({ article, sizes, preload = false }: { article: PublicArticleSummary; sizes: string; preload?: boolean }) {
  return article.cover ? (
    <Image src={article.cover.url} alt="" fill preload={preload} sizes={sizes} />
  ) : <span className="home-image-fallback" aria-hidden="true">BOECL</span>;
}

function ArticleImageLink({ article, className, locale, sizes, preload = false }: { article: PublicArticleSummary; className: string; locale: string; sizes: string; preload?: boolean }) {
  return <Link className={className} href={`/${locale}/articles/${article.slug}`} tabIndex={-1} aria-hidden="true"><ArticleImage article={article} sizes={sizes} preload={preload} /></Link>;
}

export default async function LocaleHome({ params }: PageProps<"/[locale]">) {
  const { locale } = await params;
  if (!hasLocale(locale)) notFound();

  const [dictionary, homepage, archives] = await Promise.all([
    getDictionary(locale),
    getPublicHomepage(locale),
    getPublicArchiveIndex(locale),
  ]);
  const copy = dictionary.home;
  const lead = homepage.lead;
  const secondary = homepage.secondary;
  const trending = homepage.trending;
  const picks = homepage.editors;
  const latest = homepage.latest;
  const articles = [lead,...secondary,...trending,...picks,...latest].filter((item):item is PublicArticleSummary=>item!==null).filter((item,index,all)=>all.findIndex(candidate=>candidate.slug===item.slug)===index);
  const types = [...new Set(articles.map(article => article.type))].slice(0, 3);
  const atlasFeatureSlugs = new Set<string>();
  const atlasCategories = archives.categories.slice(0, 6).map(category => {
    const feature = category.featured.find(article => !atlasFeatureSlugs.has(article.slug));
    if (feature) atlasFeatureSlugs.add(feature.slug);
    return { ...category, feature };
  });
  const formatDate = (date: string) => new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(date));
  const editionDate = new Intl.DateTimeFormat(locale, { weekday: "long", day: "numeric", month: "long" }).format(new Date());
  const editionLinks = [
    trending.length > 0 && { href: "#popular", label: copy.trending, count: trending.length },
    atlasCategories.length > 0 && { href: "#topic-atlas", label: copy.topicAtlas, count: atlasCategories.length },
    picks.length > 0 && { href: "#editors-picks", label: copy.editorsPicks, count: picks.length },
    latest.length > 0 && { href: "#latest", label: copy.latestTitle, count: latest.length },
  ].filter((item): item is { href: string; label: string; count: number } => Boolean(item));
  const structuredData = {
    "@context": "https://schema.org",
    "@graph": [
      { "@type": "Organization", "@id": absoluteUrl("/#organization"), name: siteConfig.name, url: absoluteUrl(`/${locale}`) },
      { "@type": "WebSite", "@id": absoluteUrl("/#website"), name: siteConfig.name, url: absoluteUrl(`/${locale}`), inLanguage: locale, publisher: { "@id": absoluteUrl("/#organization") }, potentialAction: { "@type": "SearchAction", target: absoluteUrl(`/${locale}/search?q={search_term_string}`), "query-input": "required name=search_term_string" } },
    ],
  };

  return (
    <div className="site-shell home-shell">
      <SiteHeader locale={locale} homeActive />
      <main id="main-content">
        <nav className="topic-strip" aria-label={copy.currentTopics}>
          <strong><span aria-hidden="true" />{copy.currentTopics}</strong>
          {archives.categories.map(category => (
            <Link key={category.slug} href={`/${locale}/categories/${category.slug}`}>{category.title}</Link>
          ))}
        </nav>

        {lead && <section className="edition-route" aria-labelledby="edition-route-title">
          <div className="edition-route-intro">
            <p className="section-kicker">{copy.editionEyebrow}</p>
            <h2 id="edition-route-title">{copy.editionTitle}</h2>
            <p>{editionDate} <span aria-hidden="true">/</span> {articles.length} {copy.editionStories}</p>
          </div>
          <nav aria-label={copy.editionNavigation}>
            <a href="#headline-title"><span>00</span><strong>{copy.featured}</strong></a>
            {editionLinks.map((item, index) => <a href={item.href} key={item.href}><span>{String(index + 1).padStart(2, "0")}</span><strong>{item.label}</strong><small>{item.count}</small></a>)}
          </nav>
        </section>}

        {lead ? (
          <section className="headline-grid" aria-labelledby="headline-title">
            <article className="lead-story">
              <ArticleImageLink article={lead} className="lead-image" locale={locale} sizes={homeImageSizes.lead} preload />
              <div className="lead-copy">
                <div className="lead-context"><p className="section-kicker">{copy.featured}</p><span>{copy.dailyEdition}</span></div>
                <h1 id="headline-title"><Link href={`/${locale}/articles/${lead.slug}`}>{lead.title}</Link></h1>
                <p>{lead.summary}</p>
                <div className="lead-byline"><span>{lead.type} · {formatDate(lead.publishedAt)}</span><Link href={`/${locale}/articles/${lead.slug}`}>{copy.readArticle}<span aria-hidden="true"> →</span></Link></div>
              </div>
            </article>
            <div className="secondary-stories">
              <header><span>{copy.moreHeadlines}</span><strong>{String(secondary.length).padStart(2, "0")}</strong></header>
              {secondary.map(article => (
                <article key={article.slug}>
                  <ArticleImageLink article={article} className="secondary-image" locale={locale} sizes={homeImageSizes.secondary} />
                  <div><p className="section-kicker">{article.type}</p><h2><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h2><time dateTime={article.publishedAt}>{formatDate(article.publishedAt)}</time></div>
                </article>
              ))}
            </div>
          </section>
        ) : (
          <section className="hero"><p className="eyebrow">{copy.eyebrow}</p><h1>{copy.title}</h1><p className="hero-description">{copy.description}</p><p className="muted">{copy.noArticles}</p></section>
        )}

        {trending.length > 0 && <section className="trending-section" id="popular" aria-labelledby="trending-title">
          <header className="home-section-header"><div><p className="section-kicker">01 / {copy.discover}</p><h2 id="trending-title">{copy.trending}</h2></div><span>{copy.trendingHint}</span></header>
          <ol>{trending.map((article, index) => <li key={article.slug}><span>{String(index + 1).padStart(2, "0")}</span><div><small>{article.type}</small><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3></div></li>)}</ol>
        </section>}

        {atlasCategories.length > 0 && <section className="topic-atlas" id="topic-atlas" aria-labelledby="topic-atlas-title">
          <header className="home-section-header">
            <div><p className="section-kicker">02 / {copy.coverage}</p><h2 id="topic-atlas-title">{copy.topicAtlas}</h2></div>
            <p>{copy.topicAtlasDescription}</p>
          </header>
          <div className="topic-atlas-grid">
            {atlasCategories.map((category, categoryIndex) => {
              const { feature } = category;
              return <article key={category.slug} className={categoryIndex === 0 ? "topic-atlas-feature" : ""}>
                {feature && <ArticleImageLink article={feature} className="topic-atlas-image" locale={locale} sizes={categoryIndex === 0 ? homeImageSizes.atlasLead : homeImageSizes.atlas} />}
                <div className="topic-atlas-copy">
                  <div><span>{String(categoryIndex + 1).padStart(2, "0")}</span><small>{category.articleCount} {copy.publications}</small></div>
                  <h3><Link href={`/${locale}/categories/${category.slug}`}>{category.title}</Link></h3>
                  {category.description && <p>{category.description}</p>}
                  {feature && <Link className="topic-atlas-story" href={`/${locale}/articles/${feature.slug}`}>{feature.title}<span aria-hidden="true"> →</span></Link>}
                </div>
              </article>;
            })}
          </div>
          <Link className="topic-atlas-all" href={`/${locale}/topics`}>{copy.exploreAllTopics}<span aria-hidden="true"> →</span></Link>
        </section>}

        {picks.length > 0 && <section className="picks-section" id="editors-picks" aria-labelledby="picks-title">
          <header className="home-section-header"><div><p className="section-kicker">03 / {copy.curated}</p><h2 id="picks-title">{copy.editorsPicks}</h2></div></header>
          <div className="pick-grid">{picks.map(article => <article key={article.slug}><ArticleImageLink article={article} className="pick-image" locale={locale} sizes={homeImageSizes.pick} /><p className="section-kicker">{article.type}</p><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3><p>{article.summary}</p></article>)}</div>
        </section>}

        <AdSlot label={copy.advertisement} />

        {types.slice(0, 2).map((type, typeIndex) => {
          const items = articles.filter(article => article.type === type).slice(0, 4);
          if (items.length < 2) return null;
          return <section className="category-showcase" key={type} aria-labelledby={`type-${typeIndex}`}>
            <header className="home-section-header"><div><p className="section-kicker">{String(typeIndex + 4).padStart(2, "0")} / {copy.coverage}</p><h2 id={`type-${typeIndex}`}>{type}</h2></div><Link href={`/${locale}/search?q=${encodeURIComponent(type)}`}>{copy.seeAll} →</Link></header>
            <div className="category-grid">{items.map((article, index) => <article className={index === 0 ? "category-feature" : ""} key={article.slug}>{index === 0 && <ArticleImageLink article={article} className="category-image" locale={locale} sizes={homeImageSizes.category} />}<div><small>{formatDate(article.publishedAt)}</small><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3>{index === 0 && <p>{article.summary}</p>}</div></article>)}</div>
          </section>;
        })}

        {latest.length > 0 && <section className="latest-feed" id="latest" aria-labelledby="latest-title">
          <header className="home-section-header"><div><p className="section-kicker">{copy.justIn}</p><h2 id="latest-title">{copy.latestTitle}</h2></div></header>
          <div>{latest.map(article => <article key={article.slug}>{article.cover && <ArticleImageLink article={article} className="latest-image" locale={locale} sizes={homeImageSizes.latest} />}<div><p className="section-kicker">{article.type}</p><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3><p>{article.summary}</p><time dateTime={article.publishedAt}>{formatDate(article.publishedAt)}</time></div></article>)}</div>
        </section>}

        <section className="home-search"><div><p className="section-kicker">{copy.explore}</p><h2>{dictionary.search.title}</h2><p>{copy.searchDescription}</p></div><form action={`/${locale}/search`} role="search"><label htmlFor="home-search">{dictionary.search.label}</label><div><input id="home-search" name="q" minLength={2} required /><button>{dictionary.search.submit}</button></div></form></section>
      </main>
      <footer className="site-footer"><span>© {new Date().getUTCFullYear()} {siteConfig.legalName}</span><span>{localeLabels[locale]}</span></footer>
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(structuredData).replace(/</g, "\\u003c") }} />
    </div>
  );
}
