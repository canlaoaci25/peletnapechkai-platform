import { NextResponse } from "next/server";
const apiBaseUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5267";
export async function POST(request: Request) {
  try {
    const body = await request.text();
    if (body.length > 512) return NextResponse.json({ message: "Invalid request." }, { status: 400 });
    const response = await fetch(new URL("/api/v1/public/web-vitals", apiBaseUrl), { method: "POST", headers: { "content-type": "application/json" }, body });
    return new NextResponse(null, { status: response.status });
  } catch { return NextResponse.json({ message: "Invalid request." }, { status: 400 }); }
}
