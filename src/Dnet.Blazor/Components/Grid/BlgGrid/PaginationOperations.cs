namespace Dnet.Blazor.Components.Grid.BlgGrid;

public partial class BlgGrid<TItem>
{
    private async Task PaginationChanged(int currentPage)
    {
        _searchModel.PaginationModel.CurrentPage = currentPage;

        if (GridOptions.EnableServerSidePagination)
        {
            await OnPaginationChanged.InvokeAsync(_searchModel);
        }
        else
            await AssignRangeRenderedRows();

        _blgCenter?.ActiveRender();
        if (_pinnedLeft) _blgPinnedLeft?.ActiveRender();
        if (_pinnedRight) _blgPinnedRight?.ActiveRender();
    }

    private Task GoToFirstPage(int currentPage)
    {
        return PaginationChanged(currentPage);
    }

    private Task GoToPreviousPage(int currentPage)
    {
        return PaginationChanged(currentPage);
    }

    private Task GoToNextPage(int currentPage)
    {
        return PaginationChanged(currentPage);
    }

    private Task GoToLastPage(int currentPage)
    {
        return PaginationChanged(currentPage);
    }

    private Task GoToSpecificPage(int specificPage)
    {
        return PaginationChanged(specificPage);
    }

    private async Task AssignRangeRenderedRows()
    {
        if (GridOptions.Pagination)
        {
            var currentPage = _searchModel.PaginationModel.CurrentPage;
            var pageSize = _searchModel.PaginationModel.PageSize;
            var itemsCount = _searchModel.PaginationModel.ItemsCount;

            if (currentPage <= 0) currentPage = 1;

            var (startIndex, count) = PaginatorService.GetRangePage(currentPage, pageSize, itemsCount);

            _renderedRowNodes = _rowNodes.GetRange(startIndex, count);
        }
        else
        {
            _renderedRowNodes = _rowNodes;
        }

        ManageGridCellSpanning();

        _gridApi.RenderedRowNodes = _renderedRowNodes;

        if (GridOptions.UseVirtualization)
        {
            if (!_firstRender) await DefaultVirtualization();
        }
        else
        {
            _itemsToShow = _renderedRowNodes;
            StateHasChanged();
        }
    }
}
