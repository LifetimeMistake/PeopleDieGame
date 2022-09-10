namespace UnturnedGameMaster.Providers
{
    public interface IDatabaseProvider<T>
    {
        T GetData();
        bool CommitData();
    }
}
