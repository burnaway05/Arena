using MultiFPS;
using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiFPS.UI
{
    public class Crosshair : MonoBehaviour
    {
        [SerializeField] int _maxSizeInPixels = 128;
        [SerializeField] int _minSizeInPixels = 32;

        private int _imageDefaultSize = 64;

        float _targetSize;

        [SerializeField] Image _horizontalCrosshair;
        [SerializeField] Image _verticalCrosshair;
        [SerializeField] Image _dotCrosshair;

        public static Crosshair Instance;

        private CharacterInstance _myPlayer;
        private Item _myItem;

        public void Setup()
        {
            ClientFrontend.GameEvent_OnObservedCharacterSet += ObserveCharacter;
        }
        private void Start()
        {
            _targetSize = _minSizeInPixels;
        }
        private void OnDestroy()
        {
            ClientFrontend.GameEvent_OnObservedCharacterSet -= ObserveCharacter;
        }

        // Update is called once per frame
        void Update()
        {
            _horizontalCrosshair.rectTransform.sizeDelta = new Vector2(_targetSize, _imageDefaultSize);
            _verticalCrosshair.rectTransform.sizeDelta = new Vector2(_targetSize, _imageDefaultSize);

            if (_myItem)
            {
                _minSizeInPixels = Mathf.FloorToInt(_myPlayer.RecoilFactor_Movement * _myItem._currentRecoilScopeMultiplier * _myItem.minSize);
                _maxSizeInPixels = Mathf.FloorToInt(_myPlayer.RecoilFactor_Movement * _myItem._currentRecoilScopeMultiplier * _myItem.maxSize);

                _targetSize = _maxSizeInPixels * (_myItem.CurrentRecoil / _myItem._recoil_maxAngle) + _minSizeInPixels;
            }

            bool showCrosshair = _myItem ? (_myItem.HideWhenAiming ? !_myPlayer.IsScoping: true) : false;
            _horizontalCrosshair.enabled = showCrosshair;
            _verticalCrosshair.enabled = showCrosshair;
            _dotCrosshair.enabled = showCrosshair;
        }

        void ObserveCharacter(CharacterInstance characterInstance)
        {
            if (_myPlayer)
            {
                _myPlayer.CharacterItemManager.Client_EquipmentChanged -= ObserveItem;
            }

            _myPlayer = characterInstance;
            _myPlayer.CharacterItemManager.Client_EquipmentChanged += ObserveItem;

            ObserveItem(_myPlayer.CharacterItemManager.CurrentlyUsedSlotID);
        }
        void ObserveItem(int currentSlot)
        {
            if (_myItem)
                _myItem = null;

            _myItem = _myPlayer.CharacterItemManager.CurrentlyUsedItem;

            if (_myItem)
            {
                _minSizeInPixels = _myItem.minSize;
                _maxSizeInPixels = _myItem.maxSize;
            }
        }
    }
}
