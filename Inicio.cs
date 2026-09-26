using BibliotecaWeb.Servicios;

namespace BibliotecaWeb;

public static class Inicio
{
    public static void Main(string[] argumentos)
    {
        var configuracion = WebApplication.CreateBuilder(argumentos);
        configuracion.Services.AddSingleton<GestorCatalogo>();
        configuracion.Services.AddSingleton<LectorXml>();
        configuracion.Services.AddSingleton<GeneradorGraficos>();

        var aplicacion = configuracion.Build();
        aplicacion.UseDefaultFiles();
        aplicacion.UseStaticFiles();

        aplicacion.MapGet("/api/catalogo", (GestorCatalogo catalogo) => Results.Text(catalogo.ObtenerCatalogoJson(), "application/json"));
        aplicacion.MapGet("/api/libros/{isbn}", (string isbn, GestorCatalogo catalogo) =>
        {
            var libro = catalogo.BuscarLibro(isbn);
            return libro is null ? Results.NotFound(new { mensaje = "No se encontró el libro." }) : Results.Ok(libro);
        });
        aplicacion.MapGet("/api/libros/{isbn}/vecinos", (string isbn, GestorCatalogo catalogo) =>
            Results.Text(catalogo.ObtenerIsbnVecinosJson(isbn), "application/json"));

        aplicacion.MapPost("/api/categorias", async (HttpRequest solicitud, GestorCatalogo catalogo) =>
        {
            var formulario = await solicitud.ReadFormAsync();
            var resultado = catalogo.AñadirCategoria(formulario["nombre"].ToString(), formulario["padre"].ToString());
            return resultado.Exito ? Results.Ok(resultado) : Results.BadRequest(resultado);
        }).DisableAntiforgery();

        aplicacion.MapPost("/api/libros", async (HttpRequest solicitud, GestorCatalogo catalogo) =>
        {
            var formulario = await solicitud.ReadFormAsync();
            var resultado = catalogo.AñadirLibro(formulario["isbn"].ToString(), formulario["titulo"].ToString(), formulario["autor"].ToString(), formulario["categoria"].ToString());
            return resultado.Exito ? Results.Ok(resultado) : Results.BadRequest(resultado);
        }).DisableAntiforgery();

        aplicacion.MapDelete("/api/libros/{isbn}", (string isbn, GestorCatalogo catalogo) =>
        {
            var resultado = catalogo.EliminarLibro(isbn);
            return resultado.Exito ? Results.Ok(resultado) : Results.NotFound(resultado);
        });

        aplicacion.MapPost("/api/cargar", async (HttpRequest solicitud, GestorCatalogo catalogo, LectorXml lector) =>
        {
            var formulario = await solicitud.ReadFormAsync();
            var archivo = formulario.Files.GetFile("archivo");
            if (archivo is null) return Results.BadRequest(new { mensaje = "Seleccione un archivo XML." });
            if (formulario["reiniciar"] == "si") catalogo.Reiniciar();
            await using var contenido = archivo.OpenReadStream();
            var resultado = lector.Procesar(contenido);
            return resultado.Exito ? Results.Ok(resultado) : Results.BadRequest(resultado);
        }).DisableAntiforgery();

        aplicacion.MapPost("/api/reiniciar", (GestorCatalogo catalogo) =>
        {
            catalogo.Reiniciar();
            return Results.Ok(new { mensaje = "Catálogo reiniciado." });
        });

        aplicacion.MapGet("/api/reportes/{tipo}", (string tipo, string? categoria, GeneradorGraficos generador) =>
        {
            var resultado = tipo switch
            {
                "completo" => generador.GenerarEstructuraCompleta(),
                "categoria" => generador.GenerarDesdeCategoria(categoria ?? ""),
                "libros" => generador.GenerarLibros(categoria ?? ""),
                _ => new ResultadoGrafico { Mensaje = "Tipo de reporte desconocido." }
            };
            return resultado.Exito ? Results.Ok(resultado) : Results.BadRequest(resultado);
        });

        aplicacion.MapGet("/documentacion", () => Results.File(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "README.md"), "text/markdown"));
        aplicacion.Run();
    }
}
