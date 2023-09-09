using SDG.Unturned;
using System;
using System.Text;
using PeopleDieGame.ServerPlugin.Autofac;
using PeopleDieGame.ServerPlugin.Enums;
using PeopleDieGame.ServerPlugin.Helpers;
using UnityEngine;
using System.Collections.Generic;
using PeopleDieGame.ServerPlugin.Models;
using Rocket.Unturned.Player;
using Steamworks;

namespace PeopleDieGame.ServerPlugin.Services.Managers
{
    public class GameManager : IDisposableService
    {
        [InjectDependency]
        private DataManager dataManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }
        [InjectDependency]
        private PlayerDataManager playerDataManager { get; set; }
        [InjectDependency]
        private TimerManager timerManager { get; set; }
        [InjectDependency]
        private AltarManager altarManager { get; set; }

        private float intermissionEndTime = 0;
        private float closingEndTime = 0;
        private Team winnerTeam;

        public event EventHandler OnGameStateChanged;
        public event EventHandler OnGameStarted;
        public event EventHandler OnGameFinished;

        public void Init()
        {
            altarManager.OnAltarSubmitItems += AltarManager_OnAltarSubmitItems;
            timerManager.Register(ProcessState, 60);
        }

        public void Dispose()
        {
            timerManager.Unregister(ProcessState);
        }

        private void AltarManager_OnAltarSubmitItems(object sender, Models.EventArgs.AltarSubmitEventArgs e)
        {
            winnerTeam = e.Team;
            EndGame();
        }

        public GameState GetGameState()
        {
            return dataManager.GameData.State;
        }

        public void SetGameState(GameState state)
        {
            dataManager.GameData.State = state;
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool StartGame()
        {
            if (GetGameState() != GameState.InLobby)
                return false;

            StartStateIntermission();
            return true;
        }

        public bool EndGame()
        {
            switch(GetGameState())
            {
                case GameState.Intermission:
                    SetGameState(GameState.InLobby);
                    return true;
                case GameState.InGame:
                    StartStateGameFinished();
                    return true;
                default:
                    return false;
            }
        }

        public string GetGameSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Witaj jasiu na serverze hentai!");
            sb.AppendLine($"Obecnie mierzy się z sobą {playerDataManager.GetPlayerCount() - 1} innych graczy z {teamManager.GetTeamCount()} drużyn");
            sb.AppendLine($"Na serwerze online jest {Provider.clients.Count} osób");
            sb.AppendLine($"Obecny stan gry: {GameStateHelper.GetFriendlyName(GetGameState())}");

            return sb.ToString();
        }

        private void StartStateIntermission()
        {
            intermissionEndTime = Time.realtimeSinceStartup + dataManager.GameData.IntermissionTime;
            ChatHelper.Say($"Gra rozpocznie się za {dataManager.GameData.IntermissionTime} sekund!");
            SetGameState(GameState.Intermission);
        }

        private void StartStateInGame()
        {
            try
            {
                Team[] teams = teamManager.GetTeams();
                List<PlayerSpawnpoint> spawns = LevelPlayers.getRegSpawns();

                if (spawns.Count < teams.Length)
                    throw new Exception("Not enough free spawn slots.");

                Dictionary<Team, PlayerSpawnpoint> mappings = new Dictionary<Team, PlayerSpawnpoint>();
                foreach(Team team in teams)
                {
                    PlayerSpawnpoint spawn = spawns.RandomOrDefault();
                    spawns.Remove(spawn);
                    mappings.Add(team, spawn);
                }

                // Spawns chosen, teleport players
                foreach (KeyValuePair<Team, PlayerSpawnpoint> kvp in mappings)
                {
                    List<PlayerData> players = teamManager.GetOnlineTeamMembers(kvp.Key);
                    foreach (PlayerData player in players)
                    {
                        UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromCSteamID((CSteamID)player.Id);
                        unturnedPlayer.Teleport(kvp.Value.point + new Vector3(UnityEngine.Random.Range(-10f, 10f), 0, UnityEngine.Random.Range(-10f, 10f)),
                            kvp.Value.angle + UnityEngine.Random.Range(-0.2f, 0.2f));
                    }
                }
            }
            catch(Exception ex)
            {
                ExceptionHelper.Handle(ex, "Nie udało się rozpocząć gry");
                Debug.LogError($"Failed to start game: {ex.Message}");
                SetGameState(GameState.InLobby);
                return;
            }

            SetGameState(GameState.InGame);
            ChatHelper.Say("Zaczynamy zabawę!");
        }

        private void StartStateGameFinished()
        {
            closingEndTime = Time.realtimeSinceStartup + dataManager.GameData.ClosingTime;
            ChatHelper.Say($"Drużyna {winnerTeam.Name} wygrała, gg i guess");
            SetGameState(GameState.Finished);
        }

        private void ProcessIntermission()
        {
            float timeRemaining = intermissionEndTime - Time.realtimeSinceStartup;
            if (timeRemaining < 0)
            {
                StartStateInGame();
                return;
            }

            ChatHelper.Say($"Gra rozpocznie się za {Math.Ceiling(timeRemaining)} sekund!");
        }

        private void ProcessGameFinished()
        {
            float timeRemaining = closingEndTime - Time.realtimeSinceStartup;
            if (timeRemaining < 0)
            {
                SetGameState(GameState.InLobby);
            }
        }

        private void ProcessState()
        {
            switch(GetGameState())
            {
                case GameState.Intermission:
                    ProcessIntermission();
                    break;
                case GameState.Finished:
                    ProcessGameFinished();
                    break;
            }
        }
    }
}
