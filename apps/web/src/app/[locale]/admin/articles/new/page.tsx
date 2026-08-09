import Link from "next/link";
import { redirect } from "next/navigation";
import { ArticleEditor } from "@/components/admin/article-editor";
import { adminCopy } from "@/i18n/admin-copy";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getSupportingLibrary } from "@/lib/admin-api";
export default async function NewArticlePage({
  params,
}: PageProps<"/[locale]/admin/articles/new">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  if (
    !session.roles.some((x) =>
      ["Owner", "Admin", "Editor", "Author"].includes(x),
    )
  )
    redirect(`/${locale}/admin/articles`);
  const copy = adminCopy[locale],
    library = await getSupportingLibrary(),
    canPublishImmediately = session.roles.some((role) =>
      ["Owner", "Admin", "Editor"].includes(role),
    );
  return (
    <main className="admin-shell new-article-page">
      <header className="admin-command-header">
        <div>
          <p className="section-kicker">YENİ YAYIN</p>
          <h1>Yeni içerik oluştur</h1>
          <p>
            Kategori ve zengin içerik alanlarıyla yeni bir Türkçe taslak
            başlatın.
          </p>
        </div>
        <Link className="secondary-button" href={`/${locale}/admin/articles`}>
          ← İçeriklere dön
        </Link>
      </header>
      <section className="admin-panel new-article-editor">
        <ArticleEditor
          copy={copy}
          categories={library.categories}
          canPublishImmediately={canPublishImmediately}
        />
      </section>
    </main>
  );
}
