using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MultiFPS.Gameplay {
    public class HealthPickup : PickupObject
    {
        public int HealthSupply = 50;

        protected override void Contact(CharacterInstance _character)
        {

            int heal = _character.ServerHeal(HealthSupply, netId);

            if (heal == 0) return;

            Pickedup();
        }

        protected override void ClientSupplyDepleted()
        {
            base.ClientSupplyDepleted();

           // GetComponent<ParticleSystem>().Play();
        }
    }
}
