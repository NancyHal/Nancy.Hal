namespace Nancy.Hal.Example.Model.Users.ViewModels
{
    using System;

    public interface IHaveCreatedModifiedTimestamps
    {
        DateTimeOffset Created { get; set; }

        DateTimeOffset? Modified { get; set; }
    }
}