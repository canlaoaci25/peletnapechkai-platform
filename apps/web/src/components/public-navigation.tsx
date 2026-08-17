"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useId, useRef, useState } from "react";

import { AccountActions } from "@/components/account-actions";
import { ThemeToggle } from "@/components/theme-toggle";
import { siteConfig } from "@/config/site";
import { localeLabels, locales, type Locale } from "@/i18n/config";

type NavigationCopy = {
  home: string; menu: string; sections: string; search: string; account: string;
  language: string; translationUnavailable: string; latest: string; allTopics: string;
  publicationPromise: string; menuDescription: string; sources: string;
};

type Props = {
  locale: Locale;
  localeHrefs?: Partial<Record<Locale, string>>;
  homeActive: boolean;
  copy: NavigationCopy;
  categories: { slug: string; title: string; articleCount: number }[];
};

export function PublicNavigation({ locale, localeHrefs, homeActive, copy, categories }: Props) {
  const [open, setOpen] = useState(false);
  const pathname = usePathname();
  const drawerId = useId();
  const triggerRef = useRef<HTMLButtonElement>(null);
  const drawerRef = useRef<HTMLElement>(null);

  useEffect(() => {
    if (!open) return;
    const drawer = drawerRef.current;
    const focusable = () => Array.from(drawer?.querySelectorAll<HTMLElement>('a[href],button:not([disabled]),summary,[tabindex]:not([tabindex="-1"])') ?? []);
    const previousOverflow = document.body.style.overflow;
    const trigger = triggerRef.current;
    document.body.style.overflow = "hidden";
    focusable()[0]?.focus();
    function onKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") { event.preventDefault(); setOpen(false); return; }
      if (event.key !== "Tab") return;
      const items = focusable();
      if (!items.length) return;
      const first = items[0], last = items[items.length - 1];
      if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
      else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
    }
    document.addEventListener("keydown", onKeyDown);
    return () => { document.removeEventListener("keydown", onKeyDown); document.body.style.overflow = previousOverflow; trigger?.focus(); };
  }, [open]);

  const isCurrent = (href: string) => href === `/${locale}` ? homeActive || pathname === href : pathname === href || pathname.startsWith(`${href}/`);
  const mainLinks = [
    { href: `/${locale}`, label: copy.latest },
    { href: `/${locale}/topics`, label: copy.allTopics },
    { href: `/${locale}/sources`, label: copy.sources },
  ];

  return <>
    <header className="public-mobile-bar">
      <button ref={triggerRef} className="drawer-trigger" type="button" aria-expanded={open} aria-controls={drawerId} aria-label={copy.menu} onClick={() => setOpen(true)}>
        <span aria-hidden="true"><i/><i/><i/></span><b>{copy.menu}</b>
      </button>
      <Link className="mobile-brand" href={`/${locale}`}>{siteConfig.name}</Link>
      <Link className="mobile-search" href={`/${locale}/search`} aria-label={copy.search}><SearchIcon /></Link>
    </header>
    {open && <button className="drawer-backdrop" type="button" aria-label={copy.menu} onClick={() => setOpen(false)} />}
    <aside ref={drawerRef} id={drawerId} className="public-sidebar" data-open={open} aria-label={copy.sections} onClick={(event) => { if ((event.target as HTMLElement).closest("a")) setOpen(false); }}>
      <div className="sidebar-brand-row">
        <Link className="sidebar-brand" href={`/${locale}`} aria-label={`${siteConfig.name} — ${copy.home}`}><strong>{siteConfig.name}</strong><span>{copy.publicationPromise}</span></Link>
        <button className="drawer-close" type="button" aria-label={`${copy.menu} — ×`} onClick={() => setOpen(false)}>×</button>
      </div>
      <Link className="sidebar-search" href={`/${locale}/search`}><SearchIcon/><span>{copy.search}</span><kbd>/</kbd></Link>
      <nav className="sidebar-primary" aria-label={copy.sections}>
        {mainLinks.map((item, index) => <Link key={item.href} href={item.href} aria-current={isCurrent(item.href) ? "page" : undefined}><span>{String(index + 1).padStart(2,"0")}</span><strong>{item.label}</strong></Link>)}
      </nav>
      <div className="sidebar-section-heading"><span>{copy.sections}</span><small>{categories.length}</small></div>
      <nav className="sidebar-categories" aria-label={copy.sections}>
        {categories.map(category => { const href = `/${locale}/categories/${category.slug}`; return <Link key={category.slug} href={href} aria-current={isCurrent(href) ? "page" : undefined}><span>{category.title}</span><small>{category.articleCount}</small></Link>; })}
      </nav>
      <div className="sidebar-utilities">
        <div className="sidebar-account"><span>{copy.account}</span><AccountActions locale={locale}/></div>
        <div className="sidebar-preferences"><details><summary>{copy.language}<b>{locale.split("-")[0].toUpperCase()}</b></summary><nav aria-label={copy.language}>{locales.map(item => localeHrefs && !localeHrefs[item] ? <span key={item} aria-disabled="true">{localeLabels[item]}<small>{copy.translationUnavailable}</small></span> : <Link key={item} href={localeHrefs?.[item] ?? `/${item}`} hrefLang={item} aria-current={item === locale ? "page" : undefined}>{localeLabels[item]}</Link>)}</nav></details><ThemeToggle locale={locale}/></div>
      </div>
    </aside>
  </>;
}

function SearchIcon() { return <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="10.5" cy="10.5" r="6.5"/><path d="m15.5 15.5 5 5"/></svg>; }
