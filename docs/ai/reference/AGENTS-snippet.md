# For your own AGENTS.md

After copying this folder into your project, paste something like this into the
file your agent reads at startup — `AGENTS.md`, `CLAUDE.md`, `.cursorrules`, or
whatever your tool uses.

Adjust the path to wherever you put the folder.

---

## Dnet.Blazor components

This project uses the Dnet.Blazor component library (6.0.0). Reference material
is in `docs/dnet-blazor/`.

- Before writing markup with any `Dnet*` component, read
  `docs/dnet-blazor/components/<Name>.md` for its parameters and styling tokens.
  `docs/dnet-blazor/component-index.md` lists them all.
- To change colours, spacing, radius or fonts, set CSS custom properties — never
  override library CSS or use `!important`. See `docs/dnet-blazor/theming.md`.
- Dialogs, tooltips, toasts and floating panels require
  `<DnetOverlay BaseZindex="1100" />` in `MainLayout.razor`, and do not inherit a
  theme unless wrapped in `<DnetThemeScope>`.
- If a parameter or token isn't listed in those files, don't invent one — a
  non-existent CSS token fails silently.
