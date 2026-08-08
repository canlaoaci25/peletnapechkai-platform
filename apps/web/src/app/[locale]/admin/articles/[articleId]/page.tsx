import Link from "next/link";
import { notFound, redirect } from "next/navigation";

import { ArticleEditor } from "@/components/admin/article-editor";
import { WorkflowActions } from "@/components/admin/workflow-actions";
import { adminCopy } from "@/i18n/admin-copy";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getArticle } from "@/lib/admin-api";

export default async function EditArticlePage({ params }: PageProps<"/[locale]/admin/articles/[articleId]">) {
  const { locale, articleId } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  const article = await getArticle(articleId);
  if (!article) notFound();
  const copy = adminCopy[locale];

  return <main className="admin-shell narrow-admin"><Link className="back-link" href={`/${locale}/admin`}>← {copy.back}</Link><WorkflowActions article={article} roles={session.roles} copy={copy} /><section className="admin-panel"><h1 className="editor-title">{copy.editDraft}</h1>{article.status === "Draft" ? <ArticleEditor copy={copy} article={article} /> : <p className="muted">{copy.lockedForReview}</p>}</section></main>;
}
