using Dnet.Blazor.Components.Grid.Infrastructure.Entities;

namespace Dnet.Blazor.Components.Grid.BlgGrid;

public partial class BlgGrid<TItem>
{
    private TreeRowNode<TItem> BuildRowNodes()
    {
        var treeRn = InitializeTree();
        var usedStableKeys = new HashSet<object>();

        foreach (var data in _gridData)
        {
            var stableKey = GridOptions.RowKeySelector?.Invoke(data);
            if (stableKey is not null && !usedStableKeys.Add(stableKey))
            {
                // A stable key must be unique among siblings. Falling back to
                // RowNodeId keeps rendering safe for duplicated source items.
                stableKey = null;
            }

            var rowNodeId = GetId();

            treeRn.Children.Add(new TreeRowNode<TItem>
            {
                Data = new RowNode<TItem>
                {
                    RowNodeId = rowNodeId,
                    StableKey = stableKey,
                    RenderKey = stableKey is null
                        ? new InternalRowRenderKey(rowNodeId)
                        : new StableRowRenderKey(stableKey),
                    RowData = data,
                    Show = true,
                    Expanded = true,
                    AdvShow = true
                },
                Children = new List<TreeRowNode<TItem>>(),
                ColumnName = ""
            });
        }

        return treeRn;
    }

    private readonly record struct InternalRowRenderKey(long Value);

    private readonly record struct StableRowRenderKey(object Value);

    private List<RowNode<TItem>> FlattenTree(TreeRowNode<TItem> tree, int level)
    {
        var rowNodeList = new List<RowNode<TItem>>();

        if (tree.Data.IsGroup)
        {
            tree.Data.GroupValue = tree.Value;
            tree.Data.Level = level;
        }

        rowNodeList.Add(tree.Data);

        if (!tree.Data.Expanded) return rowNodeList;

        foreach (var child in tree.Children)
        {
            rowNodeList.AddRange(FlattenTree(child, level + 1));
        }

        return rowNodeList;
    }

    private long GetId()
    {
        _nextId++;
        return _nextId;
    }

    private void RebuildRowIndexes()
    {
        _rowNodesById.Clear();
        _rowNodesByKey.Clear();

        foreach (var rowNode in _rowNodes)
        {
            _rowNodesById[rowNode.RowNodeId] = rowNode;
            if (!rowNode.IsGroup && rowNode.StableKey is not null)
            {
                _rowNodesByKey[rowNode.StableKey] = rowNode;
                rowNode.SelectThisNode(_selectedRowKeys.Contains(rowNode.StableKey));
            }
        }
    }

    private RowNode<TItem>? FindRowNode(long rowNodeId) =>
        _rowNodesById.GetValueOrDefault(rowNodeId);

    private void CaptureSelectionKeys()
    {
        foreach (var rowNode in GetAllRowNodes())
        {
            if (!rowNode.IsGroup && rowNode.StableKey is not null)
            {
                if (rowNode.IsSelected())
                {
                    _selectedRowKeys.Add(rowNode.StableKey);
                }
                else
                {
                    _selectedRowKeys.Remove(rowNode.StableKey);
                }
            }
        }
    }

    private IEnumerable<RowNode<TItem>> GetAllRowNodes()
    {
        if (_treeRn is null)
        {
            return _rowNodes;
        }

        return Traverse(_treeRn);

        static IEnumerable<RowNode<TItem>> Traverse(TreeRowNode<TItem> tree)
        {
            if (tree.Data is not null)
            {
                yield return tree.Data;
            }

            if (tree.Children is null)
            {
                yield break;
            }

            foreach (var child in tree.Children)
            {
                foreach (var rowNode in Traverse(child))
                {
                    yield return rowNode;
                }
            }
        }
    }

    private TreeRowNode<TItem> SelectTreeRowNode(TreeRowNode<TItem> tree, bool select)
    {
        tree.Data.SelectThisNode(select);

        foreach (var subtree in tree.Children)
            SelectTreeRowNode(subtree, select);

        return tree;
    }

    private TreeRowNode<TItem> ExpandCollapseTreeRowNode(TreeRowNode<TItem> tree, bool expandCollapse)
    {
        tree.Data.Expanded = expandCollapse;

        foreach (var subtree in tree.Children)
            ExpandCollapseTreeRowNode(subtree, expandCollapse);

        return tree;
    }

    private void AuxChangeSelectAllNodes(TreeRowNode<TItem> tree, bool value)
    {
        var disabledRow = false;

        if (tree.Data.RowData != null)
        {
            disabledRow = GridOptions.DisableRow != null && GridOptions?.DisableRow(tree.Data.RowData) == true ? true : false;
        }

        tree.Data.SelectThisNode(value && disabledRow == false);

        foreach (var subtree in tree.Children)
            AuxChangeSelectAllNodes(subtree, value);
    }
}
