import Link from "next/link";
import { redirect } from "next/navigation";
import { TaxonomyManager } from "@/components/admin/taxonomy-manager";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getSupportingLibrary } from "@/lib/admin-api";
export default async function CategoriesPage({
  params,
}: PageProps<"/[locale]/admin/articles/categories">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  if (
    !session.roles.some((role) => ["Owner", "Admin", "Editor"].includes(role))
  )
    redirect(`/${locale}/admin/articles`);
  const library = await getSupportingLibrary();
  return (
    <main className="admin-shell">
      <header className="admin-command-header">
        <div>
          <p className="section-kicker">İÇERİK MODÜLÜ</p>
          <h1>Kategoriler</h1>
          <p>
            Türkçe kategorileri ekleyin, düzenleyin ve kullanılmayanları silin.
          </p>
        </div>
        <Link className="secondary-button" href={`/${locale}/admin/articles`}>
          ← İçeriklere dön
        </Link>
      </header>
      <TaxonomyManager items={library.categories} kind="categories" health={library.taxonomyHealth} />
    </main>
  );
}
