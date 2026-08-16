import type { Locale } from "./config";

export const topicCopy: Record<Locale, { eyebrow: string; title: string; description: string; stories: string; latest: string }> = {
  "tr-TR": { eyebrow: "BOECL konu haritası", title: "Merak ettiğiniz yerden başlayın", description: "Teknoloji gündemini geniş başlıklarda kaybolmadan izleyin. Her konu alanı, BOECL arşivindeki güncel rehberleri, incelemeleri ve analizleri bir araya getirir.", stories: "yayın", latest: "Konuyu keşfet" },
  "en-US": { eyebrow: "BOECL topic map", title: "Start with what interests you", description: "Follow technology without getting lost in broad headlines. Each topic brings together current guides, reviews, and analysis from the BOECL archive.", stories: "stories", latest: "Explore topic" },
  "de-DE": { eyebrow: "BOECL-Themenkarte", title: "Beginnen Sie mit Ihrem Interesse", description: "Verfolgen Sie Technologie, ohne sich in großen Überschriften zu verlieren. Jedes Thema bündelt aktuelle Ratgeber, Tests und Analysen aus dem BOECL-Archiv.", stories: "Beiträge", latest: "Thema entdecken" },
  "fr-FR": { eyebrow: "Carte des sujets BOECL", title: "Commencez par ce qui vous intéresse", description: "Suivez la technologie sans vous perdre dans des rubriques trop larges. Chaque sujet réunit les guides, essais et analyses récents des archives BOECL.", stories: "publications", latest: "Explorer le sujet" },
};
