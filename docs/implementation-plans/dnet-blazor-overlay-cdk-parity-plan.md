# Plan de implementación: estabilización y evolución de Overlay inspirada en Angular CDK

**Estado:** implementado en `codex/overlay-cdk-parity`
**Fecha:** 2026-08-20  
**Ámbito:** `src/Dnet.Blazor/Components/Overlay`, sus consumidores, JavaScript interop, pruebas de navegador y documentación  
**Referencia funcional:** [Angular CDK Overlay](https://github.com/angular/components/tree/main/src/cdk/overlay)  
**Planes relacionados:** [`dnet-blazor-stabilization-plan.md`](./dnet-blazor-stabilization-plan.md) y [`dnet-blazor-styling-architecture-plan.md`](./dnet-blazor-styling-architecture-plan.md)

## Estado de implementación

- Viewport, teclado, puntero, scroll y observación de tamaño tienen listeners
  compartidos por circuito, con handles estables y limpieza segura frente a
  `DotNetObjectReference` ya liberadas.
- `OverlayScrollStrategy` incorpora `Noop`, `Reposition`, `Close` y `Block`.
  Los eventos de scroll se coalescen por frame y distinguen el scroll interno
  del propio panel.
- `OverlayReference` expone estado adjunto, detach/dispose idempotente,
  reposición y actualización de tamaño. La reposición está serializada por
  overlay y descarta resultados desfasados.
- Se incorporaron dispatcher para Escape/clic exterior, ARIA configurable,
  focus trap/restauración opt-in y host compatible con Fullscreen API.
- La cobertura incluye pruebas unitarias de lifecycle, scheduler y listeners,
  además de una prueba Playwright opt-in para diálogo modal.

## 1. Propósito

Consolidar Overlay como infraestructura común para `Dialog`, `Select`, `Autocomplete`, `Tooltip`, `ConnectedPanel`, `FloatingPanel`, `DatePicker`, `FloatingDoubleList` y futuros componentes flotantes. El objetivo no es copiar Angular ni introducir un sistema de portales ajeno a Blazor, sino adoptar los contratos que hacen fiable al CDK:

1. Un ciclo de vida explícito y seguro para cada overlay.
2. Un único origen de eventos globales por circuito: viewport, scroll, teclado y puntero.
3. Posicionamiento conectado que se actualice de forma predecible y sin trabajo redundante.
4. Estrategias configurables para el scroll.
5. Accesibilidad y orden de apilado correctos para diálogos y paneles anidados.
6. Pruebas que detecten listeners huérfanos, referencias .NET liberadas, fugas y regresiones visuales.

## 2. Estado actual verificado

### Capacidades que se deben preservar

- Contenedor global `DnetOverlay` y backdrops por overlay.
- Posicionamiento global y conectado mediante `OverlayConfig`.
- Lista ordenada de posiciones de fallback, márgenes de viewport, dimensiones flexibles, `GrowAfterOpen`, `WithPush`, posición bloqueada, offsets, RTL y `transform-origin`.
- Propagación del scope de tema mediante `OverlayConfig.ThemeScope`.
- Identificadores monotónicos e idempotencia básica al cerrar en `OverlayService`.
- Limpieza race-safe recién introducida en `ViewportRuler` para no invocar referencias .NET ya liberadas al abrir Developer Tools.

### Hallazgos a corregir

| Área | Evidencia | Consecuencia |
|---|---|---|
| Lifetime de viewport | `IViewportRuler` se registra como `Transient`; `DnetOverlay` y cada `DnetOverlayPane` obtienen instancias distintas. | Varios listeners globales `resize`, más interop y render que los necesarios. |
| Scroll | `WithScrollableContainers` conserva metadatos, pero no registra ni administra listeners de `scroll`. | Un panel conectado puede quedar desalineado, visible cuando su trigger desapareció o no bloquear el documento cuando corresponde. |
| Teclado | `DnetOverlayInterop` declara `AddKeyDownEventListener`, pero `dnet-overlay.js` no la implementa. | No hay contrato central para Escape ni para entregar teclado al overlay superior. |
| Clic exterior | Sólo se maneja el backdrop. | Select, tooltip y paneles sin backdrop no comparten un cierre fiable por clic exterior. |
| Referencia | `OverlayReference` sólo expone `Close`. | No hay estado, eventos de attach/detach, actualización de tamaño/posición ni una operación de dispose inequívoca. |
| Reposición | Cada panel conectado puede iniciar cálculos async por resize independientemente. | Riesgo de cálculos solapados, reflows y aplicación de estilos fuera de orden. |
| Accesibilidad | El contrato de configuración no modela dirección ni atributos ARIA/foco. | Dialogs modales y paneles complejos dependen de implementaciones ad hoc. |
| Fullscreen | El host siempre vive en el contenedor global. | Un overlay puede no ser visible si su aplicación entra en Fullscreen API. |

### Defectos concretos que se incluirán

- En `DnetOverlayPane.GetStyles`, `margin-bottom` toma `OverlayConfig.MarginTop`; debe tomar `MarginBottom`.
- El interop de teclado declarado y no implementado se elimina o se implementa dentro del dispatcher central; no debe quedar una API muerta.
- El recurso de JS de Overlay no debe depender de que RxJS esté disponible globalmente para tareas tan básicas como `resize`, `scroll` y eventos de puntero.

## 3. Principios y límites

- La API pública existente seguirá funcionando durante la serie 6.x. Las APIs nuevas se incorporarán de forma aditiva y las obsoletas se marcarán con `[Obsolete]` antes de retirarse en una versión mayor.
- Un overlay debe recibir eventos globales sólo mientras esté adjunto. Cerrar, desmontar o cancelar una apertura debe liberar todos sus recursos.
- Los listeners globales se registrarán una sola vez por circuito y se retirarán al no quedar consumidores.
- Las rutas de alta frecuencia no deben provocar un render Blazor por evento: se coalescerán en JavaScript por `requestAnimationFrame` y se notificará a .NET sólo cuando se requiera una decisión semántica.
- La responsabilidad de focus trap y restauración de foco será opt-in para diálogos modales; un tooltip o select no debe adquirir semántica modal por defecto.
- La posición debe calcularse contra `visualViewport` cuando esté disponible y degradar correctamente a `documentElement.clientWidth/clientHeight`.
- No se cambiarán las clases `cdk-*` existentes en este trabajo; su reprefijado es deuda separada de la arquitectura de estilos.

## 4. Objetivos medibles

| Métrica | Objetivo |
|---|---:|
| Listeners globales de `resize` con 20 overlays abiertos | 1 |
| Listeners globales de `scroll`, teclado y puntero | 0 sin overlays que los requieran; 1 de cada tipo con consumidores |
| Callbacks .NET por ráfaga de 20 eventos de resize/scroll | máximo 1 por overlay que use estrategia `Reposition` |
| Callbacks activos después de cerrar el último overlay | 0 |
| Excepciones de `DotNetObjectReference` liberada al abrir Developer Tools | 0 |
| Escape | sólo se entrega al overlay superior elegible |
| Click exterior | no se dispara para un gesto iniciado dentro del panel y liberado fuera |
| Reposición conectada tras scroll/resize | antes del siguiente frame visible, sin escrituras fuera de orden |
| Pruebas browser Overlay | Chromium, Firefox y WebKit |

## 5. Fase 0 — Baseline y contrato de pruebas

### OVR-CDK-001 — Laboratorio de escenarios de overlay

**Archivos principales**

- `samples/Dnet.Shared/Pages/Overlay.razor` o una página de laboratorio no enlazada en navegación.
- `tests/Dnet.Blazor.BrowserTests/Overlay*`.

**Trabajo**

- Crear escenarios reproducibles para overlay global, conectado, sin backdrop, con backdrop, múltiples overlays, anidado, RTL, viewport móvil, scroll de documento y scroll de contenedor.
- Exponer un contador de diagnóstico sólo en builds de prueba: listeners registrados, overlays adjuntos, reposiciones solicitadas/aplicadas y descartadas.
- Añadir captura de consola y trace Playwright cuando falle una prueba de interop.

**Criterios de aceptación**

- La suite puede detectar una referencia .NET liberada, listener residual y orden de overlay incorrecto.
- Las pruebas no dependen de `Task.Delay` fijos, salvo un timeout máximo de protección.

### OVR-CDK-002 — Matriz de consumidores

Documentar para cada consumidor la estrategia deseada, comportamiento de Escape, clic exterior, backdrop y foco.

| Consumidor | Scroll por defecto | Escape | Clic exterior | Foco modal |
|---|---|---|---|---|
| Dialog | `Block` | cierra si no está deshabilitado | backdrop configurable | sí, configurable |
| Select / Autocomplete | `Reposition` + autocierre si se pierde el origen | cierra | sí | no |
| Tooltip | `Reposition` | no obligatorio | cierra | no |
| Connected/Floating panel | `Reposition` | configurable | configurable | no por defecto |
| DatePicker | `Reposition` | cierra | configurable | no por defecto |

## 6. Fase 1 — Núcleo compartido: viewport, scheduling e interop

### OVR-CDK-101 — Convertir `ViewportRuler` en servicio único por circuito

**Archivos principales**

- `Infrastructure/Services/ServiceCollectionExtensions.cs`
- `Components/Overlay/Infrastructure/Interfaces/IViewportRuler.cs`
- `Components/Overlay/Infrastructure/Services/ViewportRuler.cs`
- `Components/Overlay/dnet-overlay.js`

**Trabajo**

- Registrar `IViewportRuler` como `Scoped`.
- Reemplazar el modelo de un listener por suscriptor por un único registro JS que difunda los cambios a los suscriptores C#.
- Mantener el cierre race-safe: si se quita el último suscriptor mientras la operación de alta está en vuelo, el listener y el `DotNetObjectReference` deben eliminarse al concluir el alta.
- Añadir `visualViewport.resize` y `visualViewport.scroll`, cuando existan, y normalizar la medida efectiva de viewport.
- Cachear el último tamaño y exponerlo sin un round-trip JS cuando sea válido.
- Implementar `IAsyncDisposable` como ruta primaria de limpieza; `Dispose` será un fallback seguro y sin tareas huérfanas.

**Criterios de aceptación**

- Abrir 20 overlays conectados deja exactamente un listener global de viewport.
- Abrir/cerrar rápido, navegar entre páginas y abrir Developer Tools no produce callbacks a referencias liberadas.
- No hay listener tras disponer el scope/circuito.

### OVR-CDK-102 — Scheduler de reposición por frame

**Archivos principales**

- Nuevo `OverlayPositionScheduler.cs` e interfaz asociada.
- `DnetOverlayPane.razor`, `DnetOverlayHost.razor`, `dnet-overlay.js`.

**Trabajo**

- Centralizar solicitudes de posición por `OverlayReferenceId`.
- Coalescer resize, scroll, cambios de tamaño y peticiones manuales; una única operación por overlay y frame.
- Versionar cada solicitud y descartar el resultado de cálculos async obsoletos o de un overlay ya cerrado.
- Separar lectura DOM de escritura de estilos para evitar layout thrashing.
- Exponer `RequestUpdatePosition` en la futura referencia del overlay.

**Criterios de aceptación**

- Una ráfaga de eventos sólo aplica la última posición.
- Cerrar un overlay durante el cálculo no produce excepción ni escritura posterior.
- Los paneles conectados no saltan entre posiciones por resultados que llegan fuera de orden.

### OVR-CDK-103 — Interop modular y sin dependencias implícitas

**Trabajo**

- Sustituir la dependencia de `window.rxjs` por listeners nativos, `AbortController` y `requestAnimationFrame`/timeout controlado.
- Devolver handles o usar identificadores estables para registrar y liberar listeners; nunca usar un objeto .NET como key sin una ruta de limpieza inequívoca.
- Añadir tratamiento de promesas rechazadas por dispose: no dejar `invokeMethodAsync` sin `catch`.
- Retirar `AddKeyDownEventListener` hasta que exista el dispatcher de la fase 3, o implementarlo sólo a través de éste.

**Criterios de aceptación**

- Overlay funciona aunque RxJS no haya sido cargado por la aplicación consumidora.
- JS no genera `Unhandled promise rejection` durante un dispose o una navegación.

## 7. Fase 2 — Estrategias de scroll

### OVR-CDK-201 — Contrato `IOverlayScrollStrategy`

**Archivos principales**

- Nuevo `Infrastructure/Scroll/IOverlayScrollStrategy.cs`.
- Nuevo `OverlayScrollStrategyOptions.cs`.
- `OverlayConfig.cs`, referencia de overlay y consumidores.

**Trabajo**

- Introducir los métodos `Attach`, `Enable`, `Disable`, `Detach` y `DisposeAsync`.
- Añadir `ScrollStrategy` a `OverlayConfig` de modo opcional, sin romper la configuración actual.
- Asegurar que una estrategia sólo puede estar adjunta a una referencia a la vez.
- Activarla tras attach real y desactivarla antes de detach/dispose.

### OVR-CDK-202 — `Reposition`, `Close`, `Block` y `Noop`

**Trabajo**

- `Reposition`: escuchar documento y contenedores registrados, solicitar reposición al scheduler y admitir throttle/coalescing configurable.
- `Close`: cerrar si se hace scroll o, opcionalmente, sólo cuando origen/panel queda totalmente fuera del viewport.
- `Block`: bloquear scroll de body preservando la posición, ancho de scrollbar y estilos previos; soportar varios diálogos mediante contador de bloqueo.
- `Noop`: estrategia explícita sin listener para overlays globales estáticos.
- Conectar `WithScrollableContainers` a observación real de los contenedores y evaluar clipping del origen/panel.

**Criterios de aceptación**

- Un tooltip/select conectado sigue correctamente a su trigger durante scroll.
- Un diálogo bloquea y restaura el scroll incluso con dos diálogos anidados y cierres en orden inverso.
- Un overlay `Close` no se cierra por scroll interno del propio panel salvo que se configure expresamente.

### OVR-CDK-203 — Observación de dimensiones del origen y contenido

**Trabajo**

- Añadir `ResizeObserver` opcional para origen y panel conectado.
- Reutilizar el scheduler y desconectar observers al cerrar.
- Mantener un fallback sin `ResizeObserver` para navegadores no compatibles.

**Criterios de aceptación**

- Cambiar el tamaño del trigger o del panel reposiciona sin necesidad de resize de ventana.
- Montar/desmontar repetidamente no deja observers activos.

## 8. Fase 3 — Dispatchers globales de interacción

### OVR-CDK-301 — Dispatcher de teclado

**Archivos principales**

- Nuevo `OverlayKeyboardDispatcher.cs` y soporte JS.
- `OverlayReference.cs`, `OverlayService.cs`.

**Trabajo**

- Mantener una pila de overlays adjuntos en orden de apertura/apilado.
- Registrar un único `keydown` en `document`/`body` mientras haya suscriptores elegibles.
- Entregar eventos al overlay superior que acepte teclado; no bloquear overlays inferiores si el superior no lo consume.
- Implementar Escape mediante una política configurable por `OverlayConfig` y respetar `DisableEscapeClose`.

**Criterios de aceptación**

- Escape cierra sólo el panel superior elegible.
- Un select abierto sobre un dialog no impide que el dialog reciba Escape cuando el select no lo maneja.
- No hay listener de teclado después de cerrar el último consumidor.

### OVR-CDK-302 — Dispatcher de clic/puntero exterior

**Trabajo**

- Registrar `pointerdown`, `click`, `auxclick` y `contextmenu` en captura mientras exista algún consumidor.
- Recordar el origen de `pointerdown`: un gesto iniciado dentro y liberado fuera no se considera clic exterior.
- Recorrer los overlays de arriba abajo y detenerse al encontrar uno que contiene el target.
- Exponer una señal de clic exterior en la referencia y una opción `CloseOnOutsidePointer` para que cada consumidor decida si cierra.
- Soportar Shadow DOM mediante `composedPath()`.

**Criterios de aceptación**

- Clic exterior cierra Select/Tooltip cuando así se configura y nunca cierra al interactuar dentro.
- Paneles anidados cierran primero el superior.
- Funciona con ratón, touch, teclado contextual y WebKit.

## 9. Fase 4 — Referencia y lifecycle público

### OVR-CDK-401 — Evolucionar `OverlayReference`

**Archivos principales**

- `Infrastructure/Models/OverlayReference.cs`
- `Infrastructure/Services/OverlayService.cs`
- `Infrastructure/Interfaces/IOverlayService.cs`

**Trabajo**

- Introducir operaciones idempotentes `DetachAsync`, `DisposeAsync`, `UpdatePositionAsync`, `UpdateSize` y clases de panel.
- Exponer `IsAttached`, configuración de sólo lectura y eventos `Attached`, `Detached`, `BackdropClicked`, `KeyDown`, `OutsidePointer` y `PositionChanged`.
- Separar `Detach` (permite reabrir si aplica) de `Dispose` (terminal).
- Mantener `Close` como compatibilidad; marcar la vía antigua como obsoleta sólo si existe una alternativa equivalente.
- Hacer `OverlayConfig` inmutable tras attach o copiarla internamente para evitar mutaciones de estado que rompan el scheduler.

**Criterios de aceptación**

- Dos `Detach/Dispose` consecutivos son seguros.
- No se puede actualizar ni recibir eventos de una referencia dispuesta.
- Un consumidor puede cambiar tamaño o pedir reposición sin acceder a componentes internos.

### OVR-CDK-402 — Orden de apilado y backdrop

**Trabajo**

- Mantener una pila explícita de overlays, separada del identificador interno.
- Al reabrir o re-adjuntar, mover host y backdrop al final lógico para que siempre queden por encima de los anteriores.
- Generar z-index a partir de esa pila y recuperar rangos al cerrar; evitar crecimiento innecesario de `LastZindex`.
- Mantener el backdrop inmediatamente detrás de su host y preservar clases/animaciones por instancia.

**Criterios de aceptación**

- Reabrir A después de B coloca A por encima de B.
- Backdrop y panel conservan orden correcto con tres overlays anidados.

## 10. Fase 5 — Posicionamiento, dirección y fullscreen

### OVR-CDK-501 — Completar el posicionamiento conectado

**Trabajo**

- Aplicar las correcciones funcionales pendientes de la estabilización: offsets en ambos signos, selección correcta de overflow, estilos de altura/anchura y `MarginBottom`.
- Usar el viewport visual y los rectángulos del contenedor para zoom de Safari, teclado virtual y desplazamiento de navegador móvil.
- Emitir `PositionChanged` con la posición efectiva y clases de orientación para animaciones y flechas.
- Soportar origen de tipo punto, útil para menú contextual, además de `ElementReference`.
- Definir una API de dirección `ltr`/`rtl` en `OverlayConfig` y aplicarla al host; no inferirla a partir de lógica incompleta del builder.

**Criterios de aceptación**

- La primera posición que encaje sigue teniendo preferencia; si ninguna encaja se elige la mejor alternativa, flexible o pushed según configuración.
- El mismo escenario se mantiene correcto en LTR/RTL, zoom y viewport móvil.

### OVR-CDK-502 — Contenedor fullscreen y punto de inserción

**Trabajo**

- Implementar un contenedor que se mueva al `document.fullscreenElement` al cambiar fullscreen y vuelva al body al salir.
- Permitir una inserción avanzada opcional: contenedor global por defecto, inline junto al origen o padre proporcionado.
- Documentar que el modo inline se reserva a escenarios que necesitan herencia DOM específica y debe usar el mismo lifecycle/dispatchers.

**Criterios de aceptación**

- Dialog, tooltip y panel conectado permanecen visibles al entrar/salir de fullscreen.
- La limpieza no elimina contenedores pertenecientes a otra instancia de aplicación o circuito.

## 11. Fase 6 — Accesibilidad y foco

### OVR-CDK-601 — Configuración semántica

**Trabajo**

- Añadir a la configuración campos opcionales para `Role`, `AriaLabel`, `AriaLabelledBy`, `AriaDescribedBy`, `AriaModal` y dirección.
- Emitir sólo atributos que correspondan al tipo de overlay; no asignar `dialog` a tooltips o selects.
- Documentar los roles recomendados por consumidor.

### OVR-CDK-602 — Focus trap y restauración de foco

**Trabajo**

- Crear un servicio de foco reutilizable, activado exclusivamente por configuración modal.
- Guardar el elemento previamente enfocado, enfocar el primer objetivo válido del panel y restaurar foco al cerrar si sigue conectado y no se ha indicado lo contrario.
- Usar sentinels/tab cycling o una implementación JS accesible; contemplar Shadow DOM y contenido que aparece después del primer render.
- Integrar en `Dialog` primero y ofrecerlo a futuros overlays modales.

**Criterios de aceptación**

- Tab y Shift+Tab no salen de un dialog modal.
- Escape/cierre restaura el foco al trigger cuando sigue disponible.
- Tooltip, Select y Autocomplete no atrapan foco por defecto.

## 12. Fase 7 — Integración, documentación y retirada de deuda

### OVR-CDK-701 — Migrar consumidores por lotes

Orden de integración:

1. `Dialog` y `FloatingPanel`: backdrop, bloqueo, Escape y foco.
2. `Select`, `Autocomplete`, `DatePicker`: reposición, clic exterior y autocierre si el origen desaparece.
3. `Tooltip`, `ConnectedPanel`, `FloatingDoubleList`: reposición y política específica de interacción.
4. `Toast`: validar que permanece fuera de dispatchers y scroll strategy salvo necesidad explícita.

Cada lote debe conservar las demos existentes y añadir al menos una prueba browser de apertura, interacción y cierre.

### OVR-CDK-702 — API y guía de uso

- Añadir al README y a la documentación de cada consumidor una tabla de estrategias de scroll y políticas de cierre.
- Incluir ejemplos de configuración para dialog modal, dropdown anclado, tooltip y menú contextual.
- Documentar compatibilidad, comportamiento de `DisposeAsync`, scope de tema y fullscreen.
- Añadir una guía de migración si se depreca alguna API anterior.

## 13. Verificación final

### Pruebas unitarias

- Pila, idempotencia y transición de estados de `OverlayReference`.
- Cada scroll strategy: attach/enable/disable/detach/dispose.
- Contador de bloqueo de scroll y restauración exacta de estilos.
- Scheduler: deduplicación, cancelación y descarte de versiones obsoletas.
- Selección de posición, RTL, viewport margin y clipping.

### Pruebas de navegador

- Resize y Developer Tools sin `DotNetObjectReference` inválida.
- Scroll de documento y de contenedor, incluido scroll rápido.
- Clic exterior, pointerdown interno/click externo y overlays anidados.
- Escape y foco con dos overlays de distinto tipo.
- Fullscreen, tema claro/oscuro y scope de tema en el portal.
- Navegación entre ejemplos mientras hay overlays abiertos.

### Comandos obligatorios

```bash
npm run lint:css
npm run buildDnetBlazor
dotnet test tests/Dnet.Blazor.UnitTests
dotnet test tests/Dnet.Blazor.BrowserTests
```

Las rutas exactas de los proyectos de test se verificarán antes de automatizar los comandos en CI.

## 14. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Romper el cierre actual de componentes existentes | Mantener adaptador de `OverlayReference` y migrar consumidores por lotes con pruebas de regresión. |
| Reposicionar cada scroll afecta al rendimiento | Scheduler por frame, listeners pasivos y estrategia `Close`/`Noop` cuando reposicionar no sea necesario. |
| Bloqueo de scroll altera layout | Guardar/restaurar todos los estilos y compensar scrollbar; prueba de overlays anidados. |
| Focus trap demasiado invasivo | Opt-in, inicialmente sólo `Dialog`, con restauración de foco configurable. |
| Fullscreen e inserción inline alteran tema/stacking | Mantener `ThemeScope`, una pila única y pruebas específicas en ambos modos. |
| Interop asíncrono durante dispose | Versionado, `AbortController`, cancelación y pruebas de navegación/cierre inmediato. |

## 15. Secuencia recomendada de PRs

1. **PR-Overlay-01:** baseline de pruebas, `ViewportRuler` scoped y JS seguro.
2. **PR-Overlay-02:** scheduler de reposición y correcciones de posicionamiento confirmadas.
3. **PR-Overlay-03:** contrato y cuatro estrategias de scroll.
4. **PR-Overlay-04:** dispatchers de teclado y clic exterior.
5. **PR-Overlay-05:** `OverlayReference` completo, lifecycle y stacking.
6. **PR-Overlay-06:** fullscreen, accesibilidad y focus trap de Dialog.
7. **PR-Overlay-07:** migración de consumidores, documentación y endurecimiento de pruebas.

No se iniciará un PR posterior hasta que el anterior pase las pruebas unitarias, browser y la matriz de consumidores afectada.
