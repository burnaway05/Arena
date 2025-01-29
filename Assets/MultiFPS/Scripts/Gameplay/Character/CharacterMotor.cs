using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MultiFPS.Gameplay
{
    /// <summary>
    /// Script responsible for character movement
    /// </summary>
    [RequireComponent(typeof(CharacterInstance))]
    public class CharacterMotor : MonoBehaviour
    {
        CharacterInstance _charInstance;
        CharacterController _controller;

        public float WalkSpeed = 5f;
        public float RunSpeed = 10f;
        public float CrouchSpeed = 10f;
        public float JumpHeight;
        public float Gravity = 10f;
        [SerializeField, Range(0f, 500f)]
        float maxAcceleration = 10f;
        public float CameraLerpSpeed = 2f;
        float _speed;

        Vector3 force;
        bool _jumped;

        Vector3 defaultAttackPos;
        Vector3 crouchAttackPos = new Vector3(0,0.7f,0);

        Vector3 _velocity;

        #if UNITY_EDITOR
        bool _noclip = false;
        [SerializeField] float noclipSpeed = 10;
        [SerializeField] float noclipRunSpeed = 30;
        #endif

        public Vector3 cameraTargetPosition;

        bool isGrounded;

        void Awake()
        {
            _charInstance = GetComponent<CharacterInstance>();
            _controller = GetComponent<CharacterController>();

            defaultAttackPos = _charInstance.centerPosition;

            Stand();
        }

        void Update()
        {
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.C))
                _noclip = !_noclip;
            #endif

            if (_charInstance.FPPLook.localPosition != cameraTargetPosition)
            {
                _charInstance.FPPLook.localPosition = Vector3.MoveTowards(_charInstance.FPPLook.localPosition, cameraTargetPosition, CameraLerpSpeed * Time.deltaTime);
            }
        }
        private void FixedUpdate()
        {
            if (!_charInstance.IsCrouching)
            {
                if (_charInstance.ReadActionKeyCode(ActionCodes.Crouch) && _charInstance.isGrounded && !CheckSphere())
                {
                    Crouch();
                    _charInstance.IsCrouching = true;
                }
            }
            else
            {
                if (!_charInstance.ReadActionKeyCode(ActionCodes.Crouch) && !CheckSphere())
                {
                    Stand();
                    _charInstance.IsCrouching = false;
                }
            }

            //if(isOwned || (isServer && _charInstance.BOT))
                MovementTick();

            _charInstance.isGrounded = Physics.CheckSphere(transform.position + new Vector3(0, 0.3f, 0), 0.5f, GameManager.environmentLayer);
        }

        public void MovementTick() 
        {
            #if UNITY_EDITOR
            if (_noclip /* && isOwned*/) 
            {
                _charInstance.PrepareCharacterToLerp();
                Vector3 noclipInput = new Vector3(_charInstance.movementInput.x, (Input.GetKey(KeyCode.LeftControl)? -1: Input.GetKey(KeyCode.Space)? 1f: 0), _charInstance.movementInput.y);
                noclipInput = _charInstance.FPPCameraTarget.rotation * noclipInput;
                float speed = _charInstance.ReadActionKeyCode(ActionCodes.Sprint) ? noclipRunSpeed : noclipSpeed;
                noclipInput = noclipInput * speed;
                transform.position += noclipInput * Time.fixedDeltaTime;
                _charInstance.SetCurrentPositionTargetToLerp(transform.position);
                return;
            }
            #endif


            //if (!isOwned && !_charInstance.BOT) return;

            if (_charInstance.CurrentHealth <= 0) return;

            _charInstance.PrepareCharacterToLerp();

            //decide character speed
            if (isGrounded)
            {
                //sprint/walk speed
                bool isRunning = (_charInstance.ReadActionKeyCode(ActionCodes.Sprint) && isGrounded && !_charInstance.IsUsingItem && _charInstance.movementInput.y > 0);
                _speed = _charInstance.IsCrouching? CrouchSpeed: (isRunning ? RunSpeed : WalkSpeed);
            }
            else
            {
                //dont let character be as fast when falling, as if it was running
                _speed = Mathf.Clamp(_speed, 0f, RunSpeed * 0.8f);
            }

            //get input and, make vector from that and, multiply it by speed and give it appropriate direction based on character rotation
            //Vector2 playerInput2D = new Vector3(_charInstance.movementInput.x, _charInstance.movementInput.y);
            //playerInput2D.Normalize();
            //Vector3 playerInput = new Vector3(playerInput2D.x, 0, playerInput2D.y);
            Vector3 playerInput = new Vector3(_charInstance.movementInput.x, 0, _charInstance.movementInput.y);

            playerInput = _speed * playerInput;
            playerInput = transform.rotation * playerInput; //set movement direction  player rotation


            if (isGrounded)
            {
                //if character jumped dont treat it as if it was grounded
                if (!_jumped)
                    force.y = -0.7f;
            }
            else
            {
                //when not grounded make player fall
                force.y -= Gravity * Time.fixedDeltaTime;
            }

             float maxSpeedChange = maxAcceleration * Time.deltaTime;

             _velocity.x = Mathf.MoveTowards(_velocity.x, playerInput.x, maxSpeedChange);
             _velocity.z = Mathf.MoveTowards(_velocity.z, playerInput.z, maxSpeedChange);


            //finally move character
            //this additional isGrounded check is for avoiding character controller to think that it is not grounded while player is going down stairs fast
            _controller.Move(Time.fixedDeltaTime * (_velocity + force) + (isGrounded ? new Vector3(0,-0.25f,0) : Vector3.zero));
            _jumped = false;

            isGrounded = _controller.isGrounded;

            _charInstance.SetCurrentPositionTargetToLerp(transform.position);
        }

        void Crouch() 
        {
            _controller.center = new Vector3(0, 0.5f, 0);
            _controller.height = 1f;

            _charInstance.centerPosition = crouchAttackPos;

            LerpCamera(1f);
        }
        void Stand() 
        {
            _controller.center = new Vector3(0,1,0);
            _controller.height = 2f;

            _charInstance.centerPosition = defaultAttackPos;

            LerpCamera(_charInstance.CameraHeight);
        }

        void LerpCamera(float cameraHeight) 
        {
            cameraTargetPosition = new Vector3(0, cameraHeight, 0);
        }

        public void Jump() 
        {
            if (isGrounded && !CheckSphere())
            {
                isGrounded = false;
                force.y = Mathf.Sqrt(JumpHeight *2f*Gravity);
                _jumped = true;
            }
        }

        bool CheckSphere() 
        {
            Collider[] col = Physics.OverlapSphere(transform.position + new Vector3(0, 1.5f, 0), 0.4f,GameManager.environmentLayer);

            return col.Length > 0;
        }
        /// <summary>
        /// Dont let character move when is dead
        /// </summary>
        public void Die(byte _hittedPartID, uint _attackerID)
        {
            enabled = false;
        }
    }
}
