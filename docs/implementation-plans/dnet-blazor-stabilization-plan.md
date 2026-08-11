# Plan de implementación para la estabilización de DnetBlazor

**Estado:** propuesto  
**Fecha:** 2026-08-11  
**Ámbito:** `Dnet.Blazor`, `Dnet.Blazor.Material` y aplicaciones de ejemplo  
**Origen:** auditoría estática de las 29 familias de componentes, compilación de los proyectos y revisión de ciclo de vida, estado, interoperabilidad JavaScript, accesibilidad, API y documentación.

## 1. Objetivo

Llevar la versión 5.x de DnetBlazor a un estado estable y verificable sin introducir cambios incompatibles innecesarios. El trabajo se divide en entregas pequeñas para corregir primero los fallos funcionales, después la gestión de recursos y finalmente accesibilidad, diseño de API, documentación y mantenimiento.

El plan cubre estas familias:

- AdminLayout, Autocomplete, Button, Checkbox, Chips y ConnectedPanel.
- DatePicker, DatePickerWeek, DatePickerWeekRaw, Dialog y DynamicStepper.
- ExpansionPanel, FloatingDoubleList, FloatingPanel, Form, Grid e ImageEditor.
- List, Overlay, Paginator, RadioButton, Select, Spinner, Stepper, Tabs, Toast, Tooltip, Tree y Virtualize.
- Los componentes auxiliares de `Dnet.Blazor.Material`.

## 2. Definición global de terminado

La iniciativa se considerará terminada cuando se cumplan todos estos puntos:

1. No quedan defectos P0 o P1 abiertos de este documento.
2. `Dnet.Blazor`, `Dnet.Blazor.Material` y los ejemplos Server y WebAssembly compilan de forma reproducible con cero errores y cero advertencias propias.
3. Existen pruebas unitarias, pruebas bUnit y pruebas de navegador para los flujos críticos.
4. No quedan manejadores `async void`, salvo callbacks de plataforma que estén documentados y encapsulen sus errores.
5. Todo `Timer`, `CancellationTokenSource`, `DotNetObjectReference`, observer y listener JavaScript tiene propietario y liberación determinista.
6. Los componentes reaccionan correctamente a cambios de parámetros posteriores al primer render.
7. Los patrones interactivos principales cumplen WCAG 2.2 AA y pueden utilizarse solamente con teclado.
8. Las APIs corregidas conservan aliases obsoletos durante una versión de transición.
9. Cada familia dispone de documentación, ejemplos, requisitos de DI y notas de accesibilidad.
10. El README, el paquete y el changelog muestran la misma versión.

## 3. Estrategia de entrega

- Cada bloque funcional debe publicarse en un PR independiente y con sus pruebas.
- Antes de corregir un defecto se añadirá una prueba de caracterización que falle por la causa esperada.
- Los cambios de API incompatibles se reservarán para una versión mayor. En 5.x se utilizarán aliases con `[Obsolete]`.
- No se mezclarán refactorizaciones estructurales con correcciones urgentes si eso dificulta revisar el comportamiento.
- Los componentes compartidos se corregirán primero: Overlay, formularios, ciclo de vida e interop afectan a muchas familias.
- Esfuerzo relativo: **S** hasta dos días, **M** entre tres y cinco días, **L** entre una y dos semanas, **XL** más de dos semanas. Es una referencia para secuenciar, no un compromiso de calendario.

## 4. Resumen de fases

| Fase | Resultado | Prioridad | Esfuerzo |
|---|---|---:|---:|
| 0 | Infraestructura de pruebas y línea base | P0 | M |
| 1 | Corrección de defectos funcionales confirmados | P0 | XL |
| 2 | Ciclo de vida, asincronía, estado e interop | P1 | XL |
| 3 | Accesibilidad y navegación con teclado | P1 | XL |
| 4 | Mejoras específicas por familia | P1/P2 | XL |
| 5 | Consolidación, API, documentación y paquete | P2 | L |
| 6 | Validación final y publicación | P0 de release | M |

---

## 5. Fase 0 — Infraestructura y línea base

### BAS-001 — Crear proyectos de pruebas

**Trabajo**

- Crear `tests/Dnet.Blazor.UnitTests` para servicios, algoritmos y modelos.
- Crear `tests/Dnet.Blazor.ComponentTests` con bUnit para renderizado, parámetros, eventos y ciclo de vida.
- Crear `tests/Dnet.Blazor.E2E` con Playwright para teclado, foco, overlays, Server y WebAssembly.
- Añadir los proyectos a la solución y documentar cómo ejecutarlos.

**Criterios de aceptación**

- `dotnet test` descubre y ejecuta los tres niveles de prueba.
- Un fallo de test produce salida diagnóstica útil y capturas en E2E.
- Los tests no dependen de orden, zona horaria ni cultura de la máquina.

