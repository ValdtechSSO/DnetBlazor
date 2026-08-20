# Known pitfalls

Every one of these cost at least one round-trip during the migration. Every one
passed `lint:css`, `buildDnetBlazor` and the debt counter without complaint.

**What they share: automated tooling cannot see appearance.** The linter checks
token contracts; the build checks syntax. Neither notices that an icon renders
wrong or that a `font-size` stopped varying.

---

## 1. C# appearance parameters with non-null defaults

An inline `style` beats any stylesheet. If an appearance-related `[Parameter]`
has a default value and the component writes it inline, **that component's tokens
do nothing** and dark theme is impossible.

```csharp
public string BackgroungColor { get; set; } = "#e0e0e0";  // ❌ inert token
public string? BackgroungColor { get; set; }              // ✔ only when the consumer asks
```

The default value belongs in the CSS fallback, not in C#.

**Check this before touching the CSS:**

```bash
grep -rnE 'public string\??\s+\w+\s*\{\s*get;\s*set;\s*\}\s*=\s*"(#[0-9a-fA-F]{3,8}|rgba?\(|white|black)' \
  src/Dnet.Blazor/Components/<X>
grep -rn "StyleBuilder" src/Dnet.Blazor/Components/<X>
```

**Runtime-computed geometry SHOULD be inline** (`top`, `left`, `width`, `z-index`
in `Overlay`, `SelectPanel`, `ImageEditor`). Nobody themes `top: 234px`. The
distinction is default-appearance versus measured-position.

## 2. Order within the file is semantic

Variant classes and base rules usually share specificity `(0,1,0)`, so **the last
one in the file wins**.

This bit `Chips`: the `.dnet-chip-size-*` classes sat before `.dnet-chip`, and the
`font-size` for three of the four sizes was dead. Valid CSS, green lint, clean
build, broken behaviour.

**Variants go at the end of the file, with a comment saying why.**

## 3. Never hand-transcribe a data-URI SVG

A 300-character path was rewritten by hand and ended up different between
`-webkit-mask-image` and `mask-image`. Since the unprefixed version is what modern
browsers apply, the icon rendered incorrectly.

Copy verbatim, and verify both declarations are character-for-character identical
before committing.

## 4. A "dead token" isn't always dead

The icon-size tokens in `Chips` looked unused. The `.razor` was injecting the
value through a different route (`style="background-size:@…"`). Deleting them cost
the icon scaling in three of the four chip sizes.

**Check the markup before deleting a token the linter flags as dead.**

## 5. A ghost token has no "implicit value"

When a `var()` with no fallback points at an undeclared token, the declaration
becomes *invalid at computed-value time* and the property falls back to the
**inherited** value if it's an inherited property, or the **initial** value if
not. There is no intended value stored anywhere.

Real example: `.dnet-dialog-content` had
`margin: 0 calc(var(--dnet-dialog-padding) * -1)` with the token undeclared, so the
margin resolved to `0` and the intended bleed effect never happened — for
several released versions.

For each ghost you must **decide explicitly** what was intended, and that usually
changes the appearance.

## 6. Changing a fallback changes the appearance

When you set a C# default to `null`, the CSS fallback takes over. If that fallback
doesn't reproduce the previous value **exactly**, you've changed the appearance
without noticing.

`Chips`: `#e0e0e0` inline versus `--dnet-sys-surface-hover` (#f2f2f2) in the CSS.
New semantic roles (`--dnet-sys-surface-variant`) had to be added so the migration
stayed visually neutral.

## 7. `default` and `hover` resolving to the same token

`Button` ended up with `--_background` and `--_hover` both resolving to
`--dnet-sys-state-hover`. Result: a button with no hover effect — in the very file
every other component was being migrated against.

It surfaced because its `default` and `hover` goldens were byte-identical. **Two
different states sharing a pixel is always suspicious**: either the state wasn't
applied when capturing, or the CSS doesn't distinguish them. Both need
investigating; neither should be waved through.

## 8. API declared and never wired up

Patterns seen here: a parameter passed by the service that the component never
adds to its classes; a declared `RenderFragment` that is never rendered; a CSS
class with its own tokens that no markup ever applies.

This is worse than not having the API: it promises a contract it doesn't keep, and
it gets documented. `tools/component-info.mjs` flags these in its WARNINGS.
