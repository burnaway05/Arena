using MultiFPS;
using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace MultiFPS
{
    public class TagSetupEditor : Editor
    {
        public static void SetupTags()
        {
            //create tags array that will be inseryted to tag manager
            GameTags[] arr = System.Enum.GetValues(typeof(GameTags)) as GameTags[];

            ClearAllTags();
            InsertTags(arr);


            //tools
            void ClearAllTags()
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty layers = tagManager.FindProperty("tags");

                layers.ClearArray();

                tagManager.ApplyModifiedProperties();
            }

            void InsertTags(GameTags[] layersToSet)
            {

                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty layers = tagManager.FindProperty("tags");

                for (int i = 0; i < arr.Length; i++)
                {
                    GameTags layer = layersToSet[i];
                    layers.InsertArrayElementAtIndex(i);
                    SerializedProperty element = layers.GetArrayElementAtIndex(i);
                    element.stringValue = layer.ToString();
                }

                tagManager.ApplyModifiedProperties(); //save changes
            }
        }
    }
}