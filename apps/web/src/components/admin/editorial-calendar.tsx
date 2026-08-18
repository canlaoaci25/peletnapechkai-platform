import Link from "next/link";
import type { EditorialCommandCenter } from "@/lib/admin-api";

const copy = {
  "tr-TR": { kicker:"14 GÜNLÜK YAYIN PLANI", title:"Editoryal takvim", intro:"Planlanan yayınları gün, dil ve kategori ekseninde görün; yığılmaları yayın anına kalmadan dengeleyin.", scheduled:"Planlı yayın", conflicts:"Denge uyarısı", ready:"Planlanabilir", empty:"Önümüzdeki 14 gün için planlanmış yayın yok.", open:"Planı aç", localeCollision:"Aynı dilde birden fazla yayın", categoryCollision:"Aynı kategoride yığılma", uncategorized:"Kategori bekliyor" },
  "en-US": { kicker:"14-DAY PUBLISHING PLAN", title:"Editorial calendar", intro:"See scheduled stories by day, locale, and category; rebalance pressure before publication.", scheduled:"Scheduled", conflicts:"Balance alerts", ready:"Ready to schedule", empty:"No stories are scheduled for the next 14 days.", open:"Open plan", localeCollision:"Multiple stories in the same locale", categoryCollision:"Category concentration", uncategorized:"Category pending" },
  "de-DE": { kicker:"14-TAGE-VERÖFFENTLICHUNGSPLAN", title:"Redaktionskalender", intro:"Geplante Beiträge nach Tag, Sprache und Kategorie sehen und Häufungen rechtzeitig ausgleichen.", scheduled:"Geplant", conflicts:"Balancehinweise", ready:"Planungsbereit", empty:"Für die nächsten 14 Tage sind keine Beiträge geplant.", open:"Plan öffnen", localeCollision:"Mehrere Beiträge in derselben Sprache", categoryCollision:"Kategoriehäufung", uncategorized:"Kategorie ausstehend" },
  "fr-FR": { kicker:"PLAN DE PUBLICATION SUR 14 JOURS", title:"Calendrier éditorial", intro:"Visualisez les publications par jour, langue et catégorie, puis équilibrez les concentrations avant diffusion.", scheduled:"Planifiées", conflicts:"Alertes d’équilibre", ready:"Prêtes à planifier", empty:"Aucune publication n’est planifiée pour les 14 prochains jours.", open:"Ouvrir le plan", localeCollision:"Plusieurs contenus dans la même langue", categoryCollision:"Concentration de catégorie", uncategorized:"Catégorie en attente" },
} as const;

export function EditorialCalendar({ locale, data }: { locale: string; data: EditorialCommandCenter }) {
  const c = copy[locale as keyof typeof copy] ?? copy["tr-TR"];
  const dayKey = new Intl.DateTimeFormat("sv-SE", { timeZone:"Europe/Istanbul", year:"numeric", month:"2-digit", day:"2-digit" });
  const groups = Object.entries(Object.groupBy(data.schedule, item => dayKey.format(new Date(item.scheduledAt))));
  return <section className="editorial-calendar" aria-labelledby="editorial-calendar-title">
    <header><div><p className="section-kicker">{c.kicker}</p><h3 id="editorial-calendar-title">{c.title}</h3><p>{c.intro}</p></div>
      <div className="calendar-metrics" aria-label={c.title}><span><strong>{data.summary.scheduled}</strong>{c.scheduled}</span><span data-alert={data.summary.scheduleConflicts>0}><strong>{data.summary.scheduleConflicts}</strong>{c.conflicts}</span><span><strong>{data.summary.readyToSchedule}</strong>{c.ready}</span></div>
    </header>
    {groups.length===0 ? <p className="calendar-empty">{c.empty}</p> : <div className="calendar-days">{groups.map(([day,items])=><section key={day} className="calendar-day"><header><time dateTime={day}>{new Intl.DateTimeFormat(locale,{timeZone:"Europe/Istanbul",weekday:"short",day:"numeric",month:"short"}).format(new Date(items![0].scheduledAt))}</time><b>{items?.length??0}</b></header><div>{items?.map(item=><article key={item.articleId} data-conflict={item.hasConflict}><div><time dateTime={item.scheduledAt}>{new Intl.DateTimeFormat(locale,{timeZone:"Europe/Istanbul",hour:"2-digit",minute:"2-digit",timeZoneName:"short"}).format(new Date(item.scheduledAt))}</time><span>{item.locale}</span></div><h4>{item.title}</h4><small>{item.categories.length>0?item.categories.join(" · "):c.uncategorized}</small>{item.conflictReasons.length>0&&<ul aria-label={c.conflicts}>{item.conflictReasons.map(reason=><li key={reason}>{reason==="LocaleCollision"?c.localeCollision:c.categoryCollision}</li>)}</ul>}<Link href={`/${locale}/admin/articles/${item.articleId}`}>{c.open} →</Link></article>)}</div></section>)}</div>}
  </section>;
}
