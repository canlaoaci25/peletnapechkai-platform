export const dynamic="force-dynamic";
export function GET(){const key=process.env.INDEXNOW_KEY??"";if(!/^[A-Za-z0-9-]{8,128}$/.test(key))return new Response("Not configured.",{status:404});return new Response(`${key}\n`,{headers:{"content-type":"text/plain; charset=utf-8","cache-control":"public, max-age=86400"}})}
