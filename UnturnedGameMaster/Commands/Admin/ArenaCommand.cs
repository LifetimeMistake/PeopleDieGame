using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands.Admin
{
    public class ArenaCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "arena";

        public string Help => "";

        public string Syntax => "<create/setname/setactdist/setdeactdist/setreward/setbounty/setactpoint/setbossspawn/setrewardspawn/setboss> <value>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public Dictionary<string, ArenaBuilder> Builders = new Dictionary<string, ArenaBuilder>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, $"Musisz podać argument.");
                ShowSyntax(caller);
                return;
            }

            string[] verbArgs = command.Skip(1).ToArray();
            switch (command[0].ToLowerInvariant())
            {
                case "create":
                    VerbCreate(caller);
                    break;
                case "setname":
                    VerbSetName(caller, verbArgs);
                    break;
                case "setactdist":
                    VerbSetActDist(caller, verbArgs);
                    break;
                case "setdeactdist":
                    VerbSetDeactDist(caller, verbArgs);
                    break;
                case "setreward":
                    VerbSetReward(caller, verbArgs);
                    break;
                case "setbounty":
                    VerbSetBounty(caller, verbArgs);
                    break;
                case "setactpoint":
                    VerbSetActPoint(caller);
                    break;
                case "setbossspawn":
                    VerbSetBossSpawn(caller);
                    break;
                case "setrewardspawn":
                    VerbSetRewardSpawn(caller);
                    break;
                case "setboss":
                    VerbSetBoss(caller, verbArgs);
                    break;
                case "submit":
                    VerbSubmit(caller);
                    break;
                default:
                    ChatHelper.Say(caller, $"Nieprawidłowy argument.");
                    ShowSyntax(caller);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbCreate(IRocketPlayer caller)
        {
            try
            {
                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);
                VectorPAR playerPos = new VectorPAR(callerPlayer.Position, (byte)callerPlayer.Rotation);

                ArenaBuilder arenaBuilder = new ArenaBuilder();
                arenaBuilder.SetName("default");
                arenaBuilder.SetActivationDistance(10);
                arenaBuilder.SetDeactivationDistance(20);
                arenaBuilder.SetCompletionReward(100);
                arenaBuilder.SetCompletionBounty(100);
                arenaBuilder.SetActivationPoint(callerPlayer.Position);
                arenaBuilder.SetBossSpawnPoint(playerPos);
                arenaBuilder.SetRewardSpawnPoint(playerPos);
                arenaBuilder.SetBoss(null);

                Builders.Add(caller.Id, arenaBuilder);
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się utworzyć areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetName(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać nazwę areny");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                string name = string.Join(" ", command.Skip(1));
                arenaBuilder.SetName(name);

                ChatHelper.Say(caller, "Ustawiono nazwę areny");
            }
            catch (Exception ex)
            { 
                ChatHelper.Say(caller, $"Nie udało się ustawić nazwy areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetActDist(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać dystans aktywacji areny");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                if (!double.TryParse(command[1], out double distance))
                {
                    ChatHelper.Say(caller, "Musisz podać odpowiedni dystans aktywacji areny");
                    return;
                }

                arenaBuilder.SetActivationDistance(distance);

                ChatHelper.Say(caller, "Ustawiono dystans aktywacji areny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić dystansu aktywacji areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetDeactDist(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać dystans dezaktywacji areny");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                if (!double.TryParse(command[1], out double distance))
                {
                    ChatHelper.Say(caller, "Musisz podać odpowiedni dystans dezaktywacji areny");
                    return;
                }

                arenaBuilder.SetDeactivationDistance(distance);

                ChatHelper.Say(caller, "Ustawiono dystans dezaktywacji areny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić dystansu dezaktywacji areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetReward(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać wyskość nagrody areny");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                if (!double.TryParse(command[1], out double amount))
                {
                    ChatHelper.Say(caller, "Musisz podać odpowiednią wyskość nagrody areny");
                    return;
                }

                arenaBuilder.SetCompletionReward(amount);

                ChatHelper.Say(caller, "Ustawiono wysokość nagrody areny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić wysokości nagrody areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetBounty(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać wyskość bounty areny");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                if (!double.TryParse(command[1], out double amount))
                {
                    ChatHelper.Say(caller, "Musisz podać odpowiednią wyskość bounty areny");
                    return;
                }

                arenaBuilder.SetCompletionBounty(amount);

                ChatHelper.Say(caller, "Ustawiono wysokość bounty areny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić wysokości bounty areny z powodu błędu serwera: {ex.Message}");
            }
        }
        
        private void VerbSetActPoint(IRocketPlayer caller)
        {
            try
            {
                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);

                arenaBuilder.SetActivationPoint(callerPlayer.Position);

                ChatHelper.Say(caller, "Ustawiono punkt aktywacji areny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić punktu aktywacji areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetBossSpawn(IRocketPlayer caller)
        {
            try
            {
                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);
                VectorPAR playerPos = new VectorPAR(callerPlayer.Position, (byte)callerPlayer.Rotation);

                arenaBuilder.SetBossSpawnPoint(playerPos);

                ChatHelper.Say(caller, "Ustawiono punkt spawnu boss'a areny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić punktu spawnu boss'a areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetRewardSpawn(IRocketPlayer caller)
        {
            try
            {
                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                UnturnedPlayer callerPlayer = UnturnedPlayer.FromCSteamID(((UnturnedPlayer)caller).CSteamID);
                VectorPAR playerPos = new VectorPAR(callerPlayer.Position, (byte)callerPlayer.Rotation);

                arenaBuilder.SetRewardSpawnPoint(playerPos);

                ChatHelper.Say(caller, "Ustawiono punkt spawnu nagrody areny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić punktu spawnu nagrody areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSetBoss(IRocketPlayer caller, string[] command)
        {
            try
            {
                if (command.Length == 0)
                {
                    ChatHelper.Say(caller, "Musisz podać nazwę boss'a");
                    return;
                }

                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                string searchTerm = string.Join(" ", command.Skip(1));
                IBoss boss = ServiceLocator.Instance.LocateServicesOfType<IBoss>()
                    .FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()));

                if (boss == null)
                {
                    ChatHelper.Say(caller, "Nie znaleziono podanego boss'a");
                    return;
                }

                arenaBuilder.SetBoss(boss);

                ChatHelper.Say(caller, "Ustawiono boss'a areny");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się ustawić nazwy boss'a areny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbSubmit(IRocketPlayer caller)
        {
            try
            {
                if (!Builders.TryGetValue(caller.Id, out ArenaBuilder arenaBuilder))
                {
                    ChatHelper.Say(caller, "Nie rozpcząłeś procesu tworzenia areny");
                    return;
                }

                ArenaManager arenaManager = ServiceLocator.Instance.LocateService<ArenaManager>();

                arenaManager.CreateArena(arenaBuilder);
                Builders.Remove(caller.Id);
                ChatHelper.Say(caller, "Utworzono arenę");
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się zatwierdzić areny z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
