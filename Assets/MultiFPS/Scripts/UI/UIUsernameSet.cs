using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
namespace MultiFPS
{
    /// <summary>
    /// This class is just for setting and reading user username and selected skin for ingame character
    /// </summary>
    public class UIUsernameSet : MonoBehaviour
    {
        [SerializeField] InputField UsernameField;
        [SerializeField] Dropdown CharacterSkinSelector;

        string _playerPrefs_Username = "_playerPrefs_Username";
        string _playerPrefs_CharacterSkinID = "_playerPrefs_CharacterSkinID";

        private void Start()
        {
            UsernameField.onEndEdit.AddListener(UsernameModified);
            CharacterSkinSelector.onValueChanged.AddListener(SelectedCharacterSkin);

            ReadUsernameFromPlayerPrefs();

        }

        void ReadUsernameFromPlayerPrefs()
        {
            string username = PlayerPrefs.GetString(_playerPrefs_Username);
            UsernameField.text = username;
            UserSettings.UserNickname = username;

            int characterSkinID = PlayerPrefs.GetInt(_playerPrefs_CharacterSkinID);
            CharacterSkinSelector.value = characterSkinID;
            CharacterSkinSelector.RefreshShownValue();
            UserSettings.CharacterSkinID = characterSkinID;
        }

        void SelectedCharacterSkin(int id)
        {
            UserSettings.CharacterSkinID = CharacterSkinSelector.value;

            PlayerPrefs.SetInt(_playerPrefs_CharacterSkinID, UserSettings.CharacterSkinID);
        }

        void UsernameModified(string s)
        {
            UserSettings.UserNickname = CheckUsername(UsernameField.text);
            UsernameField.text = UserSettings.UserNickname;

            PlayerPrefs.SetString(_playerPrefs_Username, UserSettings.UserNickname);
        }

        string CheckUsername(string username)
        {
          //  if (string.IsNullOrEmpty(username))
         //   {
         //       return "Guest";
         //   }
            return username;
        }
    }
}