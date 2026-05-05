using System;
using System.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

namespace ContosoUniversity.Services
{
    /// <summary>
    /// Singleton factory that configures an OpenTelemetry-backed
    /// <see cref="ILoggerFactory"/> for the application.
    ///
    /// Usage:
    ///   var logger = LoggingService.CreateLogger&lt;MyClass&gt;();
    ///
    /// Initialise once from Global.asax Application_Start() by calling
    /// <see cref="Initialize"/> and dispose on Application_End() via
    /// <see cref="Shutdown"/>.
    /// </summary>
    public static class LoggingService
    {
        private static ILoggerFactory _loggerFactory;
        private static readonly object _lock = new object();

        /// <summary>
        /// Initialise the OpenTelemetry logging pipeline.
        /// Must be called once during application startup.
        /// </summary>
        public static void Initialize()
        {
            if (_loggerFactory != null)
                return;

            lock (_lock)
            {
                if (_loggerFactory != null)
                    return;

                var serviceName = ConfigurationManager.AppSettings["OpenTelemetry:ServiceName"] ?? "ContosoUniversity";
                var serviceVersion = ConfigurationManager.AppSettings["OpenTelemetry:ServiceVersion"] ?? "1.0.0";

                _loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .SetMinimumLevel(LogLevel.Information)
                        .AddOpenTelemetry(options =>
                        {
                            options.SetResourceBuilder(
                                ResourceBuilder.CreateDefault()
                                    .AddService(
                                        serviceName: serviceName,
                                        serviceVersion: serviceVersion));

                            // Always add the console exporter so logs are visible in development.
                            options.AddConsoleExporter();

                            // Include formatted message in the log record so exporters that
                            // don't expand structured parameters still show human-readable text.
                            options.IncludeFormattedMessage = true;

                            // Include scopes for richer context information.
                            options.IncludeScopes = true;
                        });
                });
            }
        }

        /// <summary>
        /// Creates a strongly-typed <see cref="ILogger{T}"/>.
        /// </summary>
        public static ILogger<T> CreateLogger<T>()
        {
            EnsureInitialized();
            return _loggerFactory.CreateLogger<T>();
        }

        /// <summary>
        /// Creates an <see cref="ILogger"/> for the given category name.
        /// </summary>
        public static ILogger CreateLogger(string categoryName)
        {
            EnsureInitialized();
            return _loggerFactory.CreateLogger(categoryName);
        }

        /// <summary>
        /// Disposes the underlying <see cref="ILoggerFactory"/> and flushes
        /// any buffered telemetry.  Call from Application_End().
        /// </summary>
        public static void Shutdown()
        {
            lock (_lock)
            {
                _loggerFactory?.Dispose();
                _loggerFactory = null;
            }
        }

        private static void EnsureInitialized()
        {
            if (_loggerFactory == null)
                Initialize();
        }
    }
}
