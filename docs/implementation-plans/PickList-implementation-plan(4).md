# PickList<TItem, TKey> — Plan de implementación

## 1. Objetivo

Implementar `PickList<TItem, TKey>` como un componente genérico de Blazor para **selección múltiple sobre una colección paginada**, sustituyendo el patrón clásico de dos paneles con flechas de transferencia por un único panel seleccionable.

El componente debe funcionar igual de bien con:

- colecciones completas en memoria;
- colecciones filtradas y paginadas en servidor;
- elementos seleccionados que no estén cargados;
- búsquedas que cambien la ventana visible sin alterar la selección.

La identidad de selección se basa siempre en una **clave estable `TKey`**, nunca en la instancia de `TItem`.

---

## 2. Principios de diseño

1. **La selección es global.**  
   Buscar o paginar nunca elimina selecciones existentes.

2. **La clave es la identidad.**  
   `SelectedKeys` es la fuente de verdad.

3. **La selección es un conjunto.**  
   No tiene orden y no admite duplicados.

4. **Los datos visibles son solo una ventana.**  
   Un elemento seleccionado no necesita estar cargado.

5. **`TotalCount` y `FilteredCount` representan universos distintos.**  
   El contador de selección usa el total global; el pager usa el total filtrado.

6. **Los contadores tienen ciclos de invalidación distintos.**  
   `TotalCount` cambia cuando cambia el universo; `FilteredCount` cambia cuando cambia la búsqueda o el dataset.

7. **Cliente y servidor comparten el mismo modelo mental.**

8. **La selección es controlada; la búsqueda no tiene por qué serlo.**  
   `SelectedKeys` vive en el consumidor y `PickList` nunca muta la instancia recibida. `SearchText`, en cambio, puede mantenerse internamente cuando el consumidor no necesita observarlo.

9. **No existe un espejo interno de selección.**  
   El render consulta directamente `SelectedKeys.Contains(key)`.

10. **Superficie pública pequeña y opinada.**  
    Search, Counter y Pager forman parte del componente; no se convierten en flags.

11. **Sin JavaScript en v1.**

12. **Sin ordenación de seleccionados.**  
    `PickList` representa un conjunto, no una secuencia.

## 3. Decisiones de arquitectura

### ADR-01 — `TKey` explícito

El componente se define como:

```razor
@typeparam TItem
@typeparam TKey where TKey : notnull
```

No se convierten claves a `string` ni `object`.

---

### ADR-02 — Search + Counter + Pager son inseparables

`PickList` siempre incluye:

```text
Search
Counter
Pager
Items
```

No existen flags como:

```csharp
ShowSearch
ShowCounter
ShowPager
Searchable
Pageable
```

Si una zona no tiene una acción útil, el componente adapta internamente la UI.

---

### ADR-03 — `Items` significa colección local completa

Cuando se usa:

```razor
Items="@teams"
```

`teams` es el universo local completo. `Items` no se usa para entregar páginas server-side manualmente.

Para datos parciales o grandes se usa:

```razor
ItemsProvider="@LoadTeams"
```

---

### ADR-04 — Ambos contadores son cacheables y nullable en server-side

El resultado del provider es:

```csharp
public sealed record PickListItemsProviderResult<TItem>(
    IReadOnlyList<TItem> Items,
    int? TotalCount,
    int? FilteredCount);
```

Semántica:

```text
TotalCount = null
    → reutiliza el último total global conocido.

FilteredCount = null
    → reutiliza el último total filtrado conocido para la búsqueda actual.
```

`null` nunca significa cero.

---

### ADR-05 — La invalidación de los contadores es simétrica

El request incluye:

```csharp
bool RequestTotalCount
bool RequestFilteredCount
```

Ciclos de invalidación:

```text
TotalCount
    invalidado por → primera carga
                      RefreshAsync(invalidateTotalCount: true)
                      cambio de ItemsProvider

FilteredCount
    invalidado por → primera carga
                      cambio de SearchText
                      RefreshAsync(...)
                      cambio de ItemsProvider
```

Un cambio de página, manteniendo la misma búsqueda y dataset, no invalida ninguno.

Contrato:

```text
RequestTotalCount = true
    → TotalCount debe ser no nulo.

RequestFilteredCount = true
    → FilteredCount debe ser no nulo.
```

---

### ADR-06 — `SelectedKeys` es `IReadOnlySet<TKey>`

La API usa:

```csharp
IReadOnlySet<TKey>
```

Esto expresa directamente:

- sin orden;
- sin duplicados;
- `SelectedKeys.Count` es el contador real de selección.

---

### ADR-07 — `SelectedKeysChanged` es obligatorio para un PickList interactivo

`PickList` v1 es totalmente controlado para selección. No existe modo uncontrolled.

Por tanto:

```csharp
SelectedKeysChanged.HasDelegate == false
```

es un error de configuración.

El mensaje debe indicar explícitamente:

```text
PickList requires two-way binding for selection. Use @bind-SelectedKeys.
```

Se evita así un componente que parece interactivo pero convierte cada click en un no-op silencioso.

### Compatibilidad con un futuro `Disabled`

`Disabled` está reservado para evolución. Cuando exista, la regla pasará a ser:

```csharp
if (!Disabled && !SelectedKeysChanged.HasDelegate)
    throw ...
```

Un `PickList` de solo lectura/deshabilitado podrá renderizar `SelectedKeys` sin exigir binding.


---

### ADR-08 — No existe `SelectionChanged` en v1

La única notificación de selección es:

```csharp
SelectedKeysChanged
```

El delta `Key + IsSelected` puede añadirse después si aparece un caso real.

---

### ADR-09 — No existe `OnSearchChanged`; SearchText soporta modo uncontrolled

No se añade `OnSearchChanged`.

La búsqueda tiene dos modos, elegidos por la existencia del callback estándar:

```text
SearchTextChanged.HasDelegate == true
    → modo controlled.
      SearchText es la fuente de verdad y el consumidor puede usar @bind-SearchText.

SearchTextChanged.HasDelegate == false
    → modo uncontrolled.
      PickList mantiene _searchText internamente.
```

Esto evita obligar a cada consumidor a declarar:

```csharp
private string? search;
```

cuando no necesita conocer el texto escrito.

En modo uncontrolled, `SearchText` actúa como **valor inicial**. Si el consumidor necesita cambiar la búsqueda programáticamente durante la vida del componente, debe usar binding.

En ambos modos, un cambio efectivo de búsqueda:

1. invalida `_filteredCount`;
2. vuelve a página 1;
3. cancela la request anterior;
4. refiltra o recarga;
5. nunca cambia `SelectedKeys`.


---

### ADR-10 — v1 incluye `Select visible` y `Clear selection`

Semántica:

```text
Select visible
    → SelectedKeys ∪ claves de la página actual

Clear selection
    → ∅
```

No existen callbacks públicos específicos para estas acciones; ambas emiten una nueva selección mediante `SelectedKeysChanged`.

Quedan fuera:

```text
Select all global
Select all filtered
```

`Select visible` permanece deshabilitado mientras `_isLoading == true`, porque `_visibleItems` puede pertenecer a la request anterior.

