import { redirect } from "next/navigation";
import { AdminOverview } from "@/components/admin/admin-overview";
import { AutomaticContentControl } from "@/components/admin/automatic-content-control";
import { EditorialCommandCenterView } from "@/components/admin/editorial-command-center";
import { siteConfig } from "@/config/site";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getArticles, getEditorialCommandCenter, getSystemStatus } from "@/lib/admin-api";
const pageCopy={"tr-TR":{kicker:"EDİTORYAL OPERASYON",title:"Kontrol merkezi",intro:"Öncelikli işleri görün, yayın darboğazlarını çözün ve kalite kapılarını yönetin.",live:"Canlı sistem"},"en-US":{kicker:"EDITORIAL OPS",title:"Control center",intro:"See priority work, resolve publishing bottlenecks, and manage quality gates.",live:"Live system"},"de-DE":{kicker:"REDAKTIONSBETRIEB",title:"Kontrollzentrum",intro:"Prioritäten erkennen, Engpässe lösen und Qualitätstore verwalten.",live:"Live-System"},"fr-FR":{kicker:"OPÉRATIONS ÉDITORIALES",title:"Centre de contrôle",intro:"Visualisez les priorités, résolvez les blocages et gérez les contrôles qualité.",live:"Système actif"}} as const;

export default async function AdminPage({params}:PageProps<"/[locale]/admin">) {
  const {locale}=await params;
  if(!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session=await getAdminSession();
  if(!session) redirect(`/${locale}/admin/login`);
  const canManage=session.roles.some(role=>["Owner","Admin"].includes(role));
  const canManageEditorial=session.roles.some(role=>["Owner","Admin","Editor"].includes(role));
  const [articles,status,commandCenter]=await Promise.all([getArticles(),canManage?getSystemStatus():Promise.resolve(null),getEditorialCommandCenter()]);
  const copy=pageCopy[locale];
  return <main className="admin-shell admin-dashboard-shell">
    <header className="admin-command-header"><div><p className="section-kicker">{siteConfig.name} / {copy.kicker}</p><h1>{copy.title}</h1><p>{copy.intro}</p></div><div className="dashboard-live"><span/><strong>{copy.live}</strong></div></header>
    {commandCenter&&<EditorialCommandCenterView locale={locale} data={commandCenter} canReassign={canManageEditorial}/>}
    {canManage&&<AutomaticContentControl/>}
    <AdminOverview locale={locale} articles={articles} status={status}/>
  </main>;
}
