# Publicar `Dnet.Blazor` en NuGet.org

El flujo [publish-nuget.yml](../.github/workflows/publish-nuget.yml) genera los
assets de frontend, restaura, compila y prueba toda la solución antes de crear y
publicar el paquete `Dnet.Blazor`. La versión de publicación procede de la
etiqueta `vX.Y.Z`, no del valor de respaldo definido en el `.csproj`.

La autenticación usa [Trusted Publishing de NuGet.org](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing): GitHub emite un token OIDC de corta duración y NuGet.org lo canjea por una clave de publicación temporal. No se almacena una clave de API permanente en el repositorio.

## Configuración única

1. En el repositorio de GitHub, crea el entorno **`nuget`**. Añade los revisores
   requeridos que deban aprobar una publicación y configura sus secretos en ese
   entorno.
2. En NuGet.org, crea una política de *Trusted Publishing* para:

   | Campo | Valor |
   | --- | --- |
   | Proveedor | GitHub Actions |
   | Organización/propietario | `datalnet` |
   | Repositorio | `DnetBlazor` |
   | Archivo de workflow | `publish-nuget.yml` |
   | Entorno de GitHub | `nuget` |

3. Añade el secreto **`NUGET_USER`** al entorno `nuget`, con el nombre de usuario
   de la cuenta de NuGet.org que creó la política. No uses su correo electrónico
   ni una clave API.

La primera publicación debe ser realizada por un propietario del paquete en
NuGet.org. Las versiones posteriores podrán usar esta misma política mientras
conserven los valores anteriores.

## Publicar una versión

Desde `main`, cuando el contenido y las notas de versión estén listos:

```bash
git tag v5.0.3
git push origin v5.0.3
```

La etiqueta inicia el flujo y la publicación quedará esperando la aprobación del
entorno `nuget`, si este la requiere. También se puede ejecutar manualmente desde
la pestaña **Actions** indicando una versión sin el prefijo `v`; en ese caso hay
que seleccionar explícitamente la rama o el commit que se desea publicar.

El flujo conserva el `.nupkg` como artefacto durante 90 días. NuGet.org no permite
reemplazar una versión ya publicada: `--skip-duplicate` hace segura una repetición
del workflow, pero no vuelve a publicar el mismo paquete.

Las versiones preliminares se publican igual, por ejemplo con la etiqueta
`v5.0.3-rc.1`.