### BAS-002 — Establecer la matriz de CI

**Trabajo**

- Compilar bibliotecas y ejemplos en una restauración limpia.
- Ejecutar tests unitarios y bUnit en cada PR.
- Ejecutar Playwright al menos en Chromium; Firefox y WebKit en validación nocturna o de release.
- Tratar advertencias de nulabilidad y analizadores como errores en los proyectos de biblioteca.
- Añadir timeout explícito y logs binarios de MSBuild al ejemplo WebAssembly para diagnosticar su build bloqueado.

**Criterios de aceptación**

- La misma revisión puede reproducirse localmente y en CI.
- El ejemplo WebAssembly termina o falla con un diagnóstico y nunca queda bloqueado indefinidamente.

### BAS-003 — Añadir pruebas de caracterización

Crear inicialmente pruebas que reproduzcan:

- Apertura y cierre concurrente de 1.500 overlays.
- Toast con duración de uno, dos y cinco segundos.
- Seleccionar y deseleccionar todos los elementos de List.
- Selección, deselección, eliminación y estado disabled de Chips.
- Todos los valores de `InputDateType`.
- Límites de Stepper y DynamicStepper.
- Ventana virtualizada con `_loadedItemsStartIndex` distinto de cero.
- Cambio de parámetros después del primer render en Grid, List, Tree, Select, Tabs y AdminLayout.
- Conservación de un valor enlazado en DatePickerWeek.
- Liberación de cada listener y referencia .NET tras desmontar un componente.

### BAS-004 — Añadir herramientas de calidad

- Activar analizadores .NET y reglas para detectar `async void`, parámetros no inicializados y disposables no liberados.
- Añadir axe-core a las pruebas E2E.
- Definir cobertura mínima inicial del 60 % para servicios y algoritmos; subirla al 80 % al finalizar la fase 4.
- Añadir una comprobación de nombres públicos obsoletos y documentación XML.

---

## 6. Fase 1 — Defectos funcionales confirmados

### 6.1 Overlay, Dialog y paneles conectados

#### OVR-001 — Identificadores de overlay sin colisiones

**Archivos principales:** [`OverlayService.cs`](../../src/Dnet.Blazor/Components/Overlay/Infrastructure/Services/OverlayService.cs), `DnetOverlay.razor`.

**Trabajo**

- Sustituir `new Random().Next(1000)` por un contador privado monotónico con `Interlocked.Increment`.
- Eliminar la variable local que oculta `_sequenceNumber`.
- Hacer idempotente `Detach`: cerrar un ID desconocido no debe afectar a otro overlay.
- Definir el comportamiento ante overflow del contador.

**Pruebas:** adjuntar/cerrar miles de overlays, orden no secuencial y cierres repetidos.

#### OVR-002 — Corregir ViewportRuler

**Archivo:** [`ViewportRuler.cs`](../../src/Dnet.Blazor/Components/Overlay/Infrastructure/Services/ViewportRuler.cs).

**Trabajo**

- Inicializar `_viewportSize` cuando sea `null` y actualizarlo después de resize.
- Conservar un único `DotNetObjectReference<ViewportRuler>`.
- Pasar la misma referencia al registro y desregistro JavaScript.
- Convertir suscripción y desuscripción en operaciones esperables, sin `Task.Run` ni tareas descartadas.
- Implementar `IAsyncDisposable` y liberar listener y referencia.

**Pruebas:** primera llamada, resize, varias suscripciones, última desuscripción y dispose.

#### OVR-003 — Corregir posicionamiento

**Archivo:** [`DnetOverlayPane.razor`](../../src/Dnet.Blazor/Components/Overlay/DnetOverlayPane.razor).

**Trabajo**

- Elegir correctamente entre overflow izquierdo/derecho y superior/inferior.
- Admitir offsets positivos y negativos.
- Generar un único `transform: translate(x, y)` para no sobrescribir un eje.
- Corregir la asignación de altura previa que actualmente utiliza Width.
- Probar márgenes, scroll, zoom, viewport pequeño, RTL y posiciones de fallback.

#### OVR-004 — Propagar la corrección a consumidores

Validar Dialog, ConnectedPanel, FloatingPanel, FloatingDoubleList, Select, Autocomplete, DatePicker, Toast y Tooltip con el nuevo contrato. No se cerrará esta tarea hasta que todos compartan la misma ruta de apertura, actualización y cierre.

### 6.2 Toast, Tooltip y Spinner

#### NTF-001 — Temporización de Toast

**Archivos:** [`DnetToast.razor`](../../src/Dnet.Blazor/Components/Toast/DnetToast.razor), [`ToastService.cs`](../../src/Dnet.Blazor/Components/Toast/Infrastructure/Services/ToastService.cs).

