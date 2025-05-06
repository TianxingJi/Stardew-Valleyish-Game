using UnityEngine;

namespace TFarm.CropPlant
{
    public class CropManager : Singleton<CropManager>
    {
        public CropDataList_SO cropData;
        private Transform cropParent;
        private Grid currentGrid;
        private Season currentSeason;

        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }

        private void OnDisable()
        {
            EventHandler.PlantSeedEvent -= OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }

        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;
        }

        private void OnAfterSceneLoadedEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnPlantSeedEvent(int ID, TileDetails tileDetails)
        {
            CropDetails currentCrop = GetCropDetails(ID);
            if (currentCrop != null && SeasonAvailable(currentCrop) && tileDetails.seedItemID == -1)    //Used for the first seed planting
            {
                tileDetails.seedItemID = ID;
                tileDetails.growthDays = 0;
                //Display the crop plants
                DisplayCropPlant(tileDetails, currentCrop);
            }
            else if (tileDetails.seedItemID != -1)  //used to refresh the map
            {
                //Display the crop plants
                DisplayCropPlant(tileDetails, currentCrop);
            }
        }


        /// <summary>
        /// Display the crop plants
        /// </summary>
        /// <param name="tileDetails">tile map details</param>
        /// <param name="cropDetails">seed information</param>
        private void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
        {
            //during the growth stages
            int growthStages = cropDetails.growthDays.Length;
            int currentStage = 0;
            int dayCounter = cropDetails.TotalGrowthDays;

            //calculate and determine the current stage
            for (int i = growthStages - 1; i >= 0; i--)
            {
                if (tileDetails.growthDays >= dayCounter)
                {
                    currentStage = i;
                    break;
                }
                dayCounter -= cropDetails.growthDays[i];
            }

            //access the current item(crop plants) prefabs
            GameObject cropPrefab = cropDetails.growthPrefabs[currentStage];
            Sprite cropSprite = cropDetails.growthSprites[currentStage];

            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);

            GameObject cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, cropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;

            cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<Crop>().tileDetails = tileDetails;
        }


        /// <summary>
        /// Find Seed informatino through ID
        /// </summary>
        /// <param name="ID">Item ID</param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int ID)
        {
            return cropData.cropDetailsList.Find(c => c.seedItemID == ID);
        }

        /// <summary>
        /// Determine whether it is allowed to plant during this season
        /// </summary>
        /// <param name="crop">seed information</param>
        /// <returns></returns>
        private bool SeasonAvailable(CropDetails crop)
        {
            for (int i = 0; i < crop.seasons.Length; i++)
            {
                if (crop.seasons[i] == currentSeason)
                    return true;
            }
            return false;
        }
    }
}