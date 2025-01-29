
using Mirror;
using MultiFPS.Gameplay.Gamemodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiFPS.UI.Gamemodes
{   
    public class UIDefuse : UITeamBasedGamemode
    {

        [SerializeField] Image _bombPlantedIndicator;

        public override void SetupUI(Gamemode gamemode, NetworkIdentity player)
        {
            base.SetupUI(gamemode, player);
            gamemode.GetComponent<Defuse>().Defuse_OnBombPlanted += OnBombPlanted;
            gamemode.GetComponent<Defuse>().Defuse_OnBombDefused += OnBombDefused;
            

            _bombPlantedIndicator.enabled = false;
        }

        void OnBombPlanted() 
        {
            _bombPlantedIndicator.color = Color.red;
            _bombPlantedIndicator.enabled = true;
        }

        protected override void OnNewRoundStarted() 
        {
            _bombPlantedIndicator.enabled = false;
        }

        void OnBombDefused() 
        {
            _bombPlantedIndicator.color = Color.green;
            _bombPlantedIndicator.enabled = true;
        }
    }
}
