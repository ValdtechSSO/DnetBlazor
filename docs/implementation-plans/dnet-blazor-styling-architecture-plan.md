# Plan de implementación — Reestructuración de estilos y sistema de temas de DnetBlazor

**Estado:** **congelado v2.2** — listo para implementar. No se refina más arquitectura: el siguiente feedback útil sale de la Fase 0 y de la puerta de fase de `Button`.
**Fecha:** 2026-08-19
**Ámbito:** `src/Dnet.Blazor` (32 familias de componentes), `src/Dnet.Blazor.Material`, `samples/Dnet.Shared`
**Origen:** auditoría estática del árbol de estilos en `10bd9035` — 116 ficheros `.scss`, 7.786 líneas en `src/Dnet.Blazor/Components`, bundle compilado `wwwroot/dnet-blazor-styles.css` de 142 KB minificado.
**Restricciones dadas:** sin frameworks CSS; CSS escrito a mano; máxima sencillez con máxima flexibilidad.
**Versión objetivo:** `Dnet.Blazor` **6.0.0** (cambio deliberado de API pública de estilado y del mecanismo de distribución de `PickList`).
**Decisiones cerradas:** ADR-01 = **sin Sass** · ADR-02 = **bundle global único** · ADR-03 = **prefijo `--dnet-*` único, legacy en la cadena de lectura** · ADR-04a = **unificar nombres de token** · ADR-04b = **aplazado**, se decide con medición al cerrar la Fase 4 · ADR-05 = **eliminar `Dnet.Blazor.Material`** · ADR-06 = **el scope de tema se transporta al portal de `Overlay`**.

## 0. Cambios de la v2 respecto a la v1

Corregidos tras revisión. Cada uno es un contrato que en la v1 podía producir un resultado que *parece* correcto mientras rompe la flexibilidad prometida:

| # | Qué estaba mal en la v1 | Dónde |
|---|---|---|
| 1 | Los alias de retrocompatibilidad iban **al revés**: definían el nombre viejo a partir del nuevo, cuando el consumidor antiguo *escribe* el viejo. La migración le habría roto los overrides en silencio. | ADR-03, CLN-002 |
| 2 | `LST-001` declaraba el token público en el componente, lo que **blinda ese componente contra `:root`** y destruye justo la herencia que persigue el diseño. | ADR-04a, LST-001, **R10** |
| 3 | No se contemplaba que el portal global de `Overlay` **saca los overlays del DOM donde se invocan**, así que no heredan ni el tema ni los overrides de scope. | **ADR-06** (nuevo) |
| 4 | ADR-02 afirmaba que el README no documenta `{App}.styles.css`. **Era falso.** Y exigía que la especificidad no cambiara al retirar la isolation, lo cual es imposible. | ADR-02, LST-002 |
| 5 | R5 prohibía los tokens `--_*` que el propio mecanismo central usa; R8 prohibía selectores en un fichero de tema que necesariamente lleva uno. | R5, R8 |
| 6 | `dark.css` metía hex en `system` y `compact.css` tocaba `reference`, contradiciendo la regla de capas declarada. | §2, §4 |
| 7 | La excepción de R1 para data-URI habría dejado **40 iconos** con `fill` fijo negros sobre fondo oscuro. No era un caso aislado de `Checkbox`. | **R9** (nuevo), Fase 4 |
| 8 | El baseline visual capturaba páginas, no estados: no habría detectado el bug de `Button:hover` que motiva la Fase 2. | STY-002 |
| 9 | La comparación de bundles ordenaba las reglas, ocultando regresiones de cascada. Y el presupuesto de tamaño en crudo presionaba en contra de la propia arquitectura. | TOK-005, CLN-003 |

**Correcciones de una segunda revisión, ya incorporadas:** §3.6 arrastraba la afirmación falsa sobre el README · la compatibilidad legacy seguía redactada como "alias" en TOK-002, VER-001 y SUB-002 (ahora siempre *eslabón en la cadena de lectura*) · R10 daba falsos positivos sobre `tokens/` y `theme/`, y STY-001 decía R1–R8 · `system.css` llevaba `rgb(0 0 0 / .1)` en la elevación, ahora tokenizado como `--dnet-sys-shadow-color` · "nunca a un literal" en capa 3 era demasiado fuerte y habría llevado a tokenizar cada `2px` · **ADR-06 no decía de dónde salía `ThemeScope`; ahora lo resuelve el componente `DnetThemeScope`** · `patterns/listbox.css` figuraba como resultado seguro cuando ADR-04b puede decidir que no exista · Fase 4 exigía un mínimo de 4 tokens públicos, incompatible con "un token existe cuando alguien lo va a cambiar".

**Correcciones de una tercera revisión, ya incorporadas:** R5 acotaba los privados "al selector que los declara", cuando el propio patrón correcto los consume en pseudoclases y descendientes — ahora la unidad de propiedad es el **fichero/componente** · VER-001 hacía que `.dnet-icon-button` declarase tokens públicos, violando el R10 que se añadió después; corregido al mismo patrón de privados · TOK-003 hablaba de recuperar "el valor del fallback implícito", que **no existe**: sin fallback la declaración queda inválida y no hay valor pretendido guardado en ninguna parte · §5 decía "cinco decisiones" con seis ADR, y la capa `reference` conservaba una frase sobre temas incompatible con el contrato actual.

---

---

## 1. Objetivo

Que cambiar el aspecto de DnetBlazor —o crear un tema entero— sea **editar un solo fichero de ~120 líneas**, y que ajustar un componente concreto sea **declarar una custom property en cualquier ancestro**, sin tocar el código de la librería, sin recompilar la librería y sin peleas de especificidad.

Concretamente, al terminar debe ser cierto que:

1. Un consumidor crea un tema completo escribiendo únicamente valores de la capa semántica.
2. Un consumidor reestiliza un componente concreto sin `!important` y sin conocer los selectores internos.
3. Dos instancias del mismo componente pueden tener temas distintos en la misma página.
4. Existe un modo oscuro funcionando como prueba de que la arquitectura aguanta.
5. Ningún fichero de componente contiene un literal de color.

**No** es objetivo rediseñar la apariencia por defecto. El aspecto visual al final de la migración debe ser idéntico al actual, salvo las correcciones de defectos listadas en la Fase 1.

---

## 2. Principios rectores

- **Los tokens son custom properties de CSS, nunca variables de Sass.** Una variable Sass se evalúa en build y muere ahí; una custom property es un punto de extensión vivo en runtime, heredable y sobreescribible por el consumidor. Esta es la única razón por la que el sistema puede ser "tremendamente flexible".
- **Un token público se lee, nunca se declara fuera de su capa.** Un componente consume `var(--dnet-list-radius, …)`; **no** escribe `--dnet-list-radius: …`. Declararlo en el propio componente destruye la herencia y es el error más fácil de cometer en toda esta arquitectura (véase R10).
- **Capas:** `reference` contiene paleta y escalas crudas. `system` consume `reference` **en el tema base**. Un tema puede escribir valores finales directamente en `system`; no está obligado a pasar por `reference`. Los componentes consumen `system` y sus propios tokens, nunca `reference`.
- **La flexibilidad se demuestra, no se declara.** Un token que nadie usa no es flexibilidad, es deuda (hoy hay 45).
- **Validación en rodaja vertical antes de expansión horizontal.** Un componente completo y verificado antes de tocar los otros 31.

---

## 3. Diagnóstico

Los números salen de la auditoría del árbol actual, no de impresiones.

### 3.1 Fragmentación del origen de verdad

| Métrica | Valor |
|---|---:|
| Bloques `:root` distintos en la librería | **27** |
| Tokens declarados | **249** |
| Tokens realmente usados | **215** |
| Tokens declarados que nunca se usan (*dead tokens*) | **45** |
| Tokens usados que nunca se declaran (*ghost tokens*) | **11** |

No existe un fichero de tema. Existen 27 ficheros que cada uno reclama `:root` para sí. Cambiar el color primario obliga a abrir y entender N ficheros de componente.

De los 11 tokens fantasma, 10 se usan **sin fallback**: su declaración queda *invalid at computed-value time* y la propiedad cae al valor heredado o al inicial. Es un contrato implícito no documentado que la app consumidora solo cumple por casualidad. El undécimo, `--blg-focus-color`, sí lleva fallback (`currentColor`) y no está roto. El desglose y la resolución de cada uno están en TOK-003.

### 3.2 Literales incrustados

| Literal | Ocurrencias en `.scss` |
|---|---:|
| Colores hex | ~217 (incluye iconos en data-URI) |
| `rgb()` / `rgba()` | 141 |
| `white` / `black` | 96 |
| Valores en `px` | 1.092 |

Casos representativos: `#ffffff`/`#fff` aparece **79 veces**; `#5f6368` (el color de texto de facto) **26 veces**; `#4fc3f7` **7 veces** *a pesar de que existe* `--dnet-primary-color: #4fc3f7`. El token existe pero nadie lo consume: cambiarlo no cambia nada.

### 3.3 Flexibilidad aparente

`Button/dnet-button.scss` declara `--dnet-button-hover-color` y `--dnet-button-hover-opacity`, y a continuación escribe:

```scss
&:hover { background-color: rgba(0,0,0,.08); }
```

El consumidor que redefine esos dos tokens no observa ningún efecto. El mismo patrón se repite en 45 tokens. Es peor que no tener token: promete un contrato que no se cumple.

### 3.4 Triplicación de familias de lista

`List`, `Select` y `Autocomplete` mantienen **tres copias** del mismo conjunto de tokens (15 + 14 + 17), con los mismos valores salvo `border-radius` (10px / 5px / 5px) y las mismas reglas duplicadas (`-wrapper`, `-header`, `-footer`, `-item`, `:has()` para padding condicional). Las mismas familias existen para calendario: `--dnet-calendar-*`, `--dnet-datepickerweekca-*` y `--dnet-datepickerweekca-raw-*`.

### 3.5 Defectos activos

