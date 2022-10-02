using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using PeopleDieGame.ServerPlugin.Helpers;

namespace PeopleDieGame.ServerPlugin.Commands.Admin
{
    public class EvaporateCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "evaporate";

        public string Help => "death";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać obiekt(y) do zniszczenia: (players, zombies, structures, vehicles, animals, objects, resources)");
                return;
            }

            IEnumerable<string> thingsToKill = command.Select(x => x.ToLowerInvariant());
            float playerDamage = thingsToKill.Contains("players") ? 999999 : 0;
            float zombieDamage = thingsToKill.Contains("zombies") ? 999999 : 0;
            float structureDamage = thingsToKill.Contains("structures") ? 999999 : 0;
            float vehicleDamage = thingsToKill.Contains("vehicles") ? 999999 : 0;
            float animalDamage = thingsToKill.Contains("animals") ? 999999 : 0;
            float objectDamage = thingsToKill.Contains("objects") ? 999999 : 0;
            float resourceDamage = thingsToKill.Contains("resources") ? 999999 : 0;

            List<EPlayerKill> pks = new List<EPlayerKill>();

            UnturnedPlayer player = (UnturnedPlayer)caller;
            DamageTool.explode(player.Position, 100f, EDeathCause.SHRED, player.CSteamID, playerDamage, zombieDamage, structureDamage, vehicleDamage,
                structureDamage, vehicleDamage, resourceDamage, objectDamage, out pks, EExplosionDamageType.CONVENTIONAL, 0, true, true,
                EDamageOrigin.Punch, ERagdollEffect.ZERO_KELVIN);
        }
    }
}
