using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.Gameplay.Gamemodes {
    public class SpawnpointsContainer : MonoBehaviour
    {
        public SpawnpointsContainerType MySpawnpointsContainerType;
        [HideInInspector] public int _lastUsedSpawnpointID;
        public List<Transform> Spawnpoints;

        private void Awake()
        {
            switch (MySpawnpointsContainerType)
            {
                case SpawnpointsContainerType.Default:
                    Gamemode.defaultSpawnPoints = this;
                    break;
                case SpawnpointsContainerType.TeamA:
                    Gamemode._teamSpawnpoints.SetValue(this, 0);
                    break;
                case SpawnpointsContainerType.TeamB:
                    Gamemode._teamSpawnpoints.SetValue(this, 1);
                    break;
            }
        }
    }
    public enum SpawnpointsContainerType
    {
        Default,
        TeamA,
        TeamB,
    }
}