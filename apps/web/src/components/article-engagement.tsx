"use client";
import { useEffect } from "react";
import { scheduleWhenIdle } from "@/lib/browser-idle";

export function ArticleEngagement({locale,slug}:{locale:string;slug:string}){useEffect(()=>{let timer:number|undefined;const send=(kind:string,seconds=0)=>fetch("/api/engagement",{method:"POST",headers:{"content-type":"application/json"},body:JSON.stringify({locale,slug,kind,seconds}),keepalive:true});const cancelIdle=scheduleWhenIdle(()=>{void send("view");timer=window.setTimeout(()=>void send("engaged",30),30000)});return()=>{cancelIdle();if(timer!==undefined)window.clearTimeout(timer)}},[locale,slug]);return null}
