"use client";

import { useReportWebVitals } from "next/web-vitals";
import { hasOptionalConsent } from "@/lib/consent";

function routeTemplate(pathname: string) {
  const parts = pathname.split("/").filter(Boolean).slice(1);
  if (parts.length === 0) return "home";
  if (parts[0] === "search") return "search";
  if (["category", "categories", "tags", "series"].includes(parts[0])) return "category";
  if (parts[0] === "articles" || parts[0] === "article") return "article";
  return "other";
}

export function WebVitalsReporter({ locale }: { locale: string }) {
  useReportWebVitals((metric) => {
    if (!["LCP", "CLS", "INP"].includes(metric.name) || !hasOptionalConsent(localStorage)) return;
    const width = window.innerWidth;
    const payload = JSON.stringify({ locale, route: routeTemplate(location.pathname), viewport: width < 600 ? "mobile" : width < 1024 ? "tablet" : "desktop", metric: metric.name, value: metric.value });
    if (navigator.sendBeacon) navigator.sendBeacon("/api/web-vitals", new Blob([payload], { type: "application/json" }));
    else void fetch("/api/web-vitals", { method: "POST", headers: { "content-type": "application/json" }, body: payload, keepalive: true });
  });
  return null;
}
