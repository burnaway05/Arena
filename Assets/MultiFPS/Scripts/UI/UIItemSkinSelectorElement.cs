using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MultiFPS.UI
{
    public class UIItemSkinSelectorElement : MonoBehaviour
    {
        [SerializeField] Text _itemNameRenderer;
        [SerializeField] Dropdown _skinOptions;

        private int _skinID;

        public void SetSelectionTile(ItemSkinContainer itemSkins, int skinID)
        {

            _skinID = skinID;

            _itemNameRenderer.text = itemSkins.ItemName;

            _skinOptions.ClearOptions();

            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

            options.Add(new Dropdown.OptionData { text = "Default" });

            for (int i = 0; i < itemSkins.Skins.Length; i++)
            {
                options.Add(new Dropdown.OptionData { text = itemSkins.Skins[i].SkinName });
            }
            _skinOptions.AddOptions(options);

            _skinOptions.onValueChanged.AddListener(OnSkinSelected);
        }

        void OnSkinSelected(int option)
        {
            UserSettings.SelectedItemSkins[_skinID] = option - 1;
        }
    }
}