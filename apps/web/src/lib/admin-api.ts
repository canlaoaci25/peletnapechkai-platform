import "server-only";

import { cookies } from "next/headers";

export type AdminSession = {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
};

export type ArticleSummary = {
  id: string;
  articleGroupId: string;
  locale: string;
  type: string;
  slug: string;
  title: string;
  status: string;
  updatedAt: string;
  scheduledAt: string | null;
  publishedAt: string | null;
};

export type ArticleDetail = ArticleSummary & {
  summary: string;
  body: string;
  seoTitle: string | null;
  seoDescription: string | null;
  categoryIds: string[];
  tagIds: string[];
  authorIds: string[];
  sourceIds: string[];
  mediaAssetIds: string[];
  coverMediaAssetId: string | null;
  coverAltText: string | null;
  coverCaption: string | null;
  coverCredit: string | null;
  isSponsored: boolean;
  sponsorName: string | null;
  hasAffiliateLinks: boolean;
};

export type ManagedUser = {
  id: string;
  email: string;
  displayName: string;
  isActive: boolean;
  emailConfirmed: boolean;
  twoFactorEnabled: boolean;
  lockoutEnd: string | null;
  roles: string[];
};

export type SupportingLibrary = {
  categories: {
    id: string;
    locale: string;
    slug: string;
    name: string;
    parentCategoryId: string | null;
    parentName: string | null;
    childCount: number;
    articleCount: number;
    publishedCount: number;
  }[];
  tags: { id: string; locale: string; slug: string; name: string }[];
  authors: { id: string; slug: string; displayName: string }[];
  sources: {
    id: string;
    name: string;
    url: string;
    kind: string;
    lastReviewedAt: string | null;
  }[];
  taxonomyHealth: {
    publishedCount: number;
    uncategorizedCount: number;
    uncategorized: {
      id: string;
      slug: string;
      title: string;
      publishedAt: string;
    }[];
  };
};
export type MediaItem = {
  id: string;
  fileName: string;
  contentType: string;
  byteLength: number;
  width: number | null;
  height: number | null;
  optimizedByteLength: number | null;
  usageCount: number;
  canDelete: boolean;
  createdAt: string;
};
export type SystemStatus = {
  checkedAt: string;
  database: string;
  articles: number;
  published: number;
  lifecycle: Record<string, number>;
  types: Record<string, number>;
  users: number;
  mediaFiles: number;
  mediaBytes: number;
  diskFreeBytes: number;
  productionHealth: {
    checkedAt: string | null;
    available: boolean;
    healthy: boolean;
    stale: boolean;
    servicesHealthy: number;
    servicesTotal: number;
    endpointsHealthy: number;
    endpointsTotal: number;
    freeDiskGb: number | null;
    certificateDaysRemaining: number | null;
    failures: string[];
  };
  deployments: DeploymentSnapshot[];
  deploymentConsistency: { environment: "Staging" | "Production"; state: "Aligned" | "Drifted" | "Incomplete" | "AtRisk"; commit: string | null; message: string }[];
  deploymentHistory: DeploymentSnapshot[];
  deploymentReliability: {
    sampleSize: number;
    successful: number;
    recovered: number;
    failed: number;
    successRate: number;
    medianDurationSeconds: number;
    p95DurationSeconds: number;
    healthyStreak: number;
    stalled: number;
    state: "NoData" | "AtRisk" | "Watch" | "Healthy";
  };
};
export type DeploymentSnapshot = {
  deploymentId: string;
  environment: "Staging" | "Production";
  component: "Web" | "Api";
  status: string;
  commit: string;
  message: string;
  startedAt: string;
  updatedAt: string;
  durationSeconds: number;
};
export type EditorialCommandCenter = {
  checkedAt: string;
  summary: {
    overdue: number;
    dueSoon: number;
    inReview: number;
    incompleteQuality: number;
    freshnessDebt: number;
    personalOpen: number;
    personalOverdue: number;
    personalDueSoon: number;
    unassigned: number;
    teamMembers: number;
    scheduled: number;
    scheduleConflicts: number;
    readyToSchedule: number;
  };
  workloads: {
    userId: string;
    displayName: string;
    open: number;
    overdue: number;
    dueSoon: number;
  }[];
  users: { id: string; displayName: string }[];
  schedule: {
    articleId: string;
    title: string;
    locale: string;
    scheduledAt: string;
    categories: string[];
    hasConflict: boolean;
    conflictReasons: ("LocaleCollision" | "CategoryCollision")[];
  }[];
  items: {
    articleId: string;
    title: string;
    locale: string;
    kind: string;
    dueAt: string;
    taskTitle: string | null;
    assignee: string | null;
    assigneeUserId: string | null;
    priority: string | null;
    taskId: string | null;
    status: string | null;
    isMine: boolean;
    missingGates: string[] | null;
    freshnessReasons: string[] | null;
  }[];
};
export type KnowledgeLink = {
  id: string;
  articleLocalizationId: string;
  articleTitle: string;
  articleSlug: string;
  purpose: string;
  note: string | null;
  reviewDueAt: string;
  lastVerifiedAt: string;
};
export type KnowledgeCandidate = {
  id: string;
  locale: string;
  title: string;
  claim: string;
  sourceUrl: string;
  aiAssisted: boolean;
  status: string;
  createdAt: string;
  updatedAt: string;
  links: KnowledgeLink[];
};
export type ArticleRevision = {
  id: string;
  number: number;
  title: string;
  summary: string;
  body: string;
  createdByUserId: string | null;
  createdAt: string;
};
export type ArticleCollaboration = {
  tasks: {
    id: string;
    assigneeUserId: string;
    assignee: string | null;
    title: string;
    priority: string;
    status: string;
    dueAt: string;
  }[];
  comments: {
    id: string;
    author: string | null;
    body: string;
    parentCommentId: string | null;
    articleRevisionId: string | null;
    isResolved: boolean;
    deletedAt: string | null;
    createdAt: string;
  }[];
  checklist: null | {
    titleAndSummary: boolean;
    sourcesVerified: boolean;
    authorAndTaxonomy: boolean;
    seoMetadata: boolean;
    coverAccessibility: boolean;
    commercialDisclosure: boolean;
    translationReviewed: boolean;
    legalEditorialReview: boolean;
    isComplete: boolean;
  };
  users: { id: string; displayName: string }[];
};
export type ManagedLocale = {
  id: string;
  code: string;
  languageCode: string;
  displayName: string;
  nativeName: string;
  isDefault: boolean;
  isEnabled: boolean;
  articleCount: number;
  publishedCount: number;
  draftCount: number;
  sourcePublishedCount: number;
  sourceCategoryCount: number;
  sourceTagCount: number;
  missingTranslationCount: number;
  reviewPendingCount: number;
  staleTranslationCount: number;
  linkedCategoryCount: number;
  missingCategoryCount: number;
  linkedTagCount: number;
  missingTagCount: number;
  countries: {
    code: string;
    name: string;
    currencyCode: string;
    isRequired: boolean;
    isEnabled: boolean;
    isPrimary: boolean;
  }[];
};
export type LocalizationWork = {
  checkedAt: string;
  users: { id: string; displayName: string }[];
  items: {
    articleGroupId: string;
    targetLocaleId: string;
    targetLocale: string;
    sourceTitle: string;
    translationTitle: string | null;
    kind: "Missing" | "Untracked" | "Stale" | "Review";
    sourceSnapshotAt: string | null;
    changedFields: ("Title" | "Summary" | "Body" | "Seo" | "Untracked")[];
    sla: "Unassigned" | "Overdue" | "DueSoon" | "OnTrack";
    assignment: null | {
      assigneeUserId: string;
      assignee: string | null;
      dueAt: string;
      status: string;
    };
  }[];
};
export type LocalizationWorkDetail = {
  articleGroupId: string;
  targetLocaleId: string;
  targetLocale: string;
  source: {
    id: string;
    locale: string;
    title: string;
    summary: string;
    body: string;
    seoTitle: string | null;
    seoDescription: string | null;
    updatedAt: string;
  };
  translation: null | {
    id: string;
    locale: string;
    title: string;
    summary: string;
    body: string;
    seoTitle: string | null;
    seoDescription: string | null;
    status: string;
    updatedAt: string;
    sourceSnapshotUpdatedAt: string | null;
  };
  changedFields: ("Title" | "Summary" | "Body" | "Seo" | "Untracked" | "Missing")[];
};
export type LocaleCatalogItem = {
  code: string;
  displayName: string;
  nativeName: string;
  countryCode: string;
  countryName: string;
};
export type HomepageAdminData = {
  mode: "Automatic" | "Hybrid";
  placements: {
    section: string;
    position: number;
    articleLocalizationId: string;
  }[];
  articles: {
    id: string;
    title: string;
    type: string;
    publishedAt: string;
    views: number;
    score: number;
  }[];
};

const apiBaseUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5267";

async function apiGet<T>(path: string): Promise<T | null> {
  const cookieStore = await cookies();
  const response = await fetch(new URL(path, apiBaseUrl), {
    headers: { cookie: cookieStore.toString() },
    cache: "no-store",
  });

  if (response.status === 401 || response.status === 403) return null;
  if (!response.ok) throw new Error(`Admin API request failed (${response.status}).`);
  return (await response.json()) as T;
}

export function getAdminSession() {
  return apiGet<AdminSession>("/api/v1/auth/session");
}

export async function getArticles() {
  return (await apiGet<ArticleSummary[]>("/api/v1/admin/articles/")) ?? [];
}

export function getArticle(id: string) {
  return apiGet<ArticleDetail>(`/api/v1/admin/articles/${encodeURIComponent(id)}`);
}
export async function getArticleRevisions(id: string) {
  return (await apiGet<ArticleRevision[]>(`/api/v1/admin/articles/${encodeURIComponent(id)}/revisions`)) ?? [];
}
export function getArticleCollaboration(id: string) {
  return apiGet<ArticleCollaboration>(`/api/v1/admin/articles/${encodeURIComponent(id)}/collaboration/`);
}

export async function getUsers() {
  return (await apiGet<ManagedUser[]>("/api/v1/admin/users/")) ?? [];
}

