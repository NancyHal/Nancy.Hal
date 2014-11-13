namespace Nancy.Hal.ResponseProcessor
{
    using Nancy.Responses;

    public class HalJsonResponse : JsonResponse
    {
        public HalJsonResponse(object model, ISerializer serializer)
            : base(model, serializer)
        {
            ContentType = "application/hal+json";
        }
    }
}