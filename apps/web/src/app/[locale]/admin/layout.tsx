import type { Metadata } from "next";
import { AdminFrame } from "@/components/admin/admin-frame";
import { hasLocale } from "@/i18n/config";
import { getAdminSession } from "@/lib/admin-api";

export const metadata: Metadata = {
  robots: { index: false, follow: false, noarchive: true },
};

export default async function AdminLayout({ children, params }: LayoutProps<"/[locale]/admin">) {
  const {locale}=await params;if(!hasLocale(locale))return children;const session=await getAdminSession();
  return session?<AdminFrame locale={locale} session={session}>{children}</AdminFrame>:children;
}
