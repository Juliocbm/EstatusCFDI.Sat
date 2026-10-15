using Microsoft.Extensions.Logging;

namespace EstatusCFDI.Sat.Tests.Helpers;

/// <summary>
/// Guarda los mensajes registrados para revisar qué datos llegan al log.
/// </summary>
internal sealed class LoggerEnMemoria<T> : ILogger<T>
{
    public List<(LogLevel Nivel, string Mensaje)> Entradas { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Entradas.Add((logLevel, formatter(state, exception)));
}
