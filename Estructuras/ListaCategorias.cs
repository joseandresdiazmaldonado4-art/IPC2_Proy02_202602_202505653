using BibliotecaWeb.Modelos;
namespace BibliotecaWeb.Estructuras;
public sealed class ListaCategorias
{
    public NodoCategoria? Primero { get; private set; }
    public NodoCategoria? Ultimo { get; private set; }
    public int Cantidad { get; private set; }
    public void Agregar(Categoria categoria) { var nuevo = new NodoCategoria(categoria); if (Primero is null) Primero = Ultimo = nuevo; else { nuevo.Anterior = Ultimo; Ultimo!.Siguiente = nuevo; Ultimo = nuevo; } Cantidad++; }
    public Categoria? BuscarDirecta(string nombre) { var actual = Primero; while (actual is not null) { if (actual.Valor.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)) return actual.Valor; actual = actual.Siguiente; } return null; }
    public void Vaciar() { Primero = null; Ultimo = null; Cantidad = 0; }
}
