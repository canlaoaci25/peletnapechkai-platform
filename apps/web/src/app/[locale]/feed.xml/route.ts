import { siteConfig } from "@/config/site";
import { hasLocale } from "@/i18n/config";
import { getPublishedArticles } from "@/lib/public-api";
import { absoluteUrl } from "@/lib/site-url";

export const dynamic = "force-dynamic";
const escapeXml = (value: string) => value.replace(/[<>&'\"]/g, character => ({ "<": "&lt;", ">": "&gt;", "&": "&amp;", "'": "&apos;", '"': "&quot;" })[character]!);

export async function GET(_request: Request, { params }: RouteContext<"/[locale]/feed.xml">) {
  const { locale } = await params;
  if (!hasLocale(locale)) return new Response("Not found", { status: 404 });
  const items = await getPublishedArticles(locale);
  const xml = `<?xml version="1.0" encoding="UTF-8"?><rss version="2.0"><channel><title>${siteConfig.name} — ${locale}</title><link>${absoluteUrl(`/${locale}`)}</link><description>${siteConfig.name} publications</description><language>${locale}</language>${items.map(item => `<item><title>${escapeXml(item.title)}</title><link>${absoluteUrl(`/${locale}/articles/${item.slug}`)}</link><guid isPermaLink="true">${absoluteUrl(`/${locale}/articles/${item.slug}`)}</guid><description>${escapeXml(item.summary)}</description><pubDate>${new Date(item.publishedAt).toUTCString()}</pubDate></item>`).join("")}</channel></rss>`;
  return new Response(xml, { headers: { "content-type": "application/rss+xml; charset=utf-8", "cache-control": "public, max-age=300" } });
}
