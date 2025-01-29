using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.UI.HUD {
    public class HUDPickupPopup : CharacterHud
    {
        [SerializeField] GameObject _popupPrefab;
        [SerializeField] int _maxPopupsAtOnce = 8;
        [SerializeField] Transform _gridParent;

        HUDPickupPopupElement[] _popups;

        int nextPopupIDtoUse = 0;

        protected override void AssignCharacterForUI(CharacterInstance _characterInstanceToAssignForUI)
        {
            _characterInstanceToAssignForUI.Client_OnPickedupObject += PickedupObject;
        }
        protected override void DeassignCurrentCharacterFromUI(CharacterInstance _characterToDeassign)
        {
            _characterToDeassign.Client_OnPickedupObject -= PickedupObject;
        }


        protected override void Awake()
        {
            base.Awake();

            _popups = new HUDPickupPopupElement[_maxPopupsAtOnce];
            for (int i = 0; i < _maxPopupsAtOnce-1; i++)
            {
                GameObject popup = Instantiate(_popupPrefab, _gridParent);
                _popups.SetValue(popup.GetComponent<HUDPickupPopupElement>(), i);
            }

            _popups.SetValue(_popupPrefab.GetComponent<HUDPickupPopupElement>(), _maxPopupsAtOnce - 1);
        }

        void PickedupObject(string msg)
        {
            _popups[nextPopupIDtoUse].Set(msg);

            nextPopupIDtoUse++;
            if (nextPopupIDtoUse >= _popups.Length)
                nextPopupIDtoUse = 0;
        }
    }
}