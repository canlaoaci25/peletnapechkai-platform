"use client";
import Link from "next/link";
import { useMemo, useState } from "react";
import type { ArticleSummary, SystemStatus } from "@/lib/admin-api";
const labels:Record<string,string>={Draft:"Taslak",InEditorialReview:"Editoryal inceleme",InSeoReview:"SEO incelemesi",Scheduled:"Planlandı",Published:"Yayında",Archived:"Arşivlendi"};
export function AdminDashboard({locale,articles,status}:{locale:string;articles:ArticleSummary[];status:SystemStatus|null}){
 const[query,setQuery]=useState("");const[filter,setFilter]=useState("All");
 const counts=useMemo(()=>articles.reduce<Record<string,number>>((a,x)=>({...a,[x.status]:(a[x.status]??0)+1}),{}),[articles]);
 const filtered=useMemo(()=>articles.filter(x=>(filter==="All"||x.status===filter)&&(!query.trim()||`${x.title} ${x.slug} ${x.type}`.toLocaleLowerCase("tr-TR").includes(query.trim().toLocaleLowerCase("tr-TR")))),[articles,filter,query]);
 return <><section className="admin-metrics" aria-label="İçerik özeti">
  <button className={filter==="All"?"active":""} onClick={()=>setFilter("All")}><span>Toplam içerik</span><strong>{status?.articles??articles.length}</strong></button>
  <button className={filter==="Draft"?"active":""} onClick={()=>setFilter("Draft")}><span>Taslak</span><strong>{counts.Draft??0}</strong></button>
  <button onClick={()=>setFilter("InEditorialReview")}><span>İnceleme bekliyor</span><strong>{(counts.InEditorialReview??0)+(counts.InSeoReview??0)}</strong></button>
  <button className={filter==="Published"?"active":""} onClick={()=>setFilter("Published")}><span>Yayında</span><strong>{status?.published??counts.Published??0}</strong></button>
 </section><section className="admin-panel admin-content-workspace"><header className="workspace-header"><div><p className="section-kicker">İÇERİK OPERASYONU</p><h2>Tüm içerikler</h2><p className="muted">Arayın, filtreleyin ve düzenlemek için doğrudan açın.</p></div><Link className="primary-link" href={`/${locale}/admin/articles/new`}>+ Yeni içerik</Link></header>
 <div className="admin-toolbar"><label><span className="sr-only">İçerik ara</span><input value={query} onChange={e=>setQuery(e.target.value)} placeholder="Başlık, kısa ad veya tür ara…"/></label><label><span className="sr-only">Duruma göre filtrele</span><select value={filter} onChange={e=>setFilter(e.target.value)}><option value="All">Tüm durumlar</option>{Object.entries(labels).map(([v,l])=><option value={v} key={v}>{l}</option>)}</select></label><strong>{filtered.length} sonuç</strong></div>
 {filtered.length===0?<div className="admin-empty"><strong>Sonuç bulunamadı</strong><span>Arama ifadesini veya durum filtresini değiştirin.</span></div>:<div className="article-table" role="table"><div className="article-row article-row-head" role="row"><span>İçerik</span><span>Durum</span><span>Güncelleme</span><span>İşlem</span></div>{filtered.map(x=><div className="article-row" role="row" key={x.id}><span><Link href={`/${locale}/admin/articles/${x.id}`}><strong>{x.title}</strong></Link><small>{x.locale} · {x.type} · /{x.slug}</small></span><span><span className={`status-badge status-${x.status.toLowerCase()}`}>{labels[x.status]??x.status}</span></span><time dateTime={x.updatedAt}>{new Intl.DateTimeFormat("tr-TR",{dateStyle:"medium"}).format(new Date(x.updatedAt))}</time><Link className="row-action" href={`/${locale}/admin/articles/${x.id}`}>Düzenle →</Link></div>)}</div>}
 </section></>;
}
