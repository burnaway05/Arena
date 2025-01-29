using UnityEngine;

namespace MultiFPS.Gameplay {
    public class HitBox : MonoBehaviour
    {
        public CharacterPart part;
        public Health _health;

     /*   public void Damage(int damage, AttackType attackType, uint _attackerID)
        {
            _health.Server_ChangeHealthState(damage, (byte)part, attackType, _attackerID);
        }*/
        private void Awake()
        {
            gameObject.layer = (int)GameLayers.hitbox;
        }
    }
}