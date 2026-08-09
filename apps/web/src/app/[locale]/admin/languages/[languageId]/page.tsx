import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { LanguageEditForm } from "@/components/admin/language-manager";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getManagedLocales } from "@/lib/admin-api";

export default async function EditLanguagePage({
  params,
}: PageProps<"/[locale]/admin/languages/[languageId]">) {
  const { locale, languageId } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  if (!session.roles.some((role) => ["Owner", "Admin"].includes(role)))
    redirect(`/${locale}/admin`);
  const language = (await getManagedLocales()).find(
    (item) => item.id === languageId,
  );
  if (!language) notFound();
  return (
    <main className="admin-shell admin-dashboard-shell">
      <header className="admin-command-header">
        <div>
          <p className="section-kicker">DİL DÜZENLE</p>
          <h1>{language.nativeName}</h1>
          <p>Dil durumunu, görünen adları ve ülke eşleşmelerini yönetin.</p>
        </div>
        <Link className="secondary-button" href={`/${locale}/admin/languages`}>
          ← Dil listesi
        </Link>
      </header>
      <LanguageEditForm locale={language} />
    </main>
  );
}
