# Layout dividers

`DesktopLayout` uses one shared divider for its header, footer, and lateral columns. It is disabled by default.

To enable the structural dividers in a layout, set the shared width on the layout or on any ancestor:

```razor
<DesktopLayout style="--dnet-layout-divider-width: var(--dnet-sys-border-width);">
    ...
</DesktopLayout>
```

The visual can also be customized without targeting internal layout selectors:

```css
.application-shell {
    --dnet-layout-divider-width: 1px;
    --dnet-layout-divider-style: dashed;
    --dnet-layout-divider-color: var(--dnet-sys-border);
}
```

Existing `--dnet-header-border-color`, `--dnet-footer-border-color`, `--dnet-left-column-border-color`, and `--dnet-right-column-border-color` overrides remain supported and take precedence for their own edge.
