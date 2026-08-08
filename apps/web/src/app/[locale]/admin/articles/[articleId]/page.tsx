import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { ArticleEditor } from "@/components/admin/article-editor";
import { RelationshipEditor } from "@/components/admin/relationship-editor";
import { WorkflowActions } from "@/components/admin/workflow-actions";
import { adminCopy } from "@/i18n/admin-copy";
import { hasLocale } from "@/i18n/config";
import { libraryCopy } from "@/i18n/library-copy";
import { getAdminSession, getArticle, getMedia, getSupportingLibrary } from "@/lib/admin-api";

export default async function EditArticlePage({ params }: PageProps<"/[locale]/admin/articles/[articleId]">) {
  const { locale, articleId } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  const [article,library,media] = await Promise.all([getArticle(articleId),getSupportingLibrary(),getMedia()]);
  if (!article) notFound();
  const copy = adminCopy[locale];
  const rel={title:libraryCopy[locale].link,categories:libraryCopy[locale].category,tags:libraryCopy[locale].tag,authors:libraryCopy[locale].author,sources:libraryCopy[locale].source,media:libraryCopy[locale].media,save:copy.update,saved:copy.saved,error:copy.saveError};
  return <main className="admin-shell narrow-admin"><Link className="back-link" href={`/${locale}/admin`}>← {copy.back}</Link><WorkflowActions article={article} roles={session.roles} copy={copy} /><section className="admin-panel"><h1 className="editor-title">{copy.editDraft}</h1>{article.status === "Draft" ? <ArticleEditor copy={copy} article={article} /> : <p className="muted">{copy.lockedForReview}</p>}</section>{article.status === "Draft"&&session.roles.some(role=>["Owner","Admin","Editor"].includes(role))&&<section className="admin-panel"><RelationshipEditor article={article} library={library} media={media} labels={rel}/></section>}</main>;
}
