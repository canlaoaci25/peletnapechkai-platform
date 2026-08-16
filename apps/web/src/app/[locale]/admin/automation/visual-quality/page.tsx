import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { VisualQualityDesk, type VisualQualityReport } from "@/components/admin/visual-quality-desk";
import { hasLocale } from "@/i18n/config";
import { getAdminSession } from "@/lib/admin-api";

const apiUrl=process.env.API_INTERNAL_URL??"http://localhost:5267";
export default async function VisualQualityPage({params}:PageProps<"/[locale]/admin/automation/visual-quality">){const{locale}=await params;if(!hasLocale(locale))redirect("/tr-TR/admin/login");const session=await getAdminSession();if(!session)redirect(`/${locale}/admin/login`);if(!session.roles.some(role=>["Owner","Admin"].includes(role)))redirect(`/${locale}/admin`);const cookieStore=await cookies();const response=await fetch(new URL("/api/v1/admin/automation/visual-quality",apiUrl),{headers:{cookie:cookieStore.toString()},cache:"no-store"});if(!response.ok)redirect(`/${locale}/admin/automation`);return <main className="admin-shell admin-dashboard-shell"><VisualQualityDesk locale={locale} report={await response.json() as VisualQualityReport}/></main>}
