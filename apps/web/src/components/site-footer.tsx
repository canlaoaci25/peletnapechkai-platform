import Link from "next/link";
import type { Locale } from "@/i18n/config";
import { legalLabels, legalSlugs } from "@/i18n/legal-copy";
import { sourceCopy } from "@/i18n/source-copy";

export function SiteFooter({ locale }: { locale: Locale }) {
  const labels = legalLabels[locale];
  return <footer className="legal-footer"><nav aria-label="Legal"><Link href={`/${locale}/sources`}>{sourceCopy[locale].title}</Link>{legalSlugs.map(slug => <Link key={slug} href={`/${locale}/legal/${slug}`}>{labels[slug]}</Link>)}</nav><small>© {new Date().getFullYear()} BOECL</small></footer>;
}
