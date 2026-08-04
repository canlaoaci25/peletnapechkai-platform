import Link from "next/link";
import { notFound, redirect } from "next/navigation";

import { ArticleEditor } from "@/components/admin/article-editor";
import { adminCopy } from "@/i18n/admin-copy";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getArticle } from "@/lib/admin-api";

export default async function EditArticlePage({ params }: PageProps<"/[locale]/admin/articles/[articleId]">) {
  const { locale, articleId } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  if (!(await getAdminSession())) redirect(`/${locale}/admin/login`);
  const article = await getArticle(articleId);
  if (!article) notFound();
  const copy = adminCopy[locale];

  return <main className="admin-shell narrow-admin"><Link className="back-link" href={`/${locale}/admin`}>← {copy.back}</Link><section className="admin-panel"><p className="section-kicker">{article.status}</p><h1 className="editor-title">{copy.editDraft}</h1><ArticleEditor copy={copy} article={article} /></section></main>;
}