- **Funciones Sass dentro de custom properties.** `AdminLayout/_theme-variables.scss` declara `--dnet-dash-gray-dark: lighten(#000, 20%)` y `--dnet-dash-link-hover-color: darken(rgb(0, 36, 61), 15%)`. Sass **no** evalúa funciones dentro del valor de una custom property sin interpolación; el literal `lighten(#000, 20%)` viaja tal cual al CSS publicado — verificado en `wwwroot/dnet-blazor-styles.css`. Ambos tokens están rotos en producción.
- **Fuga de camelCase.** `--dnet-dash-asideWidth` y `--dnet-dash-asideWidth-minified` rompen el kebab-case del resto. Las custom properties distinguen mayúsculas: es una trampa de tipografía.
- **Errata petrificada.** `--dnet-secundary-color` (dead token, nadie lo usa).
- **Namespace global sin prefijo.** Seis tokens sin prefijo (`--header-color`, `--content-color`, …) colisionan con cualquier hoja de estilo de la app anfitriona.
- **Tres prefijos coexistiendo:** `--dnet-*`, `--dnet-dash-*` y `--blg-*`.

### 3.6 Tres mecanismos de estilado simultáneos

1. **Bundle global** — SCSS → webpack → `_content/Dnet.Blazor/dnet-blazor-styles.css` (28 componentes).
2. **CSS isolation** — `PickList.razor.css`, que viaja por `{App}.styles.css`. El README **sí** documenta este segundo enlace y advierte de que es necesario para `PickList`. El problema no es de documentación: son dos caminos de distribución y dos modelos de especificidad conviviendo (ADR-02).
3. **SCSS vendorizado muerto** — `Dnet.Blazor.Material/Components/FormField` son 38 ficheros heredados de Angular Material que **no entran en el bundle** (verificado: cero referencias desde `dnet-blazor-styles.scss`).

Nota positiva: `PickList.razor.css` ya aplica exactamente el patrón de indirección que este plan generaliza:

```css
--_pick-list-border: var(--pick-list-border, var(--dnet-list-border-color, #ebebeb));
```

El trabajo no es inventar una arquitectura, es **extender la que ya está bien en el componente más reciente** al resto del repositorio.

### 3.7 Precedente de tema ya existente

`Grid/_blg-base-theme-vars.scss` + `_blg-arcadia-theme-vars.scss` implementan ya una separación base/tema con encadenamiento (`--blg-header-background-color: var(--blg-background-color)`) y una escala de espaciado derivada (`calc(var(--blg-grid-size) * 10)`). Es el modelo mental correcto, aplicado a un solo componente. Se generaliza y se absorbe.

---

## 4. Arquitectura objetivo

Tres capas de tokens más una indirección privada dentro de cada componente.

```
Capa 1  PRIMITIVAS      --dnet-ref-*     paleta y escalas crudas
                              ↓
Capa 2  SEMÁNTICAS      --dnet-sys-*     roles. AQUÍ y SOLO AQUÍ vive un tema
                              ↓
Capa 3  COMPONENTE      --dnet-btn-*     API pública por componente
                              ↓
        privado         --_btn-*         consumo interno
```

### Capa 1 — Primitivas (`--dnet-ref-*`)

Valores crudos sin significado semántico. Los temas redefinen `system`, no `reference`.

```css
:root {
  /* paleta */
  --dnet-ref-neutral-0:   #ffffff;
  --dnet-ref-neutral-50:  #fafafa;
  --dnet-ref-neutral-100: #f2f2f2;
  --dnet-ref-neutral-200: #ebebeb;
  --dnet-ref-neutral-300: #e1e3e1;
  --dnet-ref-neutral-500: #757575;
  --dnet-ref-neutral-600: #666666;
  --dnet-ref-neutral-700: #5f6368;
  --dnet-ref-neutral-1000:#000000;
  --dnet-ref-accent-300:  #4fc3f7;
  --dnet-ref-accent-500:  #42b0d5;

  /* escala tipográfica */
  --dnet-ref-font-sans: 'Roboto', 'Helvetica Neue', Helvetica, Arial, sans-serif;
}
```

La unidad de espaciado **no** vive aquí: está en `system`, para que un tema de densidad pueda cambiarla sin salirse de su capa.

### Capa 2 — Semánticas (`--dnet-sys-*`)

La capa de roles. **Un tema es exactamente este fichero y nada más.** Objetivo: ≤ 120 tokens.

```css
:root {
  /* superficies y texto */
  --dnet-sys-surface:            var(--dnet-ref-neutral-0);
  --dnet-sys-surface-raised:     var(--dnet-ref-neutral-0);
  --dnet-sys-surface-hover:      var(--dnet-ref-neutral-100);
  --dnet-sys-on-surface:         var(--dnet-ref-neutral-700);
  --dnet-sys-on-surface-muted:   var(--dnet-ref-neutral-600);
  --dnet-sys-on-surface-subtle:  var(--dnet-ref-neutral-500);

  /* acento */
  --dnet-sys-primary:            var(--dnet-ref-accent-300);
  --dnet-sys-primary-strong:     var(--dnet-ref-accent-500);
  --dnet-sys-on-primary:         var(--dnet-ref-neutral-0);

  /* bordes y separadores */
  --dnet-sys-border:             var(--dnet-ref-neutral-200);
  --dnet-sys-border-strong:      var(--dnet-ref-neutral-300);

  /* estado — derivado de on-surface, así se invierte solo en tema oscuro */
  --dnet-sys-state-hover:        color-mix(in srgb, var(--dnet-sys-on-surface) 8%, transparent);
  --dnet-sys-state-pressed:      color-mix(in srgb, var(--dnet-sys-on-surface) 14%, transparent);
  --dnet-sys-state-selected:     color-mix(in srgb, var(--dnet-sys-primary) 12%, transparent);
  --dnet-sys-state-disabled-fg:  color-mix(in srgb, var(--dnet-sys-on-surface) 38%, transparent);
  --dnet-sys-focus-ring:         var(--dnet-sys-primary-strong);

  /* forma */
  --dnet-sys-radius-sm:  4px;
  --dnet-sys-radius-md:  5px;
  --dnet-sys-radius-lg: 10px;
  --dnet-sys-radius-pill: 9999px;

  /* elevación — el color se tokeniza aparte para que system.css no lleve literales */
  --dnet-sys-shadow-color: color-mix(in srgb, var(--dnet-ref-neutral-1000) 10%, transparent);
  --dnet-sys-elevation-1: 0 2px 1px -1px var(--dnet-sys-shadow-color),
                          0 1px 2px  0   var(--dnet-sys-shadow-color),
                          0 1px 10px 0   var(--dnet-sys-shadow-color);

  /* espaciado: la unidad vive en sys, no en ref, para que un tema
     de densidad sea estrictamente semántico */
  --dnet-sys-space-unit: 4px;
  --dnet-sys-space-1: calc(var(--dnet-sys-space-unit) * 1);   /*  4px */
  --dnet-sys-space-2: calc(var(--dnet-sys-space-unit) * 2);   /*  8px */
  --dnet-sys-space-3: calc(var(--dnet-sys-space-unit) * 3);   /* 12px */
  --dnet-sys-space-4: calc(var(--dnet-sys-space-unit) * 4);   /* 16px */
  --dnet-sys-space-5: calc(var(--dnet-sys-space-unit) * 5);   /* 20px */

  /* densidad: una palanca que reescala toda la librería */
  --dnet-sys-control-height:      50px;
  --dnet-sys-control-height-sm:   38px;

  /* tipografía */
  --dnet-sys-font:       var(--dnet-ref-font-sans);
  --dnet-sys-text-xs:    0.625rem;
  --dnet-sys-text-sm:    0.75rem;
  --dnet-sys-text-md:    0.875rem;
  --dnet-sys-text-lg:    1rem;
  --dnet-sys-text-xl:    1.25rem;

  /* movimiento */
  --dnet-sys-motion-fast:   150ms;
  --dnet-sys-motion-normal: 200ms;
  --dnet-sys-motion-ease:   cubic-bezier(0.35, 0, 0.25, 1);
}
```

### Capa 3 — Componente (`--dnet-<comp>-*`)

API pública y estable de cada componente.

**Regla de valores:** todo valor tematizable y reutilizable se deriva de `system`. Se permiten constantes estructurales locales (`0`, `transparent`, `36px`, `1px`) cuando no forman parte del contrato de tematización. El objetivo es no inventar un token para cada constante: un token existe cuando alguien lo va a cambiar, no cuando podría.

```css
/* button.css */
.dnet-button {
  --_bg:      var(--dnet-btn-background, transparent);
  --_fg:      var(--dnet-btn-foreground, var(--dnet-sys-on-surface));
  --_hover:   var(--dnet-btn-background-hover, var(--dnet-sys-state-hover));
  --_pressed: var(--dnet-btn-background-pressed, var(--dnet-sys-state-pressed));
  --_radius:  var(--dnet-btn-radius, var(--dnet-sys-radius-sm));
  --_pad:     var(--dnet-btn-padding, 0 var(--dnet-sys-space-4));
  --_height:  var(--dnet-btn-height, 36px);
  --_font:    var(--dnet-btn-font-size, var(--dnet-sys-text-md));

  background-color: var(--_bg);
  color: var(--_fg);
  border-radius: var(--_radius);
  padding: var(--_pad);
  line-height: var(--_height);
  font-size: var(--_font);
}

.dnet-button:hover  { background-color: var(--_hover); }
.dnet-button:active { background-color: var(--_pressed); }
```

### Por qué la indirección `--_x` es el núcleo de la flexibilidad

Sin ella, el token público solo puede fijarse donde se declara el defecto (típicamente `:root`), lo que hace imposible tener dos instancias distintas. Con ella, el defecto vive **en el propio selector del componente** y `var(--dnet-btn-background, …)` se resuelve por **herencia** desde cualquier ancestro. Consecuencias directas:

```css
/* toda la app */
:root                 { --dnet-btn-radius: 0; }

/* solo dentro de un panel */
.mi-panel             { --dnet-btn-background: #eee; }

/* una instancia concreta, sin !important y sin especificidad extra */
```
```razor
<DnetButton style="--dnet-btn-background: crimson; --dnet-btn-foreground: white;">Borrar</DnetButton>
```

Los tres casos funcionan con el mismo mecanismo y sin ninguna regla nueva en la librería. Ese es el rendimiento de la arquitectura.

