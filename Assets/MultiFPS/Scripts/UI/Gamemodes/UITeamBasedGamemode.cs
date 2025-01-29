using Mirror;
using MultiFPS.Gameplay;
using MultiFPS.Gameplay.Gamemodes;
using UnityEngine;
using UnityEngine.UI;
namespace MultiFPS.UI.Gamemodes
{
    public class UITeamBasedGamemode : UIGamemode
    {
        [SerializeField] Text _enemyScoreText;
        [SerializeField] Text _ourScoreText;
        [SerializeField] Text _scoreToWinText;

        [SerializeField] GameObject _teamSelectPanel;
        [SerializeField] Image _teamIndicatorImage;

        [SerializeField] Text _ourTeamabundance;
        [SerializeField] Text _enemyTeamabundance;

        protected override void Awake()
        {
            base.Awake();
            _enemyScoreText.text = 0.ToString();
            _ourScoreText.text = 0.ToString();

            _enemyTeamabundance.text = 0.ToString();
            _ourTeamabundance.text = 0.ToString();
        }

        public override void SetupUI(Gamemode gamemode, NetworkIdentity player)
        {
            base.SetupUI(gamemode, player);

            TeamBasedGamemode[] _avaibleGamemodes = RoomManager._instance.GetComponents<TeamBasedGamemode>();
            for (int i = 0; i < _avaibleGamemodes.Length; i++)
            {
                if (_avaibleGamemodes[i].Indicator == gamemode.Indicator)
                {
                    TeamBasedGamemode tbg = _avaibleGamemodes[i];
                    tbg.GamemodeEvent_TeamDeathmatch_PlayerKilled += OnRoundEnded;
                    tbg.Client_TeamGM_PlayerKilled += OnPlayerKilled;

                    _scoreToWinText.text = tbg.ScoreToWin.ToString();
                    break;
                }
            }

            //show cursor so client will be able to select team
            ClientFrontend.ShowCursor(true);
        }

        int EnemyTeamIDForUI() 
        {
            return ClientFrontend.ThisClientTeam == 0 ? 1 : 0;
        }

        public void OnPlayerKilled(int[] teamsAbundance)
        {
            if (!ClientFrontend.ClientTeamAssigned) return;

            _ourTeamabundance.text = teamsAbundance[ClientFrontend.ThisClientTeam].ToString();
            _enemyTeamabundance.text = teamsAbundance[EnemyTeamIDForUI()].ToString();
        }
        public void OnRoundEnded(int[] teamScores)
        {
            if (!ClientFrontend.ClientTeamAssigned) return;

            _ourScoreText.text = teamScores[ClientFrontend.ThisClientTeam].ToString();
            _enemyScoreText.text = teamScores[EnemyTeamIDForUI()].ToString();
        }


        public override void Btn_ShowTeamSelector()
        {
            base.Btn_ShowTeamSelector();
            ShowPanel(!UITeamSelector.Instance.gameObject.activeSelf);
        }
        protected override void OnReceivedTeamResponse(int team, int permissionCode)
        {
            base.OnReceivedTeamResponse(team, permissionCode);
            if (permissionCode == 0) // *Succesfully joined requested team*
            {
                ShowPanel(false);

                if (team != -1)
                    _teamIndicatorImage.color = ClientInterfaceManager.Instance.UIColorSet.TeamColors[team];
            }
            else if (permissionCode == -1)
                UITeamSelector.Instance.WriteRejectionReason("This team is full");
            else if (permissionCode == -2)
                UITeamSelector.Instance.WriteRejectionReason("You cannot change team while game is running");

            
        }

        void ShowPanel(bool show)
        {
            ClientFrontend.ShowCursor(show);
            UITeamSelector.Instance.gameObject.SetActive(show);

            if (!show)
                UITeamSelector.Instance.WriteRejectionReason(string.Empty);
        }


        protected override void OnObservedCharacterSet(CharacterInstance characterInstance)
        {
            base.OnObservedCharacterSet(characterInstance);
        }
    }
}