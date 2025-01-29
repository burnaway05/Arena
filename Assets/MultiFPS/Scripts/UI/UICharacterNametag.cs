using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiFPS.UI.HUD
{
    public class UICharacterNametag : UIWorldSpaceElement
    {
        [SerializeField] Text _usernameRenderer;
        [SerializeField] Image _usernameRenderer_frame;
        [SerializeField] Image _healthBarRenderer;
        [SerializeField] Image _markerRenderer;
       // [SerializeField] ContentBackground _textBackground;

        CharacterInstance _myCharInstance;

        bool _spawned;


        public void Set(CharacterInstance characterInstance)
        {
            _myCharInstance = characterInstance;

            _usernameRenderer.text = _myCharInstance.CharacterName;

            Color color = ClientInterfaceManager.Instance.UIColorSet.AppropriateColorAccordingToTeam(_myCharInstance.Team);

            _usernameRenderer.color = color;
            _usernameRenderer_frame.color = color;
            _healthBarRenderer.color = color;
            _markerRenderer.color = color;

            SetObjectToFollow(_myCharInstance.CharacterMarkerPosition);

            _myCharInstance.Client_HealthDepleted += OnMyCharacterDeath;
            _myCharInstance.Client_HealthStateChanged += OnMyCharacterDamaged;
            _myCharInstance.Client_OnDestroyed += DespawnMe;

            _spawned = true;

            _healthBarRenderer.fillAmount = (float)_myCharInstance.CurrentHealth / _myCharInstance.MaxHealth;
        }

        void OnMyCharacterDamaged(int _currentHealth, byte _hittedPartID, AttackType attackType, uint _attackerID) 
        {
                _healthBarRenderer.fillAmount = (float)_currentHealth / _myCharInstance.MaxHealth;
        }

        void OnMyCharacterDeath(byte _hittedPartID, uint _attackerID)
        {
            DespawnMe();
        }

        public void DespawnMe() 
        {
            if (!_spawned) return;

            _spawned = false;
            _myCharInstance.Client_HealthDepleted -= OnMyCharacterDeath;
            _myCharInstance.Client_HealthStateChanged -= OnMyCharacterDamaged;
            _myCharInstance.Client_OnDestroyed -= DespawnMe;
            Destroy(gameObject);
        }
        private void OnDestroy()
        {
            _spawned = false;
        }
    }
}
