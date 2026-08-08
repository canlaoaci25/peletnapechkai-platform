import "server-only";

export type PublicArticleSummary = {
  slug: string; title: string; summary: string; type: string; publishedAt: string; updatedAt: string;
};
export type PublicArticle = PublicArticleSummary & { body: string; seoTitle: string | null; seoDescription: string | null };

const apiBaseUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5267";

async function publicGet<T>(path: string): Promise<T | null> {
  try {
    const response = await fetch(new URL(path, apiBaseUrl), { cache: "no-store" });
    if (response.status === 404) return null;
    if (!response.ok) throw new Error(`Public API request failed (${response.status}).`);
    return await response.json() as T;
  } catch { return null; }
}

export async function getPublishedArticles(locale: string) {
  return await publicGet<PublicArticleSummary[]>(`/api/v1/public/${encodeURIComponent(locale)}/articles`) ?? [];
}

export function getPublishedArticle(locale: string, slug: string) {
  return publicGet<PublicArticle>(`/api/v1/public/${encodeURIComponent(locale)}/articles/${encodeURIComponent(slug)}`);
}
