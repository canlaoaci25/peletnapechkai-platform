"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";
import { LogoutButton } from "@/components/admin/logout-button";
import type { AdminSession } from "@/lib/admin-api";

const text = {
  "tr-TR": {
    overview: "Kontrol merkezi",
    contents: "İçerik modülü",
    allContents: "Tüm içerikler",
    create: "Yeni içerik",
    categories: "Kategoriler",
    tags: "Etiketler",
    library: "Medya ve sözlük",
    knowledge: "Bilgi kasası",
    users: "Kullanıcılar",
    site: "Siteyi görüntüle",
    workspace: "Yönetim alanı",
    light: "Açık tema",
    dark: "Koyu tema",
    logout: "Çıkış yap",
    collapse: "Menüyü daralt",
    expand: "Menüyü genişlet",
  },
  "en-US": {
    overview: "Control center",
    contents: "Content module",
    allContents: "All content",
    create: "New content",
    categories: "Categories",
    tags: "Tags",
    library: "Media and vocabulary",
    knowledge: "Knowledge vault",
    users: "Users",
    site: "View website",
    workspace: "Administration",
    light: "Light theme",
    dark: "Dark theme",
    logout: "Sign out",
    collapse: "Collapse menu",
    expand: "Expand menu",
  },
  "de-DE": {
    overview: "Kontrollzentrum",
    contents: "Inhaltsmodul",
    allContents: "Alle Inhalte",
    create: "Neuer Inhalt",
    categories: "Kategorien",
    tags: "Schlagwörter",
    library: "Medien und Vokabular",
    knowledge: "Wissensspeicher",
    users: "Benutzer",
    site: "Website ansehen",
    workspace: "Verwaltung",
    light: "Helles Design",
    dark: "Dunkles Design",
    logout: "Abmelden",
    collapse: "Menü einklappen",
    expand: "Menü ausklappen",
  },
} as const;

