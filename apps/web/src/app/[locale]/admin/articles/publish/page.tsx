import { redirect } from "next/navigation";
import { PublicationQueue } from "@/components/admin/publication-queue";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getArticles } from "@/lib/admin-api";

export default async function PublishArticlesPage({
  params,
}: PageProps<"/[locale]/admin/articles/publish">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  if (
    !session.roles.some((role) => ["Owner", "Admin", "Editor"].includes(role))
  )
    redirect(`/${locale}/admin/articles`);
  const articles = (await getArticles()).filter(
    (article) => !["Published", "Archived"].includes(article.status),
  );
  return (
    <main className="admin-shell admin-dashboard-shell">
      <header className="admin-command-header">
        <div>
          <p className="section-kicker">İÇERİK MODÜLÜ</p>
          <h1>Makale yayın</h1>
          <p>Onay bekleyen taslakları inceleyin ve doğrudan yayına alın.</p>
        </div>
      </header>
      <PublicationQueue locale={locale} articles={articles} />
    </main>
  );
}
