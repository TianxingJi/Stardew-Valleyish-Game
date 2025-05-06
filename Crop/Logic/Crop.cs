using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    public CropDetails cropDetails;

    public TileDetails tileDetails;

    private int harvestActionCount;

    public bool CanHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;

    private Animator anim;

    private Transform PlayerTransform => FindObjectOfType<Player>().transform;

    public void ProcessToolAction(ItemDetails tool, TileDetails tile)
    {

        tileDetails = tile;
        //The number of tool should be used
        int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
        if (requireActionCount == -1) return;

        //Determine weather there is animation and trees
        anim = GetComponentInChildren<Animator>();

        //Count the time you click
        if (harvestActionCount < requireActionCount)
        {
            harvestActionCount++;
            if(anim != null && cropDetails.hasAnimation)
            {
                if (PlayerTransform.position.x < transform.position.x)
                {
                    anim.SetTrigger("RotateRight");
                }
                else
                {
                    anim.SetTrigger("RotateLeft");
                }
            }


            // particle system
            if (cropDetails.hasParticalEffect)
            {
                EventHandler.CallParticleEffectEvent(cropDetails.effectType, transform.position + cropDetails.effectPos);
            }
            // sound effect

            if(cropDetails.soundEffect != SoundName.none)
            {
                EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }

        }

        if (harvestActionCount >= requireActionCount)
        {
            if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
            {
                //Generate Crop Plants Fruit
                SpawnHarvestItems();
            }else if (cropDetails.hasAnimation)
            {
                if (PlayerTransform.position.x < transform.position.x)
                {
                    anim.SetTrigger("FallingRight");
                }
                else
                {
                    anim.SetTrigger("FallingLeft");
                }
                EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                StartCoroutine(HarvestAfterAnimation());
            }
        }
    }

    private IEnumerator HarvestAfterAnimation()
    {
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("END"))
        {
            yield return null;
        }

        SpawnHarvestItems();

        // TODO: Transform into new items
        if(cropDetails.transferItemID > 0)
        {
            CreateTransferCrop();
        }

    }

    private void CreateTransferCrop()
    {
        tileDetails.seedItemID = cropDetails.transferItemID;
        tileDetails.daysSinceLastHarvest = -1;
        tileDetails.growthDays = 0;

        EventHandler.CallRefreshCurrentMap();
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

        if (tileDetails != null)
        {
            tileDetails.daysSinceLastHarvest++;

            // whether it can regrow
            if (cropDetails.daysToRegrow > 0 && tileDetails.daysSinceLastHarvest < cropDetails.regrowTimes - 1)
            {
                tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;
                //refresh the seed
                EventHandler.CallRefreshCurrentMap();
            }
            else    //if it cannot regrow
            {
                tileDetails.daysSinceLastHarvest = -1;
                tileDetails.seedItemID = -1;
                // tileDetails.daySinceDug = -1;
            }

            Destroy(gameObject);
        }
    }
}
