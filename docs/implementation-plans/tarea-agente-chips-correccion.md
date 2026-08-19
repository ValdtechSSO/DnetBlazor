# Tarea: corregir la migración de `Chips` (commit `65b21d6`)

La estructura de tokens del commit es correcta y se conserva. Las cadenas de
lectura legacy están bien construidas. Lo que sigue son cuatro defectos
detectados en revisión; los tres primeros son regresiones que hay que arreglar,
el cuarto es una decisión de diseño que **no debes tomar tú**.

Los tres criterios automáticos (lint, build, contador 412→396) pasaron con estas
regresiones dentro. No sirven como prueba de que el componente esté bien.

---

## 1. El path del SVG de la máscara está corrupto (regresión visible)

En `.dnet-chips-icon-remove` las dos declaraciones llevan paths **distintos**:

- `-webkit-mask-image` → el path original, correcto.
- `mask-image` (sin prefijo) → termina en `…L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z`,
  que **no aparece en ninguna versión anterior del fichero**.

La versión sin prefijo es la estándar y es la que aplican Chrome y Safari
actuales, así que la equis del botón de eliminar se dibuja mal.

**Arreglo:** el path bueno es el de `-webkit-mask-image`. Cópialo literalmente,
no lo transcribas. Verifica con un `grep` que ambas declaraciones son idénticas
carácter a carácter antes de commitear.

## 2. Los tokens de tamaño de icono no estaban muertos (regresión funcional)

Se borraron `--dnet-chips-icon-background-size-{md,sm,xs}` por «no afectar al
render». Sí afectaban, por otra vía: `DnetChip.razor` líneas 27 y 37 inyectan

```razor
<span style="background-size:@_valuesBySize.BackGroundSize" class="dnet-chips-icon …">
```

con cuatro valores según `ChipSize`: `1em` (Large), `1rem` (Medium),
`0.8em` (ExtraSmall), `0.875em` (default).

Al pasar de `background-image` a `mask-image`, ese `background-size` inline
**deja de controlar el icono**, que queda clavado en `mask-size: 1em`. Los chips
pierden el escalado del icono en tres de sus cuatro tamaños.

**Arreglo:** el tamaño de la máscara debe seguir al mismo valor. Opción
preferida: que el markup inyecte una custom property en lugar de
`background-size`, y que el CSS haga `mask-size: var(--_icon-scale)`. Si tocas el
`.razor`, mantén `background-size` **además**, porque `.dnet-chips-icon` sigue
usándose con clases de icono arbitrarias del consumidor (`AvatarIcon`,
`RemoveIcon`), que sí son `background-image` y no deben romperse.

## 3. Cambió el aspecto por defecto sin declararlo

| | Antes | Ahora |
|---|---|---|
| Fondo | `#e0e0e0` | `--dnet-sys-surface-hover` → `#f2f2f2` |
| Texto | `rgba(0,0,0,.87)` | `--dnet-sys-on-surface` → `#5f6368` |

El chip queda más claro y el texto pierde bastante contraste. El encargo pedía
no cambiar el aspecto por defecto salvo justificándolo con captura
antes/después.

**No decidas esto tú.** Prepara las dos variantes y deja que se elija:

- **(a) Conservar el aspecto:** añadir a `system.css` los tokens semánticos que
  falten para representar `#e0e0e0` y un `on-surface` de alto contraste.
- **(b) Aceptar el cambio:** dejarlo como está, aportando captura antes/después
  y comprobación de contraste AA del texto sobre el nuevo fondo.

Reporta cuál recomiendas y por qué, pero implementa solo la que se te confirme.

## 4. Los tokens nuevos de padding y font-size están muertos al nacer

Este es el defecto de fondo y no estaba en el encargo original.

`DnetChip.razor` línea 96 construye un `style` inline con `font-size`, `padding`,
`min-height`, `background-color` y `color` a partir de `ChipSize`. **Un `style`
inline gana a cualquier regla de hoja de estilos.** Por tanto:

- `--dnet-chips-padding` → un consumidor que lo escriba **no verá ningún efecto**.
- `--dnet-chips-font-size` → igual.
- `--dnet-chips-background` / `-foreground` → solo funcionan cuando el consumidor
  no pasa `BackgroungColor`/`Color`, porque entonces el inline se omite.

Es decir: el componente expone tres tokens públicos que no cumplen lo que
prometen. Eso es peor que no exponerlos.

La solución correcta es que las variantes de tamaño dejen de escribirse como
`style` inline y pasen a ser **clases** (`.dnet-chip-size-sm`, `-md`, `-lg`,
`-xs`) que definan los fallbacks de los privados en el CSS. Entonces la cascada
funciona y el token del consumidor gana.

**No lo implementes todavía.** Toca `.razor` y afecta a la API pública del
componente; requiere decisión. Lo que sí debes hacer en esta tarea:

- Documentar en el PR exactamente qué tokens quedan inertes por el inline.
- Proponer el mapeo `ChipSize` → clase, sin aplicarlo.

## Criterios de aceptación

```bash
npm run buildDnetBlazor
npm run lint:css                                # 396 violation(s), 0 new
```

Y a mano, en el sample:

- El icono de eliminar se dibuja **igual que antes** de la migración. Compáralo
  con el render de `f598dc9`.
- Un chip de cada tamaño (`Large`, `Medium`, `Small`, `ExtraSmall`) muestra el
  icono escalado como antes.
- Un `AvatarIcon` o `RemoveIcon` personalizado del consumidor sigue
  renderizándose.
- Con `data-dnet-theme="dark"`, el icono de eliminar cambia de color (esa era la
  ganancia real de pasar a `mask-image`).

## Restricciones

- No toques `system.css` salvo que se confirme la opción (a) del punto 3.
- No toques otros componentes.
- El contador del baseline no puede subir de 396.
- Si algo del punto 2 o 4 exige tocar `DnetChip.razor` más allá de inyectar una
  custom property, párate y pregunta antes de escribir.
