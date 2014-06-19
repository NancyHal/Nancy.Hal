namespace Nancy.Hal.Example.Hal
{
    using System;

    using Nancy;

    public class HalResponse<TModel> : Response
    {
        public HalResponse(TModel model, ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new InvalidOperationException("JSON Serializer not set");
            }

            this.Contents = stream => serializer.Serialize("application/hal+json", model, stream);
            this.ContentType = "application/hal+json";
            this.StatusCode = HttpStatusCode.OK;
        }
    }

    public class HalResponse : HalResponse<object>
    {
        public HalResponse(object model, ISerializer serializer)
            : base(model, serializer)
        {
        }
    }
}
