"use client";
import { useEffect } from "react";
export function ArticleEngagement({locale,slug}:{locale:string;slug:string}){useEffect(()=>{const send=(kind:string,seconds=0)=>fetch("/api/engagement",{method:"POST",headers:{"content-type":"application/json"},body:JSON.stringify({locale,slug,kind,seconds}),keepalive:true});void send("view");const timer=window.setTimeout(()=>void send("engaged",30),30000);return()=>window.clearTimeout(timer)},[locale,slug]);return null}
