using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiFPS.UI
{
    public class PlayerLodoutSingleSlot : MonoBehaviour
    {
        private List<int> itemIDs = new List<int>();
        [SerializeField] Dropdown _dropdown;

        int _playerSlotID;

        private void Awake()
        {
            _dropdown.onValueChanged.AddListener(OnItemSelected);
        }
        public void Draw(Slot slot, int playerSlotID) 
        {
            _playerSlotID = playerSlotID;

            _dropdown.ClearOptions();
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

            for (int i = 0; i < ItemManager.Instance.SlotsLodout[playerSlotID].availableItemsForSlot.Length; i++)
            {
                Item item = ItemManager.Instance.SlotsLodout[playerSlotID].availableItemsForSlot[i];
                if (item.SlotType != slot.Type) continue;
                options.Add(new Dropdown.OptionData { text = item.ItemName, image = item.ItemIcon });
                
                itemIDs.Add(i);
            }

            _dropdown.AddOptions(options);
        }

        public void OnItemSelected(int optionID) 
        {
            if (ItemManager.Instance.SlotsLodout[_playerSlotID].availableItemsForSlot.Length == 0) return;

            UserSettings.PlayerLodout[_playerSlotID] = itemIDs[optionID];

            if (ClientFrontend.ClientPlayerInstance)
                ClientFrontend.ClientPlayerInstance.CmdSendNewLodoutInfo(UserSettings.PlayerLodout);
        }
    }
}