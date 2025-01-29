using Mirror;
using MultiFPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using MultiFPS.Gameplay.Gamemodes;
using MultiFPS.UI.Gamemodes;

namespace MultiFPS.UI.HUD
{
    /// <summary>
    /// Responsible for displaying player health, ammo for current weapon
    /// </summary>
    public class UICharacter : MonoBehaviour
    {
        public GameObject Overlay;

        public static UICharacter _instance;

        [SerializeField] Text _healthText;
        [SerializeField] Image _healthBar;

        public GameObject UIMarkerPrefab;

        //borders for 3D UI icons, in this project we use it only for player nametags 
        public Canvas WorldIconBorders;

        CharacterInstance _myObservedCharacter;

        [SerializeField] HUDTakeDamageMarker _takeDamageMarker;
        [SerializeField] UIRespawnCooldown _respawnCooldown;


        [Header("Gamemodes UI")]
        /// <summary>
        /// gamemodes UI prefabs have to be placed in this array in the same order as gamemodes in enum "Gamemodes"
        /// </summary>
        [SerializeField] GameObject[] gamemodesUI;

        [Header("Ammo")]
        [SerializeField] Text _ammo;
        [SerializeField] Text _ammoSupply;

        [Header("KillConfirmation")]
        [SerializeField] Image _skullIcon;
        [SerializeField] Text _killMessage;
        [SerializeField] Color _skullIconColor;
        [SerializeField] float _killMsgLiveTime = 3f;
        Coroutine _killConfirmationMessageProcedure;

        public float vanishingSpeed;
        public float maxInclination;
        public float gainingSpeed;
        public float vanishTime = 0.5f; //amount of time for hitmarker to live without fading out
        Coroutine hitMarkerAnimation;

        [Header("DamageIndicator")]
        [SerializeField] Image _damageIndicatorImage;
        [SerializeField] Color _damageIndicatorColor;
        Coroutine _damageIndicatorAnimation;
        [SerializeField] float _damageIndicatorVanishTime = 5f;

        #region Quick Message
        [SerializeField] private Text _msgText;
        [SerializeField] private ContentBackground _msgBackground;
        Coroutine _messageLiveTimeCounter;
        #endregion



        void Awake ()   
        {

            _instance = this;

            ShowCharacterHUD(false);
            _ammo.text = string.Empty;
            _ammoSupply.text = string.Empty;

            ClientFrontend.ClientEvent_OnJoinedToGame += InstantiateUIforGivenGamemode;

            _skullIcon.color = Color.clear;
            _killMessage.color = Color.clear;
            _damageIndicatorImage.color = Color.clear;

            ClientFrontend.GameEvent_OnObservedCharacterSet += AssignCharacterStatsToUI;

            GameManager.GameEvent_GamemodeEvent_Message += GamemodeMessage;

            //hide message UI on start
            _msgBackground.gameObject.SetActive(false);
            _msgText.text = string.Empty;
        }
        
        void OnDestroy()
        {
            ClientFrontend.ClientEvent_OnJoinedToGame -= InstantiateUIforGivenGamemode;
            ClientFrontend.GameEvent_OnObservedCharacterSet -= AssignCharacterStatsToUI;
            GameManager.GameEvent_GamemodeEvent_Message -= GamemodeMessage;
        }

        void InstantiateUIforGivenGamemode(Gamemode gamemode, NetworkIdentity player) 
        {

            int gamemodeID = (int)gamemode.Indicator;

            if (gamemodeID >= gamemodesUI.Length || gamemodesUI[gamemodeID] == null) return; //no ui for this gamemode avaible

            Instantiate(gamemodesUI[gamemodeID]).GetComponent<UIGamemode>().SetupUI(gamemode, player);
        }

        public void OnAmmoStateChanged(string ammo, string supply) 
        {
            _ammo.text = $" {ammo}|";
            _ammoSupply.text = $"{supply} ";
        }

