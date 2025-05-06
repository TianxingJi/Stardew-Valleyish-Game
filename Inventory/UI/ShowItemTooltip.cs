using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TFarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ShowItemTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private SlotUI slotUI;

        private InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(slotUI.itemDetails != null)
            {
                inventoryUI.itemTooltip.gameObject.SetActive(true);
                inventoryUI.itemTooltip.SetupTooltip(slotUI.itemDetails, slotUI.slotType);

                inventoryUI.itemTooltip.GetComponent<RectTransform>().pivot = new Vector2(0.5f,0);
                inventoryUI.itemTooltip.transform.position = transform.position + Vector3.up * 60;

                if(slotUI.itemDetails.itemType == ItemType.Funiture) // if the item type is furniture, then it means it should be built with some resources
                {
                    inventoryUI.itemTooltip.resourcePanel.SetActive(true); // display them with default UI
                    inventoryUI.itemTooltip.SetupResourcePanel(slotUI.itemDetails.itemID); // show the item numbers needed
                }
                else
                {
                    inventoryUI.itemTooltip.resourcePanel.SetActive(false); // else, we close the UI
                }

            }
            else
            {
                inventoryUI.itemTooltip.gameObject.SetActive(false);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryUI.itemTooltip.gameObject.SetActive(false);
        }

    }
}