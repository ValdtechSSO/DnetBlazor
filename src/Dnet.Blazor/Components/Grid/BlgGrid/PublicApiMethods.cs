using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Enums;
using Microsoft.JSInterop;

namespace Dnet.Blazor.Components.Grid.BlgGrid;

public partial class BlgGrid<TItem>
{
    public async Task SetRowDataAsync(List<TItem> gridData, int? totalItems = null, int? currentPage = null)
    {
        CaptureSelectionKeys();
        _hasResolvedInitialVirtualWindow = false;
        _nextId = -1;
        _gridData = gridData;
        _treeRn = BuildRowNodes();
        _rowNodes = FlattenTree(_treeRn, 0).FindAll(e => e.Show);
        _sourceItemCount = _rowNodes.Count(rowNode => !rowNode.IsGroup && rowNode.RowData is not null);
        RebuildRowIndexes();

        if (GridOptions.EnableServerSidePagination && totalItems != null) 
            _searchModel.PaginationModel.ItemsCount = (int)totalItems;

        if (GridOptions.EnableServerSidePagination && currentPage != null) 
            _searchModel.PaginationModel.CurrentPage = (int)currentPage;

        _searchModel.PaginationModel.PageSize = GridOptions.PaginationPageSize;

        if (_gridData.Any()) OperationsByDefault();

        if (_blgHeader != null) _blgHeader.ActiveRender();

        if (_blgCenter is not null) _blgCenter.SelectallNotify(false);

        await Update();
    }

    public async Task SetColumnDefsAsync(List<GridColumn<TItem>> gridColumns)
    {
        ArgumentNullException.ThrowIfNull(gridColumns);

        if (gridColumns.Any())
            _gridColumns = AdvancedFilteringService.InitAdvancedFilterModels(gridColumns, GridOptions.DefaultAdvancedFilterOperator);

        _pinnedRight = _gridColumns.Where(e => e.Pinned == Pinned.Right && !e.Hide).Any();
        _pinnedLeft = _gridColumns.Where(e => e.Pinned == Pinned.Left && !e.Hide).Any();
        RebuildLayoutSnapshot();

        if (_blgHeader != null) _blgHeader.ActiveRender();

        await InitializeGrid();

        StateHasChanged();
    }

    /// <summary>
    /// Rebuilds the row tree after an in-place mutation of <see cref="GridData"/>.
    /// Prefer supplying a new collection reference when practical.
    /// </summary>
    public async Task RefreshDataAsync()
    {
        CaptureSelectionKeys();
        _hasResolvedInitialVirtualWindow = false;
        _nextId = -1;
        _gridData = GridData;
        InitializeGridWorkingData();
        await InitializeGrid();
    }

    /// <summary>
    /// Reprocesses column definitions after an in-place change.
    /// </summary>
    public Task RefreshColumnsAsync() => SetColumnDefsAsync(GridColumns);

    /// <summary>
    /// Applies a changed layout, including pinned state and row height.
    /// </summary>
    public async Task RefreshLayoutAsync()
    {
        _itemSize = GridOptions.RowHeight;
        _pinnedRight = _gridColumns.Any(e => e.Pinned == Pinned.Right && !e.Hide);
        _pinnedLeft = _gridColumns.Any(e => e.Pinned == Pinned.Left && !e.Hide);
        RebuildLayoutSnapshot();
        await Update();
    }

    private async Task RetryAsync()
    {
        _refreshException = null;

        if (GridOptions.UseVirtualization)
        {
            await UpdateItemDistributionAsync(_itemsBefore, _visibleItemCapacity, true);
            return;
        }

        await RefreshDataAsync();
    }

