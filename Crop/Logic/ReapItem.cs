using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFarm.CropPlant
{
    public class ReapItem : MonoBehaviour
    {
        private CropDetails cropDetails;

        private Transform PlayerTransform => FindObjectOfType<Player>().transform;

        public void InitCropData(int ID)
        {
            cropDetails = CropManager.Instance.GetCropDetails(ID);
        }

        /// <summary>
        /// Generate fruits
        /// </summary>
        public void SpawnHarvestItems()
        {
            for (int i = 0; i < cropDetails.producedItemID.Length; i++)
            {
                int amountToProduce;

                if (cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
                {
                    // It represents the fixed number of fruits generated
                    amountToProduce = cropDetails.producedMinAmount[i];
                }
                else    // random number of fruits generated
                {
                    amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
                }

                // Generate fixed number(amount to produce) of fruits 
                for (int j = 0; j < amountToProduce; j++)
                {
                    if (cropDetails.generateAtPlayerPosition)
                    {
                        EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                    }
                    else // generate items in the world map
                    {

                        // Determine the spawned direction, which should be opposite to the player
                        var dirX = transform.position.x > PlayerTransform.position.x ? 1 : -1;

                        // In a certain range
                        var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX),
                            transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y), 0);

                        EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                    }
                }
            }
        }

        }
}
