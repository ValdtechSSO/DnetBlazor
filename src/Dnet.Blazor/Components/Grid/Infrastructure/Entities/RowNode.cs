using Microsoft.AspNetCore.Components;

namespace Dnet.Blazor.Components.Grid.Infrastructure.Entities
{
    public class RowNode<TItem>
    {
        private ElementReference _focusElement;

        public long RowNodeId { get; set; }

        /// <summary>
        /// Stable consumer-facing identity for data rows. It is preserved across
        /// rebuilds when a unique <see cref="GridOptions{TItem}.RowKeySelector"/>
        /// is supplied. A null value uses <see cref="RowNodeId"/> for rendering.
        /// </summary>
        public object? StableKey { get; set; }

        internal object RenderKey { get; set; } = new object();

        private bool _selected { get; set; } = false;

        private bool _clicked { get; set; } = false;

        private bool _hovered { get; set; } = false;


        public bool Show { get; set; }

        public bool AdvShow { get; set; }

        public string? GroupValue { get; set; }

        public TItem? RowData { get; set; }

        public object? RowDataValue { get; set; }

        public List<TItem>? GroupData { get; set; }

        public List<TItem>? AggregatedData { get; set; }

        public RowNode<TItem>? Parent { get; set; }

        public int? Level { get; set; }

        public int? UiLevel { get; set; }

        public bool IsGroup { get; set; }

        public int? RowGroupIndex { get; set; }

        public bool LeafGroup { get; set; }

        public bool FirstChild { get; set; }

        public bool LastChild { get; set; }

        public int? ChildIndex { get; set; }

        public List<GridColumn<TItem>>? RowGroupColumn { get; set; }

        public GridColumn<TItem>? RowGridColumn { get; set; }

        public string? KeyRowGroupColumn { get; set; }

        public List<RowNode<TItem>>? ChildrenAfterGroup { get; set; }

        public List<RowNode<TItem>>? ChildrenAfterFilter { get; set; }

        public List<RowNode<TItem>>? ChildrenAfterSort { get; set; }

        public Dictionary<GridColumn<TItem>, bool>? RowSpanSkippedCells { get; set; }

        public Dictionary<GridColumn<TItem>, uint>? RowSpanTargetCells { get; set; }

        public int? AllChildrenCount { get; set; }

        public bool Expanded { get; set; }

        public int RowHeight { get; set; }

        public bool Selectable { get; set; } = true;

        public bool First { get; set; }

        /// <summary>
        /// Gets whether the row is currently rendered and has a focusable grid cell.
        /// </summary>
        public bool HasFocusElement { get; private set; }

        /// <summary>
        /// Moves focus to the row's rendered grid cell.
        /// </summary>
        /// <remarks>
        /// Callers must provide a fallback when the row may no longer be rendered,
        /// such as after virtual scrolling, filtering, or deletion.
        /// </remarks>
        public ValueTask FocusAsync() => _focusElement.FocusAsync(preventScroll: false);

        public Dictionary<GridColumn<TItem>, uint>? FirstSpanRow { get; set; }

        public Dictionary<GridColumn<TItem>, object>? FirstSpanRowData { get; set; }

        internal void SetFocusElement(ElementReference focusElement)
        {
            _focusElement = focusElement;
            HasFocusElement = true;
        }

       
        public bool IsSelected() {

            return _selected;
        }

        public bool SelectThisNode(bool newValue) 
        {
            if (!Selectable || _selected == newValue) { return false; }

            _selected = newValue;

            return true;
        }

        public bool IsClicked()
        {
            return _clicked;
        }

        public bool ClickThisNode(bool newValue)
        {
            if (!Selectable || _clicked == newValue) { return false; }

            _clicked = newValue;

            return true;
        }

        public bool IsHovered()
        {
            return _hovered;
        }

        public bool HoverThisNode(bool newValue)
        {
            if (_hovered == newValue) { return false; }

            _hovered = newValue;

            return true;
        }
    }
}
