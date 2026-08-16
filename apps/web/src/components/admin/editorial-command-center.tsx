"use client";
import Link from "next/link";
import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import type { EditorialCommandCenter } from "@/lib/admin-api";
const copy = {
  "tr-TR": {
    kicker: "GÜNLÜK EDİTORYAL ÇALIŞMA MASASI",
    title: "Bugün neye odaklanmalıyım?",
    intro:
      "Kendi sorumluluklarını önce gör, ekip kuyruğunu filtrele ve görev durumunu sayfadan ayrılmadan ilerlet.",
    mine: "Benim masam",
    team: "Ekip kuyruğu",
    all: "Tümü",
    overdue: "Geciken",
    soon: "48 saatte",
    review: "İncelemede",
    quality: "Eksik kalite kapısı",
    openTasks: "Açık görevim",
    empty: "Bu görünüm temiz",
    emptyBody: "Seçili filtrede bekleyen iş bulunmuyor.",
    open: "İçeriği aç",
    EditorialReview: "Editoryal inceleme",
    SeoReview: "SEO incelemesi",
    OverdueTask: "Gecikmiş görev",
    Task: "Atanmış görev",
    assigned: "Sorumlu",
    status: "Durum",
    Todo: "Başlanmadı",
    InProgress: "Çalışılıyor",
    Waiting: "Bekliyor",
    Completed: "Tamamlandı",
    updateError: "Görev durumu güncellenemedi.",
    priority: "Öncelik",
  },
  "en-US": {
    kicker: "DAILY EDITORIAL WORKSPACE",
    title: "What should I focus on today?",
    intro:
      "See your responsibilities first, filter the team queue, and move work forward without leaving the page.",
    mine: "My desk",
    team: "Team queue",
    all: "All",
    overdue: "Overdue",
    soon: "Due in 48h",
    review: "In review",
    quality: "Quality gates open",
    openTasks: "My open tasks",
    empty: "This view is clear",
    emptyBody: "There is no pending work in this filter.",
    open: "Open content",
    EditorialReview: "Editorial review",
    SeoReview: "SEO review",
    OverdueTask: "Overdue task",
    Task: "Assigned task",
    assigned: "Owner",
    status: "Status",
    Todo: "Not started",
    InProgress: "In progress",
    Waiting: "Waiting",
    Completed: "Completed",
    updateError: "Task status could not be updated.",
    priority: "Priority",
  },
  "de-DE": {
    kicker: "TÄGLICHER REDAKTIONSARBEITSPLATZ",
    title: "Worauf soll ich mich heute konzentrieren?",
    intro:
      "Eigene Aufgaben zuerst sehen, Teamliste filtern und den Status direkt aktualisieren.",
    mine: "Mein Arbeitsplatz",
    team: "Teamwarteschlange",
    all: "Alle",
    overdue: "Überfällig",
    soon: "In 48 Std.",
    review: "In Prüfung",
    quality: "Offene Qualitätsprüfungen",
    openTasks: "Meine offenen Aufgaben",
    empty: "Diese Ansicht ist leer",
    emptyBody: "Für diesen Filter gibt es keine offenen Arbeiten.",
    open: "Inhalt öffnen",
    EditorialReview: "Redaktionelle Prüfung",
    SeoReview: "SEO-Prüfung",
    OverdueTask: "Überfällige Aufgabe",
    Task: "Zugewiesene Aufgabe",
    assigned: "Verantwortlich",
    status: "Status",
    Todo: "Nicht begonnen",
    InProgress: "In Arbeit",
    Waiting: "Wartet",
    Completed: "Erledigt",
    updateError: "Aufgabenstatus konnte nicht aktualisiert werden.",
    priority: "Priorität",
  },
  "fr-FR": {
    kicker: "ESPACE ÉDITORIAL QUOTIDIEN",
    title: "Sur quoi dois-je me concentrer aujourd’hui ?",
    intro:
      "Affichez d’abord vos responsabilités, filtrez la file d’équipe et avancez sans quitter la page.",
    mine: "Mon espace",
    team: "File d’équipe",
    all: "Tout",
    overdue: "En retard",
    soon: "Sous 48 h",
    review: "En révision",
    quality: "Contrôles ouverts",
    openTasks: "Mes tâches ouvertes",
    empty: "Cette vue est vide",
    emptyBody: "Aucun travail en attente pour ce filtre.",
    open: "Ouvrir le contenu",
    EditorialReview: "Révision éditoriale",
    SeoReview: "Révision SEO",
    OverdueTask: "Tâche en retard",
    Task: "Tâche attribuée",
    assigned: "Responsable",
    status: "Statut",
    Todo: "À commencer",
    InProgress: "En cours",
    Waiting: "En attente",
    Completed: "Terminée",
    updateError: "Le statut n’a pas pu être mis à jour.",
    priority: "Priorité",
  },
} as const;
type Scope = "mine" | "team";
type Filter = "all" | "overdue" | "soon" | "review";
export function EditorialCommandCenterView({
  locale,
  data,
}: {
  locale: string;
  data: EditorialCommandCenter;
}) {
  const c = copy[locale as keyof typeof copy] ?? copy["tr-TR"],
    router = useRouter(),
    [scope, setScope] = useState<Scope>("mine"),
    [filter, setFilter] = useState<Filter>("all"),
    [busy, setBusy] = useState<string | null>(null),
    [error, setError] = useState("");
  const items = useMemo(
    () =>
      data.items.filter((item) => {
        if (scope === "mine" && !item.isMine) return false;
        if (filter === "overdue") return item.kind === "OverdueTask";
        if (filter === "soon")
          return (
            item.kind === "Task" &&
          new Date(item.dueAt) <=
          new Date(new Date(data.checkedAt).getTime() + 172800000)
          );
        if (filter === "review")
          return item.kind === "EditorialReview" || item.kind === "SeoReview";
        return true;
      }),
    [data.checkedAt, data.items, filter, scope],
  );
  async function update(
    item: EditorialCommandCenter["items"][number],
    status: string,
  ) {
    if (!item.taskId) return;
    setBusy(item.taskId);
    setError("");
    try {
      const csrf = await fetch("/api/admin/auth/csrf", { cache: "no-store" }),
        { token } = (await csrf.json()) as { token: string },
        response = await fetch(
          `/api/admin/articles/${item.articleId}/collaboration/tasks/${item.taskId}/status`,
          {
            method: "POST",
            headers: {
              "content-type": "application/json",
              "x-csrf-token": token,
            },
            body: JSON.stringify({ status }),
          },
        );
      if (!response.ok) throw new Error();
      router.refresh();
    } catch {
      setError(c.updateError);
    } finally {
      setBusy(null);
    }
  }
  const filters: [Filter, string, number][] = [
    [
      "all",
      c.all,
      scope === "mine" ? data.summary.personalOpen : data.items.length,
    ],
    [
      "overdue",
      c.overdue,
      scope === "mine" ? data.summary.personalOverdue : data.summary.overdue,
    ],
    [
      "soon",
      c.soon,
      scope === "mine" ? data.summary.personalDueSoon : data.summary.dueSoon,
    ],
    ["review", c.review, scope === "mine" ? 0 : data.summary.inReview],
  ];
  return (
    <section
      className="admin-panel editorial-command-center"
      aria-labelledby="editorial-command-title"
    >
      <header>
        <div>
          <p className="section-kicker">{c.kicker}</p>
          <h2 id="editorial-command-title">{c.title}</h2>
          <p>{c.intro}</p>
        </div>
        <time dateTime={data.checkedAt}>
          {new Intl.DateTimeFormat(locale, { timeStyle: "short" }).format(
            new Date(data.checkedAt),
          )}
        </time>
      </header>
      <div className="desk-scope" role="group" aria-label={c.title}>
        <button
          type="button"
          aria-pressed={scope === "mine"}
          onClick={() => setScope("mine")}
        >
          <strong>{data.summary.personalOpen}</strong>
          <span>{c.mine}</span>
        </button>
        <button
          type="button"
          aria-pressed={scope === "team"}
          onClick={() => setScope("team")}
        >
          <strong>{data.items.length}</strong>
          <span>{c.team}</span>
        </button>
      </div>
      <div className="command-summary">
        <article data-alert={data.summary.personalOverdue > 0}>
          <strong>{data.summary.personalOverdue}</strong>
          <span>{c.overdue}</span>
        </article>
        <article>
          <strong>{data.summary.personalDueSoon}</strong>
          <span>{c.soon}</span>
        </article>
        <article>
          <strong>{data.summary.personalOpen}</strong>
          <span>{c.openTasks}</span>
        </article>
        <article>
          <strong>{data.summary.incompleteQuality}</strong>
          <span>{c.quality}</span>
        </article>
      </div>
      <nav className="desk-filters" aria-label={c.title}>
        {filters.map(([value, label, count]) => (
          <button
            key={value}
            type="button"
            aria-pressed={filter === value}
            onClick={() => setFilter(value)}
          >
            {label}
            <b>{count}</b>
          </button>
        ))}
      </nav>
      {error && (
        <p className="desk-error" role="alert">
          {error}
        </p>
      )}
      {items.length === 0 ? (
        <div className="command-empty">
          <strong>{c.empty}</strong>
          <span>{c.emptyBody}</span>
        </div>
      ) : (
        <div className="command-list">
          {items.map((item, index) => (
            <article
              key={`${item.taskId ?? item.articleId}-${item.kind}`}
              data-kind={item.kind}
            >
              <div className="command-rank">
                {String(index + 1).padStart(2, "0")}
              </div>
              <div>
                <span className="command-kind">
                  {c[item.kind as keyof typeof c] ?? item.kind}
                </span>
                <h3>{item.taskTitle ?? item.title}</h3>
                {item.taskTitle && <p>{item.title}</p>}
                <small>
                  {item.locale}
                  {item.assignee ? ` · ${c.assigned}: ${item.assignee}` : ""}
                  {item.priority ? ` · ${c.priority}: ${item.priority}` : ""}
                </small>
              </div>
              <time dateTime={item.dueAt}>
                {new Intl.DateTimeFormat(locale, {
                  dateStyle: "medium",
                }).format(new Date(item.dueAt))}
              </time>
              <div className="desk-actions">
                {item.taskId && (
                  <label>
                    <span>{c.status}</span>
                    <select
                      value={item.status ?? "Todo"}
                      disabled={busy === item.taskId}
                      onChange={(event) =>
                        void update(item, event.target.value)
                      }
                    >
                      {["Todo", "InProgress", "Waiting", "Completed"].map(
                        (status) => (
                          <option key={status} value={status}>
                            {c[status as keyof typeof c]}
                          </option>
                        ),
                      )}
                    </select>
                  </label>
                )}
                <Link href={`/${locale}/admin/articles/${item.articleId}`}>
                  {c.open} →
                </Link>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
