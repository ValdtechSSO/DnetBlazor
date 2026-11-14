using Microsoft.JSInterop;

namespace Dnet.Blazor.Components.Grid.BlgGrid;

public partial class BlgGrid<TItem>
{
    private bool _isMaximized;

    /// <summary>
    /// Whether the grid fills the window. The corner button that
    /// <see cref="Infrastructure.Entities.GridOptions{TItem}.EnableMaximize"/>
    /// shows toggles it, and Escape inside the grid brings it back.
    /// </summary>
    public bool IsMaximized => _isMaximized;

    private string GridContainerClasses => !GridOptions.EnableMaximize
        ? _gridClasses
        : _isMaximized
            ? $"{_gridClasses} blg-grid-maximizable blg-grid-maximized"
            : $"{_gridClasses} blg-grid-maximizable";

    private void ToggleMaximized() => _isMaximized = !_isMaximized;

    /// <summary>
    /// Called by the grid's script when Escape is pressed inside the maximized
    /// grid. Not meant to be called from application code.
    /// </summary>
    [JSInvokable]
    public void RestoreFromMaximized()
    {
        if (!_isMaximized) return;

        _isMaximized = false;
        StateHasChanged();
    }
}
