"use client";

import "@blocknote/core/fonts/inter.css";
import "@blocknote/mantine/style.css";
import { en } from "@blocknote/core/locales";
import { BlockNoteView } from "@blocknote/mantine";
import { useCreateBlockNote } from "@blocknote/react";
import { useEffect, useRef, useState } from "react";

type Props = {
  name: string;
  initialValue?: string;
  label: string;
  onMetrics?: (words: number) => void;
};

async function csrfToken() {
  const response = await fetch("/api/admin/auth/csrf", { cache: "no-store" });
  if (!response.ok) throw new Error("Oturum doğrulanamadı.");
  return ((await response.json()) as { token: string }).token;
}

export default function BlockEditor({
  name,
  initialValue = "",
  label,
  onMetrics,
}: Props) {
  const [html, setHtml] = useState(initialValue);
  const sequence = useRef(0);
  const editor = useCreateBlockNote({
    dictionary: {
      ...en,
      placeholders: {
        ...en.placeholders,
        emptyDocument:
          "İçeriğinizi yazmaya başlayın veya / yazarak blok ekleyin…",
        default: "Metin yazın veya / ile blok seçin…",
        heading: "Başlık",
      },
    },
    uploadFile: async (file) => {
      if (!file.type.startsWith("image/"))
        throw new Error("Yalnızca JPEG, PNG ve WebP görsel yüklenebilir.");
      const data = new FormData();
      data.append("file", file);
      const response = await fetch("/api/admin/media/", {
        method: "POST",
        headers: { "x-csrf-token": await csrfToken() },
        body: data,
      });
      if (!response.ok) throw new Error("Görsel yüklenemedi.");
      const asset = (await response.json()) as { id: string };
      return `/api/media/${asset.id}`;
    },
  });

  useEffect(() => {
    if (!initialValue) return;
    const blocks = editor.tryParseHTMLToBlocks(initialValue);
    editor.replaceBlocks(editor.document, blocks);
  }, [editor, initialValue]);

  function exportHtml() {
    const current = ++sequence.current;
    const next = editor.blocksToHTMLLossy(editor.document);
    if (current === sequence.current) setHtml(next);
    const text = editor.document
      .map((block) => editor.blocksToMarkdownLossy([block]))
      .join(" ")
      .replace(/[#*_`>\-[\]()]/g, " ");
    onMetrics?.(text.trim() ? text.trim().split(/\s+/u).length : 0);
  }

  return (
    <div className="rich-editor-field">
      <span className="rich-editor-label">{label}</span>
      <div className="block-editor-shell">
        <BlockNoteView editor={editor} theme="dark" onChange={exportHtml} />
      </div>
      <textarea hidden readOnly name={name} value={html} />
      <small>
        / menüsüyle başlık, liste, tablo, kontrol listesi, kod, alıntı, görsel,
        video ve dosya blokları ekleyebilirsiniz. Görseller BOECL medya
        kütüphanesine yüklenir.
      </small>
    </div>
  );
}
