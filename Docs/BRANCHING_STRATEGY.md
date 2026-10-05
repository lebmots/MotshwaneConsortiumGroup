# Git Branching Strategy

## Branches

| Branch | Purpose |
|---|---|
| `master` | Always builds and runs. This is what gets deployed. |
| `develop` | Integration branch. Everyone's finished work meets here first. |
| `feature/<area>-<short-description>` | One piece of work. Created from `develop`. |
| `fix/<short-description>` | A bug fix. Created from `develop`. |

`<area>` is one of `ui`, `db`, `api`, `backend`, `hosting`, `docs`.
Examples: `feature/backend-services-layer`, `feature/db-core-tables`, `feature/ui-customer-booking`.

## Workflow

1. `git checkout develop` then `git pull`.
2. `git checkout -b feature/backend-services-layer`.
3. Commit small and often.
4. Before opening a pull request, bring in the latest work: `git pull origin develop` while on your branch, fix any conflicts, and check the app still builds.
5. `git push -u origin feature/backend-services-layer`, then open a pull request **into `develop`**.
6. A teammate reviews and approves. Then merge and delete the feature branch.
7. At the end of each week, or before a demo, merge `develop` into `master` once the app builds and runs.

## Commit messages

Short, in the present tense, with a prefix:

- `feat: add booking service interface`
- `fix: reject empty status on job update`
- `docs: add architecture overview`
- `test: cover booking availability rules`
- `chore: update NuGet packages`

## Rules

- Never commit directly to `master`.
- Never force-push a branch that someone else is using.
- Never commit secrets: passwords, API keys, connection strings, `appsettings.Development.json`. Use `dotnet user-secrets` locally and Azure App Service configuration in production.
- Never commit `bin/`, `obj/` or `.vs/` (already in `.gitignore`).
- Keep pull requests small and about one thing.
- Tell the file's owner before changing another person's files, so you don't collide (see `ARCHITECTURE.md`).

## Repository settings (repo owner)

- Protect `master` and `develop`: require a pull request and at least one approval before merging.
- Require the build check to pass once the GitHub Actions workflow exists.
