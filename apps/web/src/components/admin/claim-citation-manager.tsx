"use client";
import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import type { ArticleClaimCitation, SupportingLibrary } from "@/lib/admin-api";

const copy={
  "tr-TR":{title:"İddia ve kanıt bağlantıları",intro:"Okurun önemli bir iddiayı doğrudan dayandığı kaynağa kadar izleyebilmesini sağlayın.",claim:"Doğrulanan iddia",locator:"Kaynak içindeki bölüm (isteğe bağlı)",source:"Bağlı kaynak",add:"Kanıt bağlantısını onayla",remove:"Kaldır",empty:"Henüz iddia düzeyi kanıt bağlantısı yok.",error:"İşlem tamamlanamadı."},
  "en-US":{title:"Claims and evidence links",intro:"Let readers trace an important claim directly to its supporting source.",claim:"Verified claim",locator:"Location within source (optional)",source:"Attached source",add:"Approve evidence link",remove:"Remove",empty:"No claim-level evidence links yet.",error:"The action could not be completed."},
  "de-DE":{title:"Aussagen und Beleglinks",intro:"Leser können wichtige Aussagen direkt bis zur zugrunde liegenden Quelle verfolgen.",claim:"Geprüfte Aussage",locator:"Stelle in der Quelle (optional)",source:"Verknüpfte Quelle",add:"Beleglink freigeben",remove:"Entfernen",empty:"Noch keine Belege auf Aussageebene.",error:"Die Aktion konnte nicht abgeschlossen werden."},
  "fr-FR":{title:"Affirmations et preuves",intro:"Permettez au lecteur de relier une affirmation importante directement à sa source.",claim:"Affirmation vérifiée",locator:"Emplacement dans la source (facultatif)",source:"Source associée",add:"Valider le lien de preuve",remove:"Retirer",empty:"Aucune preuve au niveau de l’affirmation.",error:"L’action n’a pas pu aboutir."},
} as const;

export function ClaimCitationManager({articleId,locale,sourceIds,sources,citations}:{articleId:string;locale:keyof typeof copy;sourceIds:string[];sources:SupportingLibrary["sources"];citations:ArticleClaimCitation[]}){
  const labels=copy[locale],router=useRouter();const[message,setMessage]=useState("");const[pending,setPending]=useState(false);
  const attached=sources.filter(x=>sourceIds.includes(x.id));
  async function token(){const response=await fetch("/api/admin/auth/csrf",{cache:"no-store"});return ((await response.json()) as {token:string}).token;}
  async function submit(event:FormEvent<HTMLFormElement>){event.preventDefault();setPending(true);setMessage("");const data=new FormData(event.currentTarget);try{const response=await fetch(`/api/admin/articles/${articleId}/claim-citations`,{method:"POST",headers:{"content-type":"application/json","x-csrf-token":await token()},body:JSON.stringify({sourceId:data.get("sourceId"),claim:data.get("claim"),locator:data.get("locator")})});if(!response.ok)throw new Error();event.currentTarget.reset();router.refresh();}catch{setMessage(labels.error)}finally{setPending(false)}}
  async function remove(id:string){setPending(true);setMessage("");try{const response=await fetch(`/api/admin/articles/${articleId}/claim-citations/${id}`,{method:"DELETE",headers:{"x-csrf-token":await token()}});if(!response.ok)throw new Error();router.refresh();}catch{setMessage(labels.error)}finally{setPending(false)}}
  return <section className="admin-panel claim-citation-manager"><h2>{labels.title}</h2><p className="muted">{labels.intro}</p>{citations.length===0?<p>{labels.empty}</p>:<ol>{citations.map(item=><li key={item.id}><div><strong>{item.claim}</strong><a href={item.sourceUrl} target="_blank" rel="noreferrer">{item.sourceName}</a>{item.locator&&<small>{item.locator}</small>}</div><button type="button" className="danger-button" disabled={pending} onClick={()=>remove(item.id)}>{labels.remove}</button></li>)}</ol>}
    {attached.length>0&&<form className="admin-form" onSubmit={submit}><label>{labels.claim}<textarea name="claim" required maxLength={500}/></label><label>{labels.source}<select name="sourceId" required>{attached.map(item=><option key={item.id} value={item.id}>{item.name}</option>)}</select></label><label>{labels.locator}<input name="locator" maxLength={240}/></label><button disabled={pending}>{labels.add}</button></form>}{message&&<p role="alert">{message}</p>}</section>;
}
