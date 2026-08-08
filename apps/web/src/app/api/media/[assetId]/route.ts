const apiBaseUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5267";

export async function GET(_request:Request,{params}:RouteContext<"/api/media/[assetId]">){
  const {assetId}=await params;
  const response=await fetch(new URL(`/api/v1/public/media/${encodeURIComponent(assetId)}`,apiBaseUrl),{cache:"no-store"});
  if(response.status===404)return new Response(null,{status:404});
  if(!response.ok)return new Response(null,{status:502});
  return new Response(response.body,{headers:{"content-type":response.headers.get("content-type")??"application/octet-stream","cache-control":"public, max-age=86400, stale-while-revalidate=604800","last-modified":response.headers.get("last-modified")??new Date().toUTCString(),"x-content-type-options":"nosniff"}});
}
