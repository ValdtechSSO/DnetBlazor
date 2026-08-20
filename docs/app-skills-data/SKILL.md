---
name: dnet-blazor
description: Build Blazor apps with the Dnet.Blazor component library (NuGet package Dnet.Blazor, .NET 10) — how to install it, which parameters each component takes, and how to restyle or theme it. Covers DnetButton, DnetChip, DnetSelect, DnetAutocomplete, DnetDialog, DnetList, PickList, DnetDatePicker, DnetGrid, DnetTooltip, DnetToast, DnetTree, DnetStepper, DnetTabs, DnetOverlay and the rest. Use this skill WHENEVER a task involves Dnet.Blazor — writing markup with any Dnet* component, wiring up dialogs or tooltips, changing colours, spacing, corner radius or fonts, building a dark mode or custom theme, or debugging why a component looks wrong or fails to appear. Use it too whenever --dnet-sys-, --dnet-ref-, DnetThemeScope, DnetOverlay, dnet-blazor-styles.css or AddDnetBlazor() show up, even if the library is never named.
---

# Dnet.Blazor

A Blazor component library for .NET 10. Everything visual resolves through CSS
custom properties, so restyling means **setting tokens, never overriding library
CSS**. No `!important`, no `::deep`, no specificity fights.

This skill bundles the API and token reference for version 6.0.0 — you don't need
the library's source to use it.

## Start here

**Working with a specific component?** Open
`references/components/<Name>.md`. Each one lists the component's tags, every
public parameter with its type and default, and every styling token with what it
currently falls back to. `references/component-index.md` maps all 29 of them.

**Setting the project up, or upgrading from 5.x?** → `references/setup.md`

**Changing how things look?** → `references/theming.md`

Read only what the task needs. The component files are independent.

## The one idea worth knowing up front

Every component reads its appearance from tokens, with a fallback chain:

```css
--_radius: var(--dnet-tooltip-border-radius, var(--dnet-sys-radius-lg));
```

So you override at whatever scope you want, and inheritance does the rest:

```css
:root            { --dnet-tooltip-border-radius: 0; }   /* whole app     */
.compact-area    { --dnet-tooltip-border-radius: 4px; } /* one region    */
```
```razor
<DnetButton style="--dnet-btn-background: crimson;">Delete</DnetButton>  @* one instance *@
```

Two layers are yours to write: `--dnet-<component>-*` for a single component, and
`--dnet-sys-*` for the whole library at once. The full semantic list with current
values is in `references/design-tokens.css`.

## Two things that trip people up

**Floating components need a host.** Dialogs, tooltips, toasts and panels render
into one global `<DnetOverlay BaseZindex="1100" />` that belongs in
`MainLayout.razor`. Without it they silently render nothing — this is the most
common setup mistake.

**Themes don't reach inside overlays by default.** Overlays render outside the DOM
subtree where you opened them, so inheritance doesn't carry your theme in. Wrap
the region in `<DnetThemeScope Theme="dark">`, which carries the theme name
through to the overlay host. Details in `references/theming.md`.

## Answering questions about a component

Read its reference file rather than guessing from the component's name. Parameter
names in this library are not always what you'd predict, and several components
expose both a simple and a generic form. When a parameter or token isn't in the
file, say so rather than inventing one — a plausible-looking token that doesn't
exist fails silently, which is much worse than an error.

## Reference files

- `references/component-index.md` — all components, their tags, token counts
- `references/components/<Name>.md` — one per component: parameters and tokens
- `references/setup.md` — install, services, assets, overlay host, 5.x → 6.0
- `references/theming.md` — override scopes, writing a theme, dark mode, overlays
- `references/design-tokens.css` — every semantic token with its current value
- `references/theme-dark-example.css` — a complete real theme, as a template

Runnable examples for every component live in the repository's `samples/` folder,
and a live demo at datalnet.com.
