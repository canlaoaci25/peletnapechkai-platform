import type { Locale } from "./config";
export const archiveCopy:Record<Locale,{categories:string;tags:string;authors:string;empty:string;description:string}>={
  "tr-TR":{categories:"Kategori",tags:"Etiket",authors:"Yazar",empty:"Bu arşivde henüz yayımlanmış içerik yok.",description:"BOECL yayımlanmış içerik arşivi"},
  "en-US":{categories:"Category",tags:"Tag",authors:"Author",empty:"There is no published content in this archive yet.",description:"BOECL published content archive"},
  "de-DE":{categories:"Kategorie",tags:"Schlagwort",authors:"Autor",empty:"In diesem Archiv gibt es noch keine veröffentlichten Inhalte.",description:"BOECL-Archiv veröffentlichter Inhalte"},
  "fr-FR":{categories:"Catégorie",tags:"Étiquette",authors:"Auteur",empty:"Aucun contenu n’a encore été publié dans cette archive.",description:"Archives des contenus publiés par BOECL"},
};
