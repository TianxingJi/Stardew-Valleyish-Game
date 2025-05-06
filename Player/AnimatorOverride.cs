using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TFarm.Inventory;

public class AnimatorOverride : MonoBehaviour
{
    private Animator[] animators;

    public SpriteRenderer holdItem;

    [Header("Animation from different sections")]
    public List<AnimatorType> animatorTypes;

    private Dictionary<string, Animator> animatorNameDict = new Dictionary<string, Animator>();

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>(); // Get all the animators of each part of the player

        foreach (var anim in animators)
        {
            animatorNameDict.Add(anim.name, anim); //Store the parts of Player which have animators that can have animations
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        holdItem.enabled = false;
        SwitchAnimator(PartType.None);
    }

    private void OnHarvestAtPlayerPosition(int ID)
    {
        Sprite itemSprite = InventoryManager.Instance.GetItemDetails(ID).itemOnWorldSprite;
        if(holdItem.enabled == false)
        {
            StartCoroutine(ShowItem(itemSprite));
        }
    }

    private IEnumerator ShowItem(Sprite itemSprite)
    {
        holdItem.sprite = itemSprite;
        holdItem.enabled = true;
        yield return new WaitForSeconds(1f);
        holdItem.enabled = false;
    }

    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        //WORKFLOW:Different tool should match different animations 
        PartType currentType = itemDetails.itemType switch
        {
            ItemType.Seed => PartType.Carry,
            ItemType.Commodity => PartType.Carry,
            ItemType.HoeTool => PartType.Hoe,
            ItemType.WaterTool => PartType.Water,
            ItemType.CollectTool => PartType.Collect,
            ItemType.ChopTool => PartType.Chop,
            ItemType.BreakTool => PartType.Break,
            ItemType.ReapTool => PartType.Reap,
            ItemType.Funiture => PartType.Carry,
            _ => PartType.None
        };

        // If the item is not selected, disable the related steps
        if (isSelected == false)
        {
            currentType = PartType.None;
            holdItem.enabled = false;
        }
        else // if the item is slected
        {
            if (currentType == PartType.Carry) // and the type indicated the item is an item can be held up
            {  
                holdItem.sprite = itemDetails.itemOnWorldSprite;
                holdItem.enabled = true;
            }
            else // if not, just ignore it.
            {
                holdItem.enabled = false;
            }
        }

        // And if the item is selected and not belongs to carry, then it must be a tool, we should call the other function to handle this
        SwitchAnimator(currentType);
    }


    private void SwitchAnimator(PartType partType)
    {
        // For each item in animator Types
        foreach (var item in animatorTypes)
        {
            // If the part Type of this item is equal to the expected part Type
            if (item.partType == partType)
            {
                // Search and Run the animatior (animations) with the same name (for the expected part of Player, like Hair, Body and ...)
                animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.overrideController;
            }
        }
    }
}