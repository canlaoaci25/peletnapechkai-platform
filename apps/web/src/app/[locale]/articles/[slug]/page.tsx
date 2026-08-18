import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { ArticleEngagement } from "@/components/article-engagement";
import { SaveArticleButton } from "@/components/save-article-button";
import { siteConfig } from "@/config/site";
import { commercialCopy } from "@/i18n/commercial-copy";
import { correctionCopy } from "@/i18n/correction-copy";
import { claimCitationCopy } from "@/i18n/claim-citation-copy";
import { hasLocale, localeLabels, locales } from "@/i18n/config";
import { getDictionary } from "@/i18n/get-dictionary";
import { getPublishedArticle, getRelatedArticles } from "@/lib/public-api";
import { buildArticleStructuredData, getPublicSource } from "@/lib/article-structured-data";
import { buildArticleOutline } from "@/lib/article-outline";
import { estimateReadingMinutes, wasMeaningfullyUpdated } from "@/lib/article-reading";
import { absoluteUrl } from "@/lib/site-url";
import { focalPointStyle } from "@/lib/focal-point";

function markdownBodyToHtml(body: string) {
  if (body.trimStart().startsWith("<")) return body;
  const escape = (value: string) => value.replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");
  const output: string[] = [];
  let list: "ul" | "ol" | null = null;
  const closeList = () => { if (list) output.push(`</${list}>`); list = null; };
  for (const raw of body.replaceAll("\r\n", "\n").split("\n")) {
    const line = raw.trim();
    if (!line) { closeList(); continue; }
    if (line.startsWith("<figure") || line.startsWith("<img")) { closeList(); output.push(line); continue; }
    const heading = line.match(/^(#{2,4})\s+(.+)$/);
    if (heading) { closeList(); const level = heading[1].length; output.push(`<h${level}>${escape(heading[2])}</h${level}>`); continue; }
    const bullet = line.match(/^[-*]\s+(.+)$/), numbered = line.match(/^\d+[.)]\s+(.+)$/);
    if (bullet || numbered) {
      const wanted = bullet ? "ul" : "ol";
      if (list !== wanted) { closeList(); list = wanted; output.push(`<${wanted}>`); }
      output.push(`<li>${escape((bullet ?? numbered)![1])}</li>`); continue;
    }
    closeList(); output.push(`<p>${escape(line)}</p>`);
  }
  closeList(); return output.join("");
}

export const dynamic = "force-dynamic";
export async function generateMetadata({
  params,
}: PageProps<"/[locale]/articles/[slug]">): Promise<Metadata> {
  const { locale, slug } = await params;
  if (!hasLocale(locale)) return {};
  const article = await getPublishedArticle(locale, slug);
  if (!article) return {};
  const languages = Object.fromEntries(
    article.translations.map((item) => [
      item.locale,
      `/${item.locale}/articles/${item.slug}`,
    ]),
  );
  languages["x-default"] = languages["tr-TR"] ?? `/${locale}/articles/${slug}`;
  const image = article.cover ? absoluteUrl(article.cover.url) : undefined;
  return {
    title: article.seoTitle ?? `${article.title} — ${siteConfig.name}`,
    description: article.seoDescription ?? article.summary,
    keywords: article.tags.map((tag) => tag.name),
    alternates: { canonical: `/${locale}/articles/${slug}`, languages },
    openGraph: {
      type: "article",
      title: article.seoTitle ?? article.title,
      description: article.seoDescription ?? article.summary,
      publishedTime: article.publishedAt,
      modifiedTime: article.updatedAt,
      url: `/${locale}/articles/${slug}`,
      siteName: siteConfig.name,
      images: image ? [{ url: image, alt: article.cover!.altText }] : undefined,
    },
  };
}

export default async function ArticlePage({
  params,
}: PageProps<"/[locale]/articles/[slug]">) {
  const { locale, slug } = await params;
  if (!hasLocale(locale)) notFound();
  const [article, dictionary, related] = await Promise.all([
    getPublishedArticle(locale, slug),
    getDictionary(locale),
    getRelatedArticles(locale, slug),
  ]);
  if (!article) notFound();
  const isHtml = article.body.trimStart().startsWith("<"),
    commercial = commercialCopy[locale], correctionsCopy = correctionCopy[locale], citationsCopy=claimCitationCopy[locale];
  const { bodyHtml, outline } = buildArticleOutline(isHtml ? article.body : markdownBodyToHtml(article.body));
  const publicSources = article.sources.map(getPublicSource).filter((source) => source !== null);
  const articleCopy = dictionary.article;
  const editions = article.translations.filter(
    (translation): translation is typeof translation & { locale: keyof typeof localeLabels } => hasLocale(translation.locale),
  );
  const readingMinutes = estimateReadingMinutes(article.body);
  const hasMeaningfulUpdate = wasMeaningfullyUpdated(article.publishedAt, article.updatedAt);
  const formatDate = (value: string) => new Intl.DateTimeFormat(locale, { dateStyle: "long" }).format(new Date(value));
  const interpolate = (template: string, values: Record<string, string | number>) =>
    Object.entries(values).reduce((result, [key, value]) => result.replace(`{${key}}`, String(value)), template);
  const structuredData = buildArticleStructuredData({
    title: article.title,
    summary: article.summary,
    seoDescription: article.seoDescription,
    publishedAt: article.publishedAt,
    updatedAt: article.updatedAt,
    locale,
    canonicalUrl: absoluteUrl(`/${locale}/articles/${slug}`),
    imageUrl: article.cover ? absoluteUrl(article.cover.url) : undefined,
    categories: article.categories,
    tags: article.tags,
    authors: article.authors.map((author) => ({
      displayName: author.displayName,
      url: absoluteUrl(`/${locale}/authors/${author.slug}`),
    })),
    sources: article.sources,
    publisher: {
      id: absoluteUrl("/#organization"),
      name: siteConfig.name,
      url: absoluteUrl(`/${locale}`),
    },
  });
  const breadcrumbData = {
    "@context": "https://schema.org",
    "@type": "BreadcrumbList",
    itemListElement: [
      { "@type": "ListItem", position: 1, name: dictionary.navigation.home, item: absoluteUrl(`/${locale}`) },
      ...(article.categories[0] ? [{ "@type": "ListItem", position: 2, name: article.categories[0].name, item: absoluteUrl(`/${locale}/categories/${article.categories[0].slug}`) }] : []),
      { "@type": "ListItem", position: article.categories[0] ? 3 : 2, name: article.title, item: absoluteUrl(`/${locale}/articles/${slug}`) },
    ],
  };
  return (
    <div className="site-shell">
      <SiteHeader locale={locale} localeHrefs={Object.fromEntries(article.translations.map((item) => [item.locale, `/${item.locale}/articles/${item.slug}`]))} />
      <ArticleEngagement locale={locale} slug={slug} />
      <main className="article-page" id="main-content" tabIndex={-1}>
        <nav className="article-breadcrumbs" aria-label="Breadcrumb">
          <Link href={`/${locale}`}>{dictionary.navigation.home}</Link>
          {article.categories[0] && <><span aria-hidden="true">/</span><Link href={`/${locale}/categories/${article.categories[0].slug}`}>{article.categories[0].name}</Link></>}
        </nav>
        <article>
          {(article.isSponsored || article.hasAffiliateLinks) && (
            <aside
              className="commercial-disclosure"
              aria-label={commercial.disclosure}
            >
              {article.isSponsored && (
                <strong>
                  {commercial.sponsoredBy}: {article.sponsorName}
                </strong>
              )}
              {article.hasAffiliateLinks && <p>{commercial.affiliateNotice}</p>}
            </aside>
          )}
          <header>
            <p className="section-kicker">{article.type}</p>
            <h1>{article.title}</h1>
            <p className="article-lead">{article.summary}</p>
            <aside className="article-editions" aria-labelledby="article-editions-title">
              <div>
                <span className="article-editions-mark" aria-hidden="true">文</span>
                <div>
                  <h2 id="article-editions-title">{articleCopy.editionHeading}</h2>
                  <p>{interpolate(articleCopy.editionSummary, { available: editions.length, total: locales.length })}</p>
                </div>
              </div>
              <nav aria-label={articleCopy.editionHeading}>
                {editions.map((edition) => edition.locale === locale ? (
                  <span key={edition.locale} aria-current="page">
                    {localeLabels[edition.locale]}
                    <small>{articleCopy.currentEdition}</small>
                  </span>
                ) : (
                  <Link key={edition.locale} href={`/${edition.locale}/articles/${edition.slug}`} hrefLang={edition.locale} lang={edition.locale}>
                    {localeLabels[edition.locale]}
                  </Link>
                ))}
              </nav>
            </aside>
            <div className="article-facts" aria-label={articleCopy.editorialTrust}>
              {article.authors.length > 0 && <div className="article-byline"><span className="article-fact-label">BOECL</span><strong>{article.authors.map((author, index) => <span key={author.slug}>{index > 0 && ", "}<Link href={`/${locale}/authors/${author.slug}`}>{author.displayName}</Link></span>)}</strong></div>}
              <div><span className="article-fact-label">{articleCopy.published}</span><time dateTime={article.publishedAt}>{formatDate(article.publishedAt)}</time></div>
              {hasMeaningfulUpdate && <div><span className="article-fact-label">{articleCopy.updated}</span><time dateTime={article.updatedAt}>{formatDate(article.updatedAt)}</time></div>}
              {article.corrections[0] && <div><span className="article-fact-label">{correctionsCopy.last}</span><time dateTime={article.corrections[0].correctedAt}>{formatDate(article.corrections[0].correctedAt)}</time></div>}
              <div><span>{interpolate(articleCopy.minuteRead, { minutes: readingMinutes })}</span>{publicSources.length > 0 && <a href="#article-sources">{interpolate(articleCopy.sourcesUsed, { count: publicSources.length })}</a>}</div>
            </div>
            <SaveArticleButton locale={locale} slug={slug} />
            {article.categories.length > 0 && (
              <div className="article-taxonomy">
                {article.categories.map((category) => (
                  <Link
                    key={category.slug}
                    href={`/${locale}/categories/${category.slug}`}
                  >
                    {category.name}
                  </Link>
                ))}
              </div>
            )}
          </header>
          {article.cover && (
            <figure className="article-cover">
              <Image
                src={article.cover.url}
                alt={article.cover.altText}
                width={1200}
                height={675}
                sizes="(max-width: 820px) calc(100vw - 40px), 780px"
                preload
                style={focalPointStyle(article.cover)}
              />
              {(article.cover.caption || article.cover.credit) && (
                <figcaption>
                  {article.cover.caption?.startsWith("https://") ? <a href={article.cover.caption} target="_blank" rel="noreferrer">Pexels kaynak sayfası</a> : article.cover.caption}
                  {article.cover.caption && article.cover.credit && " — "}
                  {article.cover.credit}
                </figcaption>
              )}
            </figure>
          )}
          {outline.length >= 2 && (
            <nav className="article-outline" aria-labelledby="article-outline-title">
              <h2 id="article-outline-title">{dictionary.article.onThisPage}</h2>
              <ol>
                {outline.map((item) => (
                  <li key={item.id} data-level={item.level}>
                    <a href={`#${item.id}`}>{item.label}</a>
                  </li>
                ))}
              </ol>
            </nav>
          )}
          <aside className="article-trust-note" aria-labelledby="article-trust-title">
            <span aria-hidden="true">✓</span>
            <div><h2 id="article-trust-title">{articleCopy.editorialTrust}</h2><p>{articleCopy.trustSummary}</p></div>
          </aside>
          <div className="article-body">
            <div className="rich-article-body" dangerouslySetInnerHTML={{ __html: bodyHtml }} />
          </div>
          {article.claimCitations.length>0&&<aside className="article-claim-citations" aria-labelledby="claim-citations-title"><header><p className="section-kicker">BOECL · EVIDENCE</p><h2 id="claim-citations-title">{citationsCopy.heading}</h2><p>{citationsCopy.summary}</p></header><ol>{article.claimCitations.map((item,index)=><li key={item.id}><span aria-hidden="true">{String(index+1).padStart(2,"0")}</span><div><blockquote>{item.claim}</blockquote><a href={item.sourceUrl} target="_blank" rel="nofollow noopener noreferrer"><strong>{item.sourceName}</strong>{item.locator&&<small>{item.locator}</small>}<em>{citationsCopy.source} ↗</em></a></div></li>)}</ol></aside>}
          {article.corrections.length>0&&<section className="article-corrections" aria-labelledby="article-corrections-title"><h2 id="article-corrections-title">{correctionsCopy.heading}</h2><p>{correctionsCopy.summary}</p><ol>{article.corrections.map(item=><li key={item.id}><time dateTime={item.correctedAt}>{formatDate(item.correctedAt)}</time><h3>{item.summary}</h3><p>{item.details}</p></li>)}</ol></section>}
          {article.tags.length > 0 && (
            <footer className="article-taxonomy">
              {article.tags.map((tag) => (
                <Link key={tag.slug} href={`/${locale}/tags/${tag.slug}`}>
                  #{tag.name}
                </Link>
              ))}
            </footer>
          )}
          {publicSources.length > 0 && (
            <aside className="article-sources" id="article-sources">
              <h2>{articleCopy.sourceHeading}</h2>
              <p>{articleCopy.sourceSummary}</p>
              <ul>
                {publicSources.map((source) => (
                  <li key={source.url}>
                    <a
                      href={source.url}
                      rel="nofollow noopener noreferrer"
                      target="_blank"
                    >
                      {source.name}
                    </a>
                    <small><Link href={`/${locale}/sources/${source.host.replace(/^www\./, "")}`}>{source.host}</Link> <span aria-hidden="true">↗</span></small>
                  </li>
                ))}
              </ul>
            </aside>
          )}
          {related.length > 0 && <aside className="related-articles" aria-labelledby="related-title"><h2 id="related-title">{articleCopy.relatedHeading}</h2><div>{related.map(item=><article key={item.slug}>{item.cover && <Link className="related-article-cover" href={`/${locale}/articles/${item.slug}`} tabIndex={-1} aria-hidden="true"><Image src={item.cover.url} alt="" fill sizes="(max-width: 760px) calc(100vw - 40px), 370px" /></Link>}<div><p className="section-kicker">{item.type}</p><h3><Link href={`/${locale}/articles/${item.slug}`}>{item.title}</Link></h3><p>{item.summary}</p></div></article>)}</div></aside>}
        </article>
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{
            __html: JSON.stringify(structuredData).replace(/</g, "\\u003c"),
          }}
        />
        <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(breadcrumbData).replace(/</g, "\\u003c") }} />
      </main>
    </div>
  );
}
