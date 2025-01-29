using Mirror;
using MultiFPS.Gameplay.Gamemodes;
using UnityEngine;
using UnityEngine.UI;
namespace MultiFPS.UI.Gamemodes
{
    public class UIDeathmatch : UIGamemode
    {
        public override void SetupUI(Gamemode gamemode, NetworkIdentity player)
        {
            base.SetupUI(gamemode, player);

            Deathmatch dm = gamemode.GetComponent<Deathmatch>();
        }

        public void OnPlayerKilled(int blueScore, int orangeScore)
        {
            
        }
    }
}