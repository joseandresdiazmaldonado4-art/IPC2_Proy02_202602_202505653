using BibliotecaWeb.Modelos;
namespace BibliotecaWeb.Estructuras;
public sealed class NodoLibro
{
    public Libro Valor { get; set; }
    public NodoLibro? Siguiente { get; set; }
    public NodoLibro? Anterior { get; set; }
    public NodoLibro(Libro valor) => Valor = valor;
}
