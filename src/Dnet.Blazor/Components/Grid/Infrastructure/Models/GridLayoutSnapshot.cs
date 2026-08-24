using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Enums;

namespace Dnet.Blazor.Components.Grid.Infrastructure.Models;

/// <summary>
/// Immutable, render-ready partition of the column layout. Rebuilt only when
/// definitions or layout options change so the Razor render path does not sort
/// and split columns repeatedly.
/// </summary>
internal sealed class GridLayoutSnapshot<TItem>
{
    private GridLayoutSnapshot(
        List<GridColumn<TItem>> orderedColumns,
        List<GridColumn<TItem>> centerColumns,
        List<GridColumn<TItem>> pinnedLeftColumns,
        List<GridColumn<TItem>> pinnedRightColumns,
        Dictionary<int, GridColumn<TItem>> columnsById,
        Dictionary<string, GridColumn<TItem>> columnsByDataField,
        string pinnedLeftTemplateColumns,
        string pinnedRightTemplateColumns,
        int pinnedLeftWidth,
        int pinnedRightWidth)
    {
        OrderedColumns = orderedColumns;
        CenterColumns = centerColumns;
        PinnedLeftColumns = pinnedLeftColumns;
        PinnedRightColumns = pinnedRightColumns;
        ColumnsById = columnsById;
        ColumnsByDataField = columnsByDataField;
        PinnedLeftTemplateColumns = pinnedLeftTemplateColumns;
        PinnedRightTemplateColumns = pinnedRightTemplateColumns;
        PinnedLeftWidth = pinnedLeftWidth;
        PinnedRightWidth = pinnedRightWidth;
    }

    public List<GridColumn<TItem>> OrderedColumns { get; }

    public List<GridColumn<TItem>> CenterColumns { get; }

    public List<GridColumn<TItem>> PinnedLeftColumns { get; }

    public List<GridColumn<TItem>> PinnedRightColumns { get; }

    public IReadOnlyDictionary<int, GridColumn<TItem>> ColumnsById { get; }

    public IReadOnlyDictionary<string, GridColumn<TItem>> ColumnsByDataField { get; }

    public string PinnedLeftTemplateColumns { get; }

    public string PinnedRightTemplateColumns { get; }

    public int PinnedLeftWidth { get; }

    public int PinnedRightWidth { get; }

    public string GetCenterTemplateColumns(bool includeCheckbox, bool includeGrouping, int groupingColumnWidth)
    {
        var templates = new List<string>(CenterColumns.Count + 2);
        if (includeCheckbox)
        {
            templates.Add("40px");
        }

        if (includeGrouping)
        {
            templates.Add($"{groupingColumnWidth}px");
        }

        templates.AddRange(CenterColumns.Select(column =>
            column.CanGrow == 1 ? $"minmax({column.Width}px, 1fr)" : $"{column.Width}px"));

        return string.Join(" ", templates);
    }

    /// <summary>
    /// Builds a shared, fixed-width template for the responsive header and
    /// body. CSS Grid cannot infer a common content width from two independent
    /// grid containers, so the template is calculated once from loaded rows.
    /// </summary>
    public string GetResponsiveContentTemplateColumns(
        IEnumerable<RowNode<TItem>> rowNodes,
        bool includeCheckbox,
        bool includeGrouping,
        int groupingColumnWidth,
        int maximumColumnWidth,
        out int totalWidth)
    {
        var templates = new List<string>(CenterColumns.Count + 2);
        totalWidth = 0;

        if (includeCheckbox)
        {
            templates.Add("40px");
            totalWidth += 40;
        }

        if (includeGrouping)
        {
            templates.Add($"{groupingColumnWidth}px");
            totalWidth += groupingColumnWidth;
        }

        foreach (var column in CenterColumns)
        {
            var width = GetResponsiveContentWidth(column, rowNodes, maximumColumnWidth);
            templates.Add(column.CanGrow == 1 ? $"minmax({width}px, 1fr)" : $"{width}px");
            totalWidth += width;
        }

        return string.Join(" ", templates);
    }

    public static GridLayoutSnapshot<TItem> Create(List<GridColumn<TItem>> columns, GridOptions<TItem> options)
    {
        var ordered = columns.OrderBy(column => column.ColumnOrder).ToList();
        var visible = ordered.Where(column => !column.Hide).ToList();
        var center = visible.Where(column => column.Pinned == Pinned.None).ToList();
        var left = visible.Where(column => column.Pinned == Pinned.Left).ToList();
        var right = visible.Where(column => column.Pinned == Pinned.Right).ToList();

        var byId = new Dictionary<int, GridColumn<TItem>>();
        var byDataField = new Dictionary<string, GridColumn<TItem>>();
        foreach (var column in ordered)
        {
            byId[column.ColumnId] = column;
            byDataField[column.DataField] = column;
        }

        var leftTemplate = BuildFixedTemplate(left, options.CheckboxSelectionColumn && options.CheckboxSelectionPinned);
        var rightTemplate = BuildFixedTemplate(right, false);

        return new GridLayoutSnapshot<TItem>(
            ordered,
            center,
            left,
            right,
            byId,
            byDataField,
            leftTemplate,
            rightTemplate,
            left.Sum(column => column.Width) + (options.CheckboxSelectionPinned ? 40 : 0),
            right.Sum(column => column.Width));
    }

    private static string BuildFixedTemplate(IEnumerable<GridColumn<TItem>> columns, bool includeCheckbox)
    {
        var templates = new List<string>();
        if (includeCheckbox)
        {
            templates.Add("40px");
        }

        templates.AddRange(columns.Select(column => $"{column.Width}px"));
        return string.Join(" ", templates);
    }

    private static int GetResponsiveContentWidth(
        GridColumn<TItem> column,
        IEnumerable<RowNode<TItem>> rowNodes,
        int maximumColumnWidth)
    {
        var longestTextLength = GetHeaderTextLength(column);

        foreach (var rowNode in rowNodes)
        {
            if (rowNode.IsGroup || rowNode.RowData is null || column.CellDataFn is null)
            {
                continue;
            }

            var cellParams = new CellParams<TItem>
            {
                RowData = rowNode.RowData,
                GridColumn = column,
                RowNode = rowNode
            };

            var value = column.CellDataFn(cellParams)?.ToString() ?? string.Empty;
            longestTextLength = Math.Max(longestTextLength, value.Length);
        }

        const int cellHorizontalPadding = 24;
        const int averageCharacterWidth = 8;
        var requestedWidth = longestTextLength * averageCharacterWidth + cellHorizontalPadding;
        var minimumWidth = Math.Max(column.Width, column.MinWidth ?? 0);
        var maximumWidth = Math.Max(minimumWidth, Math.Min(maximumColumnWidth, column.MaxWidth ?? maximumColumnWidth));

        return Math.Clamp(Math.Max(minimumWidth, requestedWidth), minimumWidth, maximumWidth);
    }

    private static int GetHeaderTextLength(GridColumn<TItem> column)
    {
        var sortAndFilterAffordanceLength = column.Sortable || column.EnableAdvancedFilter ? 4 : 0;
        return (column.HeaderName?.Length ?? 0) + sortAndFilterAffordanceLength;
    }
}
