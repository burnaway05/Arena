using Mirror;
using MultiFPS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.Gameplay.Gamemodes
{
    public class TeamBasedGamemode : Gamemode
    {
        protected int[] _teamScores = new int[2];
        public int ScoreToWin = 5;
        public float FreezeTime = 5f;

        public delegate void TeamDeathmatch_PlayerKilled(int[] teamScores);
        public TeamDeathmatch_PlayerKilled GamemodeEvent_TeamDeathmatch_PlayerKilled;

        public delegate void TeamGM_PlayerKilled(int[] teamAbundance);
        public TeamGM_PlayerKilled Client_TeamGM_PlayerKilled;


        protected override void OnPlayerAddedToTeam(PlayerInstance playerInstance, int team)
        {
            if (State == GamemodeState.Warmup || State == GamemodeState.WaitingForPlayers)
            {
                OnPlayerKilled(new int[] { GetAliveTeamAbundance(0), GetAliveTeamAbundance(1) });
                playerInstance.Server_SpawnCharacter(GetNextSpawnPoint(_teamSpawnpoints[team]));
            }
        }
        protected override void OnPlayerRemovedFromTeam(PlayerInstance pi, int team)
        {
            base.OnPlayerRemovedFromTeam(pi, team);

            OnPlayerKilled(new int[] { GetAliveTeamAbundance(0), GetAliveTeamAbundance(1) });
        }

        public override void Server_OnPlayerKilled(uint victimID, uint killerID)
        {
            //count score only when game runs, not for example during warmup
            if (State != GamemodeState.Inprogress) return;

            OnPlayerKilled(new int[] { GetAliveTeamAbundance(0), GetAliveTeamAbundance(1) });

            if (RoundState != GamemodeRoundState.InProgress) return; //if round is ended, don't end it another time

            CheckAliveTeamStates();
        }

        protected virtual void CheckAliveTeamStates()
        {
            int winnerTeam = -1;

            if (GetAliveTeamAbundance(0) <= 0)
                winnerTeam = 1;
            if (GetAliveTeamAbundance(1) <= 0)
                winnerTeam = 0;

            if (winnerTeam == -1) return;

            _teamScores[winnerTeam]++;

            RPC_TBG_UpdateTeamScores(_teamScores);

            FinalizeTeamScores(winnerTeam);

            SwitchRoundState(GamemodeRoundState.RoundEnded);
        }

        protected void FinalizeTeamScores(int winnerTeam) 
        {
            if (_teamScores[0] >= ScoreToWin || _teamScores[1] >= ScoreToWin)
            {
                RPC_TBG_MatchEnd(winnerTeam);
                DelaySetGamemodeState(GamemodeState.Warmup, 5f);
            }
            else
            {
                RPC_TBG_RoundEnd(winnerTeam);
            }
        }

        [ClientRpc]
        protected void OnPlayerKilled(int[] teamsAbundance)
        {
            Client_TeamGM_PlayerKilled?.Invoke(teamsAbundance);
        }

        public override void PlayerSpawnCharacterRequest(PlayerInstance playerInstance)
        {
            playerInstance.Server_SpawnCharacter(GetNextSpawnPoint(_teamSpawnpoints[playerInstance.Team]));

            OnPlayerKilled(new int[] { GetAliveTeamAbundance(0), GetAliveTeamAbundance(1) });
        }

        

        protected override void CheckTeamStates()
        {
            if (!isServer) return;

            //start the game if there are players in both teams
            if (_teams[0].PlayerInstances.Count > 0 && _teams[1].PlayerInstances.Count > 0)
            {
                if (State == GamemodeState.WaitingForPlayers)
                    SwitchGamemodeState(GamemodeState.Warmup);

                if (State == GamemodeState.Inprogress)
                {
                    CheckAliveTeamStates();
                }
            }
            else
            {
                SwitchGamemodeState(GamemodeState.WaitingForPlayers);
            }
        }

        protected override int PlayerRequestToJoinTeamPermission(PlayerInstance player, int requestedTeam)
        {
            if (_teams[requestedTeam].PlayerInstances.Count >= _maxTeamSize)
                return -1;

            if (State == GamemodeState.Inprogress && player.Team != -1) //dont let players change team during game, let only new players join team for the first time
                return -2;

            //print("Player: " + player.playerName + " requested joining team: " + requestedTeam + " ,team stats" + _teams[requestedTeam].PlayerInstances.Count + "/" + _maxTeamSize);
            return 0;
        }
        public override void Server_OnPlayerInstanceAdded(PlayerInstance player) { }

        protected override void TimerEnded()
        {
            switch (State)
            {
                case GamemodeState.Warmup:
                    SwitchGamemodeState(GamemodeState.Inprogress);
                    break;
            }

            //round cycle
            if (State != GamemodeState.Inprogress) return;

            switch (RoundState)
            {
                case GamemodeRoundState.FreezeTime:
                    SwitchRoundState(GamemodeRoundState.InProgress);
                    break;
                case GamemodeRoundState.InProgress:
                    RoundEvent_TimerEnded();
                    SwitchRoundState(GamemodeRoundState.RoundEnded);
                    break;
                case GamemodeRoundState.RoundEnded:
                    SwitchRoundState(GamemodeRoundState.FreezeTime);
                    break;
                case GamemodeRoundState.None:
                    SwitchRoundState(GamemodeRoundState.FreezeTime);
                    break;
            }
        }

        public override void SetupGamemode(RoomProperties roomProperties)
        {
            base.SetupGamemode(roomProperties);
            _maxTeamSize = Mathf.FloorToInt(roomProperties.P_MaxPlayers / 2);
        }

        #region match events (server&host only)
        protected override void MatchEvent_StartMatch()
        {
            ResetPlayersStats();
            _teamScores[0] = 0;
            _teamScores[1] = 0;
        }
        protected override void MatchEvent_EndMatch()
        {
            base.MatchEvent_EndMatch();
            StopTimer();

            LetPlayersSpawnOnTheirOwn = false;

            BlockAllPlayers(true);

            if (_teamScores[0] == _teamScores[1])
                GamemodeMessage("Draw", 5f);
            else
                GamemodeMessage((_teamScores[0] > _teamScores[1] ? "Blue" : "Orange") + " team won!", 5f);

            DelaySetGamemodeState(GamemodeState.Warmup, 5f);
        }
        #endregion

        #region round events (server&host only)
        protected override void RoundEvent_Setup()
        {
            base.RoundEvent_Setup();
            GamemodeMessage("Prepare for fight", 3f);
            RespawnAllPlayers(_teamSpawnpoints[0], 0); //respawn all players for team blue
            RespawnAllPlayers(_teamSpawnpoints.Length>1? _teamSpawnpoints[1] : _teamSpawnpoints[0], 1); //respawn all players for team red
            OnPlayerKilled(new int[] { GetAliveTeamAbundance(0), GetAliveTeamAbundance(1) });
            BlockAllPlayers();
            CountTimer(5);
            LetPlayersSpawnOnTheirOwn = true;
        }
        protected override void RoundEvent_Start()
        {
            base.RoundEvent_Start(); 
            BlockAllPlayers(false);
            GamemodeMessage("Round started!", 3f);
            CountTimer(GameDuration);
        }
        protected override void RoundEvent_End()
        {
            base.RoundEvent_End();
            CountTimer(5);
        }
        #endregion

        public override void Relay_NewClientJoined(NetworkConnection conn, NetworkIdentity player)
        {
            base.Relay_NewClientJoined(conn, player);

            TargetRPC_TDM_ClientSetupGamemode(conn, _teamScores);
        }

        //for new clients
        [TargetRpc]
        void TargetRPC_TDM_ClientSetupGamemode(NetworkConnection conn, int[] teamScores)
        {
            _teamScores = teamScores;
            GamemodeEvent_TeamDeathmatch_PlayerKilled?.Invoke(_teamScores);
        }

        [ClientRpc]
        protected void RPC_TBG_UpdateTeamScores(int[] teamScores)
        {
            _teamScores = teamScores;

            GamemodeEvent_TeamDeathmatch_PlayerKilled?.Invoke(_teamScores);
        }

        #region gamemodeSpecific
        [ClientRpc]
        ///TBG = team base gamemode
        protected void RPC_TBG_RoundEnd(int winnerTeam)
        {
            //TODO: Move UI functionality outside of this component
            GameManager.GameEvent_GamemodeEvent_Message?.Invoke((winnerTeam == -1 ? "Draw" : (winnerTeam == ClientFrontend.ThisClientTeam) ? "Round won" : "Round lost"), 5f);
        }

        [ClientRpc]
        void RPC_TBG_MatchEnd(int winnerTeam)
        {
            //TODO: Move UI functionality outside of this component
            GameManager.GameEvent_GamemodeEvent_Message?.Invoke(winnerTeam == ClientFrontend.ThisClientTeam ? "Match won" : "Match lost", 5f);
        }
        #endregion
    }
}