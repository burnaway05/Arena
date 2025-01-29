using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.UI
{
    public class HUDItem : MonoBehaviour
    {
        [SerializeField]
        GameObject _scopeHud;

        private void Start()
        {
            _scopeHud.SetActive(false);
        }

        public virtual void Scope(bool scope)
        {
            _scopeHud.SetActive(scope);
        }

    }
}