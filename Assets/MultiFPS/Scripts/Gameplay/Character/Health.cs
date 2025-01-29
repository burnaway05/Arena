using UnityEngine;
using Mirror;

using MultiFPS.UI.HUD;
using MultiFPS;
using MultiFPS.UI;

namespace MultiFPS.Gameplay
{
    public class Health : MonoBehaviour
    {
        [Header("Health")]
        /// <summary>
        /// character name, used for example for killfeed
        /// </summary>
        public string CharacterName = "DEFAULT";
        public int CurrentHealth = 100;
        public int MaxHealth = 100;

        public int Team = -1;

        [Header("Element 1 -Body, 2 -Head, another ones can be custom")]
        [SerializeField] float[] damageMultipliers = { 1f, 2, 0.5f };

        public delegate void ClientHealthStateChanged(int _currentHealth, byte _hittedPartID, AttackType attackType, uint _attackerID);
        public ClientHealthStateChanged Client_HealthStateChanged;

        public delegate void ClientHealthAdded(int _currentHealth, uint _attackerID);
        public ClientHealthAdded Client_HealthAdded;

        public delegate void HealthDepleted(byte _hittedPartID, uint _attackerID);
        public HealthDepleted Client_HealthDepleted;

        public delegate void KillConfirmation(byte _hittedPartID, uint _victimID);
        public HealthDepleted Client_KillConfirmation;

        public delegate void SHealthDepleted(byte _hittedPartID, AttackType attackType, uint _attackerID, int attackForce);
        public SHealthDepleted Server_HealthDepleted;

        bool dead = false;
        bool clientDead = false;

        public Vector3 centerPosition = new Vector3(0, 1.5f, 0);

        #region register/deregister health object
        protected virtual void Start()
        {
            gameObject.layer = (int)GameLayers.character;
            GameManager.AddHealthInstance(this);

            //update 1.1: moved here from CharacterInstance
            CustomSceneManager.RegisterCharacter(this);
        }
        protected virtual void OnDestroy()
        {
            GameManager.RemoveHealthInstance(this);

            //update 1.1: moved here from CharacterInstance
            CustomSceneManager.DeRegisterCharacter(this);
        }
        #endregion

        public void Server_ChangeHealthState(int damage, byte hittedPartID, AttackType attackType, uint attackerID, int attackForce)
        {
            if (!(!GameManager.Gamemode.FriendyFire && /*attackerID != netId &&*/ GameManager.GetHealthInstance(attackerID).Team == Team)) //avoid friendly fire
                CurrentHealth -= Mathf.FloorToInt(damage * damageMultipliers[hittedPartID]);

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, 100000);
            //RpcHealthStateChanged(CurrentHealth, hittedPartID, attackType, attackerID);

            Server_OnDamaged(CurrentHealth, hittedPartID, attackType, attackerID);

            if (CurrentHealth <= 0 && !dead)
            {
                Server_HealthDepleted?.Invoke(hittedPartID, attackType, attackerID, attackForce);
                dead = true;
            }
        }

        public void Server_ChangeHealthStateRaw(int damage, byte hittedPartID, AttackType attackType, uint attackerID, int attackForce)
        {
            if (!(!GameManager.Gamemode.FriendyFire && /*attackerID != netId &&*/ GameManager.GetHealthInstance(attackerID).Team == Team)) //avoid friendly fir
                CurrentHealth -= damage;

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, 100000);

            if (CurrentHealth <= 0 && !dead)
            {
                Server_HealthDepleted?.Invoke(hittedPartID, attackType, attackerID, attackForce);
                dead = true;
            }

            //RpcHealthStateChanged(CurrentHealth, hittedPartID, attackType, attackerID);
        }

        //[ClientRpc]
        //void RpcHealthStateChanged(int currentHealth, byte hittedPartID, AttackType attackType, uint attackerID)
        //{
        //    //if (!isServer) 
        //        CurrentHealth = currentHealth;
        //
        //    if (!clientDead)
        //    {
        //        Client_HealthStateChanged?.Invoke(CurrentHealth, hittedPartID, attackType, attackerID);
        //
        //        if (CurrentHealth <= 0)
        //        {
        //            Client_HealthDepleted?.Invoke(hittedPartID, attackerID);
        //
        //            //GameManager.HealthInstances[attackerID].Client_KillConfirmation?.Invoke(hittedPartID, netId);
        //
        //            clientDead = true;
        //        }
        //
        //
        //        //TODO: move somewhere else
        //        if (ClientFrontend.ClientPlayerInstance && attackerID == ClientFrontend.ObservedCharacterNetID())
        //            HitMarker._instance.PlayHitMarker((CharacterPart)hittedPartID);
        //    }
        //
        //    OnClientDamaged(CurrentHealth, attackerID);
        //}
        //
        //[ClientRpc]
        //void RpcHealthAdded(int currentHealth, int addedHealth, uint healerID)
        //{
        //    //if(!isServer)
        //        CurrentHealth = currentHealth;
        //
        //    Client_HealthAdded?.Invoke(currentHealth, healerID);
        //    OnClientHealthAdded(currentHealth, addedHealth, healerID);
        //}

        protected virtual void OnClientHealthAdded(int currentHealth, int addedHealth, uint healerID) 
        {

        }
        protected virtual void OnClientDamaged(int currentHealth, uint attackerID)
        {

        }

        public int CountDamage(byte hittedPart, int damage)
        {
            return Mathf.FloorToInt(damage * damageMultipliers[hittedPart]);
        }

        //for AI to know where to aim at
        public Vector3 GetPositionToAttack()
        {
            return transform.position + transform.rotation * centerPosition;
        }

        //execute only on server, then server will update health state for every client
        public int ServerHeal(int healthToAdd, uint healerID) 
        {
            if (CurrentHealth == MaxHealth) return 0;

            int neededHealth = MaxHealth - CurrentHealth;

            if (neededHealth > healthToAdd)
                neededHealth = healthToAdd;

            CurrentHealth += neededHealth;

            //RpcHealthAdded(CurrentHealth, neededHealth, healerID);

            return neededHealth;
        }

        protected virtual void Server_OnDamaged(int damage, byte hittedPartID, AttackType attackType, uint attackerID) 
        {

        }
    }
    public enum CharacterPart : byte
    {
        body,
        head,
        legs,
    }
}