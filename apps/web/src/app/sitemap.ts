import type { MetadataRoute } from "next";
import { locales } from "@/i18n/config";
import { getPublicArchiveIndex, getPublishedArticles } from "@/lib/public-api";
import { absoluteUrl } from "@/lib/site-url";

export const dynamic = "force-dynamic";

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const collections = await Promise.all(locales.map(async locale => ({ locale, articles: await getPublishedArticles(locale) })));
  const groups = new Map<string, Record<string, string>>();
  for (const { locale, articles } of collections) for (const article of articles) groups.set(article.articleGroupId, { ...(groups.get(article.articleGroupId) ?? {}), [locale]: absoluteUrl(`/${locale}/articles/${article.slug}`) });
  const homes: MetadataRoute.Sitemap = locales.map(locale => ({ url: absoluteUrl(`/${locale}`), changeFrequency: "daily", priority: .9, alternates: { languages: Object.fromEntries(locales.map(item => [item, absoluteUrl(`/${item}`)])) } }));
  const articles: MetadataRoute.Sitemap = collections.flatMap(({ locale, articles }) => articles.map(article => ({ url: absoluteUrl(`/${locale}/articles/${article.slug}`), lastModified: article.updatedAt, changeFrequency: "weekly" as const, priority: .7, alternates: { languages: groups.get(article.articleGroupId) ?? {} } })));
  const archiveCollections = await Promise.all(locales.map(async locale => ({ locale, index: await getPublicArchiveIndex(locale) })));
  const archives: MetadataRoute.Sitemap = archiveCollections.flatMap(({locale,index}) => ([...index.categories.map(item=>`categories/${item.slug}`),...index.tags.map(item=>`tags/${item.slug}`),...index.authors.map(item=>`authors/${item.slug}`)]).map(path=>({url:absoluteUrl(`/${locale}/${path}`),changeFrequency:"weekly" as const,priority:.5})));
  return [...homes, ...articles, ...archives];
}
