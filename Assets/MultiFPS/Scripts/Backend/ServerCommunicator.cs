using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror.SimpleWeb;
using MultiFPS;
using MultiFPS.Gameplay.Gamemodes;

namespace MultiFPS
{
    public class ServerCommunicator : MonoBehaviour
    {
        public static ServerCommunicator Instance { get; private set; }

        public static bool ServerInstance;

        protected SimpleWebTransport _transport;
        CustomNetworkManager _networkManager;

        MapRepresenter[] Maps;
        int[] TimeOptionsInMinutes = { 2, 5, 10 };
        int[] PlayerNumberOptions = { 2, 4, 6, 8 };

        bool _gameListed;
        protected virtual void Awake()
        {
            if (Instance)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        protected virtual void Start()
        {
            TimeOptionsInMinutes = RoomCreator.Instance.TimeOptionsInMinutes;
            PlayerNumberOptions = RoomCreator.Instance.PlayerNumberOptions;
            Maps = RoomCreator.Instance.Maps;

            _networkManager = CustomNetworkManager._instance;
            _transport = _networkManager.GetComponent<SimpleWebTransport>();

            _networkManager.OnNewPlayerConnected += OnPlayerConnected;
            _networkManager.OnPlayerDisconnected += OnPlayerDisconnected;

            string[] args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                string[] command = args[i].Split(' ');

                if (command[0] == "server" && command.Length > 2)
                {
                    _gameListed = true;
                    ServerInstance = true;
                    _transport.port = (ushort)System.Convert.ToInt32(command[1]);

                    string correctedJson = command[2].Replace("\\", "");

                    Debug.LogError("SETTINGS:" + correctedJson + " END OF SETTINGS");

                    PlayerCreateRoomRequest playerRoomPrefs = WebRequestManager.Deserialize<PlayerCreateRoomRequest>(correctedJson);

                    //aply player preferences
                    _networkManager.onlineScene = Maps[System.Convert.ToInt32(playerRoomPrefs.MapID)].Scene;

                    int mapID = ToInt(playerRoomPrefs.MapID);
                    int gmID = ToInt(playerRoomPrefs.GamemodeForMapID);

                    RoomSetup.Properties.P_Gamemode = (Gamemodes)gmID;
                    RoomSetup.Properties.P_FillEmptySlotsWithBots = System.Convert.ToBoolean(playerRoomPrefs.FillEmptySlotsWithBots);
                    RoomSetup.Properties.P_GameDuration = TimeOptionsInMinutes[ToInt(playerRoomPrefs.GameDurationOptionID)] * 60;

                    //player count
                    int maxPlayers = PlayerNumberOptions[ToInt(playerRoomPrefs.MaxPlayers)];
                    RoomSetup.Properties.P_MaxPlayers = maxPlayers;
                    _networkManager.maxConnections = maxPlayers;

                    RoomSetup.Properties.P_RespawnCooldown = 6f;

                    _networkManager.StartServer();

                    StartCoroutine(Server_CheckIfGameIsEmpty());

                    Debug.Log("UnityServerBuild: Started headless server");

                    WWWForm newRoomProperties = new WWWForm();
                    newRoomProperties.AddField("port", CustomNetworkManager._instance.GetComponent<SimpleWebTransport>().port);
                }

                if (command[0] == "setupWebRequests" && command.Length > 1)
                {
                    WebRequestManager.Instance.SetDomain(command[1]);
                }
            }
        }
        int ToInt(string number)
        {
            return System.Convert.ToInt32(number);
        }

        protected virtual void OnPlayerConnected(NetworkConnectionToClient conn)
        {

        }
        protected virtual void OnPlayerDisconnected(NetworkConnectionToClient conn)
        {
            if (_gameListed)
            {
                if (NetworkServer.connections.Count <= 0)
                {
                    CloseGame();
                }
            }
        }

        public virtual void OnGameBooted()
        {
            WebRequestManager.Instance.Get("/server/multifps/gameBooted", null, null);
        }


        //if game running on server is empty (no players in game) then terminate it
        IEnumerator Server_CheckIfGameIsEmpty()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(30f);

                if (NetworkServer.connections.Count <= 0)
                    CloseGame();
            }
        }

        protected virtual void CloseGame()
        {
            Debug.Log("UnityServerBuild: Terminated empty game on port " + _transport.port);
            Application.Quit();
        }

        [System.Serializable]
        public class PlayerCreateRoomRequest
        {
            public string ServerName;
            public string MapID;
            public string GamemodeForMapID;
            public string MaxPlayers;
            public string GameDurationOptionID;

            public string FillEmptySlotsWithBots;
            public string Port;
        }
    }
}