- Sustituir el temporizador por `PeriodicTimer` o una tarea cancelable basada en `Task.Delay`.
- Cerrar exactamente al alcanzar la duración y cancelar al cerrar manualmente o disponer.
- Corregir el mapeo de `ToastTypeIconClass`, `TypeIconClass`, `ToastTypeColor`, `CloseIconClass` y `ToastClass`.
- Mantener un tracker independiente por posición.
- Ignorar cierres de IDs desconocidos sin decrementar contadores.
- Verificar que todas las posiciones incorporan su desplazamiento y no se solapan.

#### NTF-002 — Seguridad de Tooltip

- Eliminar callbacks de `System.Threading.Timer` que mutan componentes desde un hilo ajeno al dispatcher.
- Utilizar tareas cancelables y `InvokeAsync` en el propietario del renderizado.
- Hacer que la referencia devuelta durante el retraso represente al overlay real y propague cierre/estado.
- Cubrir entrada/salida rápida, reapertura, dispose y múltiples tooltips.

#### NTF-003 — Debounce de Spinner

**Archivo:** [`DnetSpinner.razor`](../../src/Dnet.Blazor/Components/Spinner/DnetSpinner.razor).

- Reemplazar el timer repetitivo por un retraso de una sola ejecución cancelable.
- Separar contador de solicitudes y visibilidad.
- Impedir que el contador crezca en cada tick.
- Detener y disponer todos los recursos en `DisposeAsync`.

### 6.3 List y Chips

#### SEL-001 — Corregir selección total de List

**Archivo:** [`DnetList.razor`](../../src/Dnet.Blazor/Components/List/DnetList.razor).

- Añadir nodos a `_selectedRowNodes` solamente cuando `value` sea `true`.
- Excluir nodos disabled/no seleccionables.
- Mantener `_previousSelectedRowNode` y `_allNodesSelected` coherentes.
- Sustituir `async void` por `Task`.

#### SEL-002 — Modelo de selección de Chips

**Archivos:** [`DnetChip.razor`](../../src/Dnet.Blazor/Components/Chips/DnetChip.razor), [`DnetChipList.razor`](../../src/Dnet.Blazor/Components/Chips/DnetChipList.razor).

- No seleccionar, deseleccionar ni eliminar un chip disabled.
- Añadir a `_selectedChips` solo si queda seleccionado; retirar si queda deseleccionado.
- Evitar duplicados usando ID o referencia estable.
- Retirar un chip eliminado de la selección antes del callback.
- En modo single, garantizar como máximo una selección.

### 6.4 Formularios y botones

#### FRM-001 — Aplicar InputDateType

**Archivo:** [`DnetInputDate.razor`](../../src/Dnet.Blazor/Components/Form/DnetInputDate.razor).

- Renderizar `type="@_typeAttributeValue"` en ambas ramas.
- Validar formatos y conversiones de `date`, `datetime-local`, `month` y `time` en varias culturas.
- Añadir pruebas de valor vacío, inválido, mínimo y máximo.

#### FRM-002 — Fortalecer DnetButton

**Archivo:** [`DnetButton.razor`](../../src/Dnet.Blazor/Components/Button/DnetButton.razor).

- Tratar `AdditionalAttributes` nulo como diccionario vacío.
- Usar `type="button"` por defecto, permitiendo que el consumidor lo sobrescriba.
- Recalcular clases en `OnParametersSet`, no solo en `OnInitialized`.
- Devolver y esperar el `Task` de `OnClick`.

### 6.5 Grid

#### GRD-001 — Corregir ventana virtualizada

**Archivo:** [`VirtualizationOperations.cs`](../../src/Dnet.Blazor/Components/Grid/BlgGrid/VirtualizationOperations.cs).

- Calcular `Take(lastItemIndex - _itemsBefore)` después de aplicar `Skip`.
- Normalizar límites inclusivos/exclusivos y evitar cantidades negativas.
- Cubrir buffers parciales, primera/última página y actualizaciones concurrentes del proveedor.

#### GRD-002 — Corregir refresco de áreas pinned

**Archivo:** [`SelectionOperations.cs`](../../src/Dnet.Blazor/Components/Grid/BlgGrid/SelectionOperations.cs).

- Refrescar pinned left y pinned right una sola vez.
- Verificar selección total, selección parcial, grupos y filas sin datos.

#### GRD-003 — Corregir actualización de definiciones

- `SetColumnDefsAsync` debe validar el argumento nuevo, no el valor anterior de `GridColumns`.
- Los eventos de filtro simple y avanzado deben conservar contratos diferenciados; documentar o corregir `EnableServerSideFilter`.
- Esperar callbacks de filtro, ordenación, agrupación, paginación y selección.

