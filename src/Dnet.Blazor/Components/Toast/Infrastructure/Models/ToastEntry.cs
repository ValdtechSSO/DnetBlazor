using Dnet.Blazor.Components.Toast.Infrastructure.Enums;
using Dnet.Blazor.Components.Toast;
using Microsoft.AspNetCore.Components;

namespace Dnet.Blazor.Components.Toast.Infrastructure.Models;

internal sealed class ToastEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string? Title { get; init; }

    public string? Text { get; init; }

    public ToastType ToastType { get; init; }

    public string? TypeIconClass { get; init; }

    public string? CloseIconClass { get; init; }

    public string? ToastTypeColor { get; init; }

    public string? ToastClass { get; init; }

    public int? DurationMilliseconds { get; init; }

    public IReadOnlyList<ToastAction> Actions { get; init; } = Array.Empty<ToastAction>();

    public ToastStrings? Strings { get; init; }

    public Type? ComponentType { get; init; }

    public IDictionary<string, object>? Parameters { get; init; }

    public RenderFragment? ContentChild { get; init; }
}

internal readonly record struct ToastStackKey(ToastPostion Position, string? ThemeScope);

internal sealed class ToastStackState
{
    public required ToastStackKey Key { get; init; }

    public required ToastConfig LayoutConfig { get; init; }

    public int MaxVisible { get; init; }

    public List<ToastEntry> Visible { get; } = new();

    public Queue<ToastEntry> Pending { get; } = new();

    public int OverlayReferenceId { get; set; }
}