---

### ADR-11 — No existe estado espejo de selección

No se mantienen:

```csharp
_selectedKeySet
_lastSelectedKeysReference
```

El render usa directamente:

```csharp
SelectedKeys.Contains(key)
```

Para producir un cambio se clona el set actual:

```csharp
var next = new HashSet<TKey>(SelectedKeys);
```

Esto elimina una fuente completa de desincronización entre parámetro e implementación interna.

---

### ADR-12 — PROVISIONAL: checkbox nativo + `@bind:get` / `@bind:set`

La fila usa un `<input type="checkbox">`.

El candidato inicial evita:

```razor
checked="@selected"
@onchange="..."
```

y usa:

```razor
<input type="checkbox"
       @bind:get="selected"
       @bind:set="value => SetSelectedAsync(key, value)" />
```

El objetivo es garantizar que, si el parent rechaza un cambio de selección, el DOM real vuelva a reflejar:

```csharp
SelectedKeys.Contains(key)
```

### Esta ADR no se considera cerrada con bUnit

bUnit trabaja sobre el render tree/markup virtual. No reproduce de forma suficiente el caso que queremos proteger:

```text
DOM real
  mutado primero por interacción nativa del usuario

vs.

render tree
  cuyo valor controlado no cambió
```

Por tanto un test bUnit de:

```text
parent ignora SelectedKeysChanged
→ checkbox aparece desmarcado en el markup renderizado
```

puede pasar aunque un navegador real mantenga la propiedad DOM `checked = true`.

### Gate obligatorio en navegador

Antes de implementar la fase funcional se ejecutará un spike mínimo con Playwright sobre una página Blazor real:

```text
SelectedKeys no contiene key
→ usuario hace click real
→ browser marca el checkbox
→ parent recibe SelectedKeysChanged
→ parent rechaza/ignora el nuevo set
→ termina el ciclo de render
→ locator.IsChecked() debe ser false
```

También se verificará el caso equivalente del input de búsqueda en modo controlled cuando el parent rechace el nuevo `SearchText`.

### Fallback si `@bind:get` / `@bind:set` no resuelve la divergencia

ADR-12 se reabre y el rendering deberá forzar recreación DOM tras una interacción controlada.

El fallback preferido es una generación explícita:

```razor
@key="(key, _renderGeneration)"
```

donde `_renderGeneration` cambia después de la interacción controlada.

No se usará solamente:

```razor
@key="(key, selected)"
```

porque si el parent rechaza el toggle:

```text
selected antes  = false
selected después = false
```

y esa key no cambia.

Con `PageSize` pequeño, recrear la ventana visible tras un toggle rechazado es un coste aceptable frente a mantener un DOM divergente.

**Estado:** cerrado. El gate Playwright pasa en .NET 10 usando `checked` +
`@onchange` y una generación explícita en `@key`, que fuerza la recreación del
input únicamente cuando el parent rechaza o transforma el valor. Cuando lo
acepta, se conserva el nodo DOM y el foco.


---

### ADR-13 — `RefreshAsync` tiene semántica definida en ambos modos

```csharp
public Task RefreshAsync(bool invalidateTotalCount = false)
```

En modo local:

```text
releer Items
→ reaplicar búsqueda
→ recalcular TotalCount y FilteredCount
→ corregir PageIndex
→ render
```

`invalidateTotalCount` no cambia el resultado local porque ambos contadores son derivados de `Items`.

En modo provider:

```text
FilteredCount siempre se invalida.
TotalCount solo se invalida si invalidateTotalCount = true.
```

Esto permite que una creación/eliminación refresque correctamente también el número de resultados de la búsqueda actual.

---

### ADR-14 — Duplicados: diagnóstico fuerte en DEBUG, tolerancia en Release

Si una página contiene dos items con la misma `ItemKey`:

```text
DEBUG   → Debug.Assert / diagnóstico explícito.
Release → deduplicación determinista last-wins antes de renderizar.
```

Una librería UI no debe tumbar todo el árbol de producción por un dato sucio si puede renderizar una ventana coherente.

El provider sigue teniendo la responsabilidad contractual de no producir duplicados.

Consecuencia aceptada en Release: la deduplicación puede dejar una página con menos filas que `PageSize`:

```text
provider devuelve 10 items
→ 2 comparten ItemKey
→ PickList renderiza 9 filas
```

No se intenta hacer una segunda query para rellenar la página. El pager y los contadores siguen representando el dataset reportado por el provider.

---

### ADR-15 — Localización mediante un único `PickListStrings`

No se exponen seis parámetros de texto independientes.

Se define un record:

```csharp
public sealed record PickListStrings
{
    public string SearchPlaceholder { get; init; } = "Search";
    public string SelectedLabel { get; init; } = "Selected";
    public string SelectVisibleLabel { get; init; } = "Select page";
    public string ClearLabel { get; init; } = "Clear";
    public string EmptyLabel { get; init; } = "No items";
    public string NoResultsLabel { get; init; } = "No results";

    public static PickListStrings Default { get; } = new();
}
```

Resolución:

```text
Strings parameter por instancia
    ↓ si null
PickListStrings opcional desde DI
    ↓ si no está registrado
PickListStrings.Default
```

El componente **no** usa:

```csharp
[Inject]
public PickListStrings StringsFromServices { get; set; } = default!;
```

porque esa inyección sería obligatoria y una aplicación sin registro fallaría antes de poder aplicar el fallback.

Se inyecta el proveedor de servicios:

```csharp
[Inject]
private IServiceProvider Services { get; set; } = default!;

private PickListStrings EffectiveStrings =>
    Strings
    ?? Services.GetService<PickListStrings>()
    ?? PickListStrings.Default;
```

Así se cubren sin registro obligatorio:

- defaults de librería;
- override global por DI;
- override puntual por instancia.

Puede existir un `AddPickList(...)` opcional para registrar configuración global, pero usar `PickList` no depende de haberlo llamado.

La API pública solo añade:

```csharp
[Parameter]
public PickListStrings? Strings { get; set; }
```

---

### ADR-16 — Los flags de count son funciones puras del cache

No existen campos ni asignaciones imperativas para:

```text
_requestTotalCount
_requestFilteredCount
```

Cada request deriva sus flags exclusivamente del estado de cache:

```csharp
RequestTotalCount    = _totalCount    is null;
RequestFilteredCount = _filteredCount is null;
```

La invalidación solo modifica el cache:

```csharp
_totalCount = null;
_filteredCount = null;
```

Esto cierra el caso:

```text
request pide FilteredCount
→ request se cancela
→ _filteredCount sigue null
→ siguiente request vuelve a pedir FilteredCount automáticamente
```

Un flag nunca puede quedar desincronizado del dato que representa.


## 4. Anatomía visual

```text
┌────────────────────────────────────────────────────────┐
│ [ Search.............................................. ] │
├────────────────────────────────────────────────────────┤
│ Selected: 65 / 69        [Select page] [Clear]         │
│                                                        │
│ [«] [‹] [1] [2] [3] [4] [5] [›] [»]                   │
│                                                        │
│ [✓] Alicante Thunder                                   │
│ [✓] Bilbao Titans                                      │
│ [ ] Bucaneros                                          │
│ [✓] Burgos Bears                                       │
│ ...                                                    │
└────────────────────────────────────────────────────────┘
```

