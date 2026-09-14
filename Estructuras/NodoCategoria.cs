using BibliotecaWeb.Modelos;
namespace BibliotecaWeb.Estructuras;
public sealed class NodoCategoria
{
    public Categoria Valor { get; set; }
    public NodoCategoria? Siguiente { get; set; }
    public NodoCategoria? Anterior { get; set; }
    public NodoCategoria(Categoria valor) => Valor = valor;
}
