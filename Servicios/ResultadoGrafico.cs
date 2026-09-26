namespace BibliotecaWeb.Servicios;

public sealed class ResultadoGrafico
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = "";
    public string RutaImagen { get; set; } = "";
    public string RutaCodigo { get; set; } = "";
}
