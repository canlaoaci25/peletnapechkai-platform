export type ArticleSeoInput = {
  title: string;
  summary: string;
  seoDescription: string | null;
  publishedAt: string;
  updatedAt: string;
  locale: string;
  canonicalUrl: string;
  imageUrl?: string;
  categories: { name: string }[];
  tags: { name: string }[];
  authors: { displayName: string; url: string }[];
  sources: { name: string; url: string }[];
  publisher: { id: string; name: string; url: string };
};

export function getPublicSource(value: { name: string; url: string }) {
  try {
    const url = new URL(value.url);
    const ipv4 = url.hostname.match(/^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$/)?.slice(1).map(Number);
    const privateIpv4 = ipv4 && (ipv4.some((part) => part > 255) || ipv4[0] === 10 || ipv4[0] === 127 ||
      ipv4[0] === 0 || ipv4[0] >= 224 || (ipv4[0] === 169 && ipv4[1] === 254) ||
      (ipv4[0] === 172 && ipv4[1] >= 16 && ipv4[1] <= 31) || (ipv4[0] === 192 && ipv4[1] === 168));
    if ((url.protocol !== "https:" && url.protocol !== "http:") || url.username || url.password ||
      url.hostname === "localhost" || !url.hostname.includes(".") || privateIpv4) return null;
    return { name: value.name.trim(), url: url.href, host: url.hostname.replace(/^www\./, "") };
  } catch {
    return null;
  }
}

export function buildArticleStructuredData(input: ArticleSeoInput) {
  const citationMap = new Map<string, { "@type": string; name: string; url: string }>();
  for (const source of input.sources.map(getPublicSource)) {
    if (source?.name && !citationMap.has(source.url)) {
      citationMap.set(source.url, { "@type": "CreativeWork", name: source.name, url: source.url });
    }
  }
  const citations = [...citationMap.values()];
  const sections = [...new Set(input.categories.map((category) => category.name.trim()).filter(Boolean))];
  const keywords = [...new Set(input.tags.map((tag) => tag.name.trim()).filter(Boolean))];

  return {
    "@context": "https://schema.org",
    "@type": "Article",
    headline: input.title,
    description: input.seoDescription ?? input.summary,
    datePublished: input.publishedAt,
    dateModified: input.updatedAt,
    inLanguage: input.locale,
    image: input.imageUrl ? [input.imageUrl] : undefined,
    author: input.authors.map((author) => ({
      "@type": "Person",
      name: author.displayName,
      url: author.url,
    })),
    mainEntityOfPage: input.canonicalUrl,
    articleSection: sections.length > 0 ? sections : undefined,
    keywords: keywords.length > 0 ? keywords : undefined,
    citation: citations.length > 0 ? citations : undefined,
    publisher: {
      "@type": "Organization",
      "@id": input.publisher.id,
      name: input.publisher.name,
      url: input.publisher.url,
    },
  };
}
