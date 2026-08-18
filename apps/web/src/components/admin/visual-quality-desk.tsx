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
  providers: { id: string; kind: string; status: string; canSupplyCandidates: boolean; requiresEditorialReview: boolean; rightsMetadataRequired: boolean; reasonCode: string }[];
  batch: null | { id: string; status: string; totalItems: number; processed: number; remaining: number; successful: number; rejected: number; activeArticle: string | null; currentPhase: number; lastMessage: string | null; updatedAt: string; isStale: boolean; staleAfterMinutes: number };
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
    sectionPlan: { heading: string; headingLevel: number; purpose: string; visualType: string; typeReason: string; prompt: string; negativePrompt: string }[];
    visualTask: VisualTask | null;
  }[];
};

const providerCopy = {
  "tr-TR": { title:"Sağlayıcı sağlığı", lead:"Yalnız yapılandırması ve kullanım hakkı kapıları doğrulanan kaynaklar aday sağlayabilir. Dış sağlayıcılar owner etkinleştirmesi olmadan kapalı kalır.", ready:"Hazır", disabled:"Kapalı", reviewOnly:"Yalnız inceleme", candidate:"Aday sağlayabilir", gated:"Otomatik aday kapalı", review:"Editoryal inceleme zorunlu", rights:"Lisans ve kaynak zorunlu", names:{"editorial-library":"Editoryal medya kütüphanesi","official-source":"Resmî / doğrulanmış kaynak","licensed-stock":"Lisanslı stok","generative-ai":"Temsili AI üretimi"}, reasons:{"media-library-ready":"Yerel medya deposu erişilebilir","media-storage-missing":"Medya deposu erişilemiyor","editorial-ingest-required":"Editör, hak bilgisini doğrulayarak içeri aktarmalı","owner-activation-required":"Owner etkinleştirmesi ve sağlayıcı kararı gerekli","secure-endpoint-missing":"Güvenli sağlayıcı adresi eksik","credential-missing":"Korumalı kimlik bilgisi eksik","configuration-ready":"Korumalı yapılandırma hazır"} },
  "en-US": { title:"Provider health", lead:"Only sources with verified configuration and rights gates can supply candidates. External providers stay off until owner activation.", ready:"Ready", disabled:"Off", reviewOnly:"Review only", candidate:"Can supply candidates", gated:"Automatic candidates off", review:"Editorial review required", rights:"Licence and source required", names:{"editorial-library":"Editorial media library","official-source":"Official / verified source","licensed-stock":"Licensed stock","generative-ai":"Representative AI generation"}, reasons:{"media-library-ready":"Local media storage is available","media-storage-missing":"Media storage is unavailable","editorial-ingest-required":"An editor must verify rights during ingest","owner-activation-required":"Owner activation and provider decision required","secure-endpoint-missing":"Secure provider endpoint missing","credential-missing":"Protected credential missing","configuration-ready":"Protected configuration ready"} },
  "de-DE": { title:"Anbieterstatus", lead:"Nur Quellen mit geprüfter Konfiguration und Rechtekontrolle dürfen Kandidaten liefern. Externe Anbieter bleiben bis zur Freigabe deaktiviert.", ready:"Bereit", disabled:"Aus", reviewOnly:"Nur Prüfung", candidate:"Kann Kandidaten liefern", gated:"Automatische Kandidaten aus", review:"Redaktionelle Prüfung erforderlich", rights:"Lizenz und Quelle erforderlich", names:{"editorial-library":"Redaktionelle Medienbibliothek","official-source":"Offizielle / geprüfte Quelle","licensed-stock":"Lizenzierte Stockmedien","generative-ai":"Repräsentative KI-Erzeugung"}, reasons:{"media-library-ready":"Lokaler Medienspeicher ist verfügbar","media-storage-missing":"Medienspeicher ist nicht verfügbar","editorial-ingest-required":"Rechte müssen beim Import redaktionell geprüft werden","owner-activation-required":"Freigabe und Anbieterentscheidung erforderlich","secure-endpoint-missing":"Sicherer Anbieter-Endpunkt fehlt","credential-missing":"Geschützte Zugangsdaten fehlen","configuration-ready":"Geschützte Konfiguration bereit"} },
  "fr-FR": { title:"État des fournisseurs", lead:"Seules les sources dont la configuration et les droits sont vérifiés peuvent fournir des candidats. Les fournisseurs externes restent désactivés sans validation du propriétaire.", ready:"Prêt", disabled:"Désactivé", reviewOnly:"Révision seule", candidate:"Peut fournir des candidats", gated:"Candidats automatiques désactivés", review:"Révision éditoriale obligatoire", rights:"Licence et source obligatoires", names:{"editorial-library":"Médiathèque éditoriale","official-source":"Source officielle / vérifiée","licensed-stock":"Stock sous licence","generative-ai":"Génération IA représentative"}, reasons:{"media-library-ready":"Le stockage local est disponible","media-storage-missing":"Le stockage média est indisponible","editorial-ingest-required":"Un éditeur doit vérifier les droits à l’import","owner-activation-required":"Validation du propriétaire et choix du fournisseur requis","secure-endpoint-missing":"Point d’accès sécurisé manquant","credential-missing":"Identifiant protégé manquant","configuration-ready":"Configuration protégée prête"} },
} satisfies Record<Locale, {title:string;lead:string;ready:string;disabled:string;reviewOnly:string;candidate:string;gated:string;review:string;rights:string;names:Record<string,string>;reasons:Record<string,string>}>;

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
  attestationHelp: "Every visible claim below requires an identified editorial confirmation. Originality is measured by the server.",
  articleConfirmed: "The focal subject matches the full article",
  sectionConfirmed: "The scene directly matches the named section",
  localeConfirmed: "Geography and cultural details fit this edition",
  technicalConfirmed: "Physical and technical details are credible",
  textConfirmed: "I confirmed that the image has no text, logo, watermark, or fake UI",
  artifactConfirmed: "Hands, faces, objects, perspective and reflections have no visible artifacts",
  cropConfirmed: "The focal subject survives every public crop shown above",
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
  proofMatrix: "Public crop proof",
  proofHelp: "Check the focal subject in every crop used by the public edition.",
  heroCrop: "Article hero · 16:9",
  leadCrop: "Desktop lead",
  mobileCrop: "Mobile card · 1:1",
  atlasCrop: "Topic card · 4:3",
  feedCrop: "Latest feed · 16:10",
  sectionPlan: "Section art direction",
  sectionPlanHelp: "A bounded set of H2/H3 scenes selected from the full article; each brief must match its own section.",
  noSectionPlan: "This story has no substantial section that needs a separate body visual.",
};
const copy = {
  "tr-TR": {
    ...base,
    eyebrow: "GÖRSEL ÜRETİM SERVİSİ",
    title: "Görsel Yenileme Stüdyosu",
    attestation: "Editoryal kanıt",
    attestationHelp: "Aşağıdaki her görünür iddia, kimliği kaydedilen editör onayı ister. Özgünlük sunucuda ölçülür.",
    articleConfirmed: "Ana özne makalenin tamamıyla eşleşiyor",
    sectionConfirmed: "Sahne, belirtilen bölümle doğrudan eşleşiyor",
    localeConfirmed: "Coğrafya ve kültürel ayrıntılar bu yayın diline uygun",
    technicalConfirmed: "Fiziksel ve teknik ayrıntılar güvenilir",
    textConfirmed: "Görselde yazı, logo, filigran veya sahte arayüz olmadığını doğruladım",
    artifactConfirmed: "El, yüz, nesne, perspektif ve yansımalarda görünür bozukluk yok",
    cropConfirmed: "Ana özne yukarıdaki tüm public kırpmalarda korunuyor",
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
    sectionPlan: "Bölüm görsel yönetmenliği",
    sectionPlanHelp: "Tam makaleden seçilen sınırlı H2/H3 sahneleri; her brief yalnız kendi bölümüyle eşleşmelidir.",
    noSectionPlan: "Bu yazıda ayrı gövde görseli gerektiren yeterli bir bölüm bulunamadı.",
    proofMatrix: "Public kırpma kanıtı",
    proofHelp: "Ana öznenin yayında kullanılan her kadrajda eksiksiz kaldığını denetleyin.",
    heroCrop: "Makale kapağı · 16:9",
    leadCrop: "Masaüstü manşet",
    mobileCrop: "Mobil kart · 1:1",
    atlasCrop: "Konu kartı · 4:3",
    feedCrop: "Güncel akış · 16:10",
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
    attestationHelp: "Every visible claim below requires an identified editorial confirmation. Originality is measured by the server.",
    articleConfirmed: "The focal subject matches the full article",
    sectionConfirmed: "The scene directly matches the named section",
    localeConfirmed: "Geography and cultural details fit this edition",
    technicalConfirmed: "Physical and technical details are credible",
    textConfirmed: "I confirmed that the image has no text, logo, watermark, or fake UI",
    artifactConfirmed: "Hands, faces, objects, perspective and reflections have no visible artifacts",
    cropConfirmed: "The focal subject survives every public crop shown above",
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
    attestationHelp: "Jede sichtbare Aussage unten erfordert eine namentlich protokollierte redaktionelle Bestätigung. Die Originalität misst der Server.",
    articleConfirmed: "Das Hauptmotiv passt zum vollständigen Beitrag",
    sectionConfirmed: "Die Szene passt direkt zum genannten Abschnitt",
    localeConfirmed: "Geografie und kulturelle Details passen zu dieser Ausgabe",
    technicalConfirmed: "Physische und technische Details sind plausibel",
    textConfirmed: "Ich bestätige: kein Text, Logo, Wasserzeichen oder erfundenes UI",
    artifactConfirmed: "Hände, Gesichter, Objekte, Perspektive und Spiegelungen sind frei von sichtbaren Artefakten",
    cropConfirmed: "Das Hauptmotiv bleibt in allen oben gezeigten öffentlichen Zuschnitten erhalten",
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
    proofMatrix: "Nachweis öffentlicher Zuschnitte",
    proofHelp: "Prüfen Sie das Hauptmotiv in jedem Zuschnitt der öffentlichen Ausgabe.",
    heroCrop: "Artikelbild · 16:9",
    leadCrop: "Desktop-Aufmacher",
    mobileCrop: "Mobile Karte · 1:1",
    atlasCrop: "Themenkarte · 4:3",
    feedCrop: "Aktueller Feed · 16:10",
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
    attestationHelp: "Chaque affirmation visible ci-dessous exige une confirmation éditoriale identifiée. Le serveur mesure l’originalité.",
    articleConfirmed: "Le sujet principal correspond à l’article complet",
    sectionConfirmed: "La scène correspond directement à la section indiquée",
    localeConfirmed: "La géographie et les détails culturels conviennent à cette édition",
    technicalConfirmed: "Les détails physiques et techniques sont crédibles",
    textConfirmed: "Je confirme l’absence de texte, logo, filigrane ou fausse interface",
    artifactConfirmed: "Les mains, visages, objets, perspectives et reflets ne présentent aucun artefact visible",
    cropConfirmed: "Le sujet principal reste intact dans tous les cadrages publics ci-dessus",
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
    proofMatrix: "Preuve des recadrages publics",
    proofHelp: "Vérifiez le sujet principal dans chaque recadrage utilisé par l’édition publique.",
    heroCrop: "Image d’article · 16:9",
    leadCrop: "Une sur ordinateur",
    mobileCrop: "Carte mobile · 1:1",
    atlasCrop: "Carte thématique · 4:3",
    feedCrop: "Fil récent · 16:10",
    start: "Démarrer",
    pause: "Suspendre",
    resume: "Reprendre",
    cancel: "Annuler en sécurité",
  },
} satisfies Record<Locale, Record<string, string>>;

