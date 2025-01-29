using MultiFPS;
using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS
{
    public class BotsPeacefulnessSwitch : Health
    {
        protected override void Start()
        {
            base.Start();
            CurrentHealth = int.MaxValue;
        }
        protected override void Server_OnDamaged(int damage, byte hittedPartID, AttackType attackType, uint attackerID)
        {
            GameManager.Gamemode.PeacefulBots = !GameManager.Gamemode.PeacefulBots;
        }
    }
}