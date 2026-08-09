"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import Swal from "sweetalert2";
import type { ManagedLocale } from "@/lib/admin-api";

async function mutate(path: string, method: "POST" | "PUT", body: object) {
  const csrf = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
  const { token } = (await csrf.json()) as { token: string };
  const response = await fetch(`/api/admin/locales${path}`, {
    method,
    headers: { "content-type": "application/json", "x-csrf-token": token },
    body: JSON.stringify(body),
  });
  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as {
      message?: string;
    } | null;
    throw new Error(problem?.message ?? "İşlem tamamlanamadı.");
  }
}

export function LanguageManager({ locales }: { locales: ManagedLocale[] }) {
  const router = useRouter();
  const [pending, setPending] = useState(false);
  async function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPending(true);
    const data = new FormData(event.currentTarget);
    try {
      await mutate("/", "POST", {
        code: data.get("code"),
        displayName: data.get("displayName"),
        nativeName: data.get("nativeName"),
      });
      await Swal.fire({
        title: "Dil eklendi",
        text: "Ana dilin konuşulduğu ülkeler otomatik bağlandı. Yeni dil, çeviriler hazırlanana kadar pasif tutuldu.",
        icon: "success",
        background: "#151922",
        color: "#f4f6fa",
      });
      event.currentTarget.reset();
      router.refresh();
    } catch (error) {
      await Swal.fire({
        title: "Dil eklenemedi",
        text: error instanceof Error ? error.message : "İşlem tamamlanamadı.",
        icon: "error",
        background: "#151922",
        color: "#f4f6fa",
      });
    } finally {
      setPending(false);
    }
  }
  async function save(
    locale: ManagedLocale,
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    setPending(true);
    const data = new FormData(event.currentTarget);
    try {
      await mutate(`/${locale.id}`, "PUT", {
        displayName: data.get("displayName"),
        nativeName: data.get("nativeName"),
        isEnabled: data.get("isEnabled") === "on",
      });
      router.refresh();
    } catch (error) {
      await Swal.fire({
        title: "Kaydedilemedi",
        text: error instanceof Error ? error.message : "İşlem tamamlanamadı.",
        icon: "error",
        background: "#151922",
        color: "#f4f6fa",
      });
    } finally {
      setPending(false);
    }
  }
  async function country(localeId: string, code: string, isEnabled: boolean) {
    setPending(true);
    try {
      await mutate(`/${localeId}/countries/${code}`, "PUT", { isEnabled });
      router.refresh();
    } finally {
      setPending(false);
    }
  }
  return (
    <div className="language-workspace">
      <form
        className="admin-panel admin-form language-create"
        onSubmit={create}
      >
        <header>
          <div>
            <p className="section-kicker">YENİ DİL</p>
            <h2>Dil ekle</h2>
          </div>
        </header>
        <p className="muted">
          Dil ve ülke kodunu birlikte girin. Örnek: Fransızca için{" "}
          <strong>fr-FR</strong>. İlgili ülkeler otomatik seçilir.
        </p>
        <label>
          Dil-bölge kodu
          <input name="code" placeholder="fr-FR" required maxLength={10} />
        </label>
        <label>
          Yönetim adı
          <input
            name="displayName"
            placeholder="French (France)"
            maxLength={100}
          />
        </label>
        <label>
          Yerel adı
          <input
            name="nativeName"
            placeholder="Français (France)"
            maxLength={100}
          />
        </label>
        <button disabled={pending}>Dili ekle</button>
      </form>
      <section className="language-list">
        {locales.map((locale) => (
          <article className="admin-panel language-card" key={locale.id}>
            <form
              className="admin-form"
              onSubmit={(event) => void save(locale, event)}
            >
              <header>
                <div>
                  <p className="section-kicker">{locale.code}</p>
                  <h2>{locale.nativeName}</h2>
                  <small>
                    {locale.articleCount} içerik · {locale.countries.length}{" "}
                    ülke
                  </small>
                </div>
                <span
                  className={
                    locale.isEnabled
                      ? "language-state enabled"
                      : "language-state"
                  }
                >
                  {locale.isEnabled ? "Aktif" : "Pasif"}
                </span>
              </header>
              <div className="form-grid">
                <label>
                  Yönetim adı
                  <input
                    name="displayName"
                    defaultValue={locale.displayName}
                    required
                  />
                </label>
                <label>
                  Yerel adı
                  <input
                    name="nativeName"
                    defaultValue={locale.nativeName}
                    required
                  />
                </label>
              </div>
              <label className="check-label">
                <input
                  name="isEnabled"
                  type="checkbox"
                  defaultChecked={locale.isEnabled}
                  disabled={locale.isDefault}
                />
                {locale.isDefault
                  ? "Varsayılan dil daima aktif"
                  : "Site dilini etkinleştir"}
              </label>
              <button disabled={pending}>Dil ayarlarını kaydet</button>
            </form>
            <section className="language-countries">
              <h3>Otomatik seçilen ülkeler</h3>
              {locale.countries.map((item) => (
                <label key={item.code}>
                  <span>
                    <strong>{item.name}</strong>
                    <small>
                      {item.code} · {item.currencyCode}
                      {item.isPrimary ? " · Ana ülke" : ""}
                    </small>
                  </span>
                  <input
                    type="checkbox"
                    checked={item.isEnabled}
                    disabled={pending}
                    onChange={(event) =>
                      void country(locale.id, item.code, event.target.checked)
                    }
                  />
                </label>
              ))}
            </section>
          </article>
        ))}
      </section>
    </div>
  );
}
