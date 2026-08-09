"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import Swal from "sweetalert2";
import type { ArticleSummary } from "@/lib/admin-api";

const statuses: Record<string, string> = {
  Draft: "Taslak",
  InEditorialReview: "İnceleme bekliyor",
  InSeoReview: "Yayın bekliyor",
  Scheduled: "Planlandı",
};

export function PublicationQueue({
  locale,
  articles,
}: {
  locale: string;
  articles: ArticleSummary[];
}) {
  const router = useRouter();
  const [pendingId, setPendingId] = useState<string | null>(null);

  async function publish(article: ArticleSummary) {
    const decision = await Swal.fire({
      title: "Makale yayınlansın mı?",
      text: `“${article.title}” hemen yayına alınacak.`,
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Evet, yayınla",
      cancelButtonText: "Vazgeç",
      background: "#151922",
      color: "#f4f6fa",
      confirmButtonColor: "#ff7651",
    });
    if (!decision.isConfirmed) return;
    setPendingId(article.id);
    try {
      const csrf = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
      const { token } = (await csrf.json()) as { token: string };
      const response = await fetch(
        `/api/admin/articles/${article.id}/publish-direct`,
        {
          method: "POST",
          headers: { "x-csrf-token": token },
        },
      );
      if (!response.ok) throw new Error();
      await Swal.fire({
        title: "Makale yayınlandı",
        icon: "success",
        timer: 1200,
        showConfirmButton: false,
        background: "#151922",
        color: "#f4f6fa",
      });
      router.refresh();
    } catch {
      await Swal.fire({
        title: "Yayınlama başarısız",
        text: "Makale taslak olarak korunuyor.",
        icon: "error",
        background: "#151922",
        color: "#f4f6fa",
      });
    } finally {
      setPendingId(null);
    }
  }

  if (articles.length === 0)
    return (
      <section className="admin-panel admin-empty">
        <strong>Yayın bekleyen makale yok</strong>
        <span>Tüm içerikler yayınlanmış veya arşivlenmiş.</span>
      </section>
    );
  return (
    <section className="admin-panel admin-content-workspace">
      <div className="article-table" role="table">
        <div
          className="article-row publication-row article-row-head"
          role="row"
        >
          <span>Makale</span>
          <span>Durum</span>
          <span>Güncelleme</span>
          <span>İşlem</span>
        </div>
        {articles.map((article) => (
          <div
            className="article-row publication-row"
            role="row"
            key={article.id}
          >
            <span>
              <Link href={`/${locale}/admin/articles/${article.id}`}>
                <strong>{article.title}</strong>
              </Link>
              <small>
                {article.locale} · {article.type} · /{article.slug}
              </small>
            </span>
            <span>
              <span
                className={`status-badge status-${article.status.toLowerCase()}`}
              >
                {statuses[article.status] ?? article.status}
              </span>
            </span>
            <time dateTime={article.updatedAt}>
              {new Intl.DateTimeFormat("tr-TR", { dateStyle: "medium" }).format(
                new Date(article.updatedAt),
              )}
            </time>
            <button
              className="publication-button"
              type="button"
              disabled={pendingId === article.id}
              onClick={() => void publish(article)}
            >
              {pendingId === article.id ? "Yayınlanıyor…" : "Hemen yayınla"}
            </button>
          </div>
        ))}
      </div>
    </section>
  );
}
