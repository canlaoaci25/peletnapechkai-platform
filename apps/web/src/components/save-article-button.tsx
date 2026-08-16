"use client";
import Link from "next/link";
import { useEffect, useState } from "react";
import type { Locale } from "@/i18n/config";
import { memberCopy } from "@/i18n/member-copy";

export function SaveArticleButton({locale,slug}:{locale:Locale;slug:string}) {
  const copy=memberCopy[locale],[saved,setSaved]=useState(false),[authenticated,setAuthenticated]=useState<boolean|null>(null),[busy,setBusy]=useState(false),[message,setMessage]=useState("");
  useEffect(()=>{let active=true;void fetch(`/api/admin/account/saved/${encodeURIComponent(locale)}/${encodeURIComponent(slug)}`,{cache:"no-store"}).then(async response=>{if(!active)return;if(response.status===401){setAuthenticated(false);return}if(response.ok){const result=await response.json() as {saved:boolean};setSaved(result.saved);setAuthenticated(true)}}).catch(()=>{if(active)setAuthenticated(false)});return()=>{active=false}},[locale,slug]);
  async function toggle(){setBusy(true);setMessage("");try{const csrfResponse=await fetch("/api/admin/auth/csrf",{cache:"no-store"}),{token}=await csrfResponse.json() as {token:string};const response=await fetch(`/api/admin/account/saved/${encodeURIComponent(locale)}/${encodeURIComponent(slug)}`,{method:saved?"DELETE":"PUT",headers:{"x-csrf-token":token}});if(!response.ok)throw new Error();setSaved(!saved);setMessage(saved?copy.removedSuccess:copy.savedSuccess)}catch{setMessage(copy.failed)}finally{setBusy(false)}}
  if(authenticated===false)return <div className="article-save"><Link className="save-button" href={`/${locale}/account/login`}>♡ {copy.signInToSave}</Link></div>;
  return <div className="article-save"><button className="save-button" type="button" aria-pressed={saved} disabled={busy||authenticated===null} onClick={toggle}><span aria-hidden="true">{saved?"♥":"♡"}</span> {copy.save}</button><span className="save-status" role="status">{busy?copy.saveBusy:message||(saved?copy.saved:"")}</span></div>;
}
