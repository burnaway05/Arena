using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace MultiFPS.Gameplay
{
    public class CustomSceneManager : MonoBehaviour
    {

        public static List<Health> spawnedCharacters = new List<Health>();

        public static void RegisterCharacter(Health _char) 
        {
            spawnedCharacters.Add(_char);
        }
        public static void DeRegisterCharacter(Health _char)
        {
            spawnedCharacters.Remove(_char);
        }
    }
}
