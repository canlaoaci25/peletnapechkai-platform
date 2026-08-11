import Link from "next/link";
import { cookies } from "next/headers";
import { notFound, redirect } from "next/navigation";
import { hasLocale } from "@/i18n/config";
import { getAdminSession } from "@/lib/admin-api";
import { AutomationLivePhases } from "@/components/admin/automation-live-phases";

const apiUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5267";
type JobReport = { id:string;type:string;status:string;targetLocales:string[];totalItems:number;completedItems:number;failedItems:number;currentPhase:number;lastMessage:string|null;reportText:string|null;createdAt:string;updatedAt:string;completedAt:string|null;includeImages?:boolean;autoTranslate?:boolean;autoSeo?:boolean };
const names:Record<string,string>={ContentTranslation:"Otomatik içerik çevirisi",SeoLocalization:"Otomatik SEO yerelleştirmesi",SiteLocalization:"Otomatik site dili",SystemReport:"Otomatik sistem raporu",ReadyContentGeneration:"Hazır içerik üretimi"};
const statuses:Record<string,string>={Queued:"Kuyrukta",Running:"Çalışıyor",Paused:"Durduruldu",Completed:"Tamamlandı",Failed:"Hatalı",Cancelled:"İptal edildi"};

export default async function AutomationReportPage({params}:PageProps<"/[locale]/admin/automation/[jobId]">){
  const{locale,jobId}=await params;if(!hasLocale(locale))redirect("/tr-TR/admin/login");
  const session=await getAdminSession();if(!session)redirect(`/${locale}/admin/login`);if(!session.roles.some(role=>["Owner","Admin"].includes(role)))redirect(`/${locale}/admin`);
  const cookieStore=await cookies();const response=await fetch(new URL(`/api/v1/admin/automation/${jobId}`,apiUrl),{headers:{cookie:cookieStore.toString()},cache:"no-store"});if(response.status===404)notFound();if(!response.ok)redirect(`/${locale}/admin/automation`);const job=await response.json()as JobReport;
  const started=new Date(job.createdAt),ended=job.completedAt?new Date(job.completedAt):null,duration=ended?Math.max(0,Math.round((ended.getTime()-started.getTime())/1000)):null;
  return <main className="admin-shell admin-dashboard-shell automation-report-page">
    <Link className="back-link" href={`/${locale}/admin/automation`}>← Toplu çalıştırıcılara dön</Link>
    <header className="admin-command-header"><div><p className="section-kicker">AYRINTILI İŞ RAPORU</p><h1>{names[job.type]??job.type}</h1><p>{job.lastMessage??"Henüz durum mesajı bulunmuyor."}</p></div><b className={`automation-report-status status-${job.status.toLowerCase()}`}>{statuses[job.status]??job.status}</b></header>
    <section className="automation-report-summary" aria-label="İş özeti">
      <article className="admin-panel"><small>İş kimliği</small><strong>{job.id}</strong></article><article className="admin-panel"><small>Hedef</small><strong>{job.targetLocales.join(", ")||"Sistem"}</strong></article><article className="admin-panel"><small>Sonuç</small><strong>{job.completedItems} tamamlandı · {job.failedItems} hata</strong></article><article className="admin-panel"><small>Süre</small><strong>{duration===null?"Devam ediyor":duration<60?`${duration} saniye`:`${Math.floor(duration/60)} dk ${duration%60} sn`}</strong></article>
    </section>
    <AutomationLivePhases initial={job}/>
    <section className="admin-panel automation-report-document"><header><div><p className="section-kicker">RESULT.TXT</p><h2>Codex sonuç raporu</h2></div><time dateTime={job.updatedAt}>{new Intl.DateTimeFormat("tr-TR",{dateStyle:"long",timeStyle:"short"}).format(new Date(job.updatedAt))}</time></header>{job.reportText?<pre>{job.reportText}</pre>:<p className="muted">Bu eski iş için henüz result.txt raporu veritabanına aktarılmadı.</p>}</section>
  </main>;
}
