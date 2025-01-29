using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MultiFPS.Gameplay.Gamemodes;
using Mirror.SimpleWeb;
using System.Collections;

namespace MultiFPS
{
    /// <summary>
    /// user interface class for user to be able to specify game parameters like map, gamemode
    /// max players and game duration
    /// </summary>
    public class RoomCreator : MonoBehaviour
    {
        public static RoomCreator Instance;
        CustomNetworkManager _networkManager;
        public Dropdown MapselectionDropdown;
        public Dropdown GamemodeSelectionDropdown;
        public Dropdown GameDurationDropdown;
        public Dropdown PlayerNumberDropdown;

        public InputField PortIF;
        public InputField RoomNameIF;
        public Button HostGameButton;
        public Button ServeGameButton;
        public Toggle BotsToggle;

        //user input
        int _selectedMapID;
        int _selectedTimeDurationID;
        int _selectedPlayerNumberOptionID;
        Gamemodes _selectedGamemode;

        [Header("Options for player to choose from")]
        public MapRepresenter[] Maps;
        public int[] TimeOptionsInMinutes = { 2, 5, 10 };
        public int[] PlayerNumberOptions = { 2, 4, 6, 8 };

        [Header("Optional")]
        [SerializeField] GameObject _loadingScreen;
        [SerializeField] Text _loadingScreenMessage;

        Coroutine _loadingScreenCoroutine;

        private void Awake()
        {
            Instance = this;

            if (_loadingScreen)
                _loadingScreen.gameObject.SetActive(false);
        }

        void Start()
        {
            _networkManager = CustomNetworkManager._instance;
            RoomSetup.Properties.P_Gamemode = Gamemodes.None;

            List<string> mapOptions = new List<string>();

            for (int i = 0; i < Maps.Length; i++)
            {
                mapOptions.Add(Maps[i].Name);
            }

            MapselectionDropdown.ClearOptions();
            MapselectionDropdown.AddOptions(mapOptions);

            MapselectionDropdown.onValueChanged.AddListener(OnMapselected);
            GamemodeSelectionDropdown.onValueChanged.AddListener(OnGamemodeSelected);
            GameDurationDropdown.onValueChanged.AddListener(OnGameDurationSelected);
            PlayerNumberDropdown.onValueChanged.AddListener(OnPlayerNumberOption);

            //game duration options
            List<string> durationOptions = new List<string>();

            for (int i = 0; i < TimeOptionsInMinutes.Length; i++)
            {
                durationOptions.Add(TimeOptionsInMinutes[i].ToString() + " minutes");
            }

            GameDurationDropdown.AddOptions(durationOptions);

            List<string> playerNumberOptions = new List<string>();
            //player number options
            for (int i = 0; i < PlayerNumberOptions.Length; i++)
            {
                playerNumberOptions.Add(PlayerNumberOptions[i].ToString() + " players");
            }
            PlayerNumberDropdown.ClearOptions();
            PlayerNumberDropdown.AddOptions(playerNumberOptions);

            if(HostGameButton) HostGameButton.onClick.AddListener(HostGame);
            if(ServeGameButton) ServeGameButton.onClick.AddListener(ServeGame);

            OnMapselected(0);
        }
        void OnMapselected(int mapID)
        {
            _selectedMapID = mapID;
            OnGamemodeSelected(0);

            //fill gamemodes dropdown with options avaible for given map
            Gamemodes[] avaibleGamemodesForThisMap = Maps[mapID].AvailableGamemodes;

            List<string> gamemodeOptions = new List<string>();

            for (int i = 0; i < avaibleGamemodesForThisMap.Length; i++)
            {
                gamemodeOptions.Add(avaibleGamemodesForThisMap[i].ToString());
            }

            GamemodeSelectionDropdown.ClearOptions();
            GamemodeSelectionDropdown.AddOptions(gamemodeOptions);
        }

