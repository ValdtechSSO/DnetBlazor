# Tree

Components: `<DnetTree>`, `<DnetTreeNode>`

## `<DnetTree>` — generic over TNode

| Parameter | Type | Default |
|---|---|---|
| `NodeContent` | `RenderFragment<TNode>?` | — |
| `OnTreeNodeClicked` | `EventCallback<TNode>` | — |
| `OnCheckboxClicked` | `EventCallback<TNode>` | — |
| `OnSelectionChange` | `EventCallback<List<TNode>>` | — |
| `Nodes` | `ICollection<TNode>?` | — |
| `DisplayValueConverter` | `Func<TNode, string>` | `value => value?.ToString() ?? string.Empty` |
| `ChildNodes` | `Func<TNode, List<TNode>?>?` | — |
| `ComponentType` | `Type` | — |
| `Parameters` | `IDictionary<string, object>` | — |
| `CheckboxSelection` | `bool` | — |

## `<DnetTreeNode>` — generic over TNode

| Parameter | Type | Default |
|---|---|---|
| `NodeContent` | `RenderFragment<TNode>?` | — |
| `OnTreeNodeParentToggle` | `EventCallback<bool>` | — |
| `OnTreeNodeClicked` | `EventCallback<TNode>` | — |
| `OnCheckboxClicked` | `EventCallback<TNode>` | — |
| `OnSelectionChange` | `EventCallback<List<TNode>>` | — |
| `Node` | `TreeNodeModel<TNode>` | `default!` |
| `ChildNodes` | `Func<TNode, List<TNode>?>?` | — |
| `Parameters` | `IDictionary<string, object>?` | — |
| `DisplayValueConverter` | `Func<TNode, string>` | `value => value?.ToString() ?? string.Empty` |
| `CheckboxSelection` | `bool` | — |

## Minimal usage

```razor
<DnetTree TTNode="..."
    Nodes="..."
    ChildNodes="..."
    ComponentType="..."
/>
```

## Styling tokens

Override these anywhere in the DOM above the component — `:root`, a
container, or the element's own `style`. Nothing else is needed.

| Token | Falls back to |
|---|---|
| `--dnet-tree-font-size` | — |
| `--dnet-tree-icon-width` | `25px` |

```css
:root { --dnet-tree-font-size: /* your value */; }
```