### Los temas

Un tema es un fichero que solo redefine capa 2, y es **scopeable**: se aplica a `:root` o a un subárbol.

```css
/* theme/dark.css — el fichero completo, no un extracto */
[data-dnet-theme="dark"] {
  --dnet-sys-surface:           #1e1f22;
  --dnet-sys-surface-raised:    #2b2d31;
  --dnet-sys-surface-hover:     #35373c;
  --dnet-sys-on-surface:        #e3e5e8;
  --dnet-sys-on-surface-muted:  #b5bac1;
  --dnet-sys-border:            #3f4147;
  --dnet-sys-elevation-1:       0 2px 8px rgb(0 0 0 / .5);
}
```

Un tema escribe valores finales en `system` directamente; no tiene que pasar por `reference`. Y fíjate en lo que **no** aparece: las capas de estado (`--dnet-sys-state-hover`, `-pressed`, `-disabled-fg`) no se redeclaran, porque están derivadas con `color-mix` de `--dnet-sys-on-surface` y se invierten solas al cambiar el color de texto. Ese es el rendimiento de derivar en lugar de enumerar.

Cambiar de tema en runtime: `document.documentElement.dataset.dnetTheme = 'dark'`. Sin recompilar, sin recargar hojas, sin JS de la librería.

---

## 5. Decisiones a cerrar (ADR)

Estas seis decisiones condicionan las fases. Todas están cerradas salvo ADR-04b, que por diseño se resuelve con medición al final de la Fase 4.

### ADR-01 — Sass: **retirado** ✔ decidido

Se elimina Sass. Los ficheros pasan a `.css` plano con **anidamiento nativo de CSS** (Chrome 120+, Safari 17.2+, Firefox 117+; baseline desde 2024). Se conserva webpack como empaquetador: `css-loader` resuelve los `@import` en build exactamente igual que hoy resuelve `@use`, y se mantienen `autoprefixer` y la minificación. Se elimina la dependencia `sass` de `package.json`.

Se descarta explícitamente publicar N ficheros sueltos sin build: perder el bundle único empeora la red y expone el orden de la cascada a errores del consumidor.

**Inventario real de la conversión** (medido sobre `HEAD`, no estimado):

| Construcción | Ocurrencias | Dificultad |
|---|---:|---|
| `@use` / `@import` / `@forward` | 45 / 39 / 13 | Trivial → `@import` |
| Comentarios `//` | 272 | Trivial → `/* */` |
| Partials `_x.scss` | 40 ficheros | Trivial: el guion bajo es convención de Sass, sin significado en CSS |
| Concatenación `&-suffix` | **0** | — |
| `@each` / `@for` / `@extend` | **0** | — |
| `@mixin` / `@include` | 34 / 36 | **Concentrado**: ver abajo |
| `@function` / `@return` / `@if` / `@content` | 6 / 8 / 14 / 14 | **Concentrado**: ver abajo |
| Variables `$x` | 331 | **Concentrado**: ver abajo |

El hallazgo que hace viable esta decisión: **cero concatenación `&-suffix`**, que es la única incompatibilidad real entre el anidamiento de Sass y el nativo. Y **cero `@each`/`@for`/`@extend`**, que son las construcciones sin equivalente directo. La conversión de 28 de las 32 familias es puramente mecánica.

Toda la lógica Sass real vive en **14 ficheros de 4 áreas**, y toda ella es **código de Angular Material/CDK vendorizado**:

| Área | Ficheros | Qué contiene |
|---|---:|---|
| `Form/` | 9 (899 líneas) | Sistema de elevación de Angular Material: `_elevation.scss` (197 líneas), `_private.scss`, `_vendor-prefixes.scss`, `_variables.scss` y sus cuatro shims `*.import.scss`. Concentra los 6 `@function` y casi todos los `@if`. |
| `Tabs/` | 4 | `cdk-a11y`, `cdk-high-contrast`, `_noop-animation`, `user-select`, `tab-label`, `ink-bar`, `paginated-tab-header`. Concentra los 14 `@content`. |
| `Grid/` | 3 | `row-border($color)`, `grid-header-cell($padding)`, `grid-cell($h,$p)`, `blg-base-theme()`. |
| `Overlay/` | 2 | `@mixin cdk-overlay()`, invocado una sola vez. |

**Estrategia para esas 4 áreas: compilar una vez y congelar la salida.** No se portan los mixins; se porta su salida. Es correcto porque todos se invocan un número fijo de veces con argumentos literales (`@include blg-base-theme()` una vez; `@include row-border(var(--blg-border-color))` una vez): no hay generación combinatoria, solo indirección. La salida ya está en el bundle commiteado y verificada.

Los cuatro shims `Form/*.import.scss` (41 líneas en total) existen únicamente para satisfacer la resolución de módulos de Sass: desaparecen sin sustituto.

**Nota aparte, no bloqueante:** `Tabs` y `Overlay` usan nombres de clase `mat-*` (35) y `cdk-*` (19) en lugar de `dnet-*` (576). Reprefijarlos rompería el markup de los consumidores; queda fuera de alcance (§17), pero conviene registrarlo como deuda para la próxima mayor.

### ADR-02 — CSS isolation: **retirada** ✔ decidido

El bundle global pasa a ser el mecanismo único de la librería.

**Corrección respecto a la v1 de este plan:** afirmé que el README no documentaba `{App}.styles.css`. **Es falso.** El README de `HEAD` lo documenta explícitamente y además dice que es necesario para `PickList`. Retiro ese argumento; era el más llamativo y era incorrecto.

La razón real, que se sostiene sola: **un solo mecanismo de distribución y un solo modelo de especificidad.** Dos caminos de entrega (`_content/…/dnet-blazor-styles.css` y `{App}.styles.css`) significan dos formas de que un consumidor se equivoque y dos órdenes de cascada que razonar. Y los selectores `[b-xxxxx]` que genera Blazor añaden especificidad de atributo a reglas que queremos que el consumidor pueda ganar sin `!important`.

**Esa pérdida de especificidad es intencional, no un efecto que haya que negar.** Blazor transforma `h1` en `h1[b-xxxxxxxxxx]`; al retirar la isolation, `.dnet-pick-list-item` pasa de (0,2,0) a (0,1,0). Es exactamente lo que buscamos. Lo que hay que verificar no es que la especificidad no cambie —cambiará—, sino que el resultado siga siendo correcto frente a CSS hostil de la app anfitriona.

**Coste real de la retirada, verificado:**

- Hay **un solo** `.razor.css` en toda la librería: `PickList/PickList.razor.css` (254 líneas).
- **Cero usos de `::deep`** en el repositorio, que es lo que habría hecho la migración delicada.
- Sus 13 selectores raíz están todos bajo `.dnet-pick-list*` y **ninguno colisiona** con las 1.037 reglas del bundle actual. Verificado uno a uno.

Es decir: mover el fichero al bundle es un cambio de ubicación, no una reescritura.

### ADR-03 — Prefijo único ✔ decidido

Se unifica en `--dnet-*`. Los `--blg-*` del Grid pasan a `--dnet-grid-*` como capa 3 apoyada en la capa 2.

**La retrocompatibilidad va en la cadena de lectura, no en una definición inversa.** La versión anterior de este plan proponía:

```css
/* ❌ INÚTIL: define el nombre viejo a partir del nuevo */
:root { --blg-background-color: var(--dnet-grid-background, var(--dnet-sys-surface)); }
```

No sirve de nada. El consumidor antiguo **escribe** `--blg-background-color`; no lo lee. Si las reglas del Grid ya consumen `--dnet-grid-background`, ese consumidor deja de tener efecto y la migración le rompe los estilos en silencio. Lo correcto es que el componente **lea** el nombre antiguo como eslabón intermedio:

```css
/* ✔ El nombre legacy es un eslabón de la cadena de lectura */
.dnet-grid {
  --_background: var(--dnet-grid-background,
                     var(--blg-background-color,
                         var(--dnet-sys-surface)));
  background-color: var(--_background);
}
```

Así el override nuevo gana, el antiguo sigue funcionando, y el defecto semántico actúa cuando no hay ninguno.

**Criterio de aceptación:** cada nombre legacy se prueba en **tres scopes** — `:root`, un contenedor intermedio y el atributo `style` de una instancia — y en los tres el valor antiguo debe seguir surtiendo efecto. Un test que solo pruebe `:root` no demuestra nada, porque la herencia es justo lo que puede romperse.

Los seis tokens sin prefijo se eliminan (son fantasma: nadie los declara).

La ventana de compatibilidad se retira en la siguiente versión mayor, no antes.

### ADR-04a — Unificar **nombres de token** entre familias ✔ decidido: sí

`List`, `Select` y `Autocomplete` declaran hoy 46 tokens (15 + 14 + 17) con nombres paralelos y valores casi iguales. Consecuencia práctica: para cambiar el alto de fila "de las listas" hay que escribir tres declaraciones y saber que existen tres. Eso contradice directamente el objetivo del proyecto.

Se unifican bajo `--dnet-list-*`. Igual para el trío de calendario bajo `--dnet-calendar-*`.

**La regla:** un token se unifica cuando *significa* lo mismo, no cuando *vale* lo mismo. "Alto de fila de una lista" es un único concepto y merece un único nombre.

**Unificar nombres no impone valores iguales.** Las diferencias reales sobreviven como **defectos dentro de la indirección privada**, nunca como declaración del token público:

```css
/* ❌ ROMPE LA HERENCIA: declarar el token público lo blinda contra :root */
.dnet-select-list-wrapper { --dnet-list-radius: var(--dnet-sys-radius-md); }

/* ✔ CORRECTO: el defecto vive en el fallback del var(), el token público solo se lee */
.dnet-list-wrapper        { --_radius: var(--dnet-list-radius, var(--dnet-sys-radius-lg)); }
.dnet-select-list-wrapper { --_radius: var(--dnet-list-radius, var(--dnet-sys-radius-md)); }

.dnet-list-wrapper,
.dnet-select-list-wrapper { border-radius: var(--_radius); }
```

