using System.Collections.Generic;
using UnityEngine;
using MultiFPS.Gameplay;
using MultiFPS.Gameplay.Gamemodes;

namespace MultiFPS
{
    public static class GameManager
    {
      //  public static PlayerInstance myPlayerInstance { set; get; }
        public static Dictionary<uint, PlayerInstance> Players = new Dictionary<uint, PlayerInstance>();
        public static Dictionary<uint, Health> HealthInstances = new Dictionary<uint, Health>();

        public static Gamemode Gamemode { get; private set; }
        public delegate void OnGamemodeSet(Gamemode gamemode);
        public static OnGamemodeSet GameEvent_OnGamemodeSet { set; get; }

        
        public static void SetGamemode(Gamemode gamemodeToSet)
        {
            Gamemode = gamemodeToSet;
            GameEvent_OnGamemodeSet?.Invoke(Gamemode);
        }
        public static void AddPlayerInstance(PlayerInstance pi)
        {
            if (!Players.ContainsKey(pi.netId))
            {
                Players.Add(pi.netId, pi);
            }
        }
        public static void RemovePlayerInstance(PlayerInstance pi)
        {
            if (Players.ContainsKey(pi.netId))
            {
                Players.Remove(pi.netId);
            }
        }

        public static void AddHealthInstance(Health _h)
        {
            //if (!HealthInstances.ContainsKey(_h.netId))
            //{
            //    HealthInstances.Add(_h.netId, _h);
            //}
        }
        public static void RemoveHealthInstance(Health _h)
        {
            //if (HealthInstances.ContainsKey(_h.netId))
            //{
            //    HealthInstances.Remove(_h.netId);
            //}
        }
        public static Health GetHealthInstance(uint id)
        {
            if (HealthInstances.ContainsKey(id))
                return HealthInstances[id];
            else return null;
        }
        public static void ClearGameData()
        {
            Players.Clear();
            HealthInstances.Clear();


        }

        public static void SetLayerRecursively(GameObject go, int layerNumber)
        {
            go.layer = layerNumber;
            foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = layerNumber;
            }
        }

        #region constants
        /// <summary>
        /// layer for guns hitscan
        /// </summary>
        public const int fireLayer = (1 << 0 | 1 << (int)GameLayers.hitbox | 1 << (int)GameLayers.noBulletProof | 1 << (int)GameLayers.throwables);
        public const int characterLayer = (1 << 6 | 1 << 0);
        public const int environmentLayer = (1 << 0 | 1 << (int)GameLayers.noBulletProof);
        public const int rigidbodyLayer = (1 << 9 | 1 << 7);
        public const int interactLayerMask = (1 << 0 | 1 << 7);

        /// <summary>
        /// Amount of time that item can exist after being dropped
        /// </summary>
        public const float TimeOfLivingLonelyItem = 15f;



        #endregion

        public delegate void OnCharacterTeamAssigned(CharacterInstance characterInstance);
        public static OnCharacterTeamAssigned GameEvent_CharacterTeamAssigned;

        public static void CharacterTeamAssigned(CharacterInstance characterInstance) 
        {
            GameEvent_CharacterTeamAssigned?.Invoke(characterInstance);
        }

        /// <summary>
        /// Passing messages to event so UI can read from it
        /// </summary>
        public delegate void GamemodeMsg(string msg, float liveTime);
        public static GamemodeMsg GameEvent_GamemodeEvent_Message;

        public static PlayerInstance FindPlayerInstanceByCharacter(CharacterInstance charInstance)
        {

            List<PlayerInstance> players = new List<PlayerInstance>(GameManager.Players.Values);

            for (int i = 0; i < players.Count; i++)
            {
                PlayerInstance pi = players[i];
                if (pi.MyCharacter && pi.MyCharacter == charInstance) return pi;
            }

            return null;
        }
    }

    /// <summary>
    /// game layers with respective indexes, first 6 layers are built in and cannot be changed, so we start from index 6
    /// </summary>
    public enum GameLayers
    {
        character = 6, 
        item = 7,
        hitbox = 8,
        ragdoll = 9,
        throwables = 10,
        trigger = 11,
        launchedThrowables = 12,
        fppModels = 13, //objects with this layer will be rendered on top of everything. It is for FPP models to not clip through walls
        noBulletProof = 14, //If you want certain object to be able to be penetrated by guns, change their layer to this one
    }
    public enum GameTags
    {
        Flesh,
        Wood,
    }


    public enum AttackType : byte
    {
        hitscan,
        hitscanPenetrated,
        melee,
        explosion,
        falldamage,
    }
}