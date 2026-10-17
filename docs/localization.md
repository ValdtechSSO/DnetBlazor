# Idiomas y formatos

Dnet.Blazor incluye español por defecto e inglés como segundo idioma. Los
textos propios de los controles se resuelven desde un catálogo compartido de
recursos `.resx`, mediante `IStringLocalizer`. Los títulos, nombres de opciones,
mensajes de negocio y contenido proporcionados por la aplicación siguen siendo
responsabilidad de esa aplicación.

## Registro y selección de idioma

`AddDnetBlazor()` registra automáticamente `IDnetLocalizer` con ámbito scoped.
No hace falta añadir servicios para conservar el idioma predeterminado:

```csharp
builder.Services.AddDnetBlazor(); // es-ES
```

Para elegir inglés como idioma inicial:

```csharp
using Dnet.Blazor.Localization;

builder.Services.AddDnetBlazor();
builder.Services.AddDnetLocalization(options => options.DefaultCulture = "en-US");
```

Desde un componente, el idioma puede cambiar sin recargar la página:

```razor
@using Dnet.Blazor.Localization
@inject IDnetLocalizer Localizer

<button type="button" @onclick='() => Localizer.SetCulture("es-ES")'>Español</button>
<button type="button" @onclick='() => Localizer.SetCulture("en-US")'>English</button>
```

También se admiten variantes regionales, por ejemplo `es-MX` y `en-GB`. Las
traducciones utilizan el catálogo español o inglés y los formatos conservan la
variante regional. Una cultura de otro idioma se rechaza sin alterar la selección
anterior.

El servicio conserva la cultura por ámbito de usuario: no modifica
`DefaultThreadCurrentCulture`, `DefaultThreadCurrentUICulture` ni variables
estáticas compartidas. En Blazor Server, cada circuito mantiene su propio idioma.
La aplicación decide si guarda la preferencia en una cookie o almacenamiento
local. La muestra la conserva en `dnet.language` y actualiza el atributo HTML
`lang`; ese comportamiento de navegador pertenece a la aplicación, no a la
librería.

En la cabecera de la muestra, el botón circular `ES` o `EN` abre un
`ConnectedPanel` con los idiomas disponibles. El botón de tema muestra el icono
del tema activo (sol para claro, luna para oscuro) y abre su propio panel.
La opción actual queda marcada y ambas preferencias se conservan al recargar.

## Personalizaciones y respaldo

Los textos definidos explícitamente en un componente tienen prioridad sobre el
catálogo. Para `PickListStrings` y `ToastStrings`, la resolución se realiza por
propiedad: instancia → configuración en DI → traducción del idioma seleccionado.
Los bundles parciales de `DnetDatePickerLabels` permiten personalizar sólo algunas
etiquetas. Incluso un override igual al antiguo texto inglés se conserva.

```csharp
var texts = new PickListStrings { ClearLabel = "Vaciar mi selección" };
```

Ese botón conserva su texto al cambiar de idioma; las otras etiquetas cambian
automáticamente. Se mantienen las APIs anteriores de `Strings`, `Labels` y
etiquetas individuales. Un componente presentacional que no necesite otros
servicios puede seguir utilizándose sin registrar Dnet: utiliza un respaldo
inmutable en español.

Los recursos neutrales son españoles y `DnetStrings.en.resx` contiene inglés.
Ambos catálogos se incluyen en el ensamblado principal para que WebAssembly pueda
cambiar entre los dos idiomas sin descargar un satélite ni recargar la página.
Una traducción ausente utiliza el recurso español; una clave desconocida devuelve
su propia clave. El catálogo de la librería es independiente del `ResourcesPath`
que utilice la aplicación para sus propios recursos.

## Fechas, números y estado

Los controles usan `IDnetLocalizer.Culture` para mostrar fechas, horas y números,
respetando un parámetro `Culture` explícito cuando el control lo admite. Los
valores técnicos de los inputs HTML, fechas ISO y operaciones internas del grid
mantienen formatos invariantes. Cambiar el idioma no convierte ni modifica los
valores del modelo.

Los controles y los paneles abiertos observan el cambio de idioma y actualizan
sus textos, etiquetas accesibles y formatos. Conservan selección, búsqueda,
contenido expandido y valores pendientes. Los errores de parseo propios de
Number/Date también se actualizan sin marcar el formulario como modificado.
Las validaciones de negocio o `DataAnnotations` proporcionadas por la aplicación
deben utilizar sus propios recursos.

En WebAssembly deben estar disponibles los datos de globalización de las
culturas utilizadas. La muestra incluye:

```xml
<BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>
```

La página `/Localization` demuestra cambio de idioma, persistencia de selección,
paneles abiertos, formatos regionales y textos personalizados.
