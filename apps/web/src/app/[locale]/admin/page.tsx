import Link from "next/link";
import { redirect } from "next/navigation";
import { AdminDashboard } from "@/components/admin/admin-dashboard";
import { ArticleEditor } from "@/components/admin/article-editor";
import { LogoutButton } from "@/components/admin/logout-button";
import { siteConfig } from "@/config/site";
import { adminCopy } from "@/i18n/admin-copy";
import { hasLocale } from "@/i18n/config";
import { knowledgeCopy } from "@/i18n/knowledge-copy";
import { libraryCopy } from "@/i18n/library-copy";
import { userCopy } from "@/i18n/user-copy";
import { getAdminSession,getArticles,getSystemStatus } from "@/lib/admin-api";
export default async function AdminPage({params}:PageProps<"/[locale]/admin">){
 const{locale}=await params;if(!hasLocale(locale))redirect("/tr-TR/admin/login");const session=await getAdminSession();if(!session)redirect(`/${locale}/admin/login`);
 const[articles,status]=await Promise.all([getArticles(),session.roles.some(x=>["Owner","Admin"].includes(x))?getSystemStatus():Promise.resolve(null)]);const copy=adminCopy[locale];const editor=session.roles.some(x=>["Owner","Admin","Editor"].includes(x));
 return <main className="admin-shell admin-dashboard-shell"><header className="admin-command-header"><div><p className="section-kicker">{siteConfig.name} / YÖNETİM</p><h1>Kontrol merkezi</h1><p>İçerik, yayın akışı ve site araçları tek ekranda.</p></div><div className="admin-account"><span><strong>{session.displayName}</strong><small>{session.roles.join(" · ")}</small></span><LogoutButton locale={locale} label={copy.logout}/></div></header>
 <nav className="admin-main-nav" aria-label="Yönetim bölümleri"><Link className="active" href={`/${locale}/admin`}>Genel bakış</Link><Link href="#contents">İçerikler <span>{articles.length}</span></Link>{editor&&<Link href={`/${locale}/admin/library`}>{libraryCopy[locale].link}</Link>}{editor&&<Link href={`/${locale}/admin/knowledge`}>{knowledgeCopy[locale].link}</Link>}{session.roles.some(x=>["Owner","Admin"].includes(x))&&<Link href={`/${locale}/admin/users`}>{userCopy[locale].usersLink}</Link>}<Link href={`/${locale}`} target="_blank">Siteyi görüntüle ↗</Link></nav>
 <div id="contents"><AdminDashboard locale={locale} articles={articles} status={status}/></div><details id="new-article" className="admin-panel create-drawer"><summary><span><strong>Yeni Türkçe içerik oluştur</strong><small>Taslak editörünü aç</small></span><span>+</span></summary><div className="create-drawer-body"><ArticleEditor copy={copy}/></div></details></main>;
}
