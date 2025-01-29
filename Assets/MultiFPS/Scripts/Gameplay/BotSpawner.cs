using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MultiFPS.Gameplay;
using MultiFPS;

namespace MultiFPS
{
    public class BotSpawner : NetworkBehaviour
    {
        public GameObject _object;

        bool _spawnedBot = false;

        public int Team = 0;

        public bool SpawnOnStart = true;

        //we can override base player equipment by using this
        public GameObject[] ItemsOnSpawn;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);

            if (SpawnOnStart)
                Spawn();
        }
        void Spawn()
        {
            if (!isServer) return;

            if (_spawnedBot) return;

            PlayerInstance ourBot = GameManager.Gamemode.SpawnBot(Team);


            ourBot.SetItemsOnSpawn(ItemsOnSpawn);
            _spawnedBot = true;
            /* if (_mySpawnedObject)
                 return;

             GameObject gm = Instantiate(_object, transform.position, transform.rotation);
             NetworkServer.Spawn(gm);

             PlayerInstance playerInstance = gm.GetComponent<PlayerInstance>();
             playerInstance.playerName = "BOT " + Random.Range(0, 999).ToString();

             if (playerInstance)
             {
                 playerInstance.SetAsBot();
                 playerInstance.ProcessRequestToJoinTeam(Team);
             }

             _mySpawnedObject = gm;*/
        }
    }
}