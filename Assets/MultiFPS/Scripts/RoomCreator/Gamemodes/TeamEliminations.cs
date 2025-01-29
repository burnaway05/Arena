using MultiFPS.Gameplay.Gamemodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MultiFPS.Gameplay;
using MultiFPS;
using MultiFPS.UI;

namespace MultiFPS.Gameplay.Gamemodes
{
    [AddComponentMenu("MultiFPS/Gamemodes/TeamEliminations")]
    public class TeamEliminations : TeamBasedGamemode
    {
        public TeamEliminations()
        {
            Indicator = Gamemodes.TeamEliminations;
            LetPlayersSpawnOnTheirOwn = false; //it means that player will be spawned by cooldown counter from moment of his death
                                               //instead of game event
            FriendyFire = false;
            LetPlayersTakeControlOverBots = true;
        }

        public override void SetupGamemode(RoomProperties roomProperties)
        {
            base.SetupGamemode(roomProperties);
            LetPlayersSpawnOnTheirOwn = false;

        }
        protected override void RoundEvent_Start()
        {
            base.RoundEvent_Start();
            LetPlayersSpawnOnTheirOwn = false;
        }
        protected override void MatchEvent_StartMatch()
        {
            base.MatchEvent_StartMatch();
            LetPlayersSpawnOnTheirOwn = false;
            RPC_TBG_UpdateTeamScores(_teamScores);
        }
    }
}