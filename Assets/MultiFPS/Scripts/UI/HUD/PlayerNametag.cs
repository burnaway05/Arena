using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiFPS.Gameplay;
using UnityEngine.UI;
namespace MultiFPS.UI.HUD
{
    public class PlayerNametag : UIWorldIcon
    {
        public Text namePlaceholder;
        public Image healthbar;

        CharacterInstance myCharacter;

        public void SetupNameplate(CharacterInstance _myCharacter) 
        {
            _myCharacter.Client_HealthStateChanged += OnPlayerHealthStateChanged;
            myCharacter = _myCharacter;

            namePlaceholder.text = myCharacter.CharacterName;

            InitializeWorldIcon(myCharacter.CharacterMarkerPosition, false);
        }
        void OnPlayerHealthStateChanged(int _currentHealth, byte _hittedPartID, AttackType attackType, uint _attackerID) 
        {
            healthbar.fillAmount = (float)_currentHealth/myCharacter.MaxHealth;
        }

    }
}