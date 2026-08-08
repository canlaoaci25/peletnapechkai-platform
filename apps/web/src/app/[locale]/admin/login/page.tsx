import { redirect } from "next/navigation";

import { LoginForm } from "@/components/admin/login-form";
import { adminCopy } from "@/i18n/admin-copy";
import { hasLocale } from "@/i18n/config";
import { getAdminSession } from "@/lib/admin-api";
import { siteConfig } from "@/config/site";

export default async function AdminLoginPage({ params }: PageProps<"/[locale]/admin/login">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  if (await getAdminSession()) redirect(`/${locale}/admin`);
  const copy = adminCopy[locale];

  return (
    <main className="admin-login-shell">
      <section className="admin-login-card">
        <p className="section-kicker">{siteConfig.name}</p>
        <h1>{copy.loginTitle}</h1>
        <p>{copy.loginLead}</p>
        <LoginForm locale={locale} copy={copy} />
      </section>
    </main>
  );
}
