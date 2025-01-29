using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS
{
    //Here multifps stores user settings thar can be accessed globally in runtime
    public static class UserSettings
    {
        public static string UserNickname;

        //selected skins
        public static int CharacterSkinID = 1;
        public static int[] SelectedItemSkins;

        public static float MouseSensitivity = 1f;
        public static float MouseSensitivityOnSniperScopeMultiplier = 0.34f;

        public static KeyCode FirstItemSlot = KeyCode.Alpha1;
        public static KeyCode SecondItemSlot = KeyCode.Alpha2;
        public static KeyCode ThirdtItemSlot = KeyCode.Alpha3;
        public static KeyCode FourthItemSlot = KeyCode.Alpha4;
        public static KeyCode PocketItemSlot = KeyCode.X;

        public static float UserFieldOfView = 60;

        public static int[] PlayerLodout;

    }
}