    private void ManageGridCellSpanning()
    {
        for (var i = 0; i < _renderedRowNodes.Count; ++i)
        {
            _renderedRowNodes[i].RowSpanSkippedCells = new Dictionary<GridColumn<TItem>, bool>();
            _renderedRowNodes[i].RowSpanTargetCells = new Dictionary<GridColumn<TItem>, uint>();
            _renderedRowNodes[i].FirstSpanRow = new Dictionary<GridColumn<TItem>, uint>();
        }

        var cellParams = new CellParams<TItem>
        {
            GridApi = _gridApi,
        };

        for (var i = 0; i < _renderedRowNodes.Count; ++i)
        {
            var rowNode = _renderedRowNodes[i];

            cellParams.RowNode = rowNode;
            cellParams.RowData = rowNode.RowData;

            if (rowNode.IsGroup) continue;

            foreach (var cell in _gridColumns)
            {
                cellParams.GridColumn = cell;

                var rowSpanFn = cell.RowSpanFn ?? (_ => 1);

                var skip = rowSpanFn(cellParams) - 1;

                if (rowNode.RowSpanSkippedCells.TryGetValue(cell, out bool skipped) || skip <= 0) continue;

                int j;
                for (j = 1; j <= skip && (i + j < _renderedRowNodes.Count) && !_renderedRowNodes[i + j].IsGroup; ++j)
                {
                    _renderedRowNodes[i + j].RowSpanSkippedCells.Add(cell, true);
                }

                for (var k = 1; k < j; ++k)
                {
                    _renderedRowNodes[i + k].FirstSpanRow.Add(cell, (uint)(j - k));
                }

                rowNode.RowSpanTargetCells.Add(cell, (uint)j);
            }
        }
    }

    private async Task Update()
    {
        if (_treeRn != null)
        {
            _treeRn.Data.Show = false;

            _rowNodes = FlattenTree(_treeRn, 0).FindAll(e => e.AdvShow && e.Show);
            RebuildRowIndexes();

            var numberOfRows = !GridOptions.EnableServerSidePagination ? _rowNodes.Count : _searchModel.PaginationModel.ItemsCount;

            _paginator?.UpdatePaginatorState(_searchModel.PaginationModel.CurrentPage, numberOfRows);

            _blgCenter?.ActiveRender();

            if (_pinnedLeft) _blgPinnedLeft?.ActiveRender();

            if (_pinnedRight) _blgPinnedRight?.ActiveRender();

            if (_blgHeader != null) _blgHeader?.ActiveRender();

            if (GridOptions.EnableServerSidePagination)
            {
                _renderedRowNodes = _rowNodes;

                ManageGridCellSpanning();

                if (GridOptions.UseVirtualization)
                {
                    SeedInitialVirtualWindow();
                    if (!_firstRender) await DefaultVirtualization();
                }
                else
                {
                    _itemsToShow = _renderedRowNodes;
                    StateHasChanged();
                }
            }
            else
            {
                _searchModel.PaginationModel.ItemsCount = numberOfRows;

                await AssignRangeRenderedRows();
            }
        }
    }

    private async Task OnScroll()
    {
        var result = await BlGridInterop.GetElementScrollLeft(_eBodyHorizontalScrollViewport);

        _transformX = $"-{Math.Floor(result)}px";

        StateHasChanged();
    }

    private void HeaderWithChange(int headerWidth)
    {
        eBodyHorizontalScrollContainerWidth = headerWidth;
    }

    private string GetBlgViewPortHeight()
    {
        var height = GridOptions.HeaderHeight;

        if (!GridOptions.SuppressFilterRow)
        {
            height = height + 55;
        }
        else
        {
            height = height + 15;
        }

        return $"calc(100% - {height}px)";
    }

    public async ValueTask DisposeAsync()
    {
        _refreshCts?.Cancel();

        try
        {
            await BlGridInterop.RemoveEventListeners(_eGridContainer);
            await BlGridInterop.RemoveEventListeners(_eCenterContainer);
        }
        catch (JSDisconnectedException)
        {
        }

        _windowEventReference?.Dispose();
        _touchEventReference?.Dispose();

        if (_jsInterop != null)
        {
            await _jsInterop.DisposeAsync();
        }
    }
}
