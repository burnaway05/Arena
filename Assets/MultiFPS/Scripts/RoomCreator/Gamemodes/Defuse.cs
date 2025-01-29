using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
namespace MultiFPS.Gameplay.Gamemodes
{
    [AddComponentMenu("MultiFPS/Gamemodes/Defuse")]
    public class Defuse : TeamBasedGamemode
    {
        [SerializeField] private GameObject _bombItemPrefab;
        [SerializeField] private Transform _bombSpawnPoint;
        [SerializeField] GameObject _explosion;
        private GameObject _spawnedBombInstance;

        [SerializeField] BoxCollider[] _bombSites;

        public static Defuse Instance;

        //for keeping state of bomb
        private bool _bombPlanted;
        private bool _bombExploded;

        public Gamemode_GenericEvent Defuse_OnBombPlanted;
        public Gamemode_GenericEvent Defuse_OnBombDefused;

        
        protected override void Awake()
        {
            Instance = this;

            _explosion.transform.position = Vector3.zero;
            _explosion.SetActive(false);

            base.Awake();
            //we need only those bombsite parameters, we dont need them to be active
            for (int i = 0; i < _bombSites.Length; i++)
            {
                _bombSites[i].gameObject.SetActive(false);
            }
        }

        public Defuse() 
        {
            Indicator = Gamemodes.Defuse;
            LetPlayersSpawnOnTheirOwn = false;
            LetPlayersTakeControlOverBots = true;
        }

        protected override void RoundEvent_Setup()
        {
            base.RoundEvent_Setup();

            _bombPlanted = false;
            _bombExploded = false;

            if (_spawnedBombInstance)
                NetworkServer.Destroy(_spawnedBombInstance);

            _spawnedBombInstance = Instantiate(_bombItemPrefab, _bombSpawnPoint.position, _bombSpawnPoint.rotation);
            NetworkServer.Spawn(_spawnedBombInstance);

            LetPlayersSpawnOnTheirOwn = false;
        }

        public override void SetupGamemode(RoomProperties roomProperties)
        {
            base.SetupGamemode(roomProperties);
            LetPlayersSpawnOnTheirOwn = false;
        }

        public override void Server_OnPlayerKilled(uint victimID, uint killerID)
        {
            //count score only when game runs, not for example during warmup
            if (State != GamemodeState.Inprogress) return;

            OnPlayerKilled(new int[] { GetAliveTeamAbundance(0), GetAliveTeamAbundance(1)});

            if (RoundState != GamemodeRoundState.InProgress) return; //if round is ended, don't end it another time

            if (!_bombPlanted)
            {
                //if bomb is not planted, just end the game when one of the teams is completely eliminated
                CheckAliveTeamStates();
            }
            else 
            {
                if (_bombExploded) return; //if bomb exploded and killed someone gamemode already changes state so we cant make it here
                //case when terrorists win by killing defenders while bomb was planted
                int winnerTeam = GetAliveTeamAbundance(0) <= 0 ? 1 : -1;

                if (winnerTeam == -1) return;

                _teamScores[winnerTeam]++;

                RPC_TBG_UpdateTeamScores(_teamScores);

                FinalizeTeamScores(winnerTeam);

                SwitchRoundState(GamemodeRoundState.RoundEnded);
            }
        }