### 6.6 Stepper y calendarios semanales

#### STP-001 — Límites de navegación

**Archivo:** [`DnetDynamicStepper.razor`](../../src/Dnet.Blazor/Components/DynamicStepper/DnetDynamicStepper.razor).

- Hacer `NextStep` y `PreviousStep` no-op en los extremos.
- Aplicar `CanNavigateToStep`, `Linear`, `Completed` y `Editable` también a clicks directos.
- Esperar `OnSelectionChange`.
- Sustituir el ID fijo `stepperContainer` por uno único por instancia y reaccionar a resize.

#### STP-002 — Render único del contenido vertical

**Archivo:** [`DnetStepper.razor`](../../src/Dnet.Blazor/Components/Stepper/DnetStepper.razor).

- Renderizar cada `ChildContent` solamente en su paso o renderizar una única región activa.
- No usar `visibility:hidden` para mantener instancias duplicadas en layout.
- Mantener estado de componente de forma explícita al cambiar de paso.

#### CAL-001 — Conservar el valor enlazado de DatePickerWeek

**Archivos:** `Components/DatePickerWeek/*`, `Components/DatePickerWeekRaw/*`.

- No sustituir un valor no vacío durante `OnInitialized`.
- Definir qué ocurre cuando el valor inicial es vacío: vacío por defecto o semana actual mediante parámetro explícito.
- Limpiar flags de selección antes de seleccionar otra semana.
- Hacer Reset seguro cuando el día de hoy no esté en el mes visible.
- Cubrir cambio de año, semana que cruza año y límites Min/Max.

### 6.7 Material

#### MAT-001 — Registro y ciclo de vida

- Añadir una implementación de `Dnet.Blazor.Material.Components.FormField.IFormEventService` o reutilizar explícitamente el contrato del proyecto principal.
- Añadir `AddDnetBlazorMaterial(IServiceCollection)` y documentar su orden respecto a `AddDnetBlazor`.
- Declarar `IDisposable` en `DnetFormFieldCmp` y retirar todas las suscripciones, incluida cualquier nueva suscripción futura.
- Resolver las advertencias de nulabilidad sin utilizar supresiones indiscriminadas.

---

## 7. Fase 2 — Ciclo de vida, asincronía, estado e interop

### LIF-001 — Unificar el patrón Dispose de formularios

**Trabajo**

- Convertir la limpieza de [`DnetInputBase`](../../src/Dnet.Blazor/Infrastructure/Forms/DnetInputBase.cs) en un patrón `protected virtual Dispose(bool)`/`DisposeAsyncCore` que los derivados puedan encadenar explícitamente.
- Revisar DnetInputText, DnetInputNumber, DnetInputTextArea, DnetInputDate, Autocomplete y Select.
- Liberar timers, CTS, eventos de `EditContext`, eventos de FormEventService y referencias de overlays.
- Retirar `OnCharCount` en `DnetFormField.Dispose`.
- Guardar delegates usados como lambdas para poder desuscribirlos.

**Criterio:** desmontar y volver a montar cada input no aumenta suscriptores ni produce callbacks posteriores al dispose.

### LIF-002 — Eliminar callbacks duplicados

- Auditar las asignaciones a `CurrentValue`/`CurrentValueAsString` seguidas de `ValueChanged.InvokeAsync`.
- Mantener una única fuente de notificación por cambio.
- Definir por separado eventos semánticos como `OnClearInput`.
- Probar una sola llamada por interacción y por actualización programática.

### ASY-001 — Eliminar `async void`

- Convertir los 47 casos identificados a `Task`.
- En handlers Razor usar `Func<Task>` directamente.
- En eventos .NET obligatoriamente `void`, delegar a un método `Task`, capturar errores y cancelar en dispose.
- No descartar `InvokeAsync`, `ValueTask` de JS ni `EventCallback.InvokeAsync`.
- Añadir `CancellationToken` a búsquedas, debounce y operaciones que puedan solaparse.

### STA-001 — Sincronización de parámetros

Revisar componentes que copian parámetros solo durante la inicialización:

| Componente | Cambio requerido |
|---|---|
| Grid | Reaccionar a nuevas referencias de `GridData`, `GridColumns` y `GridOptions`; definir API para mutaciones in-place. |
| List | Reprocesar `Items` y opciones cuando cambien; conservar selección por clave estable. |
| Tree | Recalcular nodos cuando cambie `Nodes`; soportar alta/baja dinámica. |
| Select/Autocomplete | Invalidar cachés al cambiar Items, propiedades o comparadores. |
| Tabs/Stepper | Sincronizar `SelectedTabId`/`SelectedStepId` y retirar hijos desmontados. |
| AdminLayout | Reaccionar a parámetros y resize; no consultar el valor original después de actualizar estado interno. |
| Button/RadioButton/Checkbox | Recalcular clases y atributos derivados en `OnParametersSet`. |

