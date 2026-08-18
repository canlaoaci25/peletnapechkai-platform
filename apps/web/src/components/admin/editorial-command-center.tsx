"use client";
import Link from "next/link";
import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import type { EditorialCommandCenter } from "@/lib/admin-api";
import { EditorialCalendar } from "./editorial-calendar";
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
    qualityFilter: "Kalite borcu", QualityGate: "Kalite kapısı", missing: "Eksik kontroller", freshnessFilter:"Tazelik borcu",FreshnessDebt:"Tazelik incelemesi",freshness:"Güncellenmesi gereken",freshnessReasons:"İnceleme nedenleri",ContentOverOneYear:"Bir yıldan uzun süredir güncellenmedi",ContentOverSixMonths:"Altı aydan uzun süredir güncellenmedi",SourcesUnreviewed:"Doğrulanmamış kaynak var",SourcesReviewStale:"Kaynak incelemesi altı aydan eski",
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
    capacity: "Ekip kapasitesi", capacityIntro: "Açık iş ve SLA riskini kişi bazında görün; yoğunluğu gecikme oluşmadan dengeleyin.", noOwner: "Sahipsiz iş", teamMembers: "Aktif ekip", reassign: "Sorumluyu değiştir", reassignError: "Görev yeniden atanamadı.", openShort: "açık", overdueShort: "gecikmiş", soonShort: "yaklaşan",
    bulkTitle:"Toplu iş yönetimi",bulkIntro:"Görevleri seçin, etkiyi görün ve tek işlemle güvenle yeniden dağıtın.",selected:"seçili görev",selectTask:"Görevi seç",selectVisible:"Görünen görevleri seç",target:"Yeni sorumlu",preview:"Değişiklikleri önizle",confirm:"Görevleri yeniden ata",cancel:"Vazgeç",undo:"Geri al",bulkSuccess:"görev yeniden atandı",undoSuccess:"Toplu atama geri alındı.",previewTitle:"İşlem önizlemesi",previewBody:"Aşağıdaki açık görevlerin sorumlusu değişecek. İçerik, durum ve teslim tarihi korunur.",bulkError:"Toplu atama tamamlanamadı; kuyruğu yenileyip tekrar deneyin.",undoError:"Atama geri alınamadı; görevlerden biri daha sonra değişmiş olabilir.",
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
    qualityFilter: "Quality debt", QualityGate: "Quality gate", missing: "Missing checks", freshnessFilter:"Freshness debt",FreshnessDebt:"Freshness review",freshness:"Needs updating",freshnessReasons:"Review reasons",ContentOverOneYear:"Not updated for over a year",ContentOverSixMonths:"Not updated for over six months",SourcesUnreviewed:"Contains unreviewed sources",SourcesReviewStale:"Source review is over six months old",
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
    capacity: "Team capacity", capacityIntro: "See open work and SLA risk by person; rebalance before delays grow.", noOwner: "Unowned work", teamMembers: "Active team", reassign: "Change owner", reassignError: "Task could not be reassigned.", openShort: "open", overdueShort: "overdue", soonShort: "due soon",
    bulkTitle:"Bulk work management",bulkIntro:"Select tasks, review the impact, and rebalance them safely in one action.",selected:"tasks selected",selectTask:"Select task",selectVisible:"Select visible tasks",target:"New owner",preview:"Preview changes",confirm:"Reassign tasks",cancel:"Cancel",undo:"Undo",bulkSuccess:"tasks reassigned",undoSuccess:"Bulk assignment was undone.",previewTitle:"Change preview",previewBody:"The owner of these open tasks will change. Content, status, and due dates stay intact.",bulkError:"Bulk assignment failed; refresh the queue and try again.",undoError:"Assignment could not be undone; a task may have changed afterwards.",
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
    qualityFilter: "Qualitätslücken", QualityGate: "Qualitätsprüfung", missing: "Fehlende Prüfungen", freshnessFilter:"Aktualitätsbedarf",FreshnessDebt:"Aktualitätsprüfung",freshness:"Zu aktualisieren",freshnessReasons:"Prüfgründe",ContentOverOneYear:"Seit über einem Jahr nicht aktualisiert",ContentOverSixMonths:"Seit über sechs Monaten nicht aktualisiert",SourcesUnreviewed:"Enthält ungeprüfte Quellen",SourcesReviewStale:"Quellenprüfung ist über sechs Monate alt",
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
    capacity: "Teamkapazität", capacityIntro: "Offene Arbeit und SLA-Risiken pro Person erkennen und rechtzeitig ausgleichen.", noOwner: "Nicht zugeordnet", teamMembers: "Aktives Team", reassign: "Verantwortung ändern", reassignError: "Aufgabe konnte nicht neu zugewiesen werden.", openShort: "offen", overdueShort: "überfällig", soonShort: "bald fällig",
    bulkTitle:"Mehrere Aufgaben verwalten",bulkIntro:"Aufgaben auswählen, Auswirkung prüfen und sicher gemeinsam neu verteilen.",selected:"Aufgaben ausgewählt",selectTask:"Aufgabe auswählen",selectVisible:"Sichtbare Aufgaben auswählen",target:"Neue verantwortliche Person",preview:"Änderungen prüfen",confirm:"Aufgaben neu zuweisen",cancel:"Abbrechen",undo:"Rückgängig",bulkSuccess:"Aufgaben neu zugewiesen",undoSuccess:"Die Sammelzuweisung wurde rückgängig gemacht.",previewTitle:"Änderungsvorschau",previewBody:"Verantwortung ändert sich; Inhalt, Status und Frist bleiben erhalten.",bulkError:"Die Sammelzuweisung ist fehlgeschlagen. Bitte Liste aktualisieren.",undoError:"Die Zuweisung konnte nicht rückgängig gemacht werden.",
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
    qualityFilter: "Dette qualité", QualityGate: "Contrôle qualité", missing: "Contrôles manquants", freshnessFilter:"Dette de fraîcheur",FreshnessDebt:"Révision de fraîcheur",freshness:"À actualiser",freshnessReasons:"Motifs de révision",ContentOverOneYear:"Non actualisé depuis plus d’un an",ContentOverSixMonths:"Non actualisé depuis plus de six mois",SourcesUnreviewed:"Contient des sources non vérifiées",SourcesReviewStale:"Vérification des sources de plus de six mois",
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
    capacity: "Capacité de l’équipe", capacityIntro: "Visualisez la charge et le risque SLA par personne, puis rééquilibrez avant les retards.", noOwner: "Sans responsable", teamMembers: "Équipe active", reassign: "Changer le responsable", reassignError: "La tâche n’a pas pu être réattribuée.", openShort: "ouvertes", overdueShort: "en retard", soonShort: "bientôt dues",
    bulkTitle:"Gestion groupée",bulkIntro:"Sélectionnez des tâches, vérifiez l’impact et redistribuez-les en une action sûre.",selected:"tâches sélectionnées",selectTask:"Sélectionner la tâche",selectVisible:"Sélectionner les tâches visibles",target:"Nouveau responsable",preview:"Prévisualiser",confirm:"Réattribuer les tâches",cancel:"Annuler",undo:"Annuler l’attribution",bulkSuccess:"tâches réattribuées",undoSuccess:"L’attribution groupée a été annulée.",previewTitle:"Aperçu des changements",previewBody:"Le responsable change; le contenu, le statut et l’échéance restent inchangés.",bulkError:"L’attribution groupée a échoué. Actualisez la file.",undoError:"Impossible d’annuler l’attribution.",
  },
} as const;
const performanceCopy = {
  "tr-TR": { kicker:"ÖLÇÜLEN ÜRETİM RİTMİ",title:"Ekip işi ne hızda tamamlıyor?",intro:"Yalnız gerçek tamamlanma kanıtı olan görevler ölçülür; eski kayıtlar tahmin edilmez.",days30:"Son 30 gün",days90:"Son 90 gün",sample:"ölçülen görev",onTime:"Zamanında",median:"Ortanca süre",p95:"P95 süre",trend:"13 haftalık tamamlanma",hours:"sa",insufficient:"Karar için henüz yeterli örnek yok",legacy:"eski tamamlanmış görev ölçüm dışında" },
  "en-US": { kicker:"MEASURED DELIVERY RHYTHM",title:"How quickly is the team completing work?",intro:"Only tasks with verified completion evidence are measured; legacy records are never estimated.",days30:"Last 30 days",days90:"Last 90 days",sample:"measured tasks",onTime:"On time",median:"Median cycle",p95:"P95 cycle",trend:"13-week completions",hours:"h",insufficient:"Not enough evidence for a decision yet",legacy:"legacy completed tasks excluded" },
  "de-DE": { kicker:"GEMESSENER ARBEITSRHYTHMUS",title:"Wie schnell schließt das Team Aufgaben ab?",intro:"Nur Aufgaben mit belegtem Abschluss werden gemessen; Altdaten werden nicht geschätzt.",days30:"Letzte 30 Tage",days90:"Letzte 90 Tage",sample:"gemessene Aufgaben",onTime:"Pünktlich",median:"Median",p95:"P95-Dauer",trend:"Abschlüsse in 13 Wochen",hours:"Std.",insufficient:"Noch nicht genügend Daten für eine Entscheidung",legacy:"alte abgeschlossene Aufgaben ausgeschlossen" },
  "fr-FR": { kicker:"RYTHME DE LIVRAISON MESURÉ",title:"À quelle vitesse l’équipe termine-t-elle le travail ?",intro:"Seules les tâches avec une preuve d’achèvement sont mesurées ; les anciennes données ne sont pas estimées.",days30:"30 derniers jours",days90:"90 derniers jours",sample:"tâches mesurées",onTime:"À temps",median:"Durée médiane",p95:"Durée P95",trend:"Livraisons sur 13 semaines",hours:"h",insufficient:"Pas encore assez de données pour décider",legacy:"anciennes tâches terminées exclues" },
} as const;
type Scope = "mine" | "team";
type Filter = "all" | "overdue" | "soon" | "review" | "quality" | "freshness";
const gateCopy={
  "tr-TR":{TitleAndSummary:"Başlık ve özet",SourcesVerified:"Kaynak doğrulama",AuthorAndTaxonomy:"Yazar ve kategori",SeoMetadata:"SEO metadata",CoverAccessibility:"Kapak ve alt metin",CommercialDisclosure:"Ticari açıklama",TranslationReviewed:"Çeviri incelemesi",LegalEditorialReview:"Hukuk/editoryal inceleme"},
  "en-US":{TitleAndSummary:"Title and summary",SourcesVerified:"Source verification",AuthorAndTaxonomy:"Author and taxonomy",SeoMetadata:"SEO metadata",CoverAccessibility:"Cover and alt text",CommercialDisclosure:"Commercial disclosure",TranslationReviewed:"Translation review",LegalEditorialReview:"Legal/editorial review"},
  "de-DE":{TitleAndSummary:"Titel und Zusammenfassung",SourcesVerified:"Quellenprüfung",AuthorAndTaxonomy:"Autor und Taxonomie",SeoMetadata:"SEO-Metadaten",CoverAccessibility:"Titelbild und Alternativtext",CommercialDisclosure:"Werbekennzeichnung",TranslationReviewed:"Übersetzungsprüfung",LegalEditorialReview:"Rechtliche/redaktionelle Prüfung"},
  "fr-FR":{TitleAndSummary:"Titre et résumé",SourcesVerified:"Vérification des sources",AuthorAndTaxonomy:"Auteur et taxonomie",SeoMetadata:"Métadonnées SEO",CoverAccessibility:"Image et texte alternatif",CommercialDisclosure:"Mention commerciale",TranslationReviewed:"Révision de traduction",LegalEditorialReview:"Révision juridique/éditoriale"},
} as const;
const freshnessWorkflowCopy={
  "tr-TR":{why:"Neden şimdi?",TrafficEvidenceUnavailable:"Trafik ölçümü yok; etki tahmin edilmedi",MeasuredReaderDemand:"Ölçülen okur ilgisi yüksek",SeoQualityOpen:"SEO kalite kapısı açık",take:"Revizyon görevini üstlen",error:"Revizyon görevi oluşturulamadı.",sourceOnly:"Revizyon akışı Türkçe kaynak yayından başlar."},
  "en-US":{why:"Why now?",TrafficEvidenceUnavailable:"No traffic measurement; impact was not estimated",MeasuredReaderDemand:"Measured reader demand is high",SeoQualityOpen:"SEO quality gate is open",take:"Take revision task",error:"The revision task could not be created.",sourceOnly:"Revision work starts from the Turkish source edition."},
  "de-DE":{why:"Warum jetzt?",TrafficEvidenceUnavailable:"Keine Verkehrsmessung; Wirkung wurde nicht geschätzt",MeasuredReaderDemand:"Gemessenes Leserinteresse ist hoch",SeoQualityOpen:"SEO-Qualitätsprüfung ist offen",take:"Revision übernehmen",error:"Revisionsaufgabe konnte nicht erstellt werden.",sourceOnly:"Revisionen beginnen mit der türkischen Quellausgabe."},
  "fr-FR":{why:"Pourquoi maintenant ?",TrafficEvidenceUnavailable:"Aucune mesure de trafic ; impact non estimé",MeasuredReaderDemand:"La demande mesurée des lecteurs est forte",SeoQualityOpen:"Le contrôle qualité SEO est ouvert",take:"Prendre la tâche de révision",error:"La tâche de révision n’a pas pu être créée.",sourceOnly:"La révision commence par l’édition source turque."},
} as const;
export function EditorialCommandCenterView({
  locale,
  data,
  canReassign,
}: {
  locale: string;
  data: EditorialCommandCenter;
  canReassign:boolean;
}) {
  const c = copy[locale as keyof typeof copy] ?? copy["tr-TR"],
    pc = performanceCopy[locale as keyof typeof performanceCopy] ?? performanceCopy["tr-TR"],
    fc = freshnessWorkflowCopy[locale as keyof typeof freshnessWorkflowCopy] ?? freshnessWorkflowCopy["tr-TR"],
    router = useRouter(),
    [scope, setScope] = useState<Scope>("mine"),
    [filter, setFilter] = useState<Filter>("all"),
    [busy, setBusy] = useState<string | null>(null),
    [error, setError] = useState(""),
    [selected,setSelected]=useState<Set<string>>(new Set()),
    [bulkOwner,setBulkOwner]=useState(""),
    [preview,setPreview]=useState(false),
    [notice,setNotice]=useState<{text:string;batchId?:string}|null>(null);
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
        if (filter === "quality") return item.kind === "QualityGate";
        if (filter === "freshness") return item.kind === "FreshnessDebt";
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
  async function reassign(item:EditorialCommandCenter["items"][number],assigneeUserId:string){
    if(!item.taskId)return;setBusy(item.taskId);setError("");
    try{const csrf=await fetch("/api/admin/auth/csrf",{cache:"no-store"}),{token}=await csrf.json() as {token:string};
      const response=await fetch(`/api/admin/editorial/tasks/${item.taskId}/assignee`,{method:"POST",headers:{"content-type":"application/json","x-csrf-token":token},body:JSON.stringify({assigneeUserId})});
      if(!response.ok)throw new Error();router.refresh();
    }catch{setError(c.reassignError)}finally{setBusy(null)}
  }
  async function createFreshnessTask(item:EditorialCommandCenter["items"][number]){
    setBusy(`freshness-${item.articleId}`);setError("");
    try{const csrf=await fetch("/api/admin/auth/csrf",{cache:"no-store"}),{token}=await csrf.json()as{token:string};const response=await fetch(`/api/admin/editorial/freshness/${item.articleId}/task`,{method:"POST",headers:{"x-csrf-token":token}});if(!response.ok)throw new Error();router.refresh()}catch{setError(fc.error)}finally{setBusy(null)}
  }
  function toggle(taskId:string){setSelected(current=>{const next=new Set(current);if(next.has(taskId))next.delete(taskId);else if(next.size<25)next.add(taskId);return next})}
  async function bulkReassign(){if(!bulkOwner||selected.size===0)return;setBusy("bulk");setError("");
    try{const csrf=await fetch("/api/admin/auth/csrf",{cache:"no-store"}),{token}=await csrf.json() as {token:string};
      const response=await fetch("/api/admin/editorial/tasks/bulk-assignee",{method:"POST",headers:{"content-type":"application/json","x-csrf-token":token},body:JSON.stringify({taskIds:[...selected],assigneeUserId:bulkOwner})});
      if(!response.ok)throw new Error();const result=await response.json() as {batchId:string|null;changed:number};setNotice({text:`${result.changed} ${c.bulkSuccess}`,...(result.batchId?{batchId:result.batchId}:{})});setSelected(new Set());setPreview(false);router.refresh();
    }catch{setError(c.bulkError)}finally{setBusy(null)}}
  async function undo(batchId:string){setBusy("undo");setError("");try{const csrf=await fetch("/api/admin/auth/csrf",{cache:"no-store"}),{token}=await csrf.json() as {token:string};
    const response=await fetch("/api/admin/editorial/tasks/bulk-assignee/undo",{method:"POST",headers:{"content-type":"application/json","x-csrf-token":token},body:JSON.stringify({batchId})});if(!response.ok)throw new Error();setNotice({text:c.undoSuccess});router.refresh();
  }catch{setError(c.undoError)}finally{setBusy(null)}}
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
    ["quality", c.qualityFilter, scope === "mine" ? 0 : data.summary.incompleteQuality],
    ["freshness", c.freshnessFilter, scope === "mine" ? 0 : data.summary.freshnessDebt],
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
        <article>
          <strong>{data.summary.freshnessDebt}</strong>
          <span>{c.freshness}</span>
        </article>
      </div>
      <section className="editorial-performance" aria-labelledby="editorial-performance-title">
        <header><div><p className="section-kicker">{pc.kicker}</p><h3 id="editorial-performance-title">{pc.title}</h3><p>{pc.intro}</p></div>{data.performance.unmeasuredCompleted>0&&<small>{data.performance.unmeasuredCompleted} {pc.legacy}</small>}</header>
        <div className="performance-windows">{[[pc.days30,data.performance.last30Days],[pc.days90,data.performance.last90Days]].map(([label,window])=><article key={String(label)}><div><strong>{label as string}</strong><span>{(window as typeof data.performance.last30Days).sampleSize} {pc.sample}</span></div>{(window as typeof data.performance.last30Days).sampleSize<3?<p>{pc.insufficient}</p>:<dl><div><dt>{pc.onTime}</dt><dd>{(window as typeof data.performance.last30Days).onTimePercent}%</dd></div><div><dt>{pc.median}</dt><dd>{(window as typeof data.performance.last30Days).medianHours} {pc.hours}</dd></div><div><dt>{pc.p95}</dt><dd>{(window as typeof data.performance.last30Days).p95Hours} {pc.hours}</dd></div></dl>}</article>)}</div>
        <div className="throughput-chart" aria-label={pc.trend}>{data.performance.weeklyThroughput.map((week,index)=>{const max=Math.max(1,...data.performance.weeklyThroughput.map(item=>item.completed)),label=`${new Intl.DateTimeFormat(locale,{dateStyle:"medium"}).format(new Date(week.startsAt))}: ${week.completed}`;return <span key={week.startsAt} aria-label={label} title={label} style={{height:`${Math.max(8,week.completed/max*100)}%`}}><i aria-hidden>{index===0||index===12?new Intl.DateTimeFormat(locale,{month:"short",day:"numeric"}).format(new Date(week.startsAt)):""}</i></span>})}</div>
      </section>
      <section className="editorial-capacity" aria-labelledby="editorial-capacity-title">
        <header><div><p className="section-kicker">{c.teamMembers}: {data.summary.teamMembers}</p><h3 id="editorial-capacity-title">{c.capacity}</h3><p>{c.capacityIntro}</p></div><strong data-alert={data.summary.unassigned>0}>{data.summary.unassigned} <span>{c.noOwner}</span></strong></header>
        <div className="capacity-grid">{data.workloads.map(person=><article key={person.userId} data-alert={person.overdue>0}><div><strong>{person.displayName}</strong><small>{person.open} {c.openShort}</small></div><span>{person.overdue} {c.overdueShort}</span><span>{person.dueSoon} {c.soonShort}</span><i aria-hidden style={{width:`${Math.min(100,person.open*12.5)}%`}}/></article>)}</div>
      </section>
      <EditorialCalendar locale={locale} data={data}/>
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
      {canReassign&&scope==="team"&&<section className="bulk-assignment" aria-labelledby="bulk-assignment-title"><header><div><h3 id="bulk-assignment-title">{c.bulkTitle}</h3><p>{c.bulkIntro}</p></div><strong>{selected.size} {c.selected}</strong></header><div className="bulk-controls"><label className="bulk-select-all"><input type="checkbox" checked={items.filter(item=>item.taskId).length>0&&items.filter(item=>item.taskId).every(item=>selected.has(item.taskId!))} onChange={event=>setSelected(event.target.checked?new Set(items.flatMap(item=>item.taskId?[item.taskId]:[]).slice(0,25)):new Set())}/><span>{c.selectVisible}</span></label><label><span>{c.target}</span><select value={bulkOwner} onChange={event=>setBulkOwner(event.target.value)}><option value="">—</option>{data.users.map(user=><option key={user.id} value={user.id}>{user.displayName}</option>)}</select></label><button type="button" disabled={!bulkOwner||selected.size===0} onClick={()=>setPreview(true)}>{c.preview}</button></div></section>}
      {error && (
        <p className="desk-error" role="alert">
          {error}
        </p>
      )}
      {notice&&<div className="bulk-notice" role="status"><strong>{notice.text}</strong>{notice.batchId&&<button type="button" disabled={busy==="undo"} onClick={()=>void undo(notice.batchId!)}>{c.undo}</button>}</div>}
      {preview&&<div className="bulk-preview" role="dialog" aria-modal="true" aria-labelledby="bulk-preview-title"><div><h3 id="bulk-preview-title">{c.previewTitle}</h3><p>{c.previewBody}</p><ul>{items.filter(item=>item.taskId&&selected.has(item.taskId)).map(item=><li key={item.taskId}><strong>{item.taskTitle}</strong><span>{item.title} · {item.assignee} → {data.users.find(user=>user.id===bulkOwner)?.displayName}</span></li>)}</ul><footer><button type="button" className="secondary-button" onClick={()=>setPreview(false)}>{c.cancel}</button><button type="button" disabled={busy==="bulk"} onClick={()=>void bulkReassign()}>{c.confirm}</button></footer></div></div>}
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
              {canReassign&&scope==="team"&&item.taskId&&<label className="task-selector"><input type="checkbox" checked={selected.has(item.taskId)} onChange={()=>toggle(item.taskId!)} aria-label={`${c.selectTask}: ${item.taskTitle??item.title}`}/></label>}
              <div className="command-rank">
                {String(index + 1).padStart(2, "0")}
              </div>
              <div>
                <span className="command-kind">
                  {c[item.kind as keyof typeof c] ?? item.kind}
                </span>
                <h3>{item.taskTitle ?? item.title}</h3>
                {item.taskTitle && <p>{item.title}</p>}
                {item.kind==="QualityGate"&&item.missingGates&&<div className="quality-debt-gates" aria-label={c.missing}>{item.missingGates.map(gate=><span key={gate}>{gateCopy[locale as keyof typeof gateCopy]?.[gate as keyof typeof gateCopy["tr-TR"]]??gate}</span>)}</div>}
                {item.kind==="FreshnessDebt"&&item.freshnessReasons&&<div className="freshness-debt-reasons" aria-label={fc.why}>{item.freshnessReasons.map(reason=><span key={reason}>{fc[reason as keyof typeof fc]??c[reason as keyof typeof c]??reason}</span>)}</div>}
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
                {item.taskId&&canReassign&&<label><span>{c.reassign}</span><select value={item.assigneeUserId??""} disabled={busy===item.taskId} onChange={event=>void reassign(item,event.target.value)}><option value="" disabled>{c.assigned}</option>{data.users.map(user=><option key={user.id} value={user.id}>{user.displayName}</option>)}</select></label>}
                {item.kind==="FreshnessDebt"&&(item.locale==="tr-TR"?<button type="button" disabled={busy===`freshness-${item.articleId}`} onClick={()=>void createFreshnessTask(item)}>{fc.take}</button>:<small>{fc.sourceOnly}</small>)}
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
