import Link from "next/link";
import type { EditorialCommandCenter } from "@/lib/admin-api";

const copy = {
  "tr-TR": { kicker:"BUGÜNÜN YAYIN PLANI", title:"Editoryal komuta kuyruğu", intro:"En yüksek riskli işlerden başlayın; her kart doğrudan çalışma alanına açılır.", overdue:"Geciken", soon:"48 saatte", review:"İncelemede", quality:"Eksik kalite kapısı", empty:"Kuyruk temiz", emptyBody:"Geciken görev veya bekleyen inceleme bulunmuyor.", open:"Çalışma alanını aç", EditorialReview:"Editoryal inceleme", SeoReview:"SEO incelemesi", OverdueTask:"Gecikmiş görev", Task:"Atanmış görev", assigned:"Sorumlu" },
  "en-US": { kicker:"TODAY'S PUBLISHING PLAN", title:"Editorial command queue", intro:"Start with the highest-risk work; each card opens its workspace.", overdue:"Overdue", soon:"Due in 48h", review:"In review", quality:"Quality gates open", empty:"Queue is clear", emptyBody:"There are no overdue tasks or pending reviews.", open:"Open workspace", EditorialReview:"Editorial review", SeoReview:"SEO review", OverdueTask:"Overdue task", Task:"Assigned task", assigned:"Owner" },
  "de-DE": { kicker:"HEUTIGER REDAKTIONSPLAN", title:"Redaktionelle Kommandozentrale", intro:"Beginnen Sie mit den größten Risiken; jede Karte öffnet den Arbeitsbereich.", overdue:"Überfällig", soon:"In 48 Std.", review:"In Prüfung", quality:"Offene Qualitätstore", empty:"Warteschlange leer", emptyBody:"Keine überfälligen Aufgaben oder offenen Prüfungen.", open:"Arbeitsbereich öffnen", EditorialReview:"Redaktionelle Prüfung", SeoReview:"SEO-Prüfung", OverdueTask:"Überfällige Aufgabe", Task:"Zugewiesene Aufgabe", assigned:"Verantwortlich" },
  "fr-FR": { kicker:"PLAN DE PUBLICATION DU JOUR", title:"Centre de commande éditorial", intro:"Commencez par les risques prioritaires ; chaque carte ouvre son espace de travail.", overdue:"En retard", soon:"Sous 48 h", review:"En révision", quality:"Contrôles qualité ouverts", empty:"File traitée", emptyBody:"Aucune tâche en retard ni révision en attente.", open:"Ouvrir l’espace", EditorialReview:"Révision éditoriale", SeoReview:"Révision SEO", OverdueTask:"Tâche en retard", Task:"Tâche attribuée", assigned:"Responsable" },
} as const;

export function EditorialCommandCenterView({locale,data}:{locale:string;data:EditorialCommandCenter}) {
  const c=copy[locale as keyof typeof copy]??copy["tr-TR"];
  return <section className="admin-panel editorial-command-center" aria-labelledby="editorial-command-title">
    <header><div><p className="section-kicker">{c.kicker}</p><h2 id="editorial-command-title">{c.title}</h2><p>{c.intro}</p></div><time dateTime={data.checkedAt}>{new Intl.DateTimeFormat(locale,{timeStyle:"short"}).format(new Date(data.checkedAt))}</time></header>
    <div className="command-summary" aria-label={c.title}>
      <article data-alert={data.summary.overdue>0}><strong>{data.summary.overdue}</strong><span>{c.overdue}</span></article>
      <article><strong>{data.summary.dueSoon}</strong><span>{c.soon}</span></article>
      <article><strong>{data.summary.inReview}</strong><span>{c.review}</span></article>
      <article><strong>{data.summary.incompleteQuality}</strong><span>{c.quality}</span></article>
    </div>
    {data.items.length===0?<div className="command-empty"><strong>{c.empty}</strong><span>{c.emptyBody}</span></div>:<div className="command-list">{data.items.map((item,index)=><article key={`${item.articleId}-${item.kind}-${index}`} data-kind={item.kind}>
      <div className="command-rank">{String(index+1).padStart(2,"0")}</div><div><span className="command-kind">{c[item.kind as keyof typeof c]??item.kind}</span><h3>{item.taskTitle??item.title}</h3>{item.taskTitle&&<p>{item.title}</p>}<small>{item.locale}{item.assignee?` · ${c.assigned}: ${item.assignee}`:""}</small></div>
      <time dateTime={item.dueAt}>{new Intl.DateTimeFormat(locale,{dateStyle:"medium"}).format(new Date(item.dueAt))}</time><Link href={`/${locale}/admin/articles/${item.articleId}`}>{c.open} →</Link>
    </article>)}</div>}
  </section>;
}
