import Link from "next/link";
import { redirect } from "next/navigation";
import { LanguageList } from "@/components/admin/language-manager";
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
          <p className="section-kicker">ULUSLARARASI YAYIN SAĞLIĞI</p>
          <h1>Diller arasındaki yayın farkını kapatın</h1>
          <p>Eksik ve güncelliğini yitiren çevirileri, inceleme borcunu ve yerelleştirilmemiş kategori yollarını tek bakışta izleyin.</p>
        </div>
        <Link className="primary-link" href={`/${locale}/admin/languages/new`}>
          + Dil ekle
        </Link>
      </header>
      <LanguageList locale={locale} locales={locales} />
    </main>
  );
}
