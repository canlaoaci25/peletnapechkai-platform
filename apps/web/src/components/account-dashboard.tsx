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
  ReadingRitual,
  ReadingDigest,
  PushPreference,
} from "@/lib/admin-api";
import type { Locale } from "@/i18n/config";
import { memberCopy, memberHubCopy } from "@/i18n/member-copy";
import { PushPreferences } from "@/components/push-preferences";

export function AccountDashboard({
  account,
  locale,
  initialSaved,
  initialFollowed,
  initialFeed,
  progressCount,
  initialRitual,
  initialDigest,
  initialPush,
}: {
  account: MemberAccount;
  locale: Locale;
  initialSaved: SavedArticle[];
  initialFollowed: FollowedCategory[];
  initialFeed: PersonalFeedArticle[];
  progressCount: number;
  initialRitual: ReadingRitual | null;
  initialDigest: ReadingDigest | null;
  initialPush: PushPreference | null;
}) {
  const copy = memberCopy[locale],
    hub = memberHubCopy[locale],
    router = useRouter();
  const [message, setMessage] = useState(""),
    [saved, setSaved] = useState(initialSaved),
    [followed, setFollowed] = useState(initialFollowed),
    [query, setQuery] = useState(""),
    [type, setType] = useState("all"),
    [ritual,setRitual]=useState(initialRitual);
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
  async function updateGoal(goal:number){
    const response=await fetch("/api/admin/account/reading-ritual",{method:"PUT",headers:{"content-type":"application/json","x-csrf-token":await csrf()},body:JSON.stringify({goal})});
    if(response.ok){setRitual(current=>current?{...current,goal}:current);setMessage(hub.ritualSaved)}else setMessage(copy.failed);
  }
  return (
    <div className="member-dashboard">
      {ritual&&<section className="reading-ritual" aria-labelledby="reading-ritual-title">
        <div className="reading-ritual-copy"><p className="section-kicker">{hub.ritualKicker}</p><h2 id="reading-ritual-title">{hub.ritualTitle}</h2><p>{hub.ritualDescription}</p>
          <div className="ritual-stats"><span><strong>{ritual.completed}/{ritual.goal}</strong>{hub.ritualCompleted}</span><span><strong>{ritual.activeDays}</strong>{hub.ritualActiveDays}</span></div>
          <div className="ritual-meter" role="progressbar" aria-label={`${ritual.completed}/${ritual.goal} ${hub.ritualCompleted}`} aria-valuemin={0} aria-valuemax={ritual.goal} aria-valuenow={Math.min(ritual.completed,ritual.goal)}><i style={{width:`${Math.min(100,(ritual.completed/ritual.goal)*100)}%`}}/></div>
          <fieldset><legend>{hub.ritualGoal}</legend>{[1,3,5].map(goal=><button key={goal} type="button" aria-pressed={ritual.goal===goal} onClick={()=>void updateGoal(goal)}>{goal}</button>)}</fieldset>
        </div>
        <div className="ritual-next"><p className="section-kicker">{hub.ritualNext}</p>{ritual.next?<>{ritual.next.cover&&<Link href={`/${locale}/articles/${ritual.next.slug}`} tabIndex={-1}><Image src={ritual.next.cover.url} alt="" width={640} height={360} sizes="(max-width: 760px) calc(100vw - 48px), 36vw"/></Link>}<h2><Link href={`/${locale}/articles/${ritual.next.slug}`}>{ritual.next.title}</Link></h2><p>{ritual.next.summary}</p></>:<Link className="ritual-discover" href={`/${locale}/topics`}>{hub.ritualDiscover} →</Link>}</div>
      </section>}
      {initialDigest&&<section className="reading-digest" aria-labelledby="reading-digest-title">
        <header><p className="section-kicker">{hub.digestKicker}</p><h2 id="reading-digest-title">{hub.digestTitle}</h2><p>{hub.digestDescription}</p></header>
        {initialDigest.items.length===0?<div className="saved-empty"><p>{hub.digestEmpty}</p><Link href={`/${locale}/topics`}>{hub.ritualDiscover} →</Link></div>:<ol>{initialDigest.items.map((item,index)=>{const href=`/${locale}/articles/${item.slug}${item.anchor?`#${item.anchor}`:""}`;return <li key={item.slug}><span>{String(index+1).padStart(2,"0")}</span>{item.cover&&<Link className="digest-cover" href={href} tabIndex={-1}><Image src={item.cover.url} alt="" fill sizes="(max-width: 700px) 92px, 160px"/></Link>}<div><p className="digest-reason">{item.reason==="continue"?hub.digestContinue:item.reason==="followed"?`${hub.digestFollowed}${item.topic?` · ${item.topic}`:""}`:hub.digestSaved}{item.progress?` · ${item.progress}%`:""}</p><h3><Link href={href}>{item.title}</Link></h3><p>{item.summary}</p></div></li>})}</ol>}
      </section>}
      <section className="account-summary">
        <p className="section-kicker">{copy.membership}</p>
        <h1>{hub.title}</h1>
        <p>{hub.description}</p>
        <p>{account.email}</p>
        <span className={account.emailConfirmed ? "verified" : "pending"}>
          {account.emailConfirmed
            ? `✓ ${copy.verified}`
            : `! ${account.verificationAvailable ? copy.verificationPending : copy.verificationUnavailable}`}
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
        <PushPreferences locale={locale} initial={initialPush} />
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
