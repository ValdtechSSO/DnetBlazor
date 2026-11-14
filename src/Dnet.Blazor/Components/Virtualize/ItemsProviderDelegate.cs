// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace Dnet.Blazor.Components.Virtualize
{
    /// <summary>
    /// A function that provides items to a virtualized source.
    /// </summary>
    /// <typeparam name="TItem">The type of the context for each item in the list.</typeparam>
    /// <param name="request">The <see cref="ItemsProviderRequest"/> defining the request details.</param>
    /// <returns>A <see cref="ValueTask"/> whose result is a <see cref="ItemsProviderResult{TItem}"/> upon successful completion.</returns>
    [Obsolete("Use Microsoft.AspNetCore.Components.Web.Virtualization.ItemsProviderDelegate<TItem> instead. This copy of the framework component is not used by the library and will be removed in the next major version.")]
    public delegate ValueTask<ItemsProviderResult<TItem>> ItemsProviderDelegate<TItem>(ItemsProviderRequest request);
}
