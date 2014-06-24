namespace Nancy.Hal.Example.Model.Users.ViewModels
{
    using System;

    public interface IHaveCreatedModifiedTimestamps
    {
        DateTime Created { get; set; }

        DateTime? Modified { get; set; }
    }
}