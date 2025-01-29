using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiFPS.UI.HUD
{
    public class HUDPickupPopupElement : MonoBehaviour
    {

        [SerializeField] Text _text;

        [SerializeField]ContentBackground _textBackground;

        Coroutine _livetime;
        private void Awake()
        {
            gameObject.SetActive(false);
        }
        public void Set(string msg)
        {
            gameObject.SetActive(true);

            if (_livetime != null)
                StopCoroutine(_livetime);

            _text.text = msg;

            _textBackground.OnSizeChanged();

            _livetime = StartCoroutine(LiveTime());

            IEnumerator LiveTime()
            {
                yield return new WaitForSeconds(5f);
                gameObject.SetActive(false);
            }
        }
    }
}