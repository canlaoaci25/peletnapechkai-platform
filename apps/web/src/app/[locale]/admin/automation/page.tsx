import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import {
  AutomationCenter,
  type AutomationJob,
  type AutomationScan,
} from "@/components/admin/automation-center";
import { hasLocale } from "@/i18n/config";
import { getAdminSession } from "@/lib/admin-api";

const apiUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5267";

async function get<T>(path: string) {
  const cookieStore = await cookies();
  const response = await fetch(new URL(path, apiUrl), {
    headers: { cookie: cookieStore.toString() },
    cache: "no-store",
  });
  return response.ok ? (await response.json()) as T : null;
}

export default async function AutomationPage({ params }: PageProps<"/[locale]/admin/automation">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR/admin/login");

  const session = await getAdminSession();
  if (!session) redirect(`/${locale}/admin/login`);
  if (!session.roles.some((role) => ["Owner", "Admin"].includes(role))) redirect(`/${locale}/admin`);

  const [scan, jobs] = await Promise.all([
    get<AutomationScan>("/api/v1/admin/automation/scan"),
    get<AutomationJob[]>("/api/v1/admin/automation/"),
  ]);
  if (!scan) redirect(`/${locale}/admin`);

  return (
    <main className="admin-shell admin-dashboard-shell">
      <header className="admin-command-header">
        <div>
          <p className="section-kicker">AI HAZIR</p>
          <h1>Toplu çalıştırıcılar</h1>
          <p>Eksikleri tarayın, kalıcı kuyruğa alın ve faz ilerlemesini tek ekrandan izleyin.</p>
        </div>
      </header>
      <AutomationCenter initialScan={scan} initialJobs={jobs ?? []} />
    </main>
  );
}
