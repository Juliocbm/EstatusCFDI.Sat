# 📦 EstatusCFDI.Sat

using ConsultaCFDI;
using EstatusCFDI.Sat.Interfaces;
using EstatusCFDI.Sat;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reflection;
using System;

[![NuGet](https://img.shields.io/nuget/v/EstatusCFDI.Sat.svg?style=flat-square)](https://www.nuget.org/packages/EstatusCFDI.Sat/)

[![NuGet Downloads](https://img.shields.io/nuget/dt/EstatusCFDI.Sat.svg?style=flat-square)](https://www.nuget.org/packages/EstatusCFDI.Sat/)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)](https://opensource.org/licenses/MIT)

[![GitHub stars](https://img.shields.io/github/stars/Juliocbm/EstatusCFDI.Sat?style=flat-square)](https://github.com/Juliocbm/EstatusCFDI.Sat/stargazers)


## Descripción general
Librería .NET 6 para consultar el **estatus de un CFDI** (Factura electrónica) a través del **servicio web del SAT (México)**. Encapsula la lógica y comunicación con el WSDL del SAT, facilitando su integración en cualquier proyecto .NET.


## ⚙️ ¿Qué hace esta librería?
- 📝 Construye automáticamente la cadena de consulta del SAT: "?re=XXX&rr=YYY&tt=ZZZ&id=UUID"
- 📥 Consume el servicio WSDL publicado por el SAT.
- 🔁 Devuelve un objeto Acuse con el estado del CFDI.

## Ventajas
🎯 Uso simplificado con una sola línea (EstatusFacade)

🧪 Fácilmente testeable gracias a interfaces (IConsultaEstatusService)

🔒 Compatible con sistemas de facturación electrónica en México

🧩 Diseñado como librería modular, ideal para APIs o backends

## 📚 Requisitos
✔️ .NET 6.0 o superior

✔️ Acceso a Internet para consumir el servicio del SAT
## 📥 Instalación
### Desde NuGet:

```bash
Install-Package EstatusCFDI.Sat
```
## Uso basico

```csharp
using EstatusCFDI;

var resultado = await EstatusFacade.ConsultarEstatusAsync(
    "HGT9312179LA",
    "RCA940729470",
    6040.07m,
    "14AE8D86-71E2-45FE-8604-BE5FF8B66399"
);

Console.WriteLine(resultado.Estado); // Ej: "Vigente", "Cancelado", etc.
```

## Author

- [@Juliocbm](https://github.com/Juliocbm)

