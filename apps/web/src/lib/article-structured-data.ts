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
  sources: { url: string }[];
  publisher: { id: string; name: string; url: string };
};

function isPublicWebUrl(value: string) {
  try {
    const url = new URL(value);
    return url.protocol === "https:" || url.protocol === "http:";
  } catch {
    return false;
  }
}

export function buildArticleStructuredData(input: ArticleSeoInput) {
  const citations = [...new Set(input.sources.map((source) => source.url).filter(isPublicWebUrl))];
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
