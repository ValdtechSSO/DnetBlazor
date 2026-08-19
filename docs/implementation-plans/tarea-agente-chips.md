# Tarea: migrar `Chips` a la arquitectura de tokens

Repo `DnetBlazor`, rama `Feature_20260819_AMU_Grid_Responsive`.
Fichero único a tocar: `src/Dnet.Blazor/Components/Chips/dnet-chip-list.css`.

Este es un **lote de calentamiento**: 16 infracciones, el componente más pequeño
con los cuatro tipos de problema representados. El patrón que establezcas aquí
se replicará en los 30 componentes restantes, así que prioriza hacerlo canónico
sobre hacerlo rápido.

---

## Contexto imprescindible

Lee antes de escribir nada:

- `docs/implementation-plans/dnet-blazor-styling-architecture-plan.md`, secciones
  **§2 (principios)**, **§4 (arquitectura)**, **§5 ADR-03** y **§6 (reglas R1–R10)**.
- `src/Dnet.Blazor/Components/Button/dnet-button.css` — **es la referencia
  canónica**. Ya está migrado y correcto. Copia su patrón exactamente.
- `src/Dnet.Blazor/Components/Assets/styles/tokens/system.css` — los 40 tokens
  semánticos disponibles. No inventes nombres nuevos en esta capa.

## La regla que más importa (R10)

Un fichero de componente **nunca declara un token público**. Solo declara
privados `--_x` y solo **lee** los públicos vía `var()`:

```css
/* ❌ Rompe la herencia: un :root del consumidor pierde contra esto */
:root { --dnet-chips-padding: 7px 12px; }

/* ✔ El defecto vive en el fallback; el token público solo se lee */
.dnet-standard-chip {
  --_padding: var(--dnet-chips-padding, 7px 12px);
  padding: var(--_padding);
}
```

Si te encuentras escribiendo `--dnet-<algo>:` en este fichero, párate: es una
infracción.

## Las 16 infracciones concretas

| Regla | Nº | Qué hay |
|---|---:|---|
| R10 público declarado | 9 | Todo el bloque `:root` de la cabecera |
| R3 token muerto | 5 | `--dnet-chips-icon-height`, `-icon-width`, `-icon-background-size-{md,sm,xs}` |
| R1 literal de color | 1 | `#e0e0e0` y `rgba(0,0,0,.87)` en `.dnet-chip.dnet-standard-chip` |
| R2 `:root` en componente | 1 | El bloque de la cabecera |

## Qué hacer, punto por punto

1. **Eliminar el bloque `:root`.** Cada token pasa a ser el fallback de un
   privado declarado en el selector que lo usa.

2. **Cadena de lectura para los legacy (ADR-03).** Dos tokens apuntan hoy a
   nombres antiguos declarados en `Assets/styles/base/shared.css`:
   `--dnet-component-sm-font-size` (0.875rem) y `--dnet-component-border-radius`
   (4px). El nombre antiguo va **como eslabón intermedio**, nunca como
   definición inversa:

   ```css
   --_font-size: var(--dnet-chips-font-size,
                     var(--dnet-component-sm-font-size,
                         var(--dnet-sys-text-md)));
   --_radius:    var(--dnet-chips-radius,
                     var(--dnet-component-border-radius,
                         var(--dnet-sys-radius-sm)));
   ```

   Un consumidor que hoy escribe `--dnet-component-border-radius` debe seguir
   viendo efecto tras el cambio.

3. **Tokenizar los colores (R1).** `#e0e0e0` y `rgba(0,0,0,.87)` deben salir del
   fichero. Mapea a `system`; si ningún token semántico encaja, dilo en el PR en
   vez de inventar uno nuevo en la capa `sys`.

4. **Resolver los 5 tokens muertos.** Ojo, hay un bug real:
   `--dnet-chips-icon-background-size-md` está **declarado dos veces**
   (`1.25em` y luego `1em`); gana el segundo. Decide si el icono debe usarlos
   —y entonces conéctalos— o si sobran —y entonces bórralos—. **No los dejes
   declarados sin usar.**

5. **Techo de API: ≤ 12 tokens públicos**, sin mínimo. Un token existe cuando
   alguien lo va a cambiar, no cuando podría. Si crees que hacen falta más de 12,
   justifícalo en el PR.

## Criterios de aceptación

```bash
npm run buildDnetBlazor                              # debe pasar
node tools/css-tokens/audit.mjs --write-baseline     # total < 396
npm run lint:css                                     # 0 new
```

Además, comprobable a mano en el sample:

- `:root { --dnet-chips-radius: 0 }` cambia el radio de los chips.
- Lo mismo puesto en un contenedor intermedio afecta solo a ese subárbol.
- Lo mismo en el atributo `style` de una instancia afecta solo a esa instancia.
- `--dnet-component-border-radius` (nombre antiguo) sigue surtiendo efecto.
- Con `data-dnet-theme="dark"` en `<html>`, los chips se ven correctos **sin
  añadir ninguna regla específica de tema**. Si hiciera falta una, es que la
  tokenización del punto 3 está incompleta.

## Restricciones

- **No toques ningún otro fichero.** Ni `system.css`, ni `reference.css`, ni
  otros componentes, ni el `baseline.json` a mano (regenéralo con el comando).
- **No cambies el aspecto por defecto.** Salvo lo que se derive de resolver el
  punto 4, que debe ir explicado en el PR con captura antes/después.
- **No añadas `!important`.**
- No hay aún capturas visuales automatizadas en la rama, así que revisa el
  render a ojo en el sample antes de dar por cerrada la tarea.

## Entregable

Un commit con el fichero migrado y el `baseline.json` regenerado, y un mensaje
de PR que incluya: el total antes/después, la decisión tomada en el punto 4, y
cualquier color del punto 3 que no hayas podido mapear a un token semántico.
