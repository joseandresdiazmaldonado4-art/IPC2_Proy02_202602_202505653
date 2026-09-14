namespace BibliotecaWeb.Modelos;
public sealed class Resultado
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = "";
    public byte[]? Contenido { get; set; }
    public static Resultado Correcto(string mensaje) => new() { Exito = true, Mensaje = mensaje };
    public static Resultado Error(string mensaje) => new() { Exito = false, Mensaje = mensaje };
}
