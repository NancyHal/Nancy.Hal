namespace Nancy.Hal.Example.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    public class PagedList<T> : IPagedList<T>
    {
        private readonly IList<T> inner;

        public PagedList()
        {
            this.inner = new List<T>();
        } 

        public PagedList(IEnumerable<T> enumerable)
        {
            this.inner = enumerable.ToList();
        }

        public PagedList(IList<T> list)
        {
            this.inner = list;
        }

        public IList<T> Data { get { return this.inner; } } 

        public long PageNumber { get; set; }

        public long PageSize { get; set; }

        public long TotalResults { get; set; }

        public long TotalPages
        {
            get
            {
                return PageSize == 0 ? 1 : (long)Math.Ceiling((double)TotalResults / PageSize);
            }
        }

        public void Add(T item)
        {
            this.inner.Add(item);
        }

    }
}