Para colecciones, documentar si se requiere referencia nueva o añadir un método explícito de actualización. No mezclar ambos contratos silenciosamente.

### INT-001 — Propiedad y liberación de interop

**Grid**

- Guardar los dos `DotNetObjectReference` creados por la grid.
- Hacer que `addTouchListeners` y `addWindowEventListeners` devuelvan un token/handle.
- Implementar las funciones JS de retirada y llamarlas en `DisposeAsync`.
- Mover `touching`, `moved`, `lastTapTime` y `touchStart` a estado por instancia.
- Retirar los `console.log` de producción.

**Overlay**

- Completar o eliminar `addKeyDownEventListener`, actualmente declarado en C# pero ausente en JavaScript.
- Desconectar observers/listeners al cerrar el último overlay.

**ImageEditor**

- Conservar suscripciones RxJS y listeners de drag para poder liberarlos.
- Implementar realmente `dnetimageeditor.dispose`.
- Hacer que `ImageEditorContent` implemente `IAsyncDisposable` y libere `_jsInterop`.

**Criterio:** una prueba repetirá 100 montajes/desmontajes y verificará que no quedan handles ni invocaciones a referencias .NET dispuestas.

### SVC-001 — Contratos de servicios

- Evitar casts de interfaces a implementaciones concretas como `((SpinnerService)SpinnerService)`.
- Exponer suscripción, actualización y dispose en las interfaces.
- Documentar lifetimes DI y verificar aislamiento entre componentes y circuitos Blazor Server.
- Evitar estado mutable compartido salvo que sea intencionado y thread-safe.

---

## 8. Fase 3 — Accesibilidad WCAG 2.2 AA

### A11Y-001 — Infraestructura de pruebas

- Añadir axe-core en páginas representativas de cada familia.
- Crear helpers E2E para tabulación, Escape, Enter, Space, flechas, Home y End.
- Probar con escala de navegador al 200 %, modo alto contraste y reducción de movimiento.
- Establecer cero violaciones críticas o serias como gate de CI.

### A11Y-002 — Formularios

- Asociar todos los `label` mediante `for`/`id` o envolver correctamente el control.
- Conectar hint, error y contador mediante `aria-describedby`.
- Añadir `aria-invalid`, `aria-required` y estados disabled/readonly coherentes.
- Convertir clear, prefix y suffix interactivos en botones accesibles.
- Conservar foco y anunciar errores sin duplicar mensajes.

### A11Y-003 — Select y Autocomplete

- Implementar el patrón `combobox` + `listbox` + `option`.
- Gestionar `aria-expanded`, `aria-controls`, `aria-activedescendant` y `aria-selected`.
- Soportar flechas, Enter, Escape, Home, End y escritura incremental.
- Restaurar foco al cerrar y desplazar la opción activa a la vista.

### A11Y-004 — Tabs, Stepper y ExpansionPanel

- Tabs: `tablist`, `tab`, `tabpanel`, roving tabindex y flechas según orientación.
- Stepper: botones reales, estado actual, disabled y descripción de errores/completado.
- ExpansionPanel: botón de cabecera, `aria-expanded`, `aria-controls` y región identificada.
- El contenido oculto no debe permanecer enfocable ni ocupar layout accidentalmente.

### A11Y-005 — Tree, List y Grid

- Tree: patrón `tree`/`treeitem`/`group`, niveles, expanded, selected y navegación jerárquica.
- List: roles según sea lista estática, listbox o grid; excluir disabled de selección.
- Grid: roles de fila/celda/columnheader, orden anunciado, selección y navegación de celda por teclado.
- Añadir nombres accesibles a sort, filter, resize y select-all.

### A11Y-006 — Calendarios

- Implementar `grid`, `row`, `gridcell` y nombres completos de fecha localizados.
- Gestionar fecha activa separada de fecha seleccionada.
- Soportar flechas, PageUp/PageDown, Home/End, Enter, Escape y foco al cambiar de vista.
- Anunciar cambios de mes/año y restricciones Min/Max.

### A11Y-007 — Dialog, Overlay y feedback

- Dialog: `role="dialog"`, `aria-modal`, nombre accesible, focus trap, foco inicial y restauración.
- Cerrar con Escape solo cuando la configuración lo permita.
- Toast: `role="status"` o `alert` según severidad y región `aria-live`.
- Spinner: `role="status"`, texto accesible y `aria-busy` sobre la región afectada.
- Tooltip: contenido descriptivo asociado por `aria-describedby`, sin depender solo de hover.

