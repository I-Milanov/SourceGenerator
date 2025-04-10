namespace MusicStore.Repositories
{
    public abstract class HttpRepository<T> where T : class
    {
        protected readonly HttpClient _httpClient;

        public abstract string ApiPath { get; }

        protected HttpRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public virtual T GetItem(int id)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<T> GetItems()
        {
            throw new NotImplementedException();
        }

        public virtual void CreateItem(T item)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateItem(T item)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteItem(int id)
        {
            throw new NotImplementedException();
        }
    }
}
