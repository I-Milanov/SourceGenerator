using System.Collections.Generic;
using System.Text;

namespace MusicStoreRepositoryGenerator
{
    public static class CodeWriter
    {
        public static string GenerateModelClass(string modelName, List<EntityColumn> columns)
        {
            var builder = new StringBuilder()
            .AppendLine("using System;")
            .AppendLine()
            .AppendLine("namespace MusicStore.Models")
            .AppendLine("{")
            .AppendLine($"    public class {modelName}Http : HttpEntity")
            .AppendLine("    {");
            foreach (var column in columns)
            {
                builder.AppendLine($@"         public {column.ColumnType} {column.ColumnName} {{ get; set; }}");
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");


            return builder.ToString();
        }

        public static string GenerateRepositoryClass(string modelName, string apiPath, List<EntityColumn> columns)
        {
            var builder = new StringBuilder()
            .AppendLine("using System;")
            .AppendLine("using MusicStore.Models;")
            .AppendLine()
            .AppendLine("namespace MusicStore.Repositories")
            .AppendLine("{")
            .AppendLine($"    public class {modelName}HttpRepository : HttpRepository<{modelName}>")
            .AppendLine("    {")
            .AppendLine($"        public override string ApiPath => \"{apiPath}\";")
            .AppendLine("    }")
            .AppendLine("}");

            return builder.ToString();
        }
    }
}
