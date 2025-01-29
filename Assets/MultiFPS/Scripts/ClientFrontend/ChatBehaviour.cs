using UnityEngine;
using Mirror;
using System;
using MultiFPS.Gameplay;
using MultiFPS.UI;
using MultiFPS.Gameplay.Gamemodes;

namespace MultiFPS
{
    public class ChatBehaviour : NetworkBehaviour
    {

        private PlayerInstance _playerInstance;
        public static ChatBehaviour _instance { get; private set; }

        public bool ChatWriting { private set; get; } = false;

        private void Start()
        {
            _playerInstance = GetComponent<PlayerInstance>();
        }
        [Command(channel = 0)]
        public void CmdRelayClientMessage(string message)
        {
            RpcHandleChatClientMessage(GameTools.CheckMessageLength(message));
        }
        [ClientRpc]
        public void RpcHandleChatClientMessage(string message)
        {
            Color colorForNickaname = _playerInstance.Team == -1 ? Color.white : ClientInterfaceManager.Instance.UIColorSet.TeamColors[_playerInstance.Team];

            //make message from player nickname and his message
            string newMessage = $" {"<b>" + $"<color=#{ColorUtility.ToHtmlStringRGBA(colorForNickaname)}>" + _playerInstance.PlayerInfo.Username + "</b>" + "</color>" + ": " + message}";

            //write it to UI
            ChatUI._instance.WriteMessageToChat(newMessage);
        }
    }
}