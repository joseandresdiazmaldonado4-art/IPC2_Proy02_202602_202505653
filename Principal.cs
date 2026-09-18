using System.Text;
using BibliotecaWeb.Servicios;

namespace BibliotecaWeb;

internal static class Principal
{
public static void Main(string[] argumentos)
{
if (argumentos.Length == 0)
{
    ProbarCatalogo();
    Console.WriteLine("Todas las pruebas se ejecutaron correctamente.");
    return;
}

var catalogo = new GestorCatalogo();
var lector = new LectorXml(catalogo);
for (var posicion = 0; posicion < argumentos.Length; posicion++)
{
    if (argumentos[posicion] == "--reiniciar")
    {
        catalogo.Reiniciar();
        Console.WriteLine("Catálogo reiniciado.");
        continue;
    }

    var resultado = lector.ProcesarArchivo(argumentos[posicion]);
    Console.WriteLine(argumentos[posicion] + ": " + resultado.Mensaje);
    if (!resultado.Exito)
    {
        Environment.ExitCode = 1;
        return;
    }
    Console.WriteLine("Categorías añadidas: " + resultado.CategoriasAñadidas
        + ", categorías existentes: " + resultado.CategoriasExistentes
        + ", libros añadidos: " + resultado.LibrosAñadidos
        + ", libros existentes: " + resultado.LibrosExistentes);
}

static void ProbarCatalogo()
{
    var catalogo = new GestorCatalogo();
    var lector = new LectorXml(catalogo);
    var primeraCarga = ProcesarTexto(lector, """
        <config>
          <listaCategorias>
            <categoria padre="Ciencia">Física</categoria>
            <categoria>Ciencia</categoria>
          </listaCategorias>
          <listaLibros>
            <libro><ISBN>978003</ISBN><titulo>Óptica</titulo><autor>Ana López</autor><categoria>Física</categoria></libro>
            <libro><ISBN>978001</ISBN><titulo>Mecánica</titulo><autor>Luis Pérez</autor><categoria>Física</categoria></libro>
          </listaLibros>
        </config>
        """);
    Comprobar(primeraCarga.Exito && primeraCarga.CategoriasAñadidas == 2 && primeraCarga.LibrosAñadidos == 2, "La primera carga debe registrar categorías y libros.");
    Comprobar(catalogo.BuscarCategoria("Física")?.Padre?.Nombre == "Ciencia", "La subcategoría debe quedar vinculada a su padre.");
    Comprobar(catalogo.BuscarCategoria("Física")?.Libros.Primero?.Valor.Isbn == "978001", "Los libros deben conservar el orden por ISBN.");

    var segundaCarga = ProcesarTexto(lector, """
        <config>
          <listaCategorias><categoria>Ciencia</categoria><categoria>Literatura</categoria></listaCategorias>
          <listaLibros>
            <libro><ISBN>978003</ISBN><titulo>Óptica</titulo><autor>Ana López</autor><categoria>Física</categoria></libro>
            <libro><ISBN>978002</ISBN><titulo>Novela</titulo><autor>Marta Díaz</autor><categoria>Literatura</categoria></libro>
          </listaLibros>
        </config>
        """);
    Comprobar(segundaCarga.Exito && segundaCarga.CategoriasAñadidas == 1 && segundaCarga.CategoriasExistentes == 1, "La carga incremental debe evitar categorías duplicadas.");
    Comprobar(segundaCarga.LibrosAñadidos == 1 && segundaCarga.LibrosExistentes == 1, "La carga incremental debe evitar ISBN duplicados.");
    Comprobar(catalogo.BuscarLibro("978001") is not null && catalogo.BuscarLibro("978002") is not null, "La carga nueva debe conservar los libros anteriores.");

    var sinCategorias = ProcesarTexto(lector, """
        <config><listaLibros><libro><ISBN>978004</ISBN><titulo>Electromagnetismo</titulo><autor>Rosa Ruiz</autor><categoria>Física</categoria></libro></listaLibros></config>
        """);
    Comprobar(sinCategorias.Exito && catalogo.BuscarLibro("978004") is not null, "Debe aceptar archivos sin lista de categorías.");
    Comprobar(ProcesarTexto(lector, "<config />").Exito, "Debe aceptar archivos sin secciones.");

    var padreInexistente = ProcesarTexto(lector, "<config><listaCategorias><categoria padre=\"Ausente\">Otra</categoria></listaCategorias></config>");
    Comprobar(!padreInexistente.Exito, "Debe rechazar categorías con un padre inexistente.");
    Comprobar(!ProcesarTexto(lector, "<config><listaLibros><libro><ISBN>ABC</ISBN><titulo>T</titulo><autor>A</autor><categoria>Física</categoria></libro></listaLibros></config>").Exito, "Debe rechazar ISBN no numéricos.");

    catalogo.Reiniciar();
    Comprobar(catalogo.Categorias.Cantidad == 0 && catalogo.BuscarLibro("978001") is null, "El reinicio debe vaciar el catálogo.");
}

static ResultadoCarga ProcesarTexto(LectorXml lector, string contenido)
{
    using var archivo = new MemoryStream(Encoding.UTF8.GetBytes(contenido));
    return lector.Procesar(archivo);
}

static void Comprobar(bool condicion, string mensaje)
{
    if (!condicion) throw new InvalidOperationException(mensaje);
}
}
}
