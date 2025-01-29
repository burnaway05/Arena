using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MultiFPS
{
    public class PooledParticleSystem : PooledObject
    {
        ParticleSystem _particleSystem;

        private Transform _targetToFollow;

        private void Update()
        {
            if (_targetToFollow) transform.SetPositionAndRotation(_targetToFollow.position, _targetToFollow.rotation);
        }

        public override void OnObjectInstantiated()
        {
            base.OnObjectInstantiated();
            _particleSystem = GetComponent<ParticleSystem>();
        }

        public void SetPositionTarget(Transform targetToFollow)
        {
            _targetToFollow = targetToFollow;
        }
        public override void OnObjectReused()
        {
            base.OnObjectReused();
            _particleSystem.Play();
        }
    }
}