using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.Gameplay
{
    public class PooledBullet : PooledObject
    {
        TrailRenderer _trailRenderer;
        [SerializeField] float _bulletSpeed = 30f;
        Coroutine _bulletLiveCounter;
        [SerializeField] MeshRenderer _bulletMesh;

        public override void OnObjectInstantiated()
        {
            base.OnObjectInstantiated();
            _trailRenderer = GetComponent<TrailRenderer>();
        }

        public override void StartBullet(Vector3[] targetPoint)
        {
            enabled = true;
            _trailRenderer.Clear();

            if (_bulletLiveCounter != null)
                StopCoroutine(_bulletLiveCounter);

            _bulletLiveCounter = StartCoroutine(CountToDisable(targetPoint));

            _bulletMesh.enabled = true;
        }

        void Update()
        {
            transform.position += Time.deltaTime * _bulletSpeed * transform.forward;
        }
        IEnumerator CountToDisable(Vector3[] targetPoints)
        {
            for (int i = 0; i < Mathf.Min(targetPoints.Length, 2); i++)
            {
                Vector3 currentTarget = targetPoints[i];
                transform.LookAt(targetPoints[i == 0 ? 0 : targetPoints.Length - 1]);
                float timeOfLiving = Vector3.Distance(transform.position, currentTarget) / _bulletSpeed;
                yield return new WaitForSeconds(timeOfLiving);
                transform.position = currentTarget;
            }
            _bulletMesh.enabled = false;
            transform.position = targetPoints[targetPoints.Length-1];
            enabled = false; //this will stop bullet
        }
    }
}
