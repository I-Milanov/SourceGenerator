using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MusicStoreRepositoryGenerator
{
    public static class CodeWriter
    {
        public static string GenerateModelClass(string modelName, List<EntityColumn> columns)
        {
            var propertiesCode = new StringBuilder();

            foreach (var column in columns)
            {
                propertiesCode.AppendLine($"    public {column.ColumnType} {column.ColumnName} {{ get; set; }}");
            }

            return $@"
        using System;
        using System.Collections.Generic;
        using System.Text.Json.Serialization;

        namespace MusicStore.Models
        {{
            public class {modelName}
            {{
        {propertiesCode}
            }}
        }}";
        }

        public static string GenerateRepositoryClass(string modelName, string apiPath, List<EntityColumn> columns)
        {
            // Намираме колоната, която е Id
            var idColumn = columns.FirstOrDefault(c => c.ColumnName.Equals("Id", StringComparison.OrdinalIgnoreCase));
            string idType = idColumn?.ColumnType ?? "int";

            return $@"
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MusicStore.Models;

namespace MusicStore.Repositories
{{
    public class {modelName}Repository : HttpRepository<{modelName}>
    {{
        public override string ApiPath => ""{apiPath}"";
    }}
}}";
        }
    }
}
