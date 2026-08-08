import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { ArticleEditor } from "@/components/admin/article-editor";
import { EditorialCollaboration } from "@/components/admin/editorial-collaboration";
import { RevisionHistory } from "@/components/admin/revision-history";
import { WorkflowActions } from "@/components/admin/workflow-actions";
import { adminCopy } from "@/i18n/admin-copy";
import { hasLocale } from "@/i18n/config";
import {
  getAdminSession,
  getArticle,
  getArticleCollaboration,
  getArticleRevisions,
  getSupportingLibrary,
} from "@/lib/admin-api";
export default async function EditArticlePage({
  params,
}: PageProps<"/[locale]/admin/articles/[articleId]">) {
  const { locale, articleId } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  const [article, library, revisions, collaboration] = await Promise.all([
    getArticle(articleId),
    getSupportingLibrary(),
    getArticleRevisions(articleId),
    getArticleCollaboration(articleId),
  ]);
  if (!article || !collaboration) notFound();
  const copy = adminCopy[locale],
    previewLabel = {
      "tr-TR": "Önizle",
      "en-US": "Preview",
      "de-DE": "Vorschau",
    }[locale];
  return (
    <main className="admin-shell">
      <Link className="back-link" href={`/${locale}/admin/articles`}>
        ← {copy.back}
      </Link>{" "}
      <Link
        className="secondary-button preview-button"
        href={`/${locale}/admin/articles/${articleId}/preview`}
      >
        {previewLabel}
      </Link>
      <WorkflowActions article={article} roles={session.roles} copy={copy} />
      <section className="admin-panel">
        <h1 className="editor-title">{copy.editDraft}</h1>
        {["Draft", "Published"].includes(article.status) ? (
          <ArticleEditor
            copy={copy}
            article={article}
            categories={library.categories}
          />
        ) : (
          <p className="muted">{copy.lockedForReview}</p>
        )}
      </section>
      <EditorialCollaboration
        articleId={articleId}
        locale={locale}
        data={collaboration}
      />
      <RevisionHistory locale={locale} revisions={revisions} />
    </main>
  );
}
