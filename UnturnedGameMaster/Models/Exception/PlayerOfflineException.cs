namespace UnturnedGameMaster.Models.Exception
{
    public class PlayerOfflineException : System.Exception
    {
        public PlayerOfflineException() : base("Player was offline.")
        { }
        public PlayerOfflineException(string playerName) : base($"Player \"{playerName}\" was offline.")
        { }
        public PlayerOfflineException(ulong playerId) : base($"Player ID {playerId} was offline.")
        { }
    }
}
