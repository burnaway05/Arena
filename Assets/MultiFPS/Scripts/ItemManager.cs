
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.Gameplay {
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance;

        public LodoutForSlot[] SlotsLodout;

        private void Awake()
        {
            if (Instance) 
            {
          //      Debug.LogWarning("MultiFPS WARNING: Only one Item manager instance is allowed at once");
          //      Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        

        private void OnValidate()
        {
           /* if (SlotsLodout == null || SlotsLodout.Length == 0) return;

            for (int slotID = 0; slotID < SlotsLodout.Length; slotID++)
            {
                for (int itemID = 0; itemID < SlotsLodout[itemID].availableItemsForSlot.Length; itemID++)
                {
            //        GameObject item = SlotsLodout[itemID].availableItemsForSlot[itemID];

                    if (!item) continue;

                    if (!item.GetComponent<Item>())
                    {
                        Debug.LogError("MultiFPS WARNING: This prefab is not game item");
                        SlotsLodout[itemID].availableItemsForSlot[itemID] = null;
                    }
                }
            }*/
        }
    }

    [System.Serializable]
    public class LodoutForSlot 
    {
        public string SlotName; //for inspector convienience only
        public Item[] availableItemsForSlot;
    }
}