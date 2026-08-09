"use client";

import { useEffect, useState } from "react";
import Swal from "sweetalert2";

export type AutomationScan = {
  activeLocales: string[];
  publishedArticles: number;
  missingTranslations: number;
  seoCandidates: number;
  siteLanguageCandidates: number;
  reportCandidates: number;
  runnerEnabled: boolean;
};

export type AutomationJob = {
  id: string;
  type: string;
  status: string;
  targetLocales: string[];
  totalItems: number;
  completedItems: number;
  failedItems: number;
  currentPhase: number;
  lastMessage: string | null;
  createdAt: string;
  updatedAt: string;
  completedAt: string | null;
};

const cards = [
  {
    type: "ContentTranslation",
    title: "Otomatik içerik çevirisi",
    description: "Yayındaki içeriklerin eksik dil sürümlerini kalıcı fazlara böler.",
    count: (scan: AutomationScan) => scan.missingTranslations,
  },
  {
    type: "SeoLocalization",
    title: "Otomatik SEO yerelleştirmesi",
    description: "Eksik SEO başlıklarını ve açıklamalarını hedef dile göre hazırlar.",
    count: (scan: AutomationScan) => scan.seoCandidates,
  },
  {
    type: "SiteLocalization",
    title: "Otomatik site dili",
    description: "Arayüzdeki eksik yerelleştirme anahtarlarını hedef dillere hazırlar.",
    count: (scan: AutomationScan) => scan.siteLanguageCandidates,
  },
  {
    type: "SystemReport",
    title: "Otomatik sistem raporu",
    description: "İşleri, hataları ve tamamlanma durumunu tek raporda toplar.",
    count: (scan: AutomationScan) => scan.reportCandidates,
  },
] as const;

const jobNames: Record<string, string> = Object.fromEntries(
  cards.map((card) => [card.type, card.title]),
);
const statusNames: Record<string, string> = {
  Queued: "Kuyrukta",
  Running: "Çalışıyor",
  Paused: "Durduruldu",
  Completed: "Tamamlandı",
  Failed: "Hatalı",
  Cancelled: "İptal edildi",
};

