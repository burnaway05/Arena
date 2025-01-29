using MultiFPS.Gameplay.Gamemodes;
using MultiFPS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiFPS;

namespace MultiFPS.UI
{
    [CreateAssetMenu(fileName = "representer_map_XXX", menuName = "MultiFPS/UIColorSet")]
    public class UIColorSet : ScriptableObject
    {
        public Color[] TeamColors;

        public Color AllyColor;
        public Color EnemyColor;

        public Color AppropriateColorAccordingToTeam(int team)
        {
            return GameManager.Gamemode.FFA ? EnemyColor : ((team == ClientFrontend.ThisClientTeam) ? AllyColor : EnemyColor);
        }

    }
}