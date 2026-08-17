import type { MetadataRoute } from "next";
import { locales } from "@/i18n/config";
import { getPublicArchiveIndex, getPublishedArticles, getPublicSourceIndex } from "@/lib/public-api";
import { absoluteUrl } from "@/lib/site-url";

export const dynamic = "force-dynamic";

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const collections = await Promise.all(locales.map(async locale => ({ locale, articles: await getPublishedArticles(locale, 1000) })));
  const groups = new Map<string, Record<string, string>>();
  for (const { locale, articles } of collections) for (const article of articles) groups.set(article.articleGroupId, { ...(groups.get(article.articleGroupId) ?? {}), [locale]: absoluteUrl(`/${locale}/articles/${article.slug}`) });
  const homeLanguages = Object.fromEntries([...locales.map(item => [item, absoluteUrl(`/${item}`)] as const), ["x-default", absoluteUrl("/tr-TR")] as const]);
  const homes: MetadataRoute.Sitemap = locales.map(locale => ({ url: absoluteUrl(`/${locale}`), changeFrequency: "daily", priority: .9, alternates: { languages: homeLanguages } }));
  const topicLanguages = Object.fromEntries([...locales.map(item => [item, absoluteUrl(`/${item}/topics`)] as const), ["x-default", absoluteUrl("/tr-TR/topics")] as const]);
  const topics: MetadataRoute.Sitemap = locales.map(locale => ({ url: absoluteUrl(`/${locale}/topics`), changeFrequency: "daily", priority: .85, alternates: { languages: topicLanguages } }));
  const articles: MetadataRoute.Sitemap = collections.flatMap(({ locale, articles }) => articles.map(article => ({ url: absoluteUrl(`/${locale}/articles/${article.slug}`), lastModified: article.updatedAt, changeFrequency: "weekly" as const, priority: .7, alternates: { languages: groups.get(article.articleGroupId) ?? {} } })));
  const archiveCollections = await Promise.all(locales.map(async locale => ({ locale, index: await getPublicArchiveIndex(locale) })));
  const categoryLanguages = new Map<string, Record<string, string>>();
  for (const { locale, index } of archiveCollections) for (const category of index.categories) {
    const languages = categoryLanguages.get(category.translationKey) ?? {};
    languages[locale] = absoluteUrl(`/${locale}/categories/${category.slug}`);
    categoryLanguages.set(category.translationKey, languages);
  }
  for (const languages of categoryLanguages.values()) languages["x-default"] = languages["tr-TR"] ?? Object.values(languages)[0];
  const archives: MetadataRoute.Sitemap = archiveCollections.flatMap(({locale,index}) => [
    ...index.categories.map(item=>({path:`categories/${item.slug}`,languages:categoryLanguages.get(item.translationKey)})),
    ...index.tags.map(item=>({path:`tags/${item.slug}`,languages:undefined})),
    ...index.authors.map(item=>({path:`authors/${item.slug}`,languages:undefined})),
  ].map(item=>({url:absoluteUrl(`/${locale}/${item.path}`),changeFrequency:"weekly" as const,priority:.5,alternates:item.languages?{languages:item.languages}:undefined})));
  const sourceCollections=await Promise.all(locales.map(async locale=>({locale,index:await getPublicSourceIndex(locale)})));
  const sourceCenters:MetadataRoute.Sitemap=sourceCollections.flatMap(({locale,index})=>[{url:absoluteUrl(`/${locale}/sources`),changeFrequency:"weekly" as const,priority:.65},...index.sources.map(source=>({url:absoluteUrl(`/${locale}/sources/${source.domain}`),lastModified:source.latestCitationAt,changeFrequency:"weekly" as const,priority:.55}))]);
  return [...homes, ...topics, ...articles, ...archives, ...sourceCenters];
}
