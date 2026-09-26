// PostToolUse hook: after an edit to a frontend locale dictionary, run the strict
// i18n check and block with its report when locale parity or placeholders break.
import { spawnSync } from "node:child_process";
import path from "node:path";

let raw = "";
for await (const chunk of process.stdin) raw += chunk;

let input;
try {
  input = JSON.parse(raw);
} catch {
  process.exit(0);
}

const filePath = input?.tool_input?.file_path;
if (typeof filePath !== "string") process.exit(0);
if (!/(^|\/)frontend\/src\/i18n\//.test(filePath.replaceAll("\\", "/"))) process.exit(0);

const repoRoot = process.env.CLAUDE_PROJECT_DIR || input.cwd || process.cwd();
const result = spawnSync(
  process.execPath,
  [
    path.join(repoRoot, "frontend", "scripts", "audit-i18n.mjs"),
    "--strict",
    "--scope",
    "frontend/src/i18n",
    "--repo-root",
    repoRoot,
  ],
  { cwd: repoRoot, encoding: "utf8" },
);

if (result.status === 0) process.exit(0);

process.stderr.write(
  "pnpm run i18n:check failed after editing " + filePath + ":\n" + (result.stdout || "") + (result.stderr || ""),
);
process.exit(2);
