import Link from "next/link";
import { redirect } from "next/navigation";
import { UserManager } from "@/components/admin/user-manager";
import { hasLocale } from "@/i18n/config";
import { userCopy } from "@/i18n/user-copy";
import { getAdminSession, getUsers } from "@/lib/admin-api";

export default async function UsersPage({ params }: PageProps<"/[locale]/admin/users">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");
  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  if (!session.roles.some((role) => ["Owner", "Admin"].includes(role))) redirect(`/${locale}/admin`);
  const copy = userCopy[locale];
  return <main className="admin-shell narrow-admin"><Link className="back-link" href={`/${locale}/admin`}>← {copy.back}</Link><section className="admin-panel"><h1 className="editor-title">{copy.title}</h1><UserManager users={await getUsers()} copy={copy} /></section></main>;
}
