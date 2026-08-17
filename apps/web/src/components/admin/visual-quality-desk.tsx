"use client";
import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import type { Locale } from "@/i18n/config";

type VisualTask = {
  id: string;
  status: string;
  sectionContext: string;
  visualPurpose: string;
  visualType: string;
  proposedPrompt: string;
  negativePrompt: string;
  attemptCount: number;
  reviewerNote: string | null;
  updatedAt: string;
  candidateMediaAssetId: string | null;
  candidateUrl: string | null;
  provider: string | null;
  licenseName: string | null;
  attribution: string | null;
  candidateAltText: string | null;
  topicScore: number | null;
  textSafetyScore: number | null;
  cropScore: number | null;
  originalityScore: number | null;
  candidateEvidenceVersion: string | null;
  candidateAttestedAt: string | null;
  closestMediaAssetId: string | null;
  closestSimilarityPercent: number | null;
  closestMediaUrl: string | null;
  candidatePasses: boolean;
  promotedAt: string | null;
};
export type VisualQualityReport = {
  checkedAt: string;
  total: number;
  passing: number;
  needsReview: number;
  missingCover: number;
  textRisk: number;
  averageScore: number;
  queued: number;
  approved: number;
  rejected: number;
  batch: null | { id: string; status: string; totalItems: number; processed: number; remaining: number; successful: number; rejected: number; activeArticle: string | null; currentPhase: number; lastMessage: string | null; updatedAt: string };
  items: {
    id: string;
    locale: string;
    slug: string;
    title: string;
    publishedAt: string | null;
    score: number;
    grade: string;
    risks: string[];
    bodyImageCount: number;
    coverUrl: string | null;
    coverAltText: string | null;
    width: number | null;
    height: number | null;
    optimizedBytes: number | null;
    visualTask: VisualTask | null;
  }[];
};

