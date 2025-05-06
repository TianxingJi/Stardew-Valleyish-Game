using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TFarm.Save;

public class SaveSlotUI : MonoBehaviour
{
    public Text dataTime, dataScene;
    private Button currentButton;
    private DataSlot currentData;

    private int Index => transform.GetSiblingIndex();

    private void Awake()
    {
        currentButton = GetComponent<Button>();
        currentButton.onClick.AddListener(LoadGameData);
    }

    private void OnEnable()
    {
        SetupSlotUI();
    }

    private void SetupSlotUI()
    {
        currentData = SaveLoadManager.Instance.dataSlots[Index];

        if(currentData != null)
        {
            dataTime.text = currentData.DataTime;
            dataScene.text = currentData.DataScene;
        }
        else
        {
            dataTime.text = "Wait For You";
            dataScene.text = "Still in the Dream";
        }
    }

    private void LoadGameData()
    {
        if(currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        else
        {
            //Debug.Log("New Game");
            EventHandler.CallStartNewGameEvent(Index);
        }
    }
}
