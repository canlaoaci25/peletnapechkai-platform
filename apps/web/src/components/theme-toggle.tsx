"use client";

import { useEffect, useState } from "react";
import type { Locale } from "@/i18n/config";

type Theme = "light" | "dark";

const labels = {
  "tr-TR": { light: "Açık temaya geç", dark: "Koyu temaya geç" },
  "en-US": { light: "Switch to light theme", dark: "Switch to dark theme" },
  "de-DE": { light: "Zum hellen Design wechseln", dark: "Zum dunklen Design wechseln" },
  "fr-FR": { light: "Passer au thème clair", dark: "Passer au thème sombre" },
};

export function ThemeToggle({ locale }: { locale: Locale }) {
  const [theme, setTheme] = useState<Theme | null>(null);

  useEffect(() => {
    const applied = document.documentElement.dataset.theme;
    const timer = window.setTimeout(() => setTheme(applied === "dark" ? "dark" : "light"), 0);
    return () => window.clearTimeout(timer);
  }, []);

  useEffect(() => { if (theme) document.documentElement.setAttribute("data-theme", theme); }, [theme]);

  function toggle() {
    const next = theme === "dark" ? "light" : "dark";
    setTheme(next);
    localStorage.setItem("boecl-theme", next);
  }

  const nextTheme = theme === "dark" ? "light" : "dark";
  return <button className="theme-lamp header-theme" type="button" onClick={toggle} aria-label={labels[locale][nextTheme]} title={labels[locale][nextTheme]} aria-pressed={theme === "dark"}>
    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M9 18h6M10 22h4M8.2 14.5A6 6 0 1 1 15.8 14.5c-.8.7-1.3 1.4-1.5 2.5h-4.6c-.2-1.1-.7-1.8-1.5-2.5Z"/><path className="lamp-rays" d="M12 1V0M4.2 4.2l-.8-.8M19.8 4.2l.8-.8M2 11H1M23 11h-1"/></svg>
  </button>;
}
