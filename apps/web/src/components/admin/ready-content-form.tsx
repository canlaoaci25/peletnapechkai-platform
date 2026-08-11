"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import Swal from "sweetalert2";
import type { Locale } from "@/i18n/config";

type Category = { id:string; name:string; slug:string };

export function ReadyContentForm({locale,categories,runnerEnabled}:{locale:Locale;categories:Category[];runnerEnabled:boolean}){
  const router=useRouter();
  const[categoryId,setCategoryId]=useState(categories[0]?.id??"");const[type,setType]=useState("Guide");const[count,setCount]=useState(5);
  const[includeImages,setIncludeImages]=useState(true);const[autoTranslate,setAutoTranslate]=useState(true);const[autoSeo,setAutoSeo]=useState(true);const[busy,setBusy]=useState(false);
  async function submit(event:React.FormEvent){event.preventDefault();const confirmation=await Swal.fire({title:"Hazır içerik işi başlatılsın mı?",html:`<b>${count}</b> özgün Türkçe makale araştırılacak ve doğrudan yayımlanacak.<br>Seçili çeviri, SEO ve görsel fazlarının tamamı bitmeden iş tamamlanmayacak.`,icon:"question",showCancelButton:true,confirmButtonText:"Fazları başlat",cancelButtonText:"Vazgeç",background:"#151922",color:"#f4f6fa",confirmButtonColor:"#ff7651"});if(!confirmation.isConfirmed)return;
    setBusy(true);try{const csrf=await fetch("/api/admin/auth/csrf",{cache:"no-store"});const{token}=await csrf.json()as{token:string};const response=await fetch("/api/admin/automation/ready-content",{method:"POST",headers:{"content-type":"application/json","x-csrf-token":token},body:JSON.stringify({categoryId,articleType:type,count,includeImages,autoTranslate,autoSeo})});const result=await response.json().catch(()=>null)as{id?:string;message?:string}|null;if(!response.ok)throw new Error(result?.message??"Hazır içerik işi başlatılamadı.");router.push(`/${locale}/admin/automation/${result!.id}`);router.refresh()}catch(error){await Swal.fire({title:"İş başlatılamadı",text:error instanceof Error?error.message:"Beklenmeyen hata",icon:"error",background:"#151922",color:"#f4f6fa"})}finally{setBusy(false)}}
  return <form className="admin-panel ready-content-form" onSubmit={submit}>
    <section><label htmlFor="ready-category">Kategori</label><select id="ready-category" value={categoryId} onChange={event=>setCategoryId(event.target.value)} required>{categories.map(category=><option key={category.id} value={category.id}>{category.name}</option>)}</select></section>
    <section><label htmlFor="ready-type">İçerik türü</label><select id="ready-type" value={type} onChange={event=>setType(event.target.value)}><option value="News">Haber</option><option value="Guide">Rehber</option><option value="Review">İnceleme</option><option value="Analysis">Analiz</option></select></section>
    <section><label htmlFor="ready-count">Makale adedi</label><input id="ready-count" type="number" min={1} max={50} value={count} onChange={event=>setCount(Number(event.target.value))} required/><small>Tek işte 1–50 ayrıntılı makale üretilebilir.</small></section>
    <fieldset><legend>Üretim fazları</legend>
      <label><input type="checkbox" checked={includeImages} onChange={event=>setIncludeImages(event.target.checked)}/><span><strong>Resimli</strong><small>Her makaleye özgün 1200×675 BOECL WebP kapağı üret.</small></span></label>
      <label><input type="checkbox" checked={autoTranslate} onChange={event=>setAutoTranslate(event.target.checked)}/><span><strong>Otomatik çeviri</strong><small>Etkin yabancı dillerin tamamına çevir ve yayımla.</small></span></label>
      <label><input type="checkbox" checked={autoSeo} onChange={event=>setAutoSeo(event.target.checked)}/><span><strong>Otomatik SEO</strong><small>Türkçe ve çevrilmiş makalelerin SEO başlık/açıklamalarını tamamla.</small></span></label>
    </fieldset>
    <aside><strong>Özgünlük ve araştırma koruması</strong><p>Canlı web araştırması yapılır; her makalede en az iki gerçek kaynak saklanır. Başlık ve özetler mevcut 208 yayınla ve aynı paketteki yeni yazılarla benzerlik kontrolünden geçer.</p></aside>
    <button disabled={busy||!runnerEnabled||categories.length===0}>{busy?"Kuyruğa alınıyor…":"ARAŞTIRMA VE ÜRETİM FAZLARINI BAŞLAT"}</button>
  </form>
}