Con la forma incorrecta, un consumidor que escriba `:root { --dnet-list-radius: 0 }` **pierde**: la declaración del propio componente sustituye al valor heredado. Con la correcta, ese `:root` sí afecta a los tres y cada componente conserva su defecto distinto. Es exactamente el patrón que `PickList` ya aplica bien, y la razón por la que R10 lo convierte en invariante comprobable.

Cada componente conserva su aspecto, pero el consumidor tiene **un** nombre que aprender y **un** sitio donde tocarlo para afectar a los tres, o un selector concreto si quiere afectar a uno.

Coste: renombrado de tokens públicos → los nombres antiguos entran en la cadena de lectura (ADR-03) y entrada en el `Changelog.md`. Sin cambios de markup. Reversible.

### ADR-04b — Unificar **reglas** en una hoja compartida ⏸ aplazado a la puerta de la Fase 4

Aquí es donde mi propuesta inicial se pasaba de frenada. Medido sobre el árbol real, normalizando los prefijos de clase:

| Comparación | Líneas idénticas | Solapamiento |
|---|---:|---:|
| `List` ∩ `Autocomplete` | 87 | ~65 % |
| `List` ∩ `Select` | 91 | ~65 % |
| **Las tres a la vez** | **85** | **~65 %** |
| `DatePickerWeek` ∩ `DatePickerWeekRaw` | 107 | ~68 % |

No son "tres copias del mismo fichero". Son **~65 % común y ~35 % genuinamente distinto**: `Select` tiene *trigger*, flecha y multi-valor; `Autocomplete` tiene sombra y truncado con elipsis; `List` tiene manejo de *drag* e iconos de ordenación.

Extraer una hoja compartida a un 65 % de solapamiento significa que esa hoja empieza a acumular excepciones desde el primer día. Así es exactamente como muere la sencillez que pides.

**Y, sobre todo, todavía no se puede medir bien.** Buena parte de lo que hoy cuenta como duplicación son *valores* duplicados, no *estructura* duplicada. Las fases 1–4 sustituyen esos valores por referencias a tokens; después de eso el número de solapamiento será otro. Puede subir mucho —y entonces extraer estará justificado— o puede quedar claro que la divergencia es estructural. **Se vuelve a medir al cerrar la Fase 4 y se decide con el dato, no con la intuición.**

Coste de esperar: cero. Coste de extraer ahora y equivocarse: una hoja compartida con tres juegos de excepciones dentro.

**Si más adelante se decide extraer**, hacerlo con una **clase compartida en el markup**, no con listas de selectores:

```css
/* Preferido: clase compartida. Cada componente añade solo sus diferencias. */
.dnet-listbox { … }
```
```razor
<div class="dnet-listbox dnet-select-list-wrapper">
```

Las listas de selectores (`.dnet-list-item, .dnet-select-list-item, .dnet-autocomplete-list-item { … }`) parecen más económicas porque no tocan el markup, pero acoplan los tres componentes: no se puede cambiar uno sin leer los otros dos. Es deuda disfrazada de ahorro.



### ADR-05 — `Dnet.Blazor.Material` ✔ decidido: se elimina el proyecto entero

Verificado antes de recomendarlo:

- **Ningún proyecto de la solución lo referencia.** Las únicas coincidencias de `Dnet.Blazor.Material` en el repositorio son sus propios `namespace`.
- **No se publica como NuGet**: su `.csproj` no define `PackageId` ni propiedades de empaquetado.
- Contiene 38 `.scss` (Angular Material vendorizado), 9 `.cs`, y **un** componente: `DnetFormFieldCmp.razor`.
- Sus estilos **no entran en ningún bundle** (cero referencias desde `dnet-blazor-styles`).

Es un huérfano completo. Se elimina el proyecto y su entrada en `DnetBlazorComponents.sln`. Si alguna vez se necesitara `DnetFormFieldCmp`, está en el historial de git; no hace falta conservarlo en el árbol por si acaso.

Bonus: esto elimina de golpe el 33 % de los ficheros `.scss` del repositorio, lo que reduce el alcance de la retirada de Sass antes de empezarla.


### ADR-06 — Alcance de un tema frente al portal global de `Overlay` ✔ decidido

**Este ADR no existía en la v1 y es el hueco más serio que tenía el plan.**

La promesa de §4 es que un tema o un override se aplican a cualquier subárbol del DOM y se heredan hacia abajo. Eso es cierto para el DOM… y los overlays de DnetBlazor **no están en el DOM donde se invocan**.

Verificado en `HEAD`: `DnetOverlay.razor` mantiene `Dictionary<int, Tuple<RenderFragment, OverlayConfig>>` y renderiza esos fragmentos dentro de su propio `<div>`, que el consumidor coloca una sola vez en `MainLayout.razor` (`<DnetOverlay BaseZindex="1100" />`, documentado en el README). `DnetOverlayHost.razor` los envuelve en `#cdk-overlay-host-{id}`.

Consecuencia directa: un `Dialog`, `Tooltip`, `ConnectedPanel`, `FloatingPanel` o `Toast` abierto desde dentro de

```razor
<div data-dnet-theme="dark"> … </div>
```

**no hereda ese tema**, porque su DOM cuelga del host global. Lo mismo vale para cualquier `--dnet-dialog-radius` puesto en un ancestro local. La herencia de custom properties sigue el árbol del DOM, no el árbol de componentes de Blazor.

**Decisión:**

1. **Se introduce un componente `DnetThemeScope`.** Es la pieza que faltaba: C# no puede saber que un ancestro del DOM lleva `data-dnet-theme`, así que el scope tiene que ser un componente y no un atributo suelto.

   ```razor
   <DnetThemeScope Theme="dark">
       <DnetButton … />
       <MiFormulario />
   </DnetThemeScope>
   ```

   Hace exactamente dos cosas, ambas triviales:

   ```razor
   <div data-dnet-theme="@Theme" class="@Class">
       <CascadingValue Value="@Theme" Name="DnetThemeScope" IsFixed="false">
           @ChildContent
       </CascadingValue>
   </div>
   ```

   El `<div>` resuelve la herencia CSS para todo lo que renderiza en su sitio; el `CascadingValue` resuelve el transporte para todo lo que se teletransporta al portal.

2. **Los componentes que abren overlays leen ese cascading value y lo escriben en `OverlayConfig.ThemeScope`.** `DnetOverlayHost` lo emite como `data-dnet-theme` en su propio `<div>`. Sin búsqueda en el DOM, sin JS, determinista y trivial de testear.

3. **Los overrides arbitrarios por custom property NO cruzan el portal.** No se intenta replicar el entorno de custom properties del origen: no hay forma de enumerar las heredadas, y el resultado sería mágico e impredecible. Solo el nombre del tema viaja.

4. **Para overrides arbitrarios se usa el canal explícito.** `OverlayConfig` ya tiene `PanelClass` y `BackdropClass`; se añade `PanelStyle` para inyectar custom properties por instancia.

5. **Se documenta como limitación conocida** en `docs/styling/theming-guide.md`. Una limitación documentada es aceptable; una promesa incumplida no.

Si el consumidor no usa `DnetThemeScope` y pone `data-dnet-theme` a mano en un `<div>`, el CSS funciona para todo lo que no sea un overlay. Es un modo degradado razonable y hay que decirlo en la guía.

**Criterios de aceptación**

- Test de navegador: `Dialog` abierto desde dentro de `<DnetThemeScope Theme="dark">` se renderiza en oscuro.
- Test de navegador: `Dialog` abierto desde un `<div data-dnet-theme="dark">` escrito a mano **no** hereda el tema. Es el comportamiento esperado y está documentado.
- Test de navegador: `--dnet-dialog-radius` en un ancestro local **no** afecta al diálogo, y el mismo valor vía `PanelStyle` **sí**.
- `DnetThemeScope` anidado: el más interno gana.
- La guía de temas incluye una sección explícita sobre overlays.

**Esfuerzo:** M. Es la única tarea del plan que toca C#, y `DnetThemeScope` es el único componente nuevo de toda la iniciativa.

---

## 6. Reglas invariantes

Son las reglas que la Fase 0 automatiza. Cualquier PR que las incumpla falla el build.

| # | Regla | Comprobación |
|---|---|---|
| R1 | Ningún fichero de componente contiene un literal de color, **incluidos los `fill`/`stroke` dentro de data-URI SVG** (ver R9) | grep + allowlist de assets decorativos |
| R2 | Ningún fichero de componente declara `:root` | grep |
| R3 | Todo token declarado se usa al menos una vez | script de auditoría |
| R4 | Todo token usado está declarado, o tiene fallback en el propio `var()` | script de auditoría |
| R5 | Tokens **públicos** `--dnet-(ref\|sys\|<comp>)-[a-z0-9-]+` en kebab-case. Tokens **privados** `--_[a-z0-9-]+`: se declaran y se consumen dentro del stylesheet de un mismo componente o familia; nunca los lee otro componente ni el consumidor. Nombres **legacy** (`--blg-*`, `--pick-list-*`) solo como eslabón de una cadena de compatibilidad | regex + validación de *ownership* por fichero |
| R6 | Los ficheros de componente no referencian `--dnet-ref-*` (solo `sys`, los suyos y legacy en cadena) | script |
| R7 | Ningún `!important` nuevo | diff contra baseline (hoy: 32) |
| R8 | Cada fichero de tema contiene **exactamente un** selector de scope y, dentro de él, únicamente declaraciones de custom properties | parser |
| **R10** | **Un fichero de componente nunca declara un token público. Solo declara `--_*` y solo lee `--dnet-*` vía `var()`** | grep `^\s*--dnet-` en `Components/**`, **excluyendo** `Components/Assets/styles/tokens/**` y `Components/Assets/styles/theme/**`, que son las únicas rutas autorizadas a declarar tokens públicos |

> **R10 es el invariante más importante de la lista.** Convierte en error de build el fallo que rompe la herencia: declarar `--dnet-list-radius: …` en `.dnet-select-list-wrapper` blinda ese componente contra cualquier `:root { --dnet-list-radius: … }` del consumidor. Es un fallo que **pasa todos los tests visuales** —el componente se ve bien— mientras incumple exactamente la flexibilidad prometida en la documentación. Sin R10, se descubriría al final de la migración o nunca.

