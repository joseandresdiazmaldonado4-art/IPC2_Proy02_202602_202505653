# IPC2_Proy02_202602_202505653

Núcleo del catálogo de biblioteca desarrollado en C#.

## Contenido

- `Modelos`: clases `Libro`, `Categoria` y `Resultado`.
- `Estructuras`: nodos y listas doblemente enlazadas creadas desde cero.
- `Servicios`: catálogo y lector de archivos XML.
- `Principal.cs`: carga de uno o varios archivos y pruebas locales.

El código no utiliza colecciones nativas de C# ni LINQ.

## Ejecución

```powershell
dotnet run
```

Para cargar archivos en una misma ejecución:

```powershell
dotnet run -- categorias.xml libros.xml
```

Para vaciar el catálogo antes de un archivo posterior:

```powershell
dotnet run -- categorias.xml --reiniciar nuevo-catalogo.xml
```

El catálogo se conserva en memoria durante la ejecución del programa. Sin argumentos se ejecutan las pruebas locales.

Cada archivo debe tener una raíz `<config>`. Las secciones `<listaCategorias>` y `<listaLibros>` son opcionales. Las categorías utilizan texto para su nombre y el atributo opcional `padre`; los libros contienen los elementos `ISBN`, `titulo`, `autor` y `categoria`.
