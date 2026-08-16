import Link from "next/link";
import type { ArticleSummary, SystemStatus } from "@/lib/admin-api";
const labels: Record<string, string> = {
  Draft: "Taslak",
  InEditorialReview: "Editoryal inceleme",
  InSeoReview: "SEO incelemesi",
  Scheduled: "Planlandı",
  Published: "Yayında",
  Archived: "Arşivlendi",
};
export function AdminOverview({
  locale,
  articles,
  status,
}: {
  locale: string;
  articles: ArticleSummary[];
  status: SystemStatus | null;
}) {
  const count = (v: string) =>
      status?.lifecycle?.[v] ?? articles.filter((x) => x.status === v).length,
    published = status?.published ?? count("Published"),
    total = status?.articles ?? articles.length,
    drafts = count("Draft"),
    review = count("InEditorialReview") + count("InSeoReview"),
    scheduled = count("Scheduled"),
    rate = total ? Math.round((published / total) * 100) : 0;
  const types = Object.entries(
    status?.types ??
      articles.reduce<Record<string, number>>(
        (a, x) => ({ ...a, [x.type]: (a[x.type] ?? 0) + 1 }),
        {},
      ),
  ).sort((a, b) => b[1] - a[1]);
  const health = status?.productionHealth;
  const healthState = !health?.available ? "unavailable" : health.stale ? "stale" : health.healthy ? "healthy" : "failed";
  const healthLabel = {healthy:"Tüm kapılar açık",failed:"Müdahale gerekiyor",stale:"Kontrol bayat",unavailable:"Sağlık verisi yok"}[healthState];
  return (
    <div className="overview-grid">
      <section
        className="overview-kpis"
        aria-label="Temel yayın istatistikleri"
      >
        <article>
          <span>Toplam içerik</span>
          <strong>{total}</strong>
          <small>Tüm yayın yaşam döngüsü</small>
        </article>
        <article>
          <span>Yayındaki içerik</span>
          <strong>{published}</strong>
          <small>%{rate} yayın oranı</small>
        </article>
        <article>
          <span>Taslak</span>
          <strong>{drafts}</strong>
          <small>Hazırlanmayı bekliyor</small>
        </article>
        <article>
          <span>İnceleme kuyruğu</span>
          <strong>{review}</strong>
          <small>Editoryal ve SEO kontrolü</small>
        </article>
        <article>
          <span>Planlanan</span>
          <strong>{scheduled}</strong>
          <small>Yayın zamanı belirlenmiş</small>
        </article>
      </section>
      <section className="admin-panel overview-publication">
        <header>
          <div>
            <p className="section-kicker">YAYIN DURUMU</p>
            <h2>İçerik dağılımı</h2>
          </div>
          <strong>%{rate}</strong>
        </header>
        <div className="publication-progress">
          <span style={{ width: `${rate}%` }} />
        </div>
        <div className="distribution-list">
          {Object.entries(labels).map(([key, label]) => {
            const value = count(key);
            return (
              <div key={key}>
                <span>{label}</span>
                <div>
                  <span
                    style={{
                      width: `${total ? (value / total) * 100 : 0}%`,
                    }}
                  />
                </div>
                <strong>{value}</strong>
              </div>
            );
          })}
        </div>
      </section>
      <section className="admin-panel overview-types">
        <p className="section-kicker">İÇERİK PORTFÖYÜ</p>
        <h2>Türlere göre dağılım</h2>
        <dl>
          {types.map(([name, value]) => (
            <div key={name}>
              <dt>{name}</dt>
              <dd>{value}</dd>
            </div>
          ))}
        </dl>
      </section>
      {status && (
        <section className={`admin-panel overview-system health-${healthState}`}>
          <header className="health-heading">
            <div><p className="section-kicker">CANLI YAYIN GÜVENİ</p><h2>Production kapıları</h2></div>
            <strong><span className="health-dot" />{healthLabel}</strong>
          </header>
          <dl>
            <div>
              <dt>Servisler</dt><dd>{health?.servicesHealthy ?? 0} / {health?.servicesTotal ?? 0}</dd>
            </div>
            <div>
              <dt>Public uçlar</dt><dd>{health?.endpointsHealthy ?? 0} / {health?.endpointsTotal ?? 0}</dd>
            </div>
            <div>
              <dt>TLS süresi</dt><dd>{health?.certificateDaysRemaining == null ? "—" : `${health.certificateDaysRemaining} gün`}</dd>
            </div>
            <div>
              <dt>Boş disk</dt><dd>{health?.freeDiskGb == null ? `${(status.diskFreeBytes / 1024 / 1024 / 1024).toFixed(1)} GB` : `${health.freeDiskGb.toFixed(1)} GB`}</dd>
            </div>
          </dl>
          {!!health?.failures.length && <ul className="health-failures" aria-label="Sağlık kontrolü hataları">{health.failures.map((failure)=><li key={failure}>{failure}</li>)}</ul>}
          <time dateTime={health?.checkedAt ?? status.checkedAt}>
            Son production kontrolü:{" "}
            {new Intl.DateTimeFormat("tr-TR", {
              dateStyle: "medium",
              timeStyle: "short",
            }).format(new Date(health?.checkedAt ?? status.checkedAt))}
          </time>
        </section>
      )}
      <section className="admin-panel overview-recent">
        <header>
          <div>
            <p className="section-kicker">SON HAREKET</p>
            <h2>Yakın zamanda güncellenenler</h2>
          </div>
          <Link href={`/${locale}/admin/articles`}>Tüm içerikler →</Link>
        </header>
        <div>
          {articles.slice(0, 6).map((x) => (
            <article key={x.id}>
              <span>
                <Link href={`/${locale}/admin/articles/${x.id}`}>
                  <strong>{x.title}</strong>
                </Link>
                <small>
                  {x.type} · {x.locale}
                </small>
              </span>
              <span className={`status-badge status-${x.status.toLowerCase()}`}>
                {labels[x.status] ?? x.status}
              </span>
              <time dateTime={x.updatedAt}>
                {new Intl.DateTimeFormat("tr-TR", {
                  dateStyle: "medium",
                }).format(new Date(x.updatedAt))}
              </time>
            </article>
          ))}
        </div>
      </section>
    </div>
  );
}
