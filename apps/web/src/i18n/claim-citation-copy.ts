import type { Locale } from "@/i18n/config";
export const claimCitationCopy:Record<Locale,{heading:string;summary:string;source:string}>={
  "tr-TR":{heading:"İddialar ve doğrudan kanıtlar",summary:"Editörün önemli iddialarla eşleştirip onayladığı kaynak bağlantıları.",source:"Kanıtı aç"},
  "en-US":{heading:"Claims and direct evidence",summary:"Source links matched to important claims and approved by an editor.",source:"Open evidence"},
  "de-DE":{heading:"Aussagen und direkte Belege",summary:"Von der Redaktion geprüfte Quellenlinks zu wichtigen Aussagen.",source:"Beleg öffnen"},
  "fr-FR":{heading:"Affirmations et preuves directes",summary:"Liens de sources associés aux affirmations importantes et validés par la rédaction.",source:"Ouvrir la preuve"},
};
