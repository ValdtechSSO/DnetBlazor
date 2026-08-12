using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Enums;
using Dnet.Blazor.Components.Grid.Infrastructure.Models;
using Dnet.Blazor.Infrastructure.Models.SearchModels;

namespace Dnet.Blazor.Components.Grid.BlgGrid;

public partial class BlgGrid<TItem>
{
    private async Task AddGroup(string columnName)
    {
        if (!_rowNodes.Any() || _treeRn == null) return;

        _activeGroups++;
        _searchModel.PaginationModel.CurrentPage = 1;

        await AuxAddGroup(columnName);

        if (GridOptions.GroupDefaultExpanded && _treeRn != null) 
            _treeRn = ExpandCollapseTreeRowNode(_treeRn, true);

        ReapplyActiveFiltersAfterGrouping();

        await Update();
    }

    private async Task AuxAddGroup(string dataField)
    {
        if (_groupByColumns.Contains(dataField)) return;

        _groupByColumns.Add(dataField);

        if (GridOptions.EnableServerSideGrouping)
        {
            await OnGroupingChanged.InvokeAsync(new GroupModel()
            {
                ColumnName = dataField,
                Operation = GroupOperation.Add
            });
        }
        else
        {
            var cellParams = new CellParams<TItem>
            {
                GridApi = _gridApi,
            };

            var lastId = _nextId;
            var gridColumn = FindGridColumn(dataField);
            if (gridColumn is null) return;

            _treeRn = GroupingService.AddGroupByColumn(_treeRn, gridColumn, _gridColumns, ref lastId, cellParams);
            _nextId = lastId;
        }
    }

    private async Task DeleteGroup(string columnName)
    {
        if (!_rowNodes.Any() || _treeRn == null) return;

        _activeGroups--;
        _searchModel.PaginationModel.CurrentPage = 1;

        await AuxDeleteGroup(columnName);

        ReapplyActiveFiltersAfterGrouping();

        await Update();
    }

    private async Task AuxDeleteGroup(string dataField)
    {
        _groupByColumns.Remove(dataField);

        if (GridOptions.EnableServerSideGrouping)
        {
            await OnGroupingChanged.InvokeAsync(new GroupModel()
            {
                ColumnName = dataField,
                Operation = GroupOperation.Delete
            });
        }
        else
        {
            var lastId = _nextId;

            var cellParams = new CellParams<TItem>
            {
                GridApi = _gridApi,
            };

            var gridColumn = FindGridColumn(dataField);
            if (gridColumn is null) return;

            _treeRn = GroupingService.RemoveGroupByColumn(_treeRn, gridColumn, _gridColumns, ref lastId, cellParams);
            _nextId = lastId;
        }
    }

    private void ReapplyActiveFiltersAfterGrouping()
    {
        // Grouping changes the shape of the tree, but it does not change the
        // visibility of any row when there are no filters. Avoid resetting
        // Show and AdvShow across the complete tree in that common path.
        // Server-side modes keep their callbacks because the remote source may
        // need to rebuild its result after the grouping query changes.
        if (GridOptions.EnableServerSideFilter || HasActiveSimpleFilters())
        {
            FilterBy();
        }

        if (GridOptions.EnableServerSideAdvancedFilter || HasActiveAdvancedFilters())
        {
            AdvancedFilterBy();
        }
    }

    private bool HasActiveSimpleFilters() => _gridColumns.Any(column =>
        column.CellDataType != CellDataType.None &&
        !string.IsNullOrWhiteSpace(column.Filter));

    private bool HasActiveAdvancedFilters() => _gridColumns.Any(column =>
        column.CellDataType != CellDataType.None &&
        (!string.IsNullOrWhiteSpace(column.AdvancedFilterModel?.Value) ||
         !string.IsNullOrWhiteSpace(column.AdvancedFilterModel?.AdditionalValue)));

    public async Task ExpandCollapse(bool isExpanded)
    {
        if (isExpanded)
        {
            await ExpandAll();
        }
        else
        {
            await CollapseAll();
        }
    }

    public async Task ExpandAll()
    {
        _treeRn = ExpandCollapseTreeRowNode(_treeRn, true);
        _isExpanded = true;

        if (_blgHeader != null) _blgHeader.ActiveRender();

        await Update();
    }

    public async Task CollapseAll()
    {
        _treeRn = ExpandCollapseTreeRowNode(_treeRn, false);
        _treeRn.Data.Expanded = true;
        _isExpanded = false;

        if (_blgHeader != null) _blgHeader.ActiveRender();

        _searchModel.PaginationModel.CurrentPage = 1;

        await Update();
    }

    private async Task ChangeExpanded(long id)
    {
        var index = _rowNodes.FindIndex(rowNode => rowNode.RowNodeId == id);
        if (index < 0) return;
        _rowNodes[index].Expanded = !_rowNodes[index].Expanded;
        await Update();
    }
}
