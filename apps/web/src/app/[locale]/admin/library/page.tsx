import Link from "next/link";
import { redirect } from "next/navigation";
import { LibraryManager } from "@/components/admin/library-manager";
import { hasLocale } from "@/i18n/config";
import { libraryCopy } from "@/i18n/library-copy";
import { getAdminSession, getMedia, getSupportingLibrary } from "@/lib/admin-api";

export default async function LibraryPage({params}:PageProps<"/[locale]/admin/library">){const {locale}=await params;if(!hasLocale(locale))redirect("/tr-TR/admin/login");const session=await getAdminSession();if(!session)redirect(`/${locale}/admin/login`);if(!session.roles.some(role=>["Owner","Admin","Editor"].includes(role)))redirect(`/${locale}/admin`);const [library,media]=await Promise.all([getSupportingLibrary(),getMedia()]);const copy=libraryCopy[locale];return <main className="admin-shell"><header className="admin-header"><div><Link href={`/${locale}/admin`}>{copy.back}</Link><h1>{copy.title}</h1></div></header><LibraryManager library={library} media={media} copy={copy}/></main>}