### A11Y-008 — Chips y botones de icono

- Usar botones nativos para selección/eliminación o aplicar el patrón equivalente completo.
- Proporcionar nombre accesible a iconos de cierre, calendario, navegación y menú.
- Garantizar target mínimo y foco visible.

---

## 9. Fase 4 — Mejoras específicas por familia

Esta fase reúne trabajos que no son necesariamente bloqueantes por sí solos, pero completan la cobertura de todos los componentes.

| Familia | Implementación prevista | Validación mínima |
|---|---|---|
| AdminLayout | Suscribirse a resize, mantener `DeviceType` actualizado, usar el estado interno de columnas y liberar ThemeMessageService. | Cambio de tamaño y montaje repetido. |
| Autocomplete | Soportar uso fuera de `EditForm` o fallar con mensaje claro; conversión genérica de TValue; cancelar búsquedas; invalidar caché; no tragar excepciones. | Form/no form, resultados tardíos, Items cambiantes. |
| Button | Null safety, tipo button y parámetros reactivos. | Dentro y fuera de formularios. |
| Checkbox | Revisar secuencias de RenderTreeBuilder, propagación disabled y atributos ARIA. | Diff repetido y validación de formulario. |
| Chips | Modelo de selección coherente, disabled y teclado. | Single/multi/remove. |
| ConnectedPanel | Heredar posicionamiento y dispose corregidos; validar pérdida del elemento origen. | Scroll, resize y origen desmontado. |
| DatePicker | Usar Culture también en panel; null safety en SelectedDay; unificar parseo y formatos. | Varias culturas y límites. |
| DatePickerWeek | No sobrescribir valor; corregir selección/reset; usar motor común. | Semana normal y cruce de año. |
| DatePickerWeekRaw | Aplicar las mismas correcciones sin duplicar algoritmo. | Paridad con DatePickerWeek. |
| Dialog | Focus trap, Escape, restore focus, cierre idempotente y callbacks esperados. | Anidamiento y cierre externo. |
| DynamicStepper | Límites, reglas de navegación, ID único y resize. | 0, 1 y múltiples pasos. |
| ExpansionPanel | Registro/desregistro dinámico, callbacks esperados y semántica accesible. | Añadir/quitar paneles. |
| FloatingDoubleList | Validar selección y estado de ambos paneles tras cambios de Items; limpiar overlay. | Mover, filtrar y cerrar. |
| FloatingPanel | Posicionamiento, cierre, foco y dispose compartidos con Overlay. | Click externo y Escape. |
| Form | Dispose encadenado, eventos sin duplicados y uso consistente de EditContext. | Validación, clear y desmontaje. |
| Grid | Parámetros reactivos, virtualización, listeners por instancia, eventos esperados, roles y rendimiento. | 100k filas virtuales, pinned y táctil. |
| ImageEditor | Aplicar AllowedFormats/MaxFileSizes/controles; escalar coordenadas a tamaño natural; devolver original si no hay cambios; conservar formato/transparencia cuando proceda. | PNG/JPEG, imagen escalada y cancelar/aceptar. |
| List | Parámetros reactivos, selección, disabled, paginación segura y búsqueda con comparación ordinal/culture configurada. | Datos vacíos y página fuera de rango. |
| Overlay | IDs, viewport, posiciones, listeners, teclado y foco. | Matriz de consumidores. |
| Paginator | Normalizar página tras cambiar total/tamaño; impedir rangos negativos; callbacks esperados. | 0 elementos y última página parcial. |
| RadioButton | Sincronizar parámetros, nombre de grupo estable, disabled y teclado nativo. | Grupos dinámicos. |
| Select | Conversión genérica, caché por versión de datos, unsubscribe completo, callbacks únicos y patrón listbox. | Tipos primitivos/objetos y multi. |
| Spinner | Debounce one-shot, dispose y semántica status. | Solicitudes solapadas. |
| Stepper | Contenido vertical único, parámetros reactivos y reglas Editable/Linear. | Cambio externo de paso. |
| Tabs | Retirar tabs desmontadas, reaccionar a SelectedTabId/resize y patrón ARIA. | Tabs dinámicas y overflow. |
| Toast | Temporización, estilos, posiciones, tracker y aria-live. | Varios corners simultáneos. |
| Tooltip | Dispatcher, referencia diferida, cancelación, hover/focus y aria-describedby. | Entrada/salida rápida. |
| Tree | Parámetros reactivos, Children null-safe, una sola evaluación de ChildNodes, selección parental/indeterminada y teclado. | Árbol lazy y mutaciones. |
| Virtualize | Comparar con la implementación oficial de Blazor y consolidar la copia usada por Grid. | Altura variable y resize. |
| Material | Registro DI, nulabilidad, dispose y reutilización de contratos comunes. | Aplicación mínima solo con extensión DI. |

