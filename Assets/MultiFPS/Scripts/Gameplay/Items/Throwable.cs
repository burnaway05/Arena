using Mirror;
using MultiFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.Gameplay 
{
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(AudioSource))]

	[DisallowMultipleComponent]
	[AddComponentMenu("MultiFPS/Items/Throwable")]
	public class Throwable : NetworkBehaviour
	{
		//if someone is killed by this throwable then we will assign kill to this owner
		CharacterInstance _myOwner;
		public float ExplosionRange = 4f;
		public int MaxDamage = 200;

		public float TimeToDetonate = 4f;
		public bool DetonateOnCollision = false;
		public int DamageOnDirectCollision = 30;
		public float ExplosionRigidbodyForce = 300;

		bool _detonated = false;

		[SerializeField] GameObject _model;
		[SerializeField] ParticleSystem _explosionEffect;
		[SerializeField] AudioClip _explosionClip;
		[SerializeField] Light _light;

		Rigidbody _rigidBody;

		CapsuleCollider _collider;

		bool _crashed = false;

        private void Awake()
        {
			GameTools.SetLayerRecursively(gameObject, 12);
			_collider = GetComponent<CapsuleCollider>();
			_collider.enabled = false;
		}
        public void Activate(CharacterInstance owner, Vector3 force)
		{
			_collider = GetComponent<CapsuleCollider>();
			_collider.enabled = true;

			_myOwner = owner;
			_detonated = false;

			_rigidBody = gameObject.AddComponent<Rigidbody>();
			_rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

			_rigidBody.AddForce(force);

			StartCoroutine(CountToDetonate());
			IEnumerator CountToDetonate() 
			{
				float time = 0;
				while (!(_myOwner && (Vector3.Distance(_myOwner.transform.position + _myOwner.transform.up * 1f, transform.position) > 1.5f || time > 0.5f))) 
				{
					time += Time.deltaTime;
					yield return null;
				}
				GameTools.SetLayerRecursively(gameObject, 10);

				yield return new WaitForSeconds(TimeToDetonate-time);
				Detonate();
			}

		}
		private void OnCollisionEnter(Collision collision)
		{

			if (!isServer) return; //server will handle collision of throwables

			if (!_myOwner) return;

			if (collision.transform.root == _myOwner.transform.root) return;

			//print("Throwable collided with: " + collision.transform.name + collision.transform.root.name);

			if (!_crashed) 
			{
				HitBox hb = collision.gameObject.GetComponent<HitBox>();
				if (hb) 
				{

					try
					{
						//hb._health.Server_ChangeHealthState(DamageOnDirectCollision, (byte)hb.part, AttackType.hitscan, _myOwner.netId, 0);
					}
					catch
					{
						Debug.LogWarning($"ERROR: {hb.name}, {hb.transform.root.name} " + hb._health != null ? "Exist" : "Does not exist");
						Time.timeScale = 0f;

					}

					
				}
				StartCoroutine(CancelVelocity());
				IEnumerator CancelVelocity() 
				{
					yield return new WaitForSeconds(0f);
					_rigidBody.velocity = Vector3.zero;
				}
			}
			_crashed = true;

			if (!DetonateOnCollision) return;

			if (_detonated) return;

			Detonate();
		}
		void Detonate()
		{
			if (_detonated) return;

			_detonated = true;

			RpcDetonate(); //visual effect for clients

			_collider.enabled = false;

			_rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
			_rigidBody.isKinematic = true;
			_rigidBody.useGravity = false;

			transform.rotation = Quaternion.identity;

			//before exploding we have to check if our owner exist, because if not, for example due to situation when player
			//thrown granade and disconnected from the game immediately after that, then this granade may kill somebody, but killer does
			//not exist anymore, and this must be prevented, because game will try to find killer which will lead to error.
			if (_myOwner)
			{
				Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRange, GameManager.characterLayer);

				foreach (Collider c in colliders)
				{
					Health _health = c.GetComponent<Health>();
					if (_health != null)
					{
						Vector3 _victimPos = _health.transform.position + _health.transform.up * 0.5f;
						Vector3 _myPos = transform.position + transform.up * 0.4f;

						float dist = Vector3.Distance(_myPos, _victimPos);
						Ray rayFire = new Ray(_myPos, (_victimPos - _myPos).normalized);


						if (!Physics.Raycast(rayFire, dist, GameManager.environmentLayer)) //avoid damaging things behind the cover
						{
							int damage;

							dist = Mathf.Clamp(dist, ExplosionRange * 0.35f, ExplosionRange); //objects that are closer than 35% of explosion range will receive full damage
							float percentOfDamage = 1f - (dist / ExplosionRange);
							Mathf.Clamp(percentOfDamage, 0, 1);
							damage = Mathf.FloorToInt(MaxDamage * percentOfDamage);
							damage = Mathf.Max(1, damage);

							//_health.Server_ChangeHealthState(damage, 0, AttackType.hitscan, _myOwner.netId, 0);
						}
					}
				}
			}

			StartCoroutine(CountToDestroyFromScene());

			IEnumerator CountToDestroyFromScene() 
			{
				yield return new WaitForEndOfFrame();

				Collider[] collidersRigidbody = Physics.OverlapSphere(transform.position, ExplosionRange, GameManager.rigidbodyLayer);

				foreach (Collider c in collidersRigidbody)
				{
					Rigidbody rg = c.GetComponent<Rigidbody>();
					if (rg)
					{
						rg.AddExplosionForce(ExplosionRigidbodyForce, transform.position, ExplosionRange);
					}
				}

				yield return new WaitForSeconds(5f);
				NetworkServer.Destroy(gameObject);
			}

		}
		[ClientRpc]
		void RpcDetonate() 
		{
			GetComponent<AudioSource>().PlayOneShot(_explosionClip);

			if (_light)
				_light.enabled = false;

			_collider.enabled = false;
			transform.rotation = Quaternion.identity;
			_model.SetActive(false);
			_explosionEffect.Play();
		}
	}
}