const recoveryCopy = {
  "tr-TR": { healthy: "Operasyon kalp atışı güncel", stale: "Operasyon kalp atışı bayat", staleHelp: "Güvenlik süresi içinde ilerleme kaydedilmedi. Devam etmeden önce kayıtlı checkpoint'ten yeniden kuyruğa alın.", lastUpdate: "Son güncelleme", recover: "Checkpoint'ten kurtar" },
  "en-US": { healthy: "Operation heartbeat is current", stale: "Operation heartbeat is stale", staleHelp: "No progress was recorded within the safety window. Requeue from the saved checkpoint before continuing.", lastUpdate: "Last update", recover: "Recover from checkpoint" },
  "de-DE": { healthy: "Der Operations-Heartbeat ist aktuell", stale: "Der Operations-Heartbeat ist veraltet", staleHelp: "Im Sicherheitsfenster wurde kein Fortschritt erfasst. Stellen Sie den gespeicherten Checkpoint wieder in die Warteschlange.", lastUpdate: "Letzte Aktualisierung", recover: "Vom Checkpoint wiederherstellen" },
  "fr-FR": { healthy: "Le signal de l’opération est à jour", stale: "Le signal de l’opération est périmé", staleHelp: "Aucune progression n’a été enregistrée dans la fenêtre de sécurité. Remettez le checkpoint enregistré en file.", lastUpdate: "Dernière mise à jour", recover: "Récupérer depuis le checkpoint" },
} satisfies Record<Locale, { healthy: string; stale: string; staleHelp: string; lastUpdate: string; recover: string }>;

