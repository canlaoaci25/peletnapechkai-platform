import type { Locale } from "@/i18n/config";

export const memberHubCopy: Record<
  Locale,
  {
    title: string;
    description: string;
    continueLabel: string;
    newLabel: string;
    savedLabel: string;
    topicsLabel: string;
    searchLabel: string;
    searchPlaceholder: string;
    filterLabel: string;
    allLabel: string;
    resultsLabel: string;
    noResults: string;
  }
> = {
  "tr-TR": {
    title: "Okuma merkezin",
    description:
      "Yarım bıraktıkların, ilgi alanlarından yeni yayınlar ve kaydettiklerin tek yerde.",
    continueLabel: "Devam et",
    newLabel: "Senin için",
    savedLabel: "Kaydedilen",
    topicsLabel: "Takip edilen",
    searchLabel: "Okuma listende ara",
    searchPlaceholder: "Başlık, özet veya tür ara",
    filterLabel: "İçerik türü",
    allLabel: "Tümü",
    resultsLabel: "sonuç",
    noResults: "Bu arama ve türle eşleşen kayıt yok.",
  },
  "en-US": {
    title: "Your reading hub",
    description:
      "Pick up unfinished stories, find fresh reads from your interests, and manage saved stories in one place.",
    continueLabel: "Continue",
    newLabel: "For you",
    savedLabel: "Saved",
    topicsLabel: "Following",
    searchLabel: "Search your reading list",
    searchPlaceholder: "Search title, summary, or format",
    filterLabel: "Story format",
    allLabel: "All",
    resultsLabel: "results",
    noResults: "No saved stories match this search and format.",
  },
  "de-DE": {
    title: "Deine Lesezentrale",
    description: "Angefangene, neue und gespeicherte Beiträge an einem Ort.",
    continueLabel: "Weiterlesen",
    newLabel: "Für dich",
    savedLabel: "Gespeichert",
    topicsLabel: "Gefolgt",
    searchLabel: "Leseliste durchsuchen",
    searchPlaceholder: "Titel, Zusammenfassung oder Format",
    filterLabel: "Beitragsformat",
    allLabel: "Alle",
    resultsLabel: "Ergebnisse",
    noResults: "Keine gespeicherten Beiträge entsprechen der Suche.",
  },
  "fr-FR": {
    title: "Votre espace de lecture",
    description:
      "Retrouvez vos lectures en cours, nouveautés personnalisées et articles enregistrés.",
    continueLabel: "Continuer",
    newLabel: "Pour vous",
    savedLabel: "Enregistrés",
    topicsLabel: "Suivis",
    searchLabel: "Rechercher dans votre liste",
    searchPlaceholder: "Titre, résumé ou format",
    filterLabel: "Format",
    allLabel: "Tous",
    resultsLabel: "résultats",
    noResults: "Aucun article enregistré ne correspond à cette recherche.",
  },
};