**Sobre el alcance de los privados en R5.** La unidad de propiedad es el **fichero/componente**, no el selector. Un `--_hover` declarado en `.dnet-button` se consume legítimamente en `.dnet-button:hover`, en `.dnet-button-focus-overlay` y en cualquier descendiente del mismo stylesheet; `PickList` hace exactamente eso a lo largo de sus 13 selectores. Lo que R5 prohíbe es que `Dialog` lea un `--_x` de `Button`, o que un consumidor intente escribirlo. El linter valida propiedad por fichero: todo `--_x` consumido en un fichero debe declararse en ese mismo fichero o en la hoja de su familia.

**R9 — Iconos monocromos funcionales.** Ningún icono que forme parte de la interfaz puede llevar color fijo dentro de un data-URI. Se implementan con `mask-image` + `background-color: var(--_icon-color)`, no con `background-image`. La allowlist de R1 se reserva para assets genuinamente decorativos, que hoy no hay ninguno.

Alcance medido de R9: **40 data-URI SVG repartidos por 15 de las 32 familias** (`Tree`, `Grid` ×3, `DatePicker`, `DatePickerWeek`, `List`, `Toast`, `Dialog`, `AdminLayout/DesktopHeader`, `Stepper`, `Chips`, `Checkbox`, `ImageEditor`, `Form`), con `fill='black'`, `fill='grey'` y `fill='white'` incrustados. Si R1 los ignorara, sobrevivirían negros sobre superficie oscura y el tema oscuro nacería roto.

De paso: `Grid/_blgGridIcons.scss` y `_blg-advanced-filter.scss` usan `fill='5f6368'` **sin `#`**, que no es un color válido — esos iconos ya se renderizan negros hoy. Es un defecto latente que R9 corrige de camino.

---

## 7. Resumen de fases

| Fase | Resultado | Prioridad | Esfuerzo |
|---|---|---:|---:|
| 0 | Red de seguridad: linter de tokens + baseline visual | P0 | M |
| 1 | Capas 1 y 2 + corrección de defectos activos | P0 | M |
| 2 | Rodaja vertical: `Button` migrado y demostrado | P0 | S |
| 2b | Retirada de Sass (TOK-004/005/006), tras la puerta de fase | P1 | M |
| 3 | Unificación de **tokens** de las familias lista y calendario | P1 | S |
| 4 | Componentes restantes por lotes | P1 | XL |
| 5 | `AdminLayout`, `Grid` y scope de tema en `Overlay` (ADR-06) | P1 | L |
| 6 | Tema oscuro, playground y documentación | P1 | M |
| 7 | Limpieza, retrocompatibilidad y publicación | P2 | M |

Esfuerzo: **S** ≤ 2 días · **M** 3–5 días · **L** 1–2 semanas · **XL** > 2 semanas.

---

## 8. Fase 0 — Red de seguridad

Sin esto, la migración es una refactorización de 7.786 líneas de CSS a ciegas. Es la fase que hace el resto seguro.

### STY-001 — Script de auditoría de tokens

**Trabajo**

- `tools/css-tokens/audit.mjs`, Node puro, sin dependencias. Recorre los ficheros de estilo, extrae declaraciones (`^\s*--x:`) y usos (`var(--x`), y aplica R1–R10.
- Salida en dos modos: informe legible y `--ci` con código de salida distinto de cero.
- Fichero `tools/css-tokens/baseline.json` con las infracciones toleradas hoy, para poder activar el linter desde el minuto uno sin bloquear el repositorio. El contador solo puede bajar.

**Criterios de aceptación**

- Ejecutado sobre `HEAD` reporta exactamente los 45 tokens muertos y los 11 fantasma de §3.1.
- `npm run lint:css` está en `package.json` y en el workflow de `.github`.
- Añadir un token muerto en un PR hace fallar el build.

**Esfuerzo:** S

### STY-002 — Baseline visual **por estados**

**Trabajo**

- Usar el proyecto existente `tests/Dnet.Blazor.BrowserTests` (Playwright) para capturar los **estados** de cada componente, no una captura por página.
- Estados obligatorios donde apliquen: `default`, `hover`, `focus-visible`, `active/pressed`, `disabled`, `selected/checked`, `open/expanded`, y el estado con overlay abierto.
- Dos viewports, animaciones desactivadas, `prefers-reduced-motion` forzado.
- **Congelar los goldens oficiales en el mismo Linux/Chromium del CI.** Las capturas generadas en la máquina de un desarrollador no son autoridad: el antialiasing y la resolución de fuentes difieren y producen ruido que erosiona la confianza en la suite.
- Documentar el procedimiento de actualización deliberada del golden.

**Criterios de aceptación**

- Cobertura de estados para las 32 familias.
- **Prueba de la propia suite:** un golden de `hover` sobre `Button` debe fallar si se cambia `rgba(0,0,0,.08)`. Una captura en estado por defecto **no** detectaría ese cambio, y ese es precisamente el defecto que motiva toda la Fase 2 — una red que no lo pilla no es una red.
- La suite pasa en verde en dos ejecuciones consecutivas del CI, sin *flakiness*.

**Esfuerzo:** M

> Esta tarea es la que convierte todas las fases siguientes en verificables. Si hubiera que recortar el plan, esto es lo último que se recorta.

### STY-003 — Congelar el estado actual

**Trabajo**

- Etiquetar el commit base.
- Registrar en `docs/` las métricas de §3 como línea base (tamaño del bundle, nº de tokens, nº de `!important`, nº de literales).

**Esfuerzo:** S

---

## 9. Fase 1 — Capas de tokens y corrección de defectos

### TOK-001 — Crear la capa de primitivas

**Trabajo**

- `src/Dnet.Blazor/Components/Assets/styles/tokens/reference.css` con el contenido de §4 capa 1.
- Los valores salen de la auditoría de literales, no de invención: `#5f6368` (26 usos), `#ffffff` (79), `#ebebeb`, `#f2f2f2`, `#e1e3e1`, `#eee`, `#666666`, `#757575`, `#fafafa`, `#4fc3f7`, `#42b0d5`.

**Criterios de aceptación**

- Cada primitiva declarada corresponde a un literal que aparece ≥ 2 veces en el árbol actual.
- Ningún componente la referencia todavía (R6 se comprueba desde ya).

**Esfuerzo:** S

### TOK-002 — Crear la capa semántica

**Trabajo**

- `.../tokens/system.css` con el contenido de §4 capa 2.
- Absorber los 13 tokens de `Assets/scss/dnet-shared-vars.scss` mapeándolos: `--dnet-component-sm-font-size` → `--dnet-sys-text-md`, `--dnet-component-border-radius` → `--dnet-sys-radius-sm`, `--dnet-component-box-shadow` → `--dnet-sys-elevation-1`, etc.
- Los nombres de `dnet-shared-vars` quedan como **eslabón legacy en la cadena de lectura** de los componentes que los usaban, nunca como definición inversa (ADR-03). Marcado en el `Changelog.md`.

**Criterios de aceptación**

- ≤ 120 tokens en la capa semántica.
- Ningún token semántico contiene un literal de color; todos apuntan a capa 1 o a una composición (`rgb()`/`calc()`) de ella.
- El bundle compila y las capturas de STY-002 siguen en verde (aún no cambia nada visualmente).

**Esfuerzo:** M

### TOK-003 — Corregir los defectos activos de §3.5

**Trabajo**

- Sustituir `lighten(#000, 20%)` y `darken(rgb(0,36,61), 15%)` por valores calculados y fijados como primitivas. *Estos dos tokens están rotos en el CSS publicado hoy; su corrección es un cambio visual real y esperado.*
- Renombrar `--dnet-dash-asideWidth` → `--dnet-sidebar-width` y `--dnet-dash-asideWidth-minified` → `--dnet-sidebar-width-collapsed`, con el nombre antiguo como eslabón legacy en la cadena de lectura.
- Eliminar `--dnet-secundary-color` (muerto).
- Declarar o eliminar los 11 tokens fantasma. **No existe un "valor implícito" que recuperar:** cuando un `var()` sin fallback apunta a un token no declarado, la declaración entera queda *invalid at computed-value time* y la propiedad toma su valor heredado si es heredable, o el inicial si no lo es. No hay un valor pretendido guardado en ninguna parte. Para cada uno hay que **decidir explícitamente** si se elimina o cuál era el valor pretendido, deduciéndolo de las reglas circundantes, y **cada corrección se registra como cambio visual deliberado** en el PR.

  Estado verificado de los cinco que no son los seis sin prefijo:

  | Token | Situación real | Resolución |
  |---|---|---|
  | `--dnet-dialog-padding` | Usado 3 veces en `Dialog` sin fallback y sin declarar. `.dnet-dialog-content` intenta `margin: 0 calc(… * -1)` + `padding: 0 …`, el patrón clásico de sangrar el contenido y volver a acolcharlo. Hoy **ambas quedan en `0`** y el efecto no ocurre. | Casi con seguridad se pretendía `--dnet-dialog-padding-left-right` (24px, ya declarado). Restaurarlo **cambia el aspecto del diálogo**: hay que decidirlo a la vista, no asumirlo. |
  | `--blg-focus-color` | **Sí tiene fallback**: `var(--blg-focus-color, currentColor)`. No está roto. | No es un fantasma dañino. Se declara y se documenta como punto de extensión. Cumple R4 tal cual. |
  | `--dnet-datepicker-input-height` | Usado en `height` y `width` sin fallback → ambos quedan en `auto`. | Deducir el valor de las reglas hermanas del icono o eliminar las dos declaraciones. |
  | `--dnet-dash-floating-menu-left` | Usado en `left` sin fallback → `auto`. | Ídem. |
  | `--dnet-dash-positioning-helper-color` | Usado en `color`, que **sí es heredable** → hoy hereda el color del padre. | Probablemente sea eso lo que se quiere: eliminar la declaración es lo más honesto. |

  Los seis sin prefijo (`--header-color`, `--footer-color`, `--content-color`, `--foreground-color`, `--left-column-color`, `--right-column-color`) se eliminan junto a sus usos.

**Criterios de aceptación**

