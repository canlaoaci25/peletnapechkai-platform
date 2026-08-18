import { redirect } from "next/navigation";
import { AccountDashboard } from "@/components/account-dashboard";
import { ContinueReading } from "@/components/continue-reading";
import { hasLocale } from "@/i18n/config";
import {
  getFollowedCategories,
  getMemberAccount,
  getPersonalFeed,
  getReadingProgress,
  getReadingDigest,
  getReadingRitual,
  getSavedArticles,
  getPushPreference,
} from "@/lib/admin-api";
export default async function AccountPage({
  params,
}: PageProps<"/[locale]/account">) {
  const { locale } = await params;
  if (!hasLocale(locale)) redirect("/tr-TR");
  const account = await getMemberAccount();
  if (!account) redirect(`/${locale}/account/login`);
  const [saved, followed, feed, progress, ritual, digest, push] = await Promise.all([
    getSavedArticles(locale),
    getFollowedCategories(locale),
    getPersonalFeed(locale),
    getReadingProgress(locale),
    getReadingRitual(locale),
    getReadingDigest(locale),
    getPushPreference(),
  ]);
  return (
    <main className="account-dashboard-page">
      <ContinueReading locale={locale} items={progress} />
      <AccountDashboard
        account={account}
        locale={locale}
        initialSaved={saved}
        initialFollowed={followed}
        initialFeed={feed}
        progressCount={progress.length}
        initialRitual={ritual}
        initialDigest={digest}
        initialPush={push}
      />
    </main>
  );
}