const base = {
  coverMissing: "Missing cover",
  altMissing: "Missing alt text",
  topicMismatch: "Weak topic match",
  textRisk: "Text / logo risk",
  cropRisk: "Mobile crop risk",
  notOptimized: "Not optimized",
  oversized: "Oversized file",
  rightsMissing: "Missing rights metadata",
  bodyMissing: "Missing body visual",
  dimensionsUnknown: "Unknown dimensions",
  clean: "All automated gates clear",
  body: "body visuals",
  sync: "Queue risky stories",
  syncing: "Preparing queue…",
  context: "Section context",
  visualType: "Selected visual type",
  brief: "Original text-free design brief",
  negative: "Exclude from output",
  reviewAction: "Start review",
  approve: "Approve brief",
  reject: "Reject",
  retry: "Request new brief",
  note: "Editorial decision note",
  queued: "Persistent queue",
  approved: "Approved",
  candidateEvidence: "Candidate quality evidence",
  current: "CURRENT",
  candidate: "CANDIDATE",
  mediaId: "Media asset ID",
  provider: "Provider",
  license: "License",
  attribution: "Attribution / source",
  altText: "Natural alt text",
  topicGate: "Topic",
  textGate: "Text-free",
  cropGate: "Mobile crop",
  originalityGate: "Originality",
  similarityEvidence: "Automatic archive similarity",
  closestMatch: "closest archive match",
  submitCandidate: "Send candidate to gates",
  attestation: "Editorial evidence",
  attestationHelp: "Topic fit and text/logo safety require an identified editorial confirmation. Crop and originality are measured by the server.",
  topicConfirmed: "I confirmed that the image matches the article and section",
  textConfirmed: "I confirmed that the image has no text, logo, watermark, or fake UI",
  evidencePending: "Evidence not yet recorded",
  promoteCandidate: "Approve and publish",
  operation: "Archive renewal operation",
  processed: "Processed",
  remaining: "Remaining",
  successful: "Successful",
  rejectedCount: "Rejected",
  activeStory: "Active story",
  noActiveStory: "Waiting for the next story",
  start: "Start",
  pause: "Pause",
  resume: "Resume",
  cancel: "Cancel safely",
};
const copy = {
  "tr-TR": {
    ...base,
    eyebrow: "GÖRSEL ÜRETİM SERVİSİ",
    title: "Görsel Yenileme Stüdyosu",
    attestation: "Editoryal kanıt",
    attestationHelp: "Konu uyumu ile yazı/logo güvenliği, kimliği kaydedilen editör onayı ister. Kırpma ve özgünlük sunucuda ölçülür.",
    topicConfirmed: "Görselin makale ve bölümle eşleştiğini doğruladım",
    textConfirmed: "Görselde yazı, logo, filigran veya sahte arayüz olmadığını doğruladım",
    evidencePending: "Kanıt henüz kaydedilmedi",
    lead: "Her makaleyi tam metin ve bölüm bağlamıyla; konu eşleşmesi, yazısız tasarım, mobil kırpma, performans ve kullanım hakkı kapılarında tarar.",
    total: "Yayındaki makale",
    passing: "Kapıdan geçen",
    review: "İnceleme gerektiren",
    average: "Ortalama kalite",
    queue: "Öncelikli görsel kuyruğu",
    queueLead:
      "Her riskli kayıt tam metinden üretilmiş somut bir brief ile incelenir; sağlam görsel otomatik değiştirilmez.",
    open: "Makaleyi aç",
    edit: "Editörde incele",
    coverMissing: "Kapak eksik",
    altMissing: "Alt metin eksik",
    topicMismatch: "Konu eşleşmesi zayıf",
    textRisk: "Yazı / logo riski",
    cropRisk: "Mobil kırpma riski",
    notOptimized: "Optimize edilmemiş",
    oversized: "Dosya büyük",
    rightsMissing: "Kaynak / hak bilgisi eksik",
    bodyMissing: "Gövde görseli eksik",
    dimensionsUnknown: "Boyut bilinmiyor",
    clean: "Tüm otomatik kapılar temiz",
    body: "gövde görseli",
    sync: "Riskli makaleleri kuyruğa al",
    syncing: "Kuyruk hazırlanıyor…",
    context: "Bölüm bağlamı",
    visualType: "Seçilen görsel türü",
    brief: "Yazısız özgün tasarım briefi",
    negative: "Üretimden dışla",
    reviewAction: "İncelemeye al",
    approve: "Briefi onayla",
    reject: "Reddet",
    retry: "Yeni brief iste",
    note: "Editoryal karar notu",
    queued: "Kalıcı kuyruk",
    approved: "Onaylanan",
    candidateEvidence: "Aday görsel kalite kanıtı",
    current: "MEVCUT",
    candidate: "ADAY",
    mediaId: "Medya varlık kimliği",
    provider: "Sağlayıcı",
    license: "Lisans",
    attribution: "Atıf / kaynak",
    altText: "Doğal alt metin",
    topicGate: "Konu",
    textGate: "Yazısız",
    cropGate: "Mobil crop",
    originalityGate: "Özgünlük",
    similarityEvidence: "Otomatik arşiv benzerliği",
    closestMatch: "en yakın arşiv eşleşmesi",
    submitCandidate: "Adayı kapılara gönder",
    promoteCandidate: "Onayla ve yayına al",
    operation: "Arşiv yenileme operasyonu",
    processed: "İşlenen",
    remaining: "Kalan",
    successful: "Başarılı",
    rejectedCount: "Reddedilen",
    activeStory: "Aktif makale",
    noActiveStory: "Sıradaki makale bekleniyor",
    start: "Başlat",
    pause: "Duraklat",
    resume: "Devam ettir",
    cancel: "Güvenle iptal et",
  },
  "en-US": {
    ...base,
    eyebrow: "VISUAL GENERATION SERVICE",
    title: "Visual Renewal Studio",
    attestation: "Editorial evidence",
    attestationHelp: "Topic fit and text/logo safety require an identified editorial confirmation. Crop and originality are measured by the server.",
    topicConfirmed: "I confirmed that the image matches the article and section",
    textConfirmed: "I confirmed that the image has no text, logo, watermark, or fake UI",
    evidencePending: "Evidence not yet recorded",
    lead: "Audits every story using its full text and section context for topic fit, text-free design, mobile crop, performance, and rights.",
    total: "Published stories",
    passing: "Passing",
    review: "Needs review",
    average: "Average quality",
    queue: "Priority visual queue",
    queueLead:
      "Every risky story gets a concrete full-text brief; healthy visuals are never replaced automatically.",
    open: "Open story",
    edit: "Review in editor",
  },
  "de-DE": {
    ...base,
    eyebrow: "BILDERZEUGUNGSDIENST",
    title: "Studio für Bilderneuerung",
    attestation: "Redaktioneller Nachweis",
    attestationHelp: "Themenbezug sowie Text- und Logosicherheit erfordern eine namentlich protokollierte Bestätigung. Beschnitt und Originalität misst der Server.",
    topicConfirmed: "Ich bestätige den Bezug zu Beitrag und Abschnitt",
    textConfirmed: "Ich bestätige: kein Text, Logo, Wasserzeichen oder erfundenes UI",
    evidencePending: "Nachweis noch nicht erfasst",
    lead: "Prüft Beiträge mit Volltext und Abschnittskontext auf Themenbezug, textfreie Gestaltung, mobilen Beschnitt, Leistung und Rechte.",
    total: "Veröffentlichte Beiträge",
    passing: "Bestanden",
    review: "Zu prüfen",
    average: "Durchschnitt",
    queue: "Priorisierte Bildwarteschlange",
    queueLead:
      "Jeder riskante Beitrag erhält ein konkretes Briefing; gute Bilder werden nie automatisch ersetzt.",
    open: "Beitrag öffnen",
    edit: "Im Editor prüfen",
    similarityEvidence: "Automatischer Archivvergleich",
    closestMatch: "ähnlichster Archivtreffer",
    sync: "Riskante Beiträge einreihen",
    syncing: "Warteschlange wird erstellt…",
    context: "Abschnittskontext",
    visualType: "Ausgewählter Bildtyp",
    brief: "Originales textfreies Bildbriefing",
    negative: "Vom Ergebnis ausschließen",
    reviewAction: "Prüfung beginnen",
    approve: "Briefing freigeben",
    reject: "Ablehnen",
    retry: "Neues Briefing anfordern",
    note: "Redaktionelle Entscheidungsnotiz",
    queued: "Dauerhafte Warteschlange",
    approved: "Freigegeben",
    operation: "Archivweite Bilderneuerung",
    processed: "Bearbeitet",
    remaining: "Verbleibend",
    successful: "Erfolgreich",
    rejectedCount: "Abgelehnt",
    activeStory: "Aktiver Beitrag",
    noActiveStory: "Nächster Beitrag wartet",
    start: "Starten",
    pause: "Pausieren",
    resume: "Fortsetzen",
    cancel: "Sicher abbrechen",
  },
  "fr-FR": {
    ...base,
    eyebrow: "SERVICE DE GÉNÉRATION VISUELLE",
    title: "Studio de renouvellement visuel",
    attestation: "Preuve éditoriale",
    attestationHelp: "La pertinence et l’absence de texte ou logo exigent une confirmation éditoriale identifiée. Le serveur mesure le cadrage et l’originalité.",
    topicConfirmed: "Je confirme la pertinence pour l’article et la section",
    textConfirmed: "Je confirme l’absence de texte, logo, filigrane ou fausse interface",
    evidencePending: "Preuve pas encore enregistrée",
    lead: "Contrôle chaque article avec son texte intégral et sa section : pertinence, création sans texte, recadrage mobile, performance et droits.",
    total: "Articles publiés",
    passing: "Conformes",
    review: "À vérifier",
    average: "Qualité moyenne",
    queue: "File visuelle prioritaire",
    queueLead:
      "Chaque article à risque reçoit un brief concret ; une image saine n’est jamais remplacée automatiquement.",
    open: "Ouvrir l’article",
    edit: "Vérifier dans l’éditeur",
    similarityEvidence: "Similarité d’archive automatique",
    closestMatch: "correspondance d’archive la plus proche",
    sync: "Mettre les articles à risque en file",
    syncing: "Préparation de la file…",
    context: "Contexte de section",
    visualType: "Type visuel sélectionné",
    brief: "Brief visuel original sans texte",
    negative: "Exclure du résultat",
    reviewAction: "Commencer la vérification",
    approve: "Approuver le brief",
    reject: "Rejeter",
    retry: "Demander un nouveau brief",
    note: "Note de décision éditoriale",
    queued: "File persistante",
    approved: "Approuvés",
    operation: "Renouvellement visuel des archives",
    processed: "Traités",
    remaining: "Restants",
    successful: "Réussis",
    rejectedCount: "Rejetés",
    activeStory: "Article actif",
    noActiveStory: "En attente du prochain article",
    start: "Démarrer",
    pause: "Suspendre",
    resume: "Reprendre",
    cancel: "Annuler en sécurité",
  },
} satisfies Record<Locale, Record<string, string>>;