### IMG-001 — Corrección detallada de ImageEditor

- Convertir coordenadas del crop desde `getBoundingClientRect` a `naturalWidth`/`naturalHeight` antes de procesar.
- Aplicar `AllowedFormats` y `MaxFileSizes` antes de crear el data URL.
- Renderizar solamente los controles habilitados en `ImageEditingControls`.
- Si el usuario confirma sin editar, devolver el stream original o una copia con propiedad documentada.
- No convertir siempre a JPEG: conservar PNG/transparencia o permitir seleccionar formato/calidad.
- Definir propiedad y dispose de streams para evitar fugas de memoria.
- Probar imágenes EXIF rotadas, dimensiones grandes y límites del buffer WASM.

### DAT-001 — Robustecer filtrado, ordenación y paginación

- Sustituir `ToUpper`/`ToLower` por comparaciones con `StringComparison` o comparador configurable.
- Validar propiedades de búsqueda y filtros nulas antes de ejecutar.
- Cachear accessors/reflection compilada por tipo y propiedad.
- Definir estabilidad del orden y comportamiento de valores null.
- Normalizar página actual cuando filtros reducen el conjunto de datos.

---

## 10. Fase 5 — Consolidación, API, documentación y paquete

### REF-001 — Unificar motores de fecha

- Extraer un único servicio/algoritmo para generar días, semanas, meses y años.
- Eliminar los tres `DnetInputBase` duplicados de DatePicker, DatePickerWeek y DatePickerWeekRaw.
- Centralizar Culture, primer día de semana, calendario y formato de intervalo.
- Considerar `DateOnly`/tipos de rango para el modelo interno sin romper la API 5.x.

### REF-002 — Consolidar Virtualize y CssBuilder

- Comparar las dos implementaciones Virtualize con `Microsoft.AspNetCore.Components.Web.Virtualization`.
- Mantener una sola implementación propia únicamente si aporta funciones verificadas que la oficial no cubre.
- Dejar un único `CssBuilder` compartido entre biblioteca principal, Grid y Material.

### API-001 — Corregir nombres públicos

Crear nombres correctamente escritos y mantener temporalmente aliases obsoletos para, entre otros:

- `CellCliked`, `TabCliked` y otros `Cliked` → `Clicked`.
- `Sufix` → `Suffix`.
- `Excution` → `Execution`.
- `Postion` → `Position`.
- `Backgroung` → `Background`.
- `EnablePagingination` → `EnablePagination`.
- `AlingContent` → `AlignContent`.
- `ConverToAdvancedFilter` → `ConvertToAdvancedFilter`.
- `GeElementReference` → `GetElementReference`.
- `Udate...` → `Update...`.

**Política**

- Alias marcado `[Obsolete("Use ... instead")]` en 5.x.
- Warning y guía de migración durante al menos una versión menor.
- Eliminación solamente en la siguiente versión mayor.

### API-002 — Normalizar contratos

- Definir nullability real de todos los parámetros públicos.
- Añadir valores predeterminados seguros o `[EditorRequired]` cuando corresponda.
- Usar `EventCallback<T>` coherente y esperar siempre `InvokeAsync`.
- Documentar si un componente admite uso fuera de `EditForm`/`DnetFormField`.
- Añadir comparadores/keys configurables a componentes genéricos en lugar de depender de reflexión repetida.

### DOC-001 — Documentación por componente

Para cada una de las 29 familias crear:

1. Propósito y ejemplo mínimo.
2. Parámetros, eventos y tipos genéricos.
3. Integración con EditForm y DI.
4. Ejemplos de actualización dinámica.
5. Accesibilidad y teclado.
6. Rendimiento, virtualización y límites.
7. Errores frecuentes y migración desde nombres obsoletos.

Además:

- Actualizar README para retirar “no documentation available”.
- Alinear versión del README, csproj y changelog.
- Crear una tabla de compatibilidad Server/WebAssembly y frameworks soportados.
- Documentar el registro `AddDnetBlazor` y `AddDnetBlazorMaterial`.

### PKG-001 — Paquete y compatibilidad

- Decidir y documentar si se mantiene solo `net10.0` o se multitargetean versiones LTS compatibles.
- Revisar los 3,9 MB de assets de `wwwroot`, WASM, RxJS y duplicados de `Components/Assets`.
- Verificar licencia y procedencia de cada dependencia JavaScript empaquetada.
- Generar paquete en CI, instalarlo en una aplicación limpia y ejecutar smoke tests.
- Añadir metadata NuGet, símbolos y documentación XML si todavía faltan.

---

