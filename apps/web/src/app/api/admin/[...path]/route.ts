import { NextRequest } from "next/server";

const apiBaseUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5267";
const allowedRoutes = [
  /^auth\/(csrf|login|login\/2fa|logout|session)$/,
  /^articles(?:\/|$)/,
  /^users(?:\/|$)/,
];

function apiPath(path: string[]) {
  const joined = path.join("/");
  if (!allowedRoutes.some((route) => route.test(joined))) return null;
  return joined.startsWith("auth/")
    ? `/api/v1/${joined}`
    : `/api/v1/admin/${joined}`;
}

async function forward(request: NextRequest, context: RouteContext<"/api/admin/[...path]">) {
  const { path } = await context.params;
  const pathname = apiPath(path);
  if (!pathname) return Response.json({ message: "Not found." }, { status: 404 });

  const target = new URL(pathname, apiBaseUrl);
  target.search = request.nextUrl.search;
  const headers = new Headers();
  for (const name of ["accept", "content-type", "cookie", "x-csrf-token"]) {
    const value = request.headers.get(name);
    if (value) headers.set(name, value);
  }

  try {
    const upstream = await fetch(target, {
      method: request.method,
      headers,
      body: request.method === "GET" || request.method === "HEAD" ? undefined : await request.arrayBuffer(),
      cache: "no-store",
      redirect: "manual",
    });
    const responseHeaders = new Headers();
    for (const name of ["content-type", "cache-control"]) {
      const value = upstream.headers.get(name);
      if (value) responseHeaders.set(name, value);
    }
    for (const cookie of upstream.headers.getSetCookie()) responseHeaders.append("set-cookie", cookie);
    responseHeaders.set("cache-control", "no-store");
    return new Response(upstream.body, { status: upstream.status, headers: responseHeaders });
  } catch {
    return Response.json({ message: "Administration service is unavailable." }, { status: 502 });
  }
}

export const GET = forward;
export const POST = forward;
export const PUT = forward;
