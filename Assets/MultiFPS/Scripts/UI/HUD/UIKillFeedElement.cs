using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MultiFPS.Gameplay;
using MultiFPS;
using MultiFPS.Gameplay.Gamemodes;
namespace MultiFPS.UI.HUD
{

    public class UIKillFeedElement : MonoBehaviour
    {
        [SerializeField] Text _textKiller;
        [SerializeField] Text _textVictim;
        [SerializeField] Image _weapon;
        [SerializeField] Image _headShotIcon;
        [SerializeField] Image _background;
        [SerializeField] Sprite _meleeIcon;
        [SerializeField] Sprite _fallDamageIcon;
        [SerializeField] Image _penetrationIcon;
        [SerializeField] ContentSizeFitter _background_contentSizeFitter;
        [SerializeField] UIKillFeed _killfeedParent;

        [SerializeField] HorizontalLayoutGroup _layoutGroup;

        Coroutine c_vanish;
        private void Awake()
        {
           // _layoutGroup = GetComponent<HorizontalLayoutGroup>();
        }

        public void Write(uint victimID, CharacterPart hittedPart, AttackType attackType, uint killerID)
        {
            //to make sure that tile will always reappear at the bottom
            gameObject.transform.SetAsLastSibling();

            _headShotIcon.gameObject.SetActive(hittedPart == CharacterPart.head);

            gameObject.SetActive(true);

            CharacterInstance killer = GameManager.GetHealthInstance(killerID).GetComponent<CharacterInstance>(); //TODO: made error

            if (killerID != victimID)
            {
                
                _textKiller.text = " "+killer.CharacterName+ " ";
                _textKiller.color = ClientInterfaceManager.Instance.UIColorSet.TeamColors[killer.Team];
            }
            else 
            {
                _textKiller.text = string.Empty;
            }

            CharacterInstance victim = GameManager.GetHealthInstance(victimID).GetComponent<CharacterInstance>();
            _textVictim.text = " "+victim.CharacterName + " ";
            _textVictim.color = ClientInterfaceManager.Instance.UIColorSet.TeamColors[victim.Team];

            Sprite weaponSprite = null;
            if (attackType == AttackType.hitscan || attackType == AttackType.hitscanPenetrated)
            {
                weaponSprite = killer.CharacterItemManager.LastUsedItem ? killer.CharacterItemManager.LastUsedItem.KillFeedIcon : null;
            }
            else 
            {
                switch (attackType)
                {
                    case AttackType.melee:
                        weaponSprite = _meleeIcon;
                        break;
                    case AttackType.falldamage:
                        weaponSprite = _fallDamageIcon;
                        break;
                }
            }


            _weapon.sprite = weaponSprite;
            _penetrationIcon.gameObject.SetActive(attackType == AttackType.hitscanPenetrated);


            StopVanishCoroutine();
            c_vanish = StartCoroutine(VanishTimer());
            IEnumerator VanishTimer()
            {
                yield return new WaitForEndOfFrame();
                _layoutGroup.CalculateLayoutInputHorizontal();
                _layoutGroup.SetLayoutHorizontal();

                _background_contentSizeFitter.SetLayoutHorizontal();

                _killfeedParent.SetTiles();


                yield return new WaitForSeconds(6f);
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            StopVanishCoroutine();
        }

        void StopVanishCoroutine() 
        {
            if (c_vanish != null) 
            {
                StopCoroutine(c_vanish);
                c_vanish = null;
            }
        }
    }
}