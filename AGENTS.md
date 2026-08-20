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

## Styling contract

- Before editing component styles, read `docs/styling/component-token-contract.md`.
- Component styles only declare private `--_*` properties. Public `--dnet-*`
  tokens are declared only in token or theme layers and are read through `var()`.
- Run `npm run lint:css` and `npm run buildDnetBlazor` after styling changes.
