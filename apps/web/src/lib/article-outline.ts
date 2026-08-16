export type ArticleOutlineItem = { id: string; label: string; level: 2 | 3 };

const headingPattern = /<h([23])(\s[^>]*)?>([\s\S]*?)<\/h\1>/gi;
const namedEntities: Record<string, string> = {
  amp: "&", apos: "'", gt: ">", lt: "<", nbsp: " ", quot: '"',
};

function plainText(value: string) {
  return value
    .replace(/<[^>]*>/g, "")
    .replace(/&(#x[\da-f]+|#\d+|[a-z]+);/gi, (entity, code: string) => {
      if (code[0] !== "#") return namedEntities[code.toLowerCase()] ?? entity;
      const numeric = code[1].toLowerCase() === "x"
        ? Number.parseInt(code.slice(2), 16)
        : Number.parseInt(code.slice(1), 10);
      return Number.isFinite(numeric) ? String.fromCodePoint(numeric) : entity;
    })
    .replace(/\s+/g, " ")
    .trim();
}

function headingId(label: string, index: number) {
  const slug = label
    .replaceAll("ı", "i")
    .replaceAll("İ", "I")
    .normalize("NFKD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLocaleLowerCase("en-US")
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 64);
  return slug || `section-${index + 1}`;
}

export function buildArticleOutline(html: string) {
  const outline: ArticleOutlineItem[] = [];
  const usedIds = new Map<string, number>();
  const bodyHtml = html.replace(headingPattern, (_heading, rawLevel: string, attributes: string | undefined, content: string) => {
    const label = plainText(content);
    if (!label) return `<h${rawLevel}${attributes ?? ""}>${content}</h${rawLevel}>`;
    const baseId = headingId(label, outline.length);
    const occurrence = (usedIds.get(baseId) ?? 0) + 1;
    usedIds.set(baseId, occurrence);
    const id = occurrence === 1 ? baseId : `${baseId}-${occurrence}`;
    const level = Number(rawLevel) as 2 | 3;
    outline.push({ id, label, level });
    return `<h${rawLevel}${attributes ?? ""} id="${id}" tabindex="-1">${content}</h${rawLevel}>`;
  });

  return { bodyHtml, outline };
}
