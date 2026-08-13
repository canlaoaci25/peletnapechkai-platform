import { NextRequest, NextResponse } from "next/server";

import { defaultLocale, hasLocale } from "@/i18n/config";

type LocaleDirectory = {
  defaultLocale: string;
  locales: Array<{ code: string; languageCode: string; region: string; countries: string[] }>;
};

type ArticleTranslationDirectory = {
  translations?: Array<{ locale: string; slug: string }>;
};

const localeCookie = "boecl-locale";
const apiBaseUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5267";

function clientIp(request: NextRequest) {
  const trustedRealIp = request.headers.get("x-real-ip")?.trim();
  if (trustedRealIp) return trustedRealIp.replace(/^::ffff:/, "");

  // IIS ARR appends the actual remote client to its forwarding chain. The first
  // item can be this server's public address, so use the last hop supplied by ARR.
  const forwardedChain = request.headers.get("x-forwarded-for")
    ?.split(",")
    .map(value => value.trim())
    .filter(Boolean);
  const forwarded = forwardedChain?.at(-1);
  return forwarded?.replace(/^::ffff:/, "") || null;
}

async function countryCode(request: NextRequest) {
  const header = ["cf-ipcountry", "x-vercel-ip-country", "x-country-code"]
    .map(name => request.headers.get(name)?.trim().toUpperCase())
    .find(value => value && /^[A-Z]{2}$/.test(value));
  if (header) return header;

  const ip = clientIp(request);
  if (!ip) return null;

  try {
    const response = await fetch(`https://api.country.is/${encodeURIComponent(ip)}`, {
      cache: "force-cache",
      signal: AbortSignal.timeout(1200),
    });
    if (!response.ok) return null;
    const result = await response.json() as { country?: string };
    return result.country && /^[A-Z]{2}$/.test(result.country) ? result.country : null;
  } catch {
    return null;
  }
}

async function localeDirectory(): Promise<LocaleDirectory | null> {
  try {
    const response = await fetch(new URL("/api/v1/locales", apiBaseUrl), {
      cache: "no-store",
      signal: AbortSignal.timeout(1200),
    });
    return response.ok ? await response.json() as LocaleDirectory : null;
  } catch {
    return null;
  }
}

function browserLocale(request: NextRequest, directory: LocaleDirectory) {
  const languages = request.headers.get("accept-language")
    ?.split(",")
    .map(part => part.split(";")[0].trim().toLowerCase()) ?? [];
  return directory.locales.find(locale => languages.some(language =>
    language === locale.code.toLowerCase() || language.split("-")[0] === locale.languageCode.toLowerCase()
  ))?.code;
}

async function translatedArticlePath(sourceLocale: string, slug: string, targetLocale: string) {
  try {
    const response = await fetch(new URL(`/api/v1/public/${sourceLocale}/articles/${encodeURIComponent(slug)}`, apiBaseUrl), {
      cache: "no-store",
      signal: AbortSignal.timeout(1200),
    });
    if (!response.ok) return null;
    const article = await response.json() as ArticleTranslationDirectory;
    const translation = article.translations?.find(item => item.locale === targetLocale);
    return translation ? `/${targetLocale}/articles/${translation.slug}` : null;
  } catch {
    return null;
  }
}

async function countryRedirectPath(pathname: string, sourceLocale: string, targetLocale: string) {
  const tail = pathname.slice(sourceLocale.length + 1);
  if (!tail || tail === "/") return `/${targetLocale}`;

  const articleMatch = tail.match(/^\/articles\/([^/]+)\/?$/);
  if (articleMatch) {
    return await translatedArticlePath(sourceLocale, decodeURIComponent(articleMatch[1]), targetLocale)
      ?? `/${targetLocale}`;
  }

  // Taxonomy slugs are localized and cannot safely be copied to another locale.
  // Send those visitors to the matching locale homepage instead of a false 404.
  if (/^\/(categories|tags|authors)\//.test(tail)) return `/${targetLocale}`;

  return `/${targetLocale}${tail}`;
}

export async function proxy(request: NextRequest) {
  const pathname = request.nextUrl.pathname;
  const firstSegment = pathname.split("/").filter(Boolean)[0];

  if (firstSegment && hasLocale(firstSegment)) {
    // Admin pages are an authenticated workspace and keep the operator's current UI
    // locale. Every public locale route follows the visitor's current country.
    if (!pathname.startsWith(`/${firstSegment}/admin`)) {
      const directory = await localeDirectory();
      const country = directory ? await countryCode(request) : null;
      const countryLocale = directory?.locales.find(locale => country && locale.countries.includes(country))?.code;
      if (countryLocale && hasLocale(countryLocale) && countryLocale !== firstSegment) {
        const destination = request.nextUrl.clone();
        destination.pathname = await countryRedirectPath(pathname, firstSegment, countryLocale);
        const redirect = NextResponse.redirect(destination);
        redirect.cookies.set(localeCookie, countryLocale, { maxAge: 60 * 60 * 24 * 365, sameSite: "lax", secure: true });
        return redirect;
      }
    }

    const response = NextResponse.next();
    response.cookies.set(localeCookie, firstSegment, { maxAge: 60 * 60 * 24 * 365, sameSite: "lax", secure: true });
    return response;
  }

  const directory = await localeDirectory();
  const enabled = new Set<string>(directory?.locales.map(locale => locale.code).filter(hasLocale) ?? []);
  const savedLocale = request.cookies.get(localeCookie)?.value;
  let selected: string | null = null;

  if (directory) {
    const country = await countryCode(request);
    selected = directory.locales.find(locale => country && locale.countries.includes(country))?.code
      ?? (savedLocale && enabled.has(savedLocale) ? savedLocale : null)
      ?? browserLocale(request, directory)
      ?? directory.defaultLocale;
  }

  const locale = selected && hasLocale(selected) ? selected : defaultLocale;

  const destination = request.nextUrl.clone();
  destination.pathname = `/${locale}${pathname === "/" ? "" : pathname}`;

  const response = NextResponse.redirect(destination);
  response.cookies.set(localeCookie, locale, { maxAge: 60 * 60 * 24 * 365, sameSite: "lax", secure: true });
  return response;
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)"],
};
