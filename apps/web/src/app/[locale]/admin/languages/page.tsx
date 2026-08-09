import { redirect } from "next/navigation";
import { LanguageManager } from "@/components/admin/language-manager";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getManagedLocales } from "@/lib/admin-api";

export default async function LanguagesPage({
  params,
}: PageProps<"/[locale]/admin/languages">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  if (!session.roles.some((role) => ["Owner", "Admin"].includes(role)))
    redirect(`/${locale}/admin`);
  const locales = await getManagedLocales();
  return (
    <main className="admin-shell admin-dashboard-shell">
      <header className="admin-command-header">
        <div>
          <p className="section-kicker">YERELLEŞTİRME</p>
          <h1>Dil işlemleri</h1>
          <p>
            Dilleri ve otomatik ülke eşleşmelerini yönetin. Yeni diller
            çeviriler hazırlanana kadar pasif başlar.
          </p>
        </div>
      </header>
      <LanguageManager locales={locales} />
    </main>
  );
}
