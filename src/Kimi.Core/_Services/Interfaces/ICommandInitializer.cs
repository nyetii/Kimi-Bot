namespace Kimi.Core._Services.Interfaces
{
    internal interface ICommandInitializer
    {
        private protected static bool IsRegistered;
        private protected static bool IsEnabled;
        Task Initialize();

        Task CreateCommand();
    }
}
