"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import type { Locale } from "@/i18n/config";

type AutomationWorkload = { count:number;targetLocales:string[];blockedReason:string|null };
export type AutomationScan = { activeLocales:string[]; publishedArticles:number; missingTranslations:number; seoCandidates:number; siteLanguageCandidates:number; reportCandidates:number; runnerEnabled:boolean;workloads:{contentTranslation:AutomationWorkload;seoLocalization:AutomationWorkload;siteLocalization:AutomationWorkload;systemReport:AutomationWorkload} };
export type AutomationJob = { id:string; type:string; status:string; targetLocales:string[]; totalItems:number; completedItems:number; failedItems:number; currentPhase:number; lastMessage:string|null; createdAt:string; updatedAt:string; completedAt:string|null };

const cards = [
  { type:"ContentTranslation", workload:"contentTranslation", title:"Otomatik içerik çevirisi", description:"Yayındaki içeriklerin eksik dil sürümlerini kalıcı fazlara böler." },
  { type:"SeoLocalization", workload:"seoLocalization", title:"Otomatik SEO yerelleştirmesi", description:"Hedef dilde mevcut taslakların eksik SEO alanlarını insan onayına hazırlar." },
  { type:"SiteLocalization", workload:"siteLocalization", title:"Otomatik site dili", description:"Arayüzdeki eksik yerelleştirme anahtarlarını hedef dillere hazırlar." },
  { type:"SystemReport", workload:"systemReport", title:"Otomatik sistem raporu", description:"İşleri, hataları ve tamamlanma durumunu ayrıntılı bir raporda toplar." },
] as const;
const jobNames:Record<string,string> = Object.fromEntries(cards.map(card=>[card.type,card.title]));
jobNames.ReadyContentGeneration="Hazır içerik üretimi";
const statusNames:Record<string,string> = {Queued:"Kuyrukta",Running:"Çalışıyor",Paused:"Durduruldu",Completed:"Tamamlandı",Failed:"Hatalı",Cancelled:"İptal edildi"};

