using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS {
    public class GameplayCamera : MonoBehaviour
    {
        public static GameplayCamera _instance;
        private Transform target;

        private Camera _camera;
        public Camera FPPCamera;

        private float FOVMultiplier = 1f;
        private float rawRequestedFOV = 60;

        private void Awake()
        {
            _instance = this;
            if (!target)
            {
                target = transform;
            }

            _camera = GetComponent<Camera>();
        }
        private void Update()
        {
            if (target)
                transform.SetPositionAndRotation(target.position, target.rotation);

            float finalFOV = rawRequestedFOV * FOVMultiplier;

            _camera.fieldOfView = finalFOV;
        }
        public void SetTarget(Transform _target)
        {
            if (!_target)
                return;
            target = _target;
        }


        public void MultiplyMovementFieldOfView(float _fieldOfViewMultiplier, float _speed = 5f)
        {
            FOVMultiplier = Mathf.Lerp(FOVMultiplier, _fieldOfViewMultiplier, _speed * Time.deltaTime);
        }
        public void SetFieldOfView(float _fieldOfView)
        {
            rawRequestedFOV = _fieldOfView;
        }
        public void LerpRequestedFOV(float _requestedFOV, float _lerpSpeed)
        {
            rawRequestedFOV = Mathf.Lerp(rawRequestedFOV, _requestedFOV, _lerpSpeed * Time.deltaTime);
        }

        public void RenderFppModels(bool render) 
        {
            FPPCamera.enabled = render;
        }
    }

}
