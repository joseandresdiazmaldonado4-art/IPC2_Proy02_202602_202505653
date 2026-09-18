using System.Xml;

namespace BibliotecaWeb.Servicios;

public sealed class LectorXml
{
    private readonly GestorCatalogo catalogo;

    public LectorXml(GestorCatalogo catalogo) => this.catalogo = catalogo;

    public ResultadoCarga ProcesarArchivo(string ruta)
    {
        try
        {
            using var archivo = File.OpenRead(ruta);
            return Procesar(archivo);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return new ResultadoCarga { Mensaje = "No se pudo abrir el archivo: " + error.Message };
        }
    }

    public ResultadoCarga Procesar(Stream archivo)
    {
        var resultado = new ResultadoCarga();
        try
        {
            var opciones = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 10_000_000
            };
            using var lector = XmlReader.Create(archivo, opciones);
            var documento = new XmlDocument { XmlResolver = null };
            documento.Load(lector);
            var raiz = documento.DocumentElement;
            if (raiz is null || raiz.Name != "config")
            {
                resultado.Mensaje = "El elemento principal debe ser <config>.";
                return resultado;
            }

            var seccionCategorias = BuscarSeccion(raiz, "listaCategorias");
            if (seccionCategorias is not null)
            {
                var pendiente = ContarElementos(seccionCategorias, "categoria");
                while (pendiente > 0)
                {
                    var incorporadas = 0;
                    for (var elemento = seccionCategorias.FirstChild; elemento is not null; elemento = elemento.NextSibling)
                    {
                        if (elemento.NodeType != XmlNodeType.Element || elemento.Name != "categoria") continue;
                        var nombre = elemento.InnerText.Trim();
                        var padre = elemento.Attributes?["padre"]?.Value.Trim();
                        if (nombre.Length == 0)
                        {
                            resultado.Mensaje = "Una categoría no tiene nombre.";
                            return resultado;
                        }
                        if (catalogo.BuscarCategoria(nombre) is not null) continue;
                        if (!string.IsNullOrEmpty(padre) && catalogo.BuscarCategoria(padre) is null) continue;
                        var registro = catalogo.AñadirCategoria(nombre, padre);
                        if (!registro.Exito)
                        {
                            resultado.Mensaje = registro.Mensaje;
                            return resultado;
                        }
                        resultado.CategoriasAñadidas++;
                        incorporadas++;
                    }
                    var restantes = 0;
                    for (var elemento = seccionCategorias.FirstChild; elemento is not null; elemento = elemento.NextSibling)
                        if (elemento.NodeType == XmlNodeType.Element && elemento.Name == "categoria" && catalogo.BuscarCategoria(elemento.InnerText.Trim()) is null)
                            restantes++;
                    if (restantes == 0) break;
                    if (incorporadas == 0)
                    {
                        resultado.Mensaje = "Hay categorías con padres inexistentes o una relación circular.";
                        return resultado;
                    }
                    pendiente = restantes;
                }
                resultado.CategoriasExistentes = ContarElementos(seccionCategorias, "categoria") - resultado.CategoriasAñadidas;
            }

            var seccionLibros = BuscarSeccion(raiz, "listaLibros");
            if (seccionLibros is not null)
            {
                for (var elemento = seccionLibros.FirstChild; elemento is not null; elemento = elemento.NextSibling)
                {
                    if (elemento.NodeType != XmlNodeType.Element || elemento.Name != "libro") continue;
                    var isbn = LeerValor(elemento, "ISBN");
                    if (catalogo.BuscarLibro(isbn) is not null)
                    {
                        resultado.LibrosExistentes++;
                        continue;
                    }
                    var registro = catalogo.AñadirLibro(isbn, LeerValor(elemento, "titulo"), LeerValor(elemento, "autor"), LeerValor(elemento, "categoria"));
                    if (!registro.Exito)
                    {
                        resultado.Mensaje = "Libro " + isbn + ": " + registro.Mensaje;
                        return resultado;
                    }
                    resultado.LibrosAñadidos++;
                }
            }

            resultado.Exito = true;
            resultado.Mensaje = "Archivo procesado correctamente.";
        }
        catch (XmlException error)
        {
            resultado.Mensaje = "XML inválido: " + error.Message;
        }
        return resultado;
    }

    private static XmlNode? BuscarSeccion(XmlNode raiz, string nombre)
    {
        for (var actual = raiz.FirstChild; actual is not null; actual = actual.NextSibling)
            if (actual.NodeType == XmlNodeType.Element && actual.Name == nombre)
                return actual;
        return null;
    }

    private static int ContarElementos(XmlNode padre, string nombre)
    {
        var cantidad = 0;
        for (var actual = padre.FirstChild; actual is not null; actual = actual.NextSibling)
            if (actual.NodeType == XmlNodeType.Element && actual.Name == nombre)
                cantidad++;
        return cantidad;
    }

    private static string LeerValor(XmlNode padre, string nombre)
    {
        for (var actual = padre.FirstChild; actual is not null; actual = actual.NextSibling)
            if (actual.NodeType == XmlNodeType.Element && actual.Name.Equals(nombre, StringComparison.OrdinalIgnoreCase))
                return actual.InnerText.Trim();
        return "";
    }
}
