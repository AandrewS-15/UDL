# Navegación pública de UDL

## Arquitectura
- Rutas MVC GET `/players/{id}` y `/levels/{id}` en PublicDetailsController; respuesta 404 para registros inexistentes.
- PlayerProfileService reutiliza PlayerRankingQuery: posición y totales exactamente iguales al ranking. Una segunda consulta obtiene exclusivamente completions al 100%, ordenadas por AREDL y nombre. Hardest es la primera completion de ese orden.
- LevelDetailsService ejecuta tres consultas controladas: metadatos, IDs de la Lista Uruguay ordenados y victors con sus puntos totales. Reutiliza UruguayLevelListQuery y PlayerRankingQuery. Victors ordenados por puntos totales DESC y nombre ASC.
- Todas las lecturas usan AsNoTracking; no se llama SaveChanges ni se añaden columnas. Posiciones, puntos totales, hardest, categoría y conteos no se persisten.
- Vistas Razor con ViewModels, encabezado compartido y componentes de avatar/completion. Enlaces nativos entre niveles y jugadores. La fila del ranking responde al clic en cualquier celda y mantiene un enlace accesible por teclado en el nombre.
- Se conservan colores, tipografía, navbar y componentes. CSS adicional limitado a banners, tamaños de avatar, estadísticas y responsive. Se adaptaron selectores de los iconos del navbar a enlaces con ruta absoluta.

## Thumbnails: fuente verificada
Fuente pública: https://github.com/All-Rated-Extreme-Demon-List/Thumbnails
README: repo de thumbnails usado por el frontend AREDL V3, con versiones full y cards y actualizaciones automatizadas.
Código verificado: https://github.com/All-Rated-Extreme-Demon-List/Thumbnails/blob/main/src/image/levels.ts
El generador utiliza `level.level_id`, consulta la fuente de thumbnails de Geode en resolución high y publica `levels/full/{level_id}.webp`.
URL encapsulada en LevelPresentation.Thumbnail:
`https://raw.githubusercontent.com/All-Rated-Extreme-Demon-List/Thumbnails/main/levels/full/{GeometryDashId}.webp`
Sakupen Circles (76962930) y Tartarus (59075347) responden HTTP 200. La imagen de Tartarus observada en navegador tiene 1920 px de ancho. Sin scraping visual ni descarga de imágenes al proyecto. Si la fuente externa no dispone de una imagen, se retira el elemento y queda el fondo UDL. Avatares ausentes o fallidos muestran una inicial.

## Ejemplos verificados contra SQL independiente
| Jugador | ID | UY | Puntos | Completions | Hardest |
|---|---:|---:|---:|---:|---|
| Santiricca | 51 | 1 | 7918.8 | 66 | Tartarus (AREDL 108) |
| cxve | 12 | 2 | 5483 | 65 | Thinking Space (AREDL 181) |
| Fif | 20 | 3 | 4820 | 44 | Zodiac (AREDL 164) |

| Nivel | ID | UY | AREDL | Puntos | Victors por orden |
|---|---:|---:|---:|---:|---|
| Sakupen Circles | 46 | 1 | 43 | 4519.3 | Ardizz |
| Tartarus | 111 | 3 | 108 | 1588 | Santiricca, Salva |

La prueba de integración de solo lectura contrasta los 66 perfiles y los 250 detalles con consultas SQL independientes: totales, conteos, posiciones, hardest, IDs de victors y avatares. También verifica registros inexistentes y un nivel sin completions (sin posición UY).

## Archivos de esta etapa
Nuevos:
- Controllers/PublicDetailsController.cs
- Services/Ranking/PlayerProfileService.cs
- Services/Levels/LevelDetailsService.cs
- Services/Levels/LevelPresentation.cs
- ViewModels/PlayerProfileViewModel.cs
- ViewModels/LevelDetailsViewModel.cs
- Views/PublicDetails/Player.cshtml, Level.cshtml, NotFound.cshtml
- Views/Shared/_PublicLayout.cshtml, _PublicHeader.cshtml, _Avatar.cshtml, _Completion.cshtml
- wwwroot/udl-public-details.js
- tests/UDL.Public.Checks/UDL.Public.Checks.csproj y Program.cs
- docs/public-navigation.md
Modificados:
- Program.cs
- Views/Home/Index.cshtml
- Services/Levels/UruguayLevelListQuery.cs
- ViewModels/UruguayLevelListItemViewModel.cs
- wwwroot/udl-ranking.js
- wwwroot/udl-level-list.js
- wwwroot/udl-styles.css

Las modificaciones anteriores de udl-app.js, udl-account.js y otros archivos de la Lista Uruguay pertenecen a la etapa previa; no se ampliaron los sistemas de demostración de login, moderación ni submissions.

## Evidencia local
`work/public-validation/service-checks.txt` contiene los resultados de la integración.
`work/public-validation/sql-before.txt` y `sql-after.txt` contienen hashes de datos y columnas para comprobar que SQL no cambió.

## Cierre de validación (2026-09-12)
- Compilación Release: 0 errores y 0 advertencias.
- Pasan los cinco proyectos de checks: Aredl, Import, Ranking, LevelList y Public.
- Navegador: entrada desde una celda de la fila de Santiricca, desde la Lista Uruguay a Sakupen Circles, perfiles de cxve/Fif, Tartarus con Santiricca y Salva en ese orden, banners y avatares cargados.
- Responsive a 390 px: sin desbordamiento horizontal en perfil y detalle; menú móvil funcional.
- Etiquetas JSON presentadas como badges; nunca como código en la página.
- SQL intacto: informes antes/después idénticos, SHA256 CA46116E091B453D813EE674016AF353AA000298129ECFC1961E9DC38A4AED9D. Sin cambios de schema, sin escrituras y sin imágenes guardadas en el proyecto.
