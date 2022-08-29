using Rocket.API;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Commands.Admin
{
    public class ManagePlayersCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "players";

        public string Help => "";

        public string Syntax => "<getTeam/joinTeam/leaveTeam/promotePlayer/setBio> <playerName/playerId> [<teamName/teamId/bio>]";
        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "manage" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedChat.Say("augh");
        }
    }
}