export function AutomationCenter({
  initialScan,
  initialJobs,
}: {
  initialScan: AutomationScan;
  initialJobs: AutomationJob[];
}) {
  const [scan, setScan] = useState(initialScan);
  const [jobs, setJobs] = useState(initialJobs);
  const [busy, setBusy] = useState(false);

  async function refresh() {
    const [scanResponse, jobsResponse] = await Promise.all([
      fetch("/api/admin/automation/scan", { cache: "no-store" }),
      fetch("/api/admin/automation/", { cache: "no-store" }),
    ]);
    if (scanResponse.ok) setScan((await scanResponse.json()) as AutomationScan);
    if (jobsResponse.ok) setJobs((await jobsResponse.json()) as AutomationJob[]);
  }

  useEffect(() => {
    const timer = window.setInterval(() => void refresh(), 5000);
    return () => window.clearInterval(timer);
  }, []);

  async function post(path: string, body?: object) {
    const csrfResponse = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
    const { token } = (await csrfResponse.json()) as { token: string };
    const response = await fetch(`/api/admin/automation${path}`, {
      method: "POST",
      headers: { "content-type": "application/json", "x-csrf-token": token },
      body: body ? JSON.stringify(body) : undefined,
    });
    if (!response.ok) {
      const problem = (await response.json().catch(() => null)) as { message?: string } | null;
      throw new Error(problem?.message ?? "Toplu iş kaydedilemedi.");
    }
    await refresh();
  }

  async function start(type: string, count: number) {
    const result = await Swal.fire({
      title: "Toplu iş başlatılsın mı?",
      text: `${count} kayıt kalıcı kuyruğa eklenecek ve fazlar halinde çalışacak.`,
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Toplu çalıştır",
      cancelButtonText: "Vazgeç",
      background: "#151922",
      color: "#f4f6fa",
      confirmButtonColor: "#ff7651",
    });
    if (!result.isConfirmed) return;

    setBusy(true);
    try {
      await post("/", {
        type,
        targetLocales: scan.activeLocales.filter((locale) => locale !== "tr-TR"),
      });
    } catch (error) {
      await Swal.fire({
        title: "İş başlatılamadı",
        text: error instanceof Error ? error.message : "Beklenmeyen bir hata oluştu.",
        icon: "error",
        background: "#151922",
        color: "#f4f6fa",
      });
    } finally {
      setBusy(false);
    }
  }

  async function changeState(id: string, action: string) {
    setBusy(true);
    try {
      await post(`/${id}/${action}`);
    } catch (error) {
      await Swal.fire({
        title: "Durum değiştirilemedi",
        text: error instanceof Error ? error.message : "Beklenmeyen bir hata oluştu.",
        icon: "error",
        background: "#151922",
        color: "#f4f6fa",
      });
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <section className={scan.runnerEnabled ? "automation-runner-status ready" : "automation-runner-status"}>
        <span aria-hidden />
        <div>
          <strong>{scan.runnerEnabled ? "Codex worker hazır" : "Codex worker güvenli kurulum bekliyor"}</strong>
          <small>Kuyruk kalıcıdır; ekran kapatılsa bile işler ve ilerleme bilgisi kaybolmaz.</small>
        </div>
      </section>

      <section className="automation-cards" aria-label="Toplu iş türleri">
        {cards.map((card) => (
          <article className="admin-panel" key={card.type}>
            <span className="automation-icon" aria-hidden>✦</span>
            <h2>{card.title}</h2>
            <p>{card.description}</p>
            <strong>{card.count(scan)} iş</strong>
            <button
              disabled={busy || !scan.runnerEnabled || card.count(scan) === 0}
              title={scan.runnerEnabled ? undefined : "Codex worker etkinleştirildikten sonra kullanılabilir."}
              onClick={() => void start(card.type, card.count(scan))}
            >
              TÜMÜNÜ FAZLARLA ÇALIŞTIR
            </button>
          </article>
        ))}
      </section>

      <section className="admin-panel automation-jobs">
        <header>
          <div>
            <p className="section-kicker">OTOMATİK SİSTEM RAPORLARI</p>
            <h2>Son işler</h2>
          </div>
          <button onClick={() => void refresh()}>Yenile</button>
        </header>
        {jobs.length === 0 ? (
          <p className="muted">Henüz toplu iş yok.</p>
        ) : (
          jobs.map((job) => {
            const processed = job.completedItems + job.failedItems;
            const percent = job.totalItems ? Math.round((processed / job.totalItems) * 100) : 0;
            return (
              <article key={job.id}>
                <header>
                  <span>
                    <strong>{jobNames[job.type] ?? job.type}</strong>
                    <small>{job.targetLocales.join(", ") || "Sistem"} · Faz {job.currentPhase}</small>
                  </span>
                  <b>{statusNames[job.status] ?? job.status}</b>
                </header>
                <div
                  className="automation-progress"
                  role="progressbar"
                  aria-label={`${jobNames[job.type] ?? job.type} ilerlemesi`}
                  aria-valuemin={0}
                  aria-valuemax={100}
                  aria-valuenow={percent}
                >
                  <span style={{ width: `${percent}%` }} />
                </div>
                <footer>
                  <span>%{percent} · {job.completedItems}/{job.totalItems} tamamlandı · {job.failedItems} hata</span>
                  <nav aria-label="İş eylemleri">
                    {(["Queued", "Running"].includes(job.status)) && (
                      <button disabled={busy} onClick={() => void changeState(job.id, "pause")}>Durdur</button>
                    )}
                    {job.status === "Paused" && (
                      <button disabled={busy} onClick={() => void changeState(job.id, "resume")}>Devam et</button>
                    )}
                    {!(["Completed", "Cancelled"].includes(job.status)) && (
                      <button disabled={busy} onClick={() => void changeState(job.id, "cancel")}>İptal</button>
                    )}
                  </nav>
                </footer>
                {job.lastMessage && <p>{job.lastMessage}</p>}
              </article>
            );
          })
        )}
      </section>
    </>
  );
}
