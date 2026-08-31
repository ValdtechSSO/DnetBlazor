# Plan de implementación — Rediseño del `Toast`

Estado: listo para implementar
Ámbito: `Dnet.Blazor` — componente `Toast` y su contenedor de pila
Rama sugerida: la misma en la que va la migración de estilos (`Feature_20260819_AMU_Grid_Responsive`)
Versión objetivo: `Dnet.Blazor` 6.0.0

---

## 0. Antes de tocar nada

El agente que implemente esto debe, en este orden:

1. Leer `docs/ai/library-development.md`, `docs/ai/architecture.md` y `docs/ai/pitfalls.md`.
2. Ejecutar `node tools/component-info.mjs Toast` para obtener la superficie pública actual (parámetros, eventos, servicio asociado).
3. Localizar el CSS actual del componente en el troceado por ficheros (`toast.css` o equivalente) y el markup en el `.razor`.
4. Comprobar el contador del linter de tokens en `baseline.json` para `Toast`. **El contador solo puede bajar.** Si el componente tiene infracciones registradas, este trabajo debe dejarlas a 0 y actualizar el baseline.

**Restricción dura:** este rediseño no cambia la API pública del servicio de toasts (métodos de disparo, severidades, duración). Es un cambio de presentación y de estructura interna del markup. Si la implementación exige un cambio de API, se para y se documenta como ADR aparte antes de continuar.

---

## 1. Contexto y motivación

El toast actual usa el patrón clásico de banda de color gruesa a la izquierda, sombra dura de una sola capa y ningún indicador del temporizador. Tres problemas concretos:

- Una banda lateral (`border-left`) obliga a `border-radius: 0` en ese lado o produce el defecto de esquina redondeada sobre borde de un solo lado. Choca con el radio de 14px del tema `modern.css`.
- El aviso desaparece de golpe: el usuario no tiene ninguna señal de cuánto tiempo le queda para leerlo o pulsar la acción.
- El componente no está migrado a la arquitectura de tokens de tres capas, así que no es tematizable ni respeta `DnetThemeScope`.

---

## 2. Decisiones de diseño (ADR)

**ADR-T1 — El color de severidad se transporta por icono y barra de progreso, no por banda lateral.**
El estado se comunica con: (a) icono en contenedor redondeado de 32px con fondo teñido al 13% del acento, (b) barra de progreso de 2px a pie del toast, (c) color del enlace de acción principal. Se retira el `border-left`. Motivo: compatibilidad con el radio grande y menos peso visual.

**ADR-T2 — Un único token de acento por severidad.**
`--_accent` alimenta icono, fondo teñido, barra y enlace. Quien tematice redefine solo la capa semántica (`--dnet-sys-color-success|danger|warning|primary`) y el toast se adapta entero. No se exponen tokens separados para icono, barra y enlace.

**ADR-T3 — La severidad se expresa con `data-severity`, no con clases de modificador.**
`[data-severity="success|error|warning|info"]`. Consistente con lo hecho en `Chips` (estado por atributo, no por clase). `info` es el valor por defecto y no necesita atributo explícito, pero se emite igualmente para que el DOM sea autodescriptivo.

**ADR-T4 — La barra de progreso es una animación CSS, no un temporizador en C#.**
La duración se inyecta como custom property (`--_duration`) y la animación `dnet-toast-progress` corre en `linear forwards`. El cierre se dispara desde `@onanimationend`. Esto da gratis la pausa en hover y foco (`animation-play-state: paused`), que con un `System.Timers.Timer` en C# habría que sincronizar a mano. Si `Duration` es `null` el toast es persistente: no se renderiza la barra y solo se cierra por acción del usuario.

**ADR-T5 — Variante compacta como atributo, no como componente aparte.**
`data-compact` cuando no hay descripción ni acciones. Se puede derivar automáticamente: si `Message` viene vacío y no hay `Actions`, el componente entra en modo compacto sin que el llamante lo pida.

**ADR-T6 — El anuncio a lectores de pantalla vive en el contenedor de la pila, no en cada toast.**
Dos regiones vivas hermanas y permanentes en el DOM: una `role="status" aria-live="polite"` para `info`/`success`, otra `role="alert" aria-live="assertive"` para `warning`/`error`. Los toasts se insertan en la región que les corresponde. Motivo: crear la región viva y su contenido en el mismo tick hace que muchos lectores no anuncien nada.

**ADR-T7 — El botón de cerrar no es la única salida.**
`Escape` cierra el toast enfocado. El toast no roba el foco al aparecer.

