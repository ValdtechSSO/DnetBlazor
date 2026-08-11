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
}
