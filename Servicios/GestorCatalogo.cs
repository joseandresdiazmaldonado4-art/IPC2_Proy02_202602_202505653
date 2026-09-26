using BibliotecaWeb.Estructuras;
using BibliotecaWeb.Modelos;
using System.Text;
using System.Text.Json;

namespace BibliotecaWeb.Servicios;

public sealed class GestorCatalogo
{
    public ListaCategorias Categorias { get; } = new();

    public Categoria? BuscarCategoria(string nombre) => BuscarCategoria(nombre, Categorias);

    public Libro? BuscarLibro(string isbn) => BuscarLibro(isbn, Categorias);

    public Resultado AñadirCategoria(string nombre, string? nombrePadre)
    {
        nombre = nombre.Trim();
        nombrePadre = nombrePadre?.Trim();

        if (nombre.Length == 0)
            return Resultado.Error("La categoría necesita un nombre.");
        if (BuscarCategoria(nombre) is not null)
            return Resultado.Error("La categoría ya existe.");

        Categoria? padre = null;
        if (!string.IsNullOrEmpty(nombrePadre))
        {
            padre = BuscarCategoria(nombrePadre);
            if (padre is null)
                return Resultado.Error("No existe la categoría padre: " + nombrePadre);
        }

        var categoria = new Categoria { Nombre = nombre, Padre = padre };
        if (padre is null)
            Categorias.Agregar(categoria);
        else
            padre.Hijos.Agregar(categoria);

        return Resultado.Correcto("Categoría registrada.");
    }

    public Resultado AñadirLibro(string isbn, string titulo, string autor, string nombreCategoria)
    {
        isbn = isbn.Trim();
        titulo = titulo.Trim();
        autor = autor.Trim();
        nombreCategoria = nombreCategoria.Trim();

        if (isbn.Length == 0 || titulo.Length == 0 || autor.Length == 0 || nombreCategoria.Length == 0)
            return Resultado.Error("El libro necesita ISBN, título, autor y categoría.");
        for (var posicion = 0; posicion < isbn.Length; posicion++)
            if (isbn[posicion] < '0' || isbn[posicion] > '9')
                return Resultado.Error("El ISBN debe contener solamente números.");
        if (BuscarLibro(isbn) is not null)
            return Resultado.Error("El ISBN ya existe.");

        var categoria = BuscarCategoria(nombreCategoria);
        if (categoria is null)
            return Resultado.Error("No existe la categoría del libro: " + nombreCategoria);

        categoria.Libros.InsertarOrdenado(new Libro
        {
            Isbn = isbn,
            Titulo = titulo,
            Autor = autor,
            Categoria = categoria.Nombre
        });
        return Resultado.Correcto("Libro registrado.");
    }

    public void Reiniciar() => Categorias.Vaciar();

    public Resultado EliminarLibro(string isbn)
    {
        return EliminarLibro(isbn.Trim(), Categorias)
            ? Resultado.Correcto("Libro eliminado.")
            : Resultado.Error("No se encontró el libro.");
    }

    public string ObtenerCatalogoJson()
    {
        var texto = new StringBuilder("{\"categorias\":[");
        EscribirCategoriasJson(Categorias, texto);
        texto.Append("]}");
        return texto.ToString();
    }

    public string ObtenerIsbnVecinosJson(string isbn)
    {
        Libro? menor = null;
        Libro? mayor = null;
        BuscarIsbnVecinos(isbn.Trim(), Categorias, ref menor, ref mayor);
        return "{\"menor\":" + (menor is null ? "null" : LibroJson(menor))
            + ",\"mayor\":" + (mayor is null ? "null" : LibroJson(mayor)) + "}";
    }

    private static Categoria? BuscarCategoria(string nombre, ListaCategorias categorias)
    {
        var actual = categorias.Primero;
        while (actual is not null)
        {
            if (actual.Valor.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase))
                return actual.Valor;
            var encontrada = BuscarCategoria(nombre, actual.Valor.Hijos);
            if (encontrada is not null)
                return encontrada;
            actual = actual.Siguiente;
        }
        return null;
    }

    private static Libro? BuscarLibro(string isbn, ListaCategorias categorias)
    {
        var actual = categorias.Primero;
        while (actual is not null)
        {
            var encontrado = actual.Valor.Libros.Buscar(isbn);
            if (encontrado is not null)
                return encontrado;
            encontrado = BuscarLibro(isbn, actual.Valor.Hijos);
            if (encontrado is not null)
                return encontrado;
            actual = actual.Siguiente;
        }
        return null;
    }

    private static bool EliminarLibro(string isbn, ListaCategorias categorias)
    {
        var actual = categorias.Primero;
        while (actual is not null)
        {
            if (actual.Valor.Libros.Eliminar(isbn)) return true;
            if (EliminarLibro(isbn, actual.Valor.Hijos)) return true;
            actual = actual.Siguiente;
        }
        return false;
    }

    private static void BuscarIsbnVecinos(string isbn, ListaCategorias categorias, ref Libro? menor, ref Libro? mayor)
    {
        var categoria = categorias.Primero;
        while (categoria is not null)
        {
            var libro = categoria.Valor.Libros.Primero;
            while (libro is not null)
            {
                var comparacion = string.CompareOrdinal(libro.Valor.Isbn, isbn);
                if (comparacion < 0 && (menor is null || string.CompareOrdinal(libro.Valor.Isbn, menor.Isbn) > 0)) menor = libro.Valor;
                if (comparacion > 0 && (mayor is null || string.CompareOrdinal(libro.Valor.Isbn, mayor.Isbn) < 0)) mayor = libro.Valor;
                libro = libro.Siguiente;
            }
            BuscarIsbnVecinos(isbn, categoria.Valor.Hijos, ref menor, ref mayor);
            categoria = categoria.Siguiente;
        }
    }

    private static void EscribirCategoriasJson(ListaCategorias categorias, StringBuilder texto)
    {
        var categoria = categorias.Primero;
        var primera = true;
        while (categoria is not null)
        {
            if (!primera) texto.Append(',');
            primera = false;
            texto.Append("{\"nombre\":").Append(JsonSerializer.Serialize(categoria.Valor.Nombre)).Append(",\"libros\":[");
            var libro = categoria.Valor.Libros.Primero;
            var primerLibro = true;
            while (libro is not null)
            {
                if (!primerLibro) texto.Append(',');
                primerLibro = false;
                texto.Append(LibroJson(libro.Valor));
                libro = libro.Siguiente;
            }
            texto.Append("],\"hijos\":[");
            EscribirCategoriasJson(categoria.Valor.Hijos, texto);
            texto.Append("]}");
            categoria = categoria.Siguiente;
        }
    }

    private static string LibroJson(Libro libro)
    {
        return "{\"isbn\":" + JsonSerializer.Serialize(libro.Isbn)
            + ",\"titulo\":" + JsonSerializer.Serialize(libro.Titulo)
            + ",\"autor\":" + JsonSerializer.Serialize(libro.Autor)
            + ",\"categoria\":" + JsonSerializer.Serialize(libro.Categoria) + "}";
    }
}
