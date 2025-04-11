using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace MusicStoreRepositoryGenerator
{
    [Generator]
    public class RepositoryGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<AdditionalText> apiEndpointsFiles = context.AdditionalTextsProvider
             .Where(file => Path.GetFileName(file.Path).Equals("endpoints.json", StringComparison.OrdinalIgnoreCase));

            IncrementalValueProvider<(ApiEndpointsConfig Config, string ConfigPath)> configProvider = apiEndpointsFiles
                .Select((file, cancellationToken) =>
                {
                    string content = file.GetText(cancellationToken)?.ToString() ?? "{}";
                    ApiEndpointsConfig config = config = JsonConvert.DeserializeObject<ApiEndpointsConfig>(content);

                    return (config, file.Path);
                })
                .Collect()
                .Select((configs, _) => configs.FirstOrDefault());

            context.RegisterSourceOutput(configProvider, (context, configData) =>
                {
                    foreach (var endpoint in configData.Config.Endpoints)
                    {
                        string modelCode = CodeWriter.GenerateModelClass(endpoint.ModelName, endpoint.Entity);
                        context.AddSource($"{endpoint.ModelName}.g.cs", SourceText.From(modelCode, Encoding.UTF8));
                        
                        string repositoryCode = CodeWriter.GenerateRepositoryClass(endpoint.ModelName, endpoint.Path, endpoint.Entity);
                        context.AddSource($"{endpoint.ModelName}Repository.g.cs", SourceText.From(repositoryCode, Encoding.UTF8));
                    }
                });
        }
    }
}
