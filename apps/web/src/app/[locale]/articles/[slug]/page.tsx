import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { ArticleEngagement } from "@/components/article-engagement";
import { siteConfig } from "@/config/site";
import { commercialCopy } from "@/i18n/commercial-copy";
import { hasLocale } from "@/i18n/config";
import { getDictionary } from "@/i18n/get-dictionary";
import { getPublishedArticle, getRelatedArticles } from "@/lib/public-api";
import { buildArticleStructuredData } from "@/lib/article-structured-data";
import { absoluteUrl } from "@/lib/site-url";

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
    commercial = commercialCopy[locale];
  const sourceTitle = {
    "tr-TR": "Kaynaklar",
    "en-US": "Sources",
    "de-DE": "Quellen",
    "fr-FR": "Sources",
  }[locale];
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
      <SiteHeader locale={locale} />
      <ArticleEngagement locale={locale} slug={slug} />
      <main className="article-page">
        <Link className="back-link" href={`/${locale}`}>
          ← {dictionary.navigation.home}
        </Link>
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
            {article.authors.length > 0 && (
              <p className="article-byline">
                {article.authors.map((author, index) => (
                  <span key={author.slug}>
                    {index > 0 && ", "}
                    <Link href={`/${locale}/authors/${author.slug}`}>
                      {author.displayName}
                    </Link>
                  </span>
                ))}
              </p>
            )}
            <time dateTime={article.publishedAt}>
              {new Intl.DateTimeFormat(locale, { dateStyle: "long" }).format(
                new Date(article.publishedAt),
              )}
            </time>
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
                sizes="(max-width: 820px) 100vw, 780px"
                priority
                unoptimized
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
          <div className="article-body">
            {isHtml ? <div className="rich-article-body" dangerouslySetInnerHTML={{ __html: article.body }} /> : <div className="rich-article-body" dangerouslySetInnerHTML={{ __html: markdownBodyToHtml(article.body) }} />}
          </div>
          {article.tags.length > 0 && (
            <footer className="article-taxonomy">
              {article.tags.map((tag) => (
                <Link key={tag.slug} href={`/${locale}/tags/${tag.slug}`}>
                  #{tag.name}
                </Link>
              ))}
            </footer>
          )}
          {article.sources.length > 0 && (
            <aside className="article-sources">
              <h2>{sourceTitle}</h2>
              <ul>
                {article.sources.map((source) => (
                  <li key={source.url}>
                    <a
                      href={source.url}
                      rel="nofollow noopener noreferrer"
                      target="_blank"
                    >
                      {source.name}
                    </a>
                  </li>
                ))}
              </ul>
            </aside>
          )}
          {related.length > 0 && <aside className="related-articles" aria-labelledby="related-title"><h2 id="related-title">{{"tr-TR":"İlgili içerikler","en-US":"Related stories","de-DE":"Ähnliche Artikel","fr-FR":"Articles associés"}[locale]}</h2><div>{related.map(item=><article key={item.slug}><p className="section-kicker">{item.type}</p><h3><Link href={`/${locale}/articles/${item.slug}`}>{item.title}</Link></h3><p>{item.summary}</p></article>)}</div></aside>}
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
