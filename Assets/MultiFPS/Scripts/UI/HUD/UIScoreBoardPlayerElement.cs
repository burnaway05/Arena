using MultiFPS.Gameplay.Gamemodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiFPS.UI.HUD
{
    public class UIScoreBoardPlayerElement : MonoBehaviour
    {
        [SerializeField] Text _playerName;
        [SerializeField] Text _kills;
        [SerializeField] Text _deaths;
        [SerializeField] Text _latency;

        public void WriteData(string nick, int kills, int deaths, int team, short latency)
        {
            _playerName.text = nick;
            _kills.text = kills.ToString();
            _deaths.text = deaths.ToString();
            _latency.text = latency.ToString();

            //assign appropriate color for player in scoreboard depending on team, if player is not in any team, give him white color
            Color teamColor = team == -1 ? Color.white : ClientInterfaceManager.Instance.UIColorSet.TeamColors[team];

            _playerName.color = teamColor;
            _kills.color = teamColor;
            _deaths.color = teamColor;
        }
    }
}