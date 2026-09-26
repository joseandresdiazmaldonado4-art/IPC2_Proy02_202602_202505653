# IPC2_Proy02_202602_202505653

Sistema web para administrar un catálogo jerárquico de categorías y libros, desarrollado con C# y ASP.NET Core.

## Contenido

- `Modelos`: clases `Libro`, `Categoria` y `Resultado`.
- `Estructuras`: nodos y listas doblemente enlazadas creadas desde cero.
- `Servicios`: catálogo y lector de archivos XML.
- `Inicio.cs`: configuración de la aplicación web y rutas del sistema.
- `wwwroot`: interfaz web, estilos y comportamiento del navegador.

El código no utiliza colecciones nativas de C# ni LINQ.

## Ejecución

```powershell
dotnet run
```

La consola mostrará una dirección local, por ejemplo `http://localhost:5261`. Abra esa dirección en el navegador.

## Funciones

- Carga incremental de archivos XML.
- Registro de categorías principales y subcategorías.
- Registro, búsqueda y eliminación de libros.
- Consulta del ISBN inmediatamente menor y mayor.
- Reinicio completo del catálogo en memoria.
- Reporte de toda la jerarquía de categorías.
- Reporte de una categoría y sus descendientes.
- Reporte de libros ordenados por ISBN dentro de una categoría.
- Exportación del código DOT y de las imágenes PNG en `wwwroot/reportes`.

## Formato XML

```xml
<config>
  <listaCategorias>
    <categoria>Ciencia</categoria>
    <categoria padre="Ciencia">Física</categoria>
  </listaCategorias>
  <listaLibros>
    <libro>
      <ISBN>978001</ISBN>
      <titulo>Fundamentos de física</titulo>
      <autor>Ana López</autor>
      <categoria>Física</categoria>
    </libro>
  </listaLibros>
</config>
```

Las secciones `listaCategorias` y `listaLibros` son opcionales. Puede cargar varios archivos sin perder la información anterior. Active la opción de reinicio en la interfaz cuando necesite reemplazar todo el catálogo.

## Reportes

La aplicación requiere que el comando `dot` esté disponible en el sistema. En Windows puede instalar la herramienta desde la [documentación oficial](https://graphviz.org/download/). Los reportes se generan desde la sección **Reportes** y ofrecen enlaces para ver el código DOT y descargar la imagen.

## Uso en Visual Studio

1. Abra `BibliotecaWeb.csproj` desde Visual Studio.
2. Espere a que se restauren los componentes de .NET.
3. Ejecute el proyecto con el botón de inicio.
4. Use `ejemplo.xml` para comprobar la carga inicial.

El catálogo se almacena en memoria y se vacía al cerrar la aplicación. Las estructuras dinámicas del catálogo se implementan con nodos propios, sin colecciones nativas ni LINQ.
