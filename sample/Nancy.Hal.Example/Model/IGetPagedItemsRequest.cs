namespace Nancy.Hal.Example.Model
{
    public interface IGetPagedItemsRequest
    {
        int? Page { get; set; }

        int? PageSize { get; set; }
    }
}