import type {Locale} from "./config";
export const correctionCopy:Record<Locale,{heading:string;summary:string;last:string}>={
  "tr-TR":{heading:"Düzeltmeler",summary:"Bu yayında sonradan düzeltilen önemli noktalar.",last:"Son düzeltme"},
  "en-US":{heading:"Corrections",summary:"Material points corrected after this article was published.",last:"Last correction"},
  "de-DE":{heading:"Korrekturen",summary:"Wesentliche Punkte, die nach der Veröffentlichung korrigiert wurden.",last:"Letzte Korrektur"},
  "fr-FR":{heading:"Corrections",summary:"Points importants corrigés après la publication de cet article.",last:"Dernière correction"},
};
