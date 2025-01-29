using UnityEngine;
using MultiFPS.UI;
namespace MultiFPS.Gameplay
{
    [DisallowMultipleComponent]
    public class PlayerInput : MonoBehaviour
    {
        public static PlayerInput Instance { private set; get; }

        private CharacterInstance _character;
        private CharacterMotor _motor;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ClientFrontend.ShowCursor(false);
        }

        void Update()
        {
            if (!_character)
            {
                return;
            }

            //game managament related input
            //if (Input.GetKeyDown(KeyCode.L))
            //{
            //    if(ClientFrontend.GamemodeUI)
            //        ClientFrontend.GamemodeUI.Btn_ShowTeamSelector();
            //}

            if (ClientFrontend.GamePlayInput())
            {
                //character related input
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _motor.Jump();
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    _character.CharacterItemManager.TryGrabItem();
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    _character.CharacterItemManager.TryDropItem();
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    _character.CharacterItemManager.ClientTakeItem(0);
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    _character.CharacterItemManager.ClientTakeItem(1);
                }

                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    _character.CharacterItemManager.ClientTakeItem(2);
                }

                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    _character.CharacterItemManager.ClientTakeItem(3);
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    _character.CharacterItemManager.Reload();
                }
                
                if (Input.GetKey(KeyCode.V) && _character.CharacterItemManager.CurrentlyUsedItem) 
                    _character.CharacterItemManager.CurrentlyUsedItem.PushMeele();

                _character.SetActionKeyCode(ActionCodes.Trigger2, Input.GetMouseButton(1));
                _character.SetActionKeyCode(ActionCodes.Trigger1, Input.GetMouseButton(0));

                if (!_character.Block)
                {
                    _character.movementInput.x = Input.GetAxis("Horizontal");
                    _character.movementInput.y = Input.GetAxis("Vertical");
                }
                else
                {
                    _character.movementInput = Vector2.zero;
                }
                
                _character.lookInput.y += Input.GetAxis("Mouse X") * UserSettings.MouseSensitivity * _character.SensitivityItemFactorMultiplier;
                _character.lookInput.x -= Input.GetAxis("Mouse Y") * UserSettings.MouseSensitivity * _character.SensitivityItemFactorMultiplier;

                //_character.SetActionKeyCode(ActionCodes.Sprint, Input.GetKey(KeyCode.LeftShift));// && !Input.GetMouseButton(0) && !Input.GetMouseButton(1);
                //_character.SetActionKeyCode(ActionCodes.Crouch, Input.GetKey(KeyCode.LeftControl));
            }
            else
            {
                _character.movementInput = Vector2.zero;
                _character.SetActionKeyCode(ActionCodes.Sprint, false);
            }
        }

        public void SetCharacter(CharacterInstance character)
        {
            _character = character;
            _motor = character.GetComponent<CharacterMotor>();
        }
    }
}