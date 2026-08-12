import "server-only";

export type PublicArticleSummary = {
  articleGroupId: string; slug: string; title: string; summary: string; type: string; publishedAt: string; updatedAt: string; cover:{url:string;altText:string}|null;
};
export type PublicArticle = Omit<PublicArticleSummary, "articleGroupId"|"cover"> & { body: string; seoTitle: string | null; seoDescription: string | null; isSponsored:boolean; sponsorName:string|null; hasAffiliateLinks:boolean; cover:{url:string;altText:string;caption:string|null;credit:string|null}|null; categories:{slug:string;name:string}[]; tags:{slug:string;name:string}[]; authors:{slug:string;displayName:string}[]; sources:{name:string;url:string}[]; translations: { locale: string; slug: string }[] };
export type PublicArchive = { kind:string; slug:string; title:string; description:string|null; articles:PublicArticleSummary[] };
export type PublicArchiveIndex = { categories:{slug:string;title:string}[]; tags:{slug:string;title:string}[]; authors:{slug:string;title:string}[] };
export type PublicHomepage = { lead:PublicArticleSummary|null;secondary:PublicArticleSummary[];trending:PublicArticleSummary[];editors:PublicArticleSummary[];latest:PublicArticleSummary[];mode:"Automatic"|"Hybrid" };

const apiBaseUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5267";

async function publicGet<T>(path: string): Promise<T | null> {
  try {
    const response = await fetch(new URL(path, apiBaseUrl), { cache: "no-store" });
    if (response.status === 404) return null;
    if (!response.ok) throw new Error(`Public API request failed (${response.status}).`);
    return await response.json() as T;
  } catch { return null; }
}

export async function getPublishedArticles(locale: string, limit = 12) {
  return await publicGet<PublicArticleSummary[]>(`/api/v1/public/${encodeURIComponent(locale)}/articles?limit=${Math.min(Math.max(limit, 1), 1000)}`) ?? [];
}
export async function getPublicHomepage(locale:string){return await publicGet<PublicHomepage>(`/api/v1/public/${encodeURIComponent(locale)}/homepage`)??{lead:null,secondary:[],trending:[],editors:[],latest:[],mode:"Automatic"}}

export function getPublishedArticle(locale: string, slug: string) {
  return publicGet<PublicArticle>(`/api/v1/public/${encodeURIComponent(locale)}/articles/${encodeURIComponent(slug)}`);
}

export async function searchPublishedArticles(locale: string, query: string) {
  if (query.trim().length < 2) return [];
  return await publicGet<PublicArticleSummary[]>(`/api/v1/public/${encodeURIComponent(locale)}/articles/search?q=${encodeURIComponent(query.trim())}`) ?? [];
}

export function getPublicArchive(locale:string,kind:string,slug:string){return publicGet<PublicArchive>(`/api/v1/public/${encodeURIComponent(locale)}/archives/${encodeURIComponent(kind)}/${encodeURIComponent(slug)}`)}
export async function getPublicArchiveIndex(locale:string){return await publicGet<PublicArchiveIndex>(`/api/v1/public/${encodeURIComponent(locale)}/archives`)??{categories:[],tags:[],authors:[]}}