export function AutomationCenter({locale,initialScan,initialJobs}:{locale:Locale;initialScan:AutomationScan;initialJobs:AutomationJob[]}) {
  const [scan,setScan]=useState(initialScan),[jobs,setJobs]=useState(initialJobs),[busy,setBusy]=useState(false);
  async function refresh(){const [s,j]=await Promise.all([fetch("/api/admin/automation/scan",{cache:"no-store"}),fetch("/api/admin/automation/",{cache:"no-store"})]);if(s.ok)setScan(await s.json() as AutomationScan);if(j.ok)setJobs(await j.json() as AutomationJob[])}
  useEffect(()=>{const timer=window.setInterval(()=>void refresh(),5000);return()=>window.clearInterval(timer)},[]);
  async function post(path:string,body?:object){const csrf=await fetch("/api/admin/auth/csrf",{cache:"no-store"});const{token}=await csrf.json()as{token:string};const response=await fetch(`/api/admin/automation${path}`,{method:"POST",headers:{"content-type":"application/json","x-csrf-token":token},body:body?JSON.stringify(body):undefined});if(!response.ok){const problem=await response.json().catch(()=>null)as{message?:string}|null;throw new Error(problem?.message??"Toplu iş kaydedilemedi.")}await refresh()}
  async function start(type:string,workload:AutomationWorkload){const report=type==="SystemReport";const result=await Swal.fire({title:report?"Yeni sistem raporu oluşturulsun mu?":"Toplu iş başlatılsın mı?",text:report?"Codex mevcut sistem durumunu inceleyip ayrıntılı bir rapor kaydedecek.":`${workload.count} kayıt kalıcı kuyruğa eklenecek ve fazlar halinde çalışacak.`,icon:"question",showCancelButton:true,confirmButtonText:report?"Rapor oluştur":"Toplu çalıştır",cancelButtonText:"Vazgeç",background:"#151922",color:"#f4f6fa",confirmButtonColor:"#ff7651"});if(!result.isConfirmed)return;setBusy(true);try{await post("/",{type,targetLocales:workload.targetLocales})}catch(error){await Swal.fire({title:"İş başlatılamadı",text:error instanceof Error?error.message:"Beklenmeyen bir hata oluştu.",icon:"error",background:"#151922",color:"#f4f6fa"})}finally{setBusy(false)}}
  async function changeState(id:string,action:string){setBusy(true);try{await post(`/${id}/${action}`)}catch(error){await Swal.fire({title:"Durum değiştirilemedi",text:error instanceof Error?error.message:"Beklenmeyen bir hata oluştu.",icon:"error",background:"#151922",color:"#f4f6fa"})}finally{setBusy(false)}}
  const activeJobs=jobs.filter(job=>["Queued","Running","Paused"].includes(job.status)).length;
  return <>
    <section className={scan.runnerEnabled?"automation-runner-status ready":"automation-runner-status"}><span aria-hidden/><div><strong>{scan.runnerEnabled?"Codex worker hazır":"Codex worker kurulumu bekliyor"}</strong><small>{activeJobs===0?"Aktif iş kalmadı. Tamamlanan işlerin raporları geçmişte saklanır.":`${activeJobs} aktif iş çalışmayı bekliyor veya sürdürüyor.`}</small></div></section>
    <section className="automation-cards" aria-label="Toplu iş türleri">{cards.map(card=>{const workload=scan.workloads[card.workload],report=card.type==="SystemReport";return <article className="admin-panel" key={card.type}><span className="automation-icon" aria-hidden>✦</span><h2>{card.title}</h2><p>{card.description}</p><strong>{report?"İsteğe bağlı":`${workload.count} iş`}</strong>{!report&&workload.blockedReason&&<small className="automation-blocked-reason">{workload.blockedReason}</small>}<button disabled={busy||!scan.runnerEnabled||(!report&&workload.count===0)} onClick={()=>void start(card.type,workload)}>{report?"YENİ RAPOR OLUŞTUR":"TÜMÜNÜ FAZLARLA ÇALIŞTIR"}</button></article>})}</section>
    <section className="admin-panel automation-jobs"><header><div><p className="section-kicker">OTOMATİK İŞ GEÇMİŞİ</p><h2>Son işler ve raporlar</h2></div><button onClick={()=>void refresh()}>Yenile</button></header>
      {jobs.length===0?<p className="muted">Henüz toplu iş yok.</p>:jobs.map(job=>{const processed=job.completedItems+job.failedItems,percent=job.totalItems?Math.round(processed/job.totalItems*100):0;return <article key={job.id}><header><span><strong>{jobNames[job.type]??job.type}</strong><small>{job.targetLocales.join(", ")||"Sistem"} · Faz {job.currentPhase}</small></span><b>{statusNames[job.status]??job.status}</b></header><div className="automation-progress" role="progressbar" aria-label="İş ilerlemesi" aria-valuemin={0} aria-valuemax={100} aria-valuenow={percent}><span style={{width:`${percent}%`}}/></div><footer><span>%{percent} · {job.completedItems}/{job.totalItems} tamamlandı · {job.failedItems} hata</span><nav aria-label="İş eylemleri"><Link className="automation-report-link" href={`/${locale}/admin/automation/${job.id}`}>Ayrıntılı rapor</Link>{["Queued","Running"].includes(job.status)&&<button disabled={busy} onClick={()=>void changeState(job.id,"pause")}>Durdur</button>}{job.status==="Paused"&&<button disabled={busy} onClick={()=>void changeState(job.id,"resume")}>Devam et</button>}{job.status==="Failed"&&<button disabled={busy} onClick={()=>void changeState(job.id,"retry")}>Yeniden dene</button>}{!["Completed","Cancelled"].includes(job.status)&&<button disabled={busy} onClick={()=>void changeState(job.id,"cancel")}>İptal</button>}</nav></footer>{job.lastMessage&&<p>{job.lastMessage}</p>}</article>})}
    </section>
  </>;
}