**ADR-T8 — El toast vive en el portal global de `Overlay`, así que necesita `DnetThemeScope`.**
Sin esto hereda los tokens de `:root` y no los del contenedor que lo lanzó, y un tema por sección de la app no se refleja en sus avisos.

---

## 3. Contrato de tokens

Capa de componente, con indirección privada `--_x` como en `Button`, `PickList` y `Chips`.

```css
.dnet-toast {
  --_bg:        var(--dnet-toast-bg,          var(--dnet-sys-color-surface-elevated));
  --_fg:        var(--dnet-toast-fg,          var(--dnet-sys-color-on-surface));
  --_fg-muted:  var(--dnet-toast-fg-muted,    var(--dnet-sys-color-on-surface-muted));
  --_border:    var(--dnet-toast-border,      var(--dnet-sys-color-outline-subtle));
  --_radius:    var(--dnet-toast-radius,      var(--dnet-sys-radius-lg));
  --_shadow:    var(--dnet-toast-shadow,      var(--dnet-sys-elevation-3));
  --_accent:    var(--dnet-toast-accent,      var(--dnet-sys-color-primary));
  --_icon-bg:   var(--dnet-toast-icon-bg,     color-mix(in srgb, var(--_accent) 13%, transparent));
  --_icon-size: var(--dnet-toast-icon-size,   32px);
  --_pad:       var(--dnet-toast-padding,     var(--dnet-sys-space-4));
  --_width:     var(--dnet-toast-width,       392px);
  --_bar-h:     var(--dnet-toast-progress-height, 2px);
}

.dnet-toast-stack {
  --_gap:       var(--dnet-toast-stack-gap,    var(--dnet-sys-space-3));
  --_inset:     var(--dnet-toast-stack-inset,  var(--dnet-sys-space-5));
  --_z:         var(--dnet-toast-stack-z,      var(--dnet-sys-z-overlay));
}
```

Tokens nuevos que hay que dar de alta en la documentación de theming (`docs/ai/reference/theming.md` y la ficha del componente): los once `--dnet-toast-*` y los tres `--dnet-toast-stack-*` de arriba. Ninguno debe tener valor literal en la capa de componente; todos caen a la capa `sys`.

Si alguno de los tokens `sys` referenciados no existe todavía (`--dnet-sys-color-surface-elevated`, `--dnet-sys-color-outline-subtle`, `--dnet-sys-elevation-3`, `--dnet-sys-z-overlay`), darlos de alta en la capa semántica con su correspondiente mapeo a `--dnet-ref-*`, no inventar un literal aquí.

---

## 4. Estructura del markup

```html
<div class="dnet-toast-stack" data-position="bottom-end">
  <div class="dnet-toast-region" role="status" aria-live="polite"></div>
  <div class="dnet-toast-region" role="alert" aria-live="assertive"></div>
</div>
```

Un toast dentro de una región:

```html
<div class="dnet-toast"
     data-severity="success"
     data-state="open"
     style="--_duration: 5000ms">

  <span class="dnet-toast__icon" aria-hidden="true">
    <!-- icono de la severidad -->
  </span>

  <div class="dnet-toast__body">
    <p class="dnet-toast__title">Configuración guardada</p>
    <p class="dnet-toast__message">Las columnas del grid se han actualizado.</p>
    <div class="dnet-toast__actions">
      <button type="button" class="dnet-toast__action">Deshacer</button>
      <button type="button" class="dnet-toast__action" data-quiet>Ver detalle</button>
    </div>
  </div>

  <button type="button" class="dnet-toast__close" aria-label="Cerrar aviso">
    <!-- icono X -->
  </button>

  <span class="dnet-toast__progress" aria-hidden="true"></span>
</div>
```

Reglas del markup:

- `dnet-toast__message` y `dnet-toast__actions` no se renderizan si están vacíos. Si faltan ambos, el toast lleva además `data-compact`.
- `dnet-toast__progress` no se renderiza si `Duration` es `null`.
- El icono es decorativo (`aria-hidden`): la severidad ya la transporta la región viva. No meter texto oculto tipo "Éxito:" en el título.
- Máximo dos acciones. Si llegan más, se ignoran las sobrantes y se emite un warning en DEBUG.

---

## 5. CSS

Fichero: `toast.css` en el troceado por componente. CSS plano con anidamiento nativo, sin Sass, sin CSS isolation.

