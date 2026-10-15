using EstatusCFDI.Sat.Internal;

namespace EstatusCFDI.Sat.Tests;

public class InterpreteAcuseTests
{
    private const string Satisfactorio = "S - Comprobante obtenido satisfactoriamente.";

    [Theory]
    [InlineData(Satisfactorio, "Vigente", EstadoConsulta.Vigente)]
    [InlineData(Satisfactorio, "Cancelado", EstadoConsulta.Cancelado)]
    [InlineData(Satisfactorio, "No Encontrado", EstadoConsulta.NoEncontrado)]
    [InlineData(Satisfactorio, " vigente ", EstadoConsulta.Vigente)]
    [InlineData(Satisfactorio, "Suspendido", EstadoConsulta.RespuestaNoReconocida)]
    [InlineData("N - 602: Comprobante no encontrado.", "No Encontrado", EstadoConsulta.NoEncontrado)]
    [InlineData("N - 601: La expresión impresa proporcionada no es válida.", "No Encontrado", EstadoConsulta.ExpresionInvalida)]
    [InlineData("N-601", "No Encontrado", EstadoConsulta.ExpresionInvalida)]
    [InlineData("N - 601: ...", "Vigente", EstadoConsulta.ExpresionInvalida)]
    [InlineData("N - 603: Otro.", "No Encontrado", EstadoConsulta.RespuestaNoReconocida)]
    [InlineData("N", "No Encontrado", EstadoConsulta.RespuestaNoReconocida)]
    [InlineData("", "Vigente", EstadoConsulta.RespuestaNoReconocida)]
    [InlineData("Satisfactorio", "Vigente", EstadoConsulta.RespuestaNoReconocida)]
    public void Interpretar_DecideConCodigoYEstado(string codigo, string estado, EstadoConsulta esperado)
    {
        var acuse = new AcuseSat(codigo, estado, "Cancelable con aceptación", "Plazo vencido", "200");

        Assert.Equal(esperado, InterpreteAcuse.Interpretar(acuse));
    }
}
