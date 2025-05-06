using UnityEngine;

[System.Serializable]
public class CropDetails
{
    public int seedItemID;

    [Header("Different Growth Days for Different Growth Stage")]
    public int[] growthDays;
    public int TotalGrowthDays
    {
        get
        {
            int amount = 0;
            foreach (var days in growthDays)
            {
                amount += days;
            }
            return amount;
        }
    }

    [Header("Different Prefabs for Different Growth Stage")]
    public GameObject[] growthPrefabs;

    [Header("Different Images for Different Growth Stage")]
    public Sprite[] growthSprites;

    [Header("Valid Seasons for the Seed")]
    public Season[] seasons;

    [Space]
    [Header("Crop Tools")]
    public int[] harvestToolItemID;

    [Header("The number of Tool use to crop")]
    public int[] requireActionCount;

    [Header("Transform into new items with new IDs")]
    public int transferItemID;

    [Space]
    [Header("The fruit of the seed, which belongs to a new ID and Amount")]
    public int[] producedItemID;
    public int[] producedMinAmount;
    public int[] producedMaxAmount;
    public Vector2 spawnRadius;

    [Header("(Possible for some vegetables and fruits) the days to grow again")]
    public int daysToRegrow;
    public int regrowTimes;

    [Header("Options")]
    public bool generateAtPlayerPosition;
    public bool hasAnimation;
    public bool hasParticalEffect;
    
    public ParticleEffectType effectType;
    public Vector3 effectPos;

    public SoundName soundEffect;

    /// <summary>
    /// Check wheather the current tool is available to crop the plants
    /// </summary>
    /// <param name="toolID"></param>
    /// <returns></returns>
    public bool CheckToolAvailable(int toolID)
    {
        foreach (var tool in harvestToolItemID)
        {
            if (tool == toolID)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get the the number of crop actions specified by each tool
    /// </summary>
    /// <param name="toolID"></param>
    /// <returns></returns>
    public int GetTotalRequireCount(int toolID)
    {
        for(int i = 0; i < harvestToolItemID.Length; i++)
        {
            if (harvestToolItemID[i] == toolID)
                return requireActionCount[i];
        }
        return -1;
    }

}