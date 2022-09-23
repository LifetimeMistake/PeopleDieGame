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
using UnturnedGameMaster.BossControllers;
using UnturnedGameMaster.Enums;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.EventArgs;
using UnturnedGameMaster.Services.Providers;

namespace UnturnedGameMaster.Services.Managers
{
    public class ArenaManager : IDisposableService
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
        [InjectDependency]
        private ZombiePoolManager zombiePoolManager { get; set; }
        [InjectDependency]
        private GameManager gameManager { get; set; }
        [InjectDependency]
        private LoadoutManager loadoutManager { get; set; }

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
            loadoutManager.OnLoadoutRemoved += LoadoutManager_OnLoadoutRemoved;
            gameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
            if (gameManager.GetGameState() == GameState.InGame)
                RegisterTimers();
        }

        public void Dispose()
        {
            loadoutManager.OnLoadoutRemoved -= LoadoutManager_OnLoadoutRemoved;
            gameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
            UnregisterTimers();
        }

        private void LoadoutManager_OnLoadoutRemoved(object sender, LoadoutEventArgs e)
        {
            foreach(BossArena arena in GetArenas())
            {
                if (arena.RewardLoadoutId == e.Loadout.Id)
                    arena.SetRewardLoadout(null);
            }
        }

        private void RegisterTimers()
        {
            UnregisterTimers();
            timerManager.Register(ProcessFightStartConditions, 60);
            timerManager.Register(ProcessFightEndConditions, 60);
            timerManager.Register(UpdateDominantTeams, 60);
            timerManager.Register(UpdateFights, 1);
        }

        private void UnregisterTimers()
        {
            timerManager.Unregister(ProcessFightStartConditions);
            timerManager.Unregister(ProcessFightEndConditions);
            timerManager.Unregister(UpdateFights);
            timerManager.Unregister(UpdateDominantTeams);
        }

        private void GameManager_OnGameStateChanged(object sender, EventArgs e)
        {
            GameState gameState = gameManager.GetGameState();
            if (gameState == GameState.InGame)
            {
                Debug.Log("Started processing arena events");
                RegisterTimers();
            }
            else
            {
                Debug.Log("Stopped processing arena events");
                UnregisterTimers();
            }
        }

        public BossArena CreateArena(ArenaBuilder arenaBuilder)
        {
            if (arenaBuilder == null)
                throw new ArgumentNullException(nameof(arenaBuilder));

            int arenaId = arenaIdProvider.GenerateId();
            BossArena arena = arenaBuilder.ToArena(arenaId);

            if (!zombiePoolManager.ZombiePoolExists(arena.BoundId))
            {
                if (!zombiePoolManager.CreateZombiePool(arena.BoundId, arenaBuilder.ZombiePoolSize))
                    return null;
            }
            else
            {
                if (!zombiePoolManager.ResizeZombiePool(arena.BoundId, arenaBuilder.ZombiePoolSize, true))
                    return null;
            }

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
            foreach (BossFight bossFight in ongoingBossFights.Where(x => x.Arena.Id == id).ToList())
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

        public BossArena[] GetArenas()
        {
            return dataManager.GameData.Arenas.Values.ToArray();
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

        public List<UnturnedPlayer> GetPlayersInsideArena(BossArena arena, Team team = null)
        {
            return Provider.clients
                .Select(x => UnturnedPlayer.FromPlayer(x.player))
                .Where(x => team == null || playerDataManager.GetPlayer((ulong)x.CSteamID).TeamId == team.Id)
                .Where(x =>
                {
                    return Vector3.Distance(arena.ActivationPoint, x.Position) <= arena.DeactivationDistance
                     || Vector3.Distance(arena.BossSpawnPoint.Position, x.Position) <= arena.DeactivationDistance;
                }).ToList();
        }

        public List<UnturnedPlayer> GetPlayersInActivationRange(BossArena arena, Team team = null)
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

            if (!zombiePoolManager.ZombiePoolExists(bossArena.BoundId))
            {
                Debug.LogWarning("Could not activate arena, zombie pool does not exist.");
                return null;
            }

            // Verify that at least one of the attackers is inside the arena
            bool hasTeamMembersInsideArena = teamManager.GetOnlineTeamMembers(team)
                .Select(x => UnturnedPlayer.FromCSteamID((CSteamID)x.Id))
                .Where(x => x != null && !x.Dead)
                .Any(x => Vector3.Distance(bossArena.ActivationPoint, x.Position) <= bossArena.ActivationDistance);

            if (!hasTeamMembersInsideArena)
            {
                Debug.LogWarning("Attempted to activate arena, but no attacker team members were inside.");
                return null;
            }

            Type bossControllerType = Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(x => x.BaseType != null && x.BaseType.IsGenericType && x.BaseType.GetGenericTypeDefinition() == typeof(BossController<>) && x.IsClass && !x.IsAbstract)
                .FirstOrDefault(x => x.BaseType.GetGenericArguments().FirstOrDefault() == bossArena.BossModel.GetType());

            if (bossControllerType == null)
            {
                Debug.LogWarning($"No matching boss controller found for boss {bossArena.BossModel.GetType().Name} ({bossArena.BossModel.Name})");
                return null;
            }

            BossFight bossFight = new BossFight(bossArena, null, team, BossFightState.Idle);
            IBossController bossController;

            try
            {
                bossController = (IBossController)Activator.CreateInstance(bossControllerType, bossFight);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create boss controller: {ex}");
                return null;
            }

            bossFight.FightController = bossController;
            if (!bossController.StartFight())
            {
                Debug.LogWarning("Boss controller failed to initialize the fight");
                return null;
            }

            bossFight.State = BossFightState.Ongoing;

            ongoingBossFights.Add(bossFight);
            OnBossFightCreated?.Invoke(this, new BossFightEventArgs(bossFight));
            return bossFight;
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

        public List<BossFight> GetBossFights()
        {
            return ongoingBossFights;
        }

        private void ProcessFightStartConditions()
        {
            List<BossArena> arenas = dataManager.GameData.Arenas.Values.ToList();
            foreach (UnturnedPlayer player in Provider.clients.Select(x => UnturnedPlayer.FromSteamPlayer(x)).ToList())
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
                BossFight bossFight = CreateBossFight(activatedArena, playerTeam);
                if (bossFight == null)
                    Debug.LogError($"Failed to start arena {activatedArena.Name}");
                else
                    Debug.Log($"Started arena {bossFight.Arena.Name}");
            }
        }

        private void ProcessFightEndConditions()
        {
            foreach (BossFight bossFight in ongoingBossFights.Where(x => x.State == BossFightState.Ongoing).ToList())
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
            foreach (BossFight bossFight in ongoingBossFights.Where(x => x.State == BossFightState.Ongoing).ToList())
            {
                if (!bossFight.FightController.Update())
                {
                    EndBossFight(bossFight, BossFightState.Cancelled);
                    continue;
                }

                if (bossFight.FightController.IsBossDefeated())
                {
                    EndBossFight(bossFight, BossFightState.BossDefeated);
                    bossFight.Arena.Conquered = true;
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
                IEnumerable<UnturnedPlayer> participants = Provider.clients
                .Select(x => UnturnedPlayer.FromSteamPlayer(x))
                .Where(x =>
                {
                    return Vector3.Distance(bossFight.Arena.ActivationPoint, x.Position) <= deactivationDistance
                     || Vector3.Distance(bossFight.Arena.BossSpawnPoint.Position, x.Position) <= deactivationDistance;
                });

                bossFight.Participants.Clear();
                bossFight.Participants.AddRange(participants);

                Dictionary<int, int> attackerGroups = participants
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
