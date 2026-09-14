using BibliotecaWeb.Modelos;
namespace BibliotecaWeb.Estructuras;
public sealed class ListaLibros
{
    public NodoLibro? Primero { get; private set; }
    public NodoLibro? Ultimo { get; private set; }
    public int Cantidad { get; private set; }
    public void InsertarOrdenado(Libro libro)
    {
        var nuevo = new NodoLibro(libro);
        if (Primero is null) Primero = Ultimo = nuevo;
        else if (string.CompareOrdinal(libro.Isbn, Primero.Valor.Isbn) < 0) { nuevo.Siguiente = Primero; Primero.Anterior = nuevo; Primero = nuevo; }
        else { var actual = Primero; while (actual.Siguiente is not null && string.CompareOrdinal(actual.Siguiente.Valor.Isbn, libro.Isbn) < 0) actual = actual.Siguiente; nuevo.Siguiente = actual.Siguiente; nuevo.Anterior = actual; if (actual.Siguiente is null) Ultimo = nuevo; else actual.Siguiente.Anterior = nuevo; actual.Siguiente = nuevo; }
        Cantidad++;
    }
    public Libro? Buscar(string isbn) { var actual = Primero; while (actual is not null) { if (actual.Valor.Isbn == isbn) return actual.Valor; actual = actual.Siguiente; } return null; }
    public bool Eliminar(string isbn)
    {
        var actual = Primero; while (actual is not null && actual.Valor.Isbn != isbn) actual = actual.Siguiente; if (actual is null) return false;
        if (actual.Anterior is null) Primero = actual.Siguiente; else actual.Anterior.Siguiente = actual.Siguiente;
        if (actual.Siguiente is null) Ultimo = actual.Anterior; else actual.Siguiente.Anterior = actual.Anterior; Cantidad--; return true;
    }
    public void Vaciar() { Primero = null; Ultimo = null; Cantidad = 0; }
}
