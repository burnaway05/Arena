using UnityEngine;

namespace MultiFPS.Gameplay
{
    [DisallowMultipleComponent]
    /// <summary>
    /// Component responsible for spawning ragdoll on character death
    /// </summary>
    public class HumanoidDeath : MonoBehaviour
    {
        [Tooltip("Ragdoll prefab that will be spawned on character death")]
        [SerializeField] GameObject ragDoll_Prefab;
        [SerializeField] Transform _head;
        
        [Tooltip("Clip that will be player when character receives headshot")]
        [SerializeField] AudioClip _impactClip_head;
        [SerializeField] AudioClip _impactClip_body;

        AudioSource _audioSource;

        RagDoll _spawnedRagdoll;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        private void Start()
        {
            GetComponent<Health>().Server_HealthDepleted += ServerHealthDepleted;
            GetComponent<Health>().Client_HealthStateChanged += CheckHealthState;
        }
        void ServerHealthDepleted (byte hittedPartID, AttackType attackType, uint attackerID, int attackForce) 
        {
            CharacterInstance characterInstance = GetComponent<CharacterInstance>();

            _spawnedRagdoll = SpawnRagdoll(characterInstance.MySkin);

            Vector3 movementDirection = transform.rotation * new Vector3(characterInstance.movementInput.x, 0, characterInstance.movementInput.y);

            _spawnedRagdoll.ServerActivateRagdoll(characterInstance.GetPositionToAttack(), GameManager.GetHealthInstance(attackerID).GetPositionToAttack(), movementDirection * (characterInstance.ReadActionKeyCode(ActionCodes.Sprint) ? 2f : 1f), attackForce);

            GetComponent<RagDollSyncer>().ServerStartSynchronizingRagdoll(_spawnedRagdoll.GetComponent<RagDoll>());
        }

        void CheckHealthState(int _currentHealth, byte _hittedPartID, AttackType attackType, uint _attackerID)
        {
            CharacterInstance characterInstance = GetComponent<CharacterInstance>();

            _audioSource.Stop();
            //play headshot clip when hitted in head, plays always when character receives damage, not only for death
            if (_hittedPartID == (byte)CharacterPart.head)
            {
                PooledObject headImpact = ObjectPooler.Instance.SpawnObjectFromFamily("Effects","HeadShot", _head.position, _head.rotation);
                headImpact.transform.LookAt(GameManager.GetHealthInstance(_attackerID).GetPositionToAttack());

                _audioSource.PlayOneShot(_impactClip_head);
            }
            else
                _audioSource.PlayOneShot(_impactClip_body);

            //executes only on death, hides player model and spawns ragdoll
            if (_currentHealth <= 0)
            {
                

                characterInstance.Animator.enabled = false;
                characterInstance.Animator.gameObject.SetActive(false);

                _spawnedRagdoll = SpawnRagdoll(characterInstance.MySkin);
                _spawnedRagdoll.ActivateRagdoll((CharacterPart)_hittedPartID);

                if (_hittedPartID == (byte)CharacterPart.head)
                {
                    PooledObject headBlood = ObjectPooler.Instance.SpawnObjectFromFamily("Effects", "NeckBlood", _spawnedRagdoll._head.transform.position, _spawnedRagdoll._head.transform.rotation);
                    //headBlood.transform.LookAt(GameManager.GetHealthInstance(_attackerID).GetPositionToAttack());
                    headBlood.GetComponent<PooledParticleSystem>().SetPositionTarget(_spawnedRagdoll._head.transform);
                }

                characterInstance.ObjectForDeathCameraToFollow = _spawnedRagdoll._head.transform;

                GetComponent<RagDollSyncer>().AssignRagdoll(_spawnedRagdoll);
            }
        }

        RagDoll SpawnRagdoll(SkinContainer skin) 
        {

            if (!_spawnedRagdoll)
            {
                _spawnedRagdoll = Instantiate(ragDoll_Prefab, transform.position, transform.rotation).GetComponent<RagDoll>();

                _spawnedRagdoll.transform.SetParent(transform);

                _spawnedRagdoll.ApplySkin(skin);
                return _spawnedRagdoll;
            }
            else 
            {
                return _spawnedRagdoll;
            }
        }
    }
}