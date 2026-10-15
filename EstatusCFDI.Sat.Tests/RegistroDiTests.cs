using EstatusCFDI.Sat.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EstatusCFDI.Sat.Tests;

public class RegistroDiTests
{
    [Fact]
    public async Task AddEstatusCfdiSat_ResuelveElClienteYConsulta()
    {
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        servicios.AddEstatusCfdiSat().ConfigurePrimaryHttpMessageHandler(() => ManejadorHttpFalso.RespondeSat("cancelado.xml"));
        using var proveedor = servicios.BuildServiceProvider();

        var consulta = proveedor.GetRequiredService<IConsultaEstatusCfdi>();
        var resultado = await consulta.ConsultarAsync(DatosPrueba.Datos());

        Assert.IsType<ConsultaEstatusCfdiSat>(consulta);
        Assert.Equal(EstadoConsulta.Cancelado, resultado.Estado);
    }

    [Fact]
    public void AddEstatusCfdiSat_AplicaLaConfiguracion()
    {
        var servicios = new ServiceCollection();
        servicios.AddEstatusCfdiSat(o => o.Timeout = TimeSpan.FromSeconds(10));
        using var proveedor = servicios.BuildServiceProvider();

        Assert.Equal(TimeSpan.FromSeconds(10), proveedor.GetRequiredService<IOptions<EstatusCfdiSatOptions>>().Value.Timeout);
    }

    [Fact]
    public void AddEstatusCfdiSat_ConfiguracionInvalida_FallaAlResolver()
    {
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        servicios.AddEstatusCfdiSat(o => o.Timeout = TimeSpan.Zero);
        using var proveedor = servicios.BuildServiceProvider();

        var ex = Assert.Throws<OptionsValidationException>(() => proveedor.GetRequiredService<IConsultaEstatusCfdi>());
        Assert.Contains("Timeout", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddEstatusCfdiSat_DosVeces_RegistraUnSoloValidador()
    {
        var servicios = new ServiceCollection();
        servicios.AddEstatusCfdiSat();
        servicios.AddEstatusCfdiSat();
        using var proveedor = servicios.BuildServiceProvider();

        Assert.Single(proveedor.GetServices<IValidateOptions<EstatusCfdiSatOptions>>());
    }

    [Fact]
    public void AddEstatusCfdiSat_ServiciosNulos_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddEstatusCfdiSat());
    }
}
