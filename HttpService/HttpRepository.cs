using System;
using System.Collections.Generic;
using MusicStore.Models;

namespace MusicStore.Repositories
{
    public abstract class HttpRepository<T> where T : HttpEntity
    {

        public abstract string ApiPath { get; }

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
