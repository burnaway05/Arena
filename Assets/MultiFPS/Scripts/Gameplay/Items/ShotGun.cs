using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MultiFPS.Gameplay
{
    [DisallowMultipleComponent]
    [AddComponentMenu("MultiFPS/Items/Shotgun")]
    public class ShotGun : Gun
    {
        [Header("Shotgun")]
        [SerializeField] int bulletsInOneShoot;

        bool _cancelReload = false;

        bool _clientCancelReload = false;

        [Header("ReloadProperties")]
        [SerializeField] float _reloadTime_start;
        [SerializeField] float _reloadTime; //single bullet load
        [SerializeField] float _reloadTime_end;

        [SerializeField] AudioClip _shellLoadClip;
        [SerializeField] AudioClip _endReloadClip;

        protected override void Update()
        {
            base.Update();
            if (!_myOwner) return;

            if (_myOwner.ReadActionKeyCode(ActionCodes.Trigger1) && CurrentAmmo > 0)
            {
                if (isOwned) //client
                {
                    _clientCancelReload = true;
                    CmdCancelReload();
                }
                else //bot
                    _cancelReload = true;
            }
        }

        protected override void Use()
        {
            if (!_myOwner) return;

            if (CurrentAmmo <= 0 || _isReloading) return;

            HitscanFireInfo[] hitscans = new HitscanFireInfo[bulletsInOneShoot]; //make raycast, deal damage, etc

            for (int i = 0; i < bulletsInOneShoot; i++) //single shoot
            {
                RandomRecoil();
                hitscans.SetValue(FireHitscan(), i);
            }

            if (isOwned)
                MultipleShoot(hitscans);

            if (_myOwner.BOT) //server
            {
                Server_CurrentAmmo--;
                RpcMultipleShoot(hitscans);
            }
            else //client
                CmdMultipleShoot(hitscans);

            _firePoint.localRotation = Quaternion.identity;


            if (isOwned)
            {
                SingleUse(); //for client to for example immediately see muzzleflash when he fires his gun
                CmdSingleUse();
            }
            else if (isServer)
            {
                SingleUse();
                RpcSingleUse();
            }

            ChangeCurrentAmmoCount(CurrentAmmo - 1);
        }

        [Command]
        void CmdMultipleShoot(HitscanFireInfo[] info)
        {
            if (Server_CurrentAmmo > 0)
            {
                Server_CurrentAmmo--;
                RpcMultipleShoot(info);
            }
        }

        //dont want to launch this method for client who took a shoot because
        //he already launched it locally
        [ClientRpc(includeOwner = false)]
        void RpcMultipleShoot(HitscanFireInfo[] info)
        {
            //there can be a situation when client receice a message to use item when it is already dropped, so
            //check if it can be used
            if (_myOwner) 
                MultipleShoot(info);
        }
        /// <summary>
        /// it just plays visuals, does not deal damage
        /// </summary>
        /// <param name="info"> Information of each bullet, direction, dealt penetration</param>

        protected void MultipleShoot(HitscanFireInfo[] info)
        {
            if (_myOwner.FPP)
                _myOwner.Animator.SetTrigger("recoil");

            _myAnimator.SetTrigger("recoil");

            _audioSource.PlayOneShot(fireClip);
            _particleSystem.Play();

            if (_huskSpawner_particleSystem)
            {
                _huskSpawner_particleSystem.gameObject.layer = 0;
                _huskSpawner_particleSystem.Play();
            }

            for (int i = 0; i < info.Length; i++)
            {
                SpawnVisualEffectsForHitscan(info[i]);
            }
        }

        public void RandomRecoil()
        {
            float finalRecoil = CurrentRecoil * _myOwner.RecoilFactor_Movement * _currentRecoilScopeMultiplier;            

            _firePoint.localRotation = Quaternion.Euler(Random.Range(-finalRecoil, finalRecoil), Random.Range(-finalRecoil, finalRecoil), 0);
        }

        protected override void Reload()
        {
            _clientCancelReload = false;

            CurrentRecoil = _recoil_minAngle;

            CancelReloading();

            _isReloading = true;

            _c_reload = StartCoroutine(ReloadProcedure());
            IEnumerator ReloadProcedure()
            {
                if (_myOwner.FPP)
                   _myOwner.Animator.SetTrigger(AnimationNames.ITEM_RELOAD);
                _myAnimator.SetTrigger(AnimationNames.ITEM_RELOAD);

                yield return new WaitForSeconds(_reloadTime_start);


                while (CurrentAmmoSupply > 0 && CurrentAmmo < MagazineCapacity && _isReloading)
                {

                    if (_myOwner.FPP)
                        _myOwner.Animator.SetTrigger(AnimationNames.ITEM_ENDRELOAD);
                    _myAnimator.SetTrigger(AnimationNames.ITEM_ENDRELOAD);

                    CurrentAmmoSupply--;
                    ChangeCurrentAmmoCount(CurrentAmmo + 1);

                    

                    _audioSource.PlayOneShot(_shellLoadClip);

                    yield return new WaitForSeconds(_reloadTime);
                    if (_clientCancelReload)
                    {
                        CmdCancelReload();
                        break;
                    }
                }

                _audioSource.PlayOneShot(_endReloadClip);

                yield return new WaitForSeconds(_reloadTime_end);

                _clientCancelReload = false;
                _coolDownTimer = Time.time; //let player shoot right after finished reloading
            }
        }



        protected override void ServerReload()
        {
            if (_server_isReloading)
            {
                print("CHECK");
                return;
            }

            RpcReload();

            ServerCancelReloading();
            _c_serverReload = StartCoroutine(ReloadProcedure());

            _server_isReloading = true;


            IEnumerator ReloadProcedure()
            {
                yield return new WaitForSeconds(_reloadTime_start);

                _cancelReload = false;

                while (Server_CurrentAmmoSupply > 0 && Server_CurrentAmmo < MagazineCapacity && !(_cancelReload && Server_CurrentAmmo>0))
                {
                    Server_CurrentAmmoSupply--;
                    Server_CurrentAmmo++;

                    if (_myOwner.BOT && _myOwner.ReadActionKeyCode(ActionCodes.Trigger1))
                        _cancelReload = true;

                    yield return new WaitForSeconds(_reloadTime);
                }
                

                yield return new WaitForSeconds(_reloadTime_end);

                _cancelReload = false;
                ServerFinishClientReload();
                SendAmmoDataToClient(Server_CurrentAmmo, Server_CurrentAmmoSupply);

                _server_isReloading = false;

                _cancelReload = false;
            }
        }
        [Command]
        void CmdCancelReload() 
        {
            _cancelReload = true;
        }

        public override void Take()
        {
            base.Take();
            UpdateAmmoInHud(CurrentAmmo.ToString(), CurrentAmmoSupply.ToString());
        }
    }
}