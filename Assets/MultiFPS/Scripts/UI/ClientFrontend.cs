using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiFPS.Gameplay.Gamemodes;
using MultiFPS.Gameplay;
using MultiFPS.UI.Gamemodes;

namespace MultiFPS.UI
{
    public static class ClientFrontend
    {

        public static bool Pause { private set; get; } = false;

        public static bool Hub { set; get; } = true;

        //this will be called from server when we receive all the neccesary info about game properties like gamemode
        public delegate void OnPlayerJoined(Gamemode gamemode, NetworkIdentity player);
        public static OnPlayerJoined ClientEvent_OnJoinedToGame { get; set; }


        public static UIGamemode GamemodeUI;

        public static PlayerInstance ClientPlayerInstance;

        public static CharacterInstance _observedCharacterInstance;
        public static CharacterInstance OwnedCharacterInstance { private set; get; }

        static int cursorRequests = 0;


        #region team managament
        public delegate void OnAssignedToTeam(int team);
        public static OnAssignedToTeam ClientEvent_OnAssignedToTeam { get; set; }

        public static int ThisClientTeam { private set; get; } = -1;
        public static bool ClientTeamAssigned;
        #endregion

        public static void ShowCursor(bool show)
        {
            cursorRequests = show ? cursorRequests + 1 : cursorRequests - 1;

            if (cursorRequests < 0) cursorRequests = 0;

            Cursor.visible = cursorRequests != 0;

            if (cursorRequests != 0)
                Cursor.lockState = CursorLockMode.Confined;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }

        public static bool GamePlayInput()
        {
            return cursorRequests == 0;
        }

        public static void SetPause(bool pause)
        {
            Pause = pause;
        }


        public delegate void OnObservedCharacterSet(CharacterInstance characterInstance);
        public static OnObservedCharacterSet GameEvent_OnObservedCharacterSet { get; set; }
        public static void SetObservedCharacter(CharacterInstance characterInstance)
        {
            //if (_observedCharacterInstance == characterInstance) return; //dont set same character twice as observed

            if (_observedCharacterInstance)
            {
                _observedCharacterInstance.IsObserved = false;
                //_observedCharacterInstance.SetFppPerspective(false);
            }

            _observedCharacterInstance = characterInstance;

            SetClientTeam(_observedCharacterInstance.Team);

            _observedCharacterInstance.IsObserved = true;
            //_observedCharacterInstance.SetFppPerspective(true);

            GameEvent_OnObservedCharacterSet?.Invoke(_observedCharacterInstance);

        }
        public static uint ObservedCharacterNetID()
        {
            //if (_observedCharacterInstance)
            //{
            //    return _observedCharacterInstance.netId;
            //}
            //else
                return uint.MaxValue;
        }

        public static void SetOwnedCharacter(CharacterInstance characterInstance) 
        {
            OwnedCharacterInstance = characterInstance;
        }

        public static void SetClientTeam(int team) 
        {
            ClientTeamAssigned = team != -1;
            ThisClientTeam = team;
            ClientEvent_OnAssignedToTeam?.Invoke(team);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeClientFrontEnd()
        {
            //ShowCursor(false);
        }
    }
}