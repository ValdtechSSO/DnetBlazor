# DnetBlazor styling

DnetBlazor uses three runtime CSS custom-property layers:

```
--dnet-ref-* -> --dnet-sys-* -> --dnet-<component>-* -> --_<private>-*
```

`reference` contains raw palette values. `system` provides semantic roles and is
the only layer a theme changes. Component tokens are optional public overrides;
components resolve them through private properties so a value inherited from any
ancestor, including a single component instance, always wins.

Load `_content/Dnet.Blazor/dnet-blazor-styles.css` once. Theme files are static
web assets under `_content/Dnet.Blazor/styles/theme/` and are loaded after the
base bundle.

See [component-token-contract.md](component-token-contract.md) and
[theming-guide.md](theming-guide.md).