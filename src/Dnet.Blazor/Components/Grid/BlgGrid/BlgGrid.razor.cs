using Dnet.Blazor.Components.Grid.Infrastructure.Entities;
using Dnet.Blazor.Components.Grid.Infrastructure.Enums;
using Dnet.Blazor.Components.Grid.Infrastructure.Interfaces;
using Dnet.Blazor.Components.Grid.Infrastructure.Models;
using Dnet.Blazor.Components.Grid.Infrastructure.Services;
using Dnet.Blazor.Components.Grid.Virtualize;
using Dnet.Blazor.Components.Grid.BlgBody;
using Dnet.Blazor.Components.Grid.BlgHeader;
using Microsoft.JSInterop;
using Dnet.Blazor.Components.Paginator;
using Dnet.Blazor.Infrastructure.Models.SearchModels;
using Microsoft.AspNetCore.Components;

namespace Dnet.Blazor.Components.Grid.BlgGrid;

public partial class BlgGrid<TItem> : ComponentBase, IVirtualizeJsCallbacks, IAsyncDisposable
{
    #region Parameters

    [Parameter]
    public EventCallback<CellClikedData<TItem>> OnCellClicked { get; set; }

    [Parameter]
    public EventCallback<RowNode<TItem>> OnRowClicked { get; set; }

    [Parameter]
    public EventCallback<RowNode<TItem>> OnRowDoubleClicked { get; set; }

    [Parameter]
    public EventCallback<List<RowNode<TItem>>> OnSelectionChanged { get; set; }

    [Parameter]
    public EventCallback<SearchModel> OnPaginationChanged { get; set; }

    [Parameter]
    public EventCallback<SearchModel> OnSortingChanged { get; set; }

    [Parameter]
    public EventCallback<SearchModel> OnFilterChanged { get; set; }

    [Parameter]
    public EventCallback<SearchModel> OnAdvancedFilterChanged { get; set; }

    [Parameter]
    public EventCallback<GroupModel> OnGroupingChanged { get; set; }

    [Parameter, EditorRequired]
    public IEnumerable<TItem> GridData { get; set; } = new List<TItem>();

    [Parameter, EditorRequired]
    public List<GridColumn<TItem>> GridColumns { get; set; } = new();

    [Parameter]
    public GridColumn<TItem> GroupGridColumn { get; set; } = new();

    [Parameter]
    public GridOptions<TItem> GridOptions { get; set; } = new();

    [Parameter]
    public bool HasGrouping { get; set; }

    [Parameter]
    public bool ItemsCount { get; set; }

    /// <summary>
    /// Compatibility alias for <see cref="GridOptions{TItem}.OverscanCount"/>.
    /// Prefer configuring the value in <see cref="GridOptions"/> so all Grid
    /// options live in one place.
    /// </summary>
    [Parameter]
    [Obsolete("Use GridOptions.OverscanCount instead.")]
    public int OverscanCount { get; set; } = -1;

    [Parameter]
    public int PaginatorHeight { get; set; } = 50;

    #endregion

    #region Injected Services

    [Inject]
    public ISorting<TItem> SortingService { get; set; } = default!;

    [Inject]
    public IFiltering<TItem> FilteringService { get; set; } = default!;

    [Inject]
    public IGrouping<TItem> GroupingService { get; set; } = default!;

    [Inject]
    public IPaginator PaginatorService { get; set; } = default!;

    [Inject]
    public IAdvancedFiltering<TItem> AdvancedFilteringService { get; set; } = default!;