De arriba abajo:

1. **Search bar**
   - binding con `SearchText`;
   - búsqueda nueva vuelve a página 1;
   - puede aplicar debounce interno.

2. **Counter + selection actions**
   - `SelectedKeys.Count / TotalCount`;
   - siempre representa el universo global;
   - `Select visible`;
   - `Clear selection`.

3. **Pager**
   - primera;
   - anterior;
   - páginas;
   - siguiente;
   - última.

4. **Items**
   - checkbox nativo;
   - fila completa pulsable;
   - contenido mediante `ItemTemplate`.

---

## 5. API pública propuesta

```razor
@typeparam TItem
@typeparam TKey where TKey : notnull
```

### Datos

```csharp
[Parameter]
public IReadOnlyList<TItem>? Items { get; set; }

[Parameter]
public PickListItemsProvider<TItem>? ItemsProvider { get; set; }
```

Son mutuamente excluyentes.

### Identidad

```csharp
[Parameter, EditorRequired]
public Func<TItem, TKey> ItemKey { get; set; } = default!;
```

### Rendering

```csharp
[Parameter, EditorRequired]
public RenderFragment<TItem> ItemTemplate { get; set; } = default!;
```

### Búsqueda local

```csharp
[Parameter]
public Func<TItem, string?>? SearchTextSelector { get; set; }
```

### Selección controlada

```csharp
[Parameter]
public IReadOnlySet<TKey> SelectedKeys { get; set; }
    = new HashSet<TKey>();

[Parameter]
public EventCallback<IReadOnlySet<TKey>> SelectedKeysChanged { get; set; }
```

`SelectedKeysChanged` es obligatorio; el uso normal es:

```razor
@bind-SelectedKeys="@selectedKeys"
```

### Búsqueda

```csharp
[Parameter]
public string? SearchText { get; set; }

[Parameter]
public EventCallback<string?> SearchTextChanged { get; set; }
```

El binding es opcional:

```razor
<PickList ... />
```

funciona con búsqueda interna.

Si el consumidor necesita observar o controlar el texto:

```razor
<PickList @bind-SearchText="@search" ... />
```

Sin `SearchTextChanged`, `SearchText` se usa como valor inicial y el componente mantiene el valor efectivo en `_searchText`.

### Paginación

```csharp
[Parameter]
public int PageSize { get; set; } = 10;
```

### Localización

```csharp
[Parameter]
public PickListStrings? Strings { get; set; }
```

### Parámetros que deliberadamente no existen

```text
TotalCount
FilteredCount

SelectionChanged
OnSearchChanged

ShowSearch
ShowCounter
ShowPager

Searchable
Pageable
```

### API imperativa

```csharp
public Task RefreshAsync(bool invalidateTotalCount = false)
```

## 6. Modelo de selección

No existe copia interna de la selección.

La lectura es siempre:

```csharp
var selected = SelectedKeys.Contains(key);
```

### Toggle

```csharp
private async Task SetSelectedAsync(TKey key, bool isSelected)
{
    var next = new HashSet<TKey>(SelectedKeys);

    if (isSelected)
        next.Add(key);
    else
        next.Remove(key);

    await SelectedKeysChanged.InvokeAsync(next);
}
```

El componente nunca muta `SelectedKeys`.

### Parent rechaza el cambio

Si el consumidor recibe `SelectedKeysChanged` pero decide conservar el set anterior, ese set sigue siendo la fuente de verdad.

La estrategia inicial usa `@bind:get` / `@bind:set`, pero su capacidad para corregir la propiedad DOM real tras un rechazo **no se asume**: se valida en navegador real durante la Fase 0.

Hasta que ese test pase, ADR-12 permanece provisional.

### Select visible

Solo cuando:

```text
_isLoading == false
```

se ejecuta:

```csharp
var next = new HashSet<TKey>(SelectedKeys);

foreach (var item in _visibleItems)
{
    next.Add(ItemKey(item));
}

await SelectedKeysChanged.InvokeAsync(next);
```

### Clear selection

```csharp
await SelectedKeysChanged.InvokeAsync(
    new HashSet<TKey>());
```

Puede ejecutarse durante loading porque no depende de `_visibleItems`.

### Seleccionados no cargados

Es válido:

```text
SelectedKeys:
{1, 5, 8, 20, 37, 102}

Página actual:
items 40–49
```

El componente conoce seis selecciones sin cargar sus `TItem`.

## 7. Contrato de identidad

`ItemKey` es obligatorio:

```csharp
Func<TItem, TKey> ItemKey
```

Reglas:

- una clave identifica un único item;
- una clave es estable entre consultas;
- dos items visibles con la misma clave son un error;
- `PickList` no depende de `ReferenceEquals(TItem)`;
- `PickList` no depende de `Equals(TItem)`;
- `PickList` no depende del índice del elemento.

### Comparador futuro

Puede añadirse después:

```csharp
IEqualityComparer<TKey>? KeyComparer
```

No forma parte de v1.

---

## 8. Fuente de datos

### 8.1. Modo local

```razor
Items="@allTeams"
```

Flujo:

```text
allTeams
  ↓ SearchTextSelector + SearchText
filteredTeams
  ↓ PageIndex + PageSize
visibleTeams
```

Contadores:

```text
TotalCount = Items.Count
FilteredCount = filteredItems.Count
```

### 8.2. Modo servidor

```razor
ItemsProvider="@LoadTeams"
```

Contrato:

```csharp
public delegate ValueTask<PickListItemsProviderResult<TItem>>
    PickListItemsProvider<TItem>(
        PickListItemsProviderRequest request);
```

Request:

```csharp
public sealed record PickListItemsProviderRequest(
    int PageIndex,
    int PageSize,
    string? SearchText,
    bool RequestTotalCount,
    bool RequestFilteredCount,
    CancellationToken CancellationToken);
```

Result:

```csharp
public sealed record PickListItemsProviderResult<TItem>(
    IReadOnlyList<TItem> Items,
    int? TotalCount,
    int? FilteredCount);
```

Los dos nullable tienen la misma semántica:

```text
null = reutiliza el último valor válido de ese contador
```

pero cada uno posee su propio ciclo de invalidación.

## 9. `TotalCount` vs `FilteredCount`

### `TotalCount`

Universo completo:

```text
69 teams
```

Es independiente de:

```text
SearchText
PageIndex
PageSize
```

### `FilteredCount`

Universo de la búsqueda actual:

```text
Search: "Bilbao"
FilteredCount = 2
```

Es independiente de `PageIndex`, pero cambia cuando cambia:

```text
SearchText
dataset
```

### Estado interno server-side

```csharp
private int? _totalCount;
private int? _filteredCount;
```

### Protocolo de caché

Primera carga:

```text
RequestTotalCount    = true
RequestFilteredCount = true

→ TotalCount    = 400000
→ FilteredCount = 400000
```