        protected override void RoundEvent_TimerEnded()
        {
            base.RoundEvent_TimerEnded();

            if (_bombPlanted)
            {
                _bombExploded = true;
                _explosion.transform.position = _spawnedBombInstance.transform.position;

                Bomb bomb = _spawnedBombInstance.GetComponent<Bomb>();

                //deal explosion damaga to nearby players
                Collider[] col = Physics.OverlapSphere(_spawnedBombInstance.transform.position, bomb.ExplosionRange, GameManager.characterLayer);

                for (int i = 0; i < col.Length; i++)
                {
                    Health health = col[i].gameObject.GetComponent<Health>();

                    if (!health) continue;

                    float distance = Vector3.Distance(_spawnedBombInstance.transform.position, health.transform.position);

                    //distance = Mathf.Clamp(distance, bomb.ExplosionRange * (bomb.MinimumExplosionRange/bomb.ExplosionRange), bomb.ExplosionRange);
                    float percentOfDamage;

                    if (distance <= bomb.MinimumExplosionRange)
                        percentOfDamage = 1;
                    else
                    {
                        percentOfDamage = 1f - ((distance - bomb.MinimumExplosionRange) / (bomb.ExplosionRange - bomb.MinimumExplosionRange));
                    }

                    //health.Server_ChangeHealthStateRaw(Mathf.FloorToInt(bomb.MaxExplosionDamage * percentOfDamage), 0, AttackType.explosion, health.netId, 1000);
                }

                StartCoroutine(PushRigidbodies(bomb.transform.position, bomb.ExplosionRange));
                IEnumerator PushRigidbodies(Vector3 explosionOrigin, float explosionRange)
                {
                    //we need to wait till the end of frame to make sure ragdolls are already spawned
                    //so we can push them
                    yield return new WaitForFixedUpdate();

                    Collider[] collidersRigidbody = Physics.OverlapSphere(explosionOrigin, explosionRange, GameManager.rigidbodyLayer);

                    foreach (Collider c in collidersRigidbody)
                    {
                        Rigidbody rg = c.GetComponent<Rigidbody>();
                        if (rg)
                        {
                            rg.AddExplosionForce(1000, explosionOrigin, explosionRange);
                        }
                    }
                }

                NetworkServer.Destroy(_spawnedBombInstance);
                RpcDetonateBomb(_spawnedBombInstance.transform.position);
            }

            _teamScores[_bombPlanted ? 1 : 0] +=1;
            RPC_TBG_UpdateTeamScores(_teamScores);

            RPC_TBG_RoundEnd(_bombPlanted? 1: 0);
            SwitchRoundState(GamemodeRoundState.RoundEnded);

        }

        [ClientRpc]
        void RpcDetonateBomb(Vector3 detonationPosition) 
        {
            _explosion.transform.position = detonationPosition;

            _explosion.SetActive(true);
            _explosion.GetComponent<ParticleSystem>().Play();
            _explosion.GetComponent<AudioSource>().Play();
        }

        protected override void MatchEvent_StartMatch()
        {
            base.MatchEvent_StartMatch();
            RPC_TBG_UpdateTeamScores(_teamScores);
        }

        public bool BombCanBePlanted(CharacterInstance bombPlanter) 
        {
            if (RoundState != GamemodeRoundState.InProgress) return false;

            for (int i = 0; i < _bombSites.Length; i++)
            {
                BoxCollider bombSite = _bombSites[i];

                Collider[] playersInBombSite = Physics.OverlapBox(bombSite.transform.position + bombSite.center, bombSite.size/2, bombSite.transform.rotation, GameManager.characterLayer);

                for (int playerID = 0; playerID < playersInBombSite.Length; playerID++)
                {
                    CharacterInstance planter = playersInBombSite[playerID].GetComponent<CharacterInstance>();
                    if (planter && planter == bombPlanter) return true;
                }
            }
            return false;
        }

        public void BombPlanted() 
        {
            _bombPlanted = true;
            GamemodeMessage("Bomb planted", 6f);
            CountTimer(30);

            //notify clients
            RpcBombPlanted();
        }
        public void BombDefused()
        {
            _bombPlanted = false;
            GamemodeMessage("Bomb defused", 5f);
            CountTimer(5);
            RpcBombDefused();
        }

        [ClientRpc]
        void RpcBombPlanted() 
        {
            Defuse_OnBombPlanted?.Invoke();
        }

        [ClientRpc]
        void RpcBombDefused()
        {
            Defuse_OnBombDefused?.Invoke();
        }


    }
}
