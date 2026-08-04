import { NextRequest, NextResponse } from "next/server";

import { defaultLocale, hasLocale } from "@/i18n/config";

export function proxy(request: NextRequest) {
  const pathname = request.nextUrl.pathname;
  const firstSegment = pathname.split("/").filter(Boolean)[0];

  if (firstSegment && hasLocale(firstSegment)) {
    return NextResponse.next();
  }

  const destination = request.nextUrl.clone();
  destination.pathname = `/${defaultLocale}${pathname === "/" ? "" : pathname}`;

  return NextResponse.redirect(destination);
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)"],
};
