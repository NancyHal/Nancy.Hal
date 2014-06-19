namespace Nancy.Hal.Example.Model
{
    using System.Collections.Generic;

    public interface IPagedList<T>
    {
        long PageNumber { get; }

        long PageSize { get;  }

        long TotalResults { get; }

        IList<T> Data { get; } 
    }
}