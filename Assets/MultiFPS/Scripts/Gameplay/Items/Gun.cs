using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MultiFPS.Gameplay
{
    [DisallowMultipleComponent]
    [AddComponentMenu("MultiFPS/Items/Gun")]
    public class Gun : Item
    {
        [Header("Gun")]
        [SerializeField] protected ParticleSystem _particleSystem;
        [SerializeField] protected ParticleSystem _huskSpawner_particleSystem;
        [SerializeField] protected AudioClip fireClip;
        [SerializeField] protected AudioClip reloadClip;

        [Header("Base gun properties")]
        public float ReloadTime = 1.5f;

        protected Coroutine _c_reload;
        protected Coroutine _c_serverReload;
        protected Transform _firePoint;

        [SerializeField] int _bulletPuncture = 2;

        [SerializeField] protected GameObject BulletPrefab;
        private int _bulletPoolerID;

        string _impactsPoolerFamilyName = "Impacts";
        int _impactsPoolerFamilyID;


        protected override void Start()
        {
            base.Start();

            _bulletPoolerID = ObjectPooler.Instance.GetPoolID(BulletPrefab.name);

            _impactsPoolerFamilyID = ObjectPooler.Instance.GetFamilyPoolID(_impactsPoolerFamilyName);
        }
        protected override void Update()
        {
            base.Update();

            if (!_myOwner) return;

            _currentRecoilScopeMultiplier = _isScoping ? _recoil_scopeMultiplier : 1;

            if (currentlyInUse)
            {
                if (CurrentRecoil > _recoil_minAngle)
                    CurrentRecoil -= _recoil_stabilizationSpeed * Time.deltaTime;
                else
                    CurrentRecoil = _recoil_minAngle;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!CanBeUsed()) return;

            //if (!isServer) return;

            if (Server_CurrentAmmo <= 0 && Server_CurrentAmmoSupply > 0 && !_server_isReloading)
                PushReload(); //set _reloadTrigger to true

            if(_reloadTrigger)
                ProcessReloadRequest();
        }

        public override void Take()
        {
            base.Take();
            _firePoint = _myOwner.characterFirePoint;
            UpdateAmmoInHud(CurrentAmmo.ToString(), CurrentAmmoSupply.ToString());
        }

        public override void PutDown()
        {
            base.PutDown();

            //there is a chance player puts down weapon while its reloading, so we need to make sure 
            //to properly cancel that procedure
            CancelReloading();

            //if (isServer)
            //    ServerCancelReloading();
        }

        protected override void SingleUse()
        {
            base.SingleUse();
            Client_OnShoot?.Invoke();
            //ChangeCurrentAmmoCount(CurrentAmmo - 1);
        }

        #region shooting
        protected override void Use()
        {
            if (CurrentAmmo <= 0 || _isReloading || _doingMelee) return;

            base.Use();

            float finalRecoil = CurrentRecoil * _myOwner.RecoilFactor_Movement * _currentRecoilScopeMultiplier;

            _firePoint.localRotation = Quaternion.Euler(Random.Range(-finalRecoil, finalRecoil), Random.Range(-finalRecoil, finalRecoil), 0);

            //fire hitscan that will deal damage to hitted enemies and provide info for visuals things, like path of bullet
            HitscanFireInfo fire = FireHitscan();


            //if (isOwned)
            Shoot(fire);

            //bots
            //if (isServer)
            //{
            //    Server_CurrentAmmo--;
            //    RpcShoot(fire);
            //}
            //else //clients
            //{
            //    CmdShoot(fire);
            //}

            ChangeCurrentAmmoCount(CurrentAmmo - 1);
        }

        //[Command]
        protected void CmdShoot(HitscanFireInfo info)
        {
            if (Server_CurrentAmmo > 0)
            {
                Server_CurrentAmmo--;
                RpcShoot(info);
            }
        }
        //[ClientRpc(includeOwner = false)]
        protected void RpcShoot(HitscanFireInfo info)
        {
            if (_myOwner)
            {
                Shoot(info);

                //if (!isServer)
                //    ChangeCurrentAmmoCount(CurrentAmmo-1);
            }
        }

        //paper shot, no damage, no game logic, only visuals
        protected void Shoot(HitscanFireInfo info)
        {
            _audioSource.PlayOneShot(fireClip);
            _particleSystem.Play();

            if (_huskSpawner_particleSystem)
            {
                //when in fpp view this would be renderer in top of everything, to avoid this we set husk spawner to default
                //layer again
                _huskSpawner_particleSystem.gameObject.layer = 0;
                _huskSpawner_particleSystem.Play();
            }

            SpawnVisualEffectsForHitscan(info);

            //if players shoots grenade, and dies in its explosion, then player will drop all of this items, so we no longer have 
            //acces to _myOwner, so simply return
            if (!_myOwner) return;

            if (_myOwner.FPP)
            {
                _myOwner.Animator.SetTrigger(AnimationNames.ITEM_FIRE);
            }

            if (_myAnimator.runtimeAnimatorController)
                _myAnimator.SetTrigger(AnimationNames.ITEM_FIRE);
        }

        protected void SpawnBullet(Vector3[] decalPos, Quaternion decalRot)
        {
            ObjectPooler.Instance.SpawnObject(_bulletPoolerID, (_isScoping ? _myOwner.FPPCameraTarget.position - _myOwner.FPPCameraTarget.up * 0.2f : _particleSystem.transform.position), decalRot).StartBullet(decalPos);//spawn bullet from barrel
        }
        #endregion

        #region reloading
        //launched by player or bot input, just tells weapoan that user has intention to reload
        public override void PushReload()
        {
            if(Server_CurrentAmmo < MagazineCapacity)
                _reloadTrigger = true;

            //if (isOwned)
                CmdClientRequestReload();
        }

        //game logic side of reload, server only
        protected void ProcessReloadRequest()
        {
            //dont start realod procedure if magazine is full or if we are already reloading, or if we dont have ammo to load in
            if (Server_CurrentAmmoSupply > 0 && Server_CurrentAmmo < MagazineCapacity && !_server_isReloading && _coolDownTimer < Time.time)
            {
                ServerReload();
                _reloadTrigger = false;
            }
        }

        //client visual part of reload, no affect on game logic, game logic is handled on server
        protected virtual void Reload() 
        {
            CurrentRecoil = _recoil_minAngle;

            CancelReloading();

            _isReloading = true;

            _c_reload = StartCoroutine(ReloadProcedure());
            IEnumerator ReloadProcedure()
            {
                _audioSource.PlayOneShot(reloadClip);

                _myOwner.Animator.ResetTrigger(AnimationNames.ITEM_ENDRELOAD);
                _myAnimator.ResetTrigger(AnimationNames.ITEM_ENDRELOAD);

                _myOwner.Animator.Play(AnimationNames.ITEM_RELOAD); //TODO: fix: this lasts even then owner no longer exist, but item was still in use by owner
                _myAnimator.Play(AnimationNames.ITEM_RELOAD);
                _myOwner.IsReloading = true; //for character TPP animations

                yield return new WaitForSeconds(ReloadTime);

                //end reload animations
                _myOwner.Animator.SetTrigger(AnimationNames.ITEM_ENDRELOAD);
                _myAnimator.SetTrigger(AnimationNames.ITEM_ENDRELOAD);

                if (!_myOwner) 
                {
                    print("TODO: Investigate why it happens");
                    yield break;
                }
            }
        }

        //[Command]
        void CmdClientRequestReload() 
        {
            if (Server_CurrentAmmo < MagazineCapacity)
                _reloadTrigger = true;
        }
        protected virtual void ServerReload()
        {
            if (_server_isReloading)
                return;


            RpcReload();

            ServerCancelReloading();
            _c_serverReload = StartCoroutine(ReloadProcedure());

            _server_isReloading = true;

            IEnumerator ReloadProcedure()
            {
                yield return new WaitForSeconds(ReloadTime);

                int neededAmmo = MagazineCapacity - Server_CurrentAmmo;
                int finalMagazine;

                if (neededAmmo > Server_CurrentAmmoSupply)
                {
                    finalMagazine = Server_CurrentAmmo + Server_CurrentAmmoSupply;
                    Server_CurrentAmmoSupply = 0;
                }
                else
                {
                    finalMagazine = MagazineCapacity;
                    Server_CurrentAmmoSupply -= neededAmmo;
                }
                Server_CurrentAmmo = finalMagazine;
                SendAmmoDataToClient(Server_CurrentAmmo, Server_CurrentAmmoSupply);

                ServerFinishClientReload();
                _server_isReloading = false;
            }
        }

        //[ClientRpc]
        protected void ServerFinishClientReload() 
        {
            if(currentlyInUse && _isReloading)
                OnReloadEnded();
        }

        /// <summary>
        /// called from the server
        /// </summary>
        protected virtual void OnReloadEnded() 
        {
            _myOwner.IsReloading = false;
            _isReloading = false;
        }

        protected void SendAmmoDataToClient(int currentAmmo, int supplyAmmo) 
        {
            RpcEndReload(currentAmmo, supplyAmmo);
        }

        //[ClientRpc]
        void RpcEndReload(int currentAmmo, int supplyAmmo) 
        {
            CurrentAmmoSupply = supplyAmmo;
            ChangeCurrentAmmoCount(currentAmmo);
        }

        //[ClientRpc]
        protected void RpcReload() 
        {
            if(_myOwner)
                Reload();
        }

        protected void CancelReloading() 
        {
            if (_c_reload != null)
            {
                StopCoroutine(_c_reload);
                _c_reload = null;
            }
            _isReloading = false;
            _reloadTrigger = false;
        }
        protected void ServerCancelReloading()
        {
            if (_c_serverReload != null)
            {
                StopCoroutine(_c_serverReload);
                _c_serverReload = null;
            }

            _server_isReloading = false;
        }

        #endregion

        protected override bool PrimaryFireAvailable()
        {
            return !_isReloading && CurrentAmmo > 0;
        }

        #region impact

        protected void SpawnImpact(Vector3 decalPos, Quaternion decalRot, byte _hittedMaterialID) 
        {
            ObjectPooler.Instance.SpawnObjectFromFamily(_impactsPoolerFamilyID, _hittedMaterialID != byte.MaxValue ? _hittedMaterialID : 0, decalPos, decalRot);//spawn decal
        }
        #endregion

        protected void ServerDamageRaw(uint hittedHealthID, CharacterPart hittedPart, int rawDamage)
        {
            //GameManager.GetHealthInstance(hittedHealthID).Server_ChangeHealthStateRaw(rawDamage, (byte)hittedPart,AttackType.hitscan, _myOwner.netId, AttackForce);
        }

        

        /// <summary>
        /// method responsible for dealing damage and penetration
        /// return struct with information about penetration points and materials that were hit in the way
        /// </summary>
        /// <returns></returns>
        protected HitscanFireInfo FireHitscan() 
        {
            Quaternion hitRotation = Quaternion.identity;
            RaycastHit[] hitScan = GameTools.HitScan(_firePoint, _myOwner.transform, GameManager.fireLayer, 250f);

            int penetratedObjects = 0;

            Vector3[] penetrationPositions;
            byte[] penetratedObjectMaterialsIDs;

            //we hit something, so we have to check what to do next
            if (hitScan.Length > 0)
            {
                hitRotation = Quaternion.FromToRotation(Vector3.forward, hitScan[0].normal);

                for (int i = 0; i < Mathf.Min(hitScan.Length, _bulletPuncture); i++)
                {
                    penetratedObjects++;

                    RaycastHit currentHit = hitScan[i];
                    GameObject go = currentHit.collider.gameObject;
                    HitBox hb = go.GetComponent<HitBox>();
                    if (hb)
                    {
                        //if (!_myOwner.BOT)
                        //{
                        //    CmdDamage(hb._health.netId, hb.part, 1f / (i + 1), i == 0 ? AttackType.hitscan : AttackType.hitscanPenetrated); //the more objects we penetrated the less damage we deal
                        //}
                        //else
                        //{
                        //    ServerDamage(hb._health.netId, hb.part, 1f / (i + 1), i == 0 ? AttackType.hitscan : AttackType.hitscanPenetrated);
                        //}
                    }
                    if (go.layer == 0) //if we hitted solid wall, dont penetrate it further
                        break;
                }

                penetrationPositions = new Vector3[penetratedObjects];
                penetratedObjectMaterialsIDs = new byte[penetratedObjects];
                //material detection for appropriate particle impact effect
                for (int i = 0; i < penetratedObjects; i++)
                {
                    penetrationPositions.SetValue(hitScan[i].point, i);

                    byte matID = 0;
                    switch (hitScan[i].collider.tag)
                    {
                        case "Flesh":
                            matID = 1;
                            break;
                    }

                    penetratedObjectMaterialsIDs.SetValue(matID, i);
                }
            }
            else 
            {
                penetrationPositions = new Vector3[1] { _firePoint.forward * 99999f };
                penetratedObjectMaterialsIDs = new byte[0];
            }



            return new HitscanFireInfo() {
                PenetrationPositions = penetrationPositions,
                PenetratedObjectMaterialsIDs = penetratedObjectMaterialsIDs,
                FirstHitRotation = hitRotation,
            } ;
        }

        //Information required to render bullets and hit effects
        protected struct HitscanFireInfo
        {
            public Vector3[] PenetrationPositions;
            public byte[] PenetratedObjectMaterialsIDs;
            public Quaternion FirstHitRotation;
        }

        //spawn impact and bullet for given hitscan that happened
        protected void SpawnVisualEffectsForHitscan(HitscanFireInfo info) 
        {
            SpawnBullet(info.PenetrationPositions, info.FirstHitRotation);
            for (int i = 0; i < info.PenetratedObjectMaterialsIDs.Length; i++)
            {
                SpawnImpact(info.PenetrationPositions[i], info.FirstHitRotation, info.PenetratedObjectMaterialsIDs[i]);
            }
        }

        protected override void OnCurrentAmmoChanged()
        {
            UpdateAmmoInHud(CurrentAmmo.ToString(), CurrentAmmoSupply.ToString());
        }
    }
 
}