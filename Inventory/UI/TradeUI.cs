using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace TFarm.Inventory
{
    public class TradeUI : MonoBehaviour
    {
        public Image itemIcon;
        public Text itemName;
        public InputField tradeAmount;
        public Button submitButton;
        public Button cancelButton;

        private ItemDetails item;
        private bool isSellTrade;

        private void Awake()
        {
            cancelButton.onClick.AddListener(CancelTrade);
            submitButton.onClick.AddListener(TradeItem);
        }

        /// <summary>
        /// Setup of TradeUI Display
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isSell"></param>
        public void SetupTradeUI(ItemDetails item, bool isSell)
        {
            this.item = item;
            itemIcon.sprite = item.itemIcon;
            itemName.text = item.itemName;
            isSellTrade = isSell;
            tradeAmount.text = string.Empty;
        }

        private void TradeItem()
        {
            var amount = Convert.ToInt32(tradeAmount.text); // record the input number
            InventoryManager.Instance.TradeItem(item, amount, isSellTrade); // call the 'Inventory Manger' to calculate related data
            CancelTrade(); // close the trade UI
        }

        private void CancelTrade()
        {
            this.gameObject.SetActive(false);
        }
    }
}
