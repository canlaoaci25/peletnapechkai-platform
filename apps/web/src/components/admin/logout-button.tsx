"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

export function LogoutButton({ locale, label }: { locale: string; label: string }) {
  const router = useRouter();
  const [pending, setPending] = useState(false);

  async function logout() {
    setPending(true);
    const csrfResponse = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
    if (csrfResponse.ok) {
      const { token } = (await csrfResponse.json()) as { token: string };
      await fetch("/api/admin/auth/logout", { method: "POST", headers: { "x-csrf-token": token } });
    }
    router.replace(`/${locale}/admin/login`);
    router.refresh();
  }

  return <button className="secondary-button" type="button" onClick={logout} disabled={pending}>{label}</button>;
}