Cambio de página sin cambiar búsqueda:

```text
RequestTotalCount    = false
RequestFilteredCount = false

→ TotalCount    = null
→ FilteredCount = null
```

No se ejecuta ningún count por navegar entre páginas.

Cambio de búsqueda:

```text
SearchText = "bil"

RequestTotalCount    = false
RequestFilteredCount = true

→ TotalCount    = null
→ FilteredCount = 812
```

El `COUNT` filtrado solo entra en el hot path cuando la búsqueda cambia o cuando un refresh invalida el dataset.

### Estado inicial

Antes de conocer `TotalCount`:

```text
Selected: 65 / —
```

Antes de conocer `FilteredCount`, el pager no inventa un número de páginas. Puede mostrar placeholder/loading y permanece deshabilitado hasta recibir el contador requerido.

### Violaciones de contrato

Si:

```text
RequestTotalCount = true
```

y llega:

```text
TotalCount = null
```

es error de provider.

Lo mismo aplica a:

```text
RequestFilteredCount = true
FilteredCount = null
```

### Regla crítica

```text
TotalCount = null
    nunca significa 0.

FilteredCount = null
    nunca significa 0 resultados.
```

`FilteredCount = null` solo puede reutilizarse para la **misma búsqueda vigente**. Un cambio de `SearchText` invalida primero el cache.

## 10. Búsqueda

### Valor efectivo

```csharp
private string? _searchText;
private bool _searchInitialized;

private bool IsSearchControlled =>
    SearchTextChanged.HasDelegate;

private string? EffectiveSearchText =>
    IsSearchControlled
        ? SearchText
        : _searchText;
```

En modo uncontrolled, durante la primera parametrización:

```csharp
if (!_searchInitialized)
{
    _searchText = SearchText;
    _searchInitialized = true;
}
```

Después, `SearchText` no se usa como canal de actualización programática. Para eso el consumidor debe usar `@bind-SearchText`.

### Cambio desde la caja de búsqueda

Conceptualmente:

```csharp
private async Task SetSearchTextAsync(string? value)
{
    if (IsSearchControlled)
    {
        await SearchTextChanged.InvokeAsync(value);
    }
    else
    {
        _searchText = value;
    }

    _pageIndex = 0;
    _filteredCount = null;

    CancelCurrentRequest();
    await ReloadAsync();
}
```

En modo controlled, el parent sigue siendo fuente de verdad. En modo uncontrolled, `_searchText` es estado UI interno.

### Flags de la request

Nunca se asignan manualmente:

```csharp
RequestTotalCount    = _totalCount    is null;
RequestFilteredCount = _filteredCount is null;
```

Como un cambio de búsqueda hace primero:

```csharp
_filteredCount = null;
```

la siguiente request solicita automáticamente un nuevo filtered count.

Si esa request se cancela, `_filteredCount` sigue siendo `null`, por lo que la siguiente request vuelve a solicitarlo sin lógica especial.

### Local

Filtro v1:

```text
Contains(EffectiveSearchText, OrdinalIgnoreCase)
```

sobre:

```csharp
SearchTextSelector(item)
```

### Server-side

`EffectiveSearchText` viaja como `SearchText` en `PickListItemsProviderRequest`.

### Debounce

Valor interno recomendado:

```text
300 ms
```

No se expone en v1.

### Requests concurrentes

Ejemplo:

```text
"b"
"bi"
"bil"
```

La respuesta de `"b"` nunca puede sobrescribir la de `"bil"`.

Se usan:

- `CancellationTokenSource`;
- generation/sequence id;
- comprobación de request vigente antes de aplicar `Items` o contadores.


## 11. Paginación

Estado:

```csharp
private int _pageIndex;
```

Internamente es base 0.

El número de páginas solo se calcula cuando `FilteredCount` es conocido:

```csharp
PageCount = _filteredCount is null
    ? 0
    : (int)Math.Ceiling(
        _filteredCount.Value / (double)PageSize);
```

### Cambio de página server-side

Manteniendo la misma búsqueda:

```text
RequestTotalCount    = false
RequestFilteredCount = false
```

si ambos caches ya son válidos.

Así cambiar de página solo carga los items de esa ventana.

### Acciones

```text
First     → 0
Previous  → max(0, PageIndex - 1)
Next      → min(PageCount - 1, PageIndex + 1)
Last      → PageCount - 1
```

### Búsqueda nueva

Siempre:

```text
PageIndex = 0
FilteredCount invalidado
```

### Dataset reducido

Tras un `RefreshAsync`, si el conteo filtrado nuevo deja la página actual fuera de rango:

```text
PageIndex → última página válida
```

Si esa corrección cambia la página solicitada, puede requerir una segunda carga de items para la página válida, pero no un segundo count.

### Ventana numérica

v1 usa internamente un máximo razonable, por ejemplo 5 páginas visibles. No se expone `MaxVisiblePages`.

## 12. Rendering de filas

### Candidato inicial, sujeto al gate de Fase 0

```razor
@foreach (var item in _visibleItems)
{
    var key = ItemKey(item);
    var selected = SelectedKeys.Contains(key);

    <label class="pick-list-item"
           @key="key">

        <input type="checkbox"
               class="pick-list-checkbox"
               @bind:get="selected"
               @bind:set="value => SetSelectedAsync(key, value)" />

        <span class="pick-list-selection-indicator"
              aria-hidden="true">
        </span>

        <span class="pick-list-item-content">
            @ItemTemplate(item)
        </span>
    </label>
}
```

El componente controla:

- identidad;
- checkbox;
- interacción;
- indicador visual;
- layout.

El consumidor controla el contenido visual del item.

### Requisito, no implementación

La regla que sí está cerrada es:

```text
SelectedKeys es la única fuente de verdad.
El DOM del checkbox no puede mantener un valor rechazado por el parent.
```

La técnica concreta todavía depende del test Playwright de Fase 0.

Si `@bind:get` / `@bind:set` pasa el test en navegador, queda confirmado para v1.

Si falla, se cambia el rendering para forzar recreación DOM mediante una generación en `@key` u otro mecanismo verificado en navegador.

No se considera válido demostrar esta propiedad únicamente con bUnit.


## 13. Accesibilidad

La fila usa un checkbox real.

Ventajas:

- semántica nativa marcado/no marcado;
- lectores de pantalla sin ARIA inventado;
- `Space` gratis;
- foco nativo;
- toda la fila pulsable mediante `<label>`;
- `:checked` utilizable desde CSS;
- `:focus-visible` utilizable desde CSS.

### CSS conceptual

```css
.pick-list-checkbox {
    /* visually hidden, but still accessible */
}

.pick-list-checkbox:checked + .pick-list-selection-indicator {
    /* selected */
}

.pick-list-checkbox:focus-visible ~ .pick-list-item-content {
    /* focus */
}
```

### Restricción de `ItemTemplate`

En v1 se considera contenido **de presentación**.

No se recomienda introducir:

```text
button
link
checkbox
input
otro control interactivo
```

dentro del template.

Si aparece un caso real de acciones por fila, se diseñará explícitamente.

