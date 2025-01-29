using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MultiFPS.UI
{
    public class PlayerLodout : MonoBehaviour
    {
        [SerializeField] GameObject _playerPrefab;
        [SerializeField] GameObject _playerSlotPrefab;
        [SerializeField] Transform _gridParent;
        PlayerLodoutSingleSlot[] _playerSlots; 

        void Start()
        {
            CharacterItemManager characterItemManager = _playerPrefab.GetComponent<CharacterItemManager>();

            if (UserSettings.PlayerLodout == null || UserSettings.PlayerLodout.Length == 0)
            {
                UserSettings.PlayerLodout = new int[characterItemManager.Slots.Count];

                for (int i = 0; i < UserSettings.PlayerLodout.Length; i++)
                {
                    UserSettings.PlayerLodout[i] = -1;
                }
            }
            int slotCount = characterItemManager.Slots.Count;

            _playerSlots = new PlayerLodoutSingleSlot[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                if (characterItemManager.Slots[i].SpecificItemOnly != string.Empty) continue;

                PlayerLodoutSingleSlot plss = Instantiate(_playerSlotPrefab, _gridParent).GetComponent<PlayerLodoutSingleSlot>();
                plss.Draw(characterItemManager.Slots[i], i);
                plss.OnItemSelected(0);

                _playerSlots.SetValue(plss, i);
            }

            _playerSlotPrefab.SetActive(false);
        }
    }
}
