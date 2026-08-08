"use client";
import {useEffect,useState} from "react";
import Link from "next/link";
import type {Locale} from "@/i18n/config";
const copy={"tr-TR":{text:"Deneyimi ölçmek için isteğe bağlı analiz kullanabiliriz.",accept:"Kabul et",reject:"Reddet",info:"Ayrıntılar"},"en-US":{text:"We may use optional analytics to measure the experience.",accept:"Accept",reject:"Reject",info:"Details"},"de-DE":{text:"Wir können optionale Analysen zur Messung verwenden.",accept:"Akzeptieren",reject:"Ablehnen",info:"Details"}};
export function ConsentBanner({locale}:{locale:Locale}){const [visible,setVisible]=useState(false);useEffect(()=>{const timer=setTimeout(()=>setVisible(!localStorage.getItem("boecl-consent")),0);return()=>clearTimeout(timer)},[]);if(!visible)return null;const choose=(value:string)=>{localStorage.setItem("boecl-consent",value);setVisible(false)};const c=copy[locale];return <aside className="consent-banner" aria-label={c.info}><p>{c.text} <Link href={`/${locale}/legal/cookies`}>{c.info}</Link></p><div><button onClick={()=>choose("denied")}>{c.reject}</button><button onClick={()=>choose("granted")}>{c.accept}</button></div></aside>}