### Otros requisitos

- input de búsqueda con label accesible;
- pager con `aria-label`;
- `Select visible` inequívoco;
- `Clear selection` inequívoco;
- controles disabled cuando no puedan actuar;
- foco visible.

---

## 14. Estados visuales

### Initial provider load

```text
Selected: 65 / —
```

El pager permanece en estado loading hasta conocer `FilteredCount`.

### Normal

Items visibles.

### Empty

```text
No items available
```

### No search results

Solo puede afirmarse cuando:

```text
FilteredCount == 0
```

Entonces:

```text
Selected: 5 / 69
No results found
```

### Loading

Durante `_isLoading`:

- `Select visible` está deshabilitado;
- `Clear selection` puede seguir activo;
- no se actúa sobre `_visibleItems` como si pertenecieran a la request nueva;
- el pager evita disparar navegación incompatible con la request en curso.

### Error

v1 no captura silenciosamente excepciones del provider.

Preferencia:

```text
integración con ErrorBoundary
```

## 15. Sin acción de creación embebida

`PickList` se limita a buscar, paginar y seleccionar. La creación de entidades
pertenece a la pantalla consumidora y no forma parte de la API ni del layout del
componente.

---

## 16. Refresh explícito

API única:

```csharp
public Task RefreshAsync(bool invalidateTotalCount = false)
```

### Modo local

`RefreshAsync()` fuerza una reevaluación de la colección actual:

```text
Items
→ Search
→ FilteredCount
→ clamp PageIndex
→ VisibleItems
→ render
```

`TotalCount` se deriva otra vez de `Items.Count`.

El parámetro `invalidateTotalCount` no tiene efecto semántico adicional en local; se conserva para mantener una API uniforme.

Esto hace útil `RefreshAsync()` incluso si el consumidor mutó el contenido de la colección local sin sustituir su referencia.

### Modo provider

Todo refresh invalida:

```text
_filteredCount = null
```

porque el dataset puede haber cambiado y, por tanto, también el número de resultados de la búsqueda vigente.

Si además:

```csharp
invalidateTotalCount: true
```

se invalida:

```text
_totalCount = null
```

La request siguiente deriva:

```text
RequestFilteredCount = (_filteredCount == null)
RequestTotalCount    = (_totalCount == null)
```

No existen flags mutables separados del cache.

### Ejemplo después de una modificación externa del dataset

```csharp
await _pickList.RefreshAsync(
    invalidateTotalCount: true);
```

Esto recalcula ambos contadores una sola vez.

`RefreshAsync` nunca cambia `SelectedKeys`.

## 17. Estado interno

```csharp
private int _pageIndex;

private int? _filteredCount;
private int? _totalCount;

private bool _isLoading;

private string? _searchText;
private bool _searchInitialized;

private IReadOnlyList<TItem> _visibleItems
    = Array.Empty<TItem>();

private CancellationTokenSource? _loadCts;
private long _requestGeneration;
```

Deliberadamente **no existen**:

```csharp
_selectedKeySet
_lastSelectedKeysReference

_requestTotalCount
_requestFilteredCount
```

### Estado externo

```text
Items / ItemsProvider
SelectedKeys
SearchText (solo en modo controlled)
PageSize
Strings
```

### Estado interno

```text
PageIndex
VisibleItems
SearchText interno cuando uncontrolled
FilteredCount cache
TotalCount cache
Loading
request lifecycle
```

La selección se deriva directamente del parámetro público en cada render.

## 18. Ciclo de parámetros

### `SelectedKeys`

No requiere sincronización interna.

El render consulta:

```csharp
SelectedKeys.Contains(key)
```

Si el consumidor sustituye o muta su implementación subyacente y provoca un nuevo render, `PickList` observa directamente el estado actual del parámetro.

### `Items`

En local:

```text
refilter
recount
repaginate
```

### `SearchText`

Si `SearchTextChanged.HasDelegate`:

```text
modo controlled
→ SearchText es fuente de verdad
→ cambio externo invalida búsqueda si el valor efectivo cambió.
```

Si no hay delegate:

```text
modo uncontrolled
→ SearchText se usa solo como valor inicial
→ _searchText mantiene la interacción posterior.
```

En cualquier cambio efectivo:

```text
PageIndex = 0
_filteredCount = null
reload/refilter
```

sin alterar selección.

### `PageSize`

No cambia `FilteredCount`, pero recalcula `PageCount` y corrige `PageIndex` si fuera necesario.

### `ItemsProvider`

Si cambia la referencia:

```text
cancel current request
_totalCount = null
_filteredCount = null
reload with both Request*Count = true
```

### `Strings`

Cambiar el objeto efectivo de strings solo provoca rerender; no recarga datos.

## 19. Validación de configuración

### `Items` + `ItemsProvider`

No permitido:

```text
Items != null
AND
ItemsProvider != null
```

Lanzar `InvalidOperationException`.

### Ninguna fuente

Permitido como estado vacío.

### Binding de selección ausente

En v1, mientras el componente sea interactivo, si:

```csharp
!SelectedKeysChanged.HasDelegate
```

lanzar:

```text
InvalidOperationException:
PickList requires two-way binding for selection. Use @bind-SelectedKeys.
```

No existe fallback uncontrolled para selección.

Cuando se añada un futuro `Disabled`, la validación será:

```csharp
!Disabled && !SelectedKeysChanged.HasDelegate
```

### Binding de búsqueda ausente

**No es error.**

Si:

```csharp
!SearchTextChanged.HasDelegate
```

la búsqueda funciona en modo uncontrolled mediante `_searchText`.

### `PageSize <= 0`

Lanzar `ArgumentOutOfRangeException`.

### `ItemKey` ausente

Error de configuración.

### `ItemTemplate` ausente

Error de configuración.

### Modo local sin `SearchTextSelector`

Como Search es parte inseparable de `PickList`, `Items` requiere selector textual.

### Provider incumple un contador solicitado

Si:

```text
RequestTotalCount = true
AND TotalCount = null
```

error de contrato.

Igualmente:

```text
RequestFilteredCount = true
AND FilteredCount = null
```

error de contrato.

### Claves duplicadas en una página

No se lanza una excepción de producción por defecto.

Pipeline:

```text
DEBUG   → Debug.Assert con diagnóstico.
Release → deduplicate by TKey, last-wins.
```

El comportamiento release es determinista y evita dos filas con la misma identidad DOM.

Se acepta que una página pueda renderizar menos filas que `PageSize`; no se hace backfill ni una segunda query.

## 20. Claves seleccionadas que ya no existen

Ejemplo:

```text
SelectedKeys = { 10, 20, 30 }
```

pero `20` fue eliminado de la base de datos.

`PickList` no puede saberlo sin consultar el universo completo.

Contrato:

- no elimina claves automáticamente;
- no valida claves no cargadas;
- cuenta las claves recibidas;
- el consumidor/dominio limpia selecciones huérfanas.

---

## 21. CSS y tematización

Usar:

```text
PickList.razor.css
```

CSS isolation.

Variables sugeridas:

