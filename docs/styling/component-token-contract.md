# Component token contract

Every component reads its public token with a local private indirection:

```css
.dnet-button {
    --_background: var(--dnet-btn-background, var(--dnet-sys-transparent));
    background: var(--_background);
}
```

Public tokens are never declared in a component stylesheet. The fallback belongs
inside `var(...)`, and variants only redefine `--_*` properties. This preserves
overrides from `:root`, a parent scope, and an individual `style` attribute.

Use `--dnet-<component>-<property>[-<state>]` names, such as
`--dnet-btn-background-hover`. Expose a token only for values consumers should
reasonably customize. Structural constants and internal layout details remain
private.

Component styles consume `--dnet-sys-*`, their own `--dnet-<component>-*`
tokens, and explicitly documented legacy names within a fallback chain. They do
not consume `--dnet-ref-*`, declare `:root`, add `!important`, or contain colors.

## Containers and materials

A component that paints a container reads its fill from a material, and the
material mechanics directly from the system layer. No component token is added
for them:

```css
.dnet-dialog-container {
    --_background: var(--dnet-dialog-background, var(--dnet-sys-material-popup));
    --_elevation: var(--dnet-dialog-elevation, var(--dnet-sys-elevation-3));

    background: var(--dnet-sys-material-sheen), var(--_background);
    box-shadow: var(--dnet-sys-material-highlight), var(--_elevation);
    backdrop-filter: var(--dnet-sys-material-backdrop);
}
```

1. **One material layer per surface.** Apply it on the outermost element that
   paints the background. A child that repainted the same colour becomes
   transparent, which is identical by default.
2. **Background and shadow on different elements.** Fill, sheen and backdrop go
   on the element that paints the background; the highlight on the one that
   carries the shadow and the radius.
3. **`none` in a shadow list.** When a consumer sets a shadow token to `none`,
   `box-shadow: <highlight>, none` is invalid at computed-value time and falls
   back to `none`: the rim disappears with the shadow. This is expected.

Keep the new declarations in the rule that already paints the container, so the
order of rules in the file does not change. Use `--dnet-sys-surface*` only for
cut-out colours, never for a container fill.
