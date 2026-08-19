# PickList

Los estilos de PickList se distribuyen en el bundle global de DnetBlazor. No
requiere una hoja `{App}.styles.css` para estilos de la librería.

`PickList<TItem, TKey>` is a key-based multi-selector for complete local
collections and paged server-side data. Selection is always controlled by the
consumer and is retained when the visible page or search changes.

## Local collection

```razor
<PickList TItem="Team"
          TKey="long"
          Items="@teams"
          ItemKey="team => team.Id"
          SearchTextSelector="team => team.Name"
          @bind-SelectedKeys="selectedTeamIds">
    <ItemTemplate Context="team">@team.Name</ItemTemplate>
</PickList>
```

`Items` represents the complete local universe. In local mode
`SearchTextSelector` is required and the component derives both counters.

## Server-side provider

```razor
<PickList TItem="Team"
          TKey="long"
          ItemsProvider="LoadTeamsAsync"
          ItemKey="team => team.Id"
          @bind-SelectedKeys="selectedTeamIds">
    <ItemTemplate Context="team">@team.Name</ItemTemplate>
</PickList>
```

```csharp
private async ValueTask<PickListItemsProviderResult<Team>> LoadTeamsAsync(
    PickListItemsProviderRequest request)
{
    var result = await teams.SearchAsync(
        request.SearchText,
        request.PageIndex,
        request.PageSize,
        request.RequestTotalCount,
        request.RequestFilteredCount,
        request.CancellationToken);

    return new(
        result.Items,
        request.RequestTotalCount ? result.TotalCount : null,
        request.RequestFilteredCount ? result.FilteredCount : null);
}
```

When a request asks for either count, the provider must return it. `null`
otherwise means that the component retains the cache for that count. A normal
page change therefore requests neither count.

## Refresh and localization

Call `await pickList.RefreshAsync(invalidateTotalCount: true)` after an external
change that modifies the global universe. `RefreshAsync()` always refreshes
the filtered count but preserves selected keys.

Set `Strings` per instance or register `PickListStrings` in DI to localize the
component globally. No registration is required: the built-in English strings
are the fallback. Pagination reuses the library's `DnetPaginator`; its visible
`Page`/`of` text and all navigation labels are supplied by the same
`PickListStrings` instance.

## Selection and controlled inputs

`SelectedKeys` is the only source of truth. The component always emits a new
`HashSet<TKey>` and never mutates the set it receives. Keys may remain selected
when their items are on another page, filtered out, not loaded, or no longer
present in the dataset. The consumer owns any cleanup of orphaned keys.

Search is uncontrolled by default. `SearchText` is then used only as its initial
value. Add `@bind-SearchText` when the parent must observe or accept/reject each
change. Accepted input keeps the same DOM node and keyboard focus. The checkbox
or search input is recreated only when the parent rejects or transforms its
controlled value, so the browser DOM cannot diverge from the source of truth.

## Provider cache contract

The first request asks for both counts. Page navigation reuses both counts. A
new search invalidates only `FilteredCount`; `RefreshAsync()` also invalidates
that count, while `RefreshAsync(true)` invalidates both. A provider must return
every requested count and may return `null` for a cached count that wasn't
requested. Requests are cancellable and stale responses are ignored.

Changing `PageSize` at runtime recalculates the last valid page and reloads the
provider window without unnecessarily requesting cached counts.

## Complete example matrix

The sample page at `/PickList` covers all v1 scenarios:

1. basic local collection;
2. local search;
3. server-side provider;
4. preselected keys;
5. selected keys that aren't loaded;
6. Select page;
7. Clear selection;
8. custom `ItemTemplate`;
9. instance-localized `PickListStrings`.

The `/PickList/controlled-rejection` fixture is intentionally minimal and is
used by the browser regression test for rejected checkbox and search changes.

Run that permanent Playwright regression locally after starting the sample:

Terminal 1:

```bash
tests/Dnet.Blazor.BrowserTests/install-playwright.sh chromium
dotnet run --project samples/Dnet.ClientSide --configuration Release --no-build --no-launch-profile --urls http://127.0.0.1:5101
```

Terminal 2:

```bash
DNET_BLAZOR_BROWSER_TESTS=true DNET_BLAZOR_BASE_URL=http://127.0.0.1:5101 \
  dotnet test tests/Dnet.Blazor.BrowserTests --configuration Release --no-build
```

The main CI workflow starts the sample and runs this gate automatically.

## Theme variables

PickList reads these optional public custom properties from any ancestor:
`--dnet-picklist-background`, `--dnet-picklist-foreground`,
`--dnet-picklist-primary`, `--dnet-picklist-border`,
`--dnet-picklist-item-background`, `--dnet-picklist-item-selected`,
`--dnet-picklist-item-hover`, `--dnet-picklist-muted`,
`--dnet-picklist-radius`, `--dnet-picklist-item-height`,
`--dnet-picklist-items-max-height`, `--dnet-picklist-header-height`,
`--dnet-picklist-footer-height`, and `--dnet-picklist-scrollbar`. The legacy
`--pick-list-*` names remain as fallback links for this major version. Defaults
then inherit the equivalent `--dnet-list-*` tokens and finally semantic tokens,
so List and PickList keep the same visual language when an application theme
changes them. The component occupies the available width; constrain it in the
consuming layout when a narrower panel is desired. The footer inherits the shared paginator tokens, including
`--dnet-icon-button-size`, so paginator sizing stays consistent with `List` and
`DoubleList`.

`ItemTemplate` should contain presentation content only. Avoid nesting links,
buttons, inputs, or other interactive controls inside its row label.