```css
--pick-list-background
--pick-list-foreground
--pick-list-primary
--pick-list-border
--pick-list-item-background
--pick-list-item-selected
--pick-list-radius
--pick-list-gap
```

Estados:

```text
hover
focus-visible
checked
disabled
loading
```

Evitar colores hardcoded propios de una aplicación concreta.

---

## 22. Estructura de archivos

```text
Components/
└── PickList/
    ├── PickList.razor
    ├── PickList.razor.cs
    ├── PickList.razor.css
    ├── PickListItemsProvider.cs
    ├── PickListItemsProviderRequest.cs
    ├── PickListItemsProviderResult.cs
    └── PickListStrings.cs
```

Opcionalmente:

```text
PickListServiceCollectionExtensions.cs
```

para facilitar registro global de `PickListStrings`.

El componente no depende de ese registro porque resuelve DI mediante `IServiceProvider.GetService<PickListStrings>()` y cae en `PickListStrings.Default`.

Para ADR-12 se añade un test de navegador real, por ejemplo:

```text
tests/
└── PickList.BrowserTests/
    └── ControlledInputSyncTests.cs
```

con Playwright contra una página Blazor mínima.

No existe en v1:

```text
PickListSelectionChangedEventArgs.cs
```

## 23. Estrategia de implementación

### Fase 0 — Gate de navegador para ADR-12

Antes de escribir la implementación completa de filas, crear una página Blazor mínima y un test Playwright real.

Caso obligatorio:

```text
checkbox inicialmente false
→ click real
→ parent recibe callback
→ parent rechaza el nuevo set
→ ciclo de render termina
→ propiedad DOM checked == false
```

No se valida leyendo solo markup; se consulta la propiedad real mediante Playwright:

```text
locator.IsChecked()
```

Repetir el principio con el input de búsqueda en modo controlled si el parent rechaza el nuevo texto.

#### Resultado A — pasa

Se confirma:

```text
checkbox nativo + @bind:get / @bind:set
```

y ADR-12 pasa de provisional a cerrada.

#### Resultado B — falla

No continuar con el rendering definitivo.

Reabrir ADR-12 e introducir un mecanismo de recreación DOM verificado en navegador, por ejemplo una generación en `@key`.

#### Criterio de salida

La estrategia concreta de sincronización controlada está demostrada en un navegador real para la versión de .NET soportada.

---

### Fase 1 — Identidad y selección controlada

Implementar:

- `TItem`;
- `TKey`;
- `ItemKey`;
- `ItemTemplate`;
- `IReadOnlySet<TKey> SelectedKeys`;
- `SelectedKeysChanged` obligatorio;
- estrategia de checkbox validada en Fase 0;
- toggle;
- contador;
- `Select visible`;
- `Clear selection`.

#### Criterio de aceptación

- cada cambio produce un nuevo set;
- el componente no muta el set recibido;
- parent que rechaza un toggle no deja el DOM divergente según el test Playwright de Fase 0;
- falta de `@bind-SelectedKeys` falla con mensaje explícito.

---

### Fase 2 — Spike vertical mínimo de `ItemsProvider`

Validar el camino de mayor riesgo antes de completar el modo local.

Implementar lo mínimo para probar:

```text
Request
  ↓
ItemsProvider
  ↓
Items
TotalCount?
FilteredCount?
  ↓
Render
```

Incluye:

- `RequestTotalCount`;
- `RequestFilteredCount`;
- cache de ambos contadores;
- primera página server-side;
- una búsqueda server-side;
- una navegación de página;
- cancelación básica;
- respuesta obsoleta;
- placeholders.

#### Criterio crítico

```text
search change
    → filtered count se recalcula

page change
    → ningún count se recalcula
```

---

### Fase 3 — Paginación local completa

Implementar:

- `PageSize`;
- first/previous/numbers/next/last;
- límites;
- ventana numérica;
- `FilteredCount` local.

---

### Fase 4 — Búsqueda local

Implementar:

- `SearchText`;
- `SearchTextChanged`;
- `SearchTextSelector`;
- filtro case-insensitive;
- reset a página 1.

---

### Fase 5 — Hardening server-side

Completar:

- cancelación robusta;
- generation id;
- respuestas obsoletas;
- invalidación independiente de contadores;
- `RefreshAsync`;
- página inválida tras reducción del filtro;
- loading state;
- validaciones del provider;
- deduplicación release de items visibles.

---

### Fase 7 — Accesibilidad

Validar:

- checkbox;
- label;
- estrategia de binding confirmada en navegador;
- Space;
- foco;
- screen reader;
- pager;
- Search;
- Select visible;
- Clear selection.

---

### Fase 8 — Localización

Implementar:

- `PickListStrings.Default`;
- lookup opcional mediante `IServiceProvider.GetService<PickListStrings>()`;
- override global por DI;
- `Strings` por instancia;
- funcionamiento correcto sin ningún registro DI;
- fallback determinista.

---

### Fase 9 — Hardening general

Cubrir:

- empty;
- no results;
- `PageSize` dinámico;
- provider lento;
- provider cancelado;
- provider con error;
- ambos contadores desconocidos;
- claves duplicadas;
- item eliminado;
- última página desaparecida;
- `Select visible` durante loading;
- refresh local con misma referencia de `Items`.

---

### Fase 10 — Documentación y ejemplos

Crear ejemplos:

```text
1. Basic local PickList
2. Local search
3. Server-side provider
4. Preselected keys
5. Selected keys not loaded
6. Select visible
7. Clear selection
8. Custom ItemTemplate
9. Localized/global PickListStrings
```

## 24. Tests

Usar bUnit.

### Selección controlada

- selección inicial;
- toggle on;
- toggle off;
- `SelectedKeysChanged` recibe nueva instancia;
- componente no muta set recibido;
- no duplica claves;
- `TKey` correcto;
- ausencia de delegate lanza mensaje con `@bind-SelectedKeys`.

### Sincronización DOM — Playwright, no bUnit

Este comportamiento **no se considera cubierto por bUnit**.

bUnit sigue siendo válido para comprobar:

- callback emitido;
- set emitido;
- render tree resultante.

Pero la divergencia que protege ADR-12 requiere un navegador real porque depende de una propiedad DOM mutada por la interacción nativa.

Test Playwright obligatorio:

```text
parent rechaza toggle
→ después del ciclo de render
→ locator.IsChecked() refleja SelectedKeys
```

Este test se ejecuta ya en la Fase 0 y queda como regresión permanente.


### Select visible

- añade página actual;
- conserva selecciones anteriores;
- no selecciona otras páginas;
- es idempotente;
- funciona con búsqueda;
- está deshabilitado durante loading.

### Clear selection

- devuelve set vacío;
- funciona con seleccionados no cargados;
- funciona durante loading.

### Persistencia

```text
select page 1
→ page 2
→ search
→ clear search
→ page 1
```

Resultado:

```text
selection preserved
```

### Búsqueda controlled / uncontrolled

