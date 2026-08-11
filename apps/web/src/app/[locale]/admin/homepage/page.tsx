import { redirect } from "next/navigation";
import { HomepageManager } from "@/components/admin/homepage-manager";
import { hasLocale } from "@/i18n/config";
import { getAdminSession,getHomepageAdmin } from "@/lib/admin-api";
export default async function HomepageAdminPage({params}:PageProps<"/[locale]/admin/homepage">){const{locale}=await params;if(!hasLocale(locale))redirect("/tr-TR/admin/login");const session=await getAdminSession();if(!session)redirect(`/${locale}/admin/login`);if(!session.roles.some(role=>["Owner","Admin","Editor"].includes(role)))redirect(`/${locale}/admin`);const data=await getHomepageAdmin(locale);if(!data)redirect(`/${locale}/admin`);return <main className="admin-shell"><header className="admin-command-header"><div><p className="section-kicker">YAYIN VİTRİNİ</p><h1>Ana sayfa yönetimi</h1><p>Otomatik trend motorunu kullanın veya önemli yayınları elle sabitleyin.</p></div></header><HomepageManager locale={locale} data={data}/></main>}
