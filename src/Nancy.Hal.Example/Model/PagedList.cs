namespace Nancy.Hal.Example.Model
{
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

        [JsonIgnore]
        public int Count { get { return this.inner.Count; } }
        
        [JsonIgnore]
        public bool IsReadOnly { get { return this.inner.IsReadOnly; } }

        public T this[int index]
        {
            get { return this.inner[index]; }
            set { this.inner[index] = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.inner.GetEnumerator();
        }

        public void Add(T item)
        {
            this.inner.Add(item);
        }

        public void Clear()
        {
            this.inner.Clear();
        }

        public bool Contains(T item)
        {
            return this.inner.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.inner.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return this.inner.Remove(item);
        }

        public int IndexOf(T item)
        {
            return this.inner.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.inner.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.inner.RemoveAt(index);
        }
    }
}