"use client";
import Script from "next/script";
import {useEffect,useState} from "react";
import {consentChangeEvent,consentStorageKey,hasOptionalConsent} from "@/lib/consent";
export function ThirdPartyIntegrations(){
 const [allowed,setAllowed]=useState(false);
 useEffect(()=>{const update=()=>setAllowed(hasOptionalConsent(localStorage));const onStorage=(event:StorageEvent)=>{if(event.key===consentStorageKey)update()};update();window.addEventListener("storage",onStorage);window.addEventListener(consentChangeEvent,update);return()=>{window.removeEventListener("storage",onStorage);window.removeEventListener(consentChangeEvent,update)}},[]);
 const ga=process.env.NEXT_PUBLIC_GA_MEASUREMENT_ID;
 const clarity=process.env.NEXT_PUBLIC_CLARITY_PROJECT_ID;
 const adsense=process.env.NEXT_PUBLIC_ADSENSE_CLIENT;
 return <>
  {allowed&&clarity&&/^[a-z0-9]+$/i.test(clarity)&&<Script id="boecl-clarity" strategy="lazyOnload">{`(function(c,l,a,r,i,t,y){c[a]=c[a]||function(){(c[a].q=c[a].q||[]).push(arguments)};t=l.createElement(r);t.async=1;t.src='https://www.clarity.ms/tag/'+i;y=l.getElementsByTagName(r)[0];y.parentNode.insertBefore(t,y)})(window,document,'clarity','script','${clarity}');clarity('consentv2',{analytics_storage:'GRANTED',ad_storage:'DENIED'});`}</Script>}
  {allowed&&adsense&&/^ca-pub-\d+$/.test(adsense)&&<Script id="boecl-adsense" async crossOrigin="anonymous" src={`https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=${adsense}`} strategy="lazyOnload"/>}
  {allowed&&ga&&/^G-[A-Z0-9]+$/.test(ga)&&<><Script src={`https://www.googletagmanager.com/gtag/js?id=${ga}`} strategy="lazyOnload"/><Script id="boecl-ga" strategy="lazyOnload">{`window.dataLayer=window.dataLayer||[];function gtag(){dataLayer.push(arguments)}gtag('js',new Date());gtag('config','${ga}',{anonymize_ip:true});`}</Script></>}
 </>;
}
