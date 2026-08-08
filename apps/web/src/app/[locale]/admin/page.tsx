import { redirect } from "next/navigation";
import Link from "next/link";

import { ArticleEditor } from "@/components/admin/article-editor";
import { LogoutButton } from "@/components/admin/logout-button";
import { adminCopy } from "@/i18n/admin-copy";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getArticles, getSystemStatus } from "@/lib/admin-api";
import { siteConfig } from "@/config/site";
import { userCopy } from "@/i18n/user-copy";
import { libraryCopy } from "@/i18n/library-copy";
import { SystemStatus } from "@/components/admin/system-status";
import { knowledgeCopy } from "@/i18n/knowledge-copy";

export default async function AdminPage({ params }: PageProps<"/[locale]/admin">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  const [articles, status, copy] = await Promise.all([getArticles(), session.roles.some(role=>["Owner","Admin"].includes(role))?getSystemStatus():Promise.resolve(null), Promise.resolve(adminCopy[locale])]);

  return (
    <main className="admin-shell">
      <header className="admin-header">
        <div><p className="section-kicker">{siteConfig.name}</p><h1>{copy.dashboard}</h1></div>
        <div className="admin-session"><span>{copy.signedInAs}: {session.displayName}</span>{session.roles.some((role) => ["Owner", "Admin", "Editor"].includes(role)) && <Link href={`/${locale}/admin/library`}>{libraryCopy[locale].link}</Link>}{session.roles.some((role) => ["Owner", "Admin", "Editor"].includes(role)) && <Link href={`/${locale}/admin/knowledge`}>{knowledgeCopy[locale].link}</Link>}{session.roles.some((role) => ["Owner", "Admin"].includes(role)) && <Link href={`/${locale}/admin/users`}>{userCopy[locale].usersLink}</Link>}<LogoutButton locale={locale} label={copy.logout} /></div>
      </header>
      <div className="admin-columns">
        <section className="admin-panel"><h2>{copy.newDraft}</h2><ArticleEditor copy={copy} /></section>
        <section className="admin-panel"><h2>{copy.recent}</h2>
          {articles.length === 0 ? <p className="muted">{copy.empty}</p> : (
            <div className="article-table" role="table">
              <div className="article-row article-row-head" role="row"><span>{copy.title}</span><span>{copy.status}</span><span>{copy.updated}</span></div>
              {articles.map((article) => <div className="article-row" role="row" key={article.id}><span><Link href={`/${locale}/admin/articles/${article.id}`}><strong>{article.title}</strong></Link><small>{article.locale} · {article.type}</small></span><span>{article.status}</span><time dateTime={article.updatedAt}>{new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(article.updatedAt))}</time></div>)}
            </div>
          )}
        </section>
      </div>{status&&<SystemStatus status={status}/>} 
    </main>
  );
}
