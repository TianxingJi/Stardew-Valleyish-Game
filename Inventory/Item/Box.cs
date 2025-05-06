using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFarm.Inventory
{
    public class Box : MonoBehaviour
    {
        public InventoryBag_SO boxBagTemplate;

        public InventoryBag_SO boxBagData;

        public GameObject mouseIcon;

        private bool canOpen = false;

        private bool isOpen;

        public int index;

        private void OnEnable()  // onEnable will be executed after every scene loaded while Start does not, which can only be executed for only one time at the start
        {
            if (boxBagData == null)
            {
                boxBagData = Instantiate(boxBagTemplate);
            }

            //var key = this.name + index;
            //if(InventoryManager.Instance.GetBoxDataList(key) != null) // refresh the map data to reload the box data
            //{
            //    boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
            //}
            //else // create a new box 
            //{
            //    if(index == 0)
            //    {
            //        index = InventoryManager.Instance.BoxDataAmount;
            //    }

            //    InventoryManager.Instance.AddBoxDataDict(this);
            //}
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = true;
                mouseIcon.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = false;
                mouseIcon.SetActive(false);
            }
        }

        private void Update()
        {
            if(!isOpen && canOpen && Input.GetMouseButtonDown(1))
            {
                // Open the Box
                EventHandler.CallBaseBagOpenEvent(SlotType.Box, boxBagData);
                isOpen = true;
            }

            if(!canOpen && isOpen)
            {
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen = false;
            }

            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen = false;
            }
        }

        public void InitBox(int boxIndex)
        {
            index = boxIndex;
            var key = this.name + index;
            if (InventoryManager.Instance.GetBoxDataList(key) != null) // refresh the map data to reload the box data
            {
                boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
            }
            else // create a new box 
            {
                InventoryManager.Instance.AddBoxDataDict(this);
            }
        }
    }
}
