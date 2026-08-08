"use client";

import dynamic from "next/dynamic";

const BlockEditor = dynamic(() => import("./block-editor"), {
  ssr: false,
  loading: () => (
    <div className="block-editor-loading">Gelişmiş editör yükleniyor…</div>
  ),
});

export function RichTextEditor(props: {
  name: string;
  initialValue?: string;
  label: string;
  onMetrics?: (words: number) => void;
}) {
  return <BlockEditor {...props} />;
}
