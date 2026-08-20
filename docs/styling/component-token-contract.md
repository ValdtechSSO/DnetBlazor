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
