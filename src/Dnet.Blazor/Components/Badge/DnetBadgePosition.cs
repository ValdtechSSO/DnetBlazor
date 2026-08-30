namespace Dnet.Blazor.Components.Badge;

/// <summary>Specifies where a badge is placed relative to its content.</summary>
public enum DnetBadgePosition
{
    /// <summary>Places the badge above and after the content.</summary>
    AboveAfter,

    /// <summary>Places the badge above and before the content.</summary>
    AboveBefore,

    /// <summary>Places the badge below and before the content.</summary>
    BelowBefore,

    /// <summary>Places the badge below and after the content.</summary>
    BelowAfter,

    /// <summary>Shortcut for <see cref="AboveBefore"/>.</summary>
    Before,

    /// <summary>Shortcut for <see cref="AboveAfter"/>.</summary>
    After,

    /// <summary>Shortcut for <see cref="AboveAfter"/>.</summary>
    Above,

    /// <summary>Shortcut for <see cref="BelowAfter"/>.</summary>
    Below
}
