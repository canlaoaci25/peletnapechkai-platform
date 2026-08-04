import type { Locale } from "./config";

export const adminCopy = {
  "tr-TR": {
    loginTitle: "Yönetim paneli", loginLead: "Editoryal çalışma alanına güvenli giriş yapın.", email: "E-posta", password: "Parola", code: "Authenticator kodu", signIn: "Giriş yap", verify: "Kodu doğrula", verifying: "Doğrulanıyor…", loginError: "Giriş bilgileri doğrulanamadı. Lütfen yeniden deneyin.",
    dashboard: "Yayın masası", signedInAs: "Oturum", logout: "Çıkış yap", newDraft: "Yeni taslak", editDraft: "Taslağı düzenle", back: "Yayın masasına dön", recent: "Son içerikler", empty: "Henüz içerik yok.", locale: "Dil", type: "İçerik türü", title: "Başlık", slug: "URL kısa adı", summary: "Özet", body: "İçerik", seoTitle: "SEO başlığı", seoDescription: "SEO açıklaması", save: "Taslak oluştur", update: "Değişiklikleri kaydet", saving: "Kaydediliyor…", saved: "Taslak kaydedildi.", saveError: "Taslak kaydedilemedi. Alanları ve bağlantıyı kontrol edin.", news: "Haber", guide: "Rehber", review: "İnceleme", analysis: "Analiz", status: "Durum", updated: "Güncelleme",
  },
  "en-US": {
    loginTitle: "Administration", loginLead: "Sign in securely to the editorial workspace.", email: "Email", password: "Password", code: "Authenticator code", signIn: "Sign in", verify: "Verify code", verifying: "Verifying…", loginError: "The sign-in details could not be verified. Please try again.",
    dashboard: "Editorial desk", signedInAs: "Session", logout: "Sign out", newDraft: "New draft", editDraft: "Edit draft", back: "Back to editorial desk", recent: "Recent content", empty: "No content yet.", locale: "Locale", type: "Content type", title: "Title", slug: "URL slug", summary: "Summary", body: "Content", seoTitle: "SEO title", seoDescription: "SEO description", save: "Create draft", update: "Save changes", saving: "Saving…", saved: "Draft saved.", saveError: "The draft could not be saved. Check the fields and connection.", news: "News", guide: "Guide", review: "Review", analysis: "Analysis", status: "Status", updated: "Updated",
  },
  "de-DE": {
    loginTitle: "Administration", loginLead: "Sicher im redaktionellen Arbeitsbereich anmelden.", email: "E-Mail", password: "Passwort", code: "Authenticator-Code", signIn: "Anmelden", verify: "Code prüfen", verifying: "Wird geprüft…", loginError: "Die Anmeldedaten konnten nicht bestätigt werden. Bitte erneut versuchen.",
    dashboard: "Redaktion", signedInAs: "Sitzung", logout: "Abmelden", newDraft: "Neuer Entwurf", editDraft: "Entwurf bearbeiten", back: "Zurück zur Redaktion", recent: "Letzte Inhalte", empty: "Noch keine Inhalte vorhanden.", locale: "Sprache", type: "Inhaltstyp", title: "Titel", slug: "URL-Kurzname", summary: "Zusammenfassung", body: "Inhalt", seoTitle: "SEO-Titel", seoDescription: "SEO-Beschreibung", save: "Entwurf erstellen", update: "Änderungen speichern", saving: "Wird gespeichert…", saved: "Entwurf gespeichert.", saveError: "Der Entwurf konnte nicht gespeichert werden. Felder und Verbindung prüfen.", news: "Nachricht", guide: "Ratgeber", review: "Test", analysis: "Analyse", status: "Status", updated: "Aktualisiert",
  },
} satisfies Record<Locale, Record<string, string>>;

export type AdminCopy = (typeof adminCopy)["tr-TR"];
