using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MultiFPS
{
    [CreateAssetMenu(fileName = "CharacterSkin_", menuName = "MultiFPS/CharacterSkin")]
    public class SkinContainer : ScriptableObject
    {
        public string SkinName;

        [Header("Model")]
        public Mesh Mesh;
        public Material Material;

        [Header("FPP Model")]
        public Mesh FPP_Mesh;
        public Material FPP_Material;

    }
}