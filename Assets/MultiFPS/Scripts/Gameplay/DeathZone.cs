using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MultiFPS.Gameplay
{
    [RequireComponent(typeof(BoxCollider))]
    public class DeathZone : NetworkBehaviour
    {
        public bool DespawnItems = false;        
        private void Awake()
        {
            gameObject.layer = (int)GameLayers.trigger;
        }
        private void OnTriggerStay(Collider other)
        {
            if (!isServer) return;
            Health h = other.GetComponent<Health>();

            //if (h)
            //    h.Server_ChangeHealthState(9999, 0,AttackType.falldamage, h.netId, 500);

            if (!DespawnItems) return;
                
            Item i = other.GetComponent<Item>();
            if (i)
            {
                NetworkServer.Destroy(i.gameObject);
            }
        }
    }
}