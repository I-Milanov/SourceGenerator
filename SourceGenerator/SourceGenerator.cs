using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGenerator;
using SourceGenerator.DTOs;

namespace MusicStoreRepositoryGenerator
{
    [Generator]
    public class SourceGenerator : IIncrementalGenerator
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

        // Клас, който ще събира информация за класовете, маркирани с нашия атрибут
        private class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<ClassInfo> CandidateClasses { get; } = new List<ClassInfo>();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                // Проверяваме дали текущият node е декларация на клас
                if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    // Получаваме символа за класа
                    var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                    if (classSymbol == null)
                        return;

                    // Проверяваме дали класът има нашия атрибут
                    var generateRepositoryAttribute = classSymbol.GetAttributes()
                        .FirstOrDefault(attr => attr.AttributeClass.Name == "GenerateRepositoryAttribute");

                    if (generateRepositoryAttribute != null)
                    {
                        // Извличаме стойността на ApiEndpoint от атрибута
                        string apiEndpoint = generateRepositoryAttribute.ConstructorArguments[0].Value.ToString();

                        // Добавяме информацията за класа в списъка с кандидати
                        CandidateClasses.Add(new ClassInfo
                        {
                            ClassSymbol = (INamedTypeSymbol)classSymbol,
                            ApiEndpoint = apiEndpoint
                        });
                    }
                }
            }
        }

        // Клас, който съхранява информация за класовете, които трябва да генерираме
        private class ClassInfo
        {
            public INamedTypeSymbol ClassSymbol { get; set; }
            public string ApiEndpoint { get; set; }
        }
    }
}
