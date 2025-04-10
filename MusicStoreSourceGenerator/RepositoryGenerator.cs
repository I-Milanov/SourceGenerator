using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MusicStoreRepositoryGenerator;

namespace MusicStoreRepositoryGenerator
{
    [Generator]
    public class RepositoryGenerator : IIncrementalGenerator
    {   

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Регистрираме доставчик на данни за допълнителните файлове
            IncrementalValuesProvider<AdditionalText> apiEndpointsFiles = context.AdditionalTextsProvider
                .Where(file => Path.GetFileName(file.Path).Equals("api-endpoints.json", StringComparison.OrdinalIgnoreCase));

            // Трансформираме файла в конфигурация
            IncrementalValueProvider<(ApiEndpointsConfig Config, string ConfigPath)> configProvider = apiEndpointsFiles
                .Select((file, cancellationToken) =>
                {
                    string content = file.GetText(cancellationToken)?.ToString() ?? "{}";
                    ApiEndpointsConfig config = null;

                    try
                    {
                        config = JsonSerializer.Deserialize<ApiEndpointsConfig>(content);
                    }
                    catch
                    {
                        config = new ApiEndpointsConfig { Endpoints = new List<ApiEndpoint>() };
                    }

                    return (config, file.Path);
                })
                .Collect()
                .Select((configs, _) => configs.FirstOrDefault());

            // Регистрираме генератор за моделите и repository класовете
            context.RegisterSourceOutput(configProvider, (context, configData) =>
            {
                if (configData.Config == null || configData.Config.Endpoints == null || !configData.Config.Endpoints.Any())
                {
                    ReportDiagnostic(context, $"Invalid or missing configuration in '{configData.ConfigPath}'.");
                    return;
                }

                // Генерираме моделите и repository класове
                foreach (var endpoint in configData.Config.Endpoints)
                {
                    // Генерираме модела
                    string modelCode = CodeWriter.GenerateModelClass(endpoint.ModelName, endpoint.Entity);
                    context.AddSource($"{endpoint.ModelName}.g.cs", SourceText.From(modelCode, Encoding.UTF8));

                    // Генерираме repository класа
                    string repositoryCode = CodeWriter.GenerateRepositoryClass(endpoint.ModelName, endpoint.Path, endpoint.Entity);
                    context.AddSource($"{endpoint.ModelName}Repository.g.cs", SourceText.From(repositoryCode, Encoding.UTF8));
                }
            });
        }

        private void ReportDiagnostic(SourceProductionContext context, string message)
        {
            var descriptor = new DiagnosticDescriptor(
                id: "MSRG001",
                title: "Music Store Repository Generator Error",
                messageFormat: message,
                category: "RepositoryGenerator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
        }
    }
}
