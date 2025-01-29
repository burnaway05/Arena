using MultiFPS.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MultiFPS.UI
{
    public class HudInventoryElement : MonoBehaviour
    {
        [SerializeField] Image _itemIcon;
        [SerializeField] Image _itemBackground;


        [SerializeField] Color _notInUsebackGroundColor;
        [SerializeField] Color _inUsebackGroundColor;

        [SerializeField] Text _itemIDtext;
        [SerializeField] Color _inUseColor;
        [SerializeField] Color _notInUseColor;

        public void Draw(Item item, SlotType slotType, bool inUse, int slotID, SlotInput input)
        {
            gameObject.SetActive(slotType == SlotType.Normal || slotType == SlotType.PocketItem && item && item.CurrentAmmoSupply > 0);

            _itemIDtext.color = inUse ? _inUseColor : _notInUseColor;
            _itemIcon.color = inUse ? _inUseColor : _notInUseColor;

            _itemBackground.color = inUse ? _inUsebackGroundColor : _notInUsebackGroundColor;
            _itemIDtext.text = input.ToString()[2].ToString();



            if (!item)
            {
                transform.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 80);
                _itemIcon.sprite = null;
                return;
            }


            _itemIcon.sprite = item.ItemIcon;

        }
    }
}