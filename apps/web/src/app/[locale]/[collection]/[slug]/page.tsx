import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { archiveCopy } from "@/i18n/archive-copy";
import { hasLocale } from "@/i18n/config";
import { getDictionary } from "@/i18n/get-dictionary";
import { getPublicArchive } from "@/lib/public-api";

export const dynamic="force-dynamic";
const collections=["categories","tags","authors"] as const;
type Props={params:Promise<{locale:string;collection:string;slug:string}>};
function isCollection(value:string):value is typeof collections[number]{return collections.includes(value as typeof collections[number])}
export async function generateMetadata({params}:Props):Promise<Metadata>{const{locale,collection,slug}=await params;if(!hasLocale(locale)||!isCollection(collection))return{};const archive=await getPublicArchive(locale,collection,slug);if(!archive)return{};return{title:archive.title,description:archive.description??archiveCopy[locale].description,alternates:{canonical:`/${locale}/${collection}/${slug}`}}}
export default async function ArchivePage({params}:Props){const{locale,collection,slug}=await params;if(!hasLocale(locale)||!isCollection(collection))notFound();const[archive,dictionary]=await Promise.all([getPublicArchive(locale,collection,slug),getDictionary(locale)]);if(!archive)notFound();const copy=archiveCopy[locale];return <div className="site-shell"><SiteHeader locale={locale}/><main className="archive-page"><Link className="back-link" href={`/${locale}`}>← {dictionary.navigation.home}</Link><header><p className="section-kicker">{copy[collection]}</p><h1>{archive.title}</h1>{archive.description&&<p className="archive-description">{archive.description}</p>}</header>{archive.articles.length===0?<p className="muted">{copy.empty}</p>:<div className="article-cards">{archive.articles.map(article=><article className="public-card" key={article.slug}><p className="section-kicker">{article.type}</p><h2><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h2><p>{article.summary}</p><time dateTime={article.publishedAt}>{new Intl.DateTimeFormat(locale,{dateStyle:"long"}).format(new Date(article.publishedAt))}</time></article>)}</div>}</main></div>}
