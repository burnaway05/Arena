using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.Gameplay
{
    public class RagDoll : MonoBehaviour
    {
        [SerializeField] SkinnedMeshRenderer _characterMesh;
        [SerializeField] float _ragdollRecoil;
        [SerializeField] float _ragdollMovementRecoil = 100f;
        [SerializeField] GameObject _headExplosion;


        public Rigidbody _head;
        public Rigidbody _footL;
        public Rigidbody _footR;
        public Rigidbody _hips;

        [SerializeField] Rigidbody[] _rigidBodies;

        [Tooltip("If You want character moving direction to be used for ragdoll then set this to true")]
        public bool ApplyMovementVelocity = true;

        private void Awake()
        {
            GameTools.SetLayerRecursively(gameObject, (int)GameLayers.ragdoll);
            EnablePhysics(false);
        }

        public void ApplySkin(SkinContainer skin) 
        {
            _characterMesh.sharedMesh = skin.Mesh;
            _characterMesh.material = skin.Material;
        }

        //client side ragdoll preparation
        public void ActivateRagdoll(CharacterPart hittedPart)
        {

            if (hittedPart == CharacterPart.head)
            {
                _head.transform.localScale = Vector3.zero;
                _ragdollRecoil *= 1.6f;
                
                if(_headExplosion)
                    Instantiate(_headExplosion, _head.position, _head.rotation).transform.SetParent(transform);
            }           
        }

        //server side ragdoll preparation
        public void ServerActivateRagdoll(Vector3 myPosition, Vector3 killerPosition, Vector3 movementDirection, int attackForce) 
        {
            //server will be the only one to calculate ragdoll physics, then data will be sent to client so they can synchronize it
            //so here we enable physics so game can calculate it
            EnablePhysics(true);

            _head.AddForce((myPosition - killerPosition).normalized * _ragdollRecoil);

            if (ApplyMovementVelocity)
                _hips.AddForce(movementDirection * _ragdollMovementRecoil);

            //apply force from attack, so for example shotgun will kick ragdoll harder than pistol
            _hips.AddForce((myPosition - killerPosition).normalized* attackForce);

            //applying force for random foot of ragdoll to make it look more fun, 50% chance for that
            int foot = Random.Range(0, 2);
            if (foot==0)
            {
                float footForceDirection = Random.Range(-1, 2);
                if (Random.Range(1, 3) == 1)
                {
                    _footL.AddForce((killerPosition - transform.position).normalized* footForceDirection * _ragdollRecoil);
                }
                else
                {
                    _footR.AddForce((killerPosition - transform.position).normalized* footForceDirection * _ragdollRecoil);
                }
            }
        }

        public void EnablePhysics(bool enable) 
        {
            foreach (Rigidbody rg in _rigidBodies) 
            {
                rg.useGravity = enable;
                rg.isKinematic = !enable;
            }

        }

        //for synchronization
        public Transform[] rigidBodies;
        public Transform[] limbFlexors;
    }
}