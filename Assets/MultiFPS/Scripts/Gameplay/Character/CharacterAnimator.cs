using MultiFPS;
using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS
{
    [RequireComponent(typeof(CharacterInstance))]

    /// <summary>
    /// Class responsible for animating character, this means: Animating character on runtime and applying appropriate animations
    /// based on currently used item
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        CharacterInstance _characterInstance;
        CharacterItemManager _characterItemManager;

        bool fppPerspective = false;

        void Awake()
        {
            _characterInstance = GetComponent<CharacterInstance>();

            _characterItemManager = GetComponent<CharacterItemManager>();

            _characterInstance.Client_OnPerspectiveSet += OnPerspectiveChanged;
            _characterItemManager.Client_EquipmentChanged += AssingCharacterAnimationsForCurrentlyUsedItem;
        }
        private void Start()
        {
            fppPerspective = _characterInstance.FPP;
        }

        private void FixedUpdate()
        {
            if (_characterInstance.CurrentHealth > 0)
                UpdateAnimationProperties();
        }
        void OnPerspectiveChanged(bool fpp)
        {
            //dont set animator controllers twice for same perspective
            if (fppPerspective != fpp)
                AssingCharacterAnimationsForCurrentlyUsedItem(_characterItemManager.CurrentlyUsedSlotID);

            fppPerspective = fpp;
        }
        void AssingCharacterAnimationsForCurrentlyUsedItem(int slotID = -1)
        {
            Item currentlyUsedItem = slotID >= 0 ? _characterItemManager.Slots[slotID].Item : null;

            if (currentlyUsedItem)
            {
                _characterInstance.Animator.runtimeAnimatorController = _characterInstance.FPP ? currentlyUsedItem.AnimatorControllerForCharacterFPP : currentlyUsedItem.AnimatorControllerForCharacter;
            }
            else
            {
                _characterInstance.Animator.runtimeAnimatorController = _characterInstance.BaseAnimatorController;
            }

            if (_characterInstance.Animator.enabled && !_characterInstance.FPP)
                _characterInstance.Animator.Play("item_quip");
        }

        void UpdateAnimationProperties()
        {
            bool animIsGrounded = _characterInstance.isGrounded;

            _characterInstance.IsRunning = _characterInstance.ReadActionKeyCode(ActionCodes.Sprint) && _characterInstance.movementInput.y > 0 && _characterInstance.isGrounded && !_characterInstance.IsUsingItem;

            if (_characterInstance.IsRunning)
                _characterInstance.IsAbleToUseItem = false;

            float speed = (_characterInstance.IsRunning && !_characterInstance.IsCrouching ? 2f : _characterInstance.movementInput != Vector2.zero ? 1f : 0f);

            //animate character
            if (!_characterInstance.Animator.runtimeAnimatorController) return;

            _characterInstance.Animator.SetFloat(AnimationNames.ITEM_SPEED, speed); //universal parameter for fpp and tpp models

            if (!_characterInstance.FPP) //parameter for tpp character model only
            {
                _characterInstance.Animator.SetFloat(AnimationNames.CHARACTER_LOOK, -_characterInstance.lookInput.x);
                _characterInstance.Animator.SetFloat(AnimationNames.CHARACTER_MOVEMENT_HORIZONTAL, _characterInstance.movementInput.x);
                _characterInstance.Animator.SetFloat(AnimationNames.CHARACTER_MOVEMENT_VERTICAL, _characterInstance.movementInput.y);
                _characterInstance.Animator.SetBool(AnimationNames.CHARACTER_ISGROUNDED, animIsGrounded);
                _characterInstance.Animator.SetBool(AnimationNames.CHARACTER_ISCROUCHING, _characterInstance.IsCrouching);
                _characterInstance.Animator.SetLayerWeight(1, Mathf.Lerp(_characterInstance.Animator.GetLayerWeight(1),
                    System.Convert.ToInt32(!_characterInstance.IsRunning
                    || _characterInstance.IsReloading ||
                    _characterInstance.IsScoping), 12f * Time.deltaTime));
            }
        }
    }
}