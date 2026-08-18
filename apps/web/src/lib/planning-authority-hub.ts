import type { Locale } from "@/i18n/config";
import type { PublicArticleSummary } from "@/lib/public-api";

export const planningHubSlugs: Record<Locale, string> = {
  "tr-TR": "zaman-odak-ve-planlama",
  "en-US": "time-focus-and-planning",
  "de-DE": "zeit-fokus-und-planung",
  "fr-FR": "temps-concentration-et-planification",
};

export const planningHubCopy: Record<Locale, {
  eyebrow: string; title: string; intro: string; guide: string; guideIntro: string;
  evidence: string; evidenceNote: string; sources: string; reviewed: string; open: string;
  clusters: { id: string; title: string; description: string; pattern: RegExp }[];
}> = {
  "tr-TR": { eyebrow:"SEÇİM REHBERİ", title:"İhtiyacından başla, aracını sonra seç", intro:"Takvim, görev yöneticisi, odak engelleyici ve zaman takip araçları aynı sorunu çözmez. Hedefini seç; kaynak kaydı bulunan BOECL incelemelerine doğrudan ilerle.", guide:"Dört karar yolu", guideIntro:"Yayınları marka adına göre değil, çözmek istediğin işe göre grupladık.", evidence:"Kaynak kanıtı", evidenceNote:"Kaynak sayısı tek başına doğruluk garantisi değildir; editoryal incelemeyi ve karşılaştırmayı kolaylaştıran şeffaflık sinyalidir.", sources:"kaynak", reviewed:"incelenmiş", open:"Rehberi aç", clusters:[
    {id:"plan",title:"Günü ve haftayı planla",description:"Takvim ile görevleri tek akışta görmek, öncelikleri zaman bloklarına yerleştirmek için.",pattern:/akiflow|marvin|flowsavvy|morgen|motion|reclaim|routine|sunsama|ticktick|todoist|vimcal|fantastical|notion/i},
    {id:"focus",title:"Dikkat dağıtıcıları azalt",description:"Derin çalışma oturumları kurmak, engelleri azaltmak ve odağı sürdürülebilir kılmak için.",pattern:/focus|forest|freedom|cold-turkey|endel|toplantisiz|mikro-mola/i},
    {id:"measure",title:"Zamanını ölç",description:"Günün nereye gittiğini görmek, alışkanlıkları ölçmek ve çalışma raporu oluşturmak için.",pattern:/clockify|toggl|rescuetime|rize|zaman-takip/i},
    {id:"choose",title:"Alternatifleri karşılaştır",description:"Kalan araçları özellik listesiyle değil, kullanım senaryosu ve iş akışı uyumuyla değerlendirmek için.",pattern:/.*/i},
  ]},
  "en-US": { eyebrow:"CHOOSING GUIDE",title:"Start with the need, then choose the tool",intro:"Calendars, task managers, focus blockers, and time trackers solve different problems. Pick your goal and explore BOECL reviews with visible source records.",guide:"Four decision paths",guideIntro:"Stories are grouped by the job to be done, not by brand name.",evidence:"Source evidence",evidenceNote:"Source counts do not guarantee accuracy; they are transparency signals that support editorial review and comparison.",sources:"sources",reviewed:"reviewed",open:"Open guide",clusters:[{id:"plan",title:"Plan your day and week",description:"Bring calendars, tasks, priorities, and time blocks into one workable flow.",pattern:/akiflow|marvin|flowsavvy|morgen|motion|reclaim|routine|sunsama|ticktick|todoist|vimcal|fantastical|notion/i},{id:"focus",title:"Reduce distractions",description:"Build deep-work sessions and make sustained focus easier.",pattern:/focus|forest|freedom|cold-turkey|endel|meeting|break/i},{id:"measure",title:"Measure your time",description:"Understand where the day goes and turn activity into useful reports.",pattern:/clockify|toggl|rescuetime|rize|time-track/i},{id:"choose",title:"Compare alternatives",description:"Assess the remaining tools by use case and workflow fit.",pattern:/.*/i}]},
  "de-DE": { eyebrow:"AUSWAHLHILFE",title:"Erst den Bedarf klären, dann das Werkzeug wählen",intro:"Kalender, Aufgabenmanager, Fokusblocker und Zeiterfassung lösen unterschiedliche Probleme. Wähle dein Ziel und öffne BOECL-Beiträge mit sichtbaren Quellenangaben.",guide:"Vier Entscheidungswege",guideIntro:"Beiträge sind nach Aufgabe statt Markenname geordnet.",evidence:"Quellennachweis",evidenceNote:"Die Zahl der Quellen garantiert keine Richtigkeit; sie ist ein Transparenzsignal für Prüfung und Vergleich.",sources:"Quellen",reviewed:"geprüft",open:"Ratgeber öffnen",clusters:[{id:"plan",title:"Tag und Woche planen",description:"Kalender, Aufgaben, Prioritäten und Zeitblöcke zusammenführen.",pattern:/akiflow|marvin|flowsavvy|morgen|motion|reclaim|routine|sunsama|ticktick|todoist|vimcal|fantastical|notion/i},{id:"focus",title:"Ablenkungen reduzieren",description:"Phasen tiefer Arbeit aufbauen und Fokus erhalten.",pattern:/focus|forest|freedom|cold-turkey|endel|meeting|pause/i},{id:"measure",title:"Zeit messen",description:"Zeitverwendung verstehen und hilfreiche Berichte erstellen.",pattern:/clockify|toggl|rescuetime|rize|zeiterfassung/i},{id:"choose",title:"Alternativen vergleichen",description:"Weitere Werkzeuge nach Einsatzzweck und Arbeitsablauf bewerten.",pattern:/.*/i}]},
  "fr-FR": { eyebrow:"GUIDE DE CHOIX",title:"Partir du besoin, puis choisir l’outil",intro:"Calendriers, gestionnaires de tâches, bloqueurs de distractions et suivi du temps répondent à des besoins différents. Choisissez votre objectif et consultez les analyses BOECL avec leurs sources visibles.",guide:"Quatre parcours de décision",guideIntro:"Les publications sont regroupées par usage, et non par marque.",evidence:"Preuves documentaires",evidenceNote:"Le nombre de sources ne garantit pas l’exactitude ; il constitue un signal de transparence utile à la vérification et à la comparaison.",sources:"sources",reviewed:"vérifiées",open:"Ouvrir le guide",clusters:[{id:"plan",title:"Planifier la journée et la semaine",description:"Réunir calendrier, tâches, priorités et blocs de temps.",pattern:/akiflow|marvin|flowsavvy|morgen|motion|reclaim|routine|sunsama|ticktick|todoist|vimcal|fantastical|notion/i},{id:"focus",title:"Réduire les distractions",description:"Créer des sessions de travail profond et préserver la concentration.",pattern:/focus|forest|freedom|cold-turkey|endel|réunion|pause/i},{id:"measure",title:"Mesurer son temps",description:"Comprendre l’usage de la journée et produire des rapports utiles.",pattern:/clockify|toggl|rescuetime|rize|suivi-du-temps/i},{id:"choose",title:"Comparer les alternatives",description:"Évaluer les autres outils selon l’usage et le flux de travail.",pattern:/.*/i}]},
};

export function buildPlanningClusters(locale: Locale, articles: PublicArticleSummary[]) {
  const remaining = [...articles];
  return planningHubCopy[locale].clusters.map(cluster => {
    const matches = remaining.filter(article => cluster.pattern.test(`${article.slug} ${article.title}`));
    for (const match of matches) remaining.splice(remaining.indexOf(match), 1);
    return { ...cluster, articles: matches.sort((a,b)=>(b.reviewedSourceCount??0)-(a.reviewedSourceCount??0)||(b.sourceCount??0)-(a.sourceCount??0)).slice(0,4) };
  }).filter(cluster => cluster.articles.length > 0);
}
