# EstatusCFDI.Sat — Changelog

Todas las versiones notables de este proyecto se documentan en este archivo.
El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y este proyecto usa [Versionado Semántico](https://semver.org/lang/es/).

## [2.0.0] - 2026-10-15

Reconstrucción completa. Cambia la API pública: ver [MIGRATION.md](MIGRATION.md).

### Agregado
- `IConsultaEstatusCfdi` / `ConsultaEstatusCfdiSat`: consulta sobre `HttpClient`, con tiempo de espera configurable (`EstatusCfdiSatOptions.Timeout`, 25 s por defecto) y `CancellationToken`.
- `ResultadoConsultaSat` con `EstadoConsulta` (`Vigente`, `Cancelado`, `NoEncontrado`, `ExpresionInvalida`, `SinRespuesta`, `RespuestaNoReconocida`), `EsConcluyente` y el acuse crudo del SAT (`AcuseSat`).
- `DatosConsultaCfdi` validado (RFC con el patrón oficial, total, UUID, fragmento del sello) y `DatosConsultaCfdi.DesdeXml(...)` para CFDI 3.3 y 4.0.
- `services.AddEstatusCfdiSat(...)`: cliente HTTP tipado, opciones validadas al arrancar; devuelve `IHttpClientBuilder`.
- Logs estructurados con el UUID y el resultado (sin RFC ni total).
- Pruebas unitarias y de integración contra el SAT real (opcionales); CI en GitHub Actions con publicación por tag.

### Corregido
- RFC con `&` (por ejemplo `SR&...`): se envía como `&amp;` en la expresión; antes el SAT respondía 601.
- Un SAT caído o lento ya no deja la consulta colgada un minuto ni lanza excepciones de WCF: el resultado es `SinRespuesta`.
- `Dispose` sobre un canal WCF en falla ya no tapa el error original (ya no hay WCF).

### Changed (BREAKING)
- Se elimina el cliente WCF (`ConsultaCFDIServiceClient`, `Acuse`), `EstatusFacade`, `IConsultaEstatusService`, `ConsultaEstatusService` y `SatQueryBuilder`.
- Frameworks: `net10.0` y `net8.0` (antes `net6.0`). Sin dependencias de `System.ServiceModel`.

### Cambiado
- El total se envía en forma canónica (punto decimal, sin separador de miles, de 2 a 6 decimales). El SAT compara por valor.
- README sin datos fiscales reales.

## [1.0.6]
- Sin detalle registrado.

## [1.0.5] - 2025-04-30
### Changed
- Ajuste de README.md.

## [1.0.1] – [1.0.4]
- Sin detalle registrado.

## [1.0.0] - 2025-04-17
### Added
- Métodos para consultar el estatus de un CFDI ante el SAT.
