# Guía de migración v1.x → v2.0.0

v2.0.0 es una reconstrucción: se eliminó el cliente WCF y la API pública es nueva. Tu proyecto en 1.x sigue funcionando hasta que decidas actualizar; 1.x se queda publicada en NuGet.

## Por qué cambió

- El cliente WCF no tenía tiempo de espera (1 minuto por consulta) ni `CancellationToken`.
- Un SAT caído llegaba como excepción cruda de WCF, y la respuesta como textos que cada consumidor tenía que interpretar.
- `using` sobre un canal WCF en falla lanzaba una segunda excepción que tapaba la original.
- Apuntaba a .NET 6, sin soporte desde noviembre de 2024.

## Equivalencias

| 1.x | 2.0.0 |
|---|---|
| `EstatusFacade.ConsultarEstatusAsync(re, rr, total, uuid)` | `consulta.ConsultarAsync(new DatosConsultaCfdi(re, rr, total, Guid.Parse(uuid)))` |
| `IConsultaEstatusService.ConsultarEstatusCfdiAsync(...)` | `IConsultaEstatusCfdi.ConsultarAsync(DatosConsultaCfdi, CancellationToken)` |
| `new ConsultaEstatusService(new ConsultaCFDIServiceClient())` | `services.AddEstatusCfdiSat()` o `new ConsultaEstatusCfdiSat(httpClient, Options.Create(new EstatusCfdiSatOptions()), logger)` |
| `ConsultaCFDI.Acuse` (comparar `Estado == "Vigente"`) | `ResultadoConsultaSat.Estado == EstadoConsulta.Vigente`; los campos crudos siguen en `ResultadoConsultaSat.Acuse` |
| Excepciones de WCF (`TimeoutException`, `EndpointNotFoundException`, …) | `EstadoConsulta.SinRespuesta`, con el motivo en `Detalle` |
| Sacar los datos del XML a mano | `DatosConsultaCfdi.DesdeXml(xml)` |
| `net6.0` | `net10.0` y `net8.0` |

## Antes y después

```csharp
// 1.x
var acuse = await EstatusFacade.ConsultarEstatusAsync("EKU9003173C9", "URE180429TM6", 1595.00m, "766049BD-0000-4000-8000-000000000001");
if (acuse.Estado == "Cancelado") { /* … */ }
```

```csharp
// 2.0.0 (registro: builder.Services.AddEstatusCfdiSat();)
var resultado = await consulta.ConsultarAsync(DatosConsultaCfdi.DesdeXml(xmlTimbrado), cancellationToken);
switch (resultado.Estado)
{
    case EstadoConsulta.Vigente: /* continuar */ break;
    case EstadoConsulta.Cancelado: /* bloquear */ break;
    default: /* el SAT no lo confirmó: reintentar más tarde */ break;
}
```

## Cambios de comportamiento a tener en cuenta

- Un SAT que no responde ya no lanza excepción: revisa `Estado` en lugar de envolver la llamada en `try/catch`.
- El total se envía en forma canónica (`1595.00`, no `1,595.00`); el SAT compara por valor, así que el resultado no cambia.
- Un RFC con `&` ahora funciona (en 1.x el SAT respondía 601).
- Los RFC se validan al construir `DatosConsultaCfdi`: un RFC mal formado ahora es `ArgumentException` antes de llamar al SAT.
