using MultiFPS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MultiFPS
{
    public class UISkinSelector : MonoBehaviour
    {
        [SerializeField] Transform _itemSkinSelectorGridParent;
        [SerializeField] GameObject _itemSelectorPrefab;

        private UIItemSkinSelectorElement[] _spawnedSeletionTiles;
        void Start()
        {
            ItemSkinContainer[] itemSkinContainer = ClientInterfaceManager.Instance.ItemSkinContainers;

            _spawnedSeletionTiles = new UIItemSkinSelectorElement[itemSkinContainer.Length];

            for (int i = 0; i < itemSkinContainer.Length; i++)
            {
                UIItemSkinSelectorElement selectionTile = Instantiate(_itemSelectorPrefab, _itemSkinSelectorGridParent).GetComponent<UIItemSkinSelectorElement>();
                _spawnedSeletionTiles.SetValue(selectionTile, 0);

                selectionTile.SetSelectionTile(itemSkinContainer[i], i);
            }

            _itemSelectorPrefab.SetActive(false);
        }

    }
}