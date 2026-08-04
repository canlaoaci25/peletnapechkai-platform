"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import type { AdminCopy } from "@/i18n/admin-copy";

type LoginFormProps = { locale: string; copy: AdminCopy };

export function LoginForm({ locale, copy }: LoginFormProps) {
  const router = useRouter();
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);
  const [needsTwoFactor, setNeedsTwoFactor] = useState(false);

  async function csrfToken() {
    const response = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
    if (!response.ok) throw new Error();
    return ((await response.json()) as { token: string }).token;
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPending(true);
    setError("");
    const data = new FormData(event.currentTarget);
    try {
      const token = await csrfToken();
      const endpoint = needsTwoFactor ? "/api/admin/auth/login/2fa" : "/api/admin/auth/login";
      const body = needsTwoFactor
        ? { authenticatorCode: data.get("code"), recoveryCode: null }
        : { email: data.get("email"), password: data.get("password") };
      const response = await fetch(endpoint, {
        method: "POST",
        headers: { "content-type": "application/json", "x-csrf-token": token },
        body: JSON.stringify(body),
      });
      if (response.status === 401 && !needsTwoFactor) {
        const result = (await response.json().catch(() => null)) as { twoFactorRequired?: boolean } | null;
        if (result?.twoFactorRequired) {
          setNeedsTwoFactor(true);
          return;
        }
      }
      if (!response.ok) throw new Error();
      router.replace(`/${locale}/admin`);
      router.refresh();
    } catch {
      setError(copy.loginError);
    } finally {
      setPending(false);
    }
  }

  return (
    <form className="admin-form login-form" onSubmit={submit}>
      {!needsTwoFactor ? (
        <>
          <label>{copy.email}<input name="email" type="email" autoComplete="username" required /></label>
          <label>{copy.password}<input name="password" type="password" autoComplete="current-password" required /></label>
        </>
      ) : (
        <label>{copy.code}<input name="code" inputMode="numeric" autoComplete="one-time-code" required /></label>
      )}
      {error && <p className="form-error" role="alert">{error}</p>}
      <button type="submit" disabled={pending}>{pending ? copy.verifying : needsTwoFactor ? copy.verify : copy.signIn}</button>
    </form>
  );
}