        void OnHealthStateChanged(int _currentHealth, byte _hittedPartID, AttackType attackType, uint _attackerID)
        {
            UpdateHealthHUD();

            if (_damageIndicatorAnimation != null) 
            {
                StopCoroutine(_damageIndicatorAnimation);
                _damageIndicatorAnimation = null;
            }

            _damageIndicatorAnimation = StartCoroutine(DamageIndicatorAnimation());

            if (_currentHealth <= 0) 
            {
                if (GameManager.Gamemode.LetPlayersSpawnOnTheirOwn) _respawnCooldown.StartUI(RoomSetup.Properties.P_RespawnCooldown);
            }

            IEnumerator DamageIndicatorAnimation() 
            {
                _damageIndicatorImage.color = _damageIndicatorColor;

                Color startColor = _damageIndicatorImage.color;
                float progress = 0;

                while (progress < 1f) 
                {
                    progress += Time.deltaTime * _damageIndicatorVanishTime;
                    _damageIndicatorImage.color = Color.Lerp(startColor, Color.clear, progress);
                    yield return null;
                }
                _damageIndicatorImage.color = Color.clear;
            }
        }
        void OnHealthAdded(int _currentHealth, uint _healerID)
        {
            UpdateHealthHUD();
        }
        public void AssignCharacterStatsToUI(CharacterInstance _charInstance)
        {
            //desub from previously observed character
            if (_myObservedCharacter) 
            {
                _myObservedCharacter.Client_HealthStateChanged -= OnHealthStateChanged;
                _myObservedCharacter.Client_HealthAdded -= OnHealthAdded;
                _myObservedCharacter.Client_KillConfirmation -= PlayKillIcon;
            }

            _myObservedCharacter = _charInstance;
            _myObservedCharacter.Client_HealthStateChanged += OnHealthStateChanged;
            _myObservedCharacter.Client_HealthAdded += OnHealthAdded;
            _myObservedCharacter.Client_KillConfirmation += PlayKillIcon;

            ShowCharacterHUD(true);
            UpdateHealthHUD();

            _takeDamageMarker.Initialize(_charInstance);
            _respawnCooldown.HideUI();

            GetComponent<HudInventory>().ObserveCharacter(_charInstance);
        }

        //update health state in HUD
        void UpdateHealthHUD() 
        {
            _healthText.text = _myObservedCharacter.CurrentHealth.ToString();
            _healthBar.fillAmount = (float)_myObservedCharacter.CurrentHealth / _myObservedCharacter.MaxHealth;
        }

        public void ShowCharacterHUD(bool _show)
        {
            Overlay.SetActive(_show);
        }

        public void PlayKillIcon(byte hittedPart, uint victimID)
        {
            if (hitMarkerAnimation != null)
            {
                StopCoroutine(hitMarkerAnimation);
            }

            hitMarkerAnimation = StartCoroutine(HitmarkerAnimation());

            //feedback fro player
            //if (victimID != _myObservedCharacter.netId)
            //    _killMessage.text = "TERMINATED: " + GameManager.HealthInstances[victimID].CharacterName;
            //else
                _killMessage.text = "SELFDESTRUCT";

            if (_killConfirmationMessageProcedure != null) 
            {
                StopCoroutine(_killConfirmationMessageProcedure);
                _killConfirmationMessageProcedure = null;
            }

            _killConfirmationMessageProcedure = StartCoroutine(KillMsg());
        }

        IEnumerator KillMsg() 
        {
            _killMessage.color = _skullIconColor;

            float progress = 0f;


            yield return new WaitForSeconds(_killMsgLiveTime);
            while (progress < 1f) 
            {
                progress += Time.deltaTime*4;
                _killMessage.color = Color.Lerp(_skullIconColor, Color.clear, progress);
                yield return null;
            }
        }

        IEnumerator HitmarkerAnimation()
        {
            _skullIcon.color = _skullIconColor;
            _skullIcon.transform.localScale = new Vector3(maxInclination, maxInclination, maxInclination);
            float timer = 0f;
            while (true)
            {
                timer += Time.deltaTime;
                if (timer >= vanishTime)
                    _skullIcon.color = Color.Lerp(_skullIcon.color, Color.clear, vanishingSpeed * Time.deltaTime);

                _skullIcon.transform.localScale = Vector3.Lerp(_skullIcon.transform.localScale, Vector3.zero, gainingSpeed * Time.deltaTime);
                yield return 0;
            }
        }


        void GamemodeMessage(string _msg, float _liveTime)
        {
            if (_messageLiveTimeCounter != null)
            {
                StopCoroutine(_messageLiveTimeCounter);
                _messageLiveTimeCounter = null;
            }
            _messageLiveTimeCounter = StartCoroutine(messageLiveTimeCounter());

            IEnumerator messageLiveTimeCounter()
            {
                _msgText.text = _msg;
                _msgBackground.gameObject.SetActive(true);
                _msgBackground.OnSizeChanged();

                yield return new WaitForSeconds(_liveTime);
                _msgBackground.gameObject.SetActive(false);
                _msgText.text = "";
            }
        }
    }
}