        /// <summary>
        /// trigged by selecting gamemode in UI room creator, tells game which gamemode to setup
        /// </summary>
        /// <param name="gamemodeID"></param>
        void OnGamemodeSelected(int gamemodeID) //gamemode ID is relevant to gamemodes order in their enum
        {
            _selectedGamemode = Maps[_selectedMapID].AvailableGamemodes != null && Maps[_selectedMapID].AvailableGamemodes.Length > 0 ? Maps[_selectedMapID].AvailableGamemodes[gamemodeID] : Gamemodes.None;
        }
        void OnGameDurationSelected(int timeOptionID)
        {
            _selectedTimeDurationID = timeOptionID;
        }
        void OnPlayerNumberOption(int playerOptionID)
        {
            _selectedPlayerNumberOptionID = playerOptionID;
        }

        //write parameters and start game as host
        void HostGame()
        {
            _networkManager.onlineScene = Maps[_selectedMapID].Scene;

            RoomSetup.Properties.P_Gamemode = _selectedGamemode;
            RoomSetup.Properties.P_FillEmptySlotsWithBots = BotsToggle.isOn;
            RoomSetup.Properties.P_GameDuration = TimeOptionsInMinutes[_selectedTimeDurationID] * 60;
            RoomSetup.Properties.P_RespawnCooldown = 6f;

            //player count
            int maxPlayers = PlayerNumberOptions[_selectedPlayerNumberOptionID];
            RoomSetup.Properties.P_MaxPlayers = maxPlayers; //for gamemode

            _networkManager.maxConnections = maxPlayers; //for handling connections
            _networkManager.StartHost();
        }

        //send parameters to dedicated server so it can start desired game, and connect us to it
        void ServeGame()
        {
            if (!WebRequestManager.Instance) 
            {
                Debug.LogWarning("Server communicator is not present in the scene!");
                return;
            }

            ShowLoadingScreen("Creating game...", 15f);

            WWWForm gameProperties = new WWWForm();
            gameProperties.AddField("ServerName", RoomNameIF? RoomNameIF.text:"server");
            gameProperties.AddField("MapID", _selectedMapID);
            gameProperties.AddField("GamemodeForMapID", (int)_selectedGamemode);
            gameProperties.AddField("MaxPlayers", _selectedPlayerNumberOptionID);
            gameProperties.AddField("GameDurationOptionID", _selectedTimeDurationID);
            gameProperties.AddField("FillEmptySlotsWithBots", BotsToggle.isOn.ToString());
            gameProperties.AddField("Port", PortIF? PortIF.text: "7777");

            WebRequestManager.Instance.Post("/client/multifps/createGame", gameProperties, OnGameServed, OnFail);

            void OnGameServed(string data, int code)
            {
                ShowLoadingScreen("Game created! Connecting...");

                PlayerConnectToRoomRequest connectInfo = WebRequestManager.Deserialize<PlayerConnectToRoomRequest>(data);
                _networkManager.GetComponent<SimpleWebTransport>().port = (ushort)System.Convert.ToInt32(connectInfo.Port);
                _networkManager.networkAddress = connectInfo.Address;
                _networkManager.StartClient();
            }
            void OnFail(string data, int code)
            {
                ShowLoadingScreen("Could not get response from server", 3f);
                print("Could not get response from server");
            }

            #region loading screen
            void ShowLoadingScreen(string message, float liveTime = 10f) 
            {
                if (_loadingScreenCoroutine != null)
                    StopCoroutine(_loadingScreenCoroutine);

                StartCoroutine(LoadingScreenCoroutine(message, liveTime));
            }
            IEnumerator LoadingScreenCoroutine(string message, float liveTime)
            {
                if (!_loadingScreen || !_loadingScreenMessage) yield break;

                _loadingScreen.SetActive(true);
                _loadingScreenMessage.text = message;

                yield return new WaitForSeconds(liveTime);

                _loadingScreen.SetActive(false);
                _loadingScreenMessage.text = string.Empty;
            }
            #endregion
        }


        [System.Serializable]
        public class PlayerConnectToRoomRequest
        {
            public string Address;
            public string Port;
        }
    }
}