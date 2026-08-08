import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { siteConfig } from "@/config/site";
import { getDictionary } from "@/i18n/get-dictionary";
import { hasLocale } from "@/i18n/config";
import { getPublishedArticle } from "@/lib/public-api";

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: PageProps<"/[locale]/articles/[slug]">): Promise<Metadata> {
  const { locale, slug } = await params;
  if (!hasLocale(locale)) return {};
  const article = await getPublishedArticle(locale, slug);
  if (!article) return {};
  return { title: article.seoTitle ?? `${article.title} — ${siteConfig.name}`, description: article.seoDescription ?? article.summary, alternates: { canonical: `/${locale}/articles/${slug}` }, openGraph: { type: "article", title: article.seoTitle ?? article.title, description: article.seoDescription ?? article.summary, publishedTime: article.publishedAt, modifiedTime: article.updatedAt, url: `/${locale}/articles/${slug}`, siteName: siteConfig.name } };
}

export default async function ArticlePage({ params }: PageProps<"/[locale]/articles/[slug]">) {
  const { locale, slug } = await params;
  if (!hasLocale(locale)) notFound();
  const [article, dictionary] = await Promise.all([getPublishedArticle(locale, slug), getDictionary(locale)]);
  if (!article) notFound();
  const paragraphs = article.body.split(/\r?\n\s*\r?\n/).filter(Boolean);
  return <div className="site-shell"><SiteHeader locale={locale}/><main className="article-page"><Link className="back-link" href={`/${locale}`}>← {dictionary.navigation.home}</Link><article><header><p className="section-kicker">{article.type}</p><h1>{article.title}</h1><p className="article-lead">{article.summary}</p><time dateTime={article.publishedAt}>{new Intl.DateTimeFormat(locale,{dateStyle:"long"}).format(new Date(article.publishedAt))}</time></header><div className="article-body">{paragraphs.map((paragraph,index)=><p key={index}>{paragraph}</p>)}</div></article></main></div>;
}