export function VisualQualityDesk({
  locale,
  report,
}: {
  locale: Locale;
  report: VisualQualityReport;
}) {
  const c = copy[locale],
    router = useRouter(),
    [busy, setBusy] = useState(""),
    [message, setMessage] = useState("");
  const labels: Record<string, string> = {
    "missing-cover": c.coverMissing,
    "missing-alt": c.altMissing,
    "topic-mismatch": c.topicMismatch,
    "text-risk": c.textRisk,
    "unsafe-crop": c.cropRisk,
    "not-optimized": c.notOptimized,
    oversized: c.oversized,
    "missing-rights": c.rightsMissing,
    "missing-body-visual": c.bodyMissing,
    "unknown-dimensions": c.dimensionsUnknown,
  };
  const field = (id: string) =>
    (document.getElementById(id) as HTMLInputElement)?.value;
  const checked = (id: string) =>
    (document.getElementById(id) as HTMLInputElement)?.checked;
  function candidatePayload(id: string) {
    return {
      mediaAssetId: field(`media-${id}`),
      provider: field(`provider-${id}`),
      licenseName: field(`license-${id}`),
      attribution: field(`credit-${id}`),
      altText: field(`alt-${id}`),
      topicConfirmed: checked(`topic-${id}`),
      textAndLogoFreeConfirmed: checked(`text-${id}`),
    };
  }
  async function send(
    path: string,
    data: string | Record<string, unknown> = {},
  ) {
    setBusy(path);
    setMessage("");
    try {
      const csrf = await fetch("/api/admin/auth/csrf", { cache: "no-store" }),
        { token } = (await csrf.json()) as { token: string };
      const response = await fetch(
        `/api/admin/automation/visual-quality/${path}`,
        {
          method: "POST",
          headers: {
            "content-type": "application/json",
            "x-csrf-token": token,
          },
          body: JSON.stringify(
            typeof data === "string" ? { note: data } : data,
          ),
        },
      );
      if (!response.ok) throw new Error();
      setMessage("✓");
      router.refresh();
    } catch {
      setMessage("Action failed");
    } finally {
      setBusy("");
    }
  }
  return (
    <>
      <header className="visual-desk-hero">
        <div>
          <p className="section-kicker">{c.eyebrow}</p>
          <h1>{c.title}</h1>
          <p>{c.lead}</p>
          <button
            type="button"
            disabled={Boolean(busy)}
            onClick={() => void send("queue")}
          >
            {busy === "queue" ? c.syncing : c.sync}
          </button>
          {message && <span role="status">{message}</span>}
        </div>
        <div
          className="visual-score-dial"
          aria-label={`${c.average}: ${report.averageScore}`}
        >
          <strong>{report.averageScore}</strong>
          <span>/ 100</span>
        </div>
      </header>
      {report.batch && (
        <section className="admin-panel visual-operation" aria-label={c.operation}>
          <header>
            <div><p className="section-kicker">{c.operation}</p><h2>{report.batch.status}</h2><p>{report.batch.lastMessage}</p></div>
            <strong>{report.batch.processed} / {report.batch.totalItems}</strong>
          </header>
          <div className="visual-operation-meter" aria-hidden="true"><span style={{width: `${report.batch.totalItems ? report.batch.processed / report.batch.totalItems * 100 : 0}%`}} /></div>
          <div className="visual-operation-stats">
            {[[c.processed,report.batch.processed],[c.remaining,report.batch.remaining],[c.successful,report.batch.successful],[c.rejectedCount,report.batch.rejected]].map(([label,value])=><article key={label}><small>{label}</small><strong>{value}</strong></article>)}
          </div>
          <p><b>{c.activeStory}:</b> {report.batch.activeArticle ?? c.noActiveStory}</p>
          <div className="visual-actions">
            {report.batch.status === "Queued" && <button type="button" disabled={Boolean(busy)} onClick={()=>void send(`batch/${report.batch!.id}/start`)}>{c.start}</button>}
            {report.batch.status === "Running" && <button type="button" disabled={Boolean(busy)} onClick={()=>void send(`batch/${report.batch!.id}/pause`)}>{c.pause}</button>}
            {report.batch.status === "Paused" && <button type="button" disabled={Boolean(busy)} onClick={()=>void send(`batch/${report.batch!.id}/resume`)}>{c.resume}</button>}
            {!["Completed","Cancelled"].includes(report.batch.status) && <button type="button" disabled={Boolean(busy)} onClick={()=>void send(`batch/${report.batch!.id}/cancel`)}>{c.cancel}</button>}
          </div>
        </section>
      )}
      <section className="visual-quality-summary" aria-label={c.title}>
        {[
          [c.total, report.total],
          [c.passing, report.passing],
          [c.queued, report.queued],
          [c.approved, report.approved],
        ].map(([label, value]) => (
          <article className="admin-panel" key={label}>
            <small>{label}</small>
            <strong>{value}</strong>
          </article>
        ))}
      </section>
      <section className="admin-panel visual-review-queue">
        <header>
          <div>
            <p className="section-kicker">{c.queue}</p>
            <h2>
              {report.needsReview} / {report.total}
            </h2>
            <p>{c.queueLead}</p>
          </div>
        </header>
        <div className="visual-review-grid">
          {report.items.map((item) => (
            <article key={item.id} data-grade={item.grade}>
              <div className="visual-review-preview">
                {item.coverUrl ? (
                  <Image
                    src={item.coverUrl}
                    alt=""
                    fill
                    sizes="(max-width: 700px) 100vw, 360px"
                    unoptimized
                  />
                ) : (
                  <span aria-hidden>∅</span>
                )}
                <b>{item.score}</b>
              </div>
              <div className="visual-review-copy">
                <div>
                  <span>
                    {item.locale} · {item.grade}
                  </span>
                  <small>
                    {item.width && item.height
                      ? `${item.width} × ${item.height}`
                      : "—"}{" "}
                    · {item.bodyImageCount} {c.body}
                  </small>
                </div>
                <h3>{item.title}</h3>
                <ul>
                  {item.risks.length ? (
                    item.risks.map((risk) => (
                      <li key={risk}>{labels[risk] ?? risk}</li>
                    ))
                  ) : (
                    <li className="quality-clean">{c.clean}</li>
                  )}
                </ul>
                {item.visualTask && (
                  <details className="visual-brief">
                    <summary>
                      {c.brief} · {item.visualTask.status}
                    </summary>
                    <strong>{c.context}</strong>
                    <p>{item.visualTask.sectionContext}</p>
                    <strong>{c.visualType}</strong>
                    <p className="visual-type-chip">{item.visualTask.visualType}</p>
                    <strong>{c.brief}</strong>
                    <p>{item.visualTask.proposedPrompt}</p>
                    <strong>{c.negative}</strong>
                    <p>{item.visualTask.negativePrompt}</p>
                    <div className="visual-candidate-form">
                      <h4>{c.candidateEvidence}</h4>
                      {item.visualTask.candidateUrl && (
                        <div className="visual-before-after">
                          <div>
                            <small>{c.current}</small>
                            {item.coverUrl && (
                              <Image
                                src={item.coverUrl}
                                alt=""
                                width={320}
                                height={180}
                                unoptimized
                              />
                            )}
                          </div>
                          <div>
                            <small>{c.candidate}</small>
                            <Image
                              src={item.visualTask.candidateUrl}
                              alt={item.visualTask.candidateAltText ?? ""}
                              width={320}
                              height={180}
                              unoptimized
                            />
                          </div>
                        </div>
                      )}
                      {item.visualTask.closestMediaUrl && (
                        <div className="visual-similarity-evidence">
                          <Image src={item.visualTask.closestMediaUrl} alt="" width={160} height={90} unoptimized />
                          <div><strong>{c.similarityEvidence}</strong><span>{item.visualTask.closestSimilarityPercent}% {c.closestMatch}</span><small>{c.originalityGate}: {item.visualTask.originalityScore}/100</small></div>
                        </div>
                      )}
                      <div className="visual-fields">
                        <input
                          id={`media-${item.visualTask.id}`}
                          aria-label={c.mediaId}
                          placeholder={c.mediaId}
                          defaultValue={
                            item.visualTask.candidateMediaAssetId ?? ""
                          }
                        />
                        <input
                          id={`provider-${item.visualTask.id}`}
                          aria-label={c.provider}
                          placeholder={c.provider}
                          defaultValue={
                            item.visualTask.provider ?? ""
                          }
                        />
                        <input
                          id={`license-${item.visualTask.id}`}
                          aria-label={c.license}
                          placeholder={c.license}
                          defaultValue={
                            item.visualTask.licenseName ?? ""
                          }
                        />
                        <input
                          id={`credit-${item.visualTask.id}`}
                          aria-label={c.attribution}
                          placeholder={c.attribution}
                          defaultValue={item.visualTask.attribution ?? ""}
                        />
                        <input
                          id={`alt-${item.visualTask.id}`}
                          aria-label={c.altText}
                          placeholder={c.altText}
                          defaultValue={item.visualTask.candidateAltText ?? ""}
                        />
                        <fieldset className="visual-attestation">
                          <legend>{c.attestation}</legend>
                          <p>{c.attestationHelp}</p>
                          <label><input id={`topic-${item.visualTask.id}`} type="checkbox" defaultChecked={item.visualTask.topicScore===100}/>{c.topicConfirmed}</label>
                          <label><input id={`text-${item.visualTask.id}`} type="checkbox" defaultChecked={item.visualTask.textSafetyScore===100}/>{c.textConfirmed}</label>
                          <small>{item.visualTask.candidateAttestedAt ? `${item.visualTask.candidateEvidenceVersion} · ${new Intl.DateTimeFormat(locale,{dateStyle:"medium",timeStyle:"short"}).format(new Date(item.visualTask.candidateAttestedAt))}` : c.evidencePending}</small>
                        </fieldset>
                        <output className="visual-originality-score"><span>{c.cropGate}</span><strong>{item.visualTask.cropScore ?? "—"}/100</strong><small>{c.similarityEvidence}</small></output>
                        <output className="visual-originality-score">
                          <span>{c.originalityGate}</span>
                          <strong>{item.visualTask.originalityScore ?? "—"}/100</strong>
                          <small>{c.similarityEvidence}</small>
                        </output>
                      </div>
                      <div className="visual-actions">
                        <button
                          type="button"
                          disabled={Boolean(busy)}
                          onClick={() =>
                            void send(
                              `${item.visualTask!.id}/candidate`,
                              candidatePayload(item.visualTask!.id),
                            )
                          }
                        >
                          {c.submitCandidate}
                        </button>
                        {item.visualTask.candidatePasses && (
                          <button
                            type="button"
                            disabled={Boolean(busy)}
                            onClick={() =>
                              void send(`${item.visualTask!.id}/promote`, {
                                note: (
                                  document.getElementById(
                                    `note-${item.visualTask!.id}`,
                                  ) as HTMLTextAreaElement
                                )?.value,
                              })
                            }
                          >
                            {c.promoteCandidate}
                          </button>
                        )}
                      </div>
                    </div>
                    <label>
                      {c.note}
                      <textarea
                        id={`note-${item.visualTask.id}`}
                        maxLength={1000}
                      />
                    </label>
                    <div className="visual-actions">
                      {[
                        ["review", c.reviewAction],
                        ["reject", c.reject],
                        ["retry", c.retry],
                      ].map(([action, label]) => (
                        <button
                          type="button"
                          key={action}
                          disabled={Boolean(busy)}
                          onClick={() =>
                            void send(
                              `${item.visualTask!.id}/${action}`,
                              (
                                document.getElementById(
                                  `note-${item.visualTask!.id}`,
                                ) as HTMLTextAreaElement
                              )?.value,
                            )
                          }
                        >
                          {label}
                        </button>
                      ))}
                    </div>
                  </details>
                )}
                <nav>
                  <Link
                    href={`/${item.locale}/articles/${item.slug}`}
                    target="_blank"
                  >
                    {c.open} ↗
                  </Link>
                  <Link href={`/${locale}/admin/articles/${item.id}`}>
                    {c.edit}
                  </Link>
                </nav>
              </div>
            </article>
          ))}
        </div>
      </section>
    </>
  );
}