    [Inject]
    public IBlGridInterop<TItem> BlGridInterop { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    #endregion

    #region Private Fields

    internal long _nextId { get; set; } = -1;

    private BlgBody<TItem> _blgCenter { get; set; } = default!;
    private BlgBody<TItem>? _blgPinnedLeft { get; set; }
    private BlgBody<TItem>? _blgPinnedRight { get; set; }
    private DnetPaginator? _paginator { get; set; }

    private ElementReference _eCenterContainer { get; set; }
    private ElementReference _eBodyHorizontalScrollContainer { get; set; }
    private ElementReference _eHeaderContainer { get; set; }
    private ElementReference _eBodyHorizontalScrollViewport { get; set; }
    private ElementReference _eGridContainer { get; set; }

    private int eBodyHorizontalScrollContainerWidth { get; set; }
    private string _transformX { get; set; } = "0px";

    private TreeRowNode<TItem> _treeRn { get; set; } = default!;
    private List<RowNode<TItem>> _rowNodes { get; set; } = new();
    private int _sourceItemCount;
    private readonly Dictionary<long, RowNode<TItem>> _rowNodesById = new();
    private readonly Dictionary<object, RowNode<TItem>> _rowNodesByKey = new();
    private readonly HashSet<object> _selectedRowKeys = new();
    private GridLayoutSnapshot<TItem>? _layoutSnapshot;

    protected Guid ElemenId { get; set; } = Guid.NewGuid();

    private List<string> _groupByColumns { get; set; } = new();
    public List<GridColumn<TItem>> _gridColumns { get; set; } = new();
    private List<RowNode<TItem>> _renderedRowNodes { get; set; } = new();

    private string _gridClasses { get; set; } = "blg-arcadia-theme";

    private bool _mouseDown { get; set; }
    private int _lastColumnPosition { get; set; }
    private GridColumn<TItem>? _columnSelected { get; set; }

    private SearchModel _searchModel { get; set; } = default!;
    private BlgHeader<TItem>? _blgHeader { get; set; }

    private int _activeGroups { get; set; } = 0;
    private bool _firstRender { get; set; } = true;

    private GridApi<TItem> _gridApi { get; set; } = default!;

    private bool _isDataNoInitialized { get; set; }
    private bool _isColumnsNoInitialized { get; set; }
    private bool _shouldRender;
    private bool _licResult = true;

    private RowNode<TItem>? _previousHoverRowNode;
    private bool _updatingGridData { get; set; } = false;

    private bool _pinnedRight { get; set; }
    private bool _pinnedLeft { get; set; }

    public IEnumerable<TItem> _gridData { get; set; } = new List<TItem>();

    private bool _isExpanded = false;
    private DotNetObjectReference<BlgGrid<TItem>>? _windowEventReference;
    private DotNetObjectReference<BlgGrid<TItem>>? _touchEventReference;
    private IEnumerable<TItem>? _gridDataSource;
    private List<GridColumn<TItem>>? _gridColumnsSource;
    private GridOptions<TItem>? _gridOptionsSource;

    #endregion

    #region Virtualization Fields

    private List<RowNode<TItem>> _itemsToShow = new();
    private VirtualizeJsInterop _jsInterop = default!;
    private ElementReference _spacerBefore;
    private ElementReference _spacerAfter;

    private int _itemsBefore;
    private int _visibleItemCapacity;
    private int _itemCount;
    private float _itemSize;
    private float _containerSize;

    private CancellationTokenSource? _refreshCts;
    private long _refreshVersion;
    private long _renderedRefreshVersion;
    private Exception? _refreshException;
    private long _virtualRequestsStarted;
    private long _virtualRequestsCancelled;
    private long _virtualResponsesDiscarded;
    private long _virtualResponsesApplied;
    private long _virtualRefreshDurationMilliseconds;
    private bool _hasResolvedInitialVirtualWindow;

    private int _lastRenderedItemCount;
    private int _lastRenderedPlaceholderCount;

    private IReadOnlyList<RowNode<TItem>> _loadedItems = Array.Empty<RowNode<TItem>>();
    private int _loadedItemsStartIndex;
    private ItemsProviderDelegate<RowNode<TItem>> _itemsProvider = default!;
    private ICollection<RowNode<TItem>> _items { get; set; } = default!;

    #endregion

    #region Component Lifecycle Methods

    public override Task SetParametersAsync(ParameterView parameters)
    {
        return base.SetParametersAsync(parameters);
    }

    protected override void OnInitialized()
    {
        _gridApi = new GridApi<TItem>();
        _searchModel = new();
        _searchModel.AdvancedFilterModels = new();
        _searchModel.PaginationModel = new();
        _searchModel.SortModel = new();

        _jsInterop = new VirtualizeJsInterop(this, JSRuntime);
        _itemsProvider = DefaultItemsProvider;

        _isDataNoInitialized = true;
        _isColumnsNoInitialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        var dataChanged = !ReferenceEquals(_gridDataSource, GridData);
        var columnsChanged = !ReferenceEquals(_gridColumnsSource, GridColumns);
        var optionsChanged = !ReferenceEquals(_gridOptionsSource, GridOptions);

        if (GridOptions.UseVirtualization)
        {
            _itemSize = GridOptions.RowHeight;
        }

#pragma warning disable CS0618
        if (OverscanCount >= 0)
        {
            GridOptions.OverscanCount = OverscanCount;
        }
#pragma warning restore CS0618

        if (dataChanged || columnsChanged || optionsChanged || _isDataNoInitialized || _isColumnsNoInitialized)
        {
            if (dataChanged || _isDataNoInitialized)
            {
                CaptureSelectionKeys();
                _hasResolvedInitialVirtualWindow = false;
                _gridData = GridData;
                InitializeGridWorkingData();
                _isDataNoInitialized = false;
                _gridDataSource = GridData;
            }

            if (columnsChanged || optionsChanged || _isColumnsNoInitialized)
            {
                _gridColumns = AdvancedFilteringService.InitAdvancedFilterModels(GridColumns, GridOptions.DefaultAdvancedFilterOperator);

                _pinnedRight = _gridColumns.Where(e => e.Pinned == Pinned.Right && !e.Hide).Any();
                _pinnedLeft = _gridColumns.Where(e => e.Pinned == Pinned.Left && !e.Hide).Any();
                RebuildLayoutSnapshot();

                if (_blgHeader != null) _blgHeader.ActiveRender();

                _isColumnsNoInitialized = false;
                _gridColumnsSource = GridColumns;
                _gridOptionsSource = GridOptions;
            }

            await InitializeGrid();
        }
    }

    /// <summary>
    /// Returns lightweight counters that can be asserted by tests or displayed
    /// by a diagnostic host without introducing per-row logging.
    /// </summary>
    public GridDiagnostics GetDiagnostics() => new()
    {
        VirtualRequestsStarted = Volatile.Read(ref _virtualRequestsStarted),
        VirtualRequestsCancelled = Volatile.Read(ref _virtualRequestsCancelled),
        VirtualResponsesDiscarded = Volatile.Read(ref _virtualResponsesDiscarded),
        VirtualResponsesApplied = Volatile.Read(ref _virtualResponsesApplied),
        VirtualRefreshDurationMilliseconds = Volatile.Read(ref _virtualRefreshDurationMilliseconds),
        TotalItemCount = _itemCount,
        ItemsBefore = _itemsBefore,
        VisibleItemCapacity = _visibleItemCapacity,
        RenderedItemCount = _itemsToShow.Count,
        OverscanCount = Math.Max(0, GridOptions.OverscanCount)
    };

    private void RebuildLayoutSnapshot() => _layoutSnapshot = GridLayoutSnapshot<TItem>.Create(_gridColumns, GridOptions);

    private GridColumn<TItem>? FindGridColumn(string dataField) =>
        _layoutSnapshot?.ColumnsByDataField.GetValueOrDefault(dataField)
        ?? _gridColumns.Find(column => column.DataField == dataField);

    private GridColumn<TItem>? FindGridColumn(int columnId) =>
        _layoutSnapshot?.ColumnsById.GetValueOrDefault(columnId)
        ?? _gridColumns.Find(column => column.ColumnId == columnId);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _firstRender = true;

            _windowEventReference = DotNetObjectReference.Create(this);
            _touchEventReference = DotNetObjectReference.Create(this);
            await BlGridInterop.AddWindowEventListeners(_eGridContainer, _windowEventReference);
            await BlGridInterop.AddTouchListeners(_eCenterContainer, _eBodyHorizontalScrollViewport, _touchEventReference);

            if (GridOptions.UseVirtualization)
            {
                await _jsInterop.InitializeAsync(_spacerBefore, _spacerAfter, GetVirtualizationRootMargin());
                await DefaultVirtualization();
            }
            else
            {
                StateHasChanged();
            }
        }

        _firstRender = false;
        _shouldRender = false;
    }

    #endregion
}
