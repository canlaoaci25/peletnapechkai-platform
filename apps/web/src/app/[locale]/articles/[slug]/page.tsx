import type {Metadata} from "next";
import Image from "next/image";
import Link from "next/link";
import {notFound} from "next/navigation";
import {SiteHeader} from "@/components/site-header";
import {siteConfig} from "@/config/site";
import {commercialCopy} from "@/i18n/commercial-copy";
import {hasLocale} from "@/i18n/config";
import {getDictionary} from "@/i18n/get-dictionary";
import {getPublishedArticle} from "@/lib/public-api";
import {absoluteUrl} from "@/lib/site-url";

export const dynamic="force-dynamic";
export async function generateMetadata({params}:PageProps<"/[locale]/articles/[slug]">):Promise<Metadata>{
  const{locale,slug}=await params;if(!hasLocale(locale))return{};
  const article=await getPublishedArticle(locale,slug);if(!article)return{};
  const languages=Object.fromEntries(article.translations.map(item=>[item.locale,`/${item.locale}/articles/${item.slug}`]));
  const image=article.cover?absoluteUrl(article.cover.url):undefined;
  return{title:article.seoTitle??`${article.title} — ${siteConfig.name}`,description:article.seoDescription??article.summary,alternates:{canonical:`/${locale}/articles/${slug}`,languages},openGraph:{type:"article",title:article.seoTitle??article.title,description:article.seoDescription??article.summary,publishedTime:article.publishedAt,modifiedTime:article.updatedAt,url:`/${locale}/articles/${slug}`,siteName:siteConfig.name,images:image?[{url:image,alt:article.cover!.altText}]:undefined}};
}

export default async function ArticlePage({params}:PageProps<"/[locale]/articles/[slug]">){
  const{locale,slug}=await params;if(!hasLocale(locale))notFound();
  const[article,dictionary]=await Promise.all([getPublishedArticle(locale,slug),getDictionary(locale)]);if(!article)notFound();
  const paragraphs=article.body.split(/\r?\n\s*\r?\n/).filter(Boolean),commercial=commercialCopy[locale];
  const sourceTitle={"tr-TR":"Kaynaklar","en-US":"Sources","de-DE":"Quellen"}[locale];
  const structuredData={"@context":"https://schema.org","@type":"Article",headline:article.title,description:article.summary,datePublished:article.publishedAt,dateModified:article.updatedAt,inLanguage:locale,image:article.cover?[absoluteUrl(article.cover.url)]:undefined,author:article.authors.map(author=>({"@type":"Person",name:author.displayName,url:absoluteUrl(`/${locale}/authors/${author.slug}`)})),mainEntityOfPage:absoluteUrl(`/${locale}/articles/${slug}`),publisher:{"@type":"Organization",name:siteConfig.name}};
  return <div className="site-shell"><SiteHeader locale={locale}/><main className="article-page"><Link className="back-link" href={`/${locale}`}>← {dictionary.navigation.home}</Link><article>{(article.isSponsored||article.hasAffiliateLinks)&&<aside className="commercial-disclosure" aria-label={commercial.disclosure}>{article.isSponsored&&<strong>{commercial.sponsoredBy}: {article.sponsorName}</strong>}{article.hasAffiliateLinks&&<p>{commercial.affiliateNotice}</p>}</aside>}<header><p className="section-kicker">{article.type}</p><h1>{article.title}</h1><p className="article-lead">{article.summary}</p>{article.authors.length>0&&<p className="article-byline">{article.authors.map((author,index)=><span key={author.slug}>{index>0&&", "}<Link href={`/${locale}/authors/${author.slug}`}>{author.displayName}</Link></span>)}</p>}<time dateTime={article.publishedAt}>{new Intl.DateTimeFormat(locale,{dateStyle:"long"}).format(new Date(article.publishedAt))}</time>{article.categories.length>0&&<div className="article-taxonomy">{article.categories.map(category=><Link key={category.slug} href={`/${locale}/categories/${category.slug}`}>{category.name}</Link>)}</div>}</header>{article.cover&&<figure className="article-cover"><Image src={article.cover.url} alt={article.cover.altText} width={1200} height={675} sizes="(max-width: 820px) 100vw, 780px" priority unoptimized/>{(article.cover.caption||article.cover.credit)&&<figcaption>{article.cover.caption}{article.cover.caption&&article.cover.credit&&" — "}{article.cover.credit}</figcaption>}</figure>}<div className="article-body">{paragraphs.map((paragraph,index)=><p key={index}>{paragraph}</p>)}</div>{article.tags.length>0&&<footer className="article-taxonomy">{article.tags.map(tag=><Link key={tag.slug} href={`/${locale}/tags/${tag.slug}`}>#{tag.name}</Link>)}</footer>}{article.sources.length>0&&<aside className="article-sources"><h2>{sourceTitle}</h2><ul>{article.sources.map(source=><li key={source.url}><a href={source.url} rel="nofollow noopener noreferrer" target="_blank">{source.name}</a></li>)}</ul></aside>}</article><script type="application/ld+json" dangerouslySetInnerHTML={{__html:JSON.stringify(structuredData).replace(/</g,"\\u003c")}}/></main></div>;
}