- `grep -E "lighten\(|darken\(" wwwroot/dnet-blazor-styles.css` no devuelve nada.
- Cero tokens fantasma en el informe de STY-001 (declarados, o eliminados con sus usos, o con fallback explícito en el `var()`).
- **Cada uno de los 11 lleva su resolución escrita en el PR**, con captura antes/después cuando el cambio es visible. `--dnet-dialog-padding` en particular requiere decisión a la vista, no deducción sobre el papel.
- Los diffs visuales de STY-002 se revisan uno a uno y cada cambio queda justificado por escrito.

**Esfuerzo:** S

### TOK-004 — Retirar Sass: vía mecánica (28 familias)

**Trabajo**

- Renombrar `.scss` → `.css` por lotes alineados con las fases 2–5. **No en un solo golpe**: cada lote va con su verificación STY-002.
- `@use "x.scss"` / `@forward` → `@import "x.css"`.
- 272 comentarios `//` → `/* */`.
- Quitar el guion bajo de los 40 partials (`_x.scss` → `x.css`).
- El anidamiento se mantiene literal: `&.clase`, `&:hover`, `&::after` y el anidamiento por descendencia son compatibles con CSS nativo tal cual están escritos.
- Quitar los BOM (`\ufeff`) que encabezan la mayoría de ficheros.

**Criterios de aceptación**

- `grep -rnE '&[-_a-zA-Z0-9]' src/Dnet.Blazor/Components --include=*.css` devuelve cero (hoy ya es cero; es una comprobación de no regresión).
- Cero `.scss` restantes en el lote.
- STY-002 en verde.

**Esfuerzo:** M

### TOK-005 — Retirar Sass: vía de congelado (`Form`, `Tabs`, `Grid`, `Overlay`)

**Trabajo**

1. Compilar con `outputStyle: 'expanded'` — la opción ya está comentada en `webpack.app.css.js`, solo hay que descomentarla.
2. Extraer del resultado la sección correspondiente a cada una de las 4 áreas.
3. Limpiar los prefijos de proveedor que `autoprefixer` volverá a generar (si no, se duplican).
4. Sustituir los ficheros fuente por esa salida, ya como `.css` plano.
5. Borrar los mixins, funciones y shims `*.import.scss`, que quedan sin consumidores.

**Criterios de aceptación**

- Equivalencia demostrada: recompilar y comparar el bundle **normalizado en espacios y formato, conservando el orden de las reglas**, contra la línea base de STY-003. El orden de la cascada es semántica, no ruido: ordenar antes de comparar ocultaría exactamente el tipo de regresión que esta comparación existe para detectar.
- Cero `@mixin`, `@include`, `@function`, `@return`, `@content` y cero variables `$` en `src/Dnet.Blazor`.
- Los ficheros congelados llevan una cabecera de comentario indicando su origen y que ya no se regeneran.

**Esfuerzo:** M

> Estas dos tareas se hacen **después** de TOK-001/002 y de la Fase 2, no antes. La rodaja vertical de `Button` se valida sobre `.scss` para no mezclar el cambio de arquitectura de tokens con el cambio de sintaxis. Un solo eje de cambio por PR.

### TOK-006 — Actualizar el pipeline

**Trabajo**

- `webpack.app.css.js`: eliminar `sass-loader`, conservar `css-loader` + `postcss-loader` (autoprefixer) + `MiniCssExtractPlugin`. El `test: /\.scss$/` pasa a `/\.css$/`.
- Eliminar `sass` de `package.json`.
- El entry `samples/Dnet.Shared/assets/scss/site.scss` y su `_mixins.scss` generan las clases utilitarias del **sample** con `@each`. No son de la librería. Se convierten una sola vez a CSS estático generado y se congelan igual que TOK-005, o se mantienen fuera del alcance si prefieres dejar el sample como está — decisión de bajo impacto, pero hay que tomarla para poder borrar `sass`.

**Criterios de aceptación**

- `npm ls sass` no devuelve nada.
- Cero ficheros `.scss` en el repositorio.
- `npm run buildDnetBlazor` produce un bundle equivalente al de la línea base.

**Esfuerzo:** S

---

## 10. Fase 2 — Rodaja vertical: `Button`

Un solo componente, completo, demostrado. No se pasa a la Fase 3 hasta que esto esté cerrado. `Button` es el candidato correcto: pequeño (~70 líneas), tiene tokens muertos que arreglar, y lo consumen `Paginator`, `Dialog` y `AdminLayout`, así que valida propagación real.

### VER-001 — Migrar `Button` a la arquitectura de tres capas

**Trabajo**

- Reescribir `Button/dnet-button.css` según el ejemplo de §4 capa 3.
- Conectar `--dnet-button-hover-color` y `--dnet-button-hover-opacity` (hoy muertos): se sustituyen por `--dnet-btn-background-hover`, que **sí** se consume. Los nombres antiguos entran como eslabón legacy en la cadena de lectura: `var(--dnet-btn-background-hover, var(--dnet-button-hover-color, var(--dnet-sys-state-hover)))`.
- Eliminar el `:root` del fichero. Los defectos viven en `.dnet-button`.
- Sustituir los literales `rgba(0,0,0,.05)`, `rgba(0,0,0,.08)`, `#000` por tokens semánticos.
- `.dnet-icon-button` pasa a ser una variante que **solo redefine los privados**, nunca los públicos (R10):
  ```css
  .dnet-icon-button {
    --_pad:       var(--dnet-btn-padding, 0);
    --_radius:    var(--dnet-btn-radius, var(--dnet-sys-radius-pill));
    --_min-width: var(--dnet-btn-min-width, 0);
  }
  ```
  **Cero reglas nuevas.** Si la variante declarase `--dnet-btn-radius` directamente, un `:root { --dnet-btn-radius: 0 }` del consumidor volvería a perder contra ella — el mismo fallo que R10 existe para impedir. Es la prueba de que la capa 3 es suficiente **y** de que el patrón se aplica igual en variantes que en componentes.

**Criterios de aceptación**

1. `Button/*.css` no contiene ningún literal de color ni ningún `:root`.
2. STY-002 en verde: apariencia idéntica.
3. Se demuestran en el sample los tres niveles de override de §4:
   - global vía `:root`,
   - por subárbol vía una clase contenedora,
   - por instancia vía atributo `style`.
4. `data-dnet-theme="dark"` aplicado a `<html>` cambia el botón sin tocar la librería.
5. `.dnet-icon-button` no introduce ninguna regla nueva, solo redefiniciones de privados, y `:root { --dnet-btn-radius: 0 }` **también le afecta a él**. Si la variante ignora ese `:root`, la tarea no está terminada.

**Esfuerzo:** S

### VER-002 — Documentar el patrón como contrato

**Trabajo**

- `docs/styling/component-token-contract.md`: la plantilla exacta que debe seguir todo componente, con `Button` como ejemplo trabajado.
- Reglas de nomenclatura: `--dnet-<comp>-<propiedad>[-<estado>]`. Estados: `-hover`, `-active`, `-disabled`, `-focus`, `-selected`.
- Actualizar `AGENTS.md` para que cualquier agente que toque estilos lea este contrato antes.

**Criterios de aceptación**

- Un desarrollador que no conozca el repositorio puede migrar un componente siguiendo solo ese documento.
- El documento incluye qué **no** debe exponerse como token (no todo es API; exponer 40 tokens por componente es lo mismo que exponer cero).

**Esfuerzo:** S

> **Puerta de fase.** Si al terminar VER-001 el componente necesitó reglas o `!important` que la arquitectura no previó, se corrige la arquitectura aquí — no en la fase 4, con 30 componentes ya migrados.

---

## 11. Fase 3 — Unificación de la familia lista

La deuda más rentable de pagar, ahora acotada a lo que de verdad rinde (ADR-04a). La extracción de reglas compartidas **no** entra aquí (ADR-04b).

### LST-001 — Unificar los nombres de token de la familia lista

**Trabajo**

- Colapsar `--dnet-list-*`, `--dnet-select-list-*` y `--dnet-autocomplete-list-*` en un único juego `--dnet-list-*` de ≤ 16 tokens.
- Cada componente conserva sus valores actuales **como fallback dentro de su indirección privada**, nunca como declaración del token público (ADR-04a, R10):
  ```css
  .dnet-select-list-wrapper {
    --_radius:    var(--dnet-list-radius, var(--dnet-sys-radius-md));
    --_elevation: var(--dnet-list-elevation, var(--dnet-sys-elevation-1));
  }
  ```
- Los ficheros y las clases **no se tocan**. Solo cambian los nombres de token dentro de cada uno.
- Los 46 nombres antiguos entran en la cadena de lectura como eslabón intermedio (ADR-03), no como definición inversa.

**Criterios de aceptación**

- 46 declaraciones de token → ≤ 16, sin ningún literal.
- **`:root { --dnet-list-item-height: 40px }` afecta a los tres componentes.** Hoy hacen falta tres declaraciones. Este es el criterio que demuestra que la tarea cumplió su propósito, y el que fallaría si se cometiera el error de R10.
- El mismo override funciona en los tres scopes: `:root`, contenedor intermedio e instancia vía `style`.
- Los nombres antiguos siguen funcionando en los tres scopes.
- Cada componente conserva píxel a píxel su aspecto: STY-002 en verde.
- Ningún fichero de componente gana ni pierde reglas en esta tarea. Si el diff toca reglas, es que se está colando ADR-04b.

**Esfuerzo:** S

> El cambio de esfuerzo respecto a la versión anterior del plan (era **M** con extracción de reglas incluida) es real: separar ADR-04a de ADR-04b convierte la tarea de mayor riesgo de la Fase 3 en la más barata.

### LST-002 — Absorber `PickList`

**Trabajo**

- Mover `PickList.razor.css` (254 líneas) al bundle global (ADR-02). Es un cambio de ubicación: sus 13 selectores raíz no colisionan con ninguna de las 1.037 reglas del bundle, y no hay `::deep` en el repositorio.
- Reemplazar su cadena de *fallbacks* de compatibilidad `var(--pick-list-x, var(--dnet-list-x, literal))` por el patrón canónico `var(--dnet-picklist-x, var(--dnet-list-x))`, ya sin literales.
- Mantener `--pick-list-*` como eslabón legacy en la cadena de lectura durante una versión mayor.
- Retirar del `.csproj` cualquier configuración de scoped CSS que quede huérfana y comprobar que el sample sigue funcionando sin `{App}.styles.css` para los estilos de la librería.

