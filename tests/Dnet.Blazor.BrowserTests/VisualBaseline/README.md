# STY-002 — Baseline visual por estados

Red de seguridad visual de la migración de estilos (Fase 0 del plan de
arquitectura). Captura los **estados** de cada componente, no una captura por
página: `default`, `hover`, `focus-visible`, `selected` y `open`, a dos
viewports y con animaciones desactivadas y `prefers-reduced-motion` forzado.

El linter de tokens (`tools/css-tokens/audit.mjs`) no detecta nada visual. Los
cinco defectos de `Chips` pasaron lint, build y contador; esta suite existe para
que una migración no avance a ciegas con el CI en verde.

## Cómo funciona

- **Playwright .NET** (`tests/Dnet.Blazor.BrowserTests`), Chromium headless,
  contra la app de muestra ya arrancada (`Dnet.App.ClientSide`, puerto 5101).
- La comparación de imágenes está implementada en `VisualGoldenComparer.cs`
  (Playwright .NET no expone `toHaveScreenshot`): decode PNG puro + diff por
  píxel con umbral por canal.
- Los goldens se guardan **por plataforma** en
  `VisualBaseline/goldens/{linux|darwin|win32}/`. El renderizado de texto
  difiere entre SO: un golden de macOS no es autoridad en Linux.
- Las páginas con datos resuelven `sample-data/person_500.json` contra el
  fixture local (`Fixtures/person_500.json`); los goldens nunca dependen de la
  red ni del contenido remoto.
- Los estados `open` que renderizan en el portal global de `Overlay`
  (Select, Autocomplete, DatePicker, Dialog, Tooltip, Toast, paneles…) se
  capturan como página completa; el resto como elemento.

## Requisitos

1. La app de muestra corriendo:

   ```bash
   dotnet run --project samples/Dnet.ClientSide/Dnet.App.ClientSide.csproj
   ```

2. Chromium de Playwright instalado (ya lo hace el CI):

   ```bash
   tests/Dnet.Blazor.BrowserTests/install-playwright.sh --with-deps chromium
   ```

## Ejecutar

```bash
dotnet test tests/Dnet.Blazor.BrowserTests \
  --filter "FullyQualifiedName~VisualBaseline" \
  -e DNET_BLAZOR_VISUAL_TESTS=true \
  -e DNET_BLAZOR_BASE_URL=http://127.0.0.1:5101
```

La suite es **opt-in** (`DNET_BLAZOR_VISUAL_TESTS=true`): el CI principal solo
activa `DNET_BLAZOR_BROWSER_TESTS`, así que el build no depende de goldens
hasta que se congelen los oficiales de Linux.

## Congelar / actualizar goldens (procedimiento deliberado)

Los goldens se regeneran **solo** cuando el cambio visual es intencional y ha
sido revisado:

```bash
DNET_BLAZOR_UPDATE_GOLDENS=true dotnet test tests/Dnet.Blazor.BrowserTests \
  --filter "FullyQualifiedName~VisualBaseline" \
  -e DNET_BLAZOR_VISUAL_TESTS=true \
  -e DNET_BLAZOR_BASE_URL=http://127.0.0.1:5101
```

Regla de oro: **un golden solo se actualiza en el mismo commit que el cambio
visual que lo produce**, nunca para «hacer verde» un diff no revisado. Cada
actualización debe revisarse dif por dif (el comparador informa del número de
píxeles y del máximo por canal).

### Goldens oficiales (CI Linux/Chromium)

Los goldens de la máquina de un desarrollador no son autoridad (antialiasing y
fuentes difieren). El procedimiento para congelar los oficiales:

1. Ejecutar la suite en el runner Linux/Chromium del CI con
   `DNET_BLAZOR_UPDATE_GOLDENS=true` y commitear los `goldens/linux/*`.
2. A partir de ahí, el CI compara contra esos ficheros.

Hasta que existan los `goldens/linux/*`, el build principal no ejecuta esta
suite (variable de activación separada).

## Cobertura y exclusiones conocidas

Cubiertos: Button, PickList, Chips (migrados) y AdminLayout, Autocomplete,
Select, Checkbox, RadioButton, DatePicker, Tabs, Stepper, DynamicStepper,
ExpansionPanel, FloatingPanel, ConnectedPanel, FloatingDoubleList, Spinner,
List, Toast, Tooltip, Tree, Grid, Forms (sin migrar).

Exclusiones deliberadas (documentadas, no olvidadas):

- **ImageEditor**: renderizado por canvas con imagen remota; no determinista
  entre plataformas ni con la red. Cuando el componente migre, decidir si se
  congela un golden con imagen local fija.
- **DatePickerWeek / DatePickerWeekRaw**: sin página de muestra en
  `Dnet.ClientSide`.
- **Paginator**: sin instancia propia en las páginas sample (se ejercita dentro
  de List/Grid); capturarlo requiere una página demo propia.
- **Assets**: no es un componente; es la capa base del bundle.
- **Grid (BlGrid)**: el renderizado con virtual scroll no es aún determinista
  para goldens — las filas visibles difieren entre ejecuciones. Su estabilidad
  funcional la cubre `GridPerformanceBaselineTests`; volver a intentar el golden
  cuando el renderizado se asiente.
- **Spinner (estado abierto)**: el spinner es timer-driven (aparece/desaparece
  con `SpinnerService`); incluso con animaciones desactivadas el instante de
  captura no es determinista. Se captura la página en su estado por defecto.
- **Toast (estado abierto)**: el toast usa un contador de 1 s y una posición
  apilada (`ToastService._positionTracker`); el instante de captura y el
  offset varían entre ejecuciones (verificado con diffs intermitentes de
  ~6 000 px). Se captura la página en su estado por defecto.
- **DatePicker (estado abierto)**: el calendario muestra variación subpixel
  intermitente en la columna de días (diffs de 200-400 px entre ejecuciones);
  se captura solo el input.
- **Tooltip y AdminLayout en móvil**: los targets de la página de Tooltip
  están posicionados en absoluto y se solapan en viewport estrecho; el sidebar
  de AdminLayout colapsa a un drawer. El golden de escritorio cubre el shell.

El Grid espera a que las filas estén renderizadas (`ReadyFunction`) y el
harness espera `document.fonts.ready` en cada captura para evitar el jitter de
intercambio de fuentes web.

## Prueba de la propia suite

El criterio de aceptación de STY-002: un golden de `hover` sobre `Button` debe
fallar si cambia su color de hover (el defecto que motiva la Fase 2 — el default
y el hover de `Button` caen hoy ambos en `--dnet-sys-state-hover`, así que los
goldens `button-hover` y `button-default` son idénticos por construcción).

Para demostrarlo, se aplica temporalmente cualquier override al color de hover
(añadiendo una regla como `.dnet-button:hover { background-color: red !important; }`
en el harness) y se ejecuta la suite: los goldens `button/hover` deben fallar.
Una captura en estado por defecto no detectaría ese cambio.

## Ajuste de tolerancia

Cada escenario acepta `MaxDiffPixels` y `ChannelThreshold`. Los valores por
defecto (0 píxeles, umbral 8 por canal) son deliberadamente estrictos: si un
golden tiembla entre ejecuciones en la misma plataforma, es inestabilidad que
hay que conocer antes de que una migración dependa de él, no un umbral que
subir en silencio.
