using BibliotecaWeb.Estructuras;
using BibliotecaWeb.Modelos;

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
}
