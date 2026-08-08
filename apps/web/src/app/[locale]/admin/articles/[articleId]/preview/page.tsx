import Image from "next/image";
import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getArticle } from "@/lib/admin-api";
export default async function PreviewPage({
  params,
}: PageProps<"/[locale]/admin/articles/[articleId]/preview">) {
  const { locale, articleId } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const [session, article] = await Promise.all([
    getAdminSession(),
    getArticle(articleId),
  ]);
  if (!session) redirect(`/${locale}/admin/login`);
  if (!article) notFound();
  const labels = {
    "tr-TR": { preview: "Taslak önizleme", back: "Düzenlemeye dön" },
    "en-US": { preview: "Draft preview", back: "Back to editor" },
    "de-DE": { preview: "Entwurfsvorschau", back: "Zurück zum Editor" },
  }[locale];
  return (
    <main className="preview-shell">
      <nav>
        <Link href={`/${locale}/admin/articles/${articleId}`}>
          ← {labels.back}
        </Link>
        <strong>{labels.preview}</strong>
        <span>{article.status}</span>
      </nav>
      <article className="article-page">
        <header>
          <p className="section-kicker">{article.type}</p>
          <h1>{article.title}</h1>
          <p className="article-lead">{article.summary}</p>
        </header>
        {article.coverMediaAssetId && article.coverAltText && (
          <figure className="article-cover">
            <Image
              src={`/api/admin/media/${article.coverMediaAssetId}`}
              alt={article.coverAltText}
              width={1200}
              height={675}
              sizes="780px"
              unoptimized
            />
            {(article.coverCaption || article.coverCredit) && (
              <figcaption>
                {article.coverCaption}
                {article.coverCaption && article.coverCredit && " — "}
                {article.coverCredit}
              </figcaption>
            )}
          </figure>
        )}
        {article.body.trimStart().startsWith("<") ? (
          <div
            className="article-body rich-article-body"
            dangerouslySetInnerHTML={{ __html: article.body }}
          />
        ) : (
          <div className="article-body">
            {article.body
              .split(/\r?\n\s*\r?\n/)
              .filter(Boolean)
              .map((paragraph, index) => (
                <p key={index}>{paragraph}</p>
              ))}
          </div>
        )}
      </article>
    </main>
  );
}
