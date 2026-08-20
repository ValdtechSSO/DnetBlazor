<!-- ## Conclave planning

- Before implementing a non-trivial feature, migration, or architectural change,
  run `conclave doctor` and resolve any failed readiness check.
- Generate a plan for the current repository with:
  `conclave plan --id "<unique-run-id>" --directory "$PWD" --prompt "<complete-user-request>" --snapshot working-tree --json`.
- Use a unique, descriptive run ID. Use `--prompt-file` instead of `--prompt`
  when the request is long or already stored in a file.
- Read the validated plan at the `planPath` returned in the JSON result.
- Implement every applicable phase of that plan; do not stop after plan
  generation unless the user requested planning only.
- Run the repository's required build, test, lint, and architecture checks after
  implementation.
- If Conclave fails, report the error instead of bypassing it silently.
- Do not use `--development` unless the user explicitly requests a
  single-provider development run. -->

## Working on this library

Before changing any component, run its fact sheet:

```bash
node tools/component-info.mjs <ComponentName>   # e.g. Tooltip
node tools/component-info.mjs --list            # every component
```

It reads the source live, so it is never stale. It reports the public token API,
the real fallback chain behind each private token, the CSS classes, the public
parameters, and a WARNINGS section listing the traps present in that specific
component.

Read the WARNINGS before writing anything. Each maps to a failure that has
already cost time in this repo.

- `docs/ai/library-development.md` — start here: the pattern, the workflow, the
  constraints
- `docs/ai/architecture.md` — the three token layers, rules R1–R10, themes, the
  overlay portal, legacy compatibility
- `docs/ai/pitfalls.md` — eight failures that already happened here, with causes
  and how to spot them

One component per PR. The linter cannot see appearance: always check the result
by eye in `samples/` before calling a change done.

## Styling contract

- Before editing component styles, read `docs/styling/component-token-contract.md`.
- Component styles only declare private `--_*` properties. Public `--dnet-*`
  tokens are declared only in token or theme layers and are read through `var()`.
- Run `npm run lint:css` and `npm run buildDnetBlazor` after styling changes.
