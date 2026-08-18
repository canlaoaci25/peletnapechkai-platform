import type { CSSProperties } from "react";

export type FocalCover = { focalX?: number | null; focalY?: number | null };

export function focalPointStyle(cover: FocalCover | null | undefined): CSSProperties {
  const x = Math.min(1, Math.max(0, cover?.focalX ?? .5));
  const y = Math.min(1, Math.max(0, cover?.focalY ?? .5));
  return { objectPosition: `${x * 100}% ${y * 100}%` };
}
