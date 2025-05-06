using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TFarm.Save;

namespace TFarm.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>, ISaveable
    {

        [Header("Item Data")]
        public ItemDataList_SO itemDataList_SO;

        [Header("Blueprint Data")]
        public BluePrintDataList_SO bluePrintData;

        [Header("Bag Data")]
        public InventoryBag_SO playerBagTemp;
        public InventoryBag_SO playerBag;
        private InventoryBag_SO currentBoxBag;

        [Header("Trade")]
        public int playerMoney;
        private Dictionary<string, List<InventoryItem>> boxDataDict = new Dictionary<string, List<InventoryItem>>();
        public int BoxDataAmount => boxDataDict.Count;

        public string GUID => GetComponent<DataGUID>().guid;

        private void OnEnable()
        {
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void Start()
        {

            ISaveable saveable = this; // The current script is the saveable
            saveable.RegisterSaveable(); //  and register it

            // EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);

        }

        private void OnStartNewGameEvent(int obj)
        {
            playerBag = Instantiate(playerBagTemp);
            playerMoney = Settings.playerStartMoney;
            boxDataDict.Clear();
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
        {
            currentBoxBag = bag_SO;
        }

        private void OnBuildFurnitureEvent(int ID, Vector3 mousePos)
        {
            RemoveItem(ID, 1);
            BluePrintDetails bluePrint = bluePrintData.GetBluePrintDetails(ID);
            foreach(var item in bluePrint.resourceItem)
            {
                RemoveItem(item.itemID, item.itemAmount);
            }
        }

        private void OnDropItemEvent(int ID, Vector3 pos, ItemType itemType)
        {
            RemoveItem(ID, 1);
        }

        private void OnHarvestAtPlayerPosition(int ID)
        {
            // if there are the existing objects in the bag
            var index = GetItemIndexInBag(ID);

            AddItemAtIndex(ID, index, 1);

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// Return the details information of the object through ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>

        public ItemDetails GetItemDetails(int ID)
        {
            return itemDataList_SO.itemDetailsList.Find(i => i.itemID == ID);
        }


        /// <summary>
        /// Add the item to the player's bag (destory the item in the scenery)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toDestory"></param>
        public void AddItem(Item item, bool toDestory)
        {
            // if there are the existing objects in the bag
            var index = GetItemIndexInBag(item.itemID);
            AddItemAtIndex(item.itemID, index, 1);

            // Debug.Log(GetItemDetails(item.itemID) + "Name: " + GetItemDetails(item.itemID).itemName);
            if (toDestory)
            {
                Destroy(item.gameObject);
            }

            Debug.Log(GetItemDetails(item.itemID).itemID + " Name: " + GetItemDetails(item.itemID).itemName);

            // TODO: Update UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }


        /// <summary>
        /// Check whether there is still free space in the bag
        /// </summary>
        /// <returns></returns>
        private bool CheckBagCapacity()
        {
            for(int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Find the ID of the existing item in the bag
        /// </summary>
        /// <param name="ID">ID of the item</param>
        /// <returns>-1 if there is not existing item in the bag</returns>
        private int GetItemIndexInBag(int ID)
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == ID)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// At the certain space, we add the item
        /// </summary>
        /// <param name="ID">ID of the object</param>
        /// <param name="index"> sequence of the item in the bag </param>
        /// <param name="amount"> the number of the item picked up </param>
        private void AddItemAtIndex(int ID, int index, int amount)
        {
            if(index == -1 && CheckBagCapacity()) // The bag does not contain the same item before but has free space
            {
                var item = new InventoryItem { itemID = ID, itemAmount = amount };

                for (int i = 0; i < playerBag.itemList.Count; i++)
                {
                    if (playerBag.itemList[i].itemID == 0)
                    {
                        playerBag.itemList[i] = item;
                        break;
                    }
                }
            }
            else // The bag contains the same item before
            {
                int currentAmount = playerBag.itemList[index].itemAmount + amount;
                var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };

                playerBag.itemList[index] = item;
            }
        }

        /// <summary>
        /// Change two things in bags through dragging
        /// </summary>
        /// <param name="fromIndex"> the original index of the item we are dragging from </param>
        /// <param name="targetIndex"> the target index we are dragging to </param>
        public void SwapItem(int fromIndex, int targetIndex)
        {
            InventoryItem currentItem = playerBag.itemList[fromIndex];
            InventoryItem targetItem = playerBag.itemList[targetIndex];

            if (targetItem.itemID != 0) // if there is already item in the targetItem bag, we should follow the logic to change two things
            {
                playerBag.itemList[fromIndex] = targetItem;
                playerBag.itemList[targetIndex] = currentItem;
            }
            else // if not
            {
                playerBag.itemList[targetIndex] = currentItem;
                playerBag.itemList[fromIndex] = new InventoryItem();
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }


        /// <summary>
        /// Transfer items between different bags
        /// </summary>
        /// <param name="locationFrom"></param>
        /// <param name="fromIndex"></param>
        /// <param name="locationTarget"></param>
        /// <param name="targetIndex"></param>
        public void SwapItem(InventoryLocation locationFrom, int fromIndex, InventoryLocation locationTarget, int targetIndex)
        {
            var currentList = GetItemList(locationFrom);
            var targetList = GetItemList(locationTarget);

            InventoryItem currentItem = currentList[fromIndex];

            if(targetIndex < targetList.Count)
            {
                InventoryItem targetItem = targetList[targetIndex];

                if(targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID) // Two different items
                {
                    currentList[fromIndex] = targetItem;
                    targetList[targetIndex] = currentItem;
                }else if(currentItem.itemID == targetItem.itemID) // two same things
                {
                    targetItem.itemAmount += currentItem.itemAmount;
                    targetList[targetIndex] = targetItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                else // nothing in target location
                {
                    targetList[targetIndex] = currentItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                EventHandler.CallUpdateInventoryUI(locationFrom, currentList);
                EventHandler.CallUpdateInventoryUI(locationTarget, targetList);
            }


        }

        /// <summary>
        /// return the bag data list according to location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private List<InventoryItem> GetItemList(InventoryLocation location)
        {
            return location switch
            {
                InventoryLocation.Player => playerBag.itemList,
                InventoryLocation.Box => currentBoxBag.itemList,
                _ => null
            };
        }


        /// <summary>
        /// Remove the items of certain number after dropped
        /// </summary>
        /// <param name="ID">Item ID</param>
        /// <param name="removeAmount">Item number</param>
        private void RemoveItem(int ID, int removeAmount)
        {
            var index = GetItemIndexInBag(ID);

            if (playerBag.itemList[index].itemAmount > removeAmount)
            {
                var amount = playerBag.itemList[index].itemAmount - removeAmount;
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                playerBag.itemList[index] = item;
            }
            else if (playerBag.itemList[index].itemAmount == removeAmount)
            {
                var item = new InventoryItem();
                playerBag.itemList[index] = item;
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// Trade Items
        /// </summary>
        /// <param name="itemDetails"></param>
        /// <param name="amount"></param>
        /// <param name="isSellTrade"></param>
        public void TradeItem(ItemDetails itemDetails, int amount, bool isSellTrade)
        {
            int cost = itemDetails.itemPrice * amount;
            // Get the bag index of the item
            int index = GetItemIndexInBag(itemDetails.itemID);

            if (isSellTrade)    // To sell something
            {
                if (playerBag.itemList[index].itemAmount >= amount)
                {
                    RemoveItem(itemDetails.itemID, amount);
                    // the total price you will get
                    cost = (int)(cost * itemDetails.sellPercentage);
                    playerMoney += cost;
                }
            }
            else if (playerMoney - cost >= 0)   // To buy something
            {
                if (CheckBagCapacity())
                {
                    AddItemAtIndex(itemDetails.itemID, index, amount);
                }
                playerMoney -= cost;
            }
            // Refresh UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// Check wheather the item stock is enough to build the furniture according to the blueprint
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool CheckStock(int ID)
        {
            var bluePrintDetails = bluePrintData.GetBluePrintDetails(ID);

            foreach(var resourceItem in bluePrintDetails.resourceItem)
            {
                var itemStock = playerBag.GetInventoryItem(resourceItem.itemID);
                if(itemStock.itemAmount >= resourceItem.itemAmount)
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Find Box Data (Dictionary-Type) through string key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<InventoryItem> GetBoxDataList(string key)
        {
            if (boxDataDict.ContainsKey(key))
                return boxDataDict[key];
            return null;
        }

        /// <summary>
        /// Add to box bag data dictionary
        /// </summary>
        /// <param name="box"></param>
        public void AddBoxDataDict(Box box)
        {
            var key = box.name + box.index;
            if (!boxDataDict.ContainsKey(key))
            {
                boxDataDict.Add(key, box.boxBagData.itemList);
            }
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.playerMoney = this.playerMoney;

            saveData.inventoryDict = new Dictionary<string, List<InventoryItem>>();
            saveData.inventoryDict.Add(playerBag.name, playerBag.itemList);

            foreach(var item in boxDataDict)
            {
                saveData.inventoryDict.Add(item.Key, item.Value);
            }

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.playerMoney = saveData.playerMoney;
            playerBag = Instantiate(playerBagTemp);
            playerBag.itemList = saveData.inventoryDict[playerBag.name];

            foreach(var item in saveData.inventoryDict)
            {
                if (boxDataDict.ContainsKey(item.Key))
                {
                    boxDataDict[item.Key] = item.Value;
                }
            }

            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
    }
}