**Criterios de aceptación**

- Cero ficheros `.razor.css` en `src/Dnet.Blazor`.
- Un consumidor que siga el README ve `PickList` correctamente estilado con una sola hoja enlazada.
- **La especificidad de los selectores de `PickList` baja** al perder el atributo `[b-xxxxx]` (por ejemplo `.dnet-pick-list-item` pasa de (0,2,0) a (0,1,0)). Es intencional. Lo que se verifica es el resultado, no la invariancia: existe un test con **CSS hostil de una app anfitriona** (reglas de clase única sobre `div`, `input`, `button` y sobre `.dnet-*`) y `PickList` sigue renderizándose correctamente.
- `PickList` hereda el tema oscuro sin cambios propios.

**Esfuerzo:** S

### LST-003 — Unificar los tokens de la familia calendario

Mismo tratamiento, mismo alcance: `--dnet-calendar-*`, `--dnet-datepickerweekca-*` y `--dnet-datepickerweekca-raw-*` colapsan en `--dnet-calendar-*`. Sin tocar reglas.

Estas tres hojas suman 1.204 líneas y son las más grandes de la librería después del Grid, así que también son las que más se benefician de la tokenización de la Fase 4. Su solapamiento medido (~68 % entre `DatePickerWeek` y `DatePickerWeekRaw`) se vuelve a medir en la puerta de la Fase 4.

**Esfuerzo:** S

### LST-004 — Medir de nuevo el solapamiento (entrada de ADR-04b)

**Trabajo**

- Reejecutar la comparación normalizada de §ADR-04b sobre las hojas ya tokenizadas, al cerrar la Fase 4.
- Registrar el resultado en `docs/styling/`.

**Criterio de decisión**

- Solapamiento ≥ 85 % → se extrae hoja compartida, con clase compartida en el markup.
- Solapamiento < 85 % → **no se extrae**. La duplicación restante es estructural y separada es más simple de mantener que compartida con excepciones.

**Esfuerzo:** S

---

## 12. Fase 4 — Componentes restantes

Se migran en lotes por afinidad, un PR por lote, cada uno con su verificación STY-002. Ningún lote empieza antes de que el anterior esté en verde.

| Lote | Componentes | Esfuerzo |
|---|---|---:|
| 4A — Controles de formulario | `Checkbox`, `RadioButton`, `Form`, `Chips` | M |
| 4B — Superposición | `Overlay`, `Dialog`, `Tooltip`, `Toast`, `FloatingPanel`, `ConnectedPanel` | M |
| 4C — Navegación | `Tabs`, `Stepper`, `DynamicStepper`, `ExpansionPanel`, `Tree`, `Paginator` | M |
| 4D — Resto | `Spinner`, `ImageEditor`, `FloatingDoubleList` | S |

**Criterios de aceptación por lote**

1. Cero literales de color y cero `:root` en los ficheros del lote.
2. Cada componente expone **≤ 12** tokens públicos documentados. No hay mínimo: un `Spinner` con 2 tokens está bien. Más de 12 requiere justificación escrita en el PR, y normalmente significa que falta apoyarse en la capa semántica.
3. STY-002 en verde.
4. Todos los componentes del lote responden a `data-dnet-theme="dark"` sin reglas específicas de tema.
5. El contador de infracciones en `baseline.json` baja; nunca sube.

**Nota sobre iconos (R9), transversal a todos los lotes:** hay **40 data-URI SVG en 15 de las 32 familias** con `fill` fijo (`black`, `grey`, `white`). No es un caso aislado de `Checkbox` como decía la v1 de este plan: `Tree`, `Grid`, `DatePicker`, `DatePickerWeek`, `List`, `Toast`, `Dialog`, `AdminLayout/DesktopHeader`, `Stepper`, `Chips`, `ImageEditor` y `Form` tienen el mismo problema. Todos se convierten de `background-image` a `mask-image` + `background-color: var(--_icon-color)`, con `--_icon-color` derivado de `--dnet-sys-on-surface` o del token del componente. `Checkbox` ya tiene `--dnet-checkbox-checkmark-path` declarado: se aprovecha como plantilla del patrón.

Si esta conversión no se hace en el mismo lote que la tokenización del componente, el tema oscuro de la Fase 6 nace roto y la corrección hay que hacerla dos veces.

**Esfuerzo total fase:** XL

---

## 13. Fase 5 — `AdminLayout`, `Grid` y scope de tema en `Overlay`

Los dos subsistemas con tema propio. Se dejan para el final porque son los que más superficie tienen y porque ya funcionan razonablemente.

### SUB-001 — `AdminLayout`

**Trabajo**

- Reducir los 49 tokens de `_theme-variables.scss`. Los que son puro color se sustituyen por semánticos; solo sobreviven como capa 3 los que son estructurales y específicos del layout: alturas de header/footer, anchos de columna, ancho de scrollbar, paddings de nivel de menú.
- Los 12 tokens muertos de este fichero se eliminan o se conectan.
- Fijar la escala de padding de menú sobre `--dnet-sys-space-*` en lugar de `em` sueltos.

**Criterios de aceptación**

- ≤ 20 tokens en capa 3 de `AdminLayout`, todos vivos.
- El layout responde al tema oscuro completo, incluidos scrollbars.

**Esfuerzo:** M

### SUB-002 — `Grid`

**Trabajo**

- `_blg-base-theme-vars.scss` se convierte en la capa 3 del Grid, renombrada a `--dnet-grid-*` y apoyada en la capa 2.
- `_blg-arcadia-theme-vars.scss` deja de ser un "tema" y pasa a ser lo que realmente es: valores por defecto del Grid. Los temas viven en `theme/`.
- Los nombres `--blg-*` entran como eslabón legacy en la cadena de lectura de cada regla del Grid (ADR-03). Nunca como definición inversa.
- Conservar la escala derivada `calc(var(--dnet-grid-unit) * n)`, que ya es correcta, reapuntándola a `--dnet-sys-space-unit`.

**Criterios de aceptación**

- Ningún fichero del Grid declara `:root`.
- Las hojas `_blg-*-theme.scss` no contienen literales.
- Los nombres `--blg-*` siguen surtiendo efecto para consumidores existentes, verificado en los tres scopes.

**Esfuerzo:** M

### SUB-003 — Transportar el scope de tema al portal de `Overlay` (ADR-06)

**Trabajo**

- Crear el componente `DnetThemeScope` (`<div data-dnet-theme>` + `CascadingValue`), según ADR-06.
- Añadir `ThemeScope` y `PanelStyle` a `OverlayConfig`; `DnetOverlayHost` emite `data-dnet-theme` en su `<div>`.
- Los componentes que abren overlays (`Dialog`, `Tooltip`, `ConnectedPanel`, `FloatingPanel`, `Toast`) leen el cascading value y lo escriben en la config.
- Usar `DnetThemeScope` en el playground de THM-003 para demostrar dos temas simultáneos en la misma página.

**Criterios de aceptación**

- Los de ADR-06, verificados con tests de navegador.
- La limitación sobre overrides arbitrarios queda escrita en `docs/styling/theming-guide.md`, no solo en el código.

**Esfuerzo:** M — única tarea del plan que toca C#.

---

## 14. Fase 6 — Temas, playground y documentación

### THM-001 — Tema oscuro

Es la prueba de carga de toda la arquitectura, no un extra.

**Criterios de aceptación**

- `theme/dark.css` ≤ 60 declaraciones y **cero selectores** distintos de `[data-dnet-theme="dark"]`.
- Los 32 componentes se ven correctos en oscuro sin una sola regla específica de componente.
- Contraste AA verificado en texto y bordes.
- Si algún componente necesita una regla especial para el modo oscuro, **es un defecto de tokenización de ese componente** y se corrige ahí, no en el tema.

**Esfuerzo:** S

### THM-002 — Temas de demostración adicionales

Dos temas más para demostrar los ejes distintos:

- `theme/compact.css`: solo toca densidad (`--dnet-sys-space-unit`, `--dnet-sys-control-height`, escala tipográfica). Debe reescalar toda la librería con ~6 declaraciones, y **sin salirse de la capa `system`**.
- `theme/high-contrast.css`: solo color y grosor de borde.

**Criterio de aceptación:** `compact.css` no contiene ningún color y `high-contrast.css` ningún tamaño. Si esa separación no es posible, la capa 2 está mal cortada.

**Esfuerzo:** S

### THM-003 — Página de playground en el sample

**Trabajo**

- Página `/theming` en `samples/Dnet.Shared` que renderiza cada componente y ofrece controles en vivo para los tokens semánticos, escribiendo sobre `document.documentElement.style`.
- Selector de tema con los cuatro temas.
- Botón "copiar CSS del tema actual" que genera el fichero de tema listo para pegar.

**Criterios de aceptación**

- Un usuario construye un tema completo sin escribir CSS a mano y se lleva el fichero.
- La página no usa ninguna librería externa.

**Esfuerzo:** M

### THM-004 — Documentación

**Trabajo**

- `docs/styling/README.md`: el modelo de tres capas en una página, con el diagrama de §4.
- `docs/styling/tokens.md`: tabla completa de tokens semánticos generada **automáticamente** por el script de STY-001 a partir del CSS, para que no se desincronice.
- `docs/styling/theming-guide.md`: los tres niveles de override, cómo crear un tema, cómo cambiar de tema en runtime.
- Una sección de tokens en la documentación de cada componente.
- Actualizar el README con el enlace de la hoja y la nota de temas.

**Criterios de aceptación**

- `tokens.md` se regenera en CI y un PR que cambie tokens sin regenerarlo falla.
- La guía incluye un ejemplo copiable y funcional por cada uno de los tres niveles.

**Esfuerzo:** M

---

## 15. Fase 7 — Limpieza y publicación

### CLN-001 — Retirar restos

**Trabajo**

