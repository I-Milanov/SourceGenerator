using System.Collections.Generic;

namespace MusicStoreRepositoryGenerator
{
    public class ApiEndpoint
    {
        public string ModelName { get; set; }
        public string Path { get; set; }
        public List<EntityColumn> Entity { get; set; }
    }
}
