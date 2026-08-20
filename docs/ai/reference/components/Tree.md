# Tree

## `<DnetTree>` — generic over TNode

```razor
<DnetTree TNode="..."
    ComponentType="..."
    Parameters="..."
    CheckboxSelection="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `NodeContent` | `RenderFragment<TNode>?` | — | Gets or sets the template used to render a tree node. |
| `OnTreeNodeClicked` | `EventCallback<TNode>` | — | Raised when tree node clicked occurs. |
| `OnCheckboxClicked` | `EventCallback<TNode>` | — | Raised when checkbox clicked occurs. |
| `OnSelectionChange` | `EventCallback<List<TNode>>` | — | Raised when the selection changes. |
| `Nodes` | `ICollection<TNode>?` | — | Gets or sets the nodes used by this component. |
| `DisplayValueConverter` | `Func<TNode, string>` | `value => value?.ToString() ?? string.Empty` | Gets or sets the function that converts an item to display text. |
| `ChildNodes` | `Func<TNode, List<TNode>?>?` | — | Gets or sets the child nodes used by this component. |
| `ComponentType` | `Type` | — | Gets or sets the component type rendered dynamically. |
| `Parameters` | `IDictionary<string, object>` | — | Gets or sets parameters passed to the dynamically rendered component. |
| `CheckboxSelection` | `bool` | — | Gets or sets the checkbox selection used by this component. |

## `<DnetTreeNode>` — generic over TNode

```razor
<DnetTreeNode TNode="..."
    CheckboxSelection="..."
/>
```

| Parameter | Type | Default | Description |
|---|---|---|---|
| `NodeContent` | `RenderFragment<TNode>?` | — | Gets or sets the template used to render a tree node. |
| `OnTreeNodeParentToggle` | `EventCallback<bool>` | — | Raised when tree node parent toggle occurs. |
| `OnTreeNodeClicked` | `EventCallback<TNode>` | — | Raised when tree node clicked occurs. |
| `OnCheckboxClicked` | `EventCallback<TNode>` | — | Raised when checkbox clicked occurs. |
| `OnSelectionChange` | `EventCallback<List<TNode>>` | — | Raised when the selection changes. |
| `Node` | `TreeNodeModel<TNode>` | `default!` | Gets or sets the node used by this component. |
| `ChildNodes` | `Func<TNode, List<TNode>?>?` | — | Gets or sets the child nodes used by this component. |
| `Parameters` | `IDictionary<string, object>?` | — | Gets or sets parameters passed to the dynamically rendered component. |
| `DisplayValueConverter` | `Func<TNode, string>` | `value => value?.ToString() ?? string.Empty` | Gets or sets the function that converts an item to display text. |
| `CheckboxSelection` | `bool` | — | Gets or sets the checkbox selection used by this component. |

## Styling tokens

Set any of these above the component in the DOM — `:root`, a container, or
the element's own `style`. Nothing else is needed.

| Token | Effective default |
|---|---|
| `--dnet-tree-font-size` | `0.875rem` <br><sub>via `--dnet-sys-text-md`</sub> |
| `--dnet-tree-icon-width` | `25px` |

```css
:root { --dnet-tree-font-size: /* your value */; }
```