```css
.dnet-toast-stack {
  position: fixed;
  z-index: var(--_z);
  display: flex;
  flex-direction: column;
  gap: var(--_gap);
  pointer-events: none;

  &[data-position="bottom-end"]   { inset: auto var(--_inset) var(--_inset) auto; }
  &[data-position="bottom-start"] { inset: auto auto var(--_inset) var(--_inset); }
  &[data-position="top-end"]      { inset: var(--_inset) var(--_inset) auto auto; }
  &[data-position="top-start"]    { inset: var(--_inset) auto auto var(--_inset); }
  &[data-position="top-center"]   { inset: var(--_inset) 0 auto 0; align-items: center; }

  &[data-position^="top"] { flex-direction: column-reverse; }

  .dnet-toast-region {
    display: contents;
  }
}

.dnet-toast {
  position: relative;
  display: flex;
  gap: var(--dnet-sys-space-3);
  width: var(--_width);
  max-width: calc(100vw - var(--_inset, 16px) * 2);
  padding: var(--_pad);
  background: var(--_bg);
  color: var(--_fg);
  border: 1px solid var(--_border);
  border-radius: var(--_radius);
  box-shadow: var(--_shadow);
  overflow: hidden;
  pointer-events: auto;

  &[data-severity="success"] { --_accent: var(--dnet-sys-color-success); }
  &[data-severity="error"]   { --_accent: var(--dnet-sys-color-danger); }
  &[data-severity="warning"] { --_accent: var(--dnet-sys-color-warning); }

  &[data-compact] {
    align-items: center;
    width: auto;
    min-width: 260px;
    padding-block: var(--dnet-sys-space-3);

    --_icon-size: 24px;
  }
}

.dnet-toast__icon {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: center;
  width: var(--_icon-size);
  height: var(--_icon-size);
  border-radius: var(--dnet-sys-radius-md);
  background: var(--_icon-bg);
  color: var(--_accent);
}

.dnet-toast__body {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: var(--dnet-sys-space-1);
}

.dnet-toast__title {
  margin: 0;
  font-size: var(--dnet-sys-font-size-sm);
  font-weight: var(--dnet-sys-font-weight-medium);
  line-height: 1.35;
}

.dnet-toast__message {
  margin: 0;
  font-size: var(--dnet-sys-font-size-xs);
  line-height: 1.5;
  color: var(--_fg-muted);
  overflow-wrap: anywhere;
}

.dnet-toast__actions {
  display: flex;
  gap: var(--dnet-sys-space-4);
  margin-top: var(--dnet-sys-space-2);
}

.dnet-toast__action {
  padding: 0;
  border: 0;
  background: none;
  font: inherit;
  font-size: var(--dnet-sys-font-size-xs);
  font-weight: var(--dnet-sys-font-weight-medium);
  color: var(--_accent);
  cursor: pointer;

  &[data-quiet] { color: var(--_fg-muted); }
  &:hover { text-decoration: underline; }
}

.dnet-toast__close {
  flex: 0 0 auto;
  align-self: flex-start;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  padding: 0;
  border: 0;
  border-radius: var(--dnet-sys-radius-sm);
  background: none;
  color: var(--_fg-muted);
  cursor: pointer;

  &:hover {
    background: color-mix(in srgb, var(--_fg) 7%, transparent);
    color: var(--_fg);
  }
}

.dnet-toast__progress {
  position: absolute;
  inset: auto 0 0 0;
  height: var(--_bar-h);
  background: color-mix(in srgb, var(--_accent) 16%, transparent);

  &::after {
    content: "";
    display: block;
    height: 100%;
    background: var(--_accent);
    transform-origin: left center;
    animation: dnet-toast-progress var(--_duration, 5000ms) linear forwards;
  }
}

.dnet-toast:hover,
.dnet-toast:focus-within {
  .dnet-toast__progress::after { animation-play-state: paused; }
}

@keyframes dnet-toast-progress {
  from { transform: scaleX(1); }
  to   { transform: scaleX(0); }
}

.dnet-toast[data-state="open"]    { animation: dnet-toast-in  180ms var(--dnet-sys-ease-out) both; }
.dnet-toast[data-state="closing"] { animation: dnet-toast-out 140ms var(--dnet-sys-ease-out) both; }

@keyframes dnet-toast-in {
  from { opacity: 0; transform: translateY(8px) scale(.97); }
  to   { opacity: 1; transform: none; }
}

@keyframes dnet-toast-out {
  from { opacity: 1; transform: none; }
  to   { opacity: 0; transform: translateY(4px) scale(.98); }
}

@media (prefers-reduced-motion: reduce) {
  .dnet-toast[data-state] { animation: none; }
  .dnet-toast__progress::after { animation-duration: var(--_duration, 5000ms); }
}
```

