// PreToolUse guard for the lilith-ui-tester agent's Playwright tools.
// - Budget: counts browser calls per subagent run and blocks them past the cap,
//   so a run always ends with a report instead of hitting its turn limit.
// - Files: blocks relative `filename`s, which Playwright MCP resolves against the
//   repository root; omitting it saves under the configured output folder.
// - browser_evaluate is read-only: no requests, storage changes or scripted
//   clicks, so every action goes through the UI tools and the click trail.
// Exit code 2 blocks the call and hands the reason back to the agent.
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { isAbsolute, join } from "node:path";

const BUDGET = 45;
const WARN_FROM = 35;

// The subagent's transcript sits next to the session's:
// <session>.jsonl -> <session>/subagents/agent-<agent_id>.jsonl
function taskAuthorisesWrites(payload) {
  try {
    const session = String(payload.transcript_path ?? "").replace(/\.jsonl$/, "");
    const file = join(session, "subagents", `agent-${payload.agent_id}.jsonl`);
    for (const line of readFileSync(file, "utf8").split("\n")) {
      if (!line.trim()) continue;
      const entry = JSON.parse(line);
      if (entry.type !== "user") continue;
      const content = entry.message?.content;
      const text = Array.isArray(content)
        ? content.map((part) => part.text ?? "").join("\n")
        : String(content ?? "");
      return /^\s*WRITES AUTHORI[SZ]ED\b/im.test(text);
    }
  } catch {}
  return false;
}

let raw = "";
process.stdin.on("data", (chunk) => (raw += chunk));
process.stdin.on("end", () => {
  let payload;
  try {
    payload = JSON.parse(raw);
  } catch {
    process.exit(0);
  }
  const tool = String(payload.tool_name ?? "").replace(/^mcp__playwright-lilith__/, "");
  const input = payload.tool_input ?? {};
  const block = (reason) => {
    process.stderr.write(reason);
    process.exit(2);
  };

  // Budget, per subagent instance.
  // CLAUDE_PROJECT_DIR, not the payload's cwd: cwd follows the session's shell.
  const root = process.env.CLAUDE_PROJECT_DIR ?? payload.cwd ?? process.cwd();
  const stateDir = join(root, ".claude", ".ui-tester", "state");
  const key = String(payload.agent_id ?? payload.session_id ?? "unknown").replace(/[^\w.-]/g, "_");
  let count = 0;
  try {
    mkdirSync(stateDir, { recursive: true });
    const file = join(stateDir, `${key}.count`);
    try {
      count = parseInt(readFileSync(file, "utf8"), 10) || 0;
    } catch {}
    count += 1;
    writeFileSync(file, String(count));
  } catch {}
  if (count > BUDGET) {
    block(
      `Budget exhausted: this would be browser call ${count} of ${BUDGET}. Make no more browser ` +
        "calls. Write your report now: criteria you did not reach are SKIP and the RESULT is " +
        "PARTIAL unless you already saw a FAIL.",
    );
  }

  // Writes need a "WRITES AUTHORISED:" line in the task (the run's first message).
  // A criterion that describes a write is not permission to make it.
  if (tool === "browser_click") {
    const label = `${input.element ?? ""} ${input.target ?? ""}`;
    const backsOut = /\b(no|cancel\w*|cancel·lar|tancar|close|reject\w*|rebutjar)\b/i.test(label);
    const writes =
      /\b(s[ií]|yes|acceptar|aceptar|accept|confirm\w*|guardar|save|desar|eliminar|delete|esborrar|borrar|trash|finalitzar|enviar|send|submit)\b|delete-cell/i.test(label);
    if (writes && !backsOut && !taskAuthorisesWrites(payload)) {
      block(
        `Blocked: "${input.element ?? input.target}" looks like a control that persists data, and the ` +
          'task has no "WRITES AUTHORISED:" line. A criterion that describes a write does not ' +
          "authorise it. Back out (cancel or close), mark the criterion SKIP (\"needs a write the " +
          'task did not authorise") and report PARTIAL.',
      );
    }
  }

  if (typeof input.filename === "string" && input.filename && !isAbsolute(input.filename)) {
    block(
      `Blocked: relative filename "${input.filename}" would be written into the repository. ` +
        "Call the tool again without `filename`; the result gives the saved file's absolute path.",
    );
  }

  if (tool === "browser_evaluate") {
    const code = String(input.function ?? input.code ?? "");
    const rules = [
      [/\bfetch\s*\(|XMLHttpRequest|\baxios\b|sendBeacon|\bWebSocket\b/, "issues a network request"],
      [/(localStorage|sessionStorage)\s*\.\s*(setItem|removeItem|clear)|document\.cookie\s*=|indexedDB/, "changes browser storage"],
      [/\.\s*(click|submit|requestSubmit)\s*\(|dispatchEvent\s*\(/, "activates controls from script"],
      [/\$patch|\.\$reset\s*\(|\$router\s*\.\s*(push|replace)/, "changes app state from script"],
    ];
    const hit = rules.find(([re]) => re.test(code));
    if (hit) {
      block(
        `Blocked: this browser_evaluate ${hit[1]}. browser_evaluate is read-only here: read the ` +
          "DOM, computed styles, URL or storage values, and act through the click/type/navigate " +
          "tools so the action lands in your click trail.",
      );
    }
  }

  if (count >= WARN_FROM) {
    process.stdout.write(
      JSON.stringify({
        hookSpecificOutput: {
          hookEventName: "PreToolUse",
          additionalContext: `Browser call ${count} of ${BUDGET}. Finish the criterion in hand, then write the report.`,
        },
      }),
    );
  }
  process.exit(0);
});
