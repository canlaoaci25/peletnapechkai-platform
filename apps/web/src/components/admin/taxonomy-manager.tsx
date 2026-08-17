"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";

type Item = { id: string; locale: string; slug: string; name: string; articleCount?:number;publishedCount?:number };
type Health={publishedCount:number;uncategorizedCount:number;uncategorized:{id:string;slug:string;title:string;publishedAt:string}[]};
type Kind = "categories" | "tags";
export function TaxonomyManager({
  items,
  kind,
  health,
}: {
  items: Item[];
  kind: Kind;
  health?:Health;
}) {
  const router = useRouter(),
    [message, setMessage] = useState(""),
    [pending, setPending] = useState(false),
    category = kind === "categories",
    singular = category ? "kategori" : "etiket",
    turkish = items.filter((item) => item.locale === "tr-TR");
  async function token() {
    const response = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
    return ((await response.json()) as { token: string }).token;
  }
  async function request(path: string, method: string, body?: unknown) {
    setPending(true);
    setMessage("");
    try {
      const response = await fetch(`/api/admin/supporting/${kind}${path}`, {
        method,
        headers: {
          ...(body ? { "content-type": "application/json" } : {}),
          "x-csrf-token": await token(),
        },
        body: body ? JSON.stringify(body) : undefined,
      });
      if (!response.ok) {
        const error = (await response.json().catch(() => null)) as {
          message?: string;
        } | null;
        throw new Error(error?.message);
      }
      setMessage("İşlem tamamlandı.");
      router.refresh();
    } catch (error) {
      setMessage(
        error instanceof Error && error.message
          ? error.message
          : "İşlem tamamlanamadı.",
      );
    } finally {
      setPending(false);
    }
  }
  async function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget,
      data = new FormData(form);
    await request("", "POST", {
      locale: "tr-TR",
      slug: data.get("slug"),
      name: data.get("name"),
    });
    form.reset();
  }
  async function update(event: FormEvent<HTMLFormElement>, id: string) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    await request(`/${id}`, "PUT", {
      slug: data.get("slug"),
      name: data.get("name"),
    });
  }
  return (
    <div className="category-workspace">
      {category&&health&&<section className="taxonomy-health" aria-labelledby="taxonomy-health-title">
        <header><div><p className="section-kicker">KEŞİF BÜTÜNLÜĞÜ</p><h2 id="taxonomy-health-title">Taxonomy kapsama masası</h2><p>Yayımdaki Türkçe arşivin kategori kapsamını ve editoryal boşluklarını canlı veriden izleyin.</p></div><div className="taxonomy-score"><strong>{health.publishedCount?Math.round((health.publishedCount-health.uncategorizedCount)/health.publishedCount*100):100}%</strong><span>kapsama</span></div></header>
        <div className="taxonomy-metrics"><article><span>Yayımdaki içerik</span><strong>{health.publishedCount}</strong></article><article><span>Kategorisiz</span><strong>{health.uncategorizedCount}</strong></article><article><span>Türkçe konu</span><strong>{turkish.length}</strong></article></div>
        {health.uncategorized.length>0?<div className="taxonomy-debt"><h3>Öncelikli sınıflandırma kuyruğu</h3>{health.uncategorized.map(item=><a key={item.id} href={`/tr-TR/admin/articles/${item.id}`}><span>{item.title}</span><time dateTime={item.publishedAt}>{new Intl.DateTimeFormat("tr-TR",{dateStyle:"medium"}).format(new Date(item.publishedAt))}</time></a>)}</div>:<p className="taxonomy-complete">Tüm yayımlanmış Türkçe içerikler en az bir konuya bağlı.</p>}
      </section>}
      <form
        className="admin-form admin-panel category-create"
        onSubmit={create}
      >
        <header>
          <div>
            <p className="section-kicker">TÜRKÇE TAKSONOMİ</p>
            <h2>Yeni {singular}</h2>
          </div>
          <span>tr-TR</span>
        </header>
        <label>
          {category ? "Kategori" : "Etiket"} adı
          <input name="name" required maxLength={160} />
        </label>
        <label>
          URL kısa adı
          <input
            name="slug"
            required
            maxLength={160}
            pattern="[a-z0-9]+(?:-[a-z0-9]+)*"
            placeholder={category ? "yapay-zeka" : "uretken-yapay-zeka"}
          />
        </label>
        <button disabled={pending}>
          {category ? "Kategori" : "Etiket"} ekle
        </button>
      </form>
      <section className="admin-panel category-list">
        <header>
          <div>
            <p className="section-kicker">İÇERİK MODÜLÜ</p>
            <h2>Türkçe {category ? "kategoriler" : "etiketler"}</h2>
          </div>
          <strong>{turkish.length}</strong>
        </header>
        {turkish.length === 0 ? (
          <p className="muted">Henüz kayıt yok.</p>
        ) : (
          <div>
            {turkish.map((item) => (
              <form
                className="category-row"
                key={item.id}
                onSubmit={(event) => update(event, item.id)}
              >
                <label>
                  Ad
                  <input
                    name="name"
                    defaultValue={item.name}
                    required
                    maxLength={160}
                  />
                </label>
                <label>
                  Kısa ad
                  <input
                    name="slug"
                    defaultValue={item.slug}
                    required
                    pattern="[a-z0-9]+(?:-[a-z0-9]+)*"
                    maxLength={160}
                  />
                </label>
                <button disabled={pending}>Kaydet</button>
                {category&&<output className="category-volume" aria-label="Yayımlanmış içerik"><strong>{item.publishedCount??0}</strong><span>yayın</span></output>}
                <button
                  className="button-secondary"
                  type="button"
                  disabled={pending}
                  onClick={() => {
                    if (window.confirm(`“${item.name}” silinsin mi?`))
                      void request(`/${item.id}`, "DELETE");
                  }}
                >
                  Sil
                </button>
              </form>
            ))}
          </div>
        )}
      </section>
      {message && (
        <p className="admin-message" role="status">
          {message}
        </p>
      )}
    </div>
  );
}
