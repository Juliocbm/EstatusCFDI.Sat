# 📦 EstatusCFDI.Sat

[![NuGet](https://img.shields.io/nuget/v/EstatusCFDI.Sat.svg?style=flat-square)](https://www.nuget.org/packages/EstatusCFDI.Sat/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EstatusCFDI.Sat.svg?style=flat-square)](https://www.nuget.org/packages/EstatusCFDI.Sat/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)](https://opensource.org/licenses/MIT)

## Descripción general

Librería .NET para saber si un **CFDI está vigente o cancelado** ante el SAT. Consulta el servicio oficial `ConsultaCFDIService` (el mismo que usa el código QR de los PDF; sin captcha ni autenticación) y devuelve un resultado tipado que distingue lo que el SAT confirma de lo que no pudo confirmar.

- Resultado tipado: `Vigente`, `Cancelado`, `NoEncontrado`, `ExpresionInvalida`, `SinRespuesta`, `RespuestaNoReconocida`.
- Nunca lanza excepción por lo que el SAT responda o deje de responder: un SAT caído o lento es `SinRespuesta`, no una excepción.
- Tiempo de espera configurable (25 s por defecto) y `CancellationToken`.
- Lee los datos directo del XML timbrado (CFDI 3.3 y 4.0): `DatosConsultaCfdi.DesdeXml(...)`.
- Integración con `IHttpClientFactory` (`services.AddEstatusCfdiSat()`), también usable sin inyección de dependencias.
- .NET 10 y .NET 8.

## 📥 Instalación

```bash
dotnet add package EstatusCFDI.Sat
```

## 📚 Requisitos

- .NET 8 o .NET 10.
- Salida a internet hacia `https://consultaqr.facturaelectronica.sat.gob.mx`.

## 💉 Uso con inyección de dependencias

```csharp
builder.Services.AddEstatusCfdiSat(o => o.Timeout = TimeSpan.FromSeconds(10));
```

```csharp
public sealed class ValidadorTraslado(IConsultaEstatusCfdi consultaSat)
{
    public async Task<bool> EstaVigenteAsync(byte[] xmlTimbrado, CancellationToken ct)
    {
        var resultado = await consultaSat.ConsultarAsync(DatosConsultaCfdi.DesdeXml(xmlTimbrado), ct);
        return resultado.Estado == EstadoConsulta.Vigente;
    }
}
```

`AddEstatusCfdiSat` devuelve el `IHttpClientBuilder` del cliente, así que puedes agregar políticas propias (por ejemplo, un circuit breaker con `Microsoft.Extensions.Http.Resilience`). **La librería no reintenta**: decide tú cuándo volver a consultar.

## 🔵 Uso sin inyección de dependencias

```csharp
using var http = new HttpClient();   // reutilízalo entre consultas
var consulta = new ConsultaEstatusCfdiSat(
    http,
    Options.Create(new EstatusCfdiSatOptions()),
    NullLogger<ConsultaEstatusCfdiSat>.Instance);

var resultado = await consulta.ConsultarAsync(DatosConsultaCfdi.DesdeXml(File.ReadAllBytes("cfdi.xml")));
```

## 📄 Datos de la consulta

Desde el XML timbrado (bytes o `Stream`), que es lo recomendado:

```csharp
var datos = DatosConsultaCfdi.DesdeXml(xmlBytes);
```

O con los datos sueltos (aquí, ficticios):

```csharp
var datos = new DatosConsultaCfdi(
    rfcEmisor: "EKU9003173C9",
    rfcReceptor: "URE180429TM6",
    total: 1595.00m,
    uuid: Guid.Parse("766049bd-0000-4000-8000-000000000001"),
    fragmentoSello: "Q2QxMjM0");   // opcional: últimos 8 caracteres del atributo Sello
```

Los datos se validan al construirse (RFC con el patrón oficial del SAT, total no negativo, UUID no vacío). `DatosConsultaCfdi.ToString()` solo muestra el UUID.

## 🧭 Qué hacer con cada resultado

| `EstadoConsulta` | Significa | Acción sugerida |
|---|---|---|
| `Vigente` | El SAT confirma que el CFDI está vigente. | Continuar. |
| `Cancelado` | El SAT confirma que está cancelado. | Bloquear. |
| `NoEncontrado` | El SAT no tiene ese UUID (602). Justo después de timbrar puede tardar unos minutos. | Reintentar más tarde. |
| `ExpresionInvalida` | El SAT rechazó la expresión (601): falta un dato o alguno no coincide, por ejemplo el total. | Revisar los datos; si vienen del XML, reintentar más tarde. |
| `SinRespuesta` | Tiempo de espera agotado, error de red, HTTP ≠ 200 o SOAP Fault. Ver `Detalle`. | Reintentar más tarde. |
| `RespuestaNoReconocida` | El SAT respondió algo que la librería no sabe interpretar. Ver `Detalle` y `Acuse`. | Reintentar más tarde y revisar. |

`resultado.EsConcluyente` es `true` solo para `Vigente` y `Cancelado`. `resultado.Acuse` trae los campos crudos del SAT (`CodigoEstatus`, `Estado`, `EsCancelable`, `EstatusCancelacion`, `ValidacionEfos`) para diagnóstico.

## ⚙️ Opciones

| Opción | Por defecto | Descripción |
|---|---|---|
| `Endpoint` | `EstatusCfdiSatOptions.EndpointSat` | URL del servicio. Solo `https`. |
| `Timeout` | 25 s | Tiempo máximo por consulta; al vencer, el resultado es `SinRespuesta`. |

Desde `appsettings.json`:

```json
"EstatusCfdiSat": { "Timeout": "00:00:10" }
```

```csharp
builder.Services.Configure<EstatusCfdiSatOptions>(builder.Configuration.GetSection("EstatusCfdiSat"));
builder.Services.AddEstatusCfdiSat();
```

Las opciones se validan al arrancar (`ValidateOnStart`) y al construir el cliente.

## 🛑 Manejo de errores

| Situación | Comportamiento |
|---|---|
| El SAT responde (lo que sea) o no responde | Nunca hay excepción: llega un `ResultadoConsultaSat`. |
| Datos inválidos al construir `DatosConsultaCfdi` | `ArgumentException`. |
| XML que no es un CFDI timbrado válido en `DesdeXml` | `FormatException`, con el dato que falta. |
| Cancelaste el `CancellationToken` | `OperationCanceledException`. El tiempo de espera propio **no** cuenta como cancelación. |
| Opciones inválidas | `OptionsValidationException` (con DI) o `ArgumentException` (sin DI). |

## 🔒 Seguridad y privacidad

- El XML del CFDI y la respuesta del SAT se leen sin DTD ni entidades externas (sin XXE).
- Solo se acepta un endpoint `https`; la respuesta tiene un tope de 1 MB.
- Los logs solo llevan el UUID y el resultado, nunca RFC ni total.

## 🧪 Comportamiento verificado del servicio del SAT

Probado contra el servicio real el 2026-10-15:

- El SAT compara el **total por valor**: acepta `11558.1`, `11558.10`, `1595` o `16,799.99`. La librería envía una forma canónica (punto decimal, de 2 a 6 decimales).
- `fe` (últimos 8 del sello) es **opcional**: con o sin él, la respuesta es la misma.
- Un **RFC con `&`** (por ejemplo `SR&...`) solo funciona escrito como `&amp;` dentro de la expresión; con el `&` crudo o con `%26` el SAT responde 601. La librería lo hace sola.
- El **UUID** se acepta en mayúsculas aunque el PAC lo haya timbrado en minúsculas.
- Un dato que no coincide (por ejemplo, el total) devuelve **601**, no 602, y aun así el SAT llena `EsCancelable`: por eso la decisión se toma solo con `CodigoEstatus` y `Estado`.
- RFC con **Ñ**: la sintaxis se acepta (con un dato ficticio el SAT respondió 602, no 601), pero no se ha verificado con un CFDI real.

## 🔁 Migración desde 1.x

La 2.0.0 cambia la API por completo (sin WCF). Ver [MIGRATION.md](MIGRATION.md).

## 🧑‍💻 Desarrollo

```bash
dotnet build -c Release
dotnet test -c Release --filter "Categoria!=Integracion"
```

Pruebas contra el SAT real (opcionales; los CFDI reales viven **fuera** del repositorio):

```bash
ESTATUSCFDI_SAT_INTEGRACION=1 \
ESTATUSCFDI_SAT_CASOS=/ruta/casos.txt \
ESTATUSCFDI_SAT_XMLS=/ruta/xmls \
dotnet test -c Release --filter "Categoria=Integracion"
```

- `casos.txt`: líneas `estado|rfcEmisor|rfcReceptor|total|uuid|fragmento`.
- `xmls/`: CFDI timbrados nombrados `<estado>__<nombre>.xml` (por ejemplo, `Cancelado__factura-1.xml`).

## 📝 Licencia

MIT. Ver [LICENSE](LICENSE).

## 👤 Autor

- [@Juliocbm](https://github.com/Juliocbm)
