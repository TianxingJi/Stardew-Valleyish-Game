using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TFarm.Inventory
{
    public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler // These can be used to implement Drag Item
    {
      
        [Header("Slot Get")]
        [SerializeField] private Image slotImage;
        [SerializeField] private TextMeshProUGUI amountText;
        public Image slotHightlight;
        [SerializeField] private Button button;

        [Header("Slot Type")]
        public SlotType slotType;

        public bool isSelected;

        public int slotIndex;

        public ItemDetails itemDetails;

        public int itemAmount;

        public InventoryLocation Location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _ => InventoryLocation.Player
                };
            }
        }

        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        // TODO: there may be hidden bugs here
        private void Start()
        {
            isSelected = false;

            if (itemDetails == null)
            {
                UpdateEmptySlot();
            }
        }

        /// <summary>
        /// Update the data of slots
        /// </summary>
        /// <param name="item">ItemDetails</param>
        /// <param name="amount">Item amount</param>
        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemDetails = item;
            slotImage.sprite = item.itemIcon;
            itemAmount = amount;
            amountText.text = amount.ToString();
            button.interactable = true;
            slotImage.enabled = true;
        }

        /// <summary>
        /// if the slot is empty, it cannot be interacted
        /// </summary>
        public void UpdateEmptySlot()
        {
            if (isSelected)
            {
                isSelected = false;

                inventoryUI.UpdateSlotHightlight(-1);

                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }
            itemDetails = null;
            slotImage.enabled = false;
            amountText.text = string.Empty;
            button.interactable = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (itemDetails == null) return;
            isSelected = !isSelected;

            inventoryUI.UpdateSlotHightlight(slotIndex);

            if(slotType == SlotType.Bag)
            {
                // inform the information of itemDetails and whether it is slected or not
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(itemAmount != 0) //if ther is an item
            {
                inventoryUI.dragItem.enabled = true; // Enable the item
                inventoryUI.dragItem.sprite = slotImage.sprite; // Assign an sprite according to the slot Image
                inventoryUI.dragItem.SetNativeSize(); // Set Native Size to avoid size problems

                isSelected = true;
                inventoryUI.UpdateSlotHightlight(slotIndex); // High Light the UI
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.enabled = false;
            //Debug.Log(eventData.pointerCurrentRaycast);

            // discard anything that is not related to UI
            if(eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                    return;

                // if the UI ray cast is not null, then we should continue
                var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                int targetIndex = targetSlot.slotIndex;

                // if both are bags type, it mean it's just change between different bags
                if(slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);

                }else if(slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag) // Buy something
                {
                    EventHandler.CallShowTradeUI(itemDetails, false);
                }
                else if(slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop) // Sell something
                {
                    EventHandler.CallShowTradeUI(itemDetails, true);
                }
                else if(slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType) 
                {
                    // Transfer Items between different bags
                    InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
                }

                // close all the slot Highlight by passing -1
                inventoryUI.UpdateSlotHightlight(-1);
            }
            //else // if you want to drop the item on the ground
            //{
            //    //print(itemDetails.canDropped);
            //    //print(itemDetails.itemID);

            //    if (itemDetails.canDropped)
            //    { 
                    
            //        // The world position according to the mouse position, where z is special because its default value is negative ten
            //        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

            //        EventHandler.CallInstantiateItemInScene(itemDetails.itemID, pos);
                    
            //    }
            //}
        }
    }
}