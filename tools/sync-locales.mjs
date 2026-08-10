import { readFile, writeFile } from "node:fs/promises";
import path from "node:path";

const root = process.cwd();
const source = JSON.parse(await readFile(path.join(root, "config/supported-locales.json"), "utf8"));
await writeFile(path.join(root, "apps/web/src/i18n/supported-locales.generated.json"), `${JSON.stringify(source, null, 2)}\n`, "utf8");
console.log("Web locale configuration synchronized.");
