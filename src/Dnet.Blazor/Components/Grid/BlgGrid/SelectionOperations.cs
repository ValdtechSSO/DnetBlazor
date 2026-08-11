using Dnet.Blazor.Components.Grid.Infrastructure.Entities;

namespace Dnet.Blazor.Components.Grid.BlgGrid;

public partial class BlgGrid<TItem>
{
    private async Task CellClicked(CellClikedEventData cellClikedEventData)
    {
        _blgCenter?.ActiveRender();

        if (_pinnedLeft) _blgPinnedLeft?.ActiveRender();
        if (_pinnedRight) _blgPinnedRight?.ActiveRender();

        var gridColumn = FindGridColumn(cellClikedEventData.CellId);
        var rownode = FindRowNode(cellClikedEventData.RowNodeId);
        if (gridColumn is null || rownode is null) return;

        var cellClikedData = new CellClikedData<TItem>();
        cellClikedData.ColumnId = gridColumn.ColumnId;
        cellClikedData.ColumnOrder = gridColumn.ColumnOrder;
        cellClikedData.HeaderName = gridColumn.HeaderName;
        cellClikedData.DataField = gridColumn.DataField;
        cellClikedData.AdvancedFilterModel = gridColumn.AdvancedFilterModel;
        cellClikedData.RowNode = rownode;

        await OnCellClicked.InvokeAsync(cellClikedData);
    }

    private async Task RowClicked(long rowNodeId)
    {
        var rowNode = FindRowNode(rowNodeId);
        if (rowNode is null) return;

        _blgCenter?.ActiveRender();
        if (_pinnedLeft) _blgPinnedLeft?.ActiveRender();
        if (_pinnedRight) _blgPinnedRight?.ActiveRender();

        await OnRowClicked.InvokeAsync(rowNode);
    }

    private async Task RowDoubleClicked(long rowNodeId)
    {
        var rowNode = FindRowNode(rowNodeId);
        if (rowNode is null) return;
        await OnRowDoubleClicked.InvokeAsync(rowNode);
    }

    private async Task SelectionChanged(List<long> rowNodeIds)
    {
        _blgCenter?.ActiveRender();
        if (_pinnedLeft) _blgPinnedLeft?.ActiveRender();
        if (_pinnedRight) _blgPinnedRight?.ActiveRender();

        CaptureSelectionKeys();
        var rowNodes = _rowNodes.Where(p => p.RowData is not null && p.IsSelected()).ToList();

        await OnSelectionChanged.InvokeAsync(rowNodes);
    }

    private async Task ChangeSelectAllNodes(bool value)
    {
        if (!_rowNodes.Any() || _treeRn == null) return;

        AuxChangeSelectAllNodes(_treeRn, value);
        CaptureSelectionKeys();

        var selectedRowNodes = new List<RowNode<TItem>>();

        if (value)
        {
            var flattenTree = FlattenTree(_treeRn, 0);
            selectedRowNodes = _rowNodes = flattenTree.Where(e => (e.IsSelected() && e.RowData != null && !e.IsGroup) || (e.IsSelected() && e.IsGroup)).Select(p => p).ToList();
        }
        else
        {
            _rowNodes = FlattenTree(_treeRn, 0).Where(e => (!e.IsSelected() && e.RowData != null && !e.IsGroup) || (!e.IsSelected() && e.IsGroup)).Select(p => p).ToList();
        }

        _blgCenter?.ActiveRender();
        if (_pinnedLeft) _blgPinnedLeft?.ActiveRender();
        if (_pinnedRight) _blgPinnedRight?.ActiveRender();

        var rowNodes = selectedRowNodes.Where(p => p.RowData is not null).ToList();

        if (_blgCenter is not null) _blgCenter.SelectallNotify(value);

        await OnSelectionChanged.InvokeAsync(rowNodes);
    }

    public async Task SelectAll()
    {
        _treeRn = SelectTreeRowNode(_treeRn, true);
        CaptureSelectionKeys();
        await Update();
    }

    public async Task DeselectAll()
    {
        _treeRn = SelectTreeRowNode(_treeRn, false);
        CaptureSelectionKeys();
        await Update();
    }

    /// <summary>
    /// Gets the selected nodes currently visible in the Grid.
    /// </summary>
    public List<RowNode<TItem>> GetSelectedNodes() => GetSelectedNodes(visibleOnly: true);

    /// <summary>
    /// Gets selected nodes. Set <paramref name="visibleOnly"/> to false to
    /// include selections retained while a row is filtered or collapsed.
    /// </summary>
    public List<RowNode<TItem>> GetSelectedNodes(bool visibleOnly)
    {
        return (visibleOnly ? _rowNodes : GetAllRowNodes())
            .Where(rowNode => rowNode.IsSelected())
            .ToList();
    }
}
