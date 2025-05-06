using System.Collections;
using System.Collections.Generic;
using TFarm.CropPlant;
using UnityEngine;

namespace TFarm.Inventory
{
    public class Item : MonoBehaviour
    {
        public int itemID;

        private SpriteRenderer spriteRenderer;

        private BoxCollider2D coll;

        public ItemDetails itemDetails;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            coll = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if(itemID != 0)
            {
                Init(itemID);
            }
        }

        public void Init(int ID)
        {
            itemID = ID;

            // Get the value from Inventory
            itemDetails = InventoryManager.Instance.GetItemDetails(itemID);

            if(itemDetails != null)
            {
                spriteRenderer.sprite = itemDetails.itemOnWorldSprite != null ? itemDetails.itemOnWorldSprite : itemDetails.itemIcon;

                // change the size of the collider with the size of the original object
                Vector2 newSize = new Vector2(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
                coll.size = newSize;
                // Because of pivot problem, we need to add an offset
                coll.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);

            }


            if(itemDetails.itemType == ItemType.ReapableScenery)
            {
                gameObject.AddComponent<ReapItem>();
                gameObject.GetComponent<ReapItem>().InitCropData(itemDetails.itemID);
                gameObject.AddComponent<ItemInteractive>();
            }
        }
    }
}
