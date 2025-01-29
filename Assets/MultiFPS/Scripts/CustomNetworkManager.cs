using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;
using Mirror.SimpleWeb;
using System.Text.RegularExpressions;

namespace MultiFPS
{
    public class CustomNetworkManager : NetworkManager
    {
        public static CustomNetworkManager _instance;

        //this event exist to send late players data about gamemode and equipment of other players.
        public delegate void NewPlayerJoinedTheGame(NetworkConnectionToClient conn);
        public NewPlayerJoinedTheGame OnNewPlayerConnected { get; set; }

        public delegate void PlayerDisconnected(NetworkConnectionToClient conn);
        public PlayerDisconnected OnPlayerDisconnected { get; set; }

        //Toggle Mirror's GUI for hosting game and connecting 
        bool _guiSet;

        public override void Awake()
        {
            base.Awake();
            Debug.Log("MultiFPS | desNetware.net | discord.gg/REnFR3wXNY");

            if (_instance)
            {
                Debug.LogError("Fatal error, two instances of Custom Network Manager");
                Destroy(_instance.gameObject);
            }

            _instance = this;
        }


        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            OnNewPlayerConnected?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            OnPlayerDisconnected?.Invoke(conn);
            base.OnServerDisconnect(conn);
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _guiSet = !_guiSet;

                GetComponent<NetworkManagerHUD>().enabled = _guiSet;
                Cursor.visible = _guiSet;
                if (_guiSet)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        public void ConnectToTheGame()
        {
            StartClient();
        }

        public void SetAddressAndPort(string address, string port)
        {
            if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(port)) return;

            networkAddress = address;

            ushort uport = (ushort)System.Convert.ToInt32(port);

            KcpTransport kcpTransport = GetComponent<KcpTransport>();

            if (kcpTransport == transport)
            {
                kcpTransport.Port = uport;
                return;
            }

            SimpleWebTransport simpleWebTransport = GetComponent<SimpleWebTransport>();

            if (simpleWebTransport == transport) 
            {
                simpleWebTransport.port = uport;
                return;
            }
        }
    }

}