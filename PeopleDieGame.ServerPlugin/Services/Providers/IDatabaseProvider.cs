namespace PeopleDieGame.ServerPlugin.Services.Providers
{
    public interface IDatabaseProvider<T>
    {
        T GetData();
        bool CommitData();
    }
}