export function AdminFrame({
  locale,
  session,
  children,
}: {
  locale: keyof typeof text;
  session: AdminSession;
  children: React.ReactNode;
}) {
  const pathname = usePathname(),
    copy = text[locale];
  const [theme, setTheme] = useState<"dark" | "light">("dark");
  const [collapsed, setCollapsed] = useState(false);
  const contentRoute =
    pathname.startsWith(`/${locale}/admin/articles`) ||
    pathname.startsWith(`/${locale}/admin/library`);
  const [contentOpen, setContentOpen] = useState(contentRoute);
  useEffect(() => {
    const saved = localStorage.getItem("boecl-admin-theme"),
      savedMenu = localStorage.getItem("boecl-admin-sidebar");
    const timer = setTimeout(() => {
      if (saved === "light" || saved === "dark") setTheme(saved);
      setCollapsed(savedMenu === "collapsed");
      setContentOpen(
        contentRoute ||
          localStorage.getItem("boecl-content-module") !== "closed",
      );
    }, 0);
    return () => clearTimeout(timer);
  }, [contentRoute]);
  function toggleTheme() {
    const next = theme === "dark" ? "light" : "dark";
    setTheme(next);
    localStorage.setItem("boecl-admin-theme", next);
  }
  function toggleSidebar() {
    setCollapsed((value) => {
      const next = !value;
      localStorage.setItem(
        "boecl-admin-sidebar",
        next ? "collapsed" : "expanded",
      );
      return next;
    });
  }
  function toggleContent() {
    setContentOpen((value) => {
      const next = !value;
      localStorage.setItem("boecl-content-module", next ? "open" : "closed");
      return next;
    });
  }
  const editorial = session.roles.some((role) =>
      ["Owner", "Admin", "Editor"].includes(role),
    ),
    admin = session.roles.some((role) => ["Owner", "Admin"].includes(role));
  const exact = (href: string) => pathname === href;
  const item = (href: string, active: boolean, icon: string, label: string) => (
    <Link
      className={active ? "active" : ""}
      href={href}
      title={collapsed ? label : undefined}
    >
      <span aria-hidden>{icon}</span>
      <span className="nav-label">{label}</span>
    </Link>
  );
  return (
    <div
      className="admin-frame"
      data-admin-theme={theme}
      data-sidebar={collapsed ? "collapsed" : "expanded"}
    >
      <aside className="admin-sidebar">
        <header>
          <Link className="admin-sidebar-brand" href={`/${locale}/admin`}>
            <span className="brand-mark">B</span>
            <span className="brand-label">
              <strong>BOECL</strong>
              <small>{copy.workspace}</small>
            </span>
          </Link>
          <button
            className="sidebar-collapse-button"
            type="button"
            onClick={toggleSidebar}
            aria-label={collapsed ? copy.expand : copy.collapse}
            title={collapsed ? copy.expand : copy.collapse}
          >
            {collapsed ? "→" : "←"}
          </button>
        </header>
        <nav aria-label={copy.workspace}>
          {item(
            `/${locale}/admin`,
            exact(`/${locale}/admin`),
            "⌂",
            copy.overview,
          )}
          <section
            className="admin-nav-module"
            aria-label={copy.contents}
            data-open={contentOpen}
          >
            <button
              className="admin-nav-module-title"
              type="button"
              onClick={toggleContent}
              aria-expanded={contentOpen}
            >
              <span aria-hidden>▤</span>
              <span className="nav-label">{copy.contents}</span>
              <span className="module-chevron" aria-hidden>
                {contentOpen ? "−" : "+"}
              </span>
            </button>
            <div className="admin-nav-submenu">
              {item(
                `/${locale}/admin/articles`,
                exact(`/${locale}/admin/articles`) ||
                  Boolean(
                    pathname.match(/\/admin\/articles\/[0-9a-f-]{36}(?:\/|$)/i),
                  ),
                "≡",
                copy.allContents,
              )}
              {item(
                `/${locale}/admin/articles/new`,
                exact(`/${locale}/admin/articles/new`),
                "+",
                copy.create,
              )}
              {editorial &&
                item(
                  `/${locale}/admin/articles/categories`,
                  exact(`/${locale}/admin/articles/categories`),
                  "#",
                  copy.categories,
                )}
              {editorial &&
                item(
                  `/${locale}/admin/articles/tags`,
                  exact(`/${locale}/admin/articles/tags`),
                  "⌗",
                  copy.tags,
                )}
              {editorial &&
                item(
                  `/${locale}/admin/library`,
                  pathname.startsWith(`/${locale}/admin/library`),
                  "▧",
                  copy.library,
                )}
            </div>
          </section>
          {editorial &&
            item(
              `/${locale}/admin/knowledge`,
              pathname.startsWith(`/${locale}/admin/knowledge`),
              "◇",
              copy.knowledge,
            )}
          {admin &&
            item(
              `/${locale}/admin/users`,
              pathname.startsWith(`/${locale}/admin/users`),
              "♙",
              copy.users,
            )}
        </nav>
        <div className="admin-sidebar-bottom">
          <Link
            className="view-site-link"
            href={`/${locale}`}
            target="_blank"
            title={collapsed ? copy.site : undefined}
          >
            <span>↗</span>
            <span className="nav-label">{copy.site}</span>
          </Link>
          <button
            className="admin-theme-button"
            onClick={toggleTheme}
            type="button"
            title={
              collapsed
                ? theme === "dark"
                  ? copy.light
                  : copy.dark
                : undefined
            }
          >
            <span aria-hidden>{theme === "dark" ? "☀" : "◐"}</span>
            <span className="nav-label">
              {theme === "dark" ? copy.light : copy.dark}
            </span>
          </button>
          <div className="admin-profile">
            <span className="profile-avatar">
              {session.displayName.slice(0, 1).toLocaleUpperCase(locale)}
            </span>
            <span className="profile-label">
              <strong>{session.displayName}</strong>
              <small>{session.roles.join(" · ")}</small>
            </span>
          </div>
          <div className="sidebar-logout">
            <LogoutButton locale={locale} label={copy.logout} />
          </div>
        </div>
      </aside>
      <div className="admin-frame-content">{children}</div>
    </div>
  );
}
