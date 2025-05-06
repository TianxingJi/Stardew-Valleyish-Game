using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFarm.Save
{
    [System.Serializable]
    public class GameSaveData
    {
        public string dataSceneName;
        /// <summary>
        /// Store Player/NPC Position and Characters' names
        /// </summary>
        public Dictionary<string, SerializableVector3> characterPosDict;

        public Dictionary<string, List<SceneItem>> sceneItemDict; // Scene Items
        public Dictionary<string, List<SceneFurniture>> sceneFurnitureDict; // Scene Furnitures
        public Dictionary<string, TileDetails> tileDetailsDict; // Tile Details
        public Dictionary<string, bool> firstLoadDict; // First Load
        public Dictionary<string, List<InventoryItem>> inventoryDict; // Inventory Items

        public Dictionary<string, int> timeDict; // Time

        public int playerMoney; // Money

        // NPC
        public string targetScene; // Target Scene
        public bool interactable; // Interactable ?
        public int animationInstanceID; // Which Animation?
    }
}