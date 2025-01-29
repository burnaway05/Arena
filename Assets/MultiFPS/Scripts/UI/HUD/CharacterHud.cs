using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.UI.HUD
{
    /// <summary>
    /// Base class for every hud script that will display information about player
    /// </summary>
    public class CharacterHud : MonoBehaviour
    {
        CharacterInstance _myObservedCharacter;
        protected virtual void Awake()
        {
            ClientFrontend.GameEvent_OnObservedCharacterSet += OnNewCharacterObserverd;
        }

        void OnDestroy()
        {
            ClientFrontend.GameEvent_OnObservedCharacterSet -= OnNewCharacterObserverd;
        }

        private void OnNewCharacterObserverd(CharacterInstance _charInstance)
        {
            //desub from previously observed character
            if (_myObservedCharacter)
            {
                DeassignCurrentCharacterFromUI(_myObservedCharacter);
            }
            _myObservedCharacter = _charInstance;
            AssignCharacterForUI(_myObservedCharacter);
        }
        protected virtual void AssignCharacterForUI(CharacterInstance _characterInstanceToAssignForUI) 
        {

        }

        protected virtual void DeassignCurrentCharacterFromUI(CharacterInstance _characterToDeassign)
        {

        }
    }
}