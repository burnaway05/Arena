using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MultiFPS.Gameplay
{
    /// <summary>
    /// This class extends basic Item melee functionalities with backstab that deals more damage than normal attack
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("MultiFPS/Items/MeleeWeapon")]

    public class MeleeWeapon : Item
    {
        [SerializeField] float _secondaryMeleeAttackCooldown = 1f;
        [SerializeField] int _secondaryMeleeDamage = 80;
        [SerializeField] int _backstabDamage = 200;
        protected override void Use()
        {
            base.Use();
            PushMeele();
        }

        protected override void SecondaryUse()
        {
            if (!_myOwner.BOT)
                SecondaryMelee();

            if (isServer)
                RpcSecondaryMeelee();
            else
                CmdSeconadaryMelee();
        }

        [Command]
        void CmdSeconadaryMelee()
        {
            RpcSecondaryMeelee();
        }
        [ClientRpc(includeOwner = false)]
        void RpcSecondaryMeelee()
        {
            SecondaryMelee();
        }
        protected virtual void SecondaryMelee ()
        {
            _myOwner.Animator.SetTrigger("recoil");
            _myAnimator.SetTrigger("recoil");

            if (_myOwner.IsObserved)
                _myOwner.PlayerRecoil.Recoil(-4f, -4f, 6, 0.15f);

            if (isOwned || (isServer&&_myOwner.BOT))
            {
                Collider[] collider = GetHealthsInMeleeRange();

                for (int i = 0; i < collider.Length; i++)
                {
                    Collider col = collider[i];
                    if (col.transform.root != _myOwner.transform.root)
                    {
                        Health victim = col.GetComponent<Health>();
                        if (victim)
                        {
                            //if (isOwned)
                            //    CmdSecondaryMeleeDamage(victim);
                            //else
                                ServerSecondaryMeleeDamage(victim);
                            break;
                        }
                    }
                }
            }
        }
        //[Command]
        //void CmdSecondaryMeleeDamage(Health health)
        //{
        //    ServerSecondaryMeleeDamage(health);
        //}
        void ServerSecondaryMeleeDamage(Health health) 
        {
            int damage = Vector3.Angle(health.transform.forward, _myOwner.transform.forward) < 50 ? _backstabDamage : _secondaryMeleeDamage;
            //health.Server_ChangeHealthState(damage, (byte)CharacterPart.body, AttackType.hitscan, _myOwner.netId, AttackForce);
        }
        protected override bool CooldownSecondary()
        {
            if (currentlyInUse && !_isReloading && _meleeCoolDownTimer <= Time.time)
            {
                _meleeCoolDownTimer = Time.time + _secondaryMeleeAttackCooldown;
                return true;
            }
            else return false;
        }
    }
}