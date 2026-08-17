import Link from "next/link";
import { redirect } from "next/navigation";
import { LanguageList, LocalizationWorkQueue } from "@/components/admin/language-manager";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getManagedLocales, getLocalizationWork } from "@/lib/admin-api";

const pageCopy = {
  "tr-TR": { kicker:"ULUSLARARASI YAYIN SAĞLIĞI", title:"Diller arasındaki yayın farkını kapatın", intro:"Çeviri, inceleme ve yerelleştirilmiş taxonomy bütünlüğünü tek bakışta yönetin.", add:"+ Dil ekle" },
  "en-US": { kicker:"INTERNATIONAL PUBLISHING HEALTH", title:"Close the gap between editions", intro:"Manage translation, review and localized taxonomy integrity in one place.", add:"+ Add locale" },
  "de-DE": { kicker:"INTERNATIONALE PUBLIKATIONSGESUNDHEIT", title:"Lücken zwischen den Ausgaben schließen", intro:"Übersetzungen, Prüfungen und lokalisierte Taxonomie zentral verwalten.", add:"+ Sprache hinzufügen" },
  "fr-FR": { kicker:"SANTÉ DE LA PUBLICATION INTERNATIONALE", title:"Réduire les écarts entre les éditions", intro:"Gérez les traductions, les révisions et l’intégrité de la taxonomie localisée.", add:"+ Ajouter une langue" },
} as const;

export default async function LanguagesPage({
  params,
}: PageProps<"/[locale]/admin/languages">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  if (!session.roles.some((role) => ["Owner", "Admin"].includes(role)))
    redirect(`/${locale}/admin`);
  const copy = pageCopy[locale];
  const [locales, work] = await Promise.all([getManagedLocales(), getLocalizationWork()]);
  return (
    <main className="admin-shell admin-dashboard-shell">
      <header className="admin-command-header">
        <div>
          <p className="section-kicker">{copy.kicker}</p>
          <h1>{copy.title}</h1>
          <p>{copy.intro}</p>
        </div>
        <Link className="primary-link" href={`/${locale}/admin/languages/new`}>
          {copy.add}
        </Link>
      </header>
      <LanguageList locale={locale} locales={locales} />
      <LocalizationWorkQueue locale={locale} work={work} />
    </main>
  );
}
