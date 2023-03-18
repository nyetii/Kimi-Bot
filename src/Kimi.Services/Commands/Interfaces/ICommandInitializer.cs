namespace Kimi.Services.Commands.Interfaces
{
    public interface ICommandInitializer
    {
        private protected static bool IsRegistered;
        private protected static bool IsEnabled;
        Task Initialize();

        Task CreateCommand();
    }
}