export function VisualQualityDesk({
  locale,
  report,
}: {
  locale: Locale;
  report: VisualQualityReport;
}) {
  const c = copy[locale], r = recoveryCopy[locale], p = providerCopy[locale],
    providerNames: Record<string,string> = p.names,
    providerReasons: Record<string,string> = p.reasons,
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
  const cropProofs = [
    ["hero", c.heroCrop], ["lead", c.leadCrop], ["mobile", c.mobileCrop],
    ["atlas", c.atlasCrop], ["feed", c.feedCrop],
  ];
  function candidatePayload(id: string) {
    return {
      mediaAssetId: field(`media-${id}`),
      provider: field(`provider-${id}`),
      licenseName: field(`license-${id}`),
      attribution: field(`credit-${id}`),
      altText: field(`alt-${id}`),
      articleConfirmed: checked(`article-${id}`),
      sectionConfirmed: checked(`section-${id}`),
      localeConfirmed: checked(`locale-${id}`),
      technicalAccuracyConfirmed: checked(`technical-${id}`),
      textAndLogoFreeConfirmed: checked(`text-${id}`),
      artifactFreeConfirmed: checked(`artifact-${id}`),
      cropConfirmed: checked(`crop-${id}`),
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
      <section className="admin-panel visual-provider-health" aria-labelledby="visual-provider-title">
        <header><div><p className="section-kicker">API / PROVIDERS</p><h2 id="visual-provider-title">{p.title}</h2><p>{p.lead}</p></div><strong>{report.providers.filter(provider=>provider.canSupplyCandidates).length} / {report.providers.length}</strong></header>
        <div className="visual-provider-grid">
          {report.providers.map(provider=><article key={provider.id} data-status={provider.status}>
            <div><h3>{providerNames[provider.id] ?? provider.id}</h3><span>{provider.status==="ready"?p.ready:provider.status==="review-only"?p.reviewOnly:p.disabled}</span></div>
            <p>{providerReasons[provider.reasonCode] ?? provider.reasonCode}</p>
            <ul><li>{provider.canSupplyCandidates?p.candidate:p.gated}</li>{provider.requiresEditorialReview&&<li>{p.review}</li>}{provider.rightsMetadataRequired&&<li>{p.rights}</li>}</ul>
          </article>)}
        </div>
      </section>
      {report.batch && (
        <section className="admin-panel visual-operation" aria-label={c.operation}>
          <header>
            <div><p className="section-kicker">{c.operation}</p><h2>{report.batch.status}</h2><p>{report.batch.lastMessage}</p></div>
            <strong>{report.batch.processed} / {report.batch.totalItems}</strong>
          </header>
          <div className="visual-operation-meter" aria-hidden="true"><span style={{width: `${report.batch.totalItems ? report.batch.processed / report.batch.totalItems * 100 : 0}%`}} /></div>
          <div className={`visual-operation-health ${report.batch.isStale ? "is-stale" : "is-healthy"}`} role={report.batch.isStale ? "alert" : "status"}>
            <strong>{report.batch.isStale ? r.stale : r.healthy}</strong>
            <span>{r.lastUpdate}: {new Intl.DateTimeFormat(locale,{dateStyle:"medium",timeStyle:"short"}).format(new Date(report.batch.updatedAt))}</span>
            {report.batch.isStale && <p>{r.staleHelp}</p>}
          </div>
          <div className="visual-operation-stats">
            {[[c.processed,report.batch.processed],[c.remaining,report.batch.remaining],[c.successful,report.batch.successful],[c.rejectedCount,report.batch.rejected]].map(([label,value])=><article key={label}><small>{label}</small><strong>{value}</strong></article>)}
          </div>
          <p><b>{c.activeStory}:</b> {report.batch.activeArticle ?? c.noActiveStory}</p>
          <div className="visual-actions">
            {report.batch.status === "Queued" && <button type="button" disabled={Boolean(busy)} onClick={()=>void send(`batch/${report.batch!.id}/start`)}>{c.start}</button>}
            {report.batch.status === "Running" && <button type="button" disabled={Boolean(busy)} onClick={()=>void send(`batch/${report.batch!.id}/pause`)}>{c.pause}</button>}
            {report.batch.status === "Paused" && <button type="button" disabled={Boolean(busy)} onClick={()=>void send(`batch/${report.batch!.id}/resume`)}>{c.resume}</button>}
            {report.batch.isStale && <button type="button" disabled={Boolean(busy)} onClick={()=>void send(`batch/${report.batch!.id}/recover`)}>{r.recover}</button>}
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
                <details className="visual-section-plan">
                  <summary>{c.sectionPlan} · {item.sectionPlan.length}</summary>
                  <p>{c.sectionPlanHelp}</p>
                  {item.sectionPlan.length ? (
                    <ol>
                      {item.sectionPlan.map((section) => (
                        <li key={`${section.headingLevel}-${section.heading}`}>
                          <span>H{section.headingLevel}</span>
                          <div><strong>{section.heading}</strong><small>{section.visualType} · {section.typeReason}</small><p>{section.prompt}</p></div>
                        </li>
                      ))}
                    </ol>
                  ) : <p>{c.noSectionPlan}</p>}
                </details>
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
                        <div className="visual-public-proof">
                          <header><strong>{c.proofMatrix}</strong><p>{c.proofHelp}</p></header>
                          <div className="visual-proof-grid">
                            {cropProofs.map(([crop, label]) => (
                              <figure className={`visual-proof visual-proof-${crop}`} key={crop}>
                                <figcaption>{label}</figcaption>
                                <div className="visual-proof-pair">
                                  <div><small>{c.current}</small>{item.coverUrl && <Image src={item.coverUrl} alt="" fill sizes="240px" unoptimized />}</div>
                                  <div><small>{c.candidate}</small><Image src={item.visualTask!.candidateUrl!} alt={item.visualTask!.candidateAltText ?? ""} fill sizes="240px" unoptimized /></div>
                                </div>
                              </figure>
                            ))}
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
                          <label><input id={`article-${item.visualTask.id}`} type="checkbox" defaultChecked={item.visualTask.topicScore===100}/>{c.articleConfirmed}</label>
                          <label><input id={`section-${item.visualTask.id}`} type="checkbox" defaultChecked={item.visualTask.topicScore===100}/>{c.sectionConfirmed}</label>
                          <label><input id={`locale-${item.visualTask.id}`} type="checkbox" defaultChecked={item.visualTask.topicScore===100}/>{c.localeConfirmed}</label>
                          <label><input id={`technical-${item.visualTask.id}`} type="checkbox" defaultChecked={item.visualTask.topicScore===100}/>{c.technicalConfirmed}</label>
                          <label><input id={`text-${item.visualTask.id}`} type="checkbox" defaultChecked={item.visualTask.textSafetyScore===100}/>{c.textConfirmed}</label>
                          <label><input id={`artifact-${item.visualTask.id}`} type="checkbox" defaultChecked={item.visualTask.textSafetyScore===100}/>{c.artifactConfirmed}</label>
                          <label><input id={`crop-${item.visualTask.id}`} type="checkbox" defaultChecked={item.visualTask.cropScore!==null&&item.visualTask.cropScore>=80}/>{c.cropConfirmed}</label>
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