- sin `SearchTextChanged`, escribir actualiza `_searchText` y la búsqueda funciona;
- sin binding, búsqueda no es un no-op;
- `SearchText` inicial se respeta en modo uncontrolled;
- con `@bind-SearchText`, el parent controla el valor;
- cambio efectivo de búsqueda pone `_filteredCount = null`;
- parent que rechaza `SearchText` controlled se valida en navegador real si aplica la misma estrategia de binding DOM.

### Flags derivados del cache

- `_totalCount == null` implica `RequestTotalCount == true`;
- `_totalCount != null` implica `RequestTotalCount == false`;
- `_filteredCount == null` implica `RequestFilteredCount == true`;
- `_filteredCount != null` implica `RequestFilteredCount == false`;
- cancelar una request que debía traer `FilteredCount` deja `_filteredCount == null`;
- la siguiente request vuelve a pedirlo automáticamente;
- no existen flags mutables independientes del cache.

### Provider — contadores

- primer load solicita ambos counts;
- después de conocerlos, page change solicita ninguno;
- search change solicita `FilteredCount` pero no `TotalCount`;
- `TotalCount = null` conserva cache global;
- `FilteredCount = null` conserva cache de la búsqueda vigente;
- cambio de SearchText invalida `FilteredCount` antes de request;
- `RefreshAsync()` invalida `FilteredCount`;
- `RefreshAsync(invalidateTotalCount: true)` invalida ambos;
- count solicitado y devuelto null falla.

### Provider — concurrencia

- request anterior se cancela;
- respuesta antigua no pisa la nueva;
- contador de una búsqueda antigua no pisa el de la búsqueda vigente;
- selected key no cargada persiste.

### Counter

Con total global conocido:

```text
SelectedKeys.Count / TotalCount
```

Con total global desconocido:

```text
SelectedKeys.Count / —
```

Nunca:

```text
selected visible / page size
selected / FilteredCount
0 / 0 falso
```

### Pager

- no inventa PageCount con `FilteredCount == null`;
- page change no solicita filtered count si cache válido;
- first/previous disabled en primera;
- next/last disabled en última;
- search vuelve a primera;
- reducción de resultados corrige página.

### Duplicados

- DEBUG produce diagnóstico;
- release-style normalization conserva el último item de cada key;
- nunca se renderizan dos filas con la misma key;
- se acepta que la página resultante tenga menos filas que `PageSize` sin backfill.

### Refresh local

- reevalúa `Items` aunque la referencia sea la misma;
- recalcula ambos contadores;
- corrige página;
- no cambia selección.

### Localización

- sin registro DI usa `PickListStrings.Default`;
- override global DI;
- `Strings` por instancia tiene prioridad;
- ausencia de `PickListStrings` en DI nunca provoca excepción.

### Accesibilidad

- checkbox alcanzable;
- Space cambia estado;
- foco visible;
- labels del pager;
- Search accesible;
- Select visible accesible;
- Clear selection accesible.

## 25. Casos límite

### `TotalCount = 0`

```text
Selected: 0 / 0
```

solo cuando el total global real es conocido y vale 0.

### `TotalCount` desconocido

```text
Selected: N / —
```

### `FilteredCount` desconocido

El pager no asume cero ni calcula páginas hasta recibir el count solicitado.

### Search sin resultados

Solo después de:

```text
FilteredCount = 0
```

se muestra `No results`.

### `SelectedKeys.Count > TotalCount`

Posible con claves huérfanas. El componente no borra estado silenciosamente.

### `PageSize > FilteredCount`

Una única página.

### Provider devuelve claves duplicadas

DEBUG diagnostica; release aplica last-wins.

### Search cambia mientras llega una respuesta

La respuesta antigua se descarta, incluidos sus contadores.

### Clear durante loading

Permitido.

### Select visible durante loading

Deshabilitado. No se permite seleccionar accidentalmente `_visibleItems` pertenecientes a la página/request anterior.

### Parent rechaza toggle

El estado visual debe volver/permanecer en el valor de `SelectedKeys`; el DOM no puede convertirse en una segunda fuente de verdad.

La técnica concreta solo se considera válida después del test Playwright de Fase 0.

### `RefreshAsync` local

Re-filtra y re-pagina incluso con la misma referencia de `Items`.

## 26. Fuera de alcance para v1

No implementar:

- dual list;
- flechas de transferencia;
- selección ordenada;
- drag & drop;
- reorder;
- select all global;
- select all filtered;
- shift range selection;
- agrupaciones;
- virtualización;
- edición inline;
- creación embebida;
- eliminación de entidades;
- sincronización automática de claves huérfanas;
- controles interactivos arbitrarios dentro de `ItemTemplate`.

---

## 27. API reservada para evolución

No implementar todavía, pero evitar bloquear:

```csharp
EmptyTemplate
NoResultsTemplate
LoadingTemplate
ErrorTemplate

SearchPredicate
SearchDebounce

KeyComparer

MaxVisiblePages

Disabled
IsItemDisabled
ItemClass
ItemClassSelector

```

Posible futuro:

```csharp
SelectionChanged
```

solo si aparece un caso real que necesite el delta.

Si se añade:

```csharp
Disabled
```

un `PickList` deshabilitado podrá omitir `@bind-SelectedKeys`; la obligatoriedad del callback solo aplica a selección interactiva.

---

## 28. Ejemplo server-side

### Razor

```razor
<PickList @ref="_teamPickList"
          TItem="Team"
          TKey="long"
          ItemsProvider="@LoadTeams"
          ItemKey="team => team.TeamId"
          @bind-SelectedKeys="@selectedTeamIds">

    <ItemTemplate Context="team">
        @team.Name
    </ItemTemplate>

</PickList>
```

### Estado

```csharp
private IReadOnlySet<long> selectedTeamIds
    = new HashSet<long>();

private PickList<Team, long>? _teamPickList;
```

La búsqueda funciona internamente sin declarar `search`. Si el parent necesita observar/controlar el texto puede añadir:

```razor
@bind-SearchText="@search"
```

### Provider

```csharp
private async ValueTask<PickListItemsProviderResult<Team>>
    LoadTeams(PickListItemsProviderRequest request)
{
    var result = await TeamService.SearchAsync(
        search: request.SearchText,
        pageIndex: request.PageIndex,
        pageSize: request.PageSize,
        includeTotalCount: request.RequestTotalCount,
        includeFilteredCount: request.RequestFilteredCount,
        cancellationToken: request.CancellationToken);

    return new PickListItemsProviderResult<Team>(
        Items: result.Items,
        TotalCount: request.RequestTotalCount
            ? result.TotalCount
            : null,
        FilteredCount: request.RequestFilteredCount
            ? result.FilteredCount
            : null);
}
```

En un cambio de página normal:

```text
includeTotalCount    = false
includeFilteredCount = false
```

por lo que el servicio solo obtiene los items de la ventana.

### Después de una modificación externa del dataset

```csharp
private async Task OnDatasetChangedAsync()
{
    await _teamPickList!
        .RefreshAsync(invalidateTotalCount: true);
}
```

El refresh pide ambos contadores porque una modificación externa puede afectar
tanto al universo global como a la búsqueda actual.

## 29. Invariantes del componente

Estas reglas son el contrato real de `PickList`.

### Invariante 1

