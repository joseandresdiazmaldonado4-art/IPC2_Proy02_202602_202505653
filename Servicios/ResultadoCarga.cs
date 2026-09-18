namespace BibliotecaWeb.Servicios;

public sealed class ResultadoCarga
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = "";
    public int CategoriasAñadidas { get; set; }
    public int CategoriasExistentes { get; set; }
    public int LibrosAñadidos { get; set; }
    public int LibrosExistentes { get; set; }
}
