using System.Diagnostics;
using System.Text;
using BibliotecaWeb.Estructuras;
using BibliotecaWeb.Modelos;

namespace BibliotecaWeb.Servicios;

public sealed class GeneradorGraficos
{
    private readonly GestorCatalogo catalogo;
    private readonly IWebHostEnvironment entorno;

    public GeneradorGraficos(GestorCatalogo catalogo, IWebHostEnvironment entorno)
    {
        this.catalogo = catalogo;
        this.entorno = entorno;
    }

    public ResultadoGrafico GenerarEstructuraCompleta()
    {
        var codigo = CrearEncabezado("Estructura completa del catálogo");
        var numero = 0;
        EscribirCategorias(catalogo.Categorias, codigo, null, ref numero);
        codigo.Append('}');
        return Exportar(codigo, "estructura-completa");
    }

    public ResultadoGrafico GenerarDesdeCategoria(string nombre)
    {
        var categoria = catalogo.BuscarCategoria(nombre.Trim());
        if (categoria is null) return Error("No existe la categoría indicada.");
        var codigo = CrearEncabezado("Estructura desde " + categoria.Nombre);
        var numero = 0;
        EscribirCategoria(categoria, codigo, null, ref numero);
        codigo.Append('}');
        return Exportar(codigo, "estructura-categoria");
    }

    public ResultadoGrafico GenerarLibros(string nombre)
    {
        var categoria = catalogo.BuscarCategoria(nombre.Trim());
        if (categoria is null) return Error("No existe la categoría indicada.");
        var codigo = CrearEncabezado("Libros de " + categoria.Nombre);
        codigo.Append("categoria [label=\"").Append(Escapar(categoria.Nombre)).Append("\", fillcolor=\"#d7a748\"];");
        var anterior = "categoria";
        var numero = 0;
        var libro = categoria.Libros.Primero;
        while (libro is not null)
        {
            var identificador = "libro" + numero++;
            codigo.Append(identificador).Append(" [label=\"")
                .Append(Escapar(libro.Valor.Isbn)).Append("\\n")
                .Append(Escapar(libro.Valor.Titulo)).Append("\\n")
                .Append(Escapar(libro.Valor.Autor)).Append("\"];")
                .Append(anterior).Append(" -> ").Append(identificador).Append(';');
            anterior = identificador;
            libro = libro.Siguiente;
        }
        if (numero == 0) codigo.Append("vacio [label=\"Sin libros registrados\", style=\"rounded,dashed\"]; categoria -> vacio;");
        codigo.Append('}');
        return Exportar(codigo, "libros-categoria");
    }

    private ResultadoGrafico Exportar(StringBuilder codigo, string nombre)
    {
        try
        {
            var carpeta = Path.Combine(entorno.WebRootPath, "reportes");
            Directory.CreateDirectory(carpeta);
            var identificador = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var nombreBase = nombre + "-" + identificador;
            var rutaCodigo = Path.Combine(carpeta, nombreBase + ".dot");
            var rutaImagen = Path.Combine(carpeta, nombreBase + ".png");
            File.WriteAllText(rutaCodigo, codigo.ToString(), new UTF8Encoding(false));

            using var proceso = Process.Start(new ProcessStartInfo
            {
                FileName = "dot",
                Arguments = "-Tpng \"" + rutaCodigo + "\" -o \"" + rutaImagen + "\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            });
            if (proceso is null) return Error("No se pudo iniciar el generador de imágenes.");
            proceso.WaitForExit();
            if (proceso.ExitCode != 0 || !File.Exists(rutaImagen))
                return Error("No se pudo crear la imagen: " + proceso.StandardError.ReadToEnd());

            return new ResultadoGrafico
            {
                Exito = true,
                Mensaje = "Reporte generado.",
                RutaImagen = "/reportes/" + nombreBase + ".png",
                RutaCodigo = "/reportes/" + nombreBase + ".dot"
            };
        }
        catch (Exception error)
        {
            return Error("No se pudo exportar el reporte: " + error.Message);
        }
    }

    private static StringBuilder CrearEncabezado(string titulo)
    {
        return new StringBuilder("digraph Catalogo { rankdir=TB; graph [bgcolor=\"#f7f4ed\", pad=\"0.4\", label=\"")
            .Append(Escapar(titulo)).Append("\", labelloc=\"t\", fontsize=\"20\"]; node [shape=box, style=\"rounded,filled\", fillcolor=\"#ffffff\", color=\"#245f56\", fontname=\"Arial\"]; edge [color=\"#78918d\"]; ");
    }

    private static void EscribirCategorias(ListaCategorias categorias, StringBuilder codigo, string? padre, ref int numero)
    {
        var actual = categorias.Primero;
        while (actual is not null)
        {
            EscribirCategoria(actual.Valor, codigo, padre, ref numero);
            actual = actual.Siguiente;
        }
    }

    private static void EscribirCategoria(Categoria categoria, StringBuilder codigo, string? padre, ref int numero)
    {
        var identificador = "categoria" + numero++;
        codigo.Append(identificador).Append(" [label=\"").Append(Escapar(categoria.Nombre)).Append("\"];");
        if (padre is not null) codigo.Append(padre).Append(" -> ").Append(identificador).Append(';');
        EscribirCategorias(categoria.Hijos, codigo, identificador, ref numero);
    }

    private static string Escapar(string texto) => texto.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");

    private static ResultadoGrafico Error(string mensaje) => new() { Mensaje = mensaje };
}
