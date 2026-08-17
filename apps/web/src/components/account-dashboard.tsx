"use client";
import Image from "next/image";
import Link from "next/link";
import { FormEvent, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import type {
  FollowedCategory,
  MemberAccount,
  PersonalFeedArticle,
  SavedArticle,
} from "@/lib/admin-api";
import type { Locale } from "@/i18n/config";
import { memberCopy, memberHubCopy } from "@/i18n/member-copy";

export function AccountDashboard({
  account,
  locale,
  initialSaved,
  initialFollowed,
  initialFeed,
  progressCount,
}: {
  account: MemberAccount;
  locale: Locale;
  initialSaved: SavedArticle[];
  initialFollowed: FollowedCategory[];
  initialFeed: PersonalFeedArticle[];
  progressCount: number;
}) {
  const copy = memberCopy[locale],
    hub = memberHubCopy[locale],
    router = useRouter();
  const [message, setMessage] = useState(""),
    [saved, setSaved] = useState(initialSaved),
    [followed, setFollowed] = useState(initialFollowed),
    [query, setQuery] = useState(""),
    [type, setType] = useState("all");
  const types = useMemo(
    () => [...new Set(saved.map((item) => item.type))].sort(),
    [saved],
  );
  const visibleSaved = useMemo(() => {
    const needle = query.trim().toLocaleLowerCase(locale);
    return saved.filter(
      (item) =>
        (type === "all" || item.type === type) &&
        (!needle ||
          `${item.title} ${item.summary} ${item.type}`
            .toLocaleLowerCase(locale)
            .includes(needle)),
    );
  }, [saved, type, query, locale]);
  async function csrf() {
    const response = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
    return ((await response.json()) as { token: string }).token;
  }
  async function send(path: string, body: unknown) {
    setMessage("");
    const token = await csrf();
    const response = await fetch(`/api/admin/account/${path}`, {
      method: path === "profile" ? "PUT" : "POST",
      headers: { "content-type": "application/json", "x-csrf-token": token },
      body: JSON.stringify(body),
    });
    setMessage(response.ok ? "✓" : copy.failed);
    if (response.ok) router.refresh();
  }
  function profile(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    void send("profile", { displayName: data.get("displayName") });
  }
  function password(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    void send("password", {
      currentPassword: data.get("currentPassword"),
      newPassword: data.get("newPassword"),
    });
  }
  async function remove(item: SavedArticle) {
    const response = await fetch(
      `/api/admin/account/saved/${encodeURIComponent(item.locale)}/${encodeURIComponent(item.slug)}`,
      { method: "DELETE", headers: { "x-csrf-token": await csrf() } },
    );
    if (response.ok) {
      setSaved((current) =>
        current.filter((savedItem) => savedItem.slug !== item.slug),
      );
      setMessage(copy.removedSuccess);
    } else setMessage(copy.failed);
  }
  async function unfollow(item: FollowedCategory) {
    const response = await fetch(
      `/api/admin/account/following/${encodeURIComponent(item.locale)}/${encodeURIComponent(item.slug)}`,
      { method: "DELETE", headers: { "x-csrf-token": await csrf() } },
    );
    if (response.ok) {
      setFollowed((current) =>
        current.filter((topic) => topic.slug !== item.slug),
      );
      setMessage(copy.unfollowSuccess);
      router.refresh();
    } else setMessage(copy.failed);
  }
  return (
    <div className="member-dashboard">
      <section className="account-summary">
        <p className="section-kicker">{copy.membership}</p>
        <h1>{hub.title}</h1>
        <p>{hub.description}</p>
        <p>{account.email}</p>
        <span className={account.emailConfirmed ? "verified" : "pending"}>
          {account.emailConfirmed ? "✓" : "!"}
        </span>
        <nav className="member-hub-nav" aria-label={hub.title}>
          <a href="#continue-reading-title">
            <strong>{progressCount}</strong>
            <span>{hub.continueLabel}</span>
          </a>
          <a href="#personal-feed-title">
            <strong>{initialFeed.length}</strong>
            <span>{hub.newLabel}</span>
          </a>
          <a href="#reading-list-title">
            <strong>{saved.length}</strong>
            <span>{hub.savedLabel}</span>
          </a>
          <a href="#followed-topics-title">
            <strong>{followed.length}</strong>
            <span>{hub.topicsLabel}</span>
          </a>
        </nav>
      </section>
      <section
        className="personal-discovery"
        aria-labelledby="personal-feed-title"
      >
        <Header
          id="personal-feed-title"
          kicker="BOECL PERSONAL"
          title={copy.feedTitle}
          description={copy.feedDescription}
          count={initialFeed.length}
        />
        {initialFeed.length === 0 ? (
          <Empty
            text={copy.feedEmpty}
            locale={locale}
            action={copy.savedEmptyAction}
          />
        ) : (
          <div className="personal-feed-grid">
            {initialFeed.map((item, index) => (
              <article
                className={index === 0 ? "personal-feed-lead" : "saved-card"}
                key={item.slug}
              >
                {item.cover && (
                  <Link
                    className="personal-feed-cover"
                    href={`/${locale}/articles/${item.slug}`}
                    tabIndex={-1}
                  >
                    <Image
                      src={item.cover.url}
                      alt=""
                      fill
                      sizes={
                        index === 0
                          ? "(max-width: 700px) 100vw, 50vw"
                          : "(max-width: 700px) 100vw, 280px"
                      }
                    />
                  </Link>
                )}
                <div>
                  <p className="section-kicker">
                    {item.categories.join(" · ")}
                  </p>
                  <h3>
                    <Link href={`/${locale}/articles/${item.slug}`}>
                      {item.title}
                    </Link>
                  </h3>
                  <p>{item.summary}</p>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
      <section className="reading-list" aria-labelledby="reading-list-title">
        <Header
          id="reading-list-title"
          kicker="BOECL LIBRARY"
          title={copy.savedTitle}
          description={copy.savedDescription}
          count={saved.length}
        />
        {saved.length === 0 ? (
          <Empty
            text={copy.savedEmpty}
            locale={locale}
            action={copy.savedEmptyAction}
          />
        ) : (
          <>
            <div className="library-controls">
              <label>
                {hub.searchLabel}
                <input
                  type="search"
                  value={query}
                  onChange={(event) => setQuery(event.target.value)}
                  placeholder={hub.searchPlaceholder}
                />
              </label>
              <label>
                {hub.filterLabel}
                <select
                  value={type}
                  onChange={(event) => setType(event.target.value)}
                >
                  <option value="all">{hub.allLabel}</option>
                  {types.map((item) => (
                    <option key={item}>{item}</option>
                  ))}
                </select>
              </label>
              <output aria-live="polite">
                {visibleSaved.length} {hub.resultsLabel}
              </output>
            </div>
            {visibleSaved.length === 0 ? (
              <div className="saved-empty">
                <p>{hub.noResults}</p>
              </div>
            ) : (
              <div className="saved-grid">
                {visibleSaved.map((item) => (
                  <article className="saved-card" key={item.slug}>
                    {item.cover && (
                      <Link
                        href={`/${locale}/articles/${item.slug}`}
                        tabIndex={-1}
                      >
                        <Image
                          src={item.cover.url}
                          alt={item.cover.altText}
                          width={560}
                          height={315}
                          sizes="(max-width: 700px) calc(100vw - 48px), 320px"
                        />
                      </Link>
                    )}
                    <div>
                      <p className="section-kicker">{item.type}</p>
                      <h3>
                        <Link href={`/${locale}/articles/${item.slug}`}>
                          {item.title}
                        </Link>
                      </h3>
                      <p>{item.summary}</p>
                      <button type="button" onClick={() => void remove(item)}>
                        {copy.remove}
                      </button>
                    </div>
                  </article>
                ))}
              </div>
            )}
          </>
        )}
      </section>
      <section
        className="followed-topics"
        aria-labelledby="followed-topics-title"
      >
        <Header
          id="followed-topics-title"
          kicker="BOECL TOPICS"
          title={copy.topicsTitle}
          description={copy.topicsDescription}
          count={followed.length}
        />
        {followed.length === 0 ? (
          <Empty
            text={copy.topicsEmpty}
            locale={locale}
            action={copy.savedEmptyAction}
          />
        ) : (
          <div className="followed-topic-grid">
            {followed.map((item) => (
              <article key={item.slug}>
                <p className="section-kicker">{item.articleCount}</p>
                <h3>
                  <Link href={`/${locale}/categories/${item.slug}`}>
                    {item.title}
                  </Link>
                </h3>
                {item.description && <p>{item.description}</p>}
                <button type="button" onClick={() => void unfollow(item)}>
                  {copy.remove}
                </button>
              </article>
            ))}
          </div>
        )}
      </section>
      <div className="member-settings">
        <form className="account-settings-card" onSubmit={profile}>
          <h2>{copy.profile}</h2>
          <label>
            {copy.displayName}
            <input
              name="displayName"
              defaultValue={account.displayName}
              required
              minLength={2}
              maxLength={160}
            />
          </label>
          <button>{copy.saveProfile}</button>
        </form>
        <form className="account-settings-card" onSubmit={password}>
          <h2>{copy.changePassword}</h2>
          <label>
            {copy.currentPassword}
            <input
              type="password"
              name="currentPassword"
              required
              autoComplete="current-password"
            />
          </label>
          <label>
            {copy.newPassword}
            <input
              type="password"
              name="newPassword"
              required
              minLength={14}
              autoComplete="new-password"
            />
          </label>
          <button>{copy.updatePassword}</button>
        </form>
      </div>
      {message && (
        <p className="account-message" role="status">
          {message}
        </p>
      )}
    </div>
  );
}
function Header({
  id,
  kicker,
  title,
  description,
  count,
}: {
  id: string;
  kicker: string;
  title: string;
  description: string;
  count: number;
}) {
  return (
    <div className="reading-list-heading">
      <div>
        <p className="section-kicker">{kicker}</p>
        <h2 id={id}>{title}</h2>
        <p>{description}</p>
      </div>
      <strong>{count}</strong>
    </div>
  );
}
function Empty({
  text,
  locale,
  action,
}: {
  text: string;
  locale: Locale;
  action: string;
}) {
  return (
    <div className="saved-empty">
      <p>{text}</p>
      <Link href={`/${locale}/topics`}>{action} →</Link>
    </div>
  );
}
