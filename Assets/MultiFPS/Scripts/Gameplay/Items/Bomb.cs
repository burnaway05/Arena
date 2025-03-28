using Mirror;
using MultiFPS.Gameplay;
using MultiFPS.Gameplay.Gamemodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MultiFPS.Gameplay {

    [DisallowMultipleComponent]
    [AddComponentMenu("MultiFPS/Items/Bomb")]
    public class Bomb : Item
    {
        [Header("Bomb")]
        bool _isPlanting;
        bool _server_isPlanting;

        [SerializeField] float _plantTime = 3f;

        Coroutine _plantingProcedure;

        public float ExplosionRange = 200;

        /// <summary>
        /// players within this radius will receive full damage
        /// </summary>
        public float MinimumExplosionRange = 50;

        public float MaxExplosionDamage = 400;

        protected override void Start()
        {
            base.Start();
            TeamOwnership = 1; //only terrorist can interact with bomb at the start
        }

        protected override void Update()
        {
            base.Update();

            if (!_myOwner) return;

            //if (!isServer) return;

            if (_isPlanting && (!_myOwner.ReadActionKeyCode(ActionCodes.Trigger1) || !Defuse.Instance.BombCanBePlanted(_myOwner)))
            {
                _server_isPlanting = false;
                EndPlanting();
                //RpcCancelPlanting();
            }
        }

        //[Command]
        protected override void CmdSingleUse()
        {
            base.CmdSingleUse();
            if (Defuse.Instance && Defuse.Instance.BombCanBePlanted(_myOwner) && !_server_isPlanting)
            {
                StartPlanting();

                //RpcStartedPlanting();

                _server_isPlanting = true;
            }
        }


        void StartPlanting()
        {
            EndPlanting();
            _plantingProcedure = StartCoroutine(PlantingProcedure());
        }
        void EndPlanting()
        {
            if (_plantingProcedure != null)
            {
                StopCoroutine(_plantingProcedure);
                _plantingProcedure = null;
            }
        }

        IEnumerator PlantingProcedure()
        {
            yield return new WaitForSeconds(_plantTime);

            if (!_myOwner) yield break;

            Vector3 ownerPos = _myOwner.transform.position;
            Quaternion ownerRot = _myOwner.transform.rotation;
            CharacterInstance planter = _myOwner;
            _myOwner.CharacterItemManager.Server_DropItem();

            Rigidbody rg = GetComponent<Rigidbody>();
            rg.isKinematic = true;
            rg.useGravity = false;

            //set bomb after being planted to be able to be interacted with only by defenders team
            TeamOwnership = 0;

            //normally items will despawn after certaing amount of time when are dropped, but we dont want this
            //behaviour for bomb so we will stop here coroutine responsible for this
            if (DestroyCoroutine != null)
            {
                StopCoroutine(DestroyCoroutine);
                DestroyCoroutine = null;
            }

            //RpcEndedPlanting(ownerPos, ownerRot, planter);

            //tell gamemode that bomb is planted
            Defuse.Instance.BombPlanted();
        }

        public override void Take()
        {
            base.Take();

            //if (TeamOwnership == 0 && isServer)
            //    Defuse.Instance.BombDefused();
        }

        public override void PutDown()
        {
            base.PutDown();
            EndPlanting();

            //_myOwner.Animator.Play(AnimationNames.ITEM_RELOAD); //TODO: fix: this lasts even then owner no longer exist, but item was still in use by owner
            //_myAnimator.Play(AnimationNames.ITEM_RELOAD);

            _server_isPlanting = false;
            _isPlanting = false;
        }

        //[ClientRpc]
        //void RpcCancelPlanting()
        //{
        //    _isPlanting = false;
        //    _myAnimator.SetTrigger("endReload");
        //    _myOwner.Animator.SetTrigger("endReload");
        //}
        //
        //[ClientRpc]
        //void RpcStartedPlanting()
        //{
        //    _isPlanting = true;
        //    _myAnimator.Play("reload");
        //    _myOwner.Animator.Play("reload");
        //}
        //[ClientRpc]
        //void RpcEndedPlanting(Vector3 position, Quaternion rotation, CharacterInstance planter)
        //{
        //    transform.rotation = rotation;
        //    transform.position = position;
        //    _isPlanting = false;
        //    planter.CharacterItemManager.TakePreviousItem();
        //}
    }
}