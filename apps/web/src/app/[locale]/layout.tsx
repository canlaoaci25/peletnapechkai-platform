import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { notFound } from "next/navigation";

import { getDictionary } from "@/i18n/get-dictionary";
import { hasLocale, locales, type Locale } from "@/i18n/config";
import { siteConfig } from "@/config/site";
import { siteUrl } from "@/lib/site-url";

import "../globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export function generateStaticParams() {
  return locales.map((locale) => ({ locale }));
}

export async function generateMetadata({
  params,
}: LayoutProps<"/[locale]">): Promise<Metadata> {
  const { locale } = await params;

  if (!hasLocale(locale)) {
    notFound();
  }

  const dictionary = await getDictionary(locale);

  return {
    metadataBase: siteUrl,
    title: dictionary.metadata.title,
    description: dictionary.metadata.description,
    alternates: {
      canonical: `/${locale}`,
      languages: Object.fromEntries(
        locales.map((supportedLocale) => [supportedLocale, `/${supportedLocale}`]),
      ),
      types: { "application/rss+xml": `/${locale}/feed.xml` },
    },
    openGraph: {
      type: "website",
      locale: locale.replace("-", "_"),
      title: dictionary.metadata.title,
      description: dictionary.metadata.description,
      url: `/${locale}`,
      siteName: siteConfig.name,
    },
  };
}

export default async function LocaleLayout({
  children,
  params,
}: LayoutProps<"/[locale]">) {
  const { locale } = await params;

  if (!hasLocale(locale)) {
    notFound();
  }

  const dictionary = await getDictionary(locale);

  return (
    <html lang={locale} className={`${geistSans.variable} ${geistMono.variable}`}>
      <body><a className="skip-link" href="#page-root">{dictionary.accessibility.skipToContent}</a><div id="page-root" tabIndex={-1}>{children}</div></body>
    </html>
  );
}

export type LocaleLayoutParams = { locale: Locale };
