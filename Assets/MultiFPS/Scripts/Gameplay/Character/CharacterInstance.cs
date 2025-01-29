 using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MultiFPS.UI.HUD;
using MultiFPS.Gameplay.Gamemodes;
using MultiFPS.UI;

namespace MultiFPS.Gameplay {

    /// <summary>
    /// This component is responsible for managing and animating character
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class CharacterInstance : Health
    {
        [Header("Character setup")]
        //determines point to which we will attach player marker with nickname and healthbar
        public Transform CharacterMarkerPosition;

        //Model of character, we need to have access to it in order to lerp it's position beetwen positions received from the server
        public Transform CharacterParent;

        public Transform FPPLook;

        //Transform that player's camera will stick to, since Camera is external object not included in player prefab, must be children of
        //FPPLook (look above) to account for camera recoil produced with shooting from weapons
        public Transform FPPCameraTarget; 

        public float CameraHeight = 1.7f;

        //weapons recoil is affected by player movement. This variable determines how fast it will transition beetwen states,
        //Given in deegrees for second
        public float RecoilMovementFactorChangeSpeed = 4f;

        /// <summary>
        /// player model animator
        /// </summary>
        [SerializeField] Animator CharacterAnimator;
        [SerializeField] Animator FppModelAnimator;
        [SerializeField] SkinnedMeshRenderer[] _characterMeshes;
        [SerializeField] SkinnedMeshRenderer _fppCharacterMesh;


        [HideInInspector] public Animator Animator;

        public RuntimeAnimatorController BaseAnimatorController;
        public RuntimeAnimatorController BaseAnimatorControllerFPP;


        public delegate void KilledCharacter(Health health);
        public KilledCharacter Server_KilledCharacter;

        public delegate void CharacterEvent_SetAsBOT(bool _set);
        public CharacterEvent_SetAsBOT Server_SetAsBOT;

        public Transform characterMind; //this objects indicated direction character is looking at
        public Transform characterFirePoint; //this object is child of characterMind, will be used for recoil of weapons

        /// <summary>
        /// Hitbox prefab to assign to player model
        /// </summary>
        [SerializeField] GameObject _hitBoxContainerPrefab;
        HitboxSetup _hitboxes;

        [HideInInspector] public PlayerRecoil PlayerRecoil { private set; get; }
        [HideInInspector] public ToolMotion ToolMotion;

        
        public bool IsReloading; //this will be driven by synchronized events
        public bool IsCrouching;
        public bool IsUsingItem;
        public bool IsRunning;
        public bool IsScoping;
        public bool IsAbleToUseItem = true;
        public bool isGrounded;

        public float RecoilFactor_Movement = 1;
        public float SensitivityItemFactorMultiplier = 1f;

        public bool Block { private set; get; } = false; //determines if character can move and shoot or not, block if it is end of round

        /// <summary>
        /// Only true for character that is controlled by client, so only for player controller
        /// </summary>
        public bool IsObserved { set; get; }

        /// <summary>
        /// Flag that informs us if player is set up to be viewed in 1st or 3rd person
        /// </summary>
        public bool FPP = false;

        /// <summary>
        /// Indicates if character is controlled by server or client
        /// </summary>
        public bool BOT = false;

        Health _killer;

        [HideInInspector] public CharacterItemManager CharacterItemManager;

        [Header("Player/Bot input")]
        #region input to synchronize
        public Vector2 lookInput; //mouse 
        public Vector2 movementInput; //WSAD
        byte _actionCode;
        #endregion

        [HideInInspector] public Transform ObjectForDeathCameraToFollow;
        public SkinContainer MySkin { private set; get; }

        Vector3 _deathCameraDirection;

        #region smooth rotation and position
        [Header("Smooth position lerp")]
        float _lastSyncTime;
        float _previousTickDuration;

        float _rotationSmoothTimer;

        float _currentRotationTargetX;
        float _currentRotationTargetY;

        Vector3 _lastPositionSync;
        Vector3 _currentPositionSync;

        float lastYPos;

        public float PositionLerpSpeed = 10f;
        #endregion

        #region for falldamage
        float _startFallingPointY;
        bool _falling;
        #endregion

        //information about skins that players selected for his items
        [HideInInspector] public int[] _skinsForItems;

        public delegate void CharacterEvent_OnPerspectiveSet(bool fpp);
        public CharacterEvent_OnPerspectiveSet Client_OnPerspectiveSet;

        public delegate void CharacterEvent_OnPickedupObject(string message);
        public CharacterEvent_OnPickedupObject Client_OnPickedupObject { get; set; }

        private void Awake()
        {
            Animator = CharacterAnimator;

            PlayerRecoil = GetComponent<PlayerRecoil>();
            if (!PlayerRecoil)
                PlayerRecoil = gameObject.AddComponent<PlayerRecoil>();

            PlayerRecoil.Initialize(FPPCameraTarget, this);

            //Update 1.1 Moved to health script
            CustomSceneManager.RegisterCharacter(this);//we have to register spawned characters in order to let bot "see" them, and select nearest enemies from that register

            //Server_HealthDepleted += ServerDeath;
            //Client_HealthStateChanged += ClientOnHealthStateChanged;

            CharacterItemManager = GetComponent<CharacterItemManager>();
            CharacterItemManager.Setup();

            //SetFppPerspective(false);
        }
        protected override void Start()
        {
            base.Start();

            _fppCharacterMesh.gameObject.layer = (int)GameLayers.fppModels;

            _lastPositionSync = transform.position;
            _currentPositionSync = transform.position;

            if (_hitBoxContainerPrefab)
            {
                _hitboxes = Instantiate(_hitBoxContainerPrefab, transform.position, transform.rotation).GetComponent<HitboxSetup>();
                _hitboxes.SetHiboxes(CharacterAnimator.gameObject, this);
            }

            GameTicker.Game_Tick += CharacterInstance_Tick;

            lookInput.y = transform.eulerAngles.y; //assigning start look rotation to spawnpoint rotation

            gameObject.layer = 6; //setting apppropriate layer for character collisions
            GameManager.SetLayerRecursively(CharacterAnimator.gameObject, 8);

            PlayerGameplayInput.Instance.AssignCharacterToBeControlledByPlayer(this);
            //ObserveCharacter(isOwned);

            //if (isServer)
            //{
            //    CharacterItemManager.SpawnStarterEquipment();
            //    CharacterItemManager.ServerCommandTakeItem(0);
            //}
        }
        void Update()
        {
            if (CharacterItemManager.CurrentlyUsedItem)
            {
                float recoilMultiplier = !isGrounded ? 2.5f : (movementInput.x != 0 || movementInput.y != 0) ? CharacterItemManager.CurrentlyUsedItem.Recoil_walkMultiplier : 1f;
                RecoilFactor_Movement = Mathf.Lerp(RecoilFactor_Movement, recoilMultiplier, RecoilMovementFactorChangeSpeed * Time.deltaTime);
            }
            else
                RecoilFactor_Movement = 1f;

            if (Block)
                movementInput = Vector2.zero;

            /*
            //3rd person camera for testing
            if (Input.GetKeyDown(KeyCode.I)&&isOwned)
            {
                SetFppPerspective(!FPP);
                GetComponent<CharacterMotor>().cameraTargetPosition = new Vector3(0.55f, 2.1f, -3f);
            }
            */

            if (ReadActionKeyCode(ActionCodes.Trigger1))
                CharacterItemManager.Fire1();
            if (ReadActionKeyCode(ActionCodes.Trigger2))
                CharacterItemManager.Fire2();


            #region observer smooth rotation
            if (!_killer)
            {
                _rotationSmoothTimer += Time.deltaTime;
                float percentage = Mathf.Clamp(_rotationSmoothTimer / _previousTickDuration, 0, 1);

                Vector3 positionForThisFrame = Vector3.Lerp(_lastPositionSync, _currentPositionSync, percentage);
                CharacterParent.position = positionForThisFrame;

                //CharacterParent.position = Vector3.Lerp(CharacterParent.position, _currentPositionSync, Time.deltaTime * PositionLerpSpeed);
                // CharacterParent.position = positionForThisFrame;

                //if (isOwned || isServer && BOT)
                //{
                //    lookInput.x = Mathf.Clamp(lookInput.x, -90f, 90f);
                //
                //    //rotate character based on player mouse input/bot input
                //    if (CurrentHealth > 0)
                //        transform.rotation = Quaternion.Euler(0, lookInput.y, 0);
                //    //rotate camera based on player mouse input/bot input
                //    FPPLook.localRotation = Quaternion.Euler(lookInput.x, 0, 0);
                //}
                //else
                //{
                //    FPPLook.transform.localRotation = Quaternion.Lerp(FPPLook.transform.localRotation, Quaternion.Euler(_currentRotationTargetX, 0, 0), percentage);
                //    if (CurrentHealth > 0)
                //        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, _currentRotationTargetY, 0), percentage);
                //}
            }
            #endregion

            if (!IsObserved) return;

            #region killcam
            if (_killer)
                FPPLook.rotation = Quaternion.Lerp(FPPLook.rotation, Quaternion.LookRotation(_killer.GetPositionToAttack() - FPPLook.position), 10f * Time.deltaTime);

            if (!ObjectForDeathCameraToFollow) return;

            RaycastHit hit;
            Vector3 castPosition = ObjectForDeathCameraToFollow.position + Vector3.up * 0.1f;

            float length;
            if (Physics.Raycast(castPosition, _deathCameraDirection, out hit, 5f, GameManager.environmentLayer))
            {
                length = Mathf.Max(0, Vector3.Distance(hit.point, castPosition) - 0.5f);
            }
            else
                length = 5f;

            FPPLook.transform.position = castPosition + _deathCameraDirection * length + transform.up*0.2f;
            #endregion
        }

        private void FixedUpdate()
        {
            if (Block)
                movementInput = Vector2.zero;

            //server authoritative fall damage
            //if (!isServer) 
            //    return;

            //count fall damage
            if (!isGrounded)
            {
                if (!_falling)
                {
                    _falling = true;
                    _startFallingPointY = transform.position.y;
                    lastYPos = _startFallingPointY + 1f;
                }
                else 
                {
                    if (transform.position.y >= lastYPos)
                    {
                        _falling = false;
                    }

                    lastYPos = transform.position.y;
                }

                if (_startFallingPointY < transform.position.y)
                    _startFallingPointY = transform.position.y;
            }
            else
            {
                if (_falling)
                {
                    float distance = _startFallingPointY - transform.position.y;
                    if (distance > 5.5f) //start applying damage on from drop 5,5 m
                    {
                        //Server_ChangeHealthState(Mathf.FloorToInt(distance * distance * 0.5f), 0, AttackType.falldamage, netId, 0);
                    }
                    _falling = false;
                }
            }
        }

        #region position lerp
        public void PrepareCharacterToLerp() 
        {
            _lastPositionSync = CharacterParent.position;
        }
        public void SetCurrentPositionTargetToLerp(Vector3 target) 
        {
            CharacterParent.position = _lastPositionSync;

            _currentPositionSync = target;


            _previousTickDuration = Time.time - _lastSyncTime;
            _lastSyncTime = Time.time;

            _rotationSmoothTimer = 0f;
        }
        #endregion

        public bool IsClientOrBot() { return /*isOwned || isServer &&*/ BOT; }
            

        #region input networking
        void CharacterInstance_Tick()
        {
            //if (isOwned)
            //{
            //    ClientSendInput(movementInput, lookInput, _actionCode, transform.position);
            //}
            //else if (BOT && isServer)
            //{
            //    RpcReceiveInputFromServer(movementInput, lookInput, _actionCode, transform.position);
            //}
        }
        //void ClientSendInput(Vector2 movement, Vector2 look, byte actionCode, Vector3 position)
        //{
        //    CmdReceiveInputFromClient(movement, look, actionCode, position);
        //}

        //[Command]
        //void CmdReceiveInputFromClient(Vector2 movement, Vector2 look, byte actionCode, Vector3 position)
        //{
        //    //if (!isOwned)
        //        ApplyTick(movement, look, actionCode, position);
        //
        //    RpcReceiveInputFromServer(movement, look, actionCode, position);
        //}

        /// <summary>
        /// send player input to every client except client who sent it, so we can rotate character correcly and 
        /// play appropriate animations for other clients
        /// </summary>
        //[ClientRpc(includeOwner = false, channel = Channels.Unreliable)] 
        //void RpcReceiveInputFromServer(Vector2 movement, Vector2 look, byte actionCode, Vector3 position)
        //{
        //    //if (isServer) return;
        //
        //    ApplyTick(movement, look, actionCode, position);
        //}

        //read and apply received data
        //void ApplyTick(Vector2 movement, Vector2 look, byte actionCode, Vector3 position) 
        //{
        //    PrepareCharacterToLerp();
        //
        //    movementInput = movement;
        //    lookInput = look;
        //    _actionCode = actionCode;
        //
        //    _currentRotationTargetX = look.x;
        //    _currentRotationTargetY = look.y;
        //
        //    transform.position = position;
        //
        //    SetCurrentPositionTargetToLerp(position);
        //}
        #endregion
        //public void ObserveCharacter(bool _observe)
        //{
        //    if (_observe)
        //        GameplayCamera._instance.SetTarget(FPPCameraTarget);
        //}


        //private void ServerDeath(byte hittedPartID, AttackType attackType, uint attackerID, int attackForce) 
        //{
        //    Health killer = GameManager.GetHealthInstance(attackerID);
        //    if (killer)
        //    {
        //        CharacterInstance killerChar = killer.GetComponent<CharacterInstance>();
        //
        //        if (killerChar)
        //        {
        //            killerChar.Server_KilledCharacter?.Invoke(this);
        //        }
        //    }
        //
        //    //GameManager.Gamemode.Server_OnPlayerKilled(netId, attackerID);
        //    GetComponent<CharacterController>().enabled = false;
        //}
        //public void ClientOnHealthStateChanged(int currentHealth, byte hittedPartID, AttackType attackType, uint attackerID)
        //{
        //    if (!FPP)
        //        CharacterAnimator.Play("onDamaged");
        //
        //    if (currentHealth > 0) return;
        //
        //    //Set camera to follow killer
        //    if (IsObserved)
        //    {
        //        GameplayCamera._instance.SetFieldOfView(UserSettings.UserFieldOfView);
        //
        //        _killer = GameManager.GetHealthInstance(attackerID);
        //
        //        //_deathCameraDirection = _killer.netId != netId ? (FPPLook.transform.position - _killer.GetPositionToAttack()).normalized : -FPPLook.forward;
        //
        //
        //        CharacterInstance killedByPlayer = _killer.GetComponent<CharacterInstance>();
        //
        //        //if (killedByPlayer && (attackerID != netId)) //dont show this message in case of suicide
        //        //    GameManager.GameEvent_GamemodeEvent_Message?.Invoke("You were killed by " + killedByPlayer.CharacterName, RoomSetup.Properties.P_RespawnCooldown);
        //    }
        //
        //    _hitboxes.DisableHitboxes();
        //    GetComponent<CharacterMotor>().Die(hittedPartID, attackerID); //disable movement for dead character
        //
        //    GetComponent<CharacterController>().enabled = false;
        //
        //    GameManager.SetLayerRecursively(CharacterAnimator.gameObject, 9); //set ragdoll layer
        //    //GameManager.Gamemode.Rpc_OnPlayerKilled(netId, (CharacterPart)hittedPartID, attackType,  attackerID); //killfeed listens to this event
        //}


        //public void BlockCharacter(bool block) 
        //{
        //    Block = block;
        //    //RpcBlockCharacter(block);
        //}
        //[ClientRpc]
        //private void RpcBlockCharacter(bool block) 
        //{
        //    Block = block;
        //}

        //public void SetFppPerspective(bool fpp) 
        //{
        //    //we cannot just disable character model, because it has hitboxes so host
        //    //would be immortal to bots
        //    foreach (SkinnedMeshRenderer mesh in _characterMeshes)
        //    {
        //        //if character is dead, then player model should not be reenabled, 
        //        //because it is replaced by ragdoll prefab
        //        mesh.enabled = !fpp && CurrentHealth > 0;
        //    }
        //
        //    FppModelAnimator.gameObject.SetActive(fpp);
        //
        //    Animator = fpp ? FppModelAnimator : CharacterAnimator;
        //
        //    FPP = fpp;
        //
        //    Client_OnPerspectiveSet?.Invoke(fpp);
        //}

        public delegate void OnDestroyed();
        public OnDestroyed Client_OnDestroyed;

        #region input
        public bool ReadActionKeyCode(ActionCodes actionCode)
        {
            return (_actionCode & (1 << (int)actionCode)) != 0;
        }
        public void SetActionKeyCode(ActionCodes actionCode, bool _set)
        {
            int a = _actionCode;
            if (_set)
            {
                a |= 1 << ((byte)actionCode);
            }
            else
            {
                a &= ~(1 << (byte)actionCode);
            }
            _actionCode = (byte)a;
        }
        #endregion

        public void SetAsBOT(bool _set)
        {
            //if turn bot to player, game will try to teleport him to his spawnpoint because server does not write these values
            //for bots every single tick
            if (BOT)
            {
                _lastPositionSync = transform.position;
                _currentPositionSync = transform.position;
            }

            BOT = _set;

            Server_SetAsBOT?.Invoke(_set);
        }

        public void ApplySkin(int skindID)
        {
            //apply skin for character, materials and meshed for both FPP hands model and TPP character model
            MySkin = ClientInterfaceManager.Instance.characterSkins[skindID];
            if (!MySkin) return;

            if(MySkin.Mesh)
                _characterMeshes[0].sharedMesh = MySkin.Mesh;

            if(MySkin.Material)
                _characterMeshes[0].material = MySkin.Material;

            if(MySkin.FPP_Mesh)
                _fppCharacterMesh.sharedMesh = MySkin.FPP_Mesh;

            if(MySkin.FPP_Material)
                _fppCharacterMesh.material = MySkin.FPP_Material;
        }

        //for ui to read this and display message in hud
        protected override void OnClientHealthAdded(int _currentHealth, int _addedHealth, uint _healerID)
        {
            Client_OnPickedupObject?.Invoke($"Health +{_addedHealth}");
        }

        protected void OnDisable()
        {
            Client_OnDestroyed?.Invoke();

            GameTicker.Game_Tick -= CharacterInstance_Tick;

            //Update 1.1 Moved to health script
            CustomSceneManager.DeRegisterCharacter(this);

            base.OnDestroy();
        }
    }
    public enum ActionCodes 
    {
        Trigger1,
        Trigger2,
        Sprint,
        Crouch,
    }
}
