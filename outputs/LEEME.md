# Uruguay Demon List · Etapa 1 UI/UX

Abrí index.html en un navegador. Los cuatro archivos de la aplicación deben permanecer juntos:

- index.html: estructura y enlaces a recursos.
- udl-styles.css: estilos, responsive y animaciones.
- udl-app.js: vistas públicas y datos iniciales de niveles y jugadores.
- udl-account.js: simulación de cuentas, submissions, moderación y administración.

## Probar el recorrido completo

1. Entrá en Ingresar y elegí Usuario en Probar un rol. Se abre la cuenta ficticia AndrewS-15.
2. Usá Enviar récord. Escribí el nombre del nivel y seleccioná una opción del buscador. Completá porcentaje, video y FPS. Raw footage y notas son opcionales.
3. El envío queda Pendiente en Mis submissions.
4. Cerrá sesión desde el menú del usuario. En Ingresar, elegí Moderador.
5. Entrá a Moderación desde el menú. Revisá el envío y aprobalo o rechazalo. La aprobación pide confirmación; el rechazo requiere un motivo.
6. Volvé a ingresar como Usuario para ver el resultado en Mis submissions. Una aprobación modifica el perfil, la lista local y los puntos de la demo. Un rechazo muestra su motivo.
7. Para revisar usuarios y cambiar roles, ingresá mediante el acceso Administrador y abrí Administración. También puede moderar. El rol propio no se puede modificar en esta demo.

No recargues entre pasos: la simulación existe solo en memoria.

## Pantallas

Se mantienen Inicio, Lista uruguaya, detalles de niveles, Jugadores, perfiles públicos y Actividad. Se agregan Clasificación, Acerca de, Reglas, registro, ingreso, recuperación, Mi perfil, cuenta, envío, confirmación, Mis submissions, moderación, revisión individual y administración.

Mi perfil tiene navegación para Perfil, Completados, Progresos, Submissions y Cuenta. Los perfiles públicos no muestran emails ni historial privado. Los emails de ejemplo aparecen enmascarados solo en las vistas privadas correspondientes.

## Formularios y sesión de demostración

El registro permite crear un nombre ficticio hasta recargar. Para probar el ingreso usá ese nombre y cualquier contraseña ficticia no vacía. No hay verificación de credenciales: es una demostración visual, no autenticación.

El email y la contraseña escritos en registro no se conservan en el modelo. No se usan cookies, localStorage, base de datos, OAuth, APIs ni emails reales. Recuperar y cambiar contraseña solo muestran sus estados visuales. Discord es una opción futura deshabilitada.

Los roles y restricciones se simulan en el navegador para recorrer las pantallas. No constituyen seguridad real; se implementarán en el servidor en una etapa posterior.

## Datos de ejemplo

Los puestos AREDL, puntos, fechas, cuentas y récords son ficticios. La lista uruguaya contiene únicamente niveles con una completación uruguaya aceptada al 100%. Aprobar una primera completación puede incorporar un nuevo nivel a esa lista. Una misma completación no suma puntos dos veces.

Las reglas son provisionales. El desempate real queda pendiente. Los videos iniciales son espacios de muestra; los enlaces ingresados en submissions pueden abrirse manualmente y no se consultan automáticamente.

## Validación realizada

Se comprobó la sintaxis de ambos archivos JavaScript y se ejecutaron verificaciones de rutas públicas y privadas, roles, errores de formulario, envío, rechazo sin motivo, aprobación, actualización de puntos, duplicados y privacidad del perfil mediante un DOM simulado. No se realizó una revisión visual automatizada en navegador.

El diseño sigue usando el sistema oscuro/celeste existente, listas, separadores finos y formularios simples. Se incluyeron adaptaciones para móvil de formularios, navegación, dropdown, submissions y usuarios administrativos. Sin frameworks nuevos.

## Imágenes y acentos uruguayos

Las filas de niveles y completados usan capturas de referencia como fondo. Las imágenes se distribuyen localmente en assets/. El sol aparece como marca de agua muy tenue. Se agregaron acentos amarillos pequeños.

Fuentes de las capturas (miniaturas de fotogramas de YouTube, conservan los derechos de sus autores):
- Bloodbath: https://www.youtube.com/watch?v=twTw4fjT0ik
- Acu, neigefeu: https://www.youtube.com/watch?v=z6l74Mkoxm8
- Cataclysm: https://www.youtube.com/watch?v=UtQnr47L7Q0
- Zodiac: https://www.youtube.com/watch?v=rVMzHiyp9oE
- Sol de Mayo: https://commons.wikimedia.org/wiki/File:Sol_de_Mayo-Bandera_de_Uruguay.svg — dominio público, Pumbaa80.

## Raw footage

Obligatorio para puestos AREDL 1–250 inclusive, tanto para progresos como para completaciones. Opcional a partir del 251. La etiqueta del formulario cambia con el nivel seleccionado; se valida el envío y se impide aprobar sin un enlace válido cuando corresponde. Los puestos AREDL de esta demo siguen siendo ficticios. Zodiac (#70 de ejemplo) permite probar esta validación.

Verificación adicional: límites 1, 250 y 251 y rechazo del formulario al faltar raw footage en el top 250.

## Ajuste de ambiente competitivo

Tipografía más firme en títulos y puestos, botones con esquinas asimétricas, acentos laterales de ranking y una cuadrícula tenue en la portada. Se mantiene el diseño original y sus colores.

Sol de Mayo ampliado con relación de aspecto 1:1 y background-size: contain, evitando deformación horizontal. En pantallas pequeñas se reduce la opacidad y se desplaza fuera del contenido.

Inspiración visual consultada: https://uy-demonlist.vercel.app/ (solo composición y carácter visual; sin copiar sus funciones ni sus reglas).