- Eliminar el proyecto `src/Dnet.Blazor.Material` completo y su entrada en `DnetBlazorComponents.sln` (ADR-05, verificado sin consumidores ni publicación NuGet). **Conviene hacerlo al principio, no al final**: quita 38 de los 116 `.scss` del repositorio antes de que la retirada de Sass tenga que ocuparse de ellos.
- Borrar de la raíz del repositorio: `Another Split content css.css`, `Untitled-3.scss`, `BlgBodyNotBlgRow.txt`, `BlgBodyPlusBlgRow.txt`, `image-viewer-pan.html`.
- Borrar `ConnectedPanel/wwwroot/dnet-connected-panel*.css` (tres copias, una minificada, ninguna en el bundle) tras confirmar que no se sirven.
- Eliminar el `@use` comentado de `FormField` en `dnet-blazor-styles`.

**Criterios de aceptación**

- Cero ficheros de estilo huérfanos: todo `.css` del repositorio o entra en un bundle, o es de un sample, o está justificado en `docs/`.

**Esfuerzo:** S

### CLN-002 — Versión mayor y retrocompatibilidad

**Esto es `Dnet.Blazor` 6.0.0, no un 5.x.** El paquete actual es 5.0.5 y aquí se cambia deliberadamente una API pública (los nombres de los tokens de estilado) y el mecanismo de distribución de `PickList`. Publicarlo como versión menor sería incorrecto: un consumidor que actualice sin leer el changelog verá romperse sus overrides.

**Trabajo**

- Bump a 6.0.0 en el `.csproj`, el README y el `Changelog.md`, con la tabla completa de renombrados de token.
- La compatibilidad **no** se implementa con un fichero de alias `old → new`, que no funciona (ADR-03). Los nombres antiguos viven como eslabón intermedio en la cadena de lectura de cada componente, dentro del propio bundle. No hay nada que el consumidor tenga que enlazar.
- Guía de migración en `docs/styling/migration-5-to-6.md`.
- Marcar la ventana de compatibilidad para retirada en 7.0.

**Criterios de aceptación**

- Una app 5.x con overrides propios de `--blg-*`, `--pick-list-*` o `--dnet-select-list-*` sigue funcionando **sin cambios ni ficheros adicionales** tras actualizar a 6.0.
- Test automatizado que ejerce cada nombre legacy en los tres scopes (`:root`, contenedor, instancia).

**Esfuerzo:** S

### CLN-003 — Cierre

**Criterios de aceptación finales**

1. `npm run lint:css` pasa sin `baseline.json` (el fichero se borra), incluidos R9 y R10.
2. Cero literales de color en ficheros de componente, data-URI incluidos.
3. Cero `:root` fuera de `tokens/` y `theme/`.
4. Cero tokens muertos, cero tokens fantasma.
5. Cuatro temas funcionando sobre los 32 componentes, overlays incluidos (ADR-06).
6. **Presupuesto de tamaño sobre el bundle comprimido**, no sobre el crudo: gzip y brotli dentro del +10 % respecto a la línea base de STY-003. La indirección por custom properties puede hacer crecer ligeramente el CSS sin comprimir aunque la arquitectura sea mejor, y esa indirección comprime muy bien por repetición. Imponer `≤ 142 KB` en crudo, como decía la v1, presionaría en contra de la arquitectura que se está construyendo.
7. Nº de `!important` igual o menor que 32.
8. STY-002 en verde con los goldens de todos los estados actualizados y revisados.

**Esfuerzo:** S

---

## 16. Estructura de ficheros resultante

```
src/Dnet.Blazor/Components/Assets/styles/
├── tokens/
│   ├── reference.css        ← capa 1: paleta y escalas crudas
│   └── system.css           ← capa 2: roles. UN tema = un clon de esto
├── theme/
│   ├── dark.css             ← ~50 líneas
│   ├── compact.css          ← ~6 líneas, solo densidad
│   ├── high-contrast.css
├── base/
│   ├── reset.css            ← el box-sizing hoy repetido en varios ficheros
│   └── a11y.css             ← foco visible, prefers-reduced-motion
├── patterns/                ← puede no existir; depende de ADR-04b
│   └── listbox.css          ← SOLO si ADR-04b resuelve extraer (§LST-004)
└── dnet-blazor-styles.css   ← punto de entrada: solo @import, en orden

src/Dnet.Blazor/Components/<Componente>/
└── dnet-<componente>.css    ← capa 3 + reglas. Cero literales, cero :root

tools/css-tokens/
├── audit.mjs                ← linter, Node puro, sin dependencias
└── baseline.json            ← contador de deuda, solo puede bajar
```

**Orden de `@import` en el punto de entrada.** Es significativo y debe estar comentado en el fichero: `tokens/reference` → `tokens/system` → `base/*` → `patterns/*` → componentes → (los temas se enlazan aparte por el consumidor, siempre después del bundle).

---

## 17. Fuera de alcance

Explícitamente **no** entran, para que la sencillez pedida sobreviva:

- Cualquier framework o librería CSS, incluidas las de solo tokens.
- CSS-in-JS o generación de estilos en runtime desde C#.
- Utilidades atómicas al estilo Tailwind. El `_mixins.scss` del sample que genera `.dnet-p-10`, `.dnet-w-200` etc. es del sample y no se promueve a la librería.
- Rediseño visual. La apariencia por defecto no cambia (salvo los defectos de TOK-003).
- Cambios en el markup de los componentes, salvo los estrictamente necesarios en LST-001.
- Sistema de plugins de temas, registro de temas o *theme manager* en C#. Un tema sigue siendo un fichero CSS que se enlaza con `<link>` y se activa con un atributo. **Única excepción:** el componente `DnetThemeScope` de ADR-06, que no gestiona temas —solo delimita un scope y transporta su nombre a través del portal de `Overlay`—. Es la pieza mínima que hace falta para que la promesa de scoping sea cierta también en overlays.
- Soporte de navegadores sin custom properties.

---

## 18. Riesgos

| Riesgo | Impacto | Mitigación |
|---|---|---|
| Regresión visual silenciosa en un componente poco usado | Alto | STY-002 es requisito previo a todo. Ninguna migración sin captura golden. |
| El anidamiento nativo rompe casos `&-suffix` | **Descartado** | Verificado: cero ocurrencias en el árbol actual. Se mantiene el grep en TOK-004 como no regresión. |
| Un componente declara su propio token público y rompe la herencia sin que nada falle | **Alto** | R10 lo convierte en error de build. Es el único fallo del plan que pasa todos los tests visuales mientras incumple la promesa central. |
| Los overlays no heredan el scope de tema por el portal global | Alto | ADR-06: `ThemeScope` en `OverlayConfig` + tests de navegador + limitación documentada para overrides arbitrarios. |
| Los iconos data-URI sobreviven negros al tema oscuro | Alto | R9 con alcance medido (40 SVG / 15 familias) y conversión a `mask-image` en el mismo lote que la tokenización de cada componente. |
| Un consumidor 5.x pierde sus overrides al actualizar | Alto | Nombres legacy en la cadena de lectura (ADR-03), no alias inversos; test en tres scopes; versión 6.0.0 y guía de migración. |
| El baseline visual no detecta regresiones de estado | Alto | STY-002 captura estados, no páginas. Su propio criterio de aceptación es detectar el bug de `Button:hover`. |
| El congelado de TOK-005 pierde comportamiento de los mixins vendorizados | Medio | Comparación del bundle **conservando el orden de las reglas** contra la línea base. |
| Los ficheros congelados se editan a mano después y divergen | Bajo | Cabecera de comentario explícita en cada uno + son ficheros que la Fase 4/5 va a tokenizar de todas formas, momento en que dejan de ser "generados". |
| Navegador antiguo sin anidamiento nativo de CSS | Bajo | Baseline desde 2024. Si apareciera un requisito, `postcss-nesting` se añade al pipeline existente sin tocar el código fuente. |
| La capa semántica se corta mal y los componentes necesitan escaparse | Alto | Es exactamente lo que la puerta de fase de VER-001 y el criterio de THM-001 detectan. Si aparece, se corrige la capa 2, no se parchea el componente. |
| Sobre-tokenización: 40 tokens por componente | Medio | Límite duro de 12 tokens públicos por componente en los criterios de la fase 4. Un token existe cuando alguien lo va a cambiar, no cuando podría. |
| La migración se queda a medias y conviven dos arquitecturas | Alto | `baseline.json` monotónico decreciente + la fase 4 va por lotes cerrados, no en paralelo. |

---

## 19. Orden recomendado de ejecución

Todos los ADR están cerrados salvo ADR-04b, que por diseño se decide con datos al final de la Fase 4. Nada bloquea el arranque.

0. **CLN-001, parcialmente adelantada:** eliminar `Dnet.Blazor.Material`. Es la única tarea de limpieza que conviene hacer antes que nada, porque reduce el alcance de todo lo demás en un 33 % de los ficheros de estilo.
1. **Fase 0 completa**, en particular STY-002. Es el único requisito duro previo al resto.
2. Fase 1, tareas TOK-001 → TOK-003, con revisión explícita de los diffs visuales de TOK-003. **Todavía sobre `.scss`.**
3. Fase 2 y **puerta de fase**. Aquí se decide si la arquitectura de tokens aguanta. También sobre `.scss`.
4. TOK-004 → TOK-006: retirada de Sass, ya con la arquitectura validada y la red de seguridad probada en un componente real.
5. Fase 3: unificación de tokens (LST-001 a LST-003). Barata y de alto retorno.
6. Fase 4 por lotes, cerrando con LST-004 (la medición que resuelve ADR-04b).
7. Fase 5.
8. Fase 6, con THM-001 como validación real del conjunto.
9. Fase 7, el resto de la limpieza.

**El orden importa.** La tentación es empezar por la conversión a CSS plano, porque es la tarea más visible y la más mecánica. Sería un error: convertir 116 ficheros de sintaxis y *después* descubrir en la Fase 2 que la capa semántica está mal cortada obliga a rehacer el trabajo sobre el doble de superficie. Primero se valida la arquitectura sobre un componente, luego se cambia la sintaxis de todos. Un solo eje de cambio por PR.