export async function getSupportingLibrary() {
  return (
    (await apiGet<SupportingLibrary>("/api/v1/admin/supporting/")) ?? {
      categories: [],
      tags: [],
      authors: [],
      sources: [],
      taxonomyHealth: {
        publishedCount: 0,
        uncategorizedCount: 0,
        uncategorized: [],
      },
    }
  );
}

export async function getMedia() {
  return (await apiGet<MediaItem[]>("/api/v1/admin/media/")) ?? [];
}
export function getSystemStatus() {
  return apiGet<SystemStatus>("/api/v1/admin/status");
}
export function getEditorialCommandCenter() {
  return apiGet<EditorialCommandCenter>("/api/v1/admin/editorial/command-center");
}
export async function getKnowledgeCandidates() {
  return (await apiGet<KnowledgeCandidate[]>("/api/v1/admin/knowledge/")) ?? [];
}
export async function getManagedLocales() {
  return (await apiGet<ManagedLocale[]>("/api/v1/admin/locales/")) ?? [];
}
export async function getLocaleCatalog() {
  return (await apiGet<LocaleCatalogItem[]>("/api/v1/admin/locales/catalog")) ?? [];
}
export async function getLocalizationWork() {
  return (
    (await apiGet<LocalizationWork>("/api/v1/admin/locales/work")) ?? {
      checkedAt: new Date().toISOString(),
      users: [],
      items: [],
    }
  );
}
export function getLocalizationWorkDetail(articleGroupId: string, targetLocaleId: string) {
  return apiGet<LocalizationWorkDetail>(`/api/v1/admin/locales/work/${encodeURIComponent(articleGroupId)}/${encodeURIComponent(targetLocaleId)}`);
}
export function getHomepageAdmin(locale: string) {
  return apiGet<HomepageAdminData>(`/api/v1/admin/homepage/${encodeURIComponent(locale)}`);
}
type AuthorityRisk = "missing_sources" | "single_source" | "single_domain" | "insecure_source" | "missing_seo" | "missing_cover" | "missing_category" | "missing_tags";
export type TrafficDashboard = {
  locale: string;
  checkedAt: string;
  published: number;
  totalViews: number;
  totalEngagedSeconds: number;
  averageEngagedSeconds: number;
  authority: {
    strong: number;
    needsWork: number;
    critical: number;
    averageScore: number;
    withoutSources: number;
    singleSource: number;
  };
  sourceDomains: { domain: string; articles: number; citations: number }[];
  measurement: {
    internalAnalytics: boolean;
    ga4: boolean;
    clarity: boolean;
    searchConsole: boolean;
  };
  searchConsole: null | {
    startDate: string;
    endDate: string;
    clicks: number;
    impressions: number;
    ctr: number;
    averagePosition: number;
    queries: {
      query: string;
      clicks: number;
      impressions: number;
      ctr: number;
      position: number;
    }[];
  };
  top: { slug: string; title: string; views: number; engagedSeconds: number }[];
  opportunities: {
    id: string;
    slug: string;
    title: string;
    views: number;
    engagedSeconds: number;
    hasSeo: boolean;
    hasCover: boolean;
    tagCount: number;
    sourceCount: number;
    domainCount: number;
    authorityScore: number;
    risks: AuthorityRisk[];
  }[];
  clusters: {
    name: string;
    articles: number;
    views: number;
    engagedSeconds: number;
  }[];
};
export function getTrafficDashboard(locale: string) {
  return apiGet<TrafficDashboard>(`/api/v1/admin/traffic/${encodeURIComponent(locale)}`);
}
export type DevelopmentStatus = {
  task: string;
  phase: string;
  status: string;
  steps: string[];
  currentStep: number;
  lastAction: string;
  commit: string;
  startedAt: string | null;
  updatedAt: string | null;
  machine: string;
};
export async function getDevelopmentStatus() {
  return (
    (await apiGet<DevelopmentStatus>("/api/v1/admin/development/status")) ?? {
      task: "Bekleyen Codex görevi yok",
      phase: "Hazır",
      status: "Paused",
      steps: [],
      currentStep: 0,
      lastAction: "—",
      commit: "",
      startedAt: null,
      updatedAt: null,
      machine: "—",
    }
  );
}
export type SavedArticle = {
  slug: string;
  title: string;
  summary: string;
  type: string;
  locale: string;
  publishedAt: string;
  savedAt: string;
  cover: null | { url: string; altText: string };
};
export type MemberAccount = {
  id: string;
  email: string;
  displayName: string;
  emailConfirmed: boolean;
  roles: string[];
  verificationAvailable: boolean;
  createdAt: string;
};
export function getMemberAccount() {
  return apiGet<MemberAccount>("/api/v1/account/");
}
export async function getSavedArticles(locale: string) {
  return (await apiGet<SavedArticle[]>(`/api/v1/account/saved?locale=${encodeURIComponent(locale)}`)) ?? [];
}
export type FollowedCategory = {
  slug: string;
  title: string;
  description: string | null;
  locale: string;
  followedAt: string;
  articleCount: number;
};
export type PersonalFeedArticle = SavedArticle & { categories: string[] };
export async function getFollowedCategories(locale: string) {
  return (await apiGet<FollowedCategory[]>(`/api/v1/account/following?locale=${encodeURIComponent(locale)}`)) ?? [];
}
export async function getPersonalFeed(locale: string) {
  return (await apiGet<PersonalFeedArticle[]>(`/api/v1/account/feed?locale=${encodeURIComponent(locale)}`)) ?? [];
}
export type ReadingProgressArticle = {
  slug: string;
  title: string;
  summary: string;
  locale: string;
  percent: number;
  anchor: string | null;
  lastReadAt: string;
  cover: null | { url: string; altText: string };
};
export async function getReadingProgress(locale: string) {
  return (await apiGet<ReadingProgressArticle[]>(`/api/v1/account/reading-progress?locale=${encodeURIComponent(locale)}`)) ?? [];
}
export type ReadingRitual = {
  goal: number;
  completed: number;
  activeDays: number;
  weekStartsAt: string;
  next: null | {
    slug: string;
    title: string;
    summary: string;
    cover: null | { url: string; altText: string };
  };
};
export async function getReadingRitual(locale: string) {
  return await apiGet<ReadingRitual>(`/api/v1/account/reading-ritual?locale=${encodeURIComponent(locale)}`);
}
