import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/site-header";
import { hasLocale, locales } from "@/i18n/config";
import { sourceCopy } from "@/i18n/source-copy";
import { getPublicSourceArchive } from "@/lib/public-api";
import { buildAvailableLocaleAlternates } from "@/lib/discovery-alternates";
import { buildDiscoveryStructuredData } from "@/lib/discovery-structured-data";
import { absoluteUrl } from "@/lib/site-url";
import { breadcrumbLabels } from "@/i18n/accessibility-copy";
import { focalPointStyle } from "@/lib/focal-point";

type Props={params:Promise<{locale:string;domain:string}>};
export const dynamic="force-dynamic";
export async function generateMetadata({params}:Props):Promise<Metadata>{const {locale,domain}=await params;if(!hasLocale(locale))return{};const archives=await Promise.all(locales.map(async candidate=>({locale:candidate,archive:await getPublicSourceArchive(candidate,domain)})));const archive=archives.find(item=>item.locale===locale)?.archive;if(!archive)return{};const paths=Object.fromEntries(archives.filter(item=>item.archive).map(item=>[item.locale,`/${item.locale}/sources/${item.archive!.domain}`]));const copy=sourceCopy[locale];return{title:copy.archiveTitle(archive.domain),description:copy.archiveDescription(archive.domain),alternates:{canonical:`/${locale}/sources/${archive.domain}`,languages:buildAvailableLocaleAlternates(paths)}}}
export default async function SourceArchivePage({params}:Props){const {locale,domain}=await params;if(!hasLocale(locale))notFound();const archive=await getPublicSourceArchive(locale,domain);if(!archive)notFound();const copy=sourceCopy[locale];
const schema=buildDiscoveryStructuredData({type:"CollectionPage",title:copy.archiveTitle(archive.domain),description:copy.archiveDescription(archive.domain),url:absoluteUrl(`/${locale}/sources/${archive.domain}`),locale,breadcrumbs:[{name:copy.back,url:absoluteUrl(`/${locale}/sources`)},{name:archive.domain,url:absoluteUrl(`/${locale}/sources/${archive.domain}`)}],items:archive.articles.map(article=>({name:article.title,url:absoluteUrl(`/${locale}/articles/${article.slug}`)}))});
return <div className="site-shell"><SiteHeader locale={locale}/><main id="main-content" className="source-center source-archive">
<script type="application/ld+json" dangerouslySetInnerHTML={{__html:JSON.stringify(schema).replace(/</g,"\\u003c")}}/>
<nav
className="archive-breadcrumbs"
aria-label={breadcrumbLabels[locale]}>
<Link href={`/${locale}/sources`}>{copy.back}</Link><span aria-hidden="true">/</span><span aria-current="page">{archive.domain}</span></nav><header className="source-center-hero"><div><p className="section-kicker">{copy.eyebrow}</p><h1>{archive.domain}</h1><p>{copy.archiveDescription(archive.domain)}</p></div><dl><div><dt>{copy.articles}</dt><dd>{archive.articleCount}</dd></div><div><dt>{copy.citations}</dt><dd>{archive.citationCount}</dd></div></dl></header><aside className="source-name-band"><strong>{copy.sourceNames}</strong><p>{archive.names.join(" · ")}</p></aside><section className="source-story-stream" aria-labelledby="source-stories"><header><p className="section-kicker">BOECL</p><h2 id="source-stories">{copy.usedFor}</h2></header><div>{archive.articles.map((article,index)=><article className={index===0?"source-story-lead":"source-story-card"} key={article.slug}>{article.cover&&<Link className="source-story-image" href={`/${locale}/articles/${article.slug}`} tabIndex={-1} aria-hidden="true"><Image src={article.cover.url} alt="" fill sizes={index===0?"(max-width: 800px) 100vw, 58vw":"(max-width: 800px) 100vw, 30vw"} style={focalPointStyle(article.cover)}/></Link>}<div><p className="section-kicker">{article.type}</p><h3><Link href={`/${locale}/articles/${article.slug}`}>{article.title}</Link></h3><p>{article.summary}</p><time dateTime={article.publishedAt}>{new Intl.DateTimeFormat(locale,{dateStyle:"long"}).format(new Date(article.publishedAt))}</time></div></article>)}</div></section></main></div>}