## 11. Fase 6 — Validación y publicación

### REL-001 — Matriz de regresión

Ejecutar para Blazor Server y WebAssembly:

- Chrome/Edge, Firefox y Safari/WebKit.
- Escritorio, viewport móvil y entrada táctil.
- Cultura `es-ES`, `en-US` y una cultura con distinto primer día de semana.
- Teclado completo, axe-core y al menos una pasada manual con lector de pantalla.
- Formularios válidos/invalidos, navegación dinámica y varios overlays simultáneos.

### REL-002 — Rendimiento

- Medir render y asignaciones de Grid/List con 1k, 10k y 100k filas virtualizadas.
- Medir apertura/cierre repetido de overlays y memoria después de GC.
- Medir búsquedas de Autocomplete/Select y evitar compilación de expresiones por interacción.
- Definir presupuestos y fallar CI solo ante regresiones estadísticamente significativas.

### REL-003 — Release candidate

- Publicar prerelease `5.x.0-rc.1`.
- Validar una aplicación limpia desde NuGet, sin referencias al código fuente.
- Ejecutar la guía de migración sobre las muestras actuales.
- Congelar nuevas funcionalidades hasta cerrar regresiones del RC.
- Publicar la versión estable con changelog, breaking changes, limitaciones conocidas y fecha de retirada de aliases.

---

## 12. Secuencia propuesta de PR

1. **PR-01:** proyectos de tests, CI, build WebAssembly diagnosticable y pruebas de caracterización.
2. **PR-02:** Overlay IDs, ViewportRuler y posicionamiento.
3. **PR-03:** Toast, Tooltip y Spinner.
4. **PR-04:** List, Chips y correcciones de selección del Grid.
5. **PR-05:** InputDate, DatePickerWeek, Stepper y DynamicStepper.
6. **PR-06:** patrón Dispose de formularios, callbacks únicos y eliminación de `async void`.
7. **PR-07:** limpieza completa de interop para Grid, Overlay e ImageEditor.
8. **PR-08:** sincronización de parámetros y colecciones dinámicas.
9. **PR-09:** accesibilidad de formularios, dialog, overlay y feedback.
10. **PR-10:** accesibilidad de Select, Autocomplete, Tabs, Tree, Grid y calendarios.
11. **PR-11:** consolidación de fechas/virtualización/CssBuilder y aliases de API.
12. **PR-12:** documentación, paquete, matriz de release y release candidate.

Los PR-03 y PR-04 pueden ejecutarse en paralelo después de PR-02. PR-09 y PR-10 pueden repartirse por familias después de que los contratos de estado y ciclo de vida estén estabilizados.

## 13. Gates por PR

Todo PR deberá incluir:

- Referencia a uno o más IDs de este plan.
- Prueba que falla antes y pasa después para cada defecto.
- Build limpio de las bibliotecas afectadas.
- Cero nuevas advertencias.
- Revisión de nulabilidad y dispose.
- Prueba de teclado/axe si cambia markup interactivo.
- Captura o vídeo breve cuando cambie comportamiento visual.
- Nota de migración si cambia API o semántica observable.

## 14. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Corregir notificaciones duplica o elimina callbacks esperados por consumidores actuales. | Pruebas de caracterización, changelog y opción de compatibilidad solo si existe uso documentado. |
| Cambiar markup por accesibilidad rompe selectores CSS. | Mantener clases existentes, añadir tests visuales y migrar selectores gradualmente. |
| Dispose de interop intenta llamar JS durante desconexión de un circuito Server. | Capturar únicamente `JSDisconnectedException` en dispose y liberar siempre referencias .NET. |
| Unificar DatePicker altera semanas/culturas. | Casos golden por cultura y comportamiento configurable para primer día de semana. |
| Multitarget aumenta complejidad del paquete. | Tomar decisión explícita en PKG-001 y mantener una matriz pequeña soportada. |
| Refactor de Grid introduce regresiones de rendimiento. | Benchmarks antes/después y PRs separados para corrección y optimización. |

## 15. Checklist final

- [ ] Fase 0 completada.
- [ ] Todos los defectos P0 reproducidos y corregidos.
- [ ] Cero `async void` no justificados.
- [ ] Cero listeners, timers o referencias JS sin propietario.
- [ ] Parámetros dinámicos cubiertos por pruebas.
- [ ] Las 29 familias tienen navegación por teclado documentada.
- [ ] Cero violaciones axe críticas/serias en páginas representativas.
- [ ] Material tiene registro DI y cero warnings de nulabilidad.
- [ ] Build Server y WebAssembly reproducible.
- [ ] Documentación y guía de migración publicadas.
- [ ] Paquete RC validado desde una aplicación limpia.
- [ ] Changelog y versiones alineados.

