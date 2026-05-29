# Skill Registry — Lilith ERP
_Generated: 2026-05-26_

## Project-Level Skills (`.opencode/skills/`)
> Project-level wins over user-level on name conflict.

| Skill | Description | Trigger Context |
|-------|-------------|-----------------|
| adding-backend-entity | 11-step workflow for new .NET 10 Clean Architecture entities | Creating domain objects, DB tables, CRUD endpoints, EF migrations |
| adding-frontend-entity | 10-step workflow for new Vue 3 entity CRUD | New views, DataTable listings, Pinia stores, Yup forms |
| backend-localization | Multilingual backend (ca/es/en) error messages and status strings | Adding error messages, StatusConstants, ILocalizationService |
| frontend-patterns | Vue 3 component patterns (PrimeVue 4, Pinia, TypeScript) | DataTable lists, detail views, dialogs, forms, dropdowns |
| git-commits | Conventional commit message generation | Writing commit messages |
| skill-creator | Guide for creating new skills | Creating or updating skills |

## User-Level Skills (`~/.config/opencode/skills/`)

| Skill | Description | Trigger Context |
|-------|-------------|-----------------|
| branch-pr | PR creation following issue-first enforcement | Creating pull requests |
| gentle-ai-chained-pr | Split large changes into chained/stacked PRs (≤400 lines) | PR exceeds 400 changed lines |
| cognitive-doc-design | Docs with progressive disclosure and low cognitive load | Writing guides, READMEs, RFCs, onboarding docs |
| comment-writer | Warm, direct human comments for PRs/issues/reviews | Drafting feedback, review comments |
| issue-creation | GitHub issue creation following issue-first enforcement | Reporting bugs, requesting features |
| judgment-day | Parallel adversarial dual-review protocol | "judgment day", "dual review", "juzgar" |
| work-unit-commits | Structure commits as deliverable work units | Implementing changes, preparing commits |
| skill-registry | Create/update the skill registry | "update skills", "skill registry" |

## SDD Skills (User-Level — SDD Workflow)

| Skill | Phase | When |
|-------|-------|------|
| sdd-init | Bootstrap | Project initialization |
| sdd-explore | Explore | Investigating ideas before committing |
| sdd-propose | Propose | Creating change proposals |
| sdd-spec | Specify | Writing requirements and scenarios |
| sdd-design | Design | Technical design documents |
| sdd-tasks | Plan | Breaking changes into task checklists |
| sdd-apply | Implement | Writing code from tasks |
| sdd-verify | Verify | Validating implementation vs specs |
| sdd-archive | Archive | Syncing delta specs and archiving changes |
| sdd-onboard | Onboard | End-to-end SDD walkthrough |

## Convention Files (Project Root)

| File | Role |
|------|------|
| `AGENTS.md` | Main agent guidelines — stack, patterns, anti-patterns, commands |
| `frontend/AGENTS.md` | Frontend-specific comprehensive reference |
| `backend/docs/architecture-layers.md` | Backend layer architecture deep-dive |
| `backend/docs/architectural-patterns.md` | Backend patterns reference |
| `backend/docs/domain-model.md` | Domain model documentation |
| `backend/docs/developer-guide.md` | Backend developer guide |
| `backend/docs/localization.md` | Localization system reference |
| `backend/docs/request-flow.md` | HTTP request flow documentation |
