using MultiFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.UI.HUD
{
    public class UIWorldSpaceElement : MonoBehaviour
    {
        Camera _camera;
        [SerializeField] float _sizeMultiplier;

        private Transform _objectToFollow;

        [SerializeField] protected Vector3 positionCorrection;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        void Start()
        {
            _camera = GameplayCamera._instance.FPPCamera;

        }

        protected virtual void Update()
        {
            if (_camera)
            {
                transform.LookAt(transform.position + _camera.transform.rotation * Vector3.back, _camera.transform.rotation * Vector3.up);
                //transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position, Vector3.up);
                float s = Mathf.Min(_sizeMultiplier * Vector3.Distance(transform.position, _camera.transform.position), 25f*_sizeMultiplier);
                transform.localScale = new Vector3(-s, s, s);
            }

            if (_objectToFollow)
                transform.position = _objectToFollow.position+positionCorrection;
        }

        public void SetObjectToFollow(Transform trans) 
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            _objectToFollow = trans;
        }
    }
}
