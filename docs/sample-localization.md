# Idiomas de la aplicación de ejemplos

Las pantallas y su contenido usan español por defecto e inglés como segundo
idioma. El menú conserva sus textos originales. El selector ES/EN de la cabecera
es la única preferencia de idioma y continúa guardándose en `dnet.language`.

## Recursos propios de la aplicación

`samples/Dnet.Shared/Localization/SampleStrings.resx` contiene español y
`SampleStrings.en.resx` contiene inglés. Ambos catálogos se incorporan al
ensamblado principal para permitir el cambio en WebAssembly sin recargar.

`SampleLocalizer` es un servicio scoped que lee la cultura de `IDnetLocalizer`.
Las pantallas lo reciben como `S` desde `_Imports.razor`:

```razor
<h1>@S["Forms.FormControls"]</h1>
```

Las claves agrupan los textos por pantalla; los textos reutilizados aparecen
en `Common`. Cada clave debe existir en los dos recursos y conservar los mismos
marcadores de formato (`{0}`, `{1}`, etc.). Se usa `S.Format(key, argumentos)`
para contadores y frases con valores. Los recursos contienen texto, nunca HTML.

## Estado y contenido dinámico

Un `DnetLocalizationObserver` solicita una nueva representación cuando cambia
el idioma. No se recrean formularios, listas ni selecciones. Las cabeceras de
listas y tablas se actualizan sobre sus opciones existentes.

`S.Text` traduce etiquetas procedentes de los datos de demostración, manteniendo
estables los valores utilizados para seleccionar o filtrar. `S.Message` traduce
los mensajes de resultado que ya estén visibles, incluidos los argumentos de
las plantillas del catálogo. Debe usarse solo con mensajes controlados por la
aplicación; las entradas del usuario, nombres, archivos, direcciones, identificadores
y fragmentos de código se muestran como datos originales.

`SampleValidationMessage` y `SampleValidationSummary` traducen los errores al
representarlos, conservando el `EditContext`, sus errores y el estado modificado
de los campos. No vuelven a validar ni restablecen el modelo al cambiar de idioma.

Los diálogos y notificaciones pasan `TextLocalizer = S.Message` para actualizar
su título, contenido y acciones mientras están abiertos. El contenido de los
paneles y tooltips observa el mismo idioma. Las muestras de derecha a izquierda
conservan `dir="rtl"` y usan los textos y el atributo `lang` del idioma elegido.

## Añadir textos

1. Añadir una clave descriptiva al catálogo de cada idioma.
2. Usar `S[key]` o `S.Format` al representar el texto.
3. Mantener valores de selección, rutas e identificadores estables.
4. Comprobar el texto en ambos idiomas, incluyendo el contenido abierto.

Para un idioma adicional, hay que añadir su catálogo y ampliar los idiomas
admitidos de la librería y del selector. El catálogo de ejemplos es independiente
del catálogo de textos internos de Dnet.Blazor.
