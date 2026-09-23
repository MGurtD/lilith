@AGENTS.md

# Claude Code

The import above loads the shared project rules from `AGENTS.md`, which OpenCode also reads. The rest of this file only adds what is specific to Claude Code.

## Exploration

- For a single known symbol or file, query CodeGraph (`codegraph_explore`) directly.
- For questions that span several files or a whole flow, delegate to the read-only explorers in `.claude/agents/` instead of reading files yourself:
  - `lilith-backend-explorer` for `backend/`
  - `lilith-frontend-explorer` for `frontend/`
- When a question crosses the API contract, launch both explorers in parallel in one message. Give each a self-contained task: what you need to know, why, and what you already know.
- After the reports arrive, cross-check the contract yourself (HTTP method and route, DTO and TypeScript property names) and flag mismatches.

## Hooks

- Editing any file under `frontend/src/i18n/` runs the strict i18n check (`.claude/hooks/i18n-check.mjs`). A blocking report means locale parity or placeholders broke; fix it before continuing.
