using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;
using MultiFPS.Gameplay;
using UnityEngine.Networking;
using System.Linq;
using MultiFPS.UI;
using Mirror.SimpleWeb;

namespace MultiFPS.Gameplay.Gamemodes {

    /// <summary>
    /// this class is responsible for initializing proper gamemode
    /// </summary>
    public class RoomManager : NetworkBehaviour
    {
        public static RoomManager _instance;

        private void Awake()
        {
            if (_instance) 
            {
                Debug.LogError($"MultiFPS: MORE THAN ONE INSTANCES OF ROOMMANAGER ARE PRESENT IN THIS SCENE: {SceneManager.GetActiveScene().name}" +
                    ", DESTROYING REDUNDANT");

                Destroy(gameObject);
                return;
            }

            _instance = this;
        }
        private void Start()
        {
            if (ServerCommunicator.Instance && ServerCommunicator.ServerInstance)
                ServerCommunicator.Instance.OnGameBooted();

            if (!GameplayCamera._instance)
            {
                Debug.Log("MultiFPS: Gameplay camera not found in the scene, creating one");
                Instantiate(ClientInterfaceManager.Instance.GameplayCamera, transform.position, transform.rotation);
            }

            if (!isServer) return;

            //initialize gamemode selected by client who hosts the game

            //If respawn cooldown is 0 set it to default value
            if (RoomSetup.Properties.P_RespawnCooldown <= 0f)
                RoomSetup.Properties.P_RespawnCooldown = 6f;

            //If respawn cooldown is 0 set it to default value
            if (RoomSetup.Properties.P_GameDuration <= 0f)
                RoomSetup.Properties.P_GameDuration = 9999;

            Gamemode _requestedGamemode = null;
            Gamemode[] _avaibleGamemodes = GetComponents<Gamemode>();

            for (int i = 0; i < _avaibleGamemodes.Length; i++)
            {
                if (_avaibleGamemodes[i].Indicator == RoomSetup.Properties.P_Gamemode) 
                    _requestedGamemode = _avaibleGamemodes[i];
            }

            if (_requestedGamemode != null)
            {
                _requestedGamemode.SetupGamemode(RoomSetup.Properties);
            }
            else
            {
                Debug.LogWarning($"MultiFPS: This map does not support {RoomSetup.Properties.P_Gamemode} gamemode, initializing {_avaibleGamemodes[0].Indicator} instead");
                //if requested gamemode is not supported on this map, than initialize first supported one
                _avaibleGamemodes[0].SetupGamemode(RoomSetup.Properties);
            }
        }

        private void OnDestroy()
        {
            _instance = null;
            GameManager.ClearGameData();
        }
    }
}
