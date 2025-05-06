using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFarm.Inventory
{
    public class ItemPickup : MonoBehaviour
    {
        // OnTriggerEnter2D is called when another object enters a trigger collider attached to this object (2D physics only).
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Attempt to retrieve the Item component from the object that entered the trigger.
            Item item = other.GetComponent<Item>();

            // Check if the object that entered the trigger has an Item component.
            if (item != null)
            {
                // Check if the item can be picked up, based on its itemDetails property.
                if (item.itemDetails.canPickedup)
                {
                    // If the item can be picked up, add it to the inventory.
                    // InventoryManager is a singleton that manages the inventory items.
                    InventoryManager.Instance.AddItem(item, true); // The 'true' means the Item should be destoryed

                    // Call an event to play a sound effect for item pickup.
                    // EventHandler is a class that handles events and might be using the observer pattern.
                    EventHandler.CallPlaySoundEvent(SoundName.Pickup);

                }
            }
        }

    }
}