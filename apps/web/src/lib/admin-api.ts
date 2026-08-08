import "server-only";

import { cookies } from "next/headers";

export type AdminSession = {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
};

export type ArticleSummary = {
  id: string;
  articleGroupId: string;
  locale: string;
  type: string;
  slug: string;
  title: string;
  status: string;
  updatedAt: string;
  scheduledAt: string | null;
  publishedAt: string | null;
};

export type ArticleDetail = ArticleSummary & {
  summary: string;
  body: string;
  seoTitle: string | null;
  seoDescription: string | null;
  categoryIds:string[]; tagIds:string[]; authorIds:string[]; sourceIds:string[]; mediaAssetIds:string[];
  coverMediaAssetId:string|null; coverAltText:string|null; coverCaption:string|null; coverCredit:string|null;
  isSponsored:boolean; sponsorName:string|null; hasAffiliateLinks:boolean;
};

export type ManagedUser = {
  id: string; email: string; displayName: string; isActive: boolean;
  emailConfirmed: boolean; twoFactorEnabled: boolean; lockoutEnd: string | null; roles: string[];
};

export type SupportingLibrary = {
  categories: { id:string; locale:string; slug:string; name:string }[];
  tags: { id:string; locale:string; slug:string; name:string }[];
  authors: { id:string; slug:string; displayName:string }[];
  sources: { id:string; name:string; url:string }[];
};
export type MediaItem = { id:string; fileName:string; contentType:string; byteLength:number; createdAt:string };
export type SystemStatus={checkedAt:string;database:string;articles:number;published:number;users:number;mediaFiles:number;mediaBytes:number;diskFreeBytes:number};
export type KnowledgeLink={id:string;articleLocalizationId:string;articleTitle:string;articleSlug:string;purpose:string;note:string|null;reviewDueAt:string;lastVerifiedAt:string};
export type KnowledgeCandidate={id:string;locale:string;title:string;claim:string;sourceUrl:string;aiAssisted:boolean;status:string;createdAt:string;updatedAt:string;links:KnowledgeLink[]};
export type ArticleRevision={id:string;number:number;title:string;summary:string;body:string;createdByUserId:string|null;createdAt:string};

const apiBaseUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5267";

async function apiGet<T>(path: string): Promise<T | null> {
  const cookieStore = await cookies();
  const response = await fetch(new URL(path, apiBaseUrl), {
    headers: { cookie: cookieStore.toString() },
    cache: "no-store",
  });

  if (response.status === 401 || response.status === 403) return null;
  if (!response.ok) throw new Error(`Admin API request failed (${response.status}).`);
  return (await response.json()) as T;
}

export function getAdminSession() {
  return apiGet<AdminSession>("/api/v1/auth/session");
}

export async function getArticles() {
  return (await apiGet<ArticleSummary[]>("/api/v1/admin/articles/")) ?? [];
}

export function getArticle(id: string) {
  return apiGet<ArticleDetail>(`/api/v1/admin/articles/${encodeURIComponent(id)}`);
}
export async function getArticleRevisions(id:string){return(await apiGet<ArticleRevision[]>(`/api/v1/admin/articles/${encodeURIComponent(id)}/revisions`))??[]}

export async function getUsers() {
  return (await apiGet<ManagedUser[]>("/api/v1/admin/users/")) ?? [];
}

export async function getSupportingLibrary() {
  return (await apiGet<SupportingLibrary>("/api/v1/admin/supporting/")) ?? { categories:[], tags:[], authors:[], sources:[] };
}

export async function getMedia() {
  return (await apiGet<MediaItem[]>("/api/v1/admin/media/")) ?? [];
}
export function getSystemStatus(){return apiGet<SystemStatus>("/api/v1/admin/status")}
export async function getKnowledgeCandidates(){return(await apiGet<KnowledgeCandidate[]>("/api/v1/admin/knowledge/"))??[]}
