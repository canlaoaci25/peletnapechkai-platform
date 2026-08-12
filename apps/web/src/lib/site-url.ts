const configuredSiteUrl = process.env.NEXT_PUBLIC_SITE_URL?.trim();

export const siteUrl = new URL(
  configuredSiteUrl ||
    (process.env.NODE_ENV === "production"
      ? "https://peletnapechkai.com"
      : "http://localhost:3000"),
);

export function absoluteUrl(path: string) {
  return new URL(path, siteUrl).toString();
}
