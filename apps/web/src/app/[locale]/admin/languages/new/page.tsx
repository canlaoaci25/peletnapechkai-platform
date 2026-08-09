import Link from "next/link";
import { redirect } from "next/navigation";
import { LanguageCreateForm } from "@/components/admin/language-manager";
import { hasLocale } from "@/i18n/config";
import {
  getAdminSession,
  getLocaleCatalog,
  getManagedLocales,
} from "@/lib/admin-api";

export default async function AddLanguagePage({
  params,
}: PageProps<"/[locale]/admin/languages/new">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  if (!session.roles.some((role) => ["Owner", "Admin"].includes(role)))
    redirect(`/${locale}/admin`);
  const [catalog, locales] = await Promise.all([
    getLocaleCatalog(),
    getManagedLocales(),
  ]);
  return (
    <main className="admin-shell admin-dashboard-shell">
      <header className="admin-command-header">
        <div>
          <p className="section-kicker">YERELLEŞTİRME</p>
          <h1>Dil ekle</h1>
          <p>
            Katalogdan dili ve ana bölgesini seçin; ülke eşleşmeleri otomatik
            oluşturulsun.
          </p>
        </div>
        <Link className="secondary-button" href={`/${locale}/admin/languages`}>
          ← Dil listesi
        </Link>
      </header>
      <LanguageCreateForm
        catalog={catalog}
        existingCodes={locales.map((item) => item.code)}
        locale={locale}
      />
    </main>
  );
}
