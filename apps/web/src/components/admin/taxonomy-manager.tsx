"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";

type Item = { id: string; locale: string; slug: string; name: string };
type Kind = "categories" | "tags";
export function TaxonomyManager({
  items,
  kind,
}: {
  items: Item[];
  kind: Kind;
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
