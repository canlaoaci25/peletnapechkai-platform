import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { ReadyContentForm } from "@/components/admin/ready-content-form";
import { ReadyContentJobs, type ReadyContentJob } from "@/components/admin/ready-content-jobs";
import { hasLocale } from "@/i18n/config";
import { getAdminSession, getSupportingLibrary } from "@/lib/admin-api";

export default async function ReadyContentPage({params}:PageProps<"/[locale]/admin/automation/ready-content">){
  const{locale}=await params;if(!hasLocale(locale))redirect("/tr-TR/admin/login");const session=await getAdminSession();if(!session)redirect(`/${locale}/admin/login`);if(!session.roles.some(role=>["Owner","Admin"].includes(role)))redirect(`/${locale}/admin`);
  const[library,jobs]=await Promise.all([getSupportingLibrary(),getReadyContentJobs()]);const categories=library.categories.filter(category=>category.locale==="tr-TR").map(({id,name,slug})=>({id,name,slug}));
  return <main className="admin-shell admin-dashboard-shell ready-content-page"><header className="admin-command-header"><div><p className="section-kicker">AI HAZIR</p><h1>Hazır içerik oluştur</h1><p>Popüler yayınları araştıran, özgünlüğü denetleyen ve seçilen tüm fazlar bittiğinde doğrudan yayımlayan kalıcı içerik işi oluşturun.</p></div></header><ReadyContentJobs locale={locale} initialJobs={jobs}/><ReadyContentForm locale={locale} categories={categories} runnerEnabled={true}/></main>;
}

async function getReadyContentJobs(){const cookieStore=await cookies();const apiUrl=process.env.API_INTERNAL_URL??"http://localhost:5267";const response=await fetch(new URL("/api/v1/admin/automation/",apiUrl),{headers:{cookie:cookieStore.toString()},cache:"no-store"});if(!response.ok)return [];const jobs=await response.json() as ReadyContentJob[];return jobs.filter(job=>job.type==="ReadyContentGeneration")}
