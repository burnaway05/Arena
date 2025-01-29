using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.Gameplay
{
	
	public class ExplosionDamage : MonoBehaviour
	{
		[SerializeField] float _explosionRange = 50;
		[SerializeField] float _maxDamage = 1000;
		[SerializeField] bool _ignoreWalls;

		private uint ownerID;

		void Detonate()
		{
			Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRange, GameManager.characterLayer);

			foreach (Collider c in colliders)
			{
				Health _health = c.GetComponent<Health>();
				if (_health != null)
				{
					Vector3 _victimPos = _health.GetPositionToAttack();
					//Vector3 _victimPos = _health.transform.position + _health.transform.up * 0.5f;
					Vector3 _myPos = transform.position + transform.up * 0.4f;
					float dist = Vector3.Distance(_myPos, _victimPos);

					Ray rayFire = new Ray(_myPos, (_victimPos - _myPos).normalized);

					if (!_ignoreWalls && Physics.Raycast(rayFire, dist, GameManager.environmentLayer)) continue; //avoid damaging things behind the cover

					int damage;
					dist = Mathf.Clamp(dist, _explosionRange * 0.35f, _explosionRange); //objects that are closer than 35% of explosion range will receive full damage
					float percentOfDamage = 1f - (dist / _explosionRange);
					Mathf.Clamp(percentOfDamage, 0, 1);
					damage = Mathf.FloorToInt(_maxDamage * percentOfDamage);
					damage = Mathf.Max(1, damage);
					_health.Server_ChangeHealthState(damage, 0, AttackType.explosion, ownerID, 0);


				}
			}
		}
	}
}