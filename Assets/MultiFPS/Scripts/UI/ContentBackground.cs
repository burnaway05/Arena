using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MultiFPS.UI
{
    public class ContentBackground : MonoBehaviour
    {
        [SerializeField] RectTransform _rectTransform;
        [SerializeField] float _marging = 5f;


        public void OnSizeChanged()
        {
            StopAllCoroutines();
            StartCoroutine(SetRect());

            IEnumerator SetRect()
            {
                yield return new WaitForEndOfFrame();
                GetComponent<RectTransform>().sizeDelta = new Vector2(_rectTransform.sizeDelta.x + _marging * 2f, _rectTransform.sizeDelta.y + _marging * 2f);
            }
        }
    }
}