using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using MultiFPS.Gameplay.Gamemodes;

namespace MultiFPS.UI
{
    public class UIPauseMenu : MonoBehaviour
    {
        public static UIPauseMenu _instance;
        public GameObject pauseMenuOverlay;
        public bool AbleToPause = true; //disable only for hub and loading screens
        public bool pauseMenuShowing = false;

        [SerializeField] Button _uiBtnResume;
        [SerializeField] Button _uiBtnDisconnect;
        [SerializeField] Text _gamemodeText;
        private void Awake()
        {
            pauseMenuShowing = true;

            _uiBtnDisconnect.onClick.AddListener(Btn_Disconnect);
            _uiBtnResume.onClick.AddListener(Btn_Resume);

        }
        void Start()
        {
            _instance = this;
            ShowPauseMenu(false);
        }
        void Update()
        {
            //check for input to show pause menu
            if (!(Input.GetKeyDown(KeyCode.Escape) && AbleToPause && !ClientFrontend.Hub)) return;

            if (ChatBehaviour._instance && ChatBehaviour._instance.ChatWriting)
            {
                return;
            }
            ShowPauseMenu(!pauseMenuShowing);
        }
        public void ShowPauseMenu(bool show)
        {
            //if we have closed pause menu, and want to close it again, return from method
            if (pauseMenuShowing == show) return;

            pauseMenuShowing = show; //this method won't always be called by keyboard button, so we have to update this state again

            ClientFrontend.SetPause(show);

            pauseMenuOverlay.SetActive(show);

            ClientFrontend.ShowCursor(show);

            if (show)
                _gamemodeText.text = $"Gamemode: {RoomSetup.Properties.P_Gamemode}";
        }

        #region buttons
        void Btn_Resume() 
        {
            ShowPauseMenu(false);
        }
        void Btn_Disconnect()
        {
            ShowPauseMenu(false);

            // stop host 
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                CustomNetworkManager._instance.StopHost();         
            }
            // stop client if client-only
            else if (NetworkClient.isConnected)
            {
                CustomNetworkManager._instance.StopClient();                
            }
            // stop server if server-only
            else if (NetworkServer.active)
            {                
                CustomNetworkManager._instance.StopServer();              
            }
        }
        #endregion
    }
}