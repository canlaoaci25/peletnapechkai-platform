import {readFile} from "node:fs/promises";
import path from "node:path";

export const dynamic="force-dynamic";

export async function GET(){
  try{
    const content=await readFile(path.join(process.cwd(),"public","sitemap.txt"),"utf8");
    if(!content.trim())throw new Error("Empty sitemap text file");
    return new Response(content,{status:200,headers:{"content-type":"text/plain; charset=utf-8","cache-control":"public, max-age=300, stale-while-revalidate=3600"}});
  }catch{
    return new Response("sitemap.txt is temporarily unavailable.\n",{status:503,headers:{"content-type":"text/plain; charset=utf-8","cache-control":"no-store"}});
  }
}
