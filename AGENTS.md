# AGENTS.md

Instructions for AI coding agents working in this repository. Humans should read them too.

## Project

ClassBook (working name) lets an organization publish courses and classes and lets families enroll in them; payments are settled by importing bank statements. See [README.md](README.md) for features and tech stack.

- Backend: ASP.NET Core Web API (.NET 10), SQL Server, Entity Framework Core
- Frontend: React + TypeScript, Tailwind CSS, shadcn/ui
- CI: GitHub Actions
- UI language is Polish, built for internationalization. A single organization at start; the architecture must be ready for multi-tenancy.

## Rules

1. **English only.** All repository content – code, comments, docs, specs, issues, PRs, commit messages – is in English. (The user-facing UI text is Polish via localization resources.)
2. **Feature branches.** Work on a feature branch, never directly on `main`. If on `main`, create a branch first.
3. **No commit, push or merge without explicit human approval.** Ask first, every time. Approval for one commit does not carry over to the next.

## The `.ai` folder

All agent-related working material lives in `.ai/`:

```
.ai/
├── specs/                 Specifications for spec-driven development
│   ├── YYYY-MM-DD-slug.md   Active specs (to be implemented / in progress)
│   ├── implemented/         Specs whose implementation is done
│   └── archive/             Obsolete, rejected or superseded specs
├── runs/                  Logs of autonomous AI runs
│   └── YYYY-MM-DD-slug.md
├── lessons.md             Index of lessons learned (long-term memory)
└── lessons/               Detailed lesson notes
    └── YYYY-MM-DD-slug.md
```

File names are `YYYY-MM-DD-slug.md` (date of creation, lowercase kebab-case slug).

### Specs (`.ai/specs/`)

- Development is spec-driven: for any non-trivial feature or change, there must be a spec before implementation. If none exists, write one and get human confirmation first.
- A spec states the goal, scope and non-goals, behavior/requirements, acceptance criteria and open questions.
- Implement against the spec. If reality diverges, update the spec in the same change.
- When implementation is complete, move the spec to `.ai/specs/implemented/`. Move obsolete or rejected specs to `.ai/specs/archive/`. Do not delete specs.

### Runs (`.ai/runs/`)

- Every autonomous run (work done without step-by-step human guidance) must produce a log file.
- Log: the task and linked spec, branch, decisions and assumptions, steps taken, files changed, commands/tests run with results, problems, and open items for the human.
- Create the log at the start of the run and keep it updated; do not reconstruct it afterwards.

### Lessons (`.ai/lessons.md` and `.ai/lessons/`)

- **Before starting work, read `.ai/lessons.md`** and any linked lessons relevant to the task.
- After fixing a bug, hitting a pitfall, or finding an improvement (code, process, tooling, docs), record it: add a note in `.ai/lessons/YYYY-MM-DD-slug.md` and a one-line entry in `.ai/lessons.md`. See that file for the format.
- Keep lessons concrete and actionable. Update or remove lessons that turn out to be wrong.

## Enforcement checklist

Before finishing any task, confirm:

- [ ] A spec exists in `.ai/specs/` for the work (or the change is trivial) and is moved to `implemented/` if complete.
- [ ] An autonomous run has a log in `.ai/runs/`.
- [ ] New lessons are recorded in `.ai/lessons/` and indexed in `.ai/lessons.md`.
- [ ] All content is in English and file names follow `YYYY-MM-DD-slug.md`.
- [ ] Work is on a feature branch, and nothing was committed, pushed or merged without explicit approval.
