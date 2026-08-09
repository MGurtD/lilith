---
name: contextual-help
description: Add, update, translate, or audit Lilith Alt+H contextual help. Use when documenting a frontend route or module, adding meta.helpKey, creating help Markdown, or checking help behavior and consistency.
compatibility: OpenCode with the Lilith frontend source available.
---

# Contextual Help

Generate documentation from actual behavior, never from route names or generic templates.

## Discover

1. Read `frontend/docs/help-module.md` as the canonical contract.
2. Resolve the route, `meta.helpKey`, view, owned components, store actions, service operations, permissions, lifecycle restrictions, and navigation outcomes.
3. Inspect sibling help files for terminology and depth.
4. Identify which locales already exist for the target help key.

## Write

- Use a stable help key consistent with current module structure.
- Document only behavior supported by source evidence.
- Follow the mandatory section order in `help-module.md`.
- Write for end users: purpose, available actions, realistic flow, restrictions, common errors, and a simple Mermaid process when useful.
- Keep business terminology consistent across the module.
- Do not expose component, store, endpoint, or implementation details unless users need them.
- Do not invent period filters, status restrictions, or workflows shared by unrelated screens.
- Add `meta.helpKey` only when the route does not already provide the correct key.

## Verify

- Confirm the help path matches runtime resolution for each changed locale.
- Confirm Markdown headings and Mermaid syntax follow the documented contract.
- Check that `Alt+H` can resolve the route key; run a relevant smoke check when practical.
- Review sibling files for consistency after adding a materially better document.

Never commit or push automatically. Summarize evidence, files created or changed, locales covered, and any runtime verification not performed.