```text
Search never changes SelectedKeys.
```

### Invariante 2

```text
Pagination never changes SelectedKeys.
```

### Invariante 3

```text
Item identity = ItemKey(item).
```

### Invariante 4

```text
A selected key does not require a loaded TItem.
```

### Invariante 5

```text
SelectedCount is global.
```

### Invariante 6

```text
Selection is an unordered set with no duplicates.
```

### Invariante 7

```text
Items and ItemsProvider cannot be active simultaneously.
```

### Invariante 8

```text
PickList never mutates or mirrors SelectedKeys.
```

El estado de selección renderizado se deriva directamente de:

```csharp
SelectedKeys.Contains(key)
```

### Invariante 9

```text
TotalCount is global and independent of SearchText.
```

```text
TotalCount = null
→ reuse last known global total.
```

### Invariante 10

```text
FilteredCount belongs to the current SearchText, not to the current page.
```

```text
FilteredCount = null
→ reuse last known count for the current search.
```

Nunca significa cero resultados.

### Invariante 11

```text
Changing page does not invalidate either count.
```

Mientras búsqueda y dataset sean los mismos.

### Invariante 12

```text
A stale provider response never overwrites a newer query.
```

Esto incluye `Items`, `TotalCount` y `FilteredCount`.

### Invariante 13

```text
Select visible affects only currently visible keys and is disabled while loading.
```

### Invariante 14

```text
Clear selection never requires loaded items.
```

### Invariante 15

```text
Interactive selection requires SelectedKeysChanged.
```

Sin `@bind-SelectedKeys`, la configuración falla en lugar de convertirse en un no-op.

### Invariante 16

```text
The DOM checkbox is never a second source of truth.
```

Si el parent rechaza el cambio, el estado visual sigue a `SelectedKeys`.

Esta propiedad debe estar demostrada por un test en navegador real; bUnit no es evidencia suficiente.

### Invariante 17

```text
Request flags are pure functions of count-cache validity.
```

Siempre:

```csharp
RequestTotalCount    == (_totalCount    is null)
RequestFilteredCount == (_filteredCount is null)
```

Los flags no tienen estado propio. Invalidar un contador significa poner su cache a `null`.

### Invariante 18

```text
Search remains functional without @bind-SearchText.
```

Sin `SearchTextChanged`, `PickList` mantiene el texto de búsqueda internamente. La ausencia de un callback de búsqueda nunca convierte la caja en un no-op.

## 30. Decisiones resueltas y todavía abiertas

### 0. ADR-12 — Resultado del spike de navegador

Resuelto empíricamente con una página Blazor real y una regresión Playwright
permanente. El navegador vuelve a `SelectedKeys` cuando el parent rechaza un
toggle y vuelve a `SearchText` cuando rechaza la búsqueda. La implementación
adopta la estrategia de recreación DOM mediante generación en `@key`.


### 1. Error visual del provider

v1 puede dejar que la excepción llegue a:

```text
ErrorBoundary
```

Más adelante podría añadirse:

```text
ErrorTemplate + Retry
```

### 2. Política exacta de debounce

El valor inicial propuesto es 300 ms y no forma parte de la API v1.

Queda por validar si la implementación debe:

- cancelar/reiniciar timer en cada pulsación;
- omitir debounce cuando `SearchText` cambia programáticamente;
- ejecutar inmediatamente al limpiar el texto.

### 3. Comportamiento visual del pager durante loading

La regla funcional está cerrada —no puede disparar requests incompatibles—, pero queda decidir si visualmente:

- se deshabilita completo;
- mantiene números visibles pero disabled;
- muestra skeleton/placeholder.

Esto no afecta al contrato público.

## 31. Definition of Done v1

- [x] `PickList<TItem, TKey>` usa clave genérica explícita.
- [x] `SelectedKeys` es `IReadOnlySet<TKey>`.
- [x] `SelectedKeysChanged` es obligatorio y el error indica `@bind-SelectedKeys`.
- [x] no existe espejo interno de selección.
- [x] selección por clave, no por instancia.
- [x] selección persiste entre páginas.
- [x] selección persiste entre búsquedas.
- [x] selección puede contener claves no cargadas.
- [x] la estrategia de sincronización del checkbox está verificada con Playwright en navegador real.
- [x] parent que rechaza toggle no deja DOM divergente en la prueba browser, no solo en bUnit.
- [x] `Select visible` selecciona solo la página actual.
- [x] `Select visible` está disabled durante loading.
- [x] `Clear selection` vacía el conjunto global.
- [x] `Items` representa la colección completa.
- [x] `ItemsProvider` soporta ventana server-side.
- [x] provider devuelve `Items + TotalCount? + FilteredCount?`.
- [x] request incluye `RequestTotalCount` y `RequestFilteredCount` derivados exclusivamente de caches nullable.
- [x] page change con cache válido no ejecuta counts.
- [x] cancelar una request de count deja cache null y obliga automáticamente a pedirlo de nuevo.
- [x] search change invalida solo `FilteredCount`.
- [x] refresh provider invalida `FilteredCount` y opcionalmente `TotalCount`.
- [x] `RefreshAsync` local re-filtra y re-pagina.
- [x] contador usa selección global / total global.
- [x] total desconocido usa placeholder.
- [x] filtered count desconocido nunca se interpreta como cero.
- [x] búsqueda nunca altera selección.
- [x] búsqueda funciona sin `@bind-SearchText`.
- [x] con `@bind-SearchText`, el texto puede ser controlado externamente.
- [x] paginación nunca altera selección.
- [x] respuestas obsoletas no pisan items ni contadores.
- [x] requests cancelables.
- [x] duplicados: diagnóstico DEBUG + last-wins release; se acepta una página con menos filas sin backfill.
- [x] `PickListStrings` funciona sin registro DI y soporta DI global + override por instancia.
- [x] teclado y screen reader validados.
- [x] tests cubren invariantes.
- [x] ejemplos local + server-side.
- [x] XML docs en API pública.
- [x] CSS aislado y tematizable.

## 32. Alcance de la primera implementación

```text
PickList<TItem, TKey>

Identity
  └── ItemKey

Data
  ├── Items
  │   └── SearchTextSelector
  │
  └── ItemsProvider
      ├── RequestTotalCount
      ├── RequestFilteredCount
      ├── Items
      ├── TotalCount?
      └── FilteredCount?

Selection
  ├── SelectedKeys : IReadOnlySet<TKey>
  ├── SelectedKeysChanged (required)
  ├── Select visible
  └── Clear selection

Search
  ├── internal _searchText when uncontrolled
  ├── SearchText
  └── SearchTextChanged (optional control)

Paging
  └── PageSize

Rendering
  ├── ItemTemplate
  └── native checkbox with browser-verified controlled sync strategy

Localization
  └── PickListStrings?

Imperative API
  └── RefreshAsync(invalidateTotalCount)
```

La prioridad no es acumular parámetros. La prioridad es conservar correctamente **identidad, selección y contadores cuando la UI solo conoce una pequeña ventana de los datos**, sin convertir la navegación entre páginas en una sucesión de `COUNT(*)` innecesarios.
