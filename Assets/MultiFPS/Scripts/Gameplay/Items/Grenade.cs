using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.Gameplay
{
    [DisallowMultipleComponent]
    [AddComponentMenu("MultiFPS/Items/Grenade")]
    public class Grenade : Item
    {
        public GameObject ThrowablePrefab;
        public float RigidBodyForce = 2500;

        protected override void Awake()
        {
            base.Awake();

            ChangeCurrentAmmoCount(CurrentAmmoSupply);
        }
        protected override void Use()
        {
            if (CurrentAmmoSupply <= 0) return;

            base.Use();

            if (isServer)
                SpawnThrowable();
            else
            {
                CmdSpawnThrowable();

                CurrentAmmoSupply--;
                ChangeCurrentAmmoCount(CurrentAmmoSupply);
            }

            if (CurrentAmmoSupply <= 0) _myOwner.CharacterItemManager.ChangeItemDelay(-1, 0.45f);
        }
        [Command]
        void CmdSpawnThrowable() 
        {
            SpawnThrowable();
        }
        void SpawnThrowable() 
        {
            if (CurrentAmmoSupply <= 0) return;

            CurrentAmmoSupply--;
            ChangeCurrentAmmoCount(CurrentAmmoSupply);

            GameObject throwable = Instantiate(ThrowablePrefab, _myOwner.GetPositionToAttack(), Quaternion.Euler(_myOwner.lookInput.x, _myOwner.lookInput.y, 0));

            Vector3 force = Quaternion.Euler(_myOwner.lookInput.x, _myOwner.lookInput.y, 0)*Vector3.forward * RigidBodyForce;

            throwable.GetComponent<Throwable>().Activate(_myOwner, force);
            NetworkServer.Spawn(throwable);
        }
        public override void Take()
        {
            base.Take();
            UpdateAmmoInHud(CurrentAmmo.ToString());
        }

        /*protected override void OnOwnerPickedupAmmo()
        {
            //UpdateAmmoInHud(CurrentAmmoSupply.ToString());
        }*/

        protected override void SingleUse()
        {
            //if we are out of granades, hide granade model from player hand
            

            _myOwner.CharacterItemManager.StartUsingItem(); //will disable ability tu run for 0.5 seconds
            _myOwner.Animator.SetTrigger("recoil");
            _myAnimator.SetTrigger("recoil");
        }

        public override bool CanBeEquiped()
        {
            return base.CanBeEquiped() && CurrentAmmoSupply > 0;
        }

        protected override void OnCurrentAmmoChanged()
        {
            UpdateAmmoInHud(CurrentAmmo.ToString());
        }
    }
}