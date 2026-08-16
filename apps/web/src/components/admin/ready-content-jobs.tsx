"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import type { Locale } from "@/i18n/config";

export type ReadyContentJob = {
  id:string; type:string; status:string; targetLocales:string[]; totalItems:number;
  completedItems:number; failedItems:number; currentPhase:number; lastMessage:string|null;
  createdAt:string; updatedAt:string; includeImages?:boolean; autoTranslate?:boolean; autoSeo?:boolean;
  turkishPublished?:number;translationPublished?:number;seoComplete?:number;latestContentAt?:string|null;
  recentArticles?:{title:string;slug:string;locale:string}[];
  isAutomaticallyScheduled?:boolean;categoryName?:string|null;requestedArticleType?:string|null;
};

const statusNames:Record<string,string>={Queued:"Kuyrukta",Running:"Çalışıyor",Paused:"Durduruldu",Completed:"Tamamlandı",Failed:"Hatalı",Cancelled:"İptal edildi"};
const phaseNames:Record<number,string>={1:"Araştırma ve konu planı",2:"Türkçe makale üretimi",3:"Kapak görselleri",4:"Otomatik çeviri",5:"Dil bazlı SEO",6:"Son doğrulama ve yayın"};
const activeStatuses=new Set(["Queued","Running","Paused"]);

export function ReadyContentJobs({locale,initialJobs}:{locale:Locale;initialJobs:ReadyContentJob[]}){
  const[jobs,setJobs]=useState(initialJobs),[refreshFailed,setRefreshFailed]=useState(false),[now,setNow]=useState(0);
  async function refresh(){try{const response=await fetch("/api/admin/automation/",{cache:"no-store"});if(!response.ok)throw new Error();const all=await response.json() as ReadyContentJob[];setJobs(all.filter(job=>job.type==="ReadyContentGeneration"));setRefreshFailed(false)}catch{setRefreshFailed(true)}}
  useEffect(()=>{const timer=window.setInterval(()=>{setNow(Date.now());void refresh()},1000);return()=>window.clearInterval(timer)},[]);
  const active=useMemo(()=>jobs.filter(job=>activeStatuses.has(job.status)),[jobs]);
  const recent=useMemo(()=>jobs.filter(job=>!activeStatuses.has(job.status)).slice(0,5),[jobs]);
  const automaticJobs=useMemo(()=>jobs.filter(job=>job.isAutomaticallyScheduled).slice(0,20),[jobs]);
  return <section className="admin-panel ready-content-jobs" aria-live="polite">
    <header><div><p className="section-kicker">CANLI TAKİP</p><h2>Devam eden işler</h2><p>Durum, faz ve kalan makale sayısı üç saniyede bir güncellenir.</p></div><button type="button" onClick={()=>void refresh()}>Şimdi yenile</button></header>
    {refreshFailed&&<p className="ready-content-refresh-error">Canlı bilgi geçici olarak alınamadı; mevcut bilgiler gösteriliyor.</p>}
    {active.length===0?<div className="ready-content-empty"><strong>Devam eden iş yok.</strong><span>Yeni iş başlattığınızda ilerlemesi burada görünecek.</span></div>:<div className="ready-content-job-list">{active.map(job=><JobCard key={job.id} job={job} locale={locale} now={now}/>)}</div>}
    {recent.length>0&&<div className="ready-content-recent"><h3>Son tamamlanan işler</h3><div className="ready-content-job-list">{recent.map(job=><JobCard key={job.id} job={job} locale={locale} compact/>)}</div></div>}
    {automaticJobs.length>0&&<div className="ready-content-recent"><h3>Otomatik üretim raporu</h3><div className="ready-content-job-list">{automaticJobs.map(job=><article className="ready-content-job" key={job.id}><header><div><strong>{job.categoryName??"Kategori"} · {job.requestedArticleType??"İçerik"}</strong><small>{statusNames[job.status]??job.status}</small></div></header>{job.recentArticles?.map(article=><Link key={article.slug} href={`/${article.locale}/articles/${article.slug}`} target="_blank">{article.title} ↗</Link>)}<footer><small>{new Intl.DateTimeFormat("tr-TR",{dateStyle:"short",timeStyle:"short"}).format(new Date(job.createdAt))}</small><Link href={`/${locale}/admin/automation/${job.id}`}>Raporu aç</Link></footer></article>)}</div></div>}
  </section>
}

function JobCard({job,locale,compact=false,now=0}:{job:ReadyContentJob;locale:Locale;compact?:boolean;now?:number}){
  const processed=Math.min(job.totalItems,job.completedItems+job.failedItems),percent=job.totalItems?Math.round(processed/job.totalItems*100):0,remaining=Math.max(0,job.totalItems-processed);
  const turkish=job.turkishPublished??job.completedItems,translationTotal=turkish*job.targetLocales.length,translations=job.translationPublished??0,seoTotal=turkish*(job.autoTranslate?job.targetLocales.length+1:1),seo=job.seoComplete??0;
  const heartbeatAge=now?Math.max(0,Math.floor((now-new Date(job.updatedAt).getTime())/1000)):0;
  return <article className="ready-content-job" data-status={job.status.toLowerCase()}>
    <header><div><strong>{phaseNames[job.currentPhase]??`Faz ${job.currentPhase}`}</strong><small>{statusNames[job.status]??job.status}</small></div><b>%{percent}</b></header>
    <div className="automation-progress" role="progressbar" aria-label="Hazır içerik işi ilerlemesi" aria-valuemin={0} aria-valuemax={100} aria-valuenow={percent}><span style={{width:`${percent}%`}}/></div>
    <div className="ready-content-job-metrics"><span><b>{Math.min(job.totalItems,turkish+1)}/{job.totalItems}</b> makale aşaması</span><span><b>{remaining}</b> kaldı</span><span><b>{job.failedItems}</b> hata</span></div>
    <div className="ready-content-job-metrics"><span><b>{turkish}/{job.totalItems}</b> Türkçe yayın</span><span><b>{translations}/{translationTotal}</b> çeviri</span><span><b>{seo}/{seoTotal}</b> SEO</span></div>
    {!compact&&<div className="ready-content-job-options"><span>Codex: <b>{job.status==="Running"?"Canlı çalışıyor":statusNames[job.status]??job.status}</b></span><span>Son sinyal: <b>{heartbeatAge} sn önce</b></span><span>Aktif faz: <b>{phaseNames[job.currentPhase]??job.currentPhase}</b></span></div>}
    {!compact&&job.recentArticles&&job.recentArticles.length>0&&<div className="ready-content-recent"><h3>Son tamamlanan içerikler</h3>{job.recentArticles.map(article=><Link key={article.slug} href={`/${article.locale}/articles/${article.slug}`} target="_blank">{article.title} ↗</Link>)}</div>}
    {!compact&&<div className="ready-content-job-options"><span>Görsel: {job.includeImages?"Açık":"Kapalı"}</span><span>Çeviri: {job.autoTranslate?job.targetLocales.join(", "):"Kapalı"}</span><span>SEO: {job.autoSeo?"Açık":"Kapalı"}</span></div>}
    {job.lastMessage&&<div className="ready-content-live-action"><span className="dashboard-live"><span/><strong>CANLI CODEX AKIŞI</strong></span><p>{job.lastMessage}</p></div>}
    <footer><small>Son güncelleme: {new Intl.DateTimeFormat("tr-TR",{dateStyle:"short",timeStyle:"medium"}).format(new Date(job.updatedAt))}</small><Link href={`/${locale}/admin/automation/${job.id}`}>Ayrıntılı takip</Link></footer>
  </article>
}
