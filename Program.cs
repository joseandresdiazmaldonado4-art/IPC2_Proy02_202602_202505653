using BibliotecaWeb.Estructuras;
using BibliotecaWeb.Modelos;

var libros = new ListaLibros();
libros.InsertarOrdenado(new Libro { Isbn = "978003", Titulo = "Estructuras", Autor = "Ana López", Categoria = "Programación" });
libros.InsertarOrdenado(new Libro { Isbn = "978001", Titulo = "Algoritmos", Autor = "Luis Pérez", Categoria = "Programación" });
libros.InsertarOrdenado(new Libro { Isbn = "978002", Titulo = "Fundamentos", Autor = "Marta Díaz", Categoria = "Programación" });

Comprobar(libros.Cantidad == 3, "La lista debe contener tres libros.");
Comprobar(libros.Primero?.Valor.Isbn == "978001", "Los libros deben ordenarse de forma ascendente.");
Comprobar(libros.Ultimo?.Valor.Isbn == "978003", "El último ISBN no es correcto.");
Comprobar(libros.Buscar("978002")?.Titulo == "Fundamentos", "La búsqueda por ISBN debe encontrar el libro.");
Comprobar(libros.Eliminar("978002"), "La eliminación debe encontrar el ISBN.");
Comprobar(libros.Buscar("978002") is null && libros.Cantidad == 2, "El libro debe desaparecer de la lista.");

var raiz = new Categoria { Nombre = "Ciencia" };
var hija = new Categoria { Nombre = "Física", Padre = raiz };
raiz.Hijos.Agregar(hija);
Comprobar(raiz.Hijos.BuscarDirecta("Física") == hija, "La categoría hija debe quedar enlazada.");
Comprobar(hija.Padre == raiz, "La categoría debe conservar la referencia a su padre.");

Console.WriteLine("Todas las pruebas se ejecutaron correctamente.");

static void Comprobar(bool condicion, string mensaje)
{
    if (!condicion) throw new InvalidOperationException(mensaje);
}
