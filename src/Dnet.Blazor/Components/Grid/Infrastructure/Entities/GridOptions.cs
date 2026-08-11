using Dnet.Blazor.Components.Grid.Infrastructure.Enums;
using Dnet.Blazor.Infrastructure.Models.SearchModels.FilterModels;
using Microsoft.AspNetCore.Components;

namespace Dnet.Blazor.Components.Grid.Infrastructure.Entities
{
    public class GridOptions<TItem>
    {
        public bool IsDebugMode { get; set; }

        public int HeaderHeight { get; set; } = 40;

        public int HeaderRowHeight { get; set; } = 40;

        public int RowHeight { get; set; } = 40;

        public List<string> RowStyle { get; set; } = new();

        public Func<CellParams<TItem>, List<string>>? RowStyleFn { get; set; }

        public List<string> RowClasses { get; set; } = new();

        public Func<CellParams<TItem>, List<string>>? RowClassFn { get; set; }

        public string? GridClass { get; set; }

        public bool EnableGrouping { get; set; }

        public bool EnableAdvancedFilter { get; set; }

        public FilterOperator DefaultAdvancedFilterOperator { get; set; } = FilterOperator.Contains;

        public bool EnableColResize { get; set; } = false;

        public bool EnableServerSideGrouping { get; set; } = false;

        public bool EnableSorting { get; set; } = false;

        public bool EnableServerSideSorting { get; set; } = false;

        public bool EnableFilter { get; set; } = false;

        public bool EnableServerSideFilter { get; set; } = false;

        public bool EnableServerSideAdvancedFilter { get; set; } = false;

        public bool EnableServerSidePagination { get; set; } = false;

        public bool Pagination { get; set; } = true;

        public bool SuppressPaginationPanel { get; set; }

        public int PaginationPageSize { get; set; } = 25;

        public int PaginationStartPage { get; set; } = 1;

        public int NumberOfRows { get; set; }

        public int? ColumnWidth { get; set; } = 200;

        public int? ColumnMinWidth { get; set; } = 200;

        public int? ColumnMaxWidth { get; set; }

        public RowSelectionType RowSelectionType { get; set; } = RowSelectionType.Single;

        public bool SuppressRowClickSelection { get; set; } = true;

        public bool RowMultiSelectWithClick { get; set; } = false;

        public bool SuppressRowDeselection { get; set; } = false;

        public bool GroupDefaultExpanded { get; set; } = true;

        public bool SuppressFilterRow { get; set; } = true;

        public bool CheckboxSelectionColumn { get; set; }

        internal bool CheckboxSelectionPinned { get; set; }

        public bool NullValueSortedToEnd { get; set; } = true;

        public int ScrollWidth { get; set; } = 6;

        public bool RowAlternateColorSchema { get; set; } = false;

        public bool UseVirtualization { get; set; } = true;

        /// <summary>
        /// Accessible name exposed by the semantic center grid.
        /// </summary>
        public string AriaLabel { get; set; } = "Grid de datos";

        /// <summary>
        /// Delay used by consumers that schedule local filtering. A zero value
        /// requests immediate filtering.
        /// </summary>
        public int FilterDebounceMilliseconds { get; set; } = 250;

        public RenderFragment? LoadingTemplate { get; set; }

        public RenderFragment? EmptyTemplate { get; set; }

        public RenderFragment? NoResultsTemplate { get; set; }

        public RenderFragment<Exception>? ErrorTemplate { get; set; }

        /// <summary>
        /// Number of extra items to render before and after the viewport
        /// to reduce blank areas when scrolling fast. The default follows
        /// Blazor's Virtualize component starting with .NET 11. Lower values
        /// reduce DOM size but make blank areas more likely during fast scrolls.
        /// </summary>
        public int OverscanCount { get; set; } = 15;

        /// <summary>
        /// Provides a unique stable identity for a data row. Configure this when
        /// data is refreshed, sorted remotely, or selection must survive a rebuild.
        /// Null or duplicated values fall back to the internal row ID.
        /// </summary>
        public Func<TItem, object?>? RowKeySelector { get; set; }

        public Func<TItem, bool>? DisableRow { get; set; }

        public bool ShowExpandCollapseButtons { get; set; } = true;
    }
}
