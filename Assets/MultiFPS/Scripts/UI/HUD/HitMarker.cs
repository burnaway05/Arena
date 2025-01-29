using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MultiFPS.Gameplay;

namespace MultiFPS.UI.HUD
{
    /// <summary>
    /// Script for playing hitmarker when currently spectated player damages something
    /// </summary>
    public class HitMarker : MonoBehaviour
    {
        public static HitMarker _instance;
        [SerializeField] AudioSource _audioSource;
        [SerializeField] AudioClip hitMarkerClip;

        [Header("Hitmarker Animation properties")]
        [SerializeField] Image hitmarkerPrefab; //object with hitmarker image
        [SerializeField] Transform hitmarkerParents; //transform that will contain all spawned hitmarkers images

        public float StartScale = 0.7f;
        public float EndScale = 0.3f;
        public float ChangeSizeSpeed = 0.04f;

        public float VanishStartTime = 0.5f; //amount of time that has to pass to start vanishing hitmarker
        public float VanishingTime = 0.3f; //how long it will take to vanish hitmarker after time specified above will pass since hitmarker launch

        public int MaxHitmarkersAtOnce = 10;

        //to make hitmarker look nicer, we will have more than one of them, to avoid situations when we damage something fast so hitmarker that was playing is
        //immediately reused to play another one, so instead we have a couple of them so they can overlap each other and play simultaneously
        private Image[] _hitMarkers;

        private int _currentlyUsedHitmarker = 0;

        Vector3 _startScale;
        Vector3 _endScale;

        private void Awake()
        {
            _instance = this;
            
            _audioSource = GetComponent<AudioSource>();

            _hitMarkers = new Image[MaxHitmarkersAtOnce];

            for (int i = 0; i < MaxHitmarkersAtOnce; i++)
            {
                Image hitmarker = Instantiate(hitmarkerPrefab.gameObject, hitmarkerParents).GetComponent<Image>();
                _hitMarkers.SetValue(hitmarker, i);
                hitmarker.color = Color.clear;
            }

            hitmarkerPrefab.gameObject.SetActive(false);

            _endScale = new Vector3(EndScale, EndScale, EndScale);
            _startScale = new Vector3(StartScale, StartScale, StartScale); 
        }

        public void PlayHitMarker(CharacterPart hittedPart)
        {
            if (_currentlyUsedHitmarker >= MaxHitmarkersAtOnce) 
                _currentlyUsedHitmarker = 0;

            StartCoroutine(HitmarkerAnimation(_hitMarkers[_currentlyUsedHitmarker], hittedPart));

            _currentlyUsedHitmarker++;

            _audioSource.PlayOneShot(hitMarkerClip);

        }
        IEnumerator HitmarkerAnimation(Image hitmarkerToAnimate, CharacterPart hittedPart)
        {
            hitmarkerToAnimate.transform.SetAsLastSibling();

            hitmarkerToAnimate.color = hittedPart == CharacterPart.head ? Color.red : Color.white;
            hitmarkerToAnimate.transform.localScale = _startScale;
            float timer = 0f;
            while (timer <= VanishStartTime+VanishingTime)
            {
                timer += Time.deltaTime;
                if (timer > VanishStartTime)
                {
                    float vanishProgress = ((timer - VanishStartTime) / VanishingTime);
                    hitmarkerToAnimate.color = Color.Lerp(hitmarkerToAnimate.color, Color.clear, vanishProgress);
                    //print($"hm: time: {timer}, vanish progress: { vanishProgress}");
                }

                hitmarkerToAnimate.transform.localScale = Vector3.Lerp(hitmarkerToAnimate.transform.localScale, _endScale, ChangeSizeSpeed * Time.deltaTime);

                yield return null;
            }
        }
    }
}
