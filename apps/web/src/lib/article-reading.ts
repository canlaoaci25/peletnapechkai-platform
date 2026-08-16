export function estimateReadingMinutes(body: string, wordsPerMinute = 220) {
  const text = body
    .replace(/<script[\s\S]*?<\/script>/gi, " ")
    .replace(/<style[\s\S]*?<\/style>/gi, " ")
    .replace(/<[^>]+>/g, " ")
    .replace(/&[a-zA-Z0-9#]+;/g, " ")
    .replace(/[#*_>`~\[\]()!-]/g, " ")
    .trim();
  if (!text) return 1;
  return Math.max(1, Math.ceil(text.split(/\s+/u).length / wordsPerMinute));
}

export function wasMeaningfullyUpdated(publishedAt: string, updatedAt: string) {
  return new Date(updatedAt).getTime() - new Date(publishedAt).getTime() >= 60 * 60 * 1000;
}
