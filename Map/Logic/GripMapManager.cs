using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using TFarm.CropPlant;
using TFarm.Save;

namespace TFarm.Map
{
    public class GridMapManager : Singleton<GridMapManager>, ISaveable
    {
        [Header("Map Tile Change Caused by watering and digging")]
        public RuleTile digTile;
        public RuleTile waterTile;

        private Tilemap digTilemap;
        private Tilemap waterTilemap;

        [Header("Map Information")]
        public List<MapData_SO> mapDataList;
        private Season currentSeason;

        // Use the scene Name plus position to store the tile information in Dictionary Type
        private Dictionary<string, TileDetails> tileDetailsDict = new Dictionary<string, TileDetails>();

        // Judge whether the scene is loaded for the first time
        private Dictionary<string, bool> firstLoadDict = new Dictionary<string, bool>();

        // The list to store all the reapable grass
        private List<ReapItem> itemsInRadius;
       

        private Grid currentGrid;

        public string GUID => GetComponent<DataGUID>().guid;

        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
            EventHandler.RefreshCurrentMap += RefreshMap;
        }

        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
            EventHandler.RefreshCurrentMap -= RefreshMap;
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();

            foreach (var mapData in mapDataList)
            {
                firstLoadDict.Add(mapData.sceneName, true);
                InitTileDetailsDict(mapData);
            }
        }

        private void OnAfterSceneLoadedEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            waterTilemap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();

            if (firstLoadDict[SceneManager.GetActiveScene().name])
            {
                // Generate Crop Plants in advance!!!
                EventHandler.CallGenerateCropEvent();

                firstLoadDict[SceneManager.GetActiveScene().name] = false;
            }
            RefreshMap();
        }

        /// <summary>
        /// Generate tile information Dictionary according to map information
        /// </summary>
        /// <param name="mapData">map data informatino</param>
        private void InitTileDetailsDict(MapData_SO mapData)
        {
            foreach (TileProperty tileProperty in mapData.tileProperties)
            {
                TileDetails tileDetails = new TileDetails
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y
                };

                // the key string in the Dictionary
                string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + mapData.sceneName;

                if (GetTileDetails(key) != null)
                {
                    tileDetails = GetTileDetails(key);
                }

                switch (tileProperty.gridType)
                {
                    case GridType.Diggable:
                        tileDetails.canDig = tileProperty.boolTypeValue;
                        break;
                    case GridType.DropItem:
                        tileDetails.canDropItem = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        tileDetails.canPlaceFurniture = tileProperty.boolTypeValue;
                        break;
                    case GridType.NPCObstacle:
                        tileDetails.isNPCObstacle = tileProperty.boolTypeValue;
                        break;
                }

                if (GetTileDetails(key) != null)
                    tileDetailsDict[key] = tileDetails;
                else
                    tileDetailsDict.Add(key, tileDetails);
            }
        }


        /// <summary>
        /// return the information according to key
        /// </summary>
        /// <param name="key">x+y+the map name</param>
        /// <returns></returns>
        public TileDetails GetTileDetails(string key)
        {
            if (tileDetailsDict.ContainsKey(key))
            {
                return tileDetailsDict[key];
            }
            return null;
        }

        /// <summary>
        /// return the grid information according to mouse position
        /// </summary>
        /// <param name="mouseGridPos">mouse position in grid layout map</param>
        /// <returns></returns>
        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
        {
            string key = mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name;
            return GetTileDetails(key);
        }


        /// <summary>
        /// Run this code if and only if the real function is called (without the need to verify some prerequisites)
        /// </summary>
        /// <param name="mouseWorldPos">mouse position</param>
        /// <param name="itemDetails">item information</param>
        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            var mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
            var currentTile = GetTileDetailsOnMousePosition(mouseGridPos);

            if (currentTile != null)
            {
                Crop currentCrop = GetCropObject(mouseWorldPos);
                //WORKFLOW:For the real function that needs done
                switch (itemDetails.itemType)
                {
                    case ItemType.Seed:
                        EventHandler.CallPlantSeedEvent(itemDetails.itemID, currentTile);
                        EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                        EventHandler.CallPlaySoundEvent(SoundName.Plant);
                        break;

                    case ItemType.Commodity:
                        EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                        break;

                    case ItemType.HoeTool:
                        SetDigGround(currentTile);
                        currentTile.daySinceDug = 0;
                        currentTile.canDig = false;
                        currentTile.canDropItem = false;
                        //TODO: Sound Effect
                        EventHandler.CallPlaySoundEvent(SoundName.Hoe);
                        break;

                    case ItemType.WaterTool:
                        SetWaterGround(currentTile);
                        currentTile.daySinceWatered = 0;
                        //TODO: Sound Effect
                        EventHandler.CallPlaySoundEvent(SoundName.Water);
                        break;

                    case ItemType.BreakTool:
                    case ItemType.ChopTool:
                        //TODO: Crop Trees
                        currentCrop?.ProcessToolAction(itemDetails, currentCrop.tileDetails);
                        break;

                    case ItemType.CollectTool:
                        // Crop currentCrop = GetCropObject(mouseWorldPos);
                        //TODO: Crop Plants
                        currentCrop.ProcessToolAction(itemDetails, currentTile);
                        break;

                    case ItemType.ReapTool:

                        var reapCount = 0;

                        for(int i = 0; i < itemsInRadius.Count; i++)
                        {
                            EventHandler.CallParticleEffectEvent(ParticleEffectType.ReapableScenery, itemsInRadius[i].transform.position + Vector3.up);

                            itemsInRadius[i].SpawnHarvestItems();

                            Destroy(itemsInRadius[i].gameObject);

                            reapCount++;

                            if (reapCount >= Settings.reapAmount) break;
                        }
                        EventHandler.CallPlaySoundEvent(SoundName.Reap);
                        break;

                    case ItemType.Funiture:
                        EventHandler.CallBuildFurnitureEvent(itemDetails.itemID, mouseWorldPos);
                        break;
                }

                UpdateTileDetails(currentTile);
            }
        }


        /// <summary>
        /// Through Physical Method to determine the crop plant clicked by the mouse
        /// </summary>
        /// <param name="mouseWorldPos">mouse position</param>
        /// <returns></returns>
        public Crop GetCropObject(Vector3 mouseWorldPos)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);

            Crop currentCrop = null;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<Crop>())
                    currentCrop = colliders[i].GetComponent<Crop>();
            }
            return currentCrop;
        }

        /// <summary>
        /// return the bool value(determine whether there is grass around the mouse/player)
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public bool HaveReapableItemsInRadius(Vector3 mouseWorldPos, ItemDetails tool)
        {
            itemsInRadius = new List<ReapItem>();

            Collider2D[] colliders = new Collider2D[20];

            Physics2D.OverlapCircleNonAlloc(mouseWorldPos, tool.itemUseRadius, colliders);

            if(colliders.Length > 0)
            {
                for(int i = 0; i < colliders.Length; i++)
                {
                    if(colliders[i] != null)
                    {
                        if (colliders[i].GetComponent<ReapItem>())
                        {
                            var item = colliders[i].GetComponent<ReapItem>();

                            itemsInRadius.Add(item);
                        }
                    }
                }
            }

            return itemsInRadius.Count > 0;
        }

        /// <summary>
        /// Set dig tile
        /// </summary>
        /// <param name="tile"></param>
        private void SetDigGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (digTilemap != null)
                digTilemap.SetTile(pos, digTile);
        }
        /// <summary>
        /// Set water tile
        /// </summary>
        /// <param name="tile"></param>
        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (waterTilemap != null)
                waterTilemap.SetTile(pos, waterTile);
        }

        /// <summary>
        /// Conduct once a day
        /// </summary>
        /// <param name="day"></param>
        /// <param name="season"></param>
        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;

            foreach (var tile in tileDetailsDict)
            {
                if (tile.Value.daySinceWatered > -1)
                {
                    tile.Value.daySinceWatered = -1;
                }
                if (tile.Value.daySinceDug > -1)
                {
                    tile.Value.daySinceDug++;
                }
                // The dug tile will recover to normal tile if no seed
                if (tile.Value.daySinceDug > 5 && tile.Value.seedItemID == -1)
                {
                    tile.Value.daySinceDug = -1;
                    tile.Value.canDig = true;
                    tile.Value.growthDays = -1;
                }

                if(tile.Value.seedItemID != -1)
                {
                    tile.Value.growthDays++;
                }
            }

            RefreshMap();
        }

        /// <summary>
        /// Update the tile information
        /// </summary>
        /// <param name="tileDetails"></param>
        public void UpdateTileDetails(TileDetails tileDetails)
        {
            string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + SceneManager.GetActiveScene().name;
            if (tileDetailsDict.ContainsKey(key))
            {
                tileDetailsDict[key] = tileDetails;
            }
            else
            {
                tileDetailsDict.Add(key, tileDetails);
            }
        }


        /// <summary>
        /// Refresh the current map
        /// </summary>
        private void RefreshMap()
        {
            if (digTilemap != null)
                digTilemap.ClearAllTiles();
            if (waterTilemap != null)
                waterTilemap.ClearAllTiles();

            foreach(var crop in FindObjectsOfType<Crop>())
            {
                Destroy(crop.gameObject);
            }
            DisplayMap(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Display the map tile
        /// </summary>
        /// <param name="sceneName">the scene name</param>
        private void DisplayMap(string sceneName)
        {
            foreach (var tile in tileDetailsDict)
            {
                var key = tile.Key; // get every tile detiles value
                var tileDetails = tile.Value;

                if (key.Contains(sceneName)) // update them
                {
                    if (tileDetails.daySinceDug > -1)
                        SetDigGround(tileDetails);
                    if (tileDetails.daySinceWatered > -1)
                        SetWaterGround(tileDetails);
                    if(tileDetails.seedItemID > -1)
                        EventHandler.CallPlantSeedEvent(tileDetails.seedItemID, tileDetails); // call the method again
                }
            }
        }

        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="gridDimensions"></param>
        /// <param name="gridOrigin"></param>
        /// <returns>The bool value whether there is the information of the current scene</returns>
        public bool GetGridDimensions(string sceneName, out Vector2Int gridDimensions, out Vector2Int gridOrigin)
        {
            gridDimensions = Vector2Int.zero;
            gridOrigin = Vector2Int.zero;

            foreach (var mapData in mapDataList)
            {
                if (mapData.sceneName == sceneName)
                {
                    gridDimensions.x = mapData.gridWidth;
                    gridDimensions.y = mapData.gridHeight;

                    gridOrigin.x = mapData.originX;
                    gridOrigin.y = mapData.originY;

                    return true;
                }
            }
            return false;
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.tileDetailsDict = this.tileDetailsDict;
            saveData.firstLoadDict = this.firstLoadDict;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.tileDetailsDict = saveData.tileDetailsDict;
            this.firstLoadDict = saveData.firstLoadDict;
        }
    }
}