using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Controllers;
using UnturnedGameMaster.Enums;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.Bosses;
using UnturnedGameMaster.Models.EventArgs;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class ArenaManager : IDisposableManager
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }
        [InjectDependency]
        private ArenaIdProvider arenaIdProvider { get; set; }
        [InjectDependency]
        private TimerManager timerManager { get; set; }

        private List<BossFight> ongoingBossFights = new List<BossFight>();

        public event EventHandler<ArenaEventArgs> OnArenaCreated;
        public event EventHandler<ArenaEventArgs> OnArenaRemoved;
        public event EventHandler<BossFightEventArgs> OnBossFightCreated;
        public event EventHandler<BossFightEventArgs> OnBossFightRemoved;
        public event EventHandler<BossFightEventArgs> OnBossFightCompleted;
        public event EventHandler<BossFightEventArgs> OnBossFightFailed;
        public event EventHandler<BossFightDominationEventArgs> OnBossFightDominantTeamChanged;

        public void Init()
        {
            timerManager.Register(ProcessFightStartConditions, 60);
            timerManager.Register(ProcessFightEndConditions, 60);
            timerManager.Register(UpdateDominantTeams, 60);
            timerManager.Register(UpdateFights, 1);

            ArenaBuilder arenaBuilder = new ArenaBuilder();
            arenaBuilder.SetName("augh");
            arenaBuilder.SetBoss(new TestBoss());
            arenaBuilder.SetBossSpawnPoint(new VectorPAR(new Vector3(10, 20, 30), 0));
            arenaBuilder.SetActivationPoint(new Vector3(50, 30, 50));
            arenaBuilder.SetActivationDistance(10);
            arenaBuilder.SetDeactivationDistance(20);
            arenaBuilder.SetCompletionReward(100);
            arenaBuilder.SetCompletionBounty(500);

            BossArena arena = CreateArena(arenaBuilder);
        }

        public void Dispose()
        {
            timerManager.Unregister(ProcessFightStartConditions);
            timerManager.Unregister(ProcessFightEndConditions);
            timerManager.Unregister(UpdateFights);
            timerManager.Unregister(UpdateDominantTeams);
        }

        public BossArena CreateArena(ArenaBuilder arenaBuilder)
        {
            if (arenaBuilder == null)
                throw new ArgumentNullException(nameof(arenaBuilder));

            int arenaId = arenaIdProvider.GenerateId();
            BossArena arena = arenaBuilder.ToArena(arenaId);

            Dictionary<int, BossArena> arenas = dataManager.GameData.Arenas;
            arenas.Add(arenaId, arena);
            OnArenaCreated?.Invoke(this, new ArenaEventArgs(arena));
            return arena;
        }

        public bool DeleteArena(int id)
        {
            Dictionary<int, BossArena> arenas = dataManager.GameData.Arenas;
            BossArena arena = GetArena(id);
            if (arena == null)
                return false;

            // Make sure all ongoing fights are killed prior to deletion.
            foreach (BossFight bossFight in ongoingBossFights.Where(x => x.Arena.Id == id))
                EndBossFight(bossFight);

            arenas.Remove(id);
            OnArenaRemoved?.Invoke(this, new ArenaEventArgs(arena));
            return true;
        }

        public BossArena GetArena(int id)
        {
            Dictionary<int, BossArena> arenas = dataManager.GameData.Arenas;
            if (!arenas.ContainsKey(id))
                return null;

            return arenas[id];
        }

        public BossArena GetArenaByName(string name, bool exactMatch = true)
        {
            Dictionary<int, BossArena> arenas = dataManager.GameData.Arenas;
            if (exactMatch)
                return arenas.Values.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            else
                return arenas.Values.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }

        public StringBuilder GetArenaSummary(BossArena arena)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Nazwa areny: \"{arena.Name}\" | ID: {arena.Id}");
            
            if (arena.Conquered)
                sb.AppendLine($"Status: Pokonana");
            else
                sb.AppendLine($"Status: Niepokonana");

            sb.AppendLine($"Nazwa boss'a: \"{arena.BossModel.Name}\"");
            sb.AppendLine($"Dystans aktywacji: \"{arena.ActivationDistance}\"");
            sb.AppendLine($"Dystans dezaktywacji: \"{arena.DeactivationDistance}\"");
            sb.AppendLine($"Bounty: ${arena.CompletionBounty}");
            sb.AppendLine($"Nagroda: ${arena.CompletionReward}");
            sb.AppendLine($"Punkt aktywacji: {arena.ActivationPoint}");
            sb.AppendLine($"Punkt spawnu boss'a: {arena.BossSpawnPoint}");
            sb.AppendLine($"Punkt spawnu nagrody: {arena.RewardSpawnPoint}");

            return sb;
        }

        public BossArena ResolveArena(string arenaNameOrId, bool exactMatch)
        {
            int id;
            if (int.TryParse(arenaNameOrId, out id))
            {
                BossArena arena = GetArena(id);
                if (arena != null)
                    return arena;
            }

            return GetArenaByName(arenaNameOrId, exactMatch);
        }

        public List<UnturnedPlayer> GetPlayersInsideArena(BossArena arena)
        {
            return Provider.clients
                .Select(x => UnturnedPlayer.FromPlayer(x.player))
                .Where(x =>
                {
                    return Vector3.Distance(arena.ActivationPoint, x.Position) <= arena.DeactivationDistance
                     || Vector3.Distance(arena.BossSpawnPoint.Position, x.Position) <= arena.DeactivationDistance;
                }).ToList();
        }

        public List<UnturnedPlayer> GetPlayerInActivationRange(BossArena arena, Team team = null)
        {
            return Provider.clients
                .Select(x => UnturnedPlayer.FromPlayer(x.player))
                .Where(x => team == null || playerDataManager.GetPlayer((ulong)x.CSteamID).TeamId == team.Id)
                .Where(x =>
                {
                    return Vector3.Distance(arena.ActivationPoint, x.Position) <= arena.ActivationDistance;
                }).ToList();
        }

        public BossFight CreateBossFight(BossArena bossArena, Team team)
        {
            if (ongoingBossFights.Any(x => x.Arena == bossArena))
                return null;

            // Verify that at least one of the attackers is inside the arena
            bool hasTeamMembersInsideArena = teamManager.GetOnlineTeamMembers(team)
                .Select(x => UnturnedPlayer.FromCSteamID((CSteamID)x.Id))
                .Where(x => x != null && !x.Dead)
                .Any(x => Vector3.Distance(bossArena.ActivationPoint, x.Position) <= bossArena.ActivationDistance);

            if (!hasTeamMembersInsideArena)
                return null;

            Type bossController = Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(x => typeof(BossController<>).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
                .FirstOrDefault(x => x.GetGenericTypeDefinition() == bossArena.BossModel.GetType());

            UnturnedChat.Say($"augh {bossController}");
            return null;
        }

        public bool EndBossFight(BossFight bossFight, BossFightState? reason = null)
        {
            if (!ongoingBossFights.Contains(bossFight))
                return false;

            bossFight.FightController.EndFight();
            ongoingBossFights.Remove(bossFight);

            if (reason.HasValue)
                bossFight.State = reason.Value;

            BossFightEventArgs bossFightEventArgs = new BossFightEventArgs(bossFight);
            OnBossFightRemoved?.Invoke(this, bossFightEventArgs);

            if (bossFight.State == BossFightState.BossDefeated)
                OnBossFightCompleted?.Invoke(this, bossFightEventArgs);
            else
                OnBossFightFailed?.Invoke(this, bossFightEventArgs);

            return true;
        }

        private void ProcessFightStartConditions()
        {
            List<BossArena> arenas = dataManager.GameData.Arenas.Values.ToList();
            foreach (UnturnedPlayer player in Provider.clients.Select(x => UnturnedPlayer.FromSteamPlayer(x)))
            {
                // Get the arena which the player stepped into
                BossArena activatedArena = arenas.Where(x => !x.Conquered && !ongoingBossFights.Any(y => y.Arena == x))
                    .FirstOrDefault(x => Vector3.Distance(x.ActivationPoint, player.Position) <= x.ActivationDistance);

                if (activatedArena == null)
                    continue;

                PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
                if (playerData == null || !playerData.TeamId.HasValue)
                    continue;

                Team playerTeam = teamManager.GetTeam(playerData.TeamId.Value);
                CreateBossFight(activatedArena, playerTeam);
            }
        }

        private void ProcessFightEndConditions()
        {
            foreach (BossFight bossFight in ongoingBossFights.Where(x => x.State == BossFightState.Ongoing))
            {
                double deactivationDistance = bossFight.Arena.DeactivationDistance;

                // Find players inside arena
                IEnumerable<UnturnedPlayer> players = Provider.clients
                .Select(x => UnturnedPlayer.FromPlayer(x.player))
                .Where(x =>
                {
                    return Vector3.Distance(bossFight.Arena.ActivationPoint, x.Position) <= deactivationDistance
                     || Vector3.Distance(bossFight.Arena.BossSpawnPoint.Position, x.Position) <= deactivationDistance;
                });

                if (players.Count() != 0 && players.All(x => x.Dead))
                {
                    EndBossFight(bossFight, BossFightState.AttackersDefeated);
                    continue;
                }

                if (players.Count() == 0)
                {
                    EndBossFight(bossFight, BossFightState.Abandoned);
                    continue;
                }
            }
        }

        private void UpdateFights()
        {
            foreach (BossFight bossFight in ongoingBossFights.Where(x => x.State == BossFightState.Ongoing))
            {
                if (!bossFight.FightController.Update())
                {
                    EndBossFight(bossFight, BossFightState.Cancelled);
                    continue;
                }

                if (bossFight.FightController.IsBossDefeated())
                {
                    EndBossFight(bossFight, BossFightState.BossDefeated);
                    continue;
                }
            }
        }

        private void UpdateDominantTeams()
        {
            foreach (BossFight bossFight in ongoingBossFights.Where(x => x.State != BossFightState.Idle))
            {
                double deactivationDistance = bossFight.Arena.DeactivationDistance;

                // Find players inside arena
                Dictionary<int, int> attackerGroups = Provider.clients
                .Select(x => UnturnedPlayer.FromSteamPlayer(x))
                .Where(x =>
                {
                    return Vector3.Distance(bossFight.Arena.ActivationPoint, x.Position) <= deactivationDistance
                     || Vector3.Distance(bossFight.Arena.BossSpawnPoint.Position, x.Position) <= deactivationDistance;
                })
                // Count the number of attackers from each team
                .Select(x => playerDataManager.GetPlayer((ulong)x.CSteamID))
                .Where(x => x != null && x.TeamId.HasValue)
                .GroupBy(x => x.TeamId)
                .ToDictionary(x => x.Key.Value, x => x.Count());

                if (attackerGroups.Count != 0 && attackerGroups.ContainsKey(bossFight.DominantTeam.Id))
                {
                    KeyValuePair<int, int> dominantKvp = attackerGroups.OrderByDescending(x => x.Value).First();
                    KeyValuePair<int, int> currentKvp = attackerGroups.First(x => x.Key == bossFight.DominantTeam.Id);

                    if (dominantKvp.Key != currentKvp.Key && dominantKvp.Value > currentKvp.Value)
                    {
                        Team dominantTeam = teamManager.GetTeam(dominantKvp.Key);
                        Team oldTeam = bossFight.DominantTeam;

                        if (dominantTeam != null)
                        {
                            bossFight.DominantTeam = dominantTeam;
                            OnBossFightDominantTeamChanged?.Invoke(this, new BossFightDominationEventArgs(oldTeam, dominantTeam));
                        }
                    }
                }
            }
        }
    }
}
