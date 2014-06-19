namespace Nancy.Hal.Example.Model.Users.Queries
{
    public class GetUserList : IGetPagedItemsRequest
    {
        public string Query { get; set; }

        public int? Page { get; set; }

        public int? PageSize { get; set; }
    }
}