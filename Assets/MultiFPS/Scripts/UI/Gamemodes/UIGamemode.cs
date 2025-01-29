using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using MultiFPS.Gameplay;
using MultiFPS.Gameplay.Gamemodes;

namespace MultiFPS.UI.Gamemodes
{
    public class UIGamemode : MonoBehaviour
    {
        [SerializeField] protected UIGamemodeTimer _timer;

        protected virtual void Awake() { }
        public virtual void SetupUI(Gamemode gamemode, NetworkIdentity player)
        {

            ClientFrontend.GamemodeUI = this;

            gamemode.GamemodeEvent_Timer += _timer.UpdateTimer;
            gamemode.GamemodeEvent_OnNewRoundSetup += OnNewRoundStarted;

            ClientFrontend.ClientPlayerInstance = player.GetComponent<PlayerInstance>();

            ClientFrontend.ClientPlayerInstance.PlayerEvent_OnReceivedTeamResponse += OnReceivedTeamResponse;

            ClientFrontend.GameEvent_OnObservedCharacterSet += OnObservedCharacterSet;

        }

        protected virtual void OnObservedCharacterSet(CharacterInstance characterInstance) 
        {

        }
        protected virtual void OnReceivedTeamResponse(int team, int permissionCode)
        {
        }
        public virtual void Btn_ShowTeamSelector()
        {
        }

        private void OnDestroy()
        {
            ClientFrontend.ClientPlayerInstance.PlayerEvent_OnReceivedTeamResponse -= OnReceivedTeamResponse;

            ClientFrontend.GameEvent_OnObservedCharacterSet -= OnObservedCharacterSet;
        }

        #region round event listeners
        protected virtual void OnNewRoundStarted()
        {
            
        }
        #endregion

    }
}