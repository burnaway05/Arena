using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiFPS.Gameplay
{
    public class AmmoPickup : PickupObject
    {
        /// <summary>
        /// player will only be able to pickup this pickup object if he owns item that this ammo is for
        /// </summary>
        [Tooltip("Item that this ammo is for")]
        [SerializeField] GameObject _itemPrefab;
        [SerializeField] int _amount;

        string _itemName;
        protected override void Awake()
        {
            base.Awake();
            _itemName = _itemPrefab.GetComponent<Item>().ItemName;
        }

        protected override void Contact(CharacterInstance _character)
        {
            CharacterItemManager _itemManager = _character.CharacterItemManager;

            for (int i = 0; i < _itemManager.Slots.Count; i++)
            {
                if (!_itemManager.Slots[i].Item) continue;

                if (_itemManager.Slots[i].Item.ItemName != _itemName) continue;

                int takenAmmo = _itemManager.Slots[i].Item.AddAmmo(_amount);

                if(takenAmmo>0)
                    Pickedup();
                break;
            }
        }

        public virtual void OnValidate()
        {
            if (!_itemPrefab) return;
            if (!_itemPrefab.GetComponent<Item>())
            {
                Debug.LogError("MultiFPS WARNING: This prefab is not game item");
                _itemPrefab = null;
            }
        }
    }
}