const shared = {
  saveProfile: "Save profile",
  currentPassword: "Current password",
  newPassword: "New password",
  saved: "Saved",
  saveBusy: "Saving",
  failed: "The action could not be completed. Please try again.",
};
type MemberCopy = {
  continueTitle: string;
  continueDescription: string;
  continueAction: string;
  progress: string;
  membership: string;
  account: string;
  profile: string;
  displayName: string;
  saveProfile: string;
  changePassword: string;
  currentPassword: string;
  newPassword: string;
  updatePassword: string;
  savedTitle: string;
  savedDescription: string;
  savedEmpty: string;
  savedEmptyAction: string;
  save: string;
  saved: string;
  saveBusy: string;
  signInToSave: string;
  remove: string;
  savedSuccess: string;
  removedSuccess: string;
  failed: string;
  profileSaved: string;
  verified: string;
  verificationPending: string;
  verificationUnavailable: string;
  follow: string;
  following: string;
  followBusy: string;
  signInToFollow: string;
  followSuccess: string;
  unfollowSuccess: string;
  topicsTitle: string;
  topicsDescription: string;
  topicsEmpty: string;
  feedTitle: string;
  feedDescription: string;
  feedEmpty: string;
};
export const memberCopy: Record<Locale, MemberCopy> = {
  "tr-TR": {
    ...shared,
    continueTitle: "Kaldığın yerden devam et",
    continueDescription:
      "Yarım bıraktığın yazılar, son okuduğun noktaya hazır.",
    continueAction: "Okumaya devam et",
    progress: "okundu",
    membership: "BOECL ÜYELİK",
    account: "Hesabım",
    profile: "Profil",
    displayName: "Görünen ad",
    saveProfile: "Profili kaydet",
    changePassword: "Parola değiştir",
    currentPassword: "Mevcut parola",
    newPassword: "Yeni parola",
    updatePassword: "Parolayı değiştir",
    savedTitle: "Okuma listen",
    savedDescription:
      "Sonra okumak istediklerini kaydet; giriş yaptığın her cihazdan dön.",
    savedEmpty: "Henüz kaydettiğin bir içerik yok.",
    savedEmptyAction: "Konuları keşfet",
    save: "Kaydet",
    saved: "Kaydedildi",
    saveBusy: "Kaydediliyor",
    signInToSave: "Kaydetmek için giriş yap",
    remove: "Listeden çıkar",
    savedSuccess: "Okuma listene eklendi.",
    removedSuccess: "Okuma listenden çıkarıldı.",
    failed: "İşlem tamamlanamadı. Lütfen yeniden dene.",
    profileSaved: "Değişiklik kaydedildi.",
    verified: "E-posta doğrulandı",
    verificationPending: "E-posta doğrulaması bekliyor",
    verificationUnavailable: "E-posta sağlayıcısı henüz yapılandırılmadı",
    follow: "Konuyu takip et",
    following: "Takip ediliyor",
    followBusy: "Güncelleniyor",
    signInToFollow: "Takip etmek için giriş yap",
    followSuccess: "Konu takiplerine eklendi.",
    unfollowSuccess: "Konu takiplerinden çıkarıldı.",
    topicsTitle: "Takip ettiğin konular",
    topicsDescription:
      "İlgi alanlarını yönet; BOECL senin için güncel bir yayın akışı oluştursun.",
    topicsEmpty: "Henüz bir konu takip etmiyorsun.",
    feedTitle: "Senin için",
    feedDescription: "Takip ettiğin konulardan en yeni yayınlar.",
    feedEmpty: "Kişisel akışın, ilk konunu takip ettiğinde burada oluşacak.",
  },
  "en-US": {
    ...shared,
    continueTitle: "Continue reading",
    continueDescription:
      "Stories you started are ready at your last reading point.",
    continueAction: "Continue",
    progress: "read",
    membership: "BOECL MEMBERSHIP",
    account: "My account",
    profile: "Profile",
    displayName: "Display name",
    changePassword: "Change password",
    updatePassword: "Update password",
    savedTitle: "Your reading list",
    savedDescription:
      "Save stories for later and return on every signed-in device.",
    savedEmpty: "You have not saved any stories yet.",
    savedEmptyAction: "Explore topics",
    save: "Save",
    signInToSave: "Sign in to save",
    remove: "Remove from list",
    savedSuccess: "Added to your reading list.",
    removedSuccess: "Removed from your reading list.",
    profileSaved: "Changes saved.",
    verified: "Email verified",
    verificationPending: "Email verification pending",
    verificationUnavailable: "Email provider is not configured yet",
    follow: "Follow topic",
    following: "Following",
    followBusy: "Updating",
    signInToFollow: "Sign in to follow",
    followSuccess: "Added to your followed topics.",
    unfollowSuccess: "Removed from your followed topics.",
    topicsTitle: "Topics you follow",
    topicsDescription:
      "Manage your interests and let BOECL build a fresh reading stream.",
    topicsEmpty: "You are not following a topic yet.",
    feedTitle: "For you",
    feedDescription: "The newest stories from topics you follow.",
    feedEmpty:
      "Your personal stream will appear after you follow your first topic.",
  },
  "de-DE": {
    ...shared,
    continueTitle: "Weiterlesen",
    continueDescription:
      "Begonnene Beiträge warten an deiner letzten Lesestelle.",
    continueAction: "Weiterlesen",
    progress: "gelesen",
    membership: "BOECL MITGLIEDSCHAFT",
    account: "Mein Konto",
    profile: "Profil",
    displayName: "Anzeigename",
    saveProfile: "Profil speichern",
    changePassword: "Passwort ändern",
    currentPassword: "Aktuelles Passwort",
    newPassword: "Neues Passwort",
    updatePassword: "Passwort aktualisieren",
    savedTitle: "Deine Leseliste",
    savedDescription:
      "Speichere Beiträge und öffne sie auf jedem angemeldeten Gerät.",
    savedEmpty: "Du hast noch keine Beiträge gespeichert.",
    savedEmptyAction: "Themen entdecken",
    save: "Speichern",
    saved: "Gespeichert",
    saveBusy: "Wird gespeichert",
    signInToSave: "Zum Speichern anmelden",
    remove: "Aus Liste entfernen",
    savedSuccess: "Zur Leseliste hinzugefügt.",
    removedSuccess: "Aus der Leseliste entfernt.",
    failed: "Die Aktion konnte nicht abgeschlossen werden.",
    profileSaved: "Änderungen gespeichert.",
    verified: "E-Mail bestätigt",
    verificationPending: "E-Mail-Bestätigung ausstehend",
    verificationUnavailable: "E-Mail-Anbieter ist nicht konfiguriert",
    follow: "Thema folgen",
    following: "Du folgst",
    followBusy: "Wird aktualisiert",
    signInToFollow: "Zum Folgen anmelden",
    followSuccess: "Zu deinen Themen hinzugefügt.",
    unfollowSuccess: "Aus deinen Themen entfernt.",
    topicsTitle: "Deine Themen",
    topicsDescription:
      "Verwalte deine Interessen für einen aktuellen Lesestrom.",
    topicsEmpty: "Du folgst noch keinem Thema.",
    feedTitle: "Für dich",
    feedDescription: "Die neuesten Beiträge aus deinen Themen.",
    feedEmpty: "Dein Lesestrom erscheint nach dem ersten gefolgten Thema.",
  },
  "fr-FR": {
    ...shared,
    continueTitle: "Reprendre la lecture",
    continueDescription:
      "Retrouvez les articles commencés à votre dernier point de lecture.",
    continueAction: "Continuer",
    progress: "lu",
    membership: "ADHÉSION BOECL",
    account: "Mon compte",
    profile: "Profil",
    displayName: "Nom affiché",
    saveProfile: "Enregistrer le profil",
    changePassword: "Modifier le mot de passe",
    currentPassword: "Mot de passe actuel",
    newPassword: "Nouveau mot de passe",
    updatePassword: "Mettre à jour",
    savedTitle: "Votre liste de lecture",
    savedDescription:
      "Enregistrez des articles et retrouvez-les sur chaque appareil connecté.",
    savedEmpty: "Vous n’avez encore enregistré aucun article.",
    savedEmptyAction: "Explorer les sujets",
    save: "Enregistrer",
    saved: "Enregistré",
    saveBusy: "Enregistrement",
    signInToSave: "Se connecter pour enregistrer",
    remove: "Retirer de la liste",
    savedSuccess: "Ajouté à votre liste de lecture.",
    removedSuccess: "Retiré de votre liste de lecture.",
    failed: "L’action n’a pas pu être effectuée.",
    profileSaved: "Modifications enregistrées.",
    verified: "E-mail vérifié",
    verificationPending: "Vérification de l’e-mail en attente",
    verificationUnavailable: "Le fournisseur d’e-mail n’est pas configuré",
    follow: "Suivre le sujet",
    following: "Sujet suivi",
    followBusy: "Mise à jour",
    signInToFollow: "Se connecter pour suivre",
    followSuccess: "Ajouté à vos sujets suivis.",
    unfollowSuccess: "Retiré de vos sujets suivis.",
    topicsTitle: "Vos sujets suivis",
    topicsDescription:
      "Gérez vos centres d’intérêt pour un fil toujours à jour.",
    topicsEmpty: "Vous ne suivez encore aucun sujet.",
    feedTitle: "Pour vous",
    feedDescription: "Les publications récentes de vos sujets.",
    feedEmpty: "Votre fil apparaîtra après votre premier sujet suivi.",
  },
};