Notas:

- `transform: scaleX()` en vez de animar `width`: no provoca layout en cada frame.
- El `display: contents` en las regiones vivas permite tener dos regiones ARIA hermanas sin romper el `flex` de la pila. Verificar en Firefox: si diera problemas de accesibilidad con `display: contents`, la alternativa es hacer cada región un `flex` propio y separar la pila en dos columnas apiladas.
- En `prefers-reduced-motion` se conservan la barra de progreso y su duración (es información, no decoración) y se eliminan entrada y salida.

---

## 6. Superficie C#

Cambios internos, sin romper la API pública del servicio:

- `Position` en el contenedor de la pila: enum con `BottomEnd` (por defecto), `BottomStart`, `TopEnd`, `TopStart`, `TopCenter`. Se serializa a `data-position` en kebab-case.
- `MaxVisible` (por defecto 4): los toasts por encima del límite quedan en cola y entran según se cierran los visibles. Sin este límite la pila crece hasta salirse de pantalla.
- Cierre por `@onanimationend` en el elemento raíz, filtrando por `e.AnimationName`:
  - `dnet-toast-progress` → iniciar el cierre (`data-state="closing"`).
  - `dnet-toast-out` → retirar el toast del árbol y liberar su entrada en la cola.
- `@onkeydown` en el raíz: `Escape` → cierre inmediato.
- El componente pone `tabindex="0"` en el raíz solo si tiene acciones; si no, no entra en el orden de tabulación.
- Envolver el contenido del portal en `DnetThemeScope`, igual que se hizo con los demás componentes que pasan por `Overlay`.

Localización: un único record `ToastStrings` con el mismo patrón que `PickListStrings` (defaults → DI global → parámetro por instancia). De entrada solo necesita `CloseLabel`.

---

## 7. Fases

1. **Tokens y CSS.** Dar de alta los tokens `sys` que falten, escribir `toast.css` completo, dejar el linter a 0 infracciones para el componente y actualizar `baseline.json`.
2. **Markup.** Reescribir el `.razor` a la estructura de la sección 4, con `data-severity`, `data-compact` derivado y renderizado condicional de mensaje, acciones y barra.
3. **Ciclo de vida.** Sustituir el temporizador por el cierre dirigido por `animationend`, añadir `data-state`, `Escape` y `MaxVisible` con cola.
4. **Pila y accesibilidad.** Contenedor con las dos regiones vivas permanentes, `Position`, `DnetThemeScope` en el portal.
5. **Documentación.** Regenerar la ficha del componente con `node tools/build-reference.mjs`, actualizar `theming.md` con los tokens nuevos y añadir una nota de migración a las notas de la 6.0.0.

Cada fase es un commit independiente y compila por sí sola.

---

## 8. Criterios de aceptación

- [ ] El componente no contiene ningún valor literal de color, radio, sombra o espaciado. El linter de tokens da 0 infracciones para `Toast`.
- [ ] Cambiar `--dnet-sys-color-primary` en un `DnetThemeScope` cambia el acento del toast lanzado desde dentro de ese scope, no el de otros.
- [ ] `modern.css` y su variante oscura se ven correctos sin ninguna regla específica de `Toast` en el fichero de tema.
- [ ] Con `Duration = 5000`, la barra tarda 5s en vaciarse y el toast se cierra al terminar. Con el ratón encima, ni la barra avanza ni el toast se cierra; al salir, continúa donde estaba.
- [ ] Con `Duration = null` no hay barra y el toast solo se cierra por el botón, por `Escape` o por acción.
- [ ] Un toast de `error` interrumpe al lector de pantalla; uno de `success` espera turno.
- [ ] Con cinco toasts disparados a la vez y `MaxVisible = 4`, solo se ven cuatro y el quinto entra cuando se cierra uno.
- [ ] A 360px de ancho el toast no provoca scroll horizontal.
- [ ] Con `prefers-reduced-motion: reduce` no hay animación de entrada ni de salida, y el temporizador sigue funcionando.
- [ ] Un mensaje sin espacios de 200 caracteres no rompe el layout.

---

## 9. Fuera de alcance

- Toasts con contenido arbitrario (`RenderFragment` en el cuerpo).
- Agrupación o colapso de toasts repetidos.
- Toast anclado a un elemento en vez de a la ventana.
- Swipe para descartar en táctil.
- Sonido o vibración.
