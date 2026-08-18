type Breadcrumb = { name: string; url: string };
type DiscoveryItem = { name: string; url: string };

export function buildDiscoveryStructuredData(input: { type: "CollectionPage" | "WebPage"; title: string; description?: string; url: string; locale: string; breadcrumbs: Breadcrumb[]; items?: DiscoveryItem[] }) {
  const page: Record<string, unknown> = { "@type": input.type, "@id": `${input.url}#page`, url: input.url, name: input.title, inLanguage: input.locale };
  if (input.description) page.description = input.description;
  if (input.items?.length) page.mainEntity = { "@type": "ItemList", numberOfItems: input.items.length, itemListElement: input.items.map((item, index) => ({ "@type": "ListItem", position: index + 1, name: item.name, url: item.url })) };
  return { "@context": "https://schema.org", "@graph": [page, { "@type": "BreadcrumbList", itemListElement: input.breadcrumbs.map((item, index) => ({ "@type": "ListItem", position: index + 1, name: item.name, item: item.url })) }] };
}
