using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.Gameplay
{
    /// <summary>
    /// It's only responsible for visual camera recoil (on shooting and receiving damage)
    /// Weapons spray is calculated in weapons classes
    /// </summary>
    public class PlayerRecoil : MonoBehaviour
    {
        [SerializeField] Transform _recoilObject;
        Coroutine _CurrentRecoilCoroutine;

        public void Initialize(Transform recoilObject, CharacterInstance characterInstance)
        {
            if (recoilObject)
                _recoilObject = recoilObject;

            //listen to event when player receives damage to shake camera a little in such case
            characterInstance.Client_HealthStateChanged += OnReceivedDamage;
        }

        public void RecoilReset()
        {
            _recoilObject.rotation = Quaternion.identity;
        }
        public void Recoil(float _recoil, float _devation, float _speed,float _duration)
        {
            if (_CurrentRecoilCoroutine != null)
            {
                StopCoroutine(_CurrentRecoilCoroutine);
                _CurrentRecoilCoroutine = null;
            }
            _CurrentRecoilCoroutine = StartCoroutine(DoRecoil(_recoil, _devation, _speed, _duration));
        }
        IEnumerator DoRecoil(float recoilVertical, float recoilHorizontal, float speed, float duration)
        {
            Quaternion recoilRot = Quaternion.Euler(_recoilObject.localEulerAngles.x -recoilVertical, _recoilObject.localEulerAngles.y + recoilHorizontal, 0);
            float timer = 0f;

            bool doingRecoil = true;
            bool recoilDone = false;

            float comingBackDuration = 3 * duration;

            while (doingRecoil)
            {
                yield return null;

                if (!recoilDone)
                {
                    timer += Time.deltaTime;

                    if (timer < duration)
                        _recoilObject.localRotation = Quaternion.Slerp(_recoilObject.localRotation, recoilRot, (timer / duration));
                    else 
                    {
                        recoilDone = true;
                        timer = 0f;
                    }
                }
                else
                {
                    timer += Time.deltaTime;

                    if (timer < comingBackDuration)
                        _recoilObject.localRotation = Quaternion.Slerp(_recoilObject.localRotation, Quaternion.identity, (timer/comingBackDuration));
                    else
                    {
                        doingRecoil = false;
                        _recoilObject.localRotation = Quaternion.identity;
                    }
                }
            }
        }

        public void OnReceivedDamage(int _currentHealth, byte _hittedPartID, AttackType attackType, uint _attackerID)
        {
            float deviation = 0.55f;
            Recoil(Random.Range(-deviation, deviation), Random.Range(-deviation, deviation), 8, 0.06f);
        }
    }
}
