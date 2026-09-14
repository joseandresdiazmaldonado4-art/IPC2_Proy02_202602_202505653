using BibliotecaWeb.Estructuras;
namespace BibliotecaWeb.Modelos;
public sealed class Categoria
{
    public string Nombre { get; set; } = "";
    public Categoria? Padre { get; set; }
    public ListaCategorias Hijos { get; } = new();
    public ListaLibros Libros { get; } = new();
}
