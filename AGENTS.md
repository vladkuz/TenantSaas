# Repository Guidelines

## Project Structure & Module Organization
- `_bmad/core/` holds shared workflow engines, resources, and core agents.
- `_bmad/bmm/` contains the BMM module: agents, workflows, templates, and test architecture guidance.
- `_bmad/_config/` stores manifests and IDE/agent configuration files (YAML/CSV).
- Most assets are Markdown, YAML, or CSV templates used by workflows rather than executable source code.

## Build, Test, and Development Commands
- No build or runtime commands are defined in this repository.
- There is no automated test runner configured; validation is typically manual review of templates and workflows.

## Coding Style & Naming Conventions
- Use 2-space indentation for YAML files (see `_bmad/_config/manifest.yaml`).
- Keep Markdown concise with clear headings and bullet lists; prefer kebab-case for new file names (e.g., `workflow-status-template.yaml`).
- Update related manifests when adding or renaming workflow files (see `_bmad/_config/*.csv`).

## Testing Guidelines
- Test artifacts live under `_bmad/bmm/testarch/` as reference material; there is no executable test suite.
- When changing workflows or templates, validate by opening the files and checking formatting, links, and referenced paths.

## Commit & Pull Request Guidelines
- Commit message conventions are not defined (no Git history in this workspace).
- For PRs, include a short summary of the workflow/module impacted, list updated paths, and link any related issue or request.

## Agent-Specific Notes
- This repository is a template and workflow library; keep changes minimal and focused on documentation or workflow correctness.
- Prefer reusing existing templates and standards in `_bmad/bmm/data/` and `_bmad/bmm/workflows/` instead of introducing new formats.
