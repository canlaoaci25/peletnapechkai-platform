import type { MetadataRoute } from "next";
import { siteUrl } from "@/lib/site-url";

export default function robots(): MetadataRoute.Robots {
  const staging = siteUrl.hostname.startsWith("staging.");
  return { rules: { userAgent: "*", allow: staging ? undefined : "/", disallow: staging ? "/" : ["/*/admin", "/api/"] }, sitemap: staging ? undefined : new URL("/sitemap.xml", siteUrl).toString(), host: staging ? undefined : siteUrl.origin };
}
