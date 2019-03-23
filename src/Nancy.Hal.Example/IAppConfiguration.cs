namespace Nancy.Hal.Example
{
    public interface IAppConfiguration
    {
        Logging Logging { get; }
        Smtp Smtp { get; }
    }
}
