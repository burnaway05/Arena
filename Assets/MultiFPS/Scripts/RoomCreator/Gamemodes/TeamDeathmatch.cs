using Mirror;
using UnityEngine;

namespace MultiFPS.Gameplay.Gamemodes
{
    [AddComponentMenu("MultiFPS/Gamemodes/TeamDeathmatch")]
    public class TeamDeathmatch : TeamBasedGamemode
    {
        int _orangeScore;
        int _blueScore;



        //set values inherited from Gamemodeclass appropriately for this gamemode
        public TeamDeathmatch()
        {
            Indicator = Gamemodes.TeamDeathmatch;
            LetPlayersSpawnOnTheirOwn = true; //it means that player will be spawned by cooldown counter from moment of his death
                                              //instead of game event
            FriendyFire = false;
        }

        public override void SetupGamemode(RoomProperties roomProperties)
        {
            base.SetupGamemode(roomProperties);
            _maxTeamSize = Mathf.FloorToInt(roomProperties.P_MaxPlayers / 2);
            LetPlayersSpawnOnTheirOwn = true;
        }

        public override void Relay_NewClientJoined(NetworkConnection conn, NetworkIdentity player)
        {
            base.Relay_NewClientJoined(conn, player);

            TargetRPC_TDM_ClientSetupGamemode(conn, _blueScore, _orangeScore);
        }

        //for new clients
        [TargetRpc]
        void TargetRPC_TDM_ClientSetupGamemode(NetworkConnection conn, int blueScore, int orangeScore)
        {
            _orangeScore = orangeScore;
            _blueScore = blueScore;
            GamemodeEvent_TeamDeathmatch_PlayerKilled?.Invoke(new int[] { _blueScore, _orangeScore });
        }

        public override void PlayerSpawnCharacterRequest(PlayerInstance playerInstance)
        {
            if (State == GamemodeState.Inprogress)
                playerInstance.Server_SpawnCharacter(GetBestSpawnPoint(defaultSpawnPoints.Spawnpoints.ToArray(), playerInstance.Team));
            else
                playerInstance.Server_SpawnCharacter(GetNextSpawnPoint(defaultSpawnPoints));
        }

        protected override void OnPlayerAddedToTeam(PlayerInstance playerInstance, int team)
        {

            if (State == GamemodeState.Inprogress)
                playerInstance.Server_SpawnCharacter(GetBestSpawnPoint(defaultSpawnPoints.Spawnpoints.ToArray(), playerInstance.Team));
            else
                playerInstance.Server_SpawnCharacter(GetNextSpawnPoint(defaultSpawnPoints));
        }

        public override void Server_OnPlayerKilled(uint victimID, uint killerID)
        {
            base.Server_OnPlayerKilled(victimID, killerID);

            //count score only when game runs, not for example during warmup
            if (State == GamemodeState.Inprogress)
            {

                if (GameManager.GetHealthInstance(victimID).Team == 1)
                    _blueScore += 100;
                else
                    _orangeScore += 100;

                TDM_UpdateGamemodeState(_blueScore, _orangeScore);

                if (_blueScore >= ScoreToWin || _orangeScore >= ScoreToWin)
                    SwitchGamemodeState(GamemodeState.Finish);
            }
        }
        protected override void CheckAliveTeamStates()
        {
            
        }


        [ClientRpc]
        void TDM_UpdateGamemodeState(int blueScore, int orangeScore)
        {
            _orangeScore = orangeScore;
            _blueScore = blueScore;

            GamemodeEvent_TeamDeathmatch_PlayerKilled?.Invoke(new int[] { _blueScore, _orangeScore });
        }

        protected override void TimerEnded()
        {
            switch (State)
            {
                case GamemodeState.Warmup:
                    SwitchGamemodeState(GamemodeState.Inprogress);
                    break;

                case GamemodeState.Inprogress:
                    SwitchGamemodeState(GamemodeState.Finish); //TODO: fix
                    break;
            }
        }


        protected override void MatchEvent_StartMatch()
        {
            base.MatchEvent_StartMatch();

            RespawnAllPlayers(_teamSpawnpoints[0], 0);
            RespawnAllPlayers(_teamSpawnpoints[1], 1);

            GamemodeMessage("Match started!", 3f);
            CountTimer(GameDuration);

            ResetPlayersStats();


            _blueScore = 0;
            _orangeScore = 0;

            TDM_UpdateGamemodeState(0, 0);


            LetPlayersSpawnOnTheirOwn = true;
        }
        protected override void MatchEvent_EndMatch()
        {
            base.MatchEvent_EndMatch();
            StopTimer();

            LetPlayersSpawnOnTheirOwn = false;

            if (_blueScore == _orangeScore)
                GamemodeMessage("Draw", 5f);
            else
                GamemodeMessage((_blueScore > _orangeScore ? "Blue" : "Orange") + " team won!", 5f);

            DelaySetGamemodeState(GamemodeState.Warmup, 5f);
        }

        protected override void CheckTeamStates()
        {
            if (!isServer) return;

            //start the game if there are players in both teams
            if (_teams[0].PlayerInstances.Count > 0 && _teams[1].PlayerInstances.Count > 0)
            {
                if (State == GamemodeState.WaitingForPlayers)
                    SwitchGamemodeState(GamemodeState.Warmup);
            }
            else
            {
                SwitchGamemodeState(GamemodeState.WaitingForPlayers);
            }
        }

        public override void Server_OnPlayerInstanceAdded(PlayerInstance player)
        {

        }


        protected override int PlayerRequestToJoinTeamPermission(PlayerInstance player, int requestedTeam) 
        {
            if (_teams[requestedTeam].PlayerInstances.Count >= _maxTeamSize)
                return -1;

            if (State == GamemodeState.Inprogress && player.Team != -1) //dont let players change team during game, let only new players join team for the first time
                return -2;

            return 0;
        }
    